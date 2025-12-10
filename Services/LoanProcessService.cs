using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;
using FinanceBank.Data;

namespace FinanceBank.Services;

/// <summary>
/// Complete Loan Process Service - Handles all stages from application to payment
/// Automatically posts to General Ledger for loan disbursements.
/// Integrates with TransactionHistoryService for AR/AP tracking.
/// All transactions are logged to AuditLog for SOX/BSA compliance.
/// </summary>
public class LoanProcessService
{
    private readonly IDbContextFactory<BFASDbContext> _contextFactory;
    private readonly AutomaticGLPostingService? _glPostingService;
    private readonly TransactionHistoryService? _transactionHistoryService;
    private readonly AuditLogService? _auditLogService;
    private const decimal DAILY_PENALTY_RATE = 0.0005m; // 0.05% daily (BPI Standard)

    // =====================================================================
    // LOAN STATUS CONSTANTS - Matching the 5-Stage Workflow
    // =====================================================================
    public static class LoanStatus
    {
        // Stage 1: Customer submits loan request
        public const string PENDING_TELLER_REVIEW = "PENDING_TELLER_REVIEW";

        // Stage 2: Teller reviews and forwards to Accountant
        public const string PENDING_ACCOUNTANT_REVIEW = "PENDING_ACCOUNTANT_REVIEW";

        // Stage 3: Accountant assesses and forwards to Finance Manager
        public const string PENDING_FINANCEMANAGER_APPROVAL = "PENDING_FINANCEMANAGER_APPROVAL";

        // Stage 4: Finance Manager approves - Ready for release
        public const string APPROVED_READY_FOR_RELEASE = "APPROVED_READY_FOR_RELEASE";

        // Stage 5: Teller releases funds - Completed
        public const string COMPLETED_RELEASED = "COMPLETED_RELEASED";

        // Rejection statuses
        public const string REJECTED_TELLER = "REJECTED_TELLER";
        public const string REJECTED_ACCOUNTANT = "REJECTED_ACCOUNTANT";
        public const string REJECTED_FINANCEMANAGER = "REJECTED_FINANCEMANAGER";

        // Legacy statuses (for backward compatibility)
        public const string SUBMITTED = "SUBMITTED";
        public const string VERIFIED = "VERIFIED";
        public const string ENCODED = "ENCODED";
        public const string ASSESSED = "ASSESSED";
        public const string APPROVED = "APPROVED";
        public const string DECLINED = "DECLINED";
        public const string DISBURSED = "DISBURSED";
    }

    public LoanProcessService(
        IDbContextFactory<BFASDbContext> contextFactory, 
        AutomaticGLPostingService? glPostingService = null, 
        TransactionHistoryService? transactionHistoryService = null,
        AuditLogService? auditLogService = null)
    {
        _contextFactory = contextFactory;
        _glPostingService = glPostingService;
        _transactionHistoryService = transactionHistoryService;
        _auditLogService = auditLogService;
    }

    // =====================================================================
    // STAGE 1: APPLICATION SUBMISSION (CUSTOMER)
    // =====================================================================

