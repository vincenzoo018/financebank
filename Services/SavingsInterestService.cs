using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services;

/// <summary>
/// Service for handling savings account interest calculation and posting
/// Implements daily interest computation with monthly/quarterly posting
/// Integrates with TransactionHistoryService for AR/AP tracking.
/// </summary>
public class SavingsInterestService
{
    private readonly IDbContextFactory<BFASDbContext> _contextFactory;
    private readonly TransactionHistoryService? _transactionHistoryService;

    public SavingsInterestService(IDbContextFactory<BFASDbContext> contextFactory, TransactionHistoryService? transactionHistoryService = null)
    {
        _contextFactory = contextFactory;
        _transactionHistoryService = transactionHistoryService;
    }

    // =============================================
    // DAILY INTEREST COMPUTATION
    // =============================================

    /// <summary>
    /// Compute daily interest for all active savings accounts
    /// Should be run daily by a background job
    /// </summary>
    public async Task<(int accountsProcessed, decimal totalInterest, string message)> ComputeDailyInterestAsync(DateTime? computationDate = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var targetDate = (computationDate ?? DateTime.Today).Date;

            // Get all active savings accounts
            var accounts = await context.SavingsAccounts
                .Where(sa => sa.Status == "ACTIVE" && sa.Balance > 0)
                .ToListAsync();

            int processed = 0;
            decimal totalInterestComputed = 0;

            foreach (var account in accounts)
            {
                // Check if already computed for this date
                var existingComputation = await context.SavingsInterestRecords
                    .AnyAsync(si => si.SavingsAccountId == account.SavingsAccountId &&
                                   si.ComputationDate == targetDate);

                if (existingComputation)
                    continue; // Skip if already computed

                // Calculate daily interest: (Balance × Annual Rate) / 365
                var annualRate = account.CurrentInterestRate / 100; // Convert percentage to decimal
                var dailyInterest = (account.Balance * (decimal)annualRate) / 365;

                // Create interest record
                var interestRecord = new SavingsInterest
                {
                    SavingsAccountId = account.SavingsAccountId,
                    ComputationDate = targetDate,
                    DailyBalance = account.Balance,
                    AppliedInterestRate = account.CurrentInterestRate,
                    DailyInterestAmount = dailyInterest,
                    IsPosted = false,
                    CreatedAt = DateTime.Now
                };

                context.SavingsInterestRecords.Add(interestRecord);
                processed++;
                totalInterestComputed += dailyInterest;
            }

            await context.SaveChangesAsync();

            return (processed, totalInterestComputed, $"Computed daily interest for {processed} accounts. Total: ₱{totalInterestComputed:N4}");
        }
        catch (Exception ex)
        {
            return (0, 0, $"Error computing daily interest: {ex.Message}");
        }
    }

    // =============================================
    // MONTHLY INTEREST POSTING
    // =============================================

    /// <summary>
    /// Create monthly interest posting batch (System generates, FM approves)
    /// </summary>
    public async Task<(bool success, string message, SavingsInterestPosting? posting)> CreateMonthlyPostingBatchAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string processedBy)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            // Get all unposted interest records for the period
            var unpostedInterest = await context.SavingsInterestRecords
                .Where(si => !si.IsPosted &&
                            si.ComputationDate >= periodStart.Date &&
                            si.ComputationDate <= periodEnd.Date)
                .GroupBy(si => si.SavingsAccountId)
                .Select(g => new
                {
                    SavingsAccountId = g.Key,
                    TotalInterest = g.Sum(si => si.DailyInterestAmount),
                    RecordCount = g.Count()
                })
                .ToListAsync();

            if (!unpostedInterest.Any())
                return (false, "No unposted interest found for the specified period", null);

            var totalInterest = unpostedInterest.Sum(ui => ui.TotalInterest);
            var accountsCount = unpostedInterest.Count;

            // Create posting batch
            var posting = new SavingsInterestPosting
            {
                PostingDate = DateTime.Now,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                TotalInterestPosted = totalInterest,
                AccountsProcessed = accountsCount,
                PostingStatus = "Pending", // Requires FM approval
                ProcessedBy = processedBy,
                CreatedAt = DateTime.Now
            };

            context.SavingsInterestPostings.Add(posting);
            await context.SaveChangesAsync();

            return (true, $"Created interest posting batch for {accountsCount} accounts. Total interest: ₱{totalInterest:N2}", posting);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating posting batch: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Finance Manager approves interest posting batch
    /// </summary>
    public async Task<(bool success, string message)> ApprovePostingBatchAsync(int postingId, string approvedBy)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var posting = await context.SavingsInterestPostings.FindAsync(postingId);
            if (posting == null)
                return (false, "Posting batch not found");

            if (posting.PostingStatus != "Pending")
                return (false, "Batch already processed");

            posting.PostingStatus = "FMApproved";
            posting.ApprovedByFM = approvedBy;
            posting.ApprovedByFMAt = DateTime.Now;

            await context.SaveChangesAsync();

            return (true, "Interest posting batch approved. Awaiting Accountant release.");
        }
        catch (Exception ex)
        {
            return (false, $"Error approving batch: {ex.Message}");
        }
    }

    /// <summary>
    /// Accountant releases interest to customer accounts (creates A/P and journal entries)
    /// </summary>
    public async Task<(bool success, string message)> ReleaseInterestToAccountsAsync(int postingId, string releasedBy)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var posting = await context.SavingsInterestPostings.FindAsync(postingId);
            if (posting == null)
                return (false, "Posting batch not found");

            if (posting.PostingStatus != "FMApproved")
                return (false, "Batch not approved by Finance Manager");

            // Get all unposted interest for this batch
            var interestRecords = await context.SavingsInterestRecords
                .Where(si => !si.IsPosted &&
                            si.ComputationDate >= posting.PeriodStart.Date &&
                            si.ComputationDate <= posting.PeriodEnd.Date)
                .GroupBy(si => si.SavingsAccountId)
                .ToListAsync();

            int accountsUpdated = 0;

            foreach (var group in interestRecords)
            {
                var savingsAccountId = group.Key;
                var totalInterest = group.Sum(si => si.DailyInterestAmount);

                // Get savings account
                var savingsAccount = await context.SavingsAccounts.FindAsync(savingsAccountId);
                if (savingsAccount == null)
                    continue;

                // Post interest to savings account
                var balanceBefore = savingsAccount.Balance;
                savingsAccount.Balance += totalInterest;
                savingsAccount.TotalInterestEarned += totalInterest;
                savingsAccount.LastInterestPostDate = DateTime.Now;
                savingsAccount.UpdatedAt = DateTime.Now;

                // Create interest posting transaction
                var transaction = new SavingsTransaction
                {
                    TransactionNumber = SavingsTransaction.GenerateTransactionNumber(),
                    SavingsAccountId = savingsAccountId,
                    TransactionType = "InterestPosting",
                    Amount = totalInterest,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = savingsAccount.Balance,
                    Source = "System",
                    Status = "Completed",
                    Description = $"Interest posting for period {posting.PeriodStart:yyyy-MM-dd} to {posting.PeriodEnd:yyyy-MM-dd}",
                    ProcessedBy = releasedBy,
                    TransactionDate = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                context.SavingsTransactions.Add(transaction);

                // Mark interest records as posted
                foreach (var record in group)
                {
                    record.IsPosted = true;
                    record.PostingBatchId = postingId;
                    record.PostedDate = DateTime.Now;
                }

                accountsUpdated++;

                // Create AP entry via TransactionHistoryService (interest = money OUT = AP)
                try
                {
                    if (_transactionHistoryService != null)
                    {
                        var customer = await context.Users.FindAsync(savingsAccount.CustomerId);
                        var customerName = customer?.FullName ?? "Customer";

                        await _transactionHistoryService.RecordSavingsInterestAsync(
                            savingsAccountId: savingsAccountId,
                            customerAccountId: savingsAccount.CustomerAccountId,
                            accountNumber: savingsAccount.AccountNumber ?? "",
                            customerName: customerName,
                            interestAmount: totalInterest,
                            interestRecordId: group.First().InterestId,
                            processedBy: releasedBy,
                            notes: $"Interest posting for period {posting.PeriodStart:yyyy-MM-dd} to {posting.PeriodEnd:yyyy-MM-dd}"
                        );
                    }
                }
                catch
                {
                    // AP tracking failure should not prevent interest posting from being recorded
                }
            }

            // Create bulk Accounts Payable entry for audit purposes (Interest expense liability)
            var apEntry = new AccountsPayable
            {
                VendorName = "Customer Interest Payable",
                InvoiceNumber = $"INT-POST-{postingId:D6}",
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now,
                Amount = posting.TotalInterestPosted,
                PaidAmount = posting.TotalInterestPosted, // Already credited to accounts
                OutstandingAmount = 0,
                Status = "Paid",
                Description = $"Savings Interest Payment - Period {posting.PeriodStart:yyyy-MM-dd} to {posting.PeriodEnd:yyyy-MM-dd} | {accountsUpdated} accounts",
                CreatedAt = DateTime.Now,
                CreatedBy = releasedBy,
                TransactionType = "SavingsInterest",
                SourceTable = "SavingsInterestPostings",
                SourceTransactionId = postingId,
                ReferenceNumber = $"INT-POST-{postingId:D6}",
                ReviewStatus = "Completed"
            };

            context.AccountsPayables.Add(apEntry);

            // Update posting batch status
            posting.PostingStatus = "Completed";
            posting.ReleasedByAccountant = releasedBy;
            posting.ReleasedByAccountantAt = DateTime.Now;

            await context.SaveChangesAsync();

            return (true, $"Successfully released interest to {accountsUpdated} accounts. Total: ₱{posting.TotalInterestPosted:N2}");
        }
        catch (Exception ex)
        {
            return (false, $"Error releasing interest: {ex.Message}");
        }
    }

    // =============================================
    // QUERY METHODS
    // =============================================

    /// <summary>
    /// Get all interest posting batches
    /// </summary>
    public async Task<List<SavingsInterestPosting>> GetInterestPostingBatchesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsInterestPostings
            .OrderByDescending(p => p.PostingDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending FM approval batches
    /// </summary>
    public async Task<List<SavingsInterestPosting>> GetPendingFMApprovalBatchesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsInterestPostings
            .Where(p => p.PostingStatus == "Pending")
            .OrderByDescending(p => p.PostingDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get FM-approved batches awaiting accountant release
    /// </summary>
    public async Task<List<SavingsInterestPosting>> GetFMApprovedBatchesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsInterestPostings
            .Where(p => p.PostingStatus == "FMApproved")
            .OrderByDescending(p => p.PostingDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get interest computation history for an account
    /// </summary>
    public async Task<List<SavingsInterest>> GetInterestHistoryAsync(int savingsAccountId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsInterestRecords
            .Where(si => si.SavingsAccountId == savingsAccountId)
            .OrderByDescending(si => si.ComputationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get unposted interest for an account
    /// </summary>
    public async Task<decimal> GetUnpostedInterestAsync(int savingsAccountId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsInterestRecords
            .Where(si => si.SavingsAccountId == savingsAccountId && !si.IsPosted)
            .SumAsync(si => si.DailyInterestAmount);
    }

    // =============================================
    // SPECIAL OPERATIONS
    // =============================================

    /// <summary>
    /// Forfeit interest for early withdrawal
    /// </summary>
    public async Task<(bool success, string message)> ForfeitInterestAsync(int savingsAccountId, decimal amountForfeited, string reason)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var savingsAccount = await context.SavingsAccounts.FindAsync(savingsAccountId);
            if (savingsAccount == null)
                return (false, "Savings account not found");

            // Delete unposted interest records
            var unpostedRecords = await context.SavingsInterestRecords
                .Where(si => si.SavingsAccountId == savingsAccountId && !si.IsPosted)
                .ToListAsync();

            context.SavingsInterestRecords.RemoveRange(unpostedRecords);

            // Deduct from total interest earned
            savingsAccount.TotalInterestEarned -= amountForfeited;
            if (savingsAccount.TotalInterestEarned < 0)
                savingsAccount.TotalInterestEarned = 0;

            savingsAccount.UpdatedAt = DateTime.Now;

            // Create penalty transaction record
            var transaction = new SavingsTransaction
            {
                TransactionNumber = SavingsTransaction.GenerateTransactionNumber(),
                SavingsAccountId = savingsAccountId,
                TransactionType = "PenaltyCharge",
                Amount = amountForfeited,
                BalanceBefore = savingsAccount.Balance,
                BalanceAfter = savingsAccount.Balance,
                Source = "System",
                Status = "Completed",
                Description = $"Interest forfeited - {reason}",
                ProcessedBy = "System",
                TransactionDate = DateTime.Now,
                ProcessedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            context.SavingsTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return (true, $"Forfeited ₱{amountForfeited:N2} in interest");
        }
        catch (Exception ex)
        {
            return (false, $"Error forfeiting interest: {ex.Message}");
        }
    }

    /// <summary>
    /// Get interest summary statistics
    /// </summary>
    public async Task<(decimal TotalAccrued, decimal TotalPosted, decimal TotalUnposted)> GetInterestSummaryAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var allInterest = await context.SavingsInterestRecords.ToListAsync();

        var totalAccrued = allInterest.Sum(si => si.DailyInterestAmount);
        var totalPosted = allInterest.Where(si => si.IsPosted).Sum(si => si.DailyInterestAmount);
        var totalUnposted = allInterest.Where(si => !si.IsPosted).Sum(si => si.DailyInterestAmount);

        return (totalAccrued, totalPosted, totalUnposted);
    }

    /// <summary>
    /// Compute interest projections for next period
    /// </summary>
    public async Task<decimal> ProjectNextMonthInterestAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activeAccounts = await context.SavingsAccounts
            .Where(sa => sa.Status == "ACTIVE" && sa.Balance > 0)
            .ToListAsync();

        decimal totalProjected = 0;

        foreach (var account in activeAccounts)
        {
            var annualRate = account.CurrentInterestRate / 100;
            var dailyInterest = (account.Balance * (decimal)annualRate) / 365;
            var monthlyProjection = dailyInterest * 30; // Approximate 30 days

            totalProjected += monthlyProjection;
        }

        return totalProjected;
    }

    /// <summary>
    /// Get total unposted interest amount
    /// </summary>
    public async Task<decimal> GetTotalUnpostedInterestAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var unpostedInterest = await context.SavingsInterestRecords
            .Where(si => !si.IsPosted)
            .SumAsync(si => si.DailyInterestAmount);

        return unpostedInterest;
    }
}
