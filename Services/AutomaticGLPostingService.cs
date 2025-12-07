using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services;

/// <summary>
/// Automatic General Ledger Posting Service
/// Automatically creates double-entry journal entries and posts to GeneralLedger
/// for all cash-related transactions in real-time.
/// 
/// Chart of Accounts Used:
/// - 1010: Cash on Hand (Asset, Debit normal)
/// - 1110: Loans Receivable (Asset, Debit normal)
/// - 2010: Customer Deposits (Liability, Credit normal)
/// - 4010: Interest Income (Revenue, Credit normal)
/// - 4020: Service Fee Income (Revenue, Credit normal)
/// - 4030: Penalty Income (Revenue, Credit normal)
/// - 5010: Bill Payment Expense (Expense, Debit normal)
/// </summary>
public class AutomaticGLPostingService
{
    private readonly IDbContextFactory<BFASDbContext> _contextFactory;

    public AutomaticGLPostingService(IDbContextFactory<BFASDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    #region Account Codes - Standard Chart of Accounts

    // Assets
    public const string ACCOUNT_CASH_ON_HAND = "1010";
    public const string ACCOUNT_CASH_IN_BANK = "1100";
    public const string ACCOUNT_LOANS_RECEIVABLE = "1110";
    public const string ACCOUNT_ACCOUNTS_RECEIVABLE = "1200";

    // Liabilities
    public const string ACCOUNT_CUSTOMER_DEPOSITS = "2010";
    public const string ACCOUNT_ACCOUNTS_PAYABLE = "2000";

    // Revenue
    public const string ACCOUNT_INTEREST_INCOME = "4010";
    public const string ACCOUNT_SERVICE_FEE_INCOME = "4020";
    public const string ACCOUNT_PENALTY_INCOME = "4030";
    public const string ACCOUNT_LOAN_PROCESSING_FEE = "4040";

    // Expenses
    public const string ACCOUNT_BILL_PAYMENT_EXPENSE = "5010";

    #endregion

    #region Deposit Transactions

    /// <summary>
    /// Post GL entries for a customer deposit
    /// Debit: Cash on Hand (Asset increases)
    /// Credit: Customer Deposits (Liability increases)
    /// </summary>
    public async Task PostDepositAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        string customerName,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "DEP");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Customer deposit - {customerName}",
            Reference = transactionNumber,
            TotalDebit = amount,
            TotalCredit = amount,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        // Create GL Entries
        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit Cash on Hand
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-DEP-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                AccountName = "Cash on Hand",
                AccountType = "Asset",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Deposit received - {customerName}",
                DebitAmount = amount,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Customer Deposits
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-DEP-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                AccountName = "Customer Deposits",
                AccountType = "Liability",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Deposit liability - {customerName}",
                DebitAmount = 0,
                CreditAmount = amount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_CASH_ON_HAND, amount, true);
        await UpdateAccountBalanceAsync(context, ACCOUNT_CUSTOMER_DEPOSITS, amount, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Withdrawal Transactions

    /// <summary>
    /// Post GL entries for a customer withdrawal
    /// Debit: Customer Deposits (Liability decreases)
    /// Credit: Cash on Hand (Asset decreases)
    /// </summary>
    public async Task PostWithdrawalAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        string customerName,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "WDR");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Customer withdrawal - {customerName}",
            Reference = transactionNumber,
            TotalDebit = amount,
            TotalCredit = amount,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        // Create GL Entries
        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit Customer Deposits (decrease liability)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-WDR-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                AccountName = "Customer Deposits",
                AccountType = "Liability",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Withdrawal - {customerName}",
                DebitAmount = amount,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Cash on Hand (decrease asset)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-WDR-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                AccountName = "Cash on Hand",
                AccountType = "Asset",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Cash disbursed - {customerName}",
                DebitAmount = 0,
                CreditAmount = amount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_CUSTOMER_DEPOSITS, amount, true); // Debit decreases liability
        await UpdateAccountBalanceAsync(context, ACCOUNT_CASH_ON_HAND, amount, false); // Credit decreases asset

        await context.SaveChangesAsync();
    }

    #endregion

    #region Transfer Transactions

    /// <summary>
    /// Post GL entries for a fund transfer
    /// Transfer itself is between customer deposit accounts (no cash movement except fees)
    /// Fee Entry:
    /// Debit: Customer Deposits (Fee deducted)
    /// Credit: Service Fee Income (Revenue)
    /// </summary>
    public async Task PostTransferAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        decimal fee,
        string senderName,
        string recipientName,
        DateTime transactionDate)
    {
        if (fee <= 0) return; // Only record if there's a fee

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "TRF");

        // Create Journal Entry for the fee
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Transfer fee - {senderName} to {recipientName} (Amount: ₱{amount:N2})",
            Reference = transactionNumber,
            TotalDebit = fee,
            TotalCredit = fee,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        // Create GL Entries for the transfer fee
        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit Customer Deposits (fee deducted from customer)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-TRF-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                AccountName = "Customer Deposits",
                AccountType = "Liability",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Transfer fee - {senderName}",
                DebitAmount = fee,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Service Fee Income
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-TRF-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_SERVICE_FEE_INCOME,
                AccountName = "Service Fee Income",
                AccountType = "Revenue",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Transfer fee income",
                DebitAmount = 0,
                CreditAmount = fee,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_CUSTOMER_DEPOSITS, fee, true);
        await UpdateAccountBalanceAsync(context, ACCOUNT_SERVICE_FEE_INCOME, fee, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Bill Payment Transactions

    /// <summary>
    /// Post GL entries for a bill payment
    /// Debit: Customer Deposits (Liability decreases - money paid out)
    /// Credit: Cash on Hand (Asset decreases - cash paid to biller)
    /// </summary>
    public async Task PostBillPaymentAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        string customerName,
        string billerName,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "BILL");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Bill payment - {customerName} to {billerName}",
            Reference = transactionNumber,
            TotalDebit = amount,
            TotalCredit = amount,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        // Create GL Entries
        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit Customer Deposits (money deducted from customer account)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-BILL-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                AccountName = "Customer Deposits",
                AccountType = "Liability",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Bill payment - {customerName} to {billerName}",
                DebitAmount = amount,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Cash on Hand (cash paid to biller)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-BILL-{transactionId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                AccountName = "Cash on Hand",
                AccountType = "Asset",
                TransactionDate = transactionDate,
                Reference = transactionNumber,
                Description = $"Cash paid to {billerName}",
                DebitAmount = 0,
                CreditAmount = amount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_CUSTOMER_DEPOSITS, amount, true);
        await UpdateAccountBalanceAsync(context, ACCOUNT_CASH_ON_HAND, amount, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Loan Disbursement Transactions

    /// <summary>
    /// Post GL entries for loan disbursement
    /// Debit: Loans Receivable (Asset increases - bank now has a receivable)
    /// Credit: Cash on Hand (Asset decreases - cash given to customer)
    /// </summary>
    public async Task PostLoanDisbursementAsync(
        int disbursalId,
        string loanNumber,
        decimal amount,
        string customerName,
        DateTime disbursalDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "LD");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = disbursalDate,
            Description = $"Loan disbursement - {loanNumber} - {customerName}",
            Reference = loanNumber,
            TotalDebit = amount,
            TotalCredit = amount,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        // Create GL Entries
        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit Loans Receivable
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LD-{disbursalId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOANS_RECEIVABLE,
                AccountName = "Loans Receivable",
                AccountType = "Asset",
                TransactionDate = disbursalDate,
                Reference = loanNumber,
                Description = $"Loan released - {loanNumber} - {customerName}",
                DebitAmount = amount,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Cash on Hand
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LD-{disbursalId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                AccountName = "Cash on Hand",
                AccountType = "Asset",
                TransactionDate = disbursalDate,
                Reference = loanNumber,
                Description = $"Cash disbursed for loan - {customerName}",
                DebitAmount = 0,
                CreditAmount = amount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_LOANS_RECEIVABLE, amount, true);
        await UpdateAccountBalanceAsync(context, ACCOUNT_CASH_ON_HAND, amount, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Loan Payment Transactions

    /// <summary>
    /// Post GL entries for loan payment received
    /// Debit: Cash on Hand (Asset increases - cash received)
    /// Credit: Loans Receivable (Asset decreases - principal portion)
    /// Credit: Interest Income (Revenue - interest portion)
    /// Credit: Penalty Income (Revenue - penalty portion if any)
    /// </summary>
    public async Task PostLoanPaymentAsync(
        int paymentId,
        string loanNumber,
        decimal totalPayment,
        decimal principalAmount,
        decimal interestAmount,
        decimal penaltyAmount,
        string customerName,
        DateTime paymentDate)
    {
        if (totalPayment <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "LP");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = paymentDate,
            Description = $"Loan payment - {loanNumber} - {customerName}",
            Reference = $"LP-{paymentId}",
            TotalDebit = totalPayment,
            TotalCredit = totalPayment,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        var glEntries = new List<GeneralLedgerEntry>();

        // Debit Cash on Hand (total payment received)
        glEntries.Add(new GeneralLedgerEntry
        {
            EntryNumber = $"GL-LP-{paymentId}",
            JournalId = journal.JournalId,
            AccountCode = ACCOUNT_CASH_ON_HAND,
            AccountName = "Cash on Hand",
            AccountType = "Asset",
            TransactionDate = paymentDate,
            Reference = $"LP-{paymentId}",
            Description = $"Loan payment received - {loanNumber}",
            DebitAmount = totalPayment,
            CreditAmount = 0,
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            Status = "Posted"
        });

        // Credit Loans Receivable (principal portion)
        if (principalAmount > 0)
        {
            glEntries.Add(new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LP-{paymentId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOANS_RECEIVABLE,
                AccountName = "Loans Receivable",
                AccountType = "Asset",
                TransactionDate = paymentDate,
                Reference = $"LP-{paymentId}",
                Description = $"Principal collected - {loanNumber}",
                DebitAmount = 0,
                CreditAmount = principalAmount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            });
        }

        // Credit Interest Income
        if (interestAmount > 0)
        {
            glEntries.Add(new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LP-{paymentId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_INTEREST_INCOME,
                AccountName = "Interest Income",
                AccountType = "Revenue",
                TransactionDate = paymentDate,
                Reference = $"LP-{paymentId}",
                Description = $"Interest income - {loanNumber}",
                DebitAmount = 0,
                CreditAmount = interestAmount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            });
        }

        // Credit Penalty Income
        if (penaltyAmount > 0)
        {
            glEntries.Add(new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LP-{paymentId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_PENALTY_INCOME,
                AccountName = "Penalty Income",
                AccountType = "Revenue",
                TransactionDate = paymentDate,
                Reference = $"LP-{paymentId}",
                Description = $"Penalty income - {loanNumber}",
                DebitAmount = 0,
                CreditAmount = penaltyAmount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            });
        }

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, ACCOUNT_CASH_ON_HAND, totalPayment, true);
        if (principalAmount > 0)
            await UpdateAccountBalanceAsync(context, ACCOUNT_LOANS_RECEIVABLE, principalAmount, false);
        if (interestAmount > 0)
            await UpdateAccountBalanceAsync(context, ACCOUNT_INTEREST_INCOME, interestAmount, false);
        if (penaltyAmount > 0)
            await UpdateAccountBalanceAsync(context, ACCOUNT_PENALTY_INCOME, penaltyAmount, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Loan Processing Fee

    /// <summary>
    /// Post GL entries for loan processing fee
    /// Debit: Cash on Hand or Loans Receivable (fee collected)
    /// Credit: Loan Processing Fee Income
    /// </summary>
    public async Task PostLoanProcessingFeeAsync(
        int loanId,
        string loanNumber,
        decimal feeAmount,
        string customerName,
        DateTime feeDate,
        bool deductedFromLoan = true)
    {
        if (feeAmount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "LPF");

        // Create Journal Entry
        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = feeDate,
            Description = $"Loan processing fee - {loanNumber} - {customerName}",
            Reference = loanNumber,
            TotalDebit = feeAmount,
            TotalCredit = feeAmount,
            Status = "Posted",
            CreatedAt = DateTime.Now,
            CreatedBy = "System",
            PostedAt = DateTime.Now,
            PostedBy = "System"
        };

        context.JournalEntries.Add(journal);
        await context.SaveChangesAsync();

        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit (either deducted from loan proceeds or collected as cash)
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LPF-{loanId}",
                JournalId = journal.JournalId,
                AccountCode = deductedFromLoan ? ACCOUNT_LOANS_RECEIVABLE : ACCOUNT_CASH_ON_HAND,
                AccountName = deductedFromLoan ? "Loans Receivable" : "Cash on Hand",
                AccountType = "Asset",
                TransactionDate = feeDate,
                Reference = loanNumber,
                Description = $"Processing fee - {loanNumber}",
                DebitAmount = feeAmount,
                CreditAmount = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            },
            // Credit Loan Processing Fee Income
            new GeneralLedgerEntry
            {
                EntryNumber = $"GL-LPF-{loanId}",
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOAN_PROCESSING_FEE,
                AccountName = "Loan Processing Fee Income",
                AccountType = "Revenue",
                TransactionDate = feeDate,
                Reference = loanNumber,
                Description = $"Processing fee income - {loanNumber}",
                DebitAmount = 0,
                CreditAmount = feeAmount,
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                Status = "Posted"
            }
        };

        context.GeneralLedgerEntries.AddRange(glEntries);

        // Update Chart of Accounts balances
        await UpdateAccountBalanceAsync(context, deductedFromLoan ? ACCOUNT_LOANS_RECEIVABLE : ACCOUNT_CASH_ON_HAND, feeAmount, true);
        await UpdateAccountBalanceAsync(context, ACCOUNT_LOAN_PROCESSING_FEE, feeAmount, false);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generate unique journal number
    /// </summary>
    private async Task<string> GenerateJournalNumberAsync(BFASDbContext context, string prefix)
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var lastJournal = await context.JournalEntries
            .Where(j => j.JournalNumber.StartsWith($"JE-{prefix}-{today}"))
            .OrderByDescending(j => j.JournalNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastJournal != null)
        {
            var parts = lastJournal.JournalNumber.Split('-');
            if (parts.Length > 3 && int.TryParse(parts[3], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"JE-{prefix}-{today}-{sequence:D4}";
    }

    /// <summary>
    /// Update Chart of Accounts balance - Note: Balances are now computed from JournalEntries, not stored on ChartOfAccounts
    /// This method is kept for compatibility but no longer modifies account balances
    /// </summary>
    private async Task UpdateAccountBalanceAsync(BFASDbContext context, string accountCode, decimal amount, bool isDebit)
    {
        // Balances are now computed dynamically from JournalEntries via GeneralLedgerService
        // The actual balance is calculated from sum of debits and credits in journal entries
        // This ensures accuracy and avoids data synchronization issues
        await Task.CompletedTask;
    }

    /// <summary>
    /// Ensure required Chart of Accounts exist
    /// </summary>
    public async Task EnsureChartOfAccountsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var requiredAccounts = new List<(string Code, string Name, string Type)>
        {
            (ACCOUNT_CASH_ON_HAND, "Cash on Hand", "Asset"),
            (ACCOUNT_CASH_IN_BANK, "Cash in Bank", "Asset"),
            (ACCOUNT_LOANS_RECEIVABLE, "Loans Receivable", "Asset"),
            (ACCOUNT_ACCOUNTS_RECEIVABLE, "Accounts Receivable", "Asset"),
            (ACCOUNT_CUSTOMER_DEPOSITS, "Customer Deposits", "Liability"),
            (ACCOUNT_ACCOUNTS_PAYABLE, "Accounts Payable", "Liability"),
            (ACCOUNT_INTEREST_INCOME, "Interest Income", "Revenue"),
            (ACCOUNT_SERVICE_FEE_INCOME, "Service Fee Income", "Revenue"),
            (ACCOUNT_PENALTY_INCOME, "Penalty Income", "Revenue"),
            (ACCOUNT_LOAN_PROCESSING_FEE, "Loan Processing Fee Income", "Revenue"),
            (ACCOUNT_BILL_PAYMENT_EXPENSE, "Bill Payment Expense", "Expense")
        };

        foreach (var (code, name, type) in requiredAccounts)
        {
            var exists = await context.ChartOfAccounts
                .AnyAsync(a => a.AccountCode == code);

            if (!exists)
            {
                context.ChartOfAccounts.Add(new ChartOfAccounts
                {
                    AccountCode = code,
                    AccountName = name,
                    AccountType = type,
                    Level = 1,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now
                });
            }
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Generic Transaction Posting

    /// <summary>
    /// Post GL entry for any CustomerTransaction automatically
    /// Call this method after saving a CustomerTransaction to ensure GL is updated
    /// </summary>
    public async Task PostCustomerTransactionAsync(CustomerTransaction transaction, string customerName)
    {
        if (transaction == null) return;

        var transactionDate = transaction.ProcessedAt ?? transaction.CreatedAt;

        switch (transaction.TransactionType?.ToUpper())
        {
            case "DEPOSIT":
                await PostDepositAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    customerName,
                    transactionDate);
                break;

            case "WITHDRAWAL":
                await PostWithdrawalAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    customerName,
                    transactionDate);
                break;

            case "TRANSFER":
                // Transfer fees are the GL relevant part
                if (transaction.Fee > 0)
                {
                    await PostTransferAsync(
                        transaction.TransactionId,
                        transaction.TransactionNumber,
                        transaction.Amount,
                        transaction.Fee,
                        customerName,
                        transaction.ToAccountName ?? "Unknown",
                        transactionDate);
                }
                break;

            case "BILLS":
            case "BILL_PAYMENT":
                await PostBillPaymentAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    customerName,
                    transaction.BillerName ?? "Unknown Biller",
                    transactionDate);
                break;
        }
    }

    #endregion
}
