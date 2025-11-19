using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        /// Verify customer password for withdrawal authorization (security)
        /// </summary>
        public async Task<(bool success, string message, int attemptCount)> VerifyCustomerPasswordAsync(string accountNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(password))
                return (false, "Account number and password are required", 0);

            var account = await _context.CustomerAccounts
                .Include(ca => ca.Customer)
                .FirstOrDefaultAsync(ca => ca.AccountNumber == accountNumber && ca.IsActive);

            if (account?.Customer == null)
                return (false, "Account not found", 0);

            // Verify password using AuthService
            var (isValidPassword, authMessage) = await _authService.AuthenticateAsync(account.Customer.Username, password);

            if (!isValidPassword)
            {
                // Log failed attempt
                LogPasswordAttempt(account.Customer.UserId, accountNumber, false);
                return (false, "Invalid password", 0);
            }

            // Log successful attempt
            LogPasswordAttempt(account.Customer.UserId, accountNumber, true);
            return (true, "Password verified successfully", 0);
        }

        /// <summary>
        /// Log password verification attempts for security audit
        /// </summary>
        private void LogPasswordAttempt(int userId, string accountNumber, bool success)
        {
            // This would typically log to an audit table
            // For now, we'll just track in console
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var status = success ? "SUCCESS" : "FAILED";
            Console.WriteLine($"[SECURITY LOG] {timestamp} - User {userId} - Account {accountNumber} - Password Verification: {status}");
        }

        /// <summary>
        /// Process a withdrawal by Teller/Admin for a customer (with password verification)
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

            // Check minimum balance requirement (must keep at least 300)
            if (account.AvailableBalance - amount < 300)
                return (false, $"Cannot withdraw. Minimum balance of ₱300.00 must be maintained. Available after withdrawal would be: ₱{account.AvailableBalance - amount:N2}", null, "");

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

        /// <summary>
        /// Generate invoice for deposit or withdrawal
        /// </summary>
        public async Task<(bool success, Invoice? invoice, string message)> GenerateInvoiceAsync(
            int accountId,
            int transactionId,
            string invoiceType,
            decimal balanceBefore,
            decimal transactionAmount,
            decimal balanceAfter,
            string transactionMethod,
            string? reference,
            string? notes)
        {
            try
            {
                var account = await _context.CustomerAccounts
                    .Include(ca => ca.Customer)
                    .FirstOrDefaultAsync(ca => ca.AccountId == accountId);

                if (account == null)
                    return (false, null, "Account not found");

                var invoiceNumber = GenerateReceiptNumber(invoiceType == "Deposit" ? "INV-DEP" : "INV-WDR");

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceType = invoiceType,
                    AccountId = accountId,
                    TransactionId = transactionId,
                    CustomerName = account.Customer?.FullName ?? "Unknown",
                    AccountNumber = account.AccountNumber,
                    BalanceBefore = balanceBefore,
                    TransactionAmount = transactionAmount,
                    BalanceAfter = balanceAfter,
                    TransactionMethod = transactionMethod,
                    Reference = reference,
                    Notes = notes,
                    ProcessedBy = _authService.CurrentUser ?? "System",
                    Status = "Completed",
                    CreatedAt = DateTime.Now
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                return (true, invoice, "Invoice generated successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error generating invoice: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.Account)
                .Include(i => i.Transaction)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
        /// Get all invoices for an account
        /// </summary>
        public async Task<List<Invoice>> GetAccountInvoicesAsync(int accountId, int limit = 50)
        {
            return await _context.Invoices
                .Where(i => i.AccountId == accountId)
                .OrderByDescending(i => i.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Format invoice as HTML for printing/download
        /// </summary>
        public string FormatInvoiceAsHtml(Invoice invoice)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invoice {invoice.InvoiceNumber}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; border-bottom: 2px solid #059669; padding-bottom: 20px; margin-bottom: 20px; }}
        .header h1 {{ margin: 0; color: #059669; font-size: 24px; }}
        .header p {{ margin: 5px 0; color: #666; font-size: 12px; }}
        .invoice-type {{ display: inline-block; padding: 8px 16px; border-radius: 4px; font-weight: bold; margin-top: 10px; }}
        .invoice-type.deposit {{ background: #d1fae5; color: #065f46; }}
        .invoice-type.withdrawal {{ background: #fee2e2; color: #991b1b; }}
        .section {{ margin-bottom: 20px; }}
        .section-title {{ font-weight: bold; color: #1f2937; border-bottom: 1px solid #e5e7eb; padding-bottom: 8px; margin-bottom: 12px; }}
        .row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #f3f4f6; }}
        .row.total {{ border-bottom: 2px solid #059669; font-weight: bold; font-size: 16px; padding: 12px 0; }}
        .label {{ color: #6b7280; font-size: 12px; }}
        .value {{ color: #1f2937; font-weight: 500; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 11px; }}
        .amount {{ color: #059669; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>BANKING INVOICE</h1>
            <p>FinanceBank System</p>
            <span class='invoice-type {(invoice.InvoiceType == "Deposit" ? "deposit" : "withdrawal")}'>{invoice.InvoiceType.ToUpper()}</span>
        </div>

        <div class='section'>
            <div class='section-title'>Invoice Details</div>
            <div class='row'>
                <span class='label'>Invoice Number:</span>
                <span class='value'>{invoice.InvoiceNumber}</span>
            </div>
            <div class='row'>
                <span class='label'>Date & Time:</span>
                <span class='value'>{invoice.CreatedAt:yyyy-MM-dd HH:mm:ss}</span>
            </div>
            <div class='row'>
                <span class='label'>Invoice Type:</span>
                <span class='value'>{invoice.InvoiceType}</span>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Customer Information</div>
            <div class='row'>
                <span class='label'>Customer Name:</span>
                <span class='value'>{invoice.CustomerName}</span>
            </div>
            <div class='row'>
                <span class='label'>Account Number:</span>
                <span class='value'>{invoice.AccountNumber}</span>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Transaction Details</div>
            <div class='row'>
                <span class='label'>Balance Before:</span>
                <span class='value amount'>₱{invoice.BalanceBefore:N2}</span>
            </div>
            <div class='row'>
                <span class='label'>Transaction Amount:</span>
                <span class='value amount'>₱{invoice.TransactionAmount:N2}</span>
            </div>
            <div class='row'>
                <span class='label'>Transaction Method:</span>
                <span class='value'>{invoice.TransactionMethod}</span>
            </div>
            {(string.IsNullOrEmpty(invoice.Reference) ? "" : $"<div class='row'><span class='label'>Reference Number:</span><span class='value'>{invoice.Reference}</span></div>")}
            {(string.IsNullOrEmpty(invoice.Notes) ? "" : $"<div class='row'><span class='label'>Notes:</span><span class='value'>{invoice.Notes}</span></div>")}
            <div class='row total'>
                <span class='label'>Balance After:</span>
                <span class='value amount'>₱{invoice.BalanceAfter:N2}</span>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Processing Information</div>
            <div class='row'>
                <span class='label'>Processed By:</span>
                <span class='value'>{invoice.ProcessedBy}</span>
            </div>
            <div class='row'>
                <span class='label'>Status:</span>
                <span class='value'>{invoice.Status}</span>
            </div>
        </div>

        <div class='footer'>
            <p>Thank you for banking with us!</p>
            <p>Please keep this invoice for your records.</p>
            <p style='margin-top: 15px; font-size: 10px;'>This is a system-generated document. No signature required.</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Format invoice as plain text for console/file output
        /// </summary>
        public string FormatInvoiceAsText(Invoice invoice)
        {
            return $@"
========================================================
                    {invoice.InvoiceType.ToUpper()} INVOICE                    
                  FINANCEBANK SYSTEM                  
========================================================

Invoice Number:    {invoice.InvoiceNumber}
Date & Time:       {invoice.CreatedAt:yyyy-MM-dd HH:mm:ss}
Invoice Type:      {invoice.InvoiceType}

========================================================
                  CUSTOMER INFORMATION                
========================================================

Customer Name:     {invoice.CustomerName}
Account Number:    {invoice.AccountNumber}

========================================================
                  TRANSACTION DETAILS                 
========================================================

Balance Before:    ₱{invoice.BalanceBefore:N2}
Transaction Amt:   ₱{invoice.TransactionAmount:N2}
Transaction Meth:  {invoice.TransactionMethod}
{(string.IsNullOrEmpty(invoice.Reference) ? "" : $"Reference:         {invoice.Reference}\n")}
{(string.IsNullOrEmpty(invoice.Notes) ? "" : $"Notes:              {invoice.Notes}\n")}
Balance After:     ₱{invoice.BalanceAfter:N2}

========================================================
                PROCESSING INFORMATION               
========================================================

Processed By:      {invoice.ProcessedBy}
Status:            {invoice.Status}

========================================================
Thank you for banking with us!
Please keep this invoice for your records.
========================================================
";
        }
    }
}
