using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services;

/// <summary>
/// Service for ERP double-entry bookkeeping system
/// Implements proper accounting principles for all financial transactions
/// </summary>
public class AccountingEntryService
{
    private readonly BFASDbContext _context;

    public AccountingEntryService(BFASDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create accounting entries for a deposit transaction
    /// Debit: Cash Account | Credit: Customer Liability Account
    /// </summary>
    public async Task<List<AccountingEntry>> CreateDepositEntries(
        int accountId,
        decimal amount,
        string reference,
        string processedBy)
    {
        var entries = new List<AccountingEntry>();

        // Debit: Cash (Asset increases)
        entries.Add(new AccountingEntry
        {
            AccountId = accountId,
            TransactionType = "DEPOSIT",
            AccountType = "CASH",
            DebitAmount = amount,
            CreditAmount = 0,
            Balance = amount,
            Reference = reference,
            Description = $"Cash deposit - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        // Credit: Customer Liability (Liability increases)
        entries.Add(new AccountingEntry
        {
            AccountId = accountId,
            TransactionType = "DEPOSIT",
            AccountType = "CUSTOMER_LIABILITY",
            DebitAmount = 0,
            CreditAmount = amount,
            Balance = amount,
            Reference = reference,
            Description = $"Customer deposit liability - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        await _context.AccountingEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();

        return entries;
    }

    /// <summary>
    /// Create accounting entries for a withdrawal transaction
    /// Debit: Customer Liability Account | Credit: Cash Account
    /// </summary>
    public async Task<List<AccountingEntry>> CreateWithdrawalEntries(
        int accountId,
        decimal amount,
        decimal tax,
        string reference,
        string processedBy)
    {
        var entries = new List<AccountingEntry>();

        // Debit: Customer Liability (Liability decreases)
        entries.Add(new AccountingEntry
        {
            AccountId = accountId,
            TransactionType = "WITHDRAWAL",
            AccountType = "CUSTOMER_LIABILITY",
            DebitAmount = amount,
            CreditAmount = 0,
            Balance = -amount,
            Reference = reference,
            Description = $"Customer withdrawal - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        // Credit: Cash (Asset decreases)
        entries.Add(new AccountingEntry
        {
            AccountId = accountId,
            TransactionType = "WITHDRAWAL",
            AccountType = "CASH",
            DebitAmount = 0,
            CreditAmount = amount - tax,
            Balance = -(amount - tax),
            Reference = reference,
            Description = $"Cash withdrawal - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        // If there's tax, record it separately
        if (tax > 0)
        {
            entries.Add(new AccountingEntry
            {
                AccountId = accountId,
                TransactionType = "WITHDRAWAL",
                AccountType = "TAX_PAYABLE",
                DebitAmount = 0,
                CreditAmount = tax,
                Balance = tax,
                Reference = reference,
                Description = $"Documentary stamp tax - {reference}",
                ProcessedBy = processedBy,
                CreatedAt = DateTime.Now
            });
        }

        await _context.AccountingEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();

        return entries;
    }

    /// <summary>
    /// Create accounting entries for a transfer transaction
    /// Debit: Sender Account | Credit: Receiver Account
    /// </summary>
    public async Task<List<AccountingEntry>> CreateTransferEntries(
        int senderAccountId,
        int receiverAccountId,
        decimal amount,
        decimal tax,
        string reference,
        string processedBy)
    {
        var entries = new List<AccountingEntry>();

        // Debit: Sender's liability (decreases)
        entries.Add(new AccountingEntry
        {
            AccountId = senderAccountId,
            TransactionType = "TRANSFER_OUT",
            AccountType = "CUSTOMER_LIABILITY",
            DebitAmount = amount,
            CreditAmount = 0,
            Balance = -amount,
            Reference = reference,
            Description = $"Transfer to account - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        // Credit: Receiver's liability (increases)
        entries.Add(new AccountingEntry
        {
            AccountId = receiverAccountId,
            TransactionType = "TRANSFER_IN",
            AccountType = "CUSTOMER_LIABILITY",
            DebitAmount = 0,
            CreditAmount = amount - tax,
            Balance = amount - tax,
            Reference = reference,
            Description = $"Transfer from account - {reference}",
            ProcessedBy = processedBy,
            CreatedAt = DateTime.Now
        });

        // If there's tax, record it
        if (tax > 0)
        {
            entries.Add(new AccountingEntry
            {
                AccountId = senderAccountId,
                TransactionType = "TRANSFER_OUT",
                AccountType = "TAX_PAYABLE",
                DebitAmount = 0,
                CreditAmount = tax,
                Balance = tax,
                Reference = reference,
                Description = $"Transfer tax - {reference}",
                ProcessedBy = processedBy,
                CreatedAt = DateTime.Now
            });
        }

        await _context.AccountingEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();

        return entries;
    }

    /// <summary>
    /// Verify that all entries balance (Total Debits = Total Credits)
    /// </summary>
    public async Task<bool> VerifyEntryBalance(string reference)
    {
        var entries = await _context.AccountingEntries
            .Where(e => e.Reference == reference)
            .ToListAsync();

        var totalDebits = entries.Sum(e => e.DebitAmount);
        var totalCredits = entries.Sum(e => e.CreditAmount);

        return Math.Abs(totalDebits - totalCredits) < 0.01m; // Allow 1 centavo variance
    }

    /// <summary>
    /// Get accounting entries for an account
    /// </summary>
    public async Task<List<AccountingEntry>> GetAccountEntries(int accountId, DateTime? fromDate = null)
    {
        var query = _context.AccountingEntries.Where(e => e.AccountId == accountId);

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Calculate account balance from accounting entries
    /// </summary>
    public async Task<decimal> CalculateAccountBalance(int accountId)
    {
        var entries = await _context.AccountingEntries
            .Where(e => e.AccountId == accountId && e.AccountType == "CUSTOMER_LIABILITY")
            .ToListAsync();

        return entries.Sum(e => e.CreditAmount - e.DebitAmount);
    }
}
