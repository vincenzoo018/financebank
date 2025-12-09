using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;
using System.Collections.Concurrent;

namespace FinanceBank.Services
{
    /// <summary>
    /// Unified Transaction History Service
    /// Handles AR/AP classification, unified transaction logging, and journal entry creation
    /// 
    /// AR (Accounts Receivable) - Money coming INTO the institution:
    /// - Deposits, Loan Payments, Incoming Transfers, Savings Deposits
    /// 
    /// AP (Accounts Payable) - Money going OUT of the institution:
    /// - Withdrawals, Loan Releases, Outgoing Transfers, Savings Withdrawals, Interest Payouts
    /// </summary>
    public class TransactionHistoryService
    {
        private readonly IDbContextFactory<BFASDbContext> _contextFactory;
        private static readonly ConcurrentDictionary<string, int> _dailySequences = new();
        private static readonly object _sequenceLock = new();

        // Transaction Type Constants
        public static class TransactionTypes
        {
            public const string Deposit = "Deposit";
            public const string Withdrawal = "Withdrawal";
            public const string Transfer = "Transfer";
            public const string LoanPayment = "LoanPayment";
            public const string LoanRelease = "LoanRelease";
            public const string SavingsDeposit = "SavingsDeposit";
            public const string SavingsWithdrawal = "SavingsWithdrawal";
            public const string SavingsInterest = "SavingsInterest";
            public const string BillPayment = "BillPayment";
        }

        // AR/AP Classification Constants
        public static class Classifications
        {
            public const string AR = "AR"; // Accounts Receivable - Money IN
            public const string AP = "AP"; // Accounts Payable - Money OUT
        }

        // Reference Number Prefixes
        public static class ReferencePrefixes
        {
            public const string Deposit = "DEP";
            public const string Withdrawal = "WTH";
            public const string Transfer = "TRF";
            public const string LoanPayment = "LNP";
            public const string LoanRelease = "LNR";
            public const string SavingsDeposit = "SVD";
            public const string SavingsWithdrawal = "SVW";
            public const string SavingsInterest = "SVI";
            public const string BillPayment = "BIL";
        }

        public TransactionHistoryService(IDbContextFactory<BFASDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Generate standardized reference number: {TYPE}-{YYYYMMDD}-{6DIGIT}
        /// </summary>
        public string GenerateReferenceNumber(string prefix)
        {
            var dateKey = DateTime.Now.ToString("yyyyMMdd");
            var sequenceKey = $"{prefix}-{dateKey}";
            int sequence;

            lock (_sequenceLock)
            {
                if (!_dailySequences.TryGetValue(sequenceKey, out sequence))
                {
                    sequence = 0;
                }
                sequence++;
                _dailySequences[sequenceKey] = sequence;
            }

            return $"{prefix}-{dateKey}-{sequence:D6}";
        }

        /// <summary>
        /// Get the AR/AP classification based on transaction type
        /// </summary>
        public static string GetClassification(string transactionType)
        {
            return transactionType switch
            {
                // Money coming IN (AR)
                TransactionTypes.Deposit => Classifications.AR,
                TransactionTypes.LoanPayment => Classifications.AR,
                TransactionTypes.SavingsDeposit => Classifications.AR,

                // Money going OUT (AP)
                TransactionTypes.Withdrawal => Classifications.AP,
                TransactionTypes.LoanRelease => Classifications.AP,
                TransactionTypes.SavingsWithdrawal => Classifications.AP,
                TransactionTypes.SavingsInterest => Classifications.AP,

                // Special cases
                TransactionTypes.Transfer => Classifications.AP, // Outgoing side is AP
                TransactionTypes.BillPayment => Classifications.AR, // Customer pays institution (AR), then AP when forwarded

                _ => Classifications.AR
            };
        }

        /// <summary>
        /// Get the reference prefix based on transaction type
        /// </summary>
        public static string GetReferencePrefix(string transactionType)
        {
            return transactionType switch
            {
                TransactionTypes.Deposit => ReferencePrefixes.Deposit,
                TransactionTypes.Withdrawal => ReferencePrefixes.Withdrawal,
                TransactionTypes.Transfer => ReferencePrefixes.Transfer,
                TransactionTypes.LoanPayment => ReferencePrefixes.LoanPayment,
                TransactionTypes.LoanRelease => ReferencePrefixes.LoanRelease,
                TransactionTypes.SavingsDeposit => ReferencePrefixes.SavingsDeposit,
                TransactionTypes.SavingsWithdrawal => ReferencePrefixes.SavingsWithdrawal,
                TransactionTypes.SavingsInterest => ReferencePrefixes.SavingsInterest,
                TransactionTypes.BillPayment => ReferencePrefixes.BillPayment,
                _ => "TXN"
            };
        }

        /// <summary>
        /// Record a DEPOSIT transaction - Creates AR entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsReceivable ar, JournalEntry? journal)> RecordDepositAsync(
            int customerAccountId,
            string accountNumber,
            string customerName,
            decimal amount,
            string depositMethod,
            int sourceTransactionId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.Deposit);

