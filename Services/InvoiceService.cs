using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for creating and managing invoices for all transactions
    /// Ensures data flows: CustomerTransaction → Invoice (for both Teller and Customer views)
    /// </summary>
    public class InvoiceService
    {
        private readonly IDbContextFactory<BFASDbContext> _contextFactory;

        public InvoiceService(IDbContextFactory<BFASDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Create an invoice for a deposit transaction
        /// Called after CustomerTransaction is created
        /// </summary>
        public async Task<(bool success, Invoice? invoice, string message)> CreateDepositInvoiceAsync(
            int accountId,
            int transactionId,
            decimal balanceBefore,
            decimal depositAmount,
            string depositMethod,
            string? reference = null,
            string? notes = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var account = await context.CustomerAccounts
                    .Include(ca => ca.Customer)
                    .FirstOrDefaultAsync(ca => ca.AccountId == accountId);

                if (account == null)
                    return (false, null, "Account not found");

                decimal balanceAfter = balanceBefore + depositAmount;

                // Get employee name from transaction
                var transaction = await context.CustomerTransactions
                    .Include(ct => ct.ProcessedByEmployee)
                    .FirstOrDefaultAsync(ct => ct.TransactionId == transactionId);

                string processedByName = "Teller";
                if (transaction?.ProcessedByEmployee != null)
                {
                    processedByName = $"{transaction.ProcessedByEmployee.FirstName} {transaction.ProcessedByEmployee.LastName}".Trim();
                }

                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber("DEP"),
                    InvoiceType = "Deposit",
                    AccountId = accountId,
                    TransactionId = transactionId,
                    CustomerName = account.Customer?.FullName ?? "Unknown",
                    AccountNumber = account.AccountNumber,
                    BalanceBefore = balanceBefore,
                    TransactionAmount = depositAmount,
                    BalanceAfter = balanceAfter,
                    TransactionMethod = depositMethod,
                    Reference = reference ?? "",
                    Notes = notes ?? "",
                    ProcessedBy = processedByName,
                    Status = "Completed",
                    CreatedAt = DateTime.Now
                };

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                return (true, invoice, "Deposit invoice created successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error creating deposit invoice: {ex.Message}");
            }
        }

        /// <summary>
        /// Create an invoice for a withdrawal transaction
        /// Called after CustomerTransaction is created
        /// </summary>
        public async Task<(bool success, Invoice? invoice, string message)> CreateWithdrawalInvoiceAsync(
            int accountId,
            int transactionId,
            decimal balanceBefore,
            decimal withdrawalAmount,
            string withdrawalMethod,
            string? reference = null,
            string? notes = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var account = await context.CustomerAccounts
                    .Include(ca => ca.Customer)
                    .FirstOrDefaultAsync(ca => ca.AccountId == accountId);

                if (account == null)
                    return (false, null, "Account not found");

                decimal balanceAfter = balanceBefore - withdrawalAmount;

                // Get employee name from transaction
                var transaction = await context.CustomerTransactions
                    .Include(ct => ct.ProcessedByEmployee)
                    .FirstOrDefaultAsync(ct => ct.TransactionId == transactionId);

                string processedByName = "Teller";
                if (transaction?.ProcessedByEmployee != null)
                {
                    processedByName = $"{transaction.ProcessedByEmployee.FirstName} {transaction.ProcessedByEmployee.LastName}".Trim();
                }

                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber("WDR"),
                    InvoiceType = "Withdrawal",
                    AccountId = accountId,
                    TransactionId = transactionId,
                    CustomerName = account.Customer?.FullName ?? "Unknown",
                    AccountNumber = account.AccountNumber,
                    BalanceBefore = balanceBefore,
                    TransactionAmount = withdrawalAmount,
                    BalanceAfter = balanceAfter,
                    TransactionMethod = withdrawalMethod,
                    Reference = reference ?? "",
                    Notes = notes ?? "",
                    ProcessedBy = processedByName,
                    Status = "Completed",
                    CreatedAt = DateTime.Now
                };

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                return (true, invoice, "Withdrawal invoice created successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error creating withdrawal invoice: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all invoices for a specific account (for customer view)
        /// Concatenates data from CustomerTransactions and Invoices
        /// </summary>
        public async Task<List<(int InvoiceId, string InvoiceNumber, string Type, decimal Amount, decimal BalanceBefore, decimal BalanceAfter, string Method, string Status, DateTime CreatedAt)>> GetAccountInvoicesAsync(int accountId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var invoices = await context.Invoices
                    .Where(i => i.AccountId == accountId)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InvoiceId,
                        i.InvoiceNumber,
                        i.InvoiceType,
                        i.TransactionAmount,
                        i.BalanceBefore,
                        i.BalanceAfter,
                        i.TransactionMethod,
                        i.Status,
                        i.CreatedAt
                    })
                    .ToListAsync();

                return invoices.Select(i => (
                    i.InvoiceId,
                    i.InvoiceNumber,
                    i.InvoiceType,
                    i.TransactionAmount,
                    i.BalanceBefore,
                    i.BalanceAfter,
                    i.TransactionMethod,
                    i.Status,
                    i.CreatedAt
                )).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching invoices: {ex.Message}");
                return new List<(int, string, string, decimal, decimal, decimal, string, string, DateTime)>();
            }
        }

        /// <summary>
        /// Get all invoices for today (for teller dashboard)
        /// </summary>
        public async Task<List<(string InvoiceNumber, string CustomerName, string Type, decimal Amount, string Status, DateTime CreatedAt)>> GetTodayInvoicesAsync()
        {
            try
            {
                var today = DateTime.Now.Date;

                using var context = await _contextFactory.CreateDbContextAsync();
                var invoices = await context.Invoices
                    .Where(i => i.CreatedAt.Date == today)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InvoiceNumber,
                        i.CustomerName,
                        i.InvoiceType,
                        i.TransactionAmount,
                        i.Status,
                        i.CreatedAt
                    })
                    .ToListAsync();

                return invoices.Select(i => (
                    i.InvoiceNumber,
                    i.CustomerName,
                    i.InvoiceType,
                    i.TransactionAmount,
                    i.Status,
                    i.CreatedAt
                )).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching today's invoices: {ex.Message}");
                return new List<(string, string, string, decimal, string, DateTime)>();
            }
        }

        /// <summary>
        /// Get invoice details with complete transaction information
        /// Concatenates data from Invoices, CustomerTransactions, and CustomerAccounts
        /// </summary>
        public async Task<dynamic?> GetInvoiceDetailsAsync(int invoiceId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var invoice = await context.Invoices
                    .Include(i => i.Account)
                    .Include(i => i.Transaction)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                    return null;

                return new
                {
                    invoice.InvoiceId,
                    invoice.InvoiceNumber,
                    invoice.InvoiceType,
                    invoice.AccountId,
                    invoice.TransactionId,
                    invoice.CustomerName,
                    invoice.AccountNumber,
                    invoice.BalanceBefore,
                    invoice.TransactionAmount,
                    invoice.BalanceAfter,
                    invoice.TransactionMethod,
                    invoice.Reference,
                    invoice.Notes,
                    invoice.ProcessedBy,
                    invoice.Status,
                    invoice.CreatedAt,
                    invoice.PrintedAt,
                    invoice.DownloadedAt,
                    // Additional transaction details
                    TransactionNumber = invoice.Transaction?.TransactionNumber,
                    TransactionType = invoice.Transaction?.TransactionType,
                    TransactionStatus = invoice.Transaction?.Status,
                    TransactionDescription = invoice.Transaction?.Description
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching invoice details: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all deposits and withdrawals for an account (combined view)
        /// Fetches from Invoices table and concatenates all relevant data
        /// </summary>
        public async Task<List<dynamic>> GetAllTransactionsForAccountAsync(int accountId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var invoices = await context.Invoices
                    .Where(i => i.AccountId == accountId)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InvoiceId,
                        i.InvoiceNumber,
                        i.InvoiceType,
                        i.TransactionId,
                        i.CustomerName,
                        i.AccountNumber,
                        i.BalanceBefore,
                        i.TransactionAmount,
                        i.BalanceAfter,
                        i.TransactionMethod,
                        i.Reference,
                        i.Notes,
                        i.ProcessedBy,
                        i.Status,
                        i.CreatedAt
                    })
                    .ToListAsync();

                var result = new List<dynamic>();
                foreach (var invoice in invoices)
                {
                    result.Add(new
                    {
                        InvoiceId = invoice.InvoiceId,
                        InvoiceNumber = invoice.InvoiceNumber,
                        Type = invoice.InvoiceType,
                        TransactionId = invoice.TransactionId,
                        CustomerName = invoice.CustomerName,
                        AccountNumber = invoice.AccountNumber,
                        BalanceBefore = invoice.BalanceBefore,
                        Amount = invoice.TransactionAmount,
                        BalanceAfter = invoice.BalanceAfter,
                        Method = invoice.TransactionMethod,
                        Reference = invoice.Reference,
                        Notes = invoice.Notes,
                        ProcessedBy = invoice.ProcessedBy,
                        Status = invoice.Status,
                        CreatedAt = invoice.CreatedAt
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all transactions: {ex.Message}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Mark invoice as printed
        /// </summary>
        public async Task<bool> MarkAsPrintedAsync(int invoiceId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var invoice = await context.Invoices.FindAsync(invoiceId);
                if (invoice == null) return false;

                invoice.PrintedAt = DateTime.Now;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking invoice as printed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mark invoice as downloaded
        /// </summary>
        public async Task<bool> MarkAsDownloadedAsync(int invoiceId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var invoice = await context.Invoices.FindAsync(invoiceId);
                if (invoice == null) return false;

                invoice.DownloadedAt = DateTime.Now;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking invoice as downloaded: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate a unique invoice number
        /// </summary>
        private string GenerateInvoiceNumber(string prefix)
        {
            return $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}
