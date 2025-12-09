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
    public const string ACCOUNT_INTEREST_EXPENSE = "5020";
    public const string ACCOUNT_UTILITIES_EXPENSE = "5030";
    public const string ACCOUNT_RENT_EXPENSE = "5040";
    public const string ACCOUNT_SUPPLIES_EXPENSE = "5050";
    public const string ACCOUNT_SALARIES_EXPENSE = "5060";
    public const string ACCOUNT_MAINTENANCE_EXPENSE = "5070";
    public const string ACCOUNT_INSURANCE_EXPENSE = "5080";
    public const string ACCOUNT_PROFESSIONAL_FEES = "5090";
    public const string ACCOUNT_TAX_EXPENSE = "5100";

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

        // Create Journal Entry Lines
        var journalLines = new List<JournalEntryLine>
        {
            // Debit Cash on Hand
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Deposit received - {customerName}"
            },
            // Credit Customer Deposits
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Deposit liability - {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);

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

        // Create Journal Entry Lines
        var journalLines = new List<JournalEntryLine>
        {
            // Debit Customer Deposits (decrease liability)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Withdrawal - {customerName}"
            },
            // Credit Cash on Hand (decrease asset)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Cash disbursed - {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);

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

        // Create Journal Entry Lines for the transfer fee
        var journalLines = new List<JournalEntryLine>
        {
            // Debit Customer Deposits (fee deducted from customer)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = fee,
                CreditAmount = 0,
                Description = $"Transfer fee - {senderName}"
            },
            // Credit Service Fee Income
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_SERVICE_FEE_INCOME,
                DebitAmount = 0,
                CreditAmount = fee,
                Description = $"Transfer fee income"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);

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

        // Create Journal Entry Lines
        var journalLines = new List<JournalEntryLine>
        {
            // Debit Customer Deposits (money deducted from customer account)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Bill payment - {customerName} to {billerName}"
            },
            // Credit Cash on Hand (cash paid to biller)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Cash paid to {billerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);

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

        // Create Journal Entry Lines
        var journalLines = new List<JournalEntryLine>
        {
            // Debit Loans Receivable
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOANS_RECEIVABLE,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Loan released - {loanNumber} - {customerName}"
            },
            // Credit Cash on Hand
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Cash disbursed for loan - {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, disbursalDate, loanNumber);

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

        var journalLines = new List<JournalEntryLine>();

        // Debit Cash on Hand (total payment received)
        journalLines.Add(new JournalEntryLine
        {
            JournalId = journal.JournalId,
            AccountCode = ACCOUNT_CASH_ON_HAND,
            DebitAmount = totalPayment,
            CreditAmount = 0,
            Description = $"Loan payment received - {loanNumber}"
        });

        // Credit Loans Receivable (principal portion)
        if (principalAmount > 0)
        {
            journalLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOANS_RECEIVABLE,
                DebitAmount = 0,
                CreditAmount = principalAmount,
                Description = $"Principal collected - {loanNumber}"
            });
        }

        // Credit Interest Income
        if (interestAmount > 0)
        {
            journalLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_INTEREST_INCOME,
                DebitAmount = 0,
                CreditAmount = interestAmount,
                Description = $"Interest income - {loanNumber}"
            });
        }

        // Credit Penalty Income
        if (penaltyAmount > 0)
        {
            journalLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_PENALTY_INCOME,
                DebitAmount = 0,
                CreditAmount = penaltyAmount,
                Description = $"Penalty income - {loanNumber}"
            });
        }

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, paymentDate, $"LP-{paymentId}");

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

        var journalLines = new List<JournalEntryLine>
        {
            // Debit (either deducted from loan proceeds or collected as cash)
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = deductedFromLoan ? ACCOUNT_LOANS_RECEIVABLE : ACCOUNT_CASH_ON_HAND,
                DebitAmount = feeAmount,
                CreditAmount = 0,
                Description = $"Processing fee - {loanNumber}"
            },
            // Credit Loan Processing Fee Income
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_LOAN_PROCESSING_FEE,
                DebitAmount = 0,
                CreditAmount = feeAmount,
                Description = $"Processing fee income - {loanNumber}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        // Create corresponding GL Transactions
        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, feeDate, loanNumber);

        await context.SaveChangesAsync();
    }

    #endregion

    #region Savings Account Transactions

    /// <summary>
    /// Post GL entries for savings deposit
    /// Debit: Cash on Hand (Asset increases)
    /// Credit: Customer Deposits (Liability increases)
    /// </summary>
    public async Task PostSavingsDepositAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        string customerName,
        string savingsAccountNumber,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "SD");

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Savings deposit - {customerName} ({savingsAccountNumber})",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Savings deposit received - {customerName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Savings deposit liability - {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Post GL entries for savings withdrawal
    /// Debit: Customer Deposits (Liability decreases)
    /// Credit: Cash on Hand (Asset decreases)
    /// </summary>
    public async Task PostSavingsWithdrawalAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        decimal penaltyAmount,
        string customerName,
        string savingsAccountNumber,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "SW");
        var netAmount = amount - penaltyAmount;

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Savings withdrawal - {customerName} ({savingsAccountNumber})",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Savings withdrawal - {customerName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_CASH_ON_HAND,
                DebitAmount = 0,
                CreditAmount = netAmount,
                Description = $"Cash disbursed - {customerName}"
            }
        };

        // If there's a penalty, record it as income
        if (penaltyAmount > 0)
        {
            journalLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_PENALTY_INCOME,
                DebitAmount = 0,
                CreditAmount = penaltyAmount,
                Description = $"Early withdrawal penalty - {customerName}"
            });
        }

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Post GL entries for savings interest payout
    /// Debit: Interest Expense (Expense increases)
    /// Credit: Cash on Hand or Customer Deposits (cash paid out or credited to account)
    /// </summary>
    public async Task PostSavingsInterestPayoutAsync(
        int transactionId,
        string transactionNumber,
        decimal amount,
        string customerName,
        string savingsAccountNumber,
        DateTime transactionDate,
        bool paidToCash = false)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "SI");

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = transactionDate,
            Description = $"Savings interest payout - {customerName} ({savingsAccountNumber})",
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

        // Interest expense account - use 5020 for interest expense paid to customers
        const string ACCOUNT_INTEREST_EXPENSE = "5020";

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_INTEREST_EXPENSE,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Interest expense - {customerName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = paidToCash ? ACCOUNT_CASH_ON_HAND : ACCOUNT_CUSTOMER_DEPOSITS,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = paidToCash ? $"Interest paid in cash - {customerName}" : $"Interest credited to savings - {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, transactionDate, transactionNumber);
        await context.SaveChangesAsync();
    }

    #endregion

    #region Accounts Payable Transactions

    /// <summary>
    /// Post GL entries for accounts payable payment
    /// Debit: Accounts Payable (Liability decreases)
    /// Credit: Cash on Hand (Asset decreases)
    /// </summary>
    public async Task PostAccountsPayablePaymentAsync(
        int payableId,
        string vendorName,
        string invoiceNumber,
        decimal amount,
        DateTime paymentDate,
        string paymentMethod = "Cash")
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "AP");

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = paymentDate,
            Description = $"AP Payment - {vendorName} (Inv: {invoiceNumber})",
            Reference = $"AP-{payableId}",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_ACCOUNTS_PAYABLE,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Payment to {vendorName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = paymentMethod == "Bank" ? ACCOUNT_CASH_IN_BANK : ACCOUNT_CASH_ON_HAND,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Cash paid to {vendorName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, paymentDate, $"AP-{payableId}");
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Post GL entries for creating accounts payable (expense recognition)
    /// Debit: Expense Account
    /// Credit: Accounts Payable (Liability increases)
    /// </summary>
    public async Task PostAccountsPayableCreationAsync(
        int payableId,
        string vendorName,
        string invoiceNumber,
        decimal amount,
        string expenseCategory,
        DateTime invoiceDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "APE");

        // Map expense category to account code
        var expenseAccountCode = GetExpenseAccountCode(expenseCategory);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = invoiceDate,
            Description = $"AP Created - {vendorName} (Inv: {invoiceNumber}) - {expenseCategory}",
            Reference = $"APE-{payableId}",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = expenseAccountCode,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"{expenseCategory} expense from {vendorName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_ACCOUNTS_PAYABLE,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Payable to {vendorName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, invoiceDate, $"APE-{payableId}");
        await context.SaveChangesAsync();
    }

    #endregion

    #region Accounts Receivable Transactions

    /// <summary>
    /// Post GL entries for accounts receivable receipt
    /// Debit: Cash on Hand (Asset increases)
    /// Credit: Accounts Receivable (Asset decreases)
    /// </summary>
    public async Task PostAccountsReceivableReceiptAsync(
        int receivableId,
        string customerName,
        string invoiceNumber,
        decimal amount,
        DateTime receiptDate,
        string paymentMethod = "Cash")
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "AR");

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = receiptDate,
            Description = $"AR Receipt - {customerName} (Inv: {invoiceNumber})",
            Reference = $"AR-{receivableId}",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = paymentMethod == "Bank" ? ACCOUNT_CASH_IN_BANK : ACCOUNT_CASH_ON_HAND,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Receipt from {customerName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_ACCOUNTS_RECEIVABLE,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"Receivable collected from {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, receiptDate, $"AR-{receivableId}");
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Post GL entries for creating accounts receivable (revenue recognition)
    /// Debit: Accounts Receivable (Asset increases)
    /// Credit: Revenue Account
    /// </summary>
    public async Task PostAccountsReceivableCreationAsync(
        int receivableId,
        string customerName,
        string invoiceNumber,
        decimal amount,
        string revenueCategory,
        DateTime invoiceDate)
    {
        if (amount <= 0) return;

        using var context = await _contextFactory.CreateDbContextAsync();

        var journalNumber = await GenerateJournalNumberAsync(context, "ARE");

        // Map revenue category to account code
        var revenueAccountCode = GetRevenueAccountCode(revenueCategory);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            TransactionDate = invoiceDate,
            Description = $"AR Created - {customerName} (Inv: {invoiceNumber}) - {revenueCategory}",
            Reference = $"ARE-{receivableId}",
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

        var journalLines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = ACCOUNT_ACCOUNTS_RECEIVABLE,
                DebitAmount = amount,
                CreditAmount = 0,
                Description = $"Receivable from {customerName}"
            },
            new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = revenueAccountCode,
                DebitAmount = 0,
                CreditAmount = amount,
                Description = $"{revenueCategory} income from {customerName}"
            }
        };

        context.JournalEntryLines.AddRange(journalLines);
        await context.SaveChangesAsync();

        await CreateGLTransactionsFromJournalLinesAsync(context, journalLines, invoiceDate, $"ARE-{receivableId}");
        await context.SaveChangesAsync();
    }

    #endregion

    #region Expense Account Mapping

    /// <summary>
    /// Map expense category to GL account code
    /// </summary>
    private string GetExpenseAccountCode(string category)
    {
        return category?.ToUpper() switch
        {
            "UTILITIES" => "5030",
            "RENT" => "5040",
            "SUPPLIES" => "5050",
            "SALARIES" => "5060",
            "MAINTENANCE" => "5070",
            "INSURANCE" => "5080",
            "PROFESSIONAL FEES" => "5090",
            "TAXES" => "5100",
            "INTEREST EXPENSE" => "5020",
            _ => "5010" // Default - Bill Payment Expense
        };
    }

    /// <summary>
    /// Map revenue category to GL account code
    /// </summary>
    private string GetRevenueAccountCode(string category)
    {
        return category?.ToUpper() switch
        {
            "INTEREST INCOME" => ACCOUNT_INTEREST_INCOME,
            "SERVICE FEE" => ACCOUNT_SERVICE_FEE_INCOME,
            "PENALTY" => ACCOUNT_PENALTY_INCOME,
            "LOAN PROCESSING FEE" => ACCOUNT_LOAN_PROCESSING_FEE,
            _ => ACCOUNT_SERVICE_FEE_INCOME // Default
        };
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
            // Assets
            (ACCOUNT_CASH_ON_HAND, "Cash on Hand", "Asset"),
            (ACCOUNT_CASH_IN_BANK, "Cash in Bank", "Asset"),
            (ACCOUNT_LOANS_RECEIVABLE, "Loans Receivable", "Asset"),
            (ACCOUNT_ACCOUNTS_RECEIVABLE, "Accounts Receivable", "Asset"),
            // Liabilities
            (ACCOUNT_CUSTOMER_DEPOSITS, "Customer Deposits", "Liability"),
            (ACCOUNT_ACCOUNTS_PAYABLE, "Accounts Payable", "Liability"),
            // Revenue
            (ACCOUNT_INTEREST_INCOME, "Interest Income", "Revenue"),
            (ACCOUNT_SERVICE_FEE_INCOME, "Service Fee Income", "Revenue"),
            (ACCOUNT_PENALTY_INCOME, "Penalty Income", "Revenue"),
            (ACCOUNT_LOAN_PROCESSING_FEE, "Loan Processing Fee Income", "Revenue"),
            // Expenses
            (ACCOUNT_BILL_PAYMENT_EXPENSE, "Bill Payment Expense", "Expense"),
            (ACCOUNT_INTEREST_EXPENSE, "Interest Expense (Savings)", "Expense"),
            (ACCOUNT_UTILITIES_EXPENSE, "Utilities Expense", "Expense"),
            (ACCOUNT_RENT_EXPENSE, "Rent Expense", "Expense"),
            (ACCOUNT_SUPPLIES_EXPENSE, "Office Supplies Expense", "Expense"),
            (ACCOUNT_SALARIES_EXPENSE, "Salaries and Wages Expense", "Expense"),
            (ACCOUNT_MAINTENANCE_EXPENSE, "Maintenance Expense", "Expense"),
            (ACCOUNT_INSURANCE_EXPENSE, "Insurance Expense", "Expense"),
            (ACCOUNT_PROFESSIONAL_FEES, "Professional Fees Expense", "Expense"),
            (ACCOUNT_TAX_EXPENSE, "Tax Expense", "Expense")
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

        // Also ensure GeneralLedger accounts exist for GL transactions
        foreach (var (code, name, type) in requiredAccounts)
        {
            var glExists = await context.GeneralLedger
                .AnyAsync(gl => gl.AccountCode == code);

            if (!glExists)
            {
                context.GeneralLedger.Add(new GeneralLedger
                {
                    AccountCode = code,
                    AccountName = name,
                    AccountType = type,
                    Balance = 0,
                    IsActive = true
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

    /// <summary>
    /// Post GL entry for any SavingsTransaction automatically
    /// Call this method after saving a SavingsTransaction to ensure GL is updated
    /// </summary>
    public async Task PostSavingsTransactionAsync(SavingsTransaction transaction, string customerName, string accountNumber)
    {
        if (transaction == null) return;

        var transactionDate = transaction.ProcessedAt ?? transaction.TransactionDate;

        switch (transaction.TransactionType?.ToUpper())
        {
            case "DEPOSIT":
                await PostSavingsDepositAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    customerName,
                    accountNumber,
                    transactionDate);
                break;

            case "WITHDRAWAL":
                await PostSavingsWithdrawalAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    0, // Penalty tracked separately
                    customerName,
                    accountNumber,
                    transactionDate);
                break;

            case "INTERESTPOSTING":
            case "INTEREST":
                await PostSavingsInterestPayoutAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    transaction.Amount,
                    customerName,
                    accountNumber,
                    transactionDate,
                    false); // Credited to account by default
                break;

            case "PENALTYCHARGE":
            case "PENALTY":
                // Penalty is revenue for the bank
                await PostSavingsWithdrawalAsync(
                    transaction.TransactionId,
                    transaction.TransactionNumber,
                    0, // Net withdrawal
                    transaction.Amount, // Penalty amount
                    customerName,
                    accountNumber,
                    transactionDate);
                break;
        }
    }

    /// <summary>
    /// Create GeneralLedgerTransactions from JournalEntryLines
    /// This ensures GL transactions are properly linked to journal lines
    /// </summary>
    private async Task CreateGLTransactionsFromJournalLinesAsync(
        BFASDbContext context,
        List<JournalEntryLine> journalLines,
        DateTime transactionDate,
        string reference)
    {
        foreach (var line in journalLines)
        {
            // Get account details
            var account = await context.GeneralLedger
                .FirstOrDefaultAsync(a => a.AccountCode == line.AccountCode);

            if (account == null) continue;

            // Create GL Transaction
            var glTransaction = new GeneralLedgerTransaction
            {
                JournalLineId = line.LineId,
                AccountCode = line.AccountCode,
                TransactionDate = transactionDate,
                Description = line.Description,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                RunningBalance = 0 // Will be calculated by triggers or stored procedures
            };

            context.GeneralLedgerTransactions.Add(glTransaction);

            // Update account balance in GeneralLedger table
            if (account.AccountType == "Asset" || account.AccountType == "Expense")
            {
                // Normal debit balance accounts
                account.Balance += line.DebitAmount - line.CreditAmount;
            }
            else
            {
                // Normal credit balance accounts (Liability, Equity, Revenue)
                account.Balance += line.CreditAmount - line.DebitAmount;
            }
        }

        await context.SaveChangesAsync();
    }

    #endregion
}