                // 1. Create Accounts Receivable entry
                var ar = new AccountsReceivable
                {
                    CustomerName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now, // Deposits are immediate
                    Amount = amount,
                    ReceivedAmount = amount, // Already received
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Deposit via {depositMethod}. {notes}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.Deposit,
                    SourceTransactionId = sourceTransactionId,
                    SourceTable = "CustomerTransactions",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsReceivables.Add(ar);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.Deposit,
                    ARAPClassification = Classifications.AR,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "CustomerTransactions",
                    SourceRecordId = sourceTransactionId,
                    AccountsReceivableId = ar.ReceivableId,
                    Description = $"Deposit via {depositMethod}",
                    TransactionMethod = depositMethod,
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry (auto-posted)
                var journal = await CreateJournalEntryAsync(context,
                    referenceNumber,
                    $"Deposit - {customerName}",
                    amount,
                    "1001", "Cash", // Debit: Cash
                    "2001", "Customer Deposits", // Credit: Customer Deposits (Liability)
                    processedBy);

                // Update AR and History with Journal ID
                ar.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ar, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a WITHDRAWAL transaction - Creates AP entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsPayable ap, JournalEntry? journal)> RecordWithdrawalAsync(
            int customerAccountId,
            string accountNumber,
            string customerName,
            decimal amount,
            string withdrawalMethod,
            int sourceTransactionId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.Withdrawal);

