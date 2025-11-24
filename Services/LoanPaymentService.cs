using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;
using FinanceBank.Data;

namespace FinanceBank.Services;

/// <summary>
/// Loan Payment Service - Handles payment cycles, penalties, and payment processing
/// </summary>
public class LoanPaymentService
{
    private readonly BFASDbContext _context;
    private const decimal DAILY_PENALTY_RATE = 0.0005m; // 0.05% daily (BPI Standard)

    public LoanPaymentService(BFASDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // PAYMENT SCHEDULE MANAGEMENT
    // =====================================================================

    /// <summary>
    /// Get upcoming payments for a loan
    /// </summary>
    public async Task<List<LoanPaymentSchedule>> GetUpcomingPaymentsAsync(int loanId, int daysAhead = 30)
    {
        var futureDate = DateTime.Now.AddDays(daysAhead);

        return await _context.LoanPaymentSchedules
            .Where(s => s.LoanId == loanId 
                && s.DueDate <= futureDate 
                && (s.PaymentStatus == "PENDING" || s.PaymentStatus == "OVERDUE"))
            .OrderBy(s => s.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get overdue payments
    /// </summary>
    public async Task<List<LoanPaymentSchedule>> GetOverduePaymentsAsync(int loanId)
    {
        return await _context.LoanPaymentSchedules
            .Where(s => s.LoanId == loanId 
                && s.PaymentStatus == "OVERDUE" 
                && s.DueDate < DateTime.Now)
            .OrderBy(s => s.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get payment history for a loan
    /// </summary>
    public async Task<List<LoanPayment>> GetPaymentHistoryAsync(int loanId)
    {
        return await _context.LoanPayments
            .Where(p => p.LoanId == loanId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    // =====================================================================
    // PENALTY CALCULATIONS
    // =====================================================================

    /// <summary>
    /// Calculate daily penalty for a payment
    /// </summary>
    public decimal CalculateDailyPenalty(decimal outstandingBalance, int daysOverdue)
    {
        return outstandingBalance * DAILY_PENALTY_RATE * daysOverdue;
    }

    /// <summary>
    /// Calculate total penalty amount including accumulated days
    /// </summary>
    public async Task<decimal> CalculateTotalPenaltyAsync(int scheduleId)
    {
        try
        {
            var schedule = await _context.LoanPaymentSchedules.FindAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Schedule {scheduleId} not found");

            if (schedule.DueDate >= DateTime.Now)
                return 0; // Not overdue yet

            var daysOverdue = (int)(DateTime.Now - schedule.DueDate).TotalDays;
            var penalty = schedule.MinimumPayment * DAILY_PENALTY_RATE * daysOverdue;

            return Math.Round(penalty, 2);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to calculate penalty", ex);
        }
    }

    /// <summary>
    /// Get total penalties for a loan
    /// </summary>
    public async Task<decimal> GetTotalPenaltiesAsync(int loanId)
    {
        return await _context.LoanPayments
            .Where(p => p.LoanId == loanId)
            .SumAsync(p => p.PenaltyPaid);
    }

    // =====================================================================
    // PAYMENT PROCESSING
    // =====================================================================

    /// <summary>
    /// Process full payment for a schedule
    /// </summary>
    public async Task<LoanPayment> ProcessFullPaymentAsync(
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
                throw new KeyNotFoundException($"Schedule {scheduleId} not found");

            var isLate = DateTime.Now > schedule.DueDate;
            var penaltyAmount = 0m;
            var daysOverdue = 0;

            // Calculate penalty if late
            if (isLate)
            {
                daysOverdue = (int)(DateTime.Now - schedule.DueDate).TotalDays;
                penaltyAmount = CalculateDailyPenalty(schedule.MinimumPayment, daysOverdue);
            }

            var totalDue = schedule.MinimumPayment + penaltyAmount;

            if (paymentAmount < totalDue)
                throw new InvalidOperationException($"Payment amount ({paymentAmount}) is less than total due ({totalDue})");

            // Create payment record
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
                IsLatePayment = isLate
            };

            // Update schedule
            schedule.PaymentStatus = "PAID";
            schedule.DaysOverdue = daysOverdue;
            schedule.Penalty = penaltyAmount;

            // Update loan
            loan.OutstandingBalance -= schedule.PrincipalAmount;
            loan.LastPaymentDate = DateTime.Now;
            loan.TotalPenalties += penaltyAmount;

            if (isLate)
            {
                loan.CumulativeLateDays += daysOverdue;

                // Create violation record
                await CreateViolationAsync(
                    loanId,
                    scheduleId,
                    "LATE_PAYMENT",
                    daysOverdue,
                    penaltyAmount,
                    loan.OutstandingBalance
                );
            }

            // Check if loan is complete
            if (loan.OutstandingBalance <= 0)
            {
                loan.Status = "Completed";
                loan.EndDate = DateTime.Now;
            }

            _context.LoanPayments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process full payment", ex);
        }
    }

    /// <summary>
    /// Process partial payment for a schedule
    /// </summary>
    public async Task<LoanPayment> ProcessPartialPaymentAsync(
        int loanId,
        int scheduleId,
        decimal partialAmount,
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
                throw new KeyNotFoundException($"Schedule {scheduleId} not found");

            if (partialAmount <= 0)
                throw new InvalidOperationException("Partial amount must be greater than zero");

            var isLate = DateTime.Now > schedule.DueDate;
            var penaltyAmount = 0m;
            var daysOverdue = 0;

            // Calculate penalty if late
            if (isLate)
            {
                daysOverdue = (int)(DateTime.Now - schedule.DueDate).TotalDays;
                penaltyAmount = CalculateDailyPenalty(schedule.MinimumPayment, daysOverdue);

                // Create violation for partial payment
                await CreateViolationAsync(
                    loanId,
                    scheduleId,
                    "LATE_PAYMENT",
                    daysOverdue,
                    penaltyAmount,
                    loan.OutstandingBalance
                );
            }

            // Create payment record
            var payment = new LoanPayment
            {
                ScheduleId = scheduleId,
                LoanId = loanId,
                PaymentAmount = partialAmount,
                PenaltyPaid = penaltyAmount,
                PaymentMethod = paymentMethod,
                Reference = reference,
                ProcessedBy = processedBy,
                PaymentDate = DateTime.Now,
                IsLatePayment = isLate
            };

            // Update schedule
            schedule.PaymentStatus = "PARTIAL";
            schedule.DaysOverdue = daysOverdue;
            schedule.Penalty = penaltyAmount;

            // Update loan
            var principalPaid = partialAmount - penaltyAmount;
            loan.OutstandingBalance -= principalPaid;
            loan.LastPaymentDate = DateTime.Now;
            loan.TotalPenalties += penaltyAmount;

            if (isLate)
                loan.CumulativeLateDays += daysOverdue;

            _context.LoanPayments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process partial payment", ex);
        }
    }

    // =====================================================================
    // MISSED PAYMENT HANDLING
    // =====================================================================
    
    /// <summary>
    /// Handle missed payment Creates violation and marks as overdue
    /// </summary>
    public async Task HandleMissedPaymentAsync(int scheduleId)
    {
        try
        {
            var schedule = await _context.LoanPaymentSchedules.FindAsync(scheduleId);
            if (schedule == null)
                throw new KeyNotFoundException($"Schedule {scheduleId} not found");

            if (DateTime.Now <= schedule.DueDate)
                return; // Not yet missed

            schedule.PaymentStatus = "OVERDUE";

            var loan = await _context.Loans.FindAsync(schedule.LoanId);
            if (loan == null)
                throw new KeyNotFoundException($"Loan {schedule.LoanId} not found");

            var daysOverdue = (int)(DateTime.Now - schedule.DueDate).TotalDays;
            var penaltyAmount = CalculateDailyPenalty(schedule.MinimumPayment, daysOverdue);

            schedule.DaysOverdue = daysOverdue;
            schedule.Penalty = penaltyAmount;

            // Create violation
            await CreateViolationAsync(
                schedule.LoanId,
                scheduleId,
                "MISSED_PAYMENT",
                daysOverdue,
                penaltyAmount,
                loan.OutstandingBalance
            );

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to handle missed payment", ex);
        }
    }

    /// <summary>
    /// Check and update all overdue payments
    /// </summary>
    public async Task UpdateAllOverduePaymentsAsync()
    {
        try
        {
            var overdueSchedules = await _context.LoanPaymentSchedules
                .Where(s => s.PaymentStatus != "PAID" 
                    && s.DueDate < DateTime.Now)
                .ToListAsync();

            foreach (var schedule in overdueSchedules)
            {
                if (schedule.PaymentStatus != "OVERDUE")
                {
                    await HandleMissedPaymentAsync(schedule.ScheduleId);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update overdue payments", ex);
        }
    }

    // =====================================================================
    // VIOLATION MANAGEMENT
    // =====================================================================

    /// <summary>
    /// Create loan violation record
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

            // Check if customer should be blacklisted (3+ unresolved violations)
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan != null)
            {
                var unResolvedCount = await _context.LoanViolations
                    .Where(v => v.LoanId == loanId && !v.IsResolved)
                    .CountAsync();

                if (unResolvedCount >= 3 && !loan.IsBlacklisted)
                {
                    loan.IsBlacklisted = true;
                    loan.BlacklistedAt = DateTime.Now;
                    loan.BlacklistedReason = "Multiple unresolved violations";
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create violation", ex);
        }
    }

    /// <summary>
    /// Get violations for a loan
    /// </summary>
    public async Task<List<LoanViolation>> GetViolationsAsync(int loanId)
    {
        return await _context.LoanViolations
            .Where(v => v.LoanId == loanId)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get unresolved violations
    /// </summary>
    public async Task<List<LoanViolation>> GetUnresolvedViolationsAsync(int loanId)
    {
        return await _context.LoanViolations
            .Where(v => v.LoanId == loanId && !v.IsResolved)
            .ToListAsync();
    }

    /// <summary>
    /// Resolve violation
    /// </summary>
    public async Task ResolveViolationAsync(int violationId)
    {
        try
        {
            var violation = await _context.LoanViolations.FindAsync(violationId);
            if (violation == null)
                throw new KeyNotFoundException($"Violation {violationId} not found");

            violation.IsResolved = true;
            violation.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to resolve violation", ex);
        }
    }

    // =====================================================================
    // LOAN COMPLETION
    // =====================================================================

    /// <summary>
    /// Mark loan as complete when all payments are made
    /// </summary>
    public async Task<Loan> MarkLoanCompleteAsync(int loanId)
    {
        try
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null)
                throw new KeyNotFoundException($"Loan {loanId} not found");

            var pendingSchedules = await _context.LoanPaymentSchedules
                .Where(s => s.LoanId == loanId && s.PaymentStatus != "PAID")
                .CountAsync();

            if (pendingSchedules > 0)
                throw new InvalidOperationException("Loan has pending payments");

            loan.Status = "Completed";
            loan.OutstandingBalance = 0;
            loan.EndDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return loan;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to mark loan complete", ex);
        }
    }

    // =====================================================================
    // LOAN SUMMARY & REPORTING
    // =====================================================================

    /// <summary>
    /// Get comprehensive loan summary
    /// </summary>
    public async Task<LoanSummary> GetLoanSummaryAsync(int loanId)
    {
        try
        {
            var loan = await _context.Loans
                .Include(l => l.PaymentSchedules)
                .Include(l => l.Payments)
                .Include(l => l.Violations)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
                throw new KeyNotFoundException($"Loan {loanId} not found");

            var paidSchedules = loan.PaymentSchedules?.Count(s => s.PaymentStatus == "PAID") ?? 0;
            var totalSchedules = loan.PaymentSchedules?.Count ?? 0;
            var totalPaid = loan.Payments?.Sum(p => p.PaymentAmount) ?? 0;
            var totalPenalties = loan.Payments?.Sum(p => p.PenaltyPaid) ?? 0;
            var unResolvedViolations = loan.Violations?.Count(v => !v.IsResolved) ?? 0;

            return new LoanSummary
            {
                LoanId = loanId,
                LoanNumber = loan.LoanNumber,
                LoanAmount = loan.LoanAmount,
                OutstandingBalance = loan.OutstandingBalance,
                InterestRate = loan.InterestRate,
                TermMonths = loan.TermMonths,
                MonthlyPayment = loan.MonthlyPayment,
                Status = loan.Status,
                TotalPaid = totalPaid,
                TotalPenalties = totalPenalties,
                PaidSchedules = paidSchedules,
                TotalSchedules = totalSchedules,
                CumulativeLateDays = loan.CumulativeLateDays,
                UnresolvedViolations = unResolvedViolations,
                IsBlacklisted = loan.IsBlacklisted,
                StartDate = loan.StartDate,
                EndDate = loan.EndDate,
                LastPaymentDate = loan.LastPaymentDate
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get loan summary", ex);
        }
    }

    /// <summary>
    /// Get account payment summary
    /// </summary>
    public async Task<List<LoanSummary>> GetAccountPaymentSummaryAsync(int accountId)
    {
        try
        {
            var loans = await _context.Loans
                .Where(l => l.AccountId == accountId)
                .Include(l => l.PaymentSchedules)
                .Include(l => l.Payments)
                .Include(l => l.Violations)
                .ToListAsync();

            var summaries = new List<LoanSummary>();

            foreach (var loan in loans)
            {
                var summary = await GetLoanSummaryAsync(loan.LoanId);
                summaries.Add(summary);
            }

            return summaries;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get account payment summary", ex);
        }
    }

    /// <summary>
    /// Process payment - wrapper for payment processing
    /// </summary>
    public async Task<LoanPayment> ProcessPaymentAsync(
        int scheduleId,
        decimal amount,
        string method,
        string processedBy)
    {
        var schedule = await _context.LoanPaymentSchedules.FindAsync(scheduleId);
        if (schedule == null)
            throw new InvalidOperationException($"Schedule {scheduleId} not found");

        var loan = await _context.Loans.FindAsync(schedule.LoanId);
        if (loan == null)
            throw new InvalidOperationException($"Loan {schedule.LoanId} not found");

        // Determine if full or partial payment
        if (amount >= schedule.MinimumPayment)
        {
            return await ProcessFullPaymentAsync(loan.LoanId, scheduleId, amount, method, processedBy);
        }
        else
        {
            return await ProcessPartialPaymentAsync(loan.LoanId, scheduleId, amount, method, processedBy);
        }
    }
}

/// <summary>
/// Loan Summary DTO for reporting
/// </summary>
public class LoanSummary
{
    public int LoanId { get; set; }
    public string? LoanNumber { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string? Status { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPenalties { get; set; }
    public int PaidSchedules { get; set; }
    public int TotalSchedules { get; set; }
    public int CumulativeLateDays { get; set; }
    public int UnresolvedViolations { get; set; }
    public bool IsBlacklisted { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

