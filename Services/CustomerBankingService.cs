using System;
using System.Linq;
using System.Threading.Tasks;
using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service that handles customer-facing banking operations such as
    /// deposits, withdrawals, and bill payments. It ties together
    /// CustomerAccount and CustomerTransaction updates in a single
    /// database operation so the balance and history stay in sync.
    /// </summary>
    public class CustomerBankingService
    {
        private readonly BFASDbContext? _context;
        private readonly AuthService _authService;

        public CustomerBankingService(BFASDbContext? context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Gets the primary active account for the currently authenticated customer.
        /// </summary>
        public async Task<CustomerAccount?> GetPrimaryAccountForCurrentCustomerAsync()
        {
            if (_context == null) return null;
            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue) return null;

            var customerId = _authService.CurrentUserId.Value;

            return await _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId && a.IsActive)
                .OrderBy(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets any account by account number (for recipient lookup in transfers).
        /// </summary>
        public async Task<CustomerAccount?> GetAccountByNumberAsync(string accountNumber)
        {
            if (_context == null || string.IsNullOrWhiteSpace(accountNumber))
                return null;

            return await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.IsActive);
        }

        /// <summary>
        /// Gets account by account number for the authenticated customer (security check).
        /// </summary>
        public async Task<(bool success, CustomerAccount? account, string message)> GetAccountByNumberForCurrentCustomerAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return (false, null, "Account number is required");

            if (_context == null)
                return (false, null, "Database context is not configured");

            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                return (false, null, "User is not authenticated");

            var customerId = _authService.CurrentUserId.Value;

            var account = await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.CustomerId == customerId && ca.IsActive);

            if (account == null)
                return (false, null, "Account not found or does not belong to this customer");

            return (true, account, "Account found");
        }

        /// <summary>
        /// Performs a deposit into the specified account (or the primary account
        /// of the current customer) and records a CustomerTransaction.
        /// </summary>
        public async Task<(bool success, string message, CustomerAccount? updatedAccount)> DepositAsync(
            decimal amount,
            string depositType,
            string? purpose = null,
            int? accountId = null)
        {
            if (amount <= 0)
                return (false, "Deposit amount must be greater than zero.", null);

            if (_context == null)
                return (false, "Database context is not configured.", null);

            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                return (false, "User is not authenticated.", null);

            var customerId = _authService.CurrentUserId.Value;

            var accountQuery = _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId && a.IsActive);

            if (accountId.HasValue && accountId.Value > 0)
            {
                accountQuery = accountQuery.Where(a => a.AccountId == accountId.Value);
            }

            var account = await accountQuery.FirstOrDefaultAsync();
            if (account == null)
                return (false, "No active account found for this customer.", null);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update balances
                account.Balance += amount;
                account.AvailableBalance += amount;
                account.LastTransactionAt = DateTime.Now;

                // Get employee ID for current user
                int? employeeId = await GetEmployeeIdForCurrentUserAsync();

                // Create customer transaction
                var customerTransaction = new CustomerTransaction
                {
                    AccountId = account.AccountId,
                    TransactionNumber = GenerateReference("DEP"),
                    TransactionType = "Deposit",
                    Amount = amount,
                    Fee = 0,
                    Status = "Completed",
                    Description = BuildDepositDescription(depositType, purpose),
                    Reference = GenerateReference("DEPREF"),
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    ProcessedByEmployeeId = employeeId
                };

                _context.CustomerTransactions.Add(customerTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Deposit successful.", account);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error processing deposit: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Performs a withdrawal for the current customer.
        /// </summary>
        public async Task<(bool success, string message, CustomerAccount? updatedAccount)> WithdrawAsync(
            decimal amount,
            string withdrawalMethod,
            string? purpose = null,
            int? accountId = null)
        {
            if (amount <= 0)
                return (false, "Withdrawal amount must be greater than zero.", null);

            if (_context == null)
                return (false, "Database context is not configured.", null);

            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                return (false, "User is not authenticated.", null);

            var customerId = _authService.CurrentUserId.Value;

            var accountQuery = _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId && a.IsActive);

            if (accountId.HasValue && accountId.Value > 0)
            {
                accountQuery = accountQuery.Where(a => a.AccountId == accountId.Value);
            }

            var account = await accountQuery.FirstOrDefaultAsync();
            if (account == null)
                return (false, "No active account found for this customer.", null);

            if (account.AvailableBalance < amount)
                return (false, "Insufficient available balance.", account);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                account.Balance -= amount;
                account.AvailableBalance -= amount;
                account.LastTransactionAt = DateTime.Now;

                // Get employee ID for current user
                int? employeeId = await GetEmployeeIdForCurrentUserAsync();

                var customerTransaction = new CustomerTransaction
                {
                    AccountId = account.AccountId,
                    TransactionNumber = GenerateReference("WDR"),
                    TransactionType = "Withdrawal",
                    Amount = amount,
                    Fee = 0,
                    Status = "Completed",
                    Description = BuildWithdrawalDescription(withdrawalMethod, purpose),
                    Reference = GenerateReference("WDRREF"),
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    ProcessedByEmployeeId = employeeId
                };

                _context.CustomerTransactions.Add(customerTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Withdrawal successful.", account);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error processing withdrawal: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Pays a bill from the current customer's account.
        /// </summary>
        public async Task<(bool success, string message, CustomerAccount? updatedAccount)> PayBillAsync(
            decimal amount,
            string billerName,
            string billerAccountNumber,
            string? description = null,
            int? accountId = null)
        {
            if (amount <= 0)
                return (false, "Payment amount must be greater than zero.", null);

            if (_context == null)
                return (false, "Database context is not configured.", null);

            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                return (false, "User is not authenticated.", null);

            var customerId = _authService.CurrentUserId.Value;

            var accountQuery = _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId && a.IsActive);

            if (accountId.HasValue && accountId.Value > 0)
            {
                accountQuery = accountQuery.Where(a => a.AccountId == accountId.Value);
            }

            var account = await accountQuery.FirstOrDefaultAsync();
            if (account == null)
                return (false, "No active account found for this customer.", null);

            if (account.AvailableBalance < amount)
                return (false, "Insufficient available balance.", account);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                account.Balance -= amount;
                account.AvailableBalance -= amount;
                account.LastTransactionAt = DateTime.Now;

                // Get employee ID for current user
                int? employeeId = await GetEmployeeIdForCurrentUserAsync();

                var customerTransaction = new CustomerTransaction
                {
                    AccountId = account.AccountId,
                    TransactionNumber = GenerateReference("BILL"),
                    TransactionType = "Bills",
                    Amount = amount,
                    Fee = 0,
                    Status = "Completed",
                    Description = description ?? $"Bills payment to {billerName}",
                    Reference = GenerateReference("BILLREF"),
                    BillerName = billerName,
                    BillerAccountNumber = billerAccountNumber,
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    ProcessedByEmployeeId = employeeId
                };

                _context.CustomerTransactions.Add(customerTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Bill payment successful.", account);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error processing bill payment: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Transfers money from the authenticated customer's account to another customer's account.
        /// </summary>
        public async Task<(bool success, string message)> TransferAsync(
            int fromAccountId,
            string toAccountNumber,
            decimal amount,
            string? purpose = null)
        {
            if (amount <= 0)
                return (false, "Transfer amount must be greater than zero.");

            if (_context == null)
                return (false, "Database context is not configured.");

            if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                return (false, "User is not authenticated.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get sender account
                    var senderAccount = await _context.CustomerAccounts
                        .FirstOrDefaultAsync(a => a.AccountId == fromAccountId && a.IsActive);

                    if (senderAccount == null)
                        return (false, "Sender account not found.");

                    if (senderAccount.Balance < amount)
                        return (false, "Insufficient balance for transfer.");

                    // Get recipient account
                    var recipientAccount = await _context.CustomerAccounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == toAccountNumber && a.IsActive);

                    if (recipientAccount == null)
                        return (false, "Recipient account not found.");

                    // Deduct from sender
                    senderAccount.Balance -= amount;
                    senderAccount.AvailableBalance -= amount;

                    // Add to recipient
                    recipientAccount.Balance += amount;
                    recipientAccount.AvailableBalance += amount;

                    // Get employee ID for current user
                    int? employeeId = await GetEmployeeIdForCurrentUserAsync();

                    // Create transaction record for sender
                    var senderTransaction = new CustomerTransaction
                    {
                        AccountId = senderAccount.AccountId,
                        TransactionNumber = GenerateReference("TRF"),
                        TransactionType = "Transfer",
                        Amount = amount,
                        Fee = 0,
                        Status = "Completed",
                        Description = string.IsNullOrWhiteSpace(purpose) 
                            ? $"Transfer to {recipientAccount.Customer?.FullName}" 
                            : $"Transfer to {recipientAccount.Customer?.FullName} - {purpose}",
                        Reference = GenerateReference("TRFREF"),
                        ToAccountName = recipientAccount.Customer?.FullName,
                        CreatedAt = DateTime.Now,
                        ProcessedAt = DateTime.Now,
                        ProcessedByEmployeeId = employeeId
                    };

                    // Create transaction record for recipient
                    var recipientTransaction = new CustomerTransaction
                    {
                        AccountId = recipientAccount.AccountId,
                        TransactionNumber = GenerateReference("TRF"),
                        TransactionType = "Transfer",
                        Amount = amount,
                        Fee = 0,
                        Status = "Completed",
                        Description = string.IsNullOrWhiteSpace(purpose) 
                            ? $"Transfer from {senderAccount.Customer?.FullName}" 
                            : $"Transfer from {senderAccount.Customer?.FullName} - {purpose}",
                        Reference = GenerateReference("TRFREF"),
                        ToAccountName = senderAccount.Customer?.FullName,
                        CreatedAt = DateTime.Now,
                        ProcessedAt = DateTime.Now,
                        ProcessedByEmployeeId = employeeId
                    };

                    _context.CustomerTransactions.Add(senderTransaction);
                    _context.CustomerTransactions.Add(recipientTransaction);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return (true, "Transfer successful.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error processing transfer: {ex.Message}");
                }
            }
        }

        private static string BuildDepositDescription(string depositType, string? purpose)
        {
            var baseText = string.IsNullOrWhiteSpace(depositType) ? "Deposit" : $"{depositType} deposit";
            if (string.IsNullOrWhiteSpace(purpose)) return baseText;
            return baseText + $" - {purpose}";
        }

        private static string BuildWithdrawalDescription(string withdrawalMethod, string? purpose)
        {
            var baseText = string.IsNullOrWhiteSpace(withdrawalMethod) ? "Withdrawal" : $"{withdrawalMethod} withdrawal";
            if (string.IsNullOrWhiteSpace(purpose)) return baseText;
            return baseText + $" - {purpose}";
        }

        private static string GenerateReference(string prefix)
        {
            return $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }

        /// <summary>
        /// Generates a formatted invoice for deposit transaction
        /// </summary>
        public static string GenerateDepositInvoice(
            string invoiceNumber,
            CustomerAccount account,
            decimal amount,
            string depositType,
            string reference,
            string? notes = null)
        {
            var invoice = $@"
========================================================
                    DEPOSIT INVOICE
                  BFAS Banking System
========================================================

Invoice Number:    {invoiceNumber}
Date & Time:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Account Number:    {account.AccountNumber}
Account Type:      Savings Account
Customer Name:     {account.Customer?.FullName ?? "N/A"}

Previous Balance:  ₱{(account.Balance - amount):N2}
Deposit Amount:    ₱{amount:N2}
New Balance:       ₱{account.Balance:N2}

Deposit Type:      {depositType}
Reference:         {reference}
Notes:             {notes ?? "N/A"}

========================================================
Thank you for banking with us!
Please keep this invoice for your records.
========================================================
";
            return invoice;
        }

        /// <summary>
        /// Generates a formatted invoice for withdrawal transaction
        /// </summary>
        public static string GenerateWithdrawalInvoice(
            string invoiceNumber,
            CustomerAccount account,
            decimal amount,
            string withdrawalMethod,
            string reference,
            string? notes = null)
        {
            var invoice = $@"
========================================================
                  WITHDRAWAL INVOICE
                  BFAS Banking System
========================================================

Invoice Number:    {invoiceNumber}
Date & Time:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Account Number:    {account.AccountNumber}
Account Type:      Savings Account
Customer Name:     {account.Customer?.FullName ?? "N/A"}

Previous Balance:  ₱{(account.Balance + amount):N2}
Withdrawal Amount: ₱{amount:N2}
New Balance:       ₱{account.Balance:N2}

Withdrawal Method: {withdrawalMethod}
Reference:         {reference}
Notes:             {notes ?? "N/A"}

========================================================
Thank you for banking with us!
Please keep this invoice for your records.
========================================================
";
            return invoice;
        }

        /// <summary>
        /// Get all invoices for a specific account
        /// </summary>
        public async Task<List<Invoice>> GetAccountInvoicesAsync(int accountId, int limit = 50)
        {
            if (_context == null)
                return new List<Invoice>();

            return await _context.Invoices
                .Where(i => i.AccountId == accountId)
                .OrderByDescending(i => i.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Get the Employee ID for the current authenticated user
        /// Joins Users → Employees to get the EmployeeId
        /// </summary>
        private async Task<int?> GetEmployeeIdForCurrentUserAsync()
        {
            try
            {
                if (_context == null || !_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
                    return null;

                var userId = _authService.CurrentUserId.Value;

                var employee = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .FirstOrDefaultAsync();

                if (employee == null)
                    return null;

                return employee.EmployeeId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee ID: {ex.Message}");
                return null;
            }
        }
    }
}