                // 1. Create Accounts Payable entry
                var ap = new AccountsPayable
                {
                    VendorName = customerName, // Customer receiving the withdrawal
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now, // Withdrawals are immediate
                    Amount = amount,
                    PaidAmount = amount, // Already paid out
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Withdrawal via {withdrawalMethod}. {notes}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.Withdrawal,
                    SourceTransactionId = sourceTransactionId,
                    SourceTable = "CustomerTransactions",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsPayables.Add(ap);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.Withdrawal,
                    ARAPClassification = Classifications.AP,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "CustomerTransactions",
                    SourceRecordId = sourceTransactionId,
                    AccountsPayableId = ap.PayableId,
                    Description = $"Withdrawal via {withdrawalMethod}",
                    TransactionMethod = withdrawalMethod,
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry (auto-posted)
                var journal = await CreateJournalEntryAsync(context,
                    referenceNumber,
                    $"Withdrawal - {customerName}",
                    amount,
                    "2001", "Customer Deposits", // Debit: Customer Deposits (Liability decrease)
                    "1001", "Cash", // Credit: Cash
                    processedBy);

                // Update AP and History with Journal ID
                ap.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ap, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a TRANSFER transaction - Creates both AP (sender) and AR (receiver) entries
        /// </summary>
        public async Task<(UnifiedTransactionHistory senderHistory, UnifiedTransactionHistory receiverHistory,
            AccountsPayable ap, AccountsReceivable ar, JournalEntry? journal)> RecordTransferAsync(
            int senderAccountId,
            string senderAccountNumber,
            string senderName,
            int receiverAccountId,
            string receiverAccountNumber,
            string receiverName,
            decimal amount,
            decimal fee,
            int senderTransactionId,
            int receiverTransactionId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? purpose = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.Transfer);

                // 1. Create Accounts Payable for sender (money going OUT)
                var ap = new AccountsPayable
                {
                    VendorName = receiverName, // Receiver is the vendor receiving payment
                    InvoiceNumber = $"{referenceNumber}-OUT",
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = amount + fee,
                    PaidAmount = amount + fee,
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Transfer to {receiverName} ({receiverAccountNumber}). Purpose: {purpose}. Fee: ₱{fee:N2}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.Transfer,
                    SourceTransactionId = senderTransactionId,
                    SourceTable = "CustomerTransactions",
                    CustomerAccountId = senderAccountId,
                    ReferenceNumber = $"{referenceNumber}-OUT",
                    ReviewStatus = "Pending"
                };
                context.AccountsPayables.Add(ap);

                // 2. Create Accounts Receivable for receiver (money coming IN)
                var ar = new AccountsReceivable
                {
                    CustomerName = receiverName,
                    InvoiceNumber = $"{referenceNumber}-IN",
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = amount,
                    ReceivedAmount = amount,
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Transfer from {senderName} ({senderAccountNumber}). Purpose: {purpose}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.Transfer,
                    SourceTransactionId = receiverTransactionId,
                    SourceTable = "CustomerTransactions",
                    CustomerAccountId = receiverAccountId,
                    ReferenceNumber = $"{referenceNumber}-IN",
                    ReviewStatus = "Pending"
                };
                context.AccountsReceivables.Add(ar);
                await context.SaveChangesAsync();

                // 3. Create Unified Transaction History for sender (AP)
                var senderHistory = new UnifiedTransactionHistory
                {
                    ReferenceNumber = $"{referenceNumber}-OUT",
                    TransactionType = TransactionTypes.Transfer,
                    ARAPClassification = Classifications.AP,
                    Amount = amount,
                    FeeAmount = fee,
                    TotalAmount = amount + fee,
                    CustomerAccountId = senderAccountId,
                    AccountNumber = senderAccountNumber,
                    CustomerName = senderName,
                    SecondaryAccountId = receiverAccountId,
                    SecondaryAccountNumber = receiverAccountNumber,
                    SourceTable = "CustomerTransactions",
                    SourceRecordId = senderTransactionId,
                    AccountsPayableId = ap.PayableId,
                    Description = $"Transfer OUT to {receiverName}",
                    TransactionMethod = "BankTransfer",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = purpose
                };
                context.UnifiedTransactionHistory.Add(senderHistory);

                // 4. Create Unified Transaction History for receiver (AR)
                var receiverHistory = new UnifiedTransactionHistory
                {
                    ReferenceNumber = $"{referenceNumber}-IN",
                    TransactionType = TransactionTypes.Transfer,
                    ARAPClassification = Classifications.AR,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = receiverAccountId,
                    AccountNumber = receiverAccountNumber,
                    CustomerName = receiverName,
                    SecondaryAccountId = senderAccountId,
                    SecondaryAccountNumber = senderAccountNumber,
                    SourceTable = "CustomerTransactions",
                    SourceRecordId = receiverTransactionId,
                    AccountsReceivableId = ar.ReceivableId,
                    Description = $"Transfer IN from {senderName}",
                    TransactionMethod = "BankTransfer",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = purpose
                };
                context.UnifiedTransactionHistory.Add(receiverHistory);
                await context.SaveChangesAsync();

                // 5. Create Journal Entry for the transfer
                var journal = await CreateTransferJournalEntryAsync(context,
                    referenceNumber,
                    $"Transfer - {senderName} to {receiverName}",
                    amount,
                    fee,
                    processedBy);

                // Update with Journal ID
                ap.JournalEntryId = journal.JournalId;
                ar.JournalEntryId = journal.JournalId;
                senderHistory.JournalEntryId = journal.JournalId;
                receiverHistory.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (senderHistory, receiverHistory, ap, ar, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a LOAN PAYMENT transaction - Creates AR entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsReceivable ar, JournalEntry? journal)> RecordLoanPaymentAsync(
            int customerAccountId,
            string accountNumber,
            string customerName,
            int loanId,
            decimal principalAmount,
            decimal interestAmount,
            decimal penaltyAmount,
            int paymentId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.LoanPayment);
                var totalAmount = principalAmount + interestAmount + penaltyAmount;

                // 1. Create Accounts Receivable entry
                var ar = new AccountsReceivable
                {
                    CustomerName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = totalAmount,
                    ReceivedAmount = totalAmount,
                    OutstandingAmount = 0,
                    InterestAmount = interestAmount,
                    PenaltyAmount = penaltyAmount,
                    Status = "Pending Review", // Needs FM approval
                    Description = $"Loan#{loanId} Payment | Principal: ₱{principalAmount:N2} | Interest: ₱{interestAmount:N2} | Penalty: ₱{penaltyAmount:N2}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.LoanPayment,
                    SourceTransactionId = paymentId,
                    SourceTable = "LoanPayments",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsReceivables.Add(ar);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.LoanPayment,
                    ARAPClassification = Classifications.AR,
                    Amount = principalAmount,
                    InterestAmount = interestAmount,
                    PenaltyAmount = penaltyAmount,
                    TotalAmount = totalAmount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "LoanPayments",
                    SourceRecordId = paymentId,
                    AccountsReceivableId = ar.ReceivableId,
                    Description = $"Loan Payment - Loan #{loanId}",
                    TransactionMethod = "LoanPayment",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry
                var journal = await CreateLoanPaymentJournalEntryAsync(context,
                    referenceNumber,
                    $"Loan Payment - {customerName} - Loan #{loanId}",
                    principalAmount,
                    interestAmount,
                    penaltyAmount,
                    processedBy);

                // Update with Journal ID
                ar.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ar, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a LOAN RELEASE/DISBURSAL transaction - Creates AP entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsPayable ap, JournalEntry? journal)> RecordLoanReleaseAsync(
            int customerAccountId,
            string accountNumber,
            string customerName,
            int loanId,
            decimal amount,
            int disbursalId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.LoanRelease);

                // 1. Create Accounts Payable entry
                var ap = new AccountsPayable
                {
                    VendorName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = amount,
                    PaidAmount = amount,
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Loan#{loanId} Release/Disbursal",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.LoanRelease,
                    SourceTransactionId = disbursalId,
                    SourceTable = "LoanDisbursals",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsPayables.Add(ap);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.LoanRelease,
                    ARAPClassification = Classifications.AP,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "LoanDisbursals",
                    SourceRecordId = disbursalId,
                    AccountsPayableId = ap.PayableId,
                    Description = $"Loan Release - Loan #{loanId}",
                    TransactionMethod = "LoanDisbursal",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry
                var journal = await CreateJournalEntryAsync(context,
                    referenceNumber,
                    $"Loan Release - {customerName} - Loan #{loanId}",
                    amount,
                    "1301", "Loans Receivable", // Debit: Loans Receivable (Asset)
                    "1001", "Cash", // Credit: Cash
                    processedBy);

                // Update with Journal ID
                ap.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ap, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a SAVINGS DEPOSIT transaction - Creates AR entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsReceivable ar, JournalEntry? journal)> RecordSavingsDepositAsync(
            int savingsAccountId,
            int customerAccountId,
            string accountNumber,
            string customerName,
            decimal amount,
            int transactionId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.SavingsDeposit);

                // 1. Create Accounts Receivable entry
                var ar = new AccountsReceivable
                {
                    CustomerName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = amount,
                    ReceivedAmount = amount,
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Savings Deposit - Account #{savingsAccountId}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.SavingsDeposit,
                    SourceTransactionId = transactionId,
                    SourceTable = "SavingsTransactions",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsReceivables.Add(ar);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.SavingsDeposit,
                    ARAPClassification = Classifications.AR,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "SavingsTransactions",
                    SourceRecordId = transactionId,
                    AccountsReceivableId = ar.ReceivableId,
                    Description = $"Savings Deposit",
                    TransactionMethod = "SavingsDeposit",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry
                var journal = await CreateJournalEntryAsync(context,
                    referenceNumber,
                    $"Savings Deposit - {customerName}",
                    amount,
                    "1001", "Cash", // Debit: Cash
                    "2101", "Savings Deposits", // Credit: Savings Deposits (Liability)
                    processedBy);

                ar.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ar, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record a SAVINGS WITHDRAWAL transaction - Creates AP entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsPayable ap, JournalEntry? journal)> RecordSavingsWithdrawalAsync(
            int savingsAccountId,
            int customerAccountId,
            string accountNumber,
            string customerName,
            decimal amount,
            int transactionId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.SavingsWithdrawal);

                // 1. Create Accounts Payable entry
                var ap = new AccountsPayable
                {
                    VendorName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = amount,
                    PaidAmount = amount,
                    OutstandingAmount = 0,
                    Status = "Paid",
                    Description = $"Savings Withdrawal - Account #{savingsAccountId}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.SavingsWithdrawal,
                    SourceTransactionId = transactionId,
                    SourceTable = "SavingsTransactions",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsPayables.Add(ap);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.SavingsWithdrawal,
                    ARAPClassification = Classifications.AP,
                    Amount = amount,
                    TotalAmount = amount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "SavingsTransactions",
                    SourceRecordId = transactionId,
                    AccountsPayableId = ap.PayableId,
                    Description = $"Savings Withdrawal",
                    TransactionMethod = "SavingsWithdrawal",
                    Status = "Completed",
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // 3. Create Journal Entry
                var journal = await CreateJournalEntryAsync(context,
                    referenceNumber,
                    $"Savings Withdrawal - {customerName}",
                    amount,
                    "2101", "Savings Deposits", // Debit: Savings Deposits (Liability decrease)
                    "1001", "Cash", // Credit: Cash
                    processedBy);

                ap.JournalEntryId = journal.JournalId;
                history.JournalEntryId = journal.JournalId;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (history, ap, journal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Record SAVINGS INTEREST PAYOUT - Creates AP entry + Unified History + Journal Entry
        /// </summary>
        public async Task<(UnifiedTransactionHistory history, AccountsPayable ap, JournalEntry? journal)> RecordSavingsInterestAsync(
            int savingsAccountId,
            int customerAccountId,
            string accountNumber,
            string customerName,
            decimal interestAmount,
            int interestRecordId,
            string processedBy,
            int? processedByEmployeeId = null,
            string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var referenceNumber = GenerateReferenceNumber(ReferencePrefixes.SavingsInterest);

                // 1. Create Accounts Payable entry (Bank pays interest to customer)
                var ap = new AccountsPayable
                {
                    VendorName = customerName,
                    InvoiceNumber = referenceNumber,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Amount = interestAmount,
                    PaidAmount = 0, // Not paid until FM approves
                    OutstandingAmount = interestAmount,
                    Status = "Pending", // Needs FM review
                    Description = $"Savings Interest Payout - Account #{savingsAccountId}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = processedBy,
                    TransactionType = TransactionTypes.SavingsInterest,
                    SourceTransactionId = interestRecordId,
                    SourceTable = "SavingsInterestRecords",
                    CustomerAccountId = customerAccountId,
                    ReferenceNumber = referenceNumber,
                    ReviewStatus = "Pending"
                };
                context.AccountsPayables.Add(ap);
                await context.SaveChangesAsync();

                // 2. Create Unified Transaction History
                var history = new UnifiedTransactionHistory
                {
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionTypes.SavingsInterest,
                    ARAPClassification = Classifications.AP,
                    Amount = interestAmount,
                    InterestAmount = interestAmount,
                    TotalAmount = interestAmount,
                    CustomerAccountId = customerAccountId,
                    AccountNumber = accountNumber,
                    CustomerName = customerName,
                    SourceTable = "SavingsInterestRecords",
                    SourceRecordId = interestRecordId,
                    AccountsPayableId = ap.PayableId,
                    Description = $"Savings Interest Payout",
                    TransactionMethod = "InterestPayout",
                    Status = "Pending", // Pending FM approval
                    ProcessedBy = processedBy,
                    ProcessedByEmployeeId = processedByEmployeeId,
                    TransactionDate = DateTime.Now,
                    Notes = notes
                };
                context.UnifiedTransactionHistory.Add(history);
                await context.SaveChangesAsync();

                // Journal Entry will be created when FM approves
                await transaction.CommitAsync();

                return (history, ap, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Get unified transaction history with filters
        /// </summary>
        public async Task<List<UnifiedTransactionHistory>> GetTransactionHistoryAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? transactionType = null,
            string? arapClassification = null,
            int? customerAccountId = null,
            string? status = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.UnifiedTransactionHistory
                .Include(h => h.CustomerAccount)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(h => h.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(h => h.TransactionDate <= endDate.Value);

            if (!string.IsNullOrEmpty(transactionType))
                query = query.Where(h => h.TransactionType == transactionType);

            if (!string.IsNullOrEmpty(arapClassification))
                query = query.Where(h => h.ARAPClassification == arapClassification);

            if (customerAccountId.HasValue)
                query = query.Where(h => h.CustomerAccountId == customerAccountId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(h => h.Status == status);

            return await query
                .OrderByDescending(h => h.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get AR entries pending FM review
        /// </summary>
        public async Task<List<AccountsReceivable>> GetPendingARReviewAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountsReceivables
                .Where(ar => ar.ReviewStatus == "Pending")
                .OrderByDescending(ar => ar.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get AP entries pending FM review
        /// </summary>
        public async Task<List<AccountsPayable>> GetPendingAPReviewAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountsPayables
                .Where(ap => ap.ReviewStatus == "Pending")
                .OrderByDescending(ap => ap.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Approve AR entry (Finance Manager)
        /// </summary>
        public async Task ApproveAREntryAsync(int receivableId, string approvedBy, string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var ar = await context.AccountsReceivables.FindAsync(receivableId);
            if (ar == null) throw new KeyNotFoundException($"AR entry {receivableId} not found");

            ar.ReviewStatus = "Approved";
            ar.ReviewedBy = approvedBy;
            ar.ReviewedAt = DateTime.Now;
            ar.ReviewNotes = notes;
            ar.Status = "Paid"; // FM approved

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Approve AP entry (Finance Manager)
        /// </summary>
        public async Task ApproveAPEntryAsync(int payableId, string approvedBy, string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var ap = await context.AccountsPayables.FindAsync(payableId);
            if (ap == null) throw new KeyNotFoundException($"AP entry {payableId} not found");

            ap.ReviewStatus = "Approved";
            ap.ReviewedBy = approvedBy;
            ap.ReviewedAt = DateTime.Now;
            ap.ReviewNotes = notes;
            ap.Status = "Approved";

            await context.SaveChangesAsync();
        }

        // =============================================
        // PRIVATE HELPER METHODS
        // =============================================

        private async Task<JournalEntry> CreateJournalEntryAsync(
            BFASDbContext context,
            string reference,
            string description,
            decimal amount,
            string debitAccountCode,
            string debitAccountName,
            string creditAccountCode,
            string creditAccountName,
            string createdBy)
        {
            var journalNumber = $"JE-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            var journal = new JournalEntry
            {
                JournalNumber = journalNumber,
                TransactionDate = DateTime.Now,
                Description = description,
                Reference = reference,
                TotalDebit = amount,
                TotalCredit = amount,
                Status = "Posted",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                PostedAt = DateTime.Now,
                PostedBy = createdBy
            };
            context.JournalEntries.Add(journal);
            await context.SaveChangesAsync();

            // Add debit line
            var debitLine = new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = debitAccountCode,
                AccountName = debitAccountName,
                Description = description,
                DebitAmount = amount,
                CreditAmount = 0
            };
            context.JournalEntryLines.Add(debitLine);

            // Add credit line
            var creditLine = new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = creditAccountCode,
                AccountName = creditAccountName,
                Description = description,
                DebitAmount = 0,
                CreditAmount = amount
            };
            context.JournalEntryLines.Add(creditLine);

            await context.SaveChangesAsync();

            return journal;
        }

        private async Task<JournalEntry> CreateTransferJournalEntryAsync(
            BFASDbContext context,
            string reference,
            string description,
            decimal amount,
            decimal fee,
            string createdBy)
        {
            var journalNumber = $"JE-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            var totalAmount = amount + fee;

            var journal = new JournalEntry
            {
                JournalNumber = journalNumber,
                TransactionDate = DateTime.Now,
                Description = description,
                Reference = reference,
                TotalDebit = totalAmount,
                TotalCredit = totalAmount,
                Status = "Posted",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                PostedAt = DateTime.Now,
                PostedBy = createdBy
            };
            context.JournalEntries.Add(journal);
            await context.SaveChangesAsync();

            // Debit: Sender's Account (decrease liability)
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = "2001",
                AccountName = "Customer Deposits - Sender",
                Description = $"Transfer out - {description}",
                DebitAmount = totalAmount,
                CreditAmount = 0
            });

            // Credit: Receiver's Account (increase liability)
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = "2001",
                AccountName = "Customer Deposits - Receiver",
                Description = $"Transfer in - {description}",
                DebitAmount = 0,
                CreditAmount = amount
            });

            // Credit: Transfer Fee Income
            if (fee > 0)
            {
                context.JournalEntryLines.Add(new JournalEntryLine
                {
                    JournalId = journal.JournalId,
                    AccountCode = "4101",
                    AccountName = "Transfer Fee Income",
                    Description = $"Transfer fee - {description}",
                    DebitAmount = 0,
                    CreditAmount = fee
                });
            }

            await context.SaveChangesAsync();
            return journal;
        }

        private async Task<JournalEntry> CreateLoanPaymentJournalEntryAsync(
            BFASDbContext context,
            string reference,
            string description,
            decimal principalAmount,
            decimal interestAmount,
            decimal penaltyAmount,
            string createdBy)
        {
            var journalNumber = $"JE-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            var totalAmount = principalAmount + interestAmount + penaltyAmount;

            var journal = new JournalEntry
            {
                JournalNumber = journalNumber,
                TransactionDate = DateTime.Now,
                Description = description,
                Reference = reference,
                TotalDebit = totalAmount,
                TotalCredit = totalAmount,
                Status = "Posted",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                PostedAt = DateTime.Now,
                PostedBy = createdBy
            };
            context.JournalEntries.Add(journal);
            await context.SaveChangesAsync();

            // Debit: Cash (total payment received)
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = "1001",
                AccountName = "Cash",
                Description = $"Loan payment received - {description}",
                DebitAmount = totalAmount,
                CreditAmount = 0
            });

            // Credit: Loans Receivable (principal)
            context.JournalEntryLines.Add(new JournalEntryLine
            {
                JournalId = journal.JournalId,
                AccountCode = "1301",
                AccountName = "Loans Receivable",
                Description = $"Principal reduction - {description}",
                DebitAmount = 0,
                CreditAmount = principalAmount
            });

            // Credit: Interest Income
            if (interestAmount > 0)
            {
                context.JournalEntryLines.Add(new JournalEntryLine
                {
                    JournalId = journal.JournalId,
                    AccountCode = "4201",
                    AccountName = "Interest Income",
                    Description = $"Loan interest - {description}",
                    DebitAmount = 0,
                    CreditAmount = interestAmount
                });
            }

            // Credit: Penalty Income
            if (penaltyAmount > 0)
            {
                context.JournalEntryLines.Add(new JournalEntryLine
                {
                    JournalId = journal.JournalId,
                    AccountCode = "4202",
                    AccountName = "Penalty Income",
                    Description = $"Late payment penalty - {description}",
                    DebitAmount = 0,
                    CreditAmount = penaltyAmount
                });
            }

            await context.SaveChangesAsync();
            return journal;
        }
    }
}
