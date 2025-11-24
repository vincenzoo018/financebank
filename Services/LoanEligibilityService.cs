using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;
using FinanceBank.Data;

namespace FinanceBank.Services;

/// <summary>
/// Loan Eligibility Service - Checks re-loan eligibility and blacklist status
/// </summary>
public class LoanEligibilityService
{
    private readonly BFASDbContext _context;

    public LoanEligibilityService(BFASDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // RE-LOAN ELIGIBILITY CHECKS
    // =====================================================================

    /// <summary>
    /// Check if customer is eligible for re-loan
    /// Returns: (IsEligible, ReasonIfNotEligible)
    /// </summary>
    public async Task<(bool IsEligible, string Reason)> CheckReLoanEligibilityAsync(int accountId)
    {
        try
        {
            // Check 1: No outstanding balance
            var (hasOutstanding, outstandingReason) = await CheckOutstandingBalanceAsync(accountId);
            if (hasOutstanding)
                return (false, outstandingReason);

            // Check 2: No recent violations (in last 6 months)
            var (hasViolations, violationReason) = await CheckRecentViolationsAsync(accountId, monthsBack: 6);
            if (hasViolations)
                return (false, violationReason);

            // Check 3: Not blacklisted
            var (isBlacklisted, blacklistReason) = await CheckBlacklistStatusAsync(accountId);
            if (isBlacklisted)
                return (false, blacklistReason);

            return (true, "Customer is eligible for re-loan");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check re-loan eligibility", ex);
        }
    }

    /// <summary>
    /// Get detailed eligibility report
    /// </summary>
    public async Task<EligibilityReport> GetDetailedEligibilityReportAsync(int accountId)
    {
        try
        {
            var report = new EligibilityReport { AccountId = accountId };

            // Outstanding balance check
            var (hasOutstanding, outstandingReason) = await CheckOutstandingBalanceAsync(accountId);
            report.HasOutstandingBalance = hasOutstanding;
            report.OutstandingBalanceReason = outstandingReason;

            // Violations check
            var (hasViolations, violationReason) = await CheckRecentViolationsAsync(accountId);
            report.HasRecentViolations = hasViolations;
            report.RecentViolationsReason = violationReason;
            report.ViolationCount = await GetViolationCountAsync(accountId, 6);

            // Blacklist check
            var (isBlacklisted, blacklistReason) = await CheckBlacklistStatusAsync(accountId);
            report.IsBlacklisted = isBlacklisted;
            report.BlacklistReason = blacklistReason;

            // Get outstanding balance details
            report.OutstandingBalance = await GetTotalOutstandingBalanceAsync(accountId);

            // Get loan history
            report.CompletedLoans = await GetCompletedLoansAsync(accountId);
            report.TotalAmountBorrowed = await GetTotalBorrowedAsync(accountId);
            report.OnTimePaymentRate = await CalculateOnTimePaymentRateAsync(accountId);

            // Overall eligibility
            report.IsEligible = !hasOutstanding && !hasViolations && !isBlacklisted;

            return report;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get eligibility report", ex);
        }
    }

    // =====================================================================
    // INDIVIDUAL ELIGIBILITY CHECKS
    // =====================================================================

    /// <summary>
    /// Check outstanding balance
    /// </summary>
    public async Task<(bool HasOutstanding, string Reason)> CheckOutstandingBalanceAsync(int accountId)
    {
        try
        {
            var totalOutstanding = await _context.Loans
                .Where(l => l.AccountId == accountId && l.Status != "Completed")
                .SumAsync(l => l.OutstandingBalance);

            if (totalOutstanding > 0)
                return (true, $"Customer has outstanding loan balance of {totalOutstanding:C}");

            return (false, "No outstanding balance");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check outstanding balance", ex);
        }
    }

    /// <summary>
    /// Check recent violations (in last N months)
    /// </summary>
    public async Task<(bool HasViolations, string Reason)> CheckRecentViolationsAsync(
        int accountId, 
        int monthsBack = 6)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddMonths(-monthsBack);

            var recentViolations = await _context.LoanViolations
                .Join(_context.Loans, v => v.LoanId, l => l.LoanId, (v, l) => new { v, l })
                .Where(x => x.l.AccountId == accountId 
                    && !x.v.IsResolved 
                    && x.v.ViolationDate > cutoffDate)
                .CountAsync();

            if (recentViolations > 0)
                return (true, $"Customer has {recentViolations} unresolved violation(s) in the last {monthsBack} months");

            return (false, $"No violations in the last {monthsBack} months");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check recent violations", ex);
        }
    }

    /// <summary>
    /// Check blacklist status
    /// </summary>
    public async Task<(bool IsBlacklisted, string Reason)> CheckBlacklistStatusAsync(int accountId)
    {
        try
        {
            var isBlacklisted = await _context.Loans
                .Where(l => l.AccountId == accountId && l.IsBlacklisted)
                .AnyAsync();

            if (isBlacklisted)
            {
                var blacklistReason = await _context.Loans
                    .Where(l => l.AccountId == accountId && l.IsBlacklisted)
                    .Select(l => l.BlacklistedReason)
                    .FirstOrDefaultAsync();

                return (true, $"Customer is blacklisted: {blacklistReason ?? "Reason not specified"}");
            }

            return (false, "Customer is not blacklisted");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check blacklist status", ex);
        }
    }

    // =====================================================================
    // BLACKLIST MANAGEMENT
    // =====================================================================

    /// <summary>
    /// Blacklist customer for specific reason
    /// </summary>
    public async Task<bool> BlacklistAccountAsync(int accountId, string reason)
    {
        try
        {
            var loans = await _context.Loans
                .Where(l => l.AccountId == accountId)
                .ToListAsync();

            if (!loans.Any())
                return false;

            foreach (var loan in loans)
            {
                loan.IsBlacklisted = true;
                loan.BlacklistedAt = DateTime.Now;
                loan.BlacklistedReason = reason;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to blacklist account", ex);
        }
    }

    /// <summary>
    /// Remove blacklist status
    /// </summary>
    public async Task<bool> RemoveBlacklistAsync(int accountId, string? reason = null)
    {
        try
        {
            var loans = await _context.Loans
                .Where(l => l.AccountId == accountId && l.IsBlacklisted)
                .ToListAsync();

            if (!loans.Any())
                return false;

            foreach (var loan in loans)
            {
                loan.IsBlacklisted = false;
                loan.BlacklistedAt = null;
                loan.BlacklistedReason = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to remove blacklist", ex);
        }
    }

    /// <summary>
    /// Get list of blacklisted customers
    /// </summary>
    public async Task<List<BlacklistedCustomer>> GetBlacklistedCustomersAsync()
    {
        try
        {
            var blacklisted = await _context.Loans
                .Where(l => l.IsBlacklisted)
                .GroupBy(l => l.AccountId)
                .Select(g => new BlacklistedCustomer
                {
                    AccountId = g.Key,
                    LoanCount = g.Count(),
                    BlacklistedSince = g.Min(l => l.BlacklistedAt),
                    Reason = g.First().BlacklistedReason,
                    LastBlacklistedLoan = g.OrderByDescending(l => l.BlacklistedAt).Select(l => l.LoanNumber).First()
                })
                .ToListAsync();

            return blacklisted;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get blacklisted customers", ex);
        }
    }

    // =====================================================================
    // VIOLATION REPORTING
    // =====================================================================

    /// <summary>
    /// Get violation count for account (in last N months)
    /// </summary>
    public async Task<int> GetViolationCountAsync(int accountId, int monthsBack = 6)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddMonths(-monthsBack);

            return await _context.LoanViolations
                .Join(_context.Loans, v => v.LoanId, l => l.LoanId, (v, l) => new { v, l })
                .Where(x => x.l.AccountId == accountId && x.v.ViolationDate > cutoffDate)
                .CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get violation count", ex);
        }
    }

    /// <summary>
    /// Get detailed violation history
    /// </summary>
    public async Task<List<ViolationDetail>> GetViolationHistoryAsync(int accountId)
    {
        try
        {
            var violations = await _context.LoanViolations
                .Join(_context.Loans, v => v.LoanId, l => l.LoanId, (v, l) => new { v, l })
                .Where(x => x.l.AccountId == accountId)
                .Select(x => new ViolationDetail
                {
                    ViolationId = x.v.ViolationId,
                    LoanNumber = x.l.LoanNumber,
                    ViolationType = x.v.ViolationType,
                    DaysOverdue = x.v.DaysOverdue,
                    PenaltyAmount = x.v.PenaltyAmount,
                    ViolationDate = x.v.ViolationDate,
                    IsResolved = x.v.IsResolved,
                    ResolvedAt = x.v.ResolvedAt
                })
                .OrderByDescending(v => v.ViolationDate)
                .ToListAsync();

            return violations;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get violation history", ex);
        }
    }

    // =====================================================================
    // LOAN HISTORY & STATISTICS
    // =====================================================================

    /// <summary>
    /// Get total outstanding balance for account
    /// </summary>
    public async Task<decimal> GetTotalOutstandingBalanceAsync(int accountId)
    {
        try
        {
            return await _context.Loans
                .Where(l => l.AccountId == accountId && l.Status != "Completed")
                .SumAsync(l => l.OutstandingBalance);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get outstanding balance", ex);
        }
    }

    /// <summary>
    /// Get number of completed loans
    /// </summary>
    public async Task<int> GetCompletedLoansAsync(int accountId)
    {
        try
        {
            return await _context.Loans
                .Where(l => l.AccountId == accountId && l.Status == "Completed")
                .CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get completed loans count", ex);
        }
    }

    /// <summary>
    /// Get total amount borrowed
    /// </summary>
    public async Task<decimal> GetTotalBorrowedAsync(int accountId)
    {
        try
        {
            return await _context.Loans
                .Where(l => l.AccountId == accountId)
                .SumAsync(l => l.LoanAmount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get total borrowed", ex);
        }
    }

    /// <summary>
    /// Calculate on-time payment rate (percentage of payments made on time)
    /// </summary>
    public async Task<decimal> CalculateOnTimePaymentRateAsync(int accountId)
    {
        try
        {
            var loans = await _context.Loans
                .Where(l => l.AccountId == accountId)
                .Select(l => l.LoanId)
                .ToListAsync();

            if (!loans.Any())
                return 0;

            var totalPayments = await _context.LoanPayments
                .Where(p => loans.Contains(p.LoanId))
                .CountAsync();

            if (totalPayments == 0)
                return 0;

            var onTimePayments = await _context.LoanPayments
                .Where(p => loans.Contains(p.LoanId) && !p.IsLatePayment)
                .CountAsync();

            return totalPayments > 0 ? (decimal)onTimePayments / totalPayments * 100 : 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to calculate on-time payment rate", ex);
        }
    }

    // =====================================================================
    // LOAN ELIGIBILITY LIMITS
    // =====================================================================

    /// <summary>
    /// Get maximum loan amount customer can borrow
    /// </summary>
    public async Task<decimal> GetMaximumLoanAmountAsync(int accountId)
    {
        try
        {
            var account = await _context.CustomerAccounts.FindAsync(accountId);
            if (account == null)
                return 0;

            // Check eligibility first
            var (isEligible, _) = await CheckReLoanEligibilityAsync(accountId);
            if (!isEligible)
                return 0;

            // Max loan = Account Balance * 10 (or use configuration)
            var maxLoan = account.Balance * 10;

            return maxLoan;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get maximum loan amount", ex);
        }
    }

    /// <summary>
    /// Validate loan application eligibility
    /// </summary>
    public async Task<(bool IsValid, string Message)> ValidateLoanApplicationAsync(
        int accountId,
        decimal requestedAmount)
    {
        try
        {
            // Check re-loan eligibility
            var (isEligible, reason) = await CheckReLoanEligibilityAsync(accountId);
            if (!isEligible)
                return (false, reason);

            // Check maximum loan amount
            var maxAmount = await GetMaximumLoanAmountAsync(accountId);
            if (requestedAmount > maxAmount)
                return (false, $"Requested amount ({requestedAmount:C}) exceeds maximum loan amount ({maxAmount:C})");

            // Check minimum loan amount (e.g., at least 5000)
            if (requestedAmount < 5000)
                return (false, "Minimum loan amount is 5000");

            return (true, "Loan application is valid");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to validate loan application", ex);
        }
    }
}