    /// <summary>
    /// Submit loan application - Status becomes PENDING_TELLER_REVIEW
    /// </summary>
    public async Task<LoanApplication> SubmitApplicationAsync(
        int accountId,
        string loanType,
        decimal requestedAmount,
        string? purpose,
        string? employmentStatus,
        decimal? monthlyIncome,
        string submittedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            // LOAN RESTRICTION CHECK: Block application if user has existing unpaid loans
            // 1. Check for ANY active loan with outstanding balance > 0
            var hasUnpaidLoan = await _context.Loans
                .AnyAsync(l => l.AccountId == accountId &&
                              l.Status == "ACTIVE" &&
                              l.OutstandingBalance > 0);

            if (hasUnpaidLoan)
            {
                throw new InvalidOperationException("You have an existing loan that is not fully paid. Please complete all payments on your current loan before applying for a new one.");
            }

            // 2. Check for overdue loans (past due date with outstanding balance)
            var hasOverdueLoan = await _context.Loans
                .AnyAsync(l => l.AccountId == accountId &&
                              (l.Status == "ACTIVE" || l.Status == "OVERDUE") &&
                              l.OutstandingBalance > 0 &&
                              l.NextDueDate < DateTime.Today);

            if (hasOverdueLoan)
            {
                throw new InvalidOperationException("You have an overdue loan payment. Please settle your overdue balance before applying for a new loan.");
            }

            // 3. Check for any loan with outstanding balance > 0 (even if not active)
            var hasAnyOutstandingBalance = await _context.Loans
                .AnyAsync(l => l.AccountId == accountId &&
                              l.OutstandingBalance > 0 &&
                              l.Status != "Closed" &&
                              l.Status != "Rejected");

            if (hasAnyOutstandingBalance)
            {
                throw new InvalidOperationException("You still have an outstanding loan balance. Please fully pay your current loan before applying for a new one.");
            }

            // Also check for pending applications (excluding CANCELLED)
            var hasPendingApplication = await _context.LoanApplications
                .AnyAsync(a => a.AccountId == accountId &&
                              !a.Status.Contains("REJECTED") &&
                              a.Status != LoanStatus.COMPLETED_RELEASED &&
                              a.Status != "REJECTED" &&
                              a.Status != "CANCELLED");

            if (hasPendingApplication)
            {
                throw new InvalidOperationException("You have a pending loan application. Please wait for it to be processed.");
            }

            var applicationNumber = $"APP-{DateTime.Now:yyyyMMddHHmmss}";

            var application = new LoanApplication
            {
                ApplicationNumber = applicationNumber,
                AccountId = accountId,
                LoanType = loanType,
                RequestedAmount = requestedAmount,
                Purpose = purpose,
                EmploymentStatus = employmentStatus,
                MonthlyIncome = monthlyIncome,
                Status = LoanStatus.PENDING_TELLER_REVIEW, // New status
                SubmittedBy = submittedBy,
                ApplicationDate = DateTime.Now
            };

            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: accountId,
                transactionType: "APPLICATION",
                amount: requestedAmount,
                description: $"Loan application submitted: {loanType} - ₱{requestedAmount:N2}",
                processedBy: submittedBy,
                applicationId: application.ApplicationId,
                status: application.Status
            );

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message.Contains("unpaid loan") || ex.Message.Contains("pending")
                ? ex.Message
                : "Failed to submit loan application", ex);
        }
    }

    // =====================================================================
    // STAGE 2: TELLER REVIEW (TELLER)
    // =====================================================================

    /// <summary>
    /// Teller reviews and approves - forwards to Accountant
    /// Status changes from PENDING_TELLER_REVIEW to PENDING_ACCOUNTANT_REVIEW
    /// </summary>
    public async Task<LoanApplication> TellerApproveAsync(int applicationId, string approvedBy, string? remarks = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            // Accept both old and new statuses
            if (application.Status != LoanStatus.PENDING_TELLER_REVIEW &&
                application.Status != LoanStatus.SUBMITTED)
                throw new InvalidOperationException("Application is not pending teller review");

            application.Status = LoanStatus.PENDING_ACCOUNTANT_REVIEW;
            _context.LoanApplications.Update(application);
            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: application.AccountId,
                transactionType: "TELLER_REVIEW",
                amount: application.RequestedAmount,
                description: $"Teller approved application and forwarded to accountant",
                processedBy: approvedBy,
                applicationId: applicationId,
                status: application.Status
            );

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to approve application", ex);
        }
    }

    /// <summary>
    /// Teller rejects application
    /// </summary>
    public async Task<LoanApplication> TellerRejectAsync(int applicationId, string rejectionReason, string rejectedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            application.Status = LoanStatus.REJECTED_TELLER;
            application.RejectionReason = rejectionReason;
            application.RejectedAt = DateTime.Now;
            application.RejectedBy = rejectedBy;

            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to reject application", ex);
        }
    }

    /// <summary>
    /// Verify applicant identity - Teller verification (legacy support)
    /// </summary>
    public async Task<LoanApplication> VerifyIdentityAsync(int applicationId, string verifiedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            // Update status to VERIFIED so it appears in Encode page
            application.Status = "VERIFIED";
            _context.LoanApplications.Update(application);
            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to verify identity", ex);
        }
    }

    /// <summary>
    /// Reject application with reason
    /// </summary>
    public async Task<LoanApplication> RejectApplicationAsync(
        int applicationId,
        string rejectionReason,
        string rejectedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            application.Status = "REJECTED";
            application.RejectionReason = rejectionReason;
            application.RejectedAt = DateTime.Now;
            application.RejectedBy = rejectedBy;

            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to reject application", ex);
        }
    }

    // =====================================================================
    // STAGE 3: ASSESSMENT (ACCOUNTANT)
    // =====================================================================

    /// <summary>
    /// Create loan assessment - Accountant computes loan details
    /// Status changes to PENDING_FINANCEMANAGER_APPROVAL
    /// </summary>
    public async Task<LoanAssessment> CreateAssessmentAsync(
        int applicationId,
        int accountId,
        decimal loanAmount,
        decimal interestRate,
        int termMonths,
        string assessedBy,
        string? remarks = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            // Validate application exists
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            // Accept both old and new statuses
            if (application.Status != LoanStatus.PENDING_ACCOUNTANT_REVIEW &&
                application.Status != LoanStatus.ENCODED)
                throw new InvalidOperationException("Application must be pending accountant review");

            // Compute monthly payment and total payable
            var monthlyRate = interestRate / 100 / 12;
            var monthlyPayment = loanAmount * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, termMonths))
                / ((decimal)Math.Pow(1 + (double)monthlyRate, termMonths) - 1);
            var totalPayable = monthlyPayment * termMonths;

            var assessment = new LoanAssessment
            {
                ApplicationId = applicationId,
                AccountId = accountId,
                LoanAmount = loanAmount,
                InterestRate = interestRate,
                TermMonths = termMonths,
                ComputedMonthlyPayment = Math.Round(monthlyPayment, 2),
                TotalPayable = Math.Round(totalPayable, 2),
                AssessmentStatus = "ASSESSED",
                AssessedBy = assessedBy,
                Remarks = remarks,
                AssessmentDate = DateTime.Now
            };

            // Update application status to PENDING_FINANCEMANAGER_APPROVAL
            application.Status = LoanStatus.PENDING_FINANCEMANAGER_APPROVAL;
            _context.LoanApplications.Update(application);

            _context.LoanAssessments.Add(assessment);
            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: accountId,
                transactionType: "ACCOUNTANT_ASSESSMENT",
                amount: loanAmount,
                description: $"Accountant assessed loan: ₱{loanAmount:N2} at {interestRate}% for {termMonths} months",
                processedBy: assessedBy,
                applicationId: applicationId,
                status: application.Status
            );

            return assessment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create loan assessment", ex);
        }
    }

    /// <summary>
    /// Accountant rejects application
    /// </summary>
    public async Task<LoanApplication> AccountantRejectAsync(int applicationId, string rejectionReason, string rejectedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            application.Status = LoanStatus.REJECTED_ACCOUNTANT;
            application.RejectionReason = rejectionReason;
            application.RejectedAt = DateTime.Now;
            application.RejectedBy = rejectedBy;

            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to reject application", ex);
        }
    }

    // =====================================================================
    // STAGE 4: APPROVAL (FINANCE MANAGER)
    // =====================================================================

    /// <summary>
    /// Finance Manager approves loan - Status becomes APPROVED_READY_FOR_RELEASE
    /// </summary>
    public async Task<LoanApproval> ApproveLoanAsync(
        int assessmentId,
        int applicationId,
        int accountId,
        decimal approvedAmount,
        decimal approvedInterestRate,
        int approvedTermMonths,
        string approvedBy,
        string? specialConditions = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var assessment = await _context.LoanAssessments.FindAsync(assessmentId);
            if (assessment == null)
                throw new KeyNotFoundException($"Assessment {assessmentId} not found");

            // Update application status to APPROVED_READY_FOR_RELEASE
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application != null)
            {
                application.Status = LoanStatus.APPROVED_READY_FOR_RELEASE;
                _context.LoanApplications.Update(application);
            }

            // Update assessment status to approved (so it doesn't appear in pending list)
            assessment.AssessmentStatus = "FM_APPROVED";
            _context.LoanAssessments.Update(assessment);

            var approval = new LoanApproval
            {
                AssessmentId = assessmentId,
                ApplicationId = applicationId,
                AccountId = accountId,
                ApprovalStatus = "APPROVED",
                ApprovedAmount = approvedAmount,
                ApprovedInterestRate = approvedInterestRate,
                ApprovedTermMonths = approvedTermMonths,
                ApprovedBy = approvedBy,
                SpecialConditions = specialConditions,
                ApprovalDate = DateTime.Now
            };

            _context.LoanApprovals.Add(approval);
            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: accountId,
                transactionType: "FM_APPROVAL",
                amount: approvedAmount,
                description: $"Finance Manager approved loan: ₱{approvedAmount:N2} at {approvedInterestRate}% for {approvedTermMonths} months",
                processedBy: approvedBy,
                applicationId: applicationId,
                approvalId: approval.ApprovalId,
                status: "APPROVED"
            );

            return approval;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to approve loan", ex);
        }
    }

    /// <summary>
    /// Finance Manager declines loan
    /// </summary>
    public async Task<LoanApproval> DeclineLoanAsync(
        int assessmentId,
        int applicationId,
        int accountId,
        string declinationReason,
        string declinedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            // Update assessment status (so it doesn't appear in pending list)
            var assessment = await _context.LoanAssessments.FindAsync(assessmentId);
            if (assessment != null)
            {
                assessment.AssessmentStatus = "FM_DECLINED";
                _context.LoanAssessments.Update(assessment);
            }

            // Update application status to REJECTED_FINANCEMANAGER
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application != null)
            {
                application.Status = LoanStatus.REJECTED_FINANCEMANAGER;
                application.RejectionReason = declinationReason;
                application.RejectedAt = DateTime.Now;
                application.RejectedBy = declinedBy;
                _context.LoanApplications.Update(application);
            }

            var approval = new LoanApproval
            {
                AssessmentId = assessmentId,
                ApplicationId = applicationId,
                AccountId = accountId,
                ApprovalStatus = "DECLINED",
                DeclinationReason = declinationReason,
                ApprovedBy = declinedBy,
                ApprovalDate = DateTime.Now
            };

            _context.LoanApprovals.Add(approval);
            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: accountId,
                transactionType: "FM_DECLINE",
                amount: assessment?.LoanAmount ?? 0,
                description: $"Finance Manager declined loan: {declinationReason}",
                processedBy: declinedBy,
                applicationId: applicationId,
                approvalId: approval.ApprovalId,
                status: "DECLINED"
            );

            return approval;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decline loan", ex);
        }
    }

    // =====================================================================
    // STAGE 5: DISBURSEMENT / RELEASE (TELLER)
    // =====================================================================

    /// <summary>
    /// Teller releases funds - Final stage
    /// Status becomes COMPLETED_RELEASED
    /// </summary>
    public async Task<(Loan, LoanDisbursal)> TellerReleaseFundsAsync(
        int approvalId,
        int applicationId,
        int accountId,
        decimal disbursalAmount,
        string disbursedTo,
        string processedBy,
        string? contractReference = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var approval = await _context.LoanApprovals
                .Include(a => a.Assessment)
                .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);

            if (approval == null)
                throw new KeyNotFoundException($"Approval {approvalId} not found");

            if (approval.ApprovalStatus != "APPROVED")
                throw new InvalidOperationException("Only approved loans can be released");

            // Validate required approval values
            if (!approval.ApprovedInterestRate.HasValue || !approval.ApprovedTermMonths.HasValue || !approval.ApprovedAmount.HasValue)
                throw new InvalidOperationException("Approval is missing required values (amount, interest rate, or term)");

            var interestRate = approval.ApprovedInterestRate.Value;
            var termMonths = approval.ApprovedTermMonths.Value;

            // Update application status to COMPLETED_RELEASED
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application != null)
            {
                application.Status = LoanStatus.COMPLETED_RELEASED;
                _context.LoanApplications.Update(application);
            }

            // Create Loan record
            var loanNumber = $"LOAN-{DateTime.Now:yyyyMMddHHmmss}";
            var monthlyRate = interestRate / 100 / 12;
            decimal monthlyPayment;
            if (monthlyRate == 0)
            {
                monthlyPayment = disbursalAmount / termMonths;
            }
            else
            {
                monthlyPayment = disbursalAmount * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, termMonths))
                    / ((decimal)Math.Pow(1 + (double)monthlyRate, termMonths) - 1);
            }

            // Get loan type from assessment or application
            var loanType = approval.Assessment?.Application?.LoanType ?? "Personal";

            var loan = new Loan
            {
                LoanNumber = loanNumber,
                ApprovalId = approvalId,
                AccountId = accountId,
                LoanType = loanType,
                LoanAmount = disbursalAmount,
                OutstandingBalance = disbursalAmount,
                InterestRate = interestRate,
                TermMonths = termMonths,
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(termMonths),
                Status = "ACTIVE",
                CreatedAt = DateTime.Now
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Create Disbursal record
            var disbursal = new LoanDisbursal
            {
                LoanId = loan.LoanId,
                ApprovalId = approvalId,
                DisbursalAmount = disbursalAmount,
                DisbursalDate = DateTime.Now,
                DisbursalStatus = "RELEASED",
                DisbursedTo = disbursedTo,
                ProcessedBy = processedBy,
                Reference = contractReference ?? $"REL-{DateTime.Now:yyyyMMddHHmmss}"
            };

            _context.LoanDisbursals.Add(disbursal);

            // Credit customer account
            var customerAccount = await _context.CustomerAccounts.FindAsync(accountId);
            var balanceBefore = customerAccount?.Balance ?? 0;
            if (customerAccount != null)
            {
                customerAccount.Balance += disbursalAmount;
                customerAccount.AvailableBalance += disbursalAmount;
                customerAccount.LastTransactionAt = DateTime.Now;
                _context.CustomerAccounts.Update(customerAccount);
            }

            // Generate payment schedule
            await GeneratePaymentScheduleAsync(
                loan.LoanId,
                loan.MonthlyPayment,
                loan.InterestRate,
                loan.TermMonths,
                loan.StartDate
            );

            await _context.SaveChangesAsync();

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: accountId,
                transactionType: "RELEASE",
                amount: disbursalAmount,
                description: $"Loan funds released: ₱{disbursalAmount:N2} - {loanNumber}",
                processedBy: processedBy,
                applicationId: applicationId,
                approvalId: approvalId,
                loanId: loan.LoanId,
                reference: disbursal.Reference,
                status: "RELEASED",
                balanceBefore: balanceBefore,
                balanceAfter: customerAccount?.Balance ?? disbursalAmount
            );

            // Generate Release Invoice
            await GenerateInvoiceAsync(
                invoiceType: "RELEASE",
                loanId: loan.LoanId,
                disbursalId: disbursal.DisbursalId,
                accountId: accountId,
                customerName: disbursedTo,
                principalAmount: disbursalAmount,
                totalAmount: disbursalAmount,
                processedBy: processedBy,
                reference: disbursal.Reference,
                balanceBefore: balanceBefore,
                balanceAfter: customerAccount?.Balance ?? disbursalAmount
            );

            // Post to General Ledger automatically
            try
            {
                if (_glPostingService != null)
                {
                    await _glPostingService.PostLoanDisbursementAsync(
                        disbursal.DisbursalId,
                        loan.LoanNumber,
                        disbursalAmount,
                        disbursedTo,
                        disbursal.DisbursalDate);
                }
            }
            catch
            {
                // GL posting failure should not prevent disbursement from being recorded
            }

            // Create AP entry (loan release = money OUT = AP)
            try
            {
                if (_transactionHistoryService != null)
                {
                    var accountNumber = customerAccount?.AccountNumber ?? "";

                    await _transactionHistoryService.RecordLoanReleaseAsync(
                        customerAccountId: accountId,
                        accountNumber: accountNumber,
                        customerName: disbursedTo,
                        loanId: loan.LoanId,
                        amount: disbursalAmount,
                        disbursalId: disbursal.DisbursalId,
                        processedBy: processedBy,
                        notes: $"Loan#{loan.LoanNumber} | Term: {loan.TermMonths} months | Rate: {loan.InterestRate}%"
                    );
                }
            }
            catch
            {
                // AP tracking failure should not prevent disbursement from being recorded
            }

            return (loan, disbursal);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to release funds", ex);
        }
    }

    /// <summary>
    /// Get approved loans ready for release (Teller)
    /// </summary>
    public async Task<List<LoanApproval>> GetApprovedLoansForReleaseAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApprovals
            .Include(a => a.Assessment)
                .ThenInclude(a => a!.Application)
            .Include(a => a.Account)
                .ThenInclude(a => a!.Customer)
            .Include(a => a.Application)
                .ThenInclude(a => a!.Account)
                    .ThenInclude(a => a!.Customer)
            .Where(a => a.ApprovalStatus == "APPROVED")
            .Where(a => !_context.LoanDisbursals.Any(d => d.ApprovalId == a.ApprovalId))
            .OrderByDescending(a => a.ApprovalDate)
            .ToListAsync();
    }

    // =====================================================================
    // LEGACY DISBURSEMENT (for backward compatibility)
    // =====================================================================

    /// <summary>
    /// Create loan record and disburse funds
    /// </summary>
    public async Task<(Loan, LoanDisbursal)> DisburseLoanAsync(
        int approvalId,
        int accountId,
        decimal disbursalAmount,
        string disbursedTo,
        string processedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var approval = await _context.LoanApprovals
                .Include(a => a.Assessment)
                .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);

            if (approval == null)
                throw new KeyNotFoundException($"Approval {approvalId} not found");

            if (approval.ApprovalStatus != "APPROVED")
                throw new InvalidOperationException("Only approved loans can be disbursed");

            // Create Loan record
            var loanNumber = $"LOAN-{DateTime.Now:yyyyMMddHHmmss}";
            var monthlyRate = approval.ApprovedInterestRate!.Value / 100 / 12;
            var monthlyPayment = disbursalAmount * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, approval.ApprovedTermMonths!.Value))
                / ((decimal)Math.Pow(1 + (double)monthlyRate, approval.ApprovedTermMonths!.Value) - 1);

            var loan = new Loan
            {
                LoanNumber = loanNumber,
                AccountId = accountId,
                LoanType = approval.Assessment?.Application?.LoanType ?? "Personal",
                LoanAmount = disbursalAmount,
                OutstandingBalance = disbursalAmount,
                InterestRate = approval.ApprovedInterestRate!.Value,
                TermMonths = approval.ApprovedTermMonths!.Value,
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(approval.ApprovedTermMonths!.Value),
                Status = "Active",
                Purpose = approval.Assessment?.Application?.Purpose,
                CreatedAt = DateTime.Now,
                ApplicationId = approval.ApplicationId,
                AssessmentId = approval.AssessmentId,
                ApprovalId = approvalId
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Create payment schedule
            await GeneratePaymentScheduleAsync(loan.LoanId, loan.MonthlyPayment, loan.InterestRate, loan.TermMonths, loan.StartDate);

            // Create disbursal record
            var disbursal = new LoanDisbursal
            {
                LoanId = loan.LoanId,
                ApprovalId = approvalId,
                DisbursalAmount = disbursalAmount,
                DisbursalStatus = "RELEASED",
                DisbursedTo = disbursedTo,
                ProcessedBy = processedBy,
                DisbursalDate = DateTime.Now,
                Reference = loanNumber
            };

            _context.LoanDisbursals.Add(disbursal);
            loan.DisbursalId = disbursal.DisbursalId;

            await _context.SaveChangesAsync();

            // Post to General Ledger automatically
            try
            {
                if (_glPostingService != null)
                {
                    await _glPostingService.PostLoanDisbursementAsync(
                        disbursal.DisbursalId,
                        loan.LoanNumber,
                        disbursalAmount,
                        disbursedTo,
                        disbursal.DisbursalDate);
                }
            }
            catch
            {
                // GL posting failure should not prevent disbursement from being recorded
            }

            // =============================================
            // AUDIT LOG - Record loan disbursement for compliance
            // =============================================
            try
            {
                if (_auditLogService != null)
                {
                    var account = await _context.CustomerAccounts
                        .Include(ca => ca.Customer)
                        .FirstOrDefaultAsync(ca => ca.AccountId == accountId);
                    var customerName = account?.Customer?.FullName ?? disbursedTo;

                    await _auditLogService.LogLoanDisbursementAsync(
                        employeeId: null, // Will be enhanced when we have employee context
                        employeeName: processedBy,
                        employeeRole: "Teller",
                        customerId: account?.Customer?.UserId,
                        customerName: customerName,
                        loanId: loan.LoanId,
                        loanNumber: loan.LoanNumber,
                        loanType: loan.LoanType ?? "Personal",
                        accountNumber: account?.AccountNumber ?? "",
                        amount: disbursalAmount,
                        balanceAfter: account?.Balance ?? 0,
                        transactionNumber: disbursal.Reference ?? loan.LoanNumber,
                        referenceNumber: $"DISB-{disbursal.DisbursalId}",
                        approvedBy: approval.ApprovedBy,
                        approvedAt: approval.ApprovalDate,
                        status: "Completed",
                        notes: $"Loan disbursement for {loan.LoanType}. Term: {loan.TermMonths} months. Rate: {loan.InterestRate}%");
                }
            }
            catch (Exception auditEx)
            {
                // Audit log failure should not affect transaction
                System.Diagnostics.Debug.WriteLine($"Audit log creation failed: {auditEx.Message}");
            }

            return (loan, disbursal);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to disburse loan", ex);
        }
    }

    // =====================================================================
    // PAYMENT SCHEDULE GENERATION
    // =====================================================================

    /// <summary>
    /// Generate payment schedule for loan installments
    /// </summary>
    private async Task GeneratePaymentScheduleAsync(
        int loanId,
        decimal monthlyPayment,
        decimal interestRate,
        int termMonths,
        DateTime startDate)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var monthlyRate = interestRate / 100 / 12;
            var outstandingBalance = await _context.Loans
                .Where(l => l.LoanId == loanId)
                .Select(l => l.LoanAmount)
                .FirstOrDefaultAsync();

            var schedules = new List<LoanPaymentSchedule>();

            for (int i = 1; i <= termMonths; i++)
            {
                var dueDate = startDate.AddMonths(i);
                var interestAmount = outstandingBalance * monthlyRate;
                var principalAmount = monthlyPayment - interestAmount;

                var schedule = new LoanPaymentSchedule
                {
                    LoanId = loanId,
                    PaymentNumber = i,
                    DueDate = dueDate,
                    MinimumPayment = Math.Round(monthlyPayment, 2),
                    PrincipalAmount = Math.Round(principalAmount, 2),
                    InterestAmount = Math.Round(interestAmount, 2),
                    PaymentStatus = "PENDING",
                    DaysOverdue = 0,
                    Penalty = 0
                };

                schedules.Add(schedule);
                outstandingBalance -= principalAmount;
            }

            _context.LoanPaymentSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate payment schedule", ex);
        }
    }

    // =====================================================================
    // STAGE 6: PAYMENT PROCESSING (TELLER & ACCOUNTANT)
    // =====================================================================

    /// <summary>
    /// Process loan payment - Physical cash payment (no account deduction)
    /// </summary>
    public async Task<LoanPayment> ProcessPaymentAsync(
        int loanId,
        int scheduleId,
        decimal paymentAmount,
        string paymentMethod,
        string processedBy,
        string? reference = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var loan = await _context.Loans
                .Include(l => l.Account)
                .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);
            if (loan == null)
                throw new KeyNotFoundException($"Loan {loanId} not found");

            var schedule = await _context.LoanPaymentSchedules.FindAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Payment schedule {scheduleId} not found");

            var isLatePayment = DateTime.Now > schedule.DueDate;
            var penaltyAmount = 0m;

            // Calculate penalty if late
            if (isLatePayment)
            {
                var daysOverdue = (int)(DateTime.Now - schedule.DueDate).TotalDays;
                schedule.DaysOverdue = daysOverdue;
                penaltyAmount = schedule.MinimumPayment * DAILY_PENALTY_RATE * daysOverdue;

                // Create violation record
                await CreateViolationAsync(loanId, scheduleId, "LATE_PAYMENT", daysOverdue, penaltyAmount, loan.OutstandingBalance);

                // Track cumulative late days
                loan.CumulativeLateDays += daysOverdue;
                loan.TotalPenalties += penaltyAmount;
            }

            var balanceBefore = loan.OutstandingBalance;

            // Record payment
            var paymentRef = reference ?? $"PAY-{DateTime.Now:yyyyMMddHHmmss}";
            var payment = new LoanPayment
            {
                ScheduleId = scheduleId,
                LoanId = loanId,
                PaymentAmount = paymentAmount,
                PenaltyPaid = penaltyAmount,
                PaymentMethod = paymentMethod,
                Reference = paymentRef,
                ProcessedBy = processedBy,
                PaymentDate = DateTime.Now,
                IsLatePayment = isLatePayment
            };

            // Update schedule status and loan balance
            // NOTE: Physical cash payment - do NOT deduct from customer account balance
            if (paymentAmount >= schedule.MinimumPayment + penaltyAmount)
            {
                schedule.PaymentStatus = "PAID";
                schedule.Penalty = penaltyAmount;
                loan.OutstandingBalance -= schedule.PrincipalAmount;
            }
            else
            {
                schedule.PaymentStatus = "PARTIAL";
                schedule.Penalty = penaltyAmount;
                loan.OutstandingBalance -= (paymentAmount - penaltyAmount);
            }

            loan.LastPaymentDate = DateTime.Now;
            var balanceAfter = loan.OutstandingBalance;

            // Check if loan is fully paid
            var isFullyPaid = false;
            if (loan.OutstandingBalance <= 0)
            {
                loan.Status = "Completed";
                isFullyPaid = true;
            }

            _context.LoanPayments.Add(payment);
            await _context.SaveChangesAsync();

            // Get customer name
            var customerName = loan.Account?.Customer?.FullName ?? "Customer";

            // Log transaction history
            await LogTransactionHistoryAsync(
                accountId: loan.AccountId,
                transactionType: isFullyPaid ? "FULL_PAYMENT" : "PAYMENT",
                amount: paymentAmount,
                description: isFullyPaid
                    ? $"Loan fully paid: ₱{paymentAmount:N2} (Final payment) - {loan.LoanNumber}"
                    : $"Loan payment: ₱{paymentAmount:N2} - {loan.LoanNumber}",
                processedBy: processedBy,
                loanId: loanId,
                reference: paymentRef,
                status: isFullyPaid ? "COMPLETED" : "ACTIVE",
                penaltyAmount: penaltyAmount,
                balanceBefore: balanceBefore,
                balanceAfter: balanceAfter
            );

            // Generate Payment Invoice
            await GenerateInvoiceAsync(
                invoiceType: "PAYMENT",
                loanId: loanId,
                paymentId: payment.PaymentId,
                accountId: loan.AccountId,
                customerName: customerName,
                principalAmount: schedule.PrincipalAmount,
                interestAmount: schedule.InterestAmount,
                penaltyAmount: penaltyAmount,
                totalAmount: paymentAmount,
                processedBy: processedBy,
                paymentMethod: paymentMethod,
                reference: paymentRef,
                balanceBefore: balanceBefore,
                balanceAfter: balanceAfter,
                notes: isFullyPaid ? "LOAN FULLY PAID - Thank you!" : null
            );

            // Create AR entry (loan payment = money IN = AR)
            try
            {
                if (_transactionHistoryService != null)
                {
                    // Get account number and calculate interest from schedule
                    var accountNumber = loan.Account?.AccountNumber ?? "";
                    var interestAmount = schedule.InterestAmount;
                    var principalPaid = paymentAmount - interestAmount - penaltyAmount;
                    if (principalPaid < 0) principalPaid = 0;

                    await _transactionHistoryService.RecordLoanPaymentAsync(
                        customerAccountId: loan.AccountId,
                        accountNumber: accountNumber,
                        customerName: customerName,
                        loanId: loanId,
                        principalAmount: principalPaid,
                        interestAmount: interestAmount,
                        penaltyAmount: penaltyAmount,
                        paymentId: payment.PaymentId,
                        processedBy: processedBy,
                        notes: $"Payment Method: {paymentMethod}. Outstanding: ₱{balanceAfter:N2}"
                    );
                }
            }
            catch
            {
                // AR tracking failure should not prevent payment from being recorded
            }

            // =============================================
            // AUDIT LOG - Record loan payment for compliance
            // =============================================
            try
            {
                if (_auditLogService != null)
                {
                    var accountNumber = loan.Account?.AccountNumber ?? "";
                    var interestAmount = schedule.InterestAmount;
                    var principalPaid = paymentAmount - interestAmount - penaltyAmount;
                    if (principalPaid < 0) principalPaid = 0;

                    await _auditLogService.LogLoanPaymentAsync(
                        employeeId: null, // Will be enhanced when we have employee context
                        employeeName: processedBy,
                        employeeRole: "Teller",
                        customerId: loan.Account?.Customer?.UserId,
                        customerName: customerName,
                        loanId: loan.LoanId,
                        loanNumber: loan.LoanNumber,
                        loanType: loan.LoanType ?? "Personal",
                        accountNumber: accountNumber,
                        amount: paymentAmount,
                        principalAmount: principalPaid,
                        interestAmount: interestAmount,
                        balanceBefore: balanceBefore,
                        balanceAfter: balanceAfter,
                        remainingLoanBalance: loan.OutstandingBalance,
                        transactionNumber: paymentRef,
                        referenceNumber: $"PAY-{payment.PaymentId}",
                        paymentMethod: paymentMethod,
                        transactionChannel: "Teller Window",
                        status: isFullyPaid ? "Fully Paid" : "Completed",
                        notes: isLatePayment ? $"Late payment. Days overdue: {schedule.DaysOverdue}. Penalty: ₱{penaltyAmount:N2}" : null);
                }
            }
            catch (Exception auditEx)
            {
                // Audit log failure should not affect transaction
                System.Diagnostics.Debug.WriteLine($"Audit log creation failed: {auditEx.Message}");
            }

            return payment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process payment", ex);
        }
    }

    // =====================================================================
    // VIOLATION HANDLING
    // =====================================================================

    /// <summary>
    /// Create loan violation record for late/missed payments
    /// </summary>
    private async Task CreateViolationAsync(
        int loanId,
        int scheduleId,
        string violationType,
        int daysOverdue,
        decimal penaltyAmount,
        decimal outstandingBalance)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var violation = new LoanViolation
            {
                LoanId = loanId,
                ScheduleId = scheduleId,
                ViolationType = violationType,
                DaysOverdue = daysOverdue,
                PenaltyRate = DAILY_PENALTY_RATE,
                PenaltyAmount = penaltyAmount,
                OutstandingBalance = outstandingBalance,
                ViolationDate = DateTime.Now,
                IsResolved = false
            };

            _context.LoanViolations.Add(violation);

            // Check if customer should be blacklisted (3+ violations)
            var violationCount = await _context.LoanViolations
                .Where(v => v.LoanId == loanId && !v.IsResolved)
                .CountAsync();

            if (violationCount >= 3)
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan != null)
                {
                    loan.IsBlacklisted = true;
                    loan.BlacklistedAt = DateTime.Now;
                    loan.BlacklistedReason = "Multiple loan violations (3+)";
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create violation record", ex);
        }
    }

    // =====================================================================
    // RE-LOAN ELIGIBILITY CHECK
    // =====================================================================

    /// <summary>
    /// Check if customer is eligible for re-loan
    /// </summary>
    public async Task<(bool IsEligible, string Reason)> CheckReloamEligibilityAsync(int accountId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            // Get all loans for customer
            var loans = await _context.Loans
                .Where(l => l.AccountId == accountId)
                .ToListAsync();

            // Check 1: Outstanding balance must be zero
            var hasOutstandingBalance = loans.Any(l => l.OutstandingBalance > 0);
            if (hasOutstandingBalance)
                return (false, "Customer has outstanding loan balance");

            // Check 2: No active violations in last 6 months
            var recentViolations = await _context.LoanViolations
                .Join(_context.Loans, v => v.LoanId, l => l.LoanId, (v, l) => new { v, l })
                .Where(x => x.l.AccountId == accountId
                    && !x.v.IsResolved
                    && x.v.ViolationDate > DateTime.Now.AddMonths(-6))
                .CountAsync();

            if (recentViolations > 0)
                return (false, "Customer has unresolved violations in the last 6 months");

            // Check 3: Not blacklisted
            var isBlacklisted = loans.Any(l => l.IsBlacklisted);
            if (isBlacklisted)
                return (false, "Customer is blacklisted");

            return (true, "Customer is eligible for re-loan");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check re-loan eligibility", ex);
        }
    }

    // =====================================================================
    // UTILITY METHODS
    // =====================================================================

    /// <summary>
    /// Get loan application details
    /// </summary>
    public async Task<LoanApplication?> GetApplicationAsync(int applicationId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApplications
            .Include(a => a.Account)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
    }

    /// <summary>
    /// Get loan details
    /// </summary>
    public async Task<Loan?> GetLoanAsync(int loanId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Loans
            .Include(l => l.Account)
            .Include(l => l.PaymentSchedules)
            .Include(l => l.Payments)
            .Include(l => l.Violations)
            .FirstOrDefaultAsync(l => l.LoanId == loanId);
    }

    /// <summary>
    /// Get all loans for account
    /// </summary>
    public async Task<List<Loan>> GetAccountLoansAsync(int accountId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Loans
            .Where(l => l.AccountId == accountId)
            .Include(l => l.PaymentSchedules)
            .Include(l => l.Violations)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending payment schedules
    /// </summary>
    public async Task<List<LoanPaymentSchedule>> GetPendingPaymentsAsync(int loanId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanPaymentSchedules
            .Where(s => s.LoanId == loanId && (s.PaymentStatus == "PENDING" || s.PaymentStatus == "OVERDUE"))
            .OrderBy(s => s.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get loan violations
    /// </summary>
    public async Task<List<LoanViolation>> GetLoanViolationsAsync(int loanId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanViolations
            .Where(v => v.LoanId == loanId)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending assessments for approval
    /// </summary>
    public async Task<List<LoanAssessment>> GetPendingAssessmentsAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanAssessments
            .Where(a => a.AssessmentStatus == "ASSESSED")
            .Include(a => a.Account)
                .ThenInclude(a => a!.Customer)
            .Include(a => a.Application)
            .OrderBy(a => a.AssessmentDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get approved loans ready for contract preparation
    /// </summary>
    public async Task<List<LoanApproval>> GetApprovedLoansAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApprovals
            .Where(a => a.ApprovalStatus == "APPROVED")
            .Include(a => a.Account)
                .ThenInclude(a => a!.Customer)
            .Include(a => a.Assessment)
                .ThenInclude(a => a!.Application)
            .Include(a => a.Application)
            .OrderByDescending(a => a.ApprovalDate)
            .ToListAsync();
    }

    /// <summary>
    /// Prepare loan contract document with industry-standard fees and calculations
    /// </summary>
    public async Task<string> PrepareContractAsync(int approvalId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var approval = await _context.LoanApprovals
            .Include(a => a.Account)
                .ThenInclude(a => a!.Customer)
            .Include(a => a.Assessment)
            .Include(a => a.Application)
            .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);

        if (approval == null)
            return "Approval record not found";

        // Calculate loan details with industry-standard fees
        var principal = approval.ApprovedAmount ?? 0;
        var annualRate = approval.ApprovedInterestRate ?? 0;
        var termMonths = approval.ApprovedTermMonths ?? 12;
        var monthlyRate = annualRate / 100 / 12;

        // Calculate monthly payment using amortization formula
        decimal monthlyPayment;
        if (monthlyRate == 0)
        {
            monthlyPayment = principal / termMonths;
        }
        else
        {
            monthlyPayment = principal * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, termMonths))
                / ((decimal)Math.Pow(1 + (double)monthlyRate, termMonths) - 1);
        }
        monthlyPayment = Math.Round(monthlyPayment, 2);

        // Industry-standard fees (Philippines banking standards)
        var documentaryStampTax = Math.Round(principal * 0.015m, 2); // 1.5% DST
        var processingFee = Math.Round(principal * 0.01m, 2); // 1% processing fee
        var notarialFee = 500m; // Fixed notarial fee
        var insuranceFee = Math.Round(principal * 0.002m * termMonths / 12, 2); // Credit life insurance
        var totalFees = documentaryStampTax + processingFee + notarialFee + insuranceFee;

        // Total interest and payable
        var totalInterest = (monthlyPayment * termMonths) - principal;
        var totalPayable = (monthlyPayment * termMonths) + totalFees;

        // Late payment penalty (0.05% per day)
        var dailyPenaltyRate = 0.0005m;
        var monthlyPenaltyExample = Math.Round(monthlyPayment * dailyPenaltyRate * 30, 2);

        var contractContent = new System.Text.StringBuilder();
        contractContent.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        contractContent.AppendLine("║                         LOAN AGREEMENT CONTRACT                               ║");
        contractContent.AppendLine("║                    FINEBANK BANKING CORPORATION                               ║");
        contractContent.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        contractContent.AppendLine();
        contractContent.AppendLine($"Contract Reference: CONTRACT-{approval.ApprovalId:D6}-{DateTime.Now:yyyyMMdd}");
        contractContent.AppendLine($"Date Prepared: {DateTime.Now:MMMM dd, yyyy}");
        contractContent.AppendLine($"Effective Date: Upon fund release");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                              PARTIES TO THE CONTRACT                          ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine("CREDITOR (LENDER):");
        contractContent.AppendLine("  Name: FINEBANK Banking Corporation");
        contractContent.AppendLine("  Address: FINEBANK Tower, Makati City, Philippines");
        contractContent.AppendLine("  License: BSP Licensed Universal Bank");
        contractContent.AppendLine();
        contractContent.AppendLine("DEBTOR (BORROWER):");
        contractContent.AppendLine($"  Name: {approval.Account?.Customer?.FullName ?? "N/A"}");
        contractContent.AppendLine($"  Account Number: {approval.Account?.AccountNumber ?? "N/A"}");
        contractContent.AppendLine($"  Account ID: {approval.AccountId}");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                              LOAN PARTICULARS                                 ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"  Approval Reference:        APV-{approval.ApprovalId:D6}");
        contractContent.AppendLine($"  Application Number:        {approval.Application?.ApplicationNumber ?? "N/A"}");
        contractContent.AppendLine($"  Loan Type:                 {approval.Application?.LoanType ?? "Personal Loan"}");
        contractContent.AppendLine($"  Purpose:                   {approval.Application?.Purpose ?? "General Purpose"}");
        contractContent.AppendLine();
        contractContent.AppendLine("  FINANCIAL TERMS:");
        contractContent.AppendLine($"  ├─ Principal Amount:       ₱{principal:N2}");
        contractContent.AppendLine($"  ├─ Annual Interest Rate:   {annualRate:N2}% per annum");
        contractContent.AppendLine($"  ├─ Monthly Interest Rate:  {monthlyRate * 100:N4}%");
        contractContent.AppendLine($"  ├─ Loan Term:              {termMonths} months");
        contractContent.AppendLine($"  ├─ Monthly Payment:        ₱{monthlyPayment:N2}");
        contractContent.AppendLine($"  ├─ Total Interest:         ₱{totalInterest:N2}");
        contractContent.AppendLine($"  └─ Total Amount Payable:   ₱{monthlyPayment * termMonths:N2}");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                         FEES AND CHARGES (One-time)                           ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"  ├─ Documentary Stamp Tax (1.5%):      ₱{documentaryStampTax:N2}");
        contractContent.AppendLine($"  ├─ Processing Fee (1%):               ₱{processingFee:N2}");
        contractContent.AppendLine($"  ├─ Notarial Fee:                      ₱{notarialFee:N2}");
        contractContent.AppendLine($"  ├─ Credit Life Insurance:             ₱{insuranceFee:N2}");
        contractContent.AppendLine($"  └─ TOTAL FEES:                        ₱{totalFees:N2}");
        contractContent.AppendLine();
        contractContent.AppendLine($"  NET PROCEEDS (Principal - Fees):      ₱{principal - totalFees:N2}");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                            PAYMENT SCHEDULE                                   ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"  First Payment Due:   {DateTime.Now.AddMonths(1):MMMM dd, yyyy}");
        contractContent.AppendLine($"  Last Payment Due:    {DateTime.Now.AddMonths(termMonths):MMMM dd, yyyy}");
        contractContent.AppendLine($"  Payment Due Date:    Same day of each month");
        contractContent.AppendLine($"  Payment Amount:      ₱{monthlyPayment:N2} per month");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                         PENALTY AND DEFAULT CHARGES                           ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"  Late Payment Penalty:      0.05% per day on amount due");
        contractContent.AppendLine($"  Example (30 days late):    ₱{monthlyPenaltyExample:N2} on ₱{monthlyPayment:N2} due");
        contractContent.AppendLine($"  Returned Check Fee:        ₱500.00 per instance");
        contractContent.AppendLine($"  Collection Fee:            10% of outstanding balance after 90 days");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                          TERMS AND CONDITIONS                                 ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine("  1. The DEBTOR agrees to repay the loan according to the payment schedule.");
        contractContent.AppendLine("  2. Payments must be made on or before the due date each month.");
        contractContent.AppendLine("  3. Late payments will incur penalties as stated above.");
        contractContent.AppendLine("  4. The CREDITOR reserves the right to recall the loan upon default.");
        contractContent.AppendLine("  5. Prepayment is allowed without penalty after 6 months.");
        contractContent.AppendLine("  6. All disputes shall be settled in Philippine courts.");
        contractContent.AppendLine("  7. This contract is governed by Philippine banking laws.");
        if (!string.IsNullOrEmpty(approval.SpecialConditions))
        {
            contractContent.AppendLine($"  8. SPECIAL CONDITIONS: {approval.SpecialConditions}");
        }
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                              SIGNATURES                                       ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine();
        contractContent.AppendLine("  _________________________          _________________________");
        contractContent.AppendLine($"  {approval.Account?.Customer?.FullName ?? "BORROWER"}");
        contractContent.AppendLine("  (Debtor/Borrower)                  (Authorized Bank Officer)");
        contractContent.AppendLine();
        contractContent.AppendLine($"  Date: _______________              Date: _______________");
        contractContent.AppendLine();
        contractContent.AppendLine();
        contractContent.AppendLine("  WITNESSES:");
        contractContent.AppendLine();
        contractContent.AppendLine("  1. _________________________       2. _________________________");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                           APPROVAL INFORMATION                                ");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"  Approved By:     {approval.ApprovedBy}");
        contractContent.AppendLine($"  Approval Date:   {approval.ApprovalDate:MMMM dd, yyyy hh:mm tt}");
        contractContent.AppendLine($"  Contract Valid:  60 days from approval date");
        contractContent.AppendLine();
        contractContent.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        contractContent.AppendLine("║                      END OF CONTRACT DOCUMENT                                ║");
        contractContent.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");

        return contractContent.ToString();
    }

    /// <summary>
    /// Get active loans
    /// </summary>
    public async Task<List<Loan>> GetActiveLoansAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Loans
            .Where(l => l.Status == "ACTIVE")
            .Include(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Include(l => l.LoanAssessment)
            .Include(l => l.LoanApprovalRecord)
            .Include(l => l.PaymentSchedules)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get completed loans
    /// </summary>
    public async Task<List<Loan>> GetCompletedLoansAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Loans
            .Where(l => l.Status == "COMPLETED")
            .Include(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Include(l => l.LoanAssessment)
            .Include(l => l.LoanApprovalRecord)
            .OrderByDescending(l => l.EndDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending applications by role - Updated for 5-stage workflow
    /// </summary>
    public async Task<List<LoanApplication>> GetPendingApplicationsAsync(string role)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        IQueryable<LoanApplication> query = _context.LoanApplications
            .Include(a => a.Account)
            .ThenInclude(a => a.Customer);

        // Filter based on role-specific status
        if (role == "TELLER")
        {
            // Teller sees PENDING_TELLER_REVIEW (new) + SUBMITTED/VERIFIED (legacy)
            query = query.Where(a =>
                a.Status == LoanStatus.PENDING_TELLER_REVIEW ||
                a.Status == LoanStatus.SUBMITTED ||
                a.Status == LoanStatus.VERIFIED);
        }
        else if (role == "ACCOUNTANT")
        {
            // Accountant sees PENDING_ACCOUNTANT_REVIEW (new) + ENCODED (legacy)
            query = query.Where(a =>
                a.Status == LoanStatus.PENDING_ACCOUNTANT_REVIEW ||
                a.Status == LoanStatus.ENCODED);
        }
        else if (role == "FINANCEMANAGER" || role == "FINANCE_MANAGER")
        {
            // Finance Manager sees PENDING_FINANCEMANAGER_APPROVAL (new) + ASSESSED (legacy)
            query = query.Where(a =>
                a.Status == LoanStatus.PENDING_FINANCEMANAGER_APPROVAL ||
                a.Status == LoanStatus.ASSESSED);
        }
        else if (role == "TELLER_RELEASE")
        {
            // Teller sees approved loans ready for release
            query = query.Where(a =>
                a.Status == LoanStatus.APPROVED_READY_FOR_RELEASE ||
                a.Status == LoanStatus.APPROVED);
        }

        return await query.OrderByDescending(a => a.ApplicationDate).ToListAsync();
    }

    /// <summary>
    /// Encode application documents
    /// </summary>
    public async Task<LoanApplication> EncodeApplicationAsync(int applicationId, string encodedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new InvalidOperationException($"Application {applicationId} not found");

            if (application.Status != "VERIFIED")
                throw new InvalidOperationException("Application must be verified before encoding");

            application.Status = "ENCODED";
            _context.LoanApplications.Update(application);
            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to encode application", ex);
        }
    }

    /// <summary>
    /// Release funds for approved loan
    /// </summary>
    public async Task<LoanDisbursal> ReleaseFundsAsync(int approvalId, decimal amount, string disbursedTo, string processedBy)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var approval = await _context.LoanApprovals
                .Include(a => a.Assessment)
                .Include(a => a.Application)
                .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);

            if (approval == null)
                throw new InvalidOperationException($"Approval {approvalId} not found");

            // Create Loan record if it doesn't exist yet
            var existingLoan = await _context.Loans.FirstOrDefaultAsync(l => l.ApprovalId == approvalId);
            Loan loan;

            if (existingLoan == null)
            {
                var approvedAmount = approval.ApprovedAmount ?? approval.Assessment?.LoanAmount ?? 0m;
                var approvedRate = approval.ApprovedInterestRate ?? approval.Assessment?.InterestRate ?? 0m;
                var approvedTerm = approval.ApprovedTermMonths ?? approval.Assessment?.TermMonths ?? 12;

                loan = new Loan
                {
                    LoanNumber = $"LN-{DateTime.Now:yyyyMMddHHmmss}",
                    ApprovalId = approvalId,
                    AccountId = approval.AccountId,
                    LoanAmount = approvedAmount,
                    InterestRate = approvedRate,
                    TermMonths = approvedTerm,
                    MonthlyPayment = approval.Assessment?.ComputedMonthlyPayment ?? 0m,
                    OutstandingBalance = approvedAmount,
                    Status = "ACTIVE",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(approvedTerm),
                    CreatedAt = DateTime.Now
                };
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
            }
            else
            {
                loan = existingLoan;
            }

            // Create disbursal record
            var disbursal = new LoanDisbursal
            {
                LoanId = loan.LoanId,
                ApprovalId = approvalId,
                DisbursalAmount = amount,
                DisbursalDate = DateTime.Now,
                DisbursalStatus = "RELEASED",
                DisbursedTo = disbursedTo,
                ProcessedBy = processedBy,
                Reference = $"DISB-{DateTime.Now:yyyyMMddHHmmss}"
            };

            // Update application status to DISBURSED
            if (approval.Application != null)
            {
                approval.Application.Status = "DISBURSED";
                _context.LoanApplications.Update(approval.Application);
            }

            _context.LoanDisbursals.Add(disbursal);
            await _context.SaveChangesAsync();

            // Post to General Ledger automatically
            try
            {
                if (_glPostingService != null)
                {
                    await _glPostingService.PostLoanDisbursementAsync(
                        disbursal.DisbursalId,
                        loan.LoanNumber,
                        amount,
                        disbursedTo,
                        disbursal.DisbursalDate);
                }
            }
            catch
            {
                // GL posting failure should not prevent disbursement from being recorded
            }

            return disbursal;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to release funds", ex);
        }
    }

    // =====================================================================
    // HELPER METHODS - TRANSACTION HISTORY & INVOICE GENERATION
    // =====================================================================

    /// <summary>
    /// Log transaction history for all loan operations
    /// </summary>
    private async Task LogTransactionHistoryAsync(
        int accountId,
        string transactionType,
        decimal amount,
        string description,
        string processedBy,
        int? applicationId = null,
        int? approvalId = null,
        int? loanId = null,
        string? reference = null,
        string? status = null,
        decimal? penaltyAmount = null,
        decimal? balanceBefore = null,
        decimal? balanceAfter = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var history = new LoanTransactionHistory
            {
                LoanId = loanId,
                ApplicationId = applicationId,
                ApprovalId = approvalId,
                AccountId = accountId,
                TransactionType = transactionType,
                Description = description,
                Amount = amount,
                PenaltyAmount = penaltyAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Reference = reference,
                ProcessedBy = processedBy,
                TransactionDate = DateTime.Now,
                Status = status
            };

            _context.LoanTransactionHistory.Add(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Silently fail - history logging should not break main operation
        }
    }

    /// <summary>
    /// Generate invoice for loan release or payment
    /// </summary>
    private async Task<LoanInvoice> GenerateInvoiceAsync(
        string invoiceType,
        int loanId,
        int accountId,
        string customerName,
        decimal totalAmount,
        string processedBy,
        int? paymentId = null,
        int? disbursalId = null,
        decimal principalAmount = 0,
        decimal interestAmount = 0,
        decimal penaltyAmount = 0,
        decimal feesAmount = 0,
        string? paymentMethod = null,
        string? reference = null,
        decimal? balanceBefore = null,
        decimal? balanceAfter = null,
        string? notes = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            // Get account number
            var account = await _context.CustomerAccounts.FindAsync(accountId);
            var accountNumber = account?.AccountNumber;

            var invoiceNumber = $"INV-{invoiceType.Substring(0, 3)}-{DateTime.Now:yyyyMMddHHmmss}";

            var invoice = new LoanInvoice
            {
                InvoiceNumber = invoiceNumber,
                InvoiceType = invoiceType,
                LoanId = loanId,
                PaymentId = paymentId,
                DisbursalId = disbursalId,
                AccountId = accountId,
                CustomerName = customerName,
                AccountNumber = accountNumber,
                PrincipalAmount = principalAmount,
                InterestAmount = interestAmount,
                PenaltyAmount = penaltyAmount,
                FeesAmount = feesAmount,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                Reference = reference,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ProcessedBy = processedBy,
                InvoiceDate = DateTime.Now,
                Notes = notes
            };

            _context.LoanInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }
        catch (Exception)
        {
            // Return empty invoice if generation fails - don't break main operation
            return new LoanInvoice { InvoiceNumber = "FAILED" };
        }
    }

    /// <summary>
    /// Get transaction history for a specific loan or account
    /// </summary>
    public async Task<List<LoanTransactionHistory>> GetTransactionHistoryAsync(int? loanId = null, int? accountId = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var query = _context.LoanTransactionHistory.AsQueryable();

        if (loanId.HasValue)
            query = query.Where(h => h.LoanId == loanId);

        if (accountId.HasValue)
            query = query.Where(h => h.AccountId == accountId);

        return await query
            .OrderByDescending(h => h.TransactionDate)
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    public async Task<LoanInvoice?> GetInvoiceAsync(int invoiceId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanInvoices
            .Include(i => i.Account)
            .Include(i => i.Loan)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    /// <summary>
    /// Get invoices for a loan
    /// </summary>
    public async Task<List<LoanInvoice>> GetLoanInvoicesAsync(int loanId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanInvoices
            .Where(i => i.LoanId == loanId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get latest invoice by payment or disbursal
    /// </summary>
    public async Task<LoanInvoice?> GetLatestInvoiceAsync(int? paymentId = null, int? disbursalId = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var query = _context.LoanInvoices.AsQueryable();

        if (paymentId.HasValue)
            query = query.Where(i => i.PaymentId == paymentId);

        if (disbursalId.HasValue)
            query = query.Where(i => i.DisbursalId == disbursalId);

        return await query.OrderByDescending(i => i.InvoiceDate).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get FM approval history (approved/declined)
    /// </summary>
    public async Task<List<LoanApproval>> GetApprovalHistoryAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApprovals
            .Include(a => a.Assessment)
            .ThenInclude(a => a.Application)
            .ThenInclude(a => a.Account)
            .ThenInclude(a => a.Customer)
            .Where(a => a.ApprovalStatus == "APPROVED" || a.ApprovalStatus == "DECLINED")
            .OrderByDescending(a => a.ApprovalDate)
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending loan applications for a specific account (customer-side)
    /// </summary>
    public async Task<List<LoanApplication>> GetPendingApplicationsByAccountAsync(int accountId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApplications
            .Where(a => a.AccountId == accountId &&
                (a.Status == LoanStatus.SUBMITTED ||
                 a.Status == LoanStatus.PENDING_TELLER_REVIEW ||
                 a.Status == LoanStatus.VERIFIED ||
                 a.Status == LoanStatus.ENCODED ||
                 a.Status == LoanStatus.PENDING_ACCOUNTANT_REVIEW ||
                 a.Status == LoanStatus.ASSESSED ||
                 a.Status == LoanStatus.PENDING_FINANCEMANAGER_APPROVAL))
            .Include(a => a.Account)
            .ThenInclude(a => a.Customer)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Cancel a loan application (customer-side) - only if still in early stages
    /// </summary>
    public async Task<(bool success, string message)> CancelApplicationAsync(int applicationId, int accountId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                return (false, "Application not found.");

            // Verify ownership
            if (application.AccountId != accountId)
                return (false, "You are not authorized to cancel this application.");

            // Only allow cancellation if in early stages
            var cancellableStatuses = new[] {
                LoanStatus.SUBMITTED,
                LoanStatus.PENDING_TELLER_REVIEW,
                LoanStatus.VERIFIED
            };

            if (!cancellableStatuses.Contains(application.Status))
                return (false, "This application can no longer be cancelled. It has already progressed to assessment stage.");

            application.Status = "CANCELLED";
            application.RejectionReason = "Cancelled by customer";
            application.RejectedAt = DateTime.Now;
            application.RejectedBy = "CUSTOMER";

            await _context.SaveChangesAsync();

            return (true, $"Application {application.ApplicationNumber} has been cancelled successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error cancelling application: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a loan application (customer-side) - only if still in early stages
    /// </summary>
    public async Task<(bool success, string message)> UpdateApplicationAsync(
        int applicationId,
        int accountId,
        decimal? newAmount = null,
        string? newPurpose = null,
        string? newEmploymentStatus = null,
        decimal? newMonthlyIncome = null)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                return (false, "Application not found.");

            // Verify ownership
            if (application.AccountId != accountId)
                return (false, "You are not authorized to update this application.");

            // Only allow updates if in early stages
            var editableStatuses = new[] {
                LoanStatus.SUBMITTED,
                LoanStatus.PENDING_TELLER_REVIEW
            };

            if (!editableStatuses.Contains(application.Status))
                return (false, "This application can no longer be edited. It has already progressed beyond the initial review stage.");

            // Update fields if provided
            if (newAmount.HasValue && newAmount.Value > 0)
                application.RequestedAmount = newAmount.Value;

            if (!string.IsNullOrWhiteSpace(newPurpose))
                application.Purpose = newPurpose;

            if (!string.IsNullOrWhiteSpace(newEmploymentStatus))
                application.EmploymentStatus = newEmploymentStatus;

            if (newMonthlyIncome.HasValue && newMonthlyIncome.Value >= 0)
                application.MonthlyIncome = newMonthlyIncome.Value;

            await _context.SaveChangesAsync();

            return (true, $"Application {application.ApplicationNumber} has been updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating application: {ex.Message}");
        }
    }

    /// <summary>
    /// Get cancelled and rejected loan applications for a specific account (customer-side history)
    /// </summary>
    public async Task<List<LoanApplication>> GetCancelledAndRejectedApplicationsByAccountAsync(int accountId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApplications
            .Where(a => a.AccountId == accountId &&
                (a.Status == "CANCELLED" ||
                 a.Status == LoanStatus.REJECTED_TELLER ||
                 a.Status == LoanStatus.REJECTED_ACCOUNTANT ||
                 a.Status == LoanStatus.REJECTED_FINANCEMANAGER ||
                 a.Status == "REJECTED"))
            .Include(a => a.Account)
            .ThenInclude(a => a.Customer)
            .OrderByDescending(a => a.RejectedAt ?? a.ApplicationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get all cancelled and rejected loan applications (for Teller review history)
    /// </summary>
    public async Task<List<LoanApplication>> GetAllCancelledAndRejectedApplicationsAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.LoanApplications
            .Where(a => a.Status == "CANCELLED" ||
                 a.Status == LoanStatus.REJECTED_TELLER ||
                 a.Status == LoanStatus.REJECTED_ACCOUNTANT ||
                 a.Status == LoanStatus.REJECTED_FINANCEMANAGER ||
                 a.Status == "REJECTED")
            .Include(a => a.Account)
            .ThenInclude(a => a.Customer)
            .OrderByDescending(a => a.RejectedAt ?? a.ApplicationDate)
            .ToListAsync();
    }
}
