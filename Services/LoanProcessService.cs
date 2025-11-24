using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;
using FinanceBank.Data;

namespace FinanceBank.Services;

/// <summary>
/// Complete Loan Process Service - Handles all stages from application to payment
/// </summary>
public class LoanProcessService
{
    private readonly BFASDbContext _context;
    private const decimal DAILY_PENALTY_RATE = 0.0005m; // 0.05% daily (BPI Standard)

    public LoanProcessService(BFASDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // STAGE 1: APPLICATION SUBMISSION (CUSTOMER)
    // =====================================================================

    /// <summary>
    /// Submit loan application
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
        try
        {
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
                Status = "SUBMITTED",
                SubmittedBy = submittedBy,
                ApplicationDate = DateTime.Now
            };

            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to submit loan application", ex);
        }
    }

    // =====================================================================
    // STAGE 2: IDENTITY VERIFICATION (TELLER)
    // =====================================================================

    /// <summary>
    /// Verify applicant identity - Teller verification
    /// </summary>
    public async Task<LoanApplication> VerifyIdentityAsync(int applicationId, string verifiedBy)
    {
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
        try
        {
            // Validate application exists and is encoded
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException($"Application {applicationId} not found");

            if (application.Status != "ENCODED")
                throw new InvalidOperationException("Application must be encoded before assessment");

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

            // Update application status to ASSESSED
            application.Status = "ASSESSED";
            _context.LoanApplications.Update(application);
            
            _context.LoanAssessments.Add(assessment);
            await _context.SaveChangesAsync();

            return assessment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create loan assessment", ex);
        }
    }

    // =====================================================================
    // STAGE 4: APPROVAL (FINANCE MANAGER)
    // =====================================================================

    /// <summary>
    /// Approve loan application
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
        try
        {
            var assessment = await _context.LoanAssessments.FindAsync(assessmentId);
            if (assessment == null)
                throw new KeyNotFoundException($"Assessment {assessmentId} not found");

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

            return approval;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to approve loan", ex);
        }
    }

    /// <summary>
    /// Decline loan application
    /// </summary>
    public async Task<LoanApproval> DeclineLoanAsync(
        int assessmentId,
        int applicationId,
        int accountId,
        string declinationReason,
        string declinedBy)
    {
        try
        {
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

            return approval;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decline loan", ex);
        }
    }

    // =====================================================================
    // STAGE 5: DISBURSEMENT (TELLER + ACCOUNTANT)
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
    /// Process loan payment
    /// </summary>
    public async Task<LoanPayment> ProcessPaymentAsync(
        int loanId,
        int scheduleId,
        decimal paymentAmount,
        string paymentMethod,
        string processedBy,
        string? reference = null)
    {
        try
        {
            var loan = await _context.Loans.FindAsync(loanId);
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

            // Record payment
            var payment = new LoanPayment
            {
                ScheduleId = scheduleId,
                LoanId = loanId,
                PaymentAmount = paymentAmount,
                PenaltyPaid = penaltyAmount,
                PaymentMethod = paymentMethod,
                Reference = reference,
                ProcessedBy = processedBy,
                PaymentDate = DateTime.Now,
                IsLatePayment = isLatePayment
            };

            // Update schedule status
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

            // Check if loan is fully paid
            if (loan.OutstandingBalance <= 0)
            {
                loan.Status = "Completed";
            }

            _context.LoanPayments.Add(payment);
            await _context.SaveChangesAsync();

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
        return await _context.LoanApplications
            .Include(a => a.Account)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
    }

    /// <summary>
    /// Get loan details
    /// </summary>
    public async Task<Loan?> GetLoanAsync(int loanId)
    {
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
        return await _context.LoanAssessments
            .Where(a => a.AssessmentStatus == "ASSESSED")
            .Include(a => a.Account)
            .Include(a => a.Application)
            .OrderBy(a => a.AssessmentDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get approved loans ready for contract preparation
    /// </summary>
    public async Task<List<LoanApproval>> GetApprovedLoansAsync()
    {
        return await _context.LoanApprovals
            .Where(a => a.ApprovalStatus == "APPROVED")
            .Include(a => a.Account)
            .Include(a => a.Assessment)
            .Include(a => a.Application)
            .OrderByDescending(a => a.ApprovalDate)
            .ToListAsync();
    }

    /// <summary>
    /// Prepare loan contract document
    /// </summary>
    public async Task<string> PrepareContractAsync(int approvalId)
    {
        var approval = await _context.LoanApprovals
            .Include(a => a.Account)
            .Include(a => a.Assessment)
            .Include(a => a.Application)
            .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);

        if (approval == null)
            return "Approval record not found";

        var contractContent = new System.Text.StringBuilder();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════");
        contractContent.AppendLine("                        LOAN AGREEMENT CONTRACT");
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════");
        contractContent.AppendLine();
        contractContent.AppendLine($"Date: {DateTime.Now:MMM dd, yyyy}");
        contractContent.AppendLine();
        contractContent.AppendLine("PARTIES:");
        contractContent.AppendLine($"  Creditor: FinanceBank Corporation");
        contractContent.AppendLine($"  Debtor: {approval.Account?.Customer?.FullName ?? "N/A"}");
        contractContent.AppendLine($"  Account ID: {approval.AccountId}");
        contractContent.AppendLine();
        contractContent.AppendLine("LOAN DETAILS:");
        contractContent.AppendLine($"  Approval ID: {approval.ApprovalId}");
        contractContent.AppendLine($"  Loan Type: {approval.Application?.LoanType ?? "N/A"}");
        contractContent.AppendLine($"  Principal Amount: ₱{approval.ApprovedAmount:N2}");
        contractContent.AppendLine($"  Interest Rate: {approval.ApprovedInterestRate}% per annum");
        contractContent.AppendLine($"  Term: {approval.ApprovedTermMonths} months");
        contractContent.AppendLine($"  Purpose: {approval.Application?.Purpose ?? "N/A"}");
        contractContent.AppendLine();
        contractContent.AppendLine("PAYMENT SCHEDULE:");
        contractContent.AppendLine($"  Monthly Payment: {(approval.Assessment?.ComputedMonthlyPayment ?? 0):N2}");
        contractContent.AppendLine($"  Total Amount Payable: {(approval.Assessment?.TotalPayable ?? 0):N2}");
        contractContent.AppendLine();
        contractContent.AppendLine("TERMS AND CONDITIONS:");
        contractContent.AppendLine("  1. The debtor agrees to repay the principal with interest.");
        contractContent.AppendLine("  2. Payments must be made on or before the due date.");
        contractContent.AppendLine("  3. Late payments incur penalties as per bank policy.");
        contractContent.AppendLine("  4. Loan may be recalled if debtor defaults on payments.");
        if (!string.IsNullOrEmpty(approval.SpecialConditions))
        {
            contractContent.AppendLine($"  5. Special Conditions: {approval.SpecialConditions}");
        }
        contractContent.AppendLine();
        contractContent.AppendLine("AUTHORIZED BY:");
        contractContent.AppendLine($"  {approval.ApprovedBy}");
        contractContent.AppendLine($"  {approval.ApprovalDate:MMM dd, yyyy}");
        contractContent.AppendLine();
        contractContent.AppendLine("═══════════════════════════════════════════════════════════════");

        return contractContent.ToString();
    }

    /// <summary>
    /// Get active loans
    /// </summary>
    public async Task<List<Loan>> GetActiveLoansAsync()
    {
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
    /// Get pending applications by role
    /// </summary>
    public async Task<List<LoanApplication>> GetPendingApplicationsAsync(string role)
    {
        IQueryable<LoanApplication> query = _context.LoanApplications
            .Include(a => a.Account)
            .ThenInclude(a => a.Customer);

        // Filter based on role-specific status
        if (role == "TELLER")
        {
            // Teller sees SUBMITTED (to verify) and VERIFIED (to encode)
            query = query.Where(a => a.Status == "SUBMITTED" || a.Status == "VERIFIED");
        }
        else if (role == "ACCOUNTANT")
        {
            // Accountant sees ENCODED (to assess) - applications that have been encoded by teller
            query = query.Where(a => a.Status == "ENCODED");
        }

        return await query.OrderByDescending(a => a.ApplicationDate).ToListAsync();
    }

    /// <summary>
    /// Encode application documents
    /// </summary>
    public async Task<LoanApplication> EncodeApplicationAsync(int applicationId, string encodedBy)
    {
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

            return disbursal;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to release funds", ex);
        }
    }
}

