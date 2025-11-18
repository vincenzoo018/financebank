using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for Teller and Admin to process customer deposits and withdrawals
    /// </summary>
    public class TellerBankingService
    {
        private readonly BFASDbContext _context;
        private readonly AuthService _authService;

        public TellerBankingService(BFASDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Search for customer accounts by customer name
        /// </summary>
        public async Task<List<(int AccountId, string AccountNumber, string CustomerName, decimal Balance)>> SearchCustomersByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return new List<(int, string, string, decimal)>();

            var results = await _context.CustomerAccounts
                .Where(ca => ca.Customer != null && 
                       (ca.Customer.FullName.Contains(searchTerm) || 
                        ca.AccountNumber.Contains(searchTerm)) &&
                       ca.IsActive)
                .Select(ca => new
                {
                    ca.AccountId,
                    ca.AccountNumber,
                    CustomerName = ca.Customer.FullName,
                    ca.Balance
                })
                .ToListAsync();

            return results.Select(r => (r.AccountId, r.AccountNumber, r.CustomerName, r.Balance)).ToList();
        }

        /// <summary>
        /// Get customer account details by account number
        /// </summary>
        public async Task<(bool success, CustomerAccount? account, string message)> GetAccountByNumberAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return (false, null, "Account number is required");

            var account = await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.IsActive);

            if (account == null)
                return (false, null, "Account not found or inactive");

            return (true, account, "Account found");
        }

        /// <summary>
        /// Process a deposit by Teller/Admin for a customer
        /// </summary>
        public async Task<(bool success, string message, CustomerTransaction? transaction, string receiptNumber)> ProcessDepositAsync(
            string accountNumber,
            decimal amount,
            string depositMethod,
            string? reference = null,
            string? notes = null)
        {
            if (amount <= 0)
                return (false, "Deposit amount must be greater than 0", null, "");

            if (string.IsNullOrWhiteSpace(accountNumber))
                return (false, "Account number is required", null, "");

            if (string.IsNullOrWhiteSpace(depositMethod))
                return (false, "Deposit method is required", null, "");

            if (_context == null)
                return (false, "Database context is not configured", null, "");

            var account = await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.IsActive);

            if (account == null)
                return (false, "Account not found or inactive", null, "");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update account balance
                account.Balance += amount;
                account.AvailableBalance += amount;
                account.LastTransactionAt = DateTime.Now;

                // Generate receipt number
                string receiptNumber = GenerateReceiptNumber("DEP");

                // Create transaction record
                var customerTransaction = new CustomerTransaction
                {
                    AccountId = account.AccountId,
                    TransactionNumber = receiptNumber,
                    TransactionType = "Deposit",
                    Amount = amount,
                    Fee = 0,
                    Status = "Completed",
                    Description = $"Deposit via {depositMethod}. {notes}",
                    Reference = reference ?? GenerateReference("DEPREF"),
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    ProcessedBy = _authService.CurrentUser ?? "Teller"
                };

                _context.CustomerTransactions.Add(customerTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Deposit of ₱{amount:N2} processed successfully", customerTransaction, receiptNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error processing deposit: {ex.Message}", null, "");
            }
        }

        /// <summary>
        /// Process a withdrawal by Teller/Admin for a customer
        /// </summary>
        public async Task<(bool success, string message, CustomerTransaction? transaction, string receiptNumber)> ProcessWithdrawalAsync(
            string accountNumber,
            decimal amount,
            string withdrawalMethod,
            string? reference = null,
            string? notes = null)
        {
            if (amount <= 0)
                return (false, "Withdrawal amount must be greater than 0", null, "");

            if (string.IsNullOrWhiteSpace(accountNumber))
                return (false, "Account number is required", null, "");

            if (string.IsNullOrWhiteSpace(withdrawalMethod))
                return (false, "Withdrawal method is required", null, "");

            if (_context == null)
                return (false, "Database context is not configured", null, "");

            var account = await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.IsActive);

            if (account == null)
                return (false, "Account not found or inactive", null, "");

            if (account.AvailableBalance < amount)
                return (false, $"Insufficient balance. Available: ₱{account.AvailableBalance:N2}", null, "");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update account balance
                account.Balance -= amount;
                account.AvailableBalance -= amount;
                account.LastTransactionAt = DateTime.Now;

                // Generate receipt number
                string receiptNumber = GenerateReceiptNumber("WDR");

                // Create transaction record
                var customerTransaction = new CustomerTransaction
                {
                    AccountId = account.AccountId,
                    TransactionNumber = receiptNumber,
                    TransactionType = "Withdrawal",
                    Amount = amount,
                    Fee = 0,
                    Status = "Completed",
                    Description = $"Withdrawal via {withdrawalMethod}. {notes}",
                    Reference = reference ?? GenerateReference("WDRREF"),
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now,
                    ProcessedBy = _authService.CurrentUser ?? "Teller"
                };

                _context.CustomerTransactions.Add(customerTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Withdrawal of ₱{amount:N2} processed successfully", customerTransaction, receiptNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error processing withdrawal: {ex.Message}", null, "");
            }
        }

        /// <summary>
        /// Get transaction history for a specific account
        /// </summary>
        public async Task<List<CustomerTransaction>> GetAccountTransactionHistoryAsync(int accountId, int limit = 50)
        {
            return await _context.CustomerTransactions
                .Where(ct => ct.AccountId == accountId)
                .OrderByDescending(ct => ct.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Get all transactions for today (for Teller dashboard)
        /// </summary>
        public async Task<List<(string AccountNumber, string CustomerName, string Type, decimal Amount, DateTime Time)>> GetTodayTransactionsAsync()
        {
            var today = DateTime.Now.Date;

            var transactions = await _context.CustomerTransactions
                .Include(ct => ct.Account)
                .ThenInclude(ca => ca.Customer)
                .Where(ct => ct.CreatedAt.Date == today)
                .OrderByDescending(ct => ct.CreatedAt)
                .Select(ct => new
                {
                    ct.Account.AccountNumber,
                    CustomerName = ct.Account.Customer.FullName,
                    ct.Amount,
                    ct.TransactionType,
                    ct.CreatedAt
                })
                .ToListAsync();

            return transactions.Select(t => (t.AccountNumber, t.CustomerName, t.TransactionType, t.Amount, t.CreatedAt)).ToList();
        }

        /// <summary>
        /// Generate a unique receipt number
        /// </summary>
        private string GenerateReceiptNumber(string prefix)
        {
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// Generate a unique reference number
        /// </summary>
        private string GenerateReference(string prefix)
        {
            return $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}