// =====================================================================
// DTOs FOR REPORTING
// =====================================================================

/// <summary>
/// Loan eligibility report
/// </summary>
public class EligibilityReport
{
    public int AccountId { get; set; }
    public bool IsEligible { get; set; }
    public bool HasOutstandingBalance { get; set; }
    public string? OutstandingBalanceReason { get; set; }
    public bool HasRecentViolations { get; set; }
    public string? RecentViolationsReason { get; set; }
    public int ViolationCount { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int CompletedLoans { get; set; }
    public decimal TotalAmountBorrowed { get; set; }
    public decimal OnTimePaymentRate { get; set; }
}

/// <summary>
/// Blacklisted customer info
/// </summary>
public class BlacklistedCustomer
{
    public int AccountId { get; set; }
    public int LoanCount { get; set; }
    public DateTime? BlacklistedSince { get; set; }
    public string? Reason { get; set; }
    public string? LastBlacklistedLoan { get; set; }
}

/// <summary>
/// Violation detail for history
/// </summary>
public class ViolationDetail
{
    public int ViolationId { get; set; }
    public string? LoanNumber { get; set; }
    public string? ViolationType { get; set; }
    public int DaysOverdue { get; set; }
    public decimal PenaltyAmount { get; set; }
    public DateTime ViolationDate { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

