using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for Accounting module CRUD operations
    /// </summary>
    public class AccountingService
    {
        private readonly BFASDbContext _context;

        public AccountingService(BFASDbContext context)
        {
            _context = context;
        }

        // Chart of Accounts
        public async Task<List<ChartOfAccounts>> GetAllChartOfAccountsAsync()
        {
            try
            {
                return await _context.ChartOfAccounts.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving chart of accounts: {ex.Message}", ex);
            }
        }

        public async Task<ChartOfAccounts?> GetChartOfAccountsByIdAsync(int id)
        {
            try
            {
                return await _context.ChartOfAccounts.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving chart of accounts: {ex.Message}", ex);
            }
        }

        public async Task<ChartOfAccounts> CreateChartOfAccountsAsync(ChartOfAccounts account)
        {
            try
            {
                _context.ChartOfAccounts.Add(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating chart of accounts: {ex.Message}", ex);
            }
        }

        public async Task<ChartOfAccounts> UpdateChartOfAccountsAsync(ChartOfAccounts account)
        {
            try
            {
                _context.ChartOfAccounts.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating chart of accounts: {ex.Message}", ex);
            }
        }

        public async Task DeleteChartOfAccountsAsync(int id)
        {
            try
            {
                var account = await GetChartOfAccountsByIdAsync(id);
                if (account != null)
                {
                    _context.ChartOfAccounts.Remove(account);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting chart of accounts: {ex.Message}", ex);
            }
        }

        // Enhanced Chart of Accounts Methods
        public async Task<List<ChartOfAccounts>> GetChartOfAccountsByTypeAsync(string accountType)
        {
            try
            {
                return await _context.ChartOfAccounts
                    .Where(a => a.AccountType == accountType)
                    .OrderBy(a => a.AccountCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving chart of accounts by type: {ex.Message}", ex);
            }
        }

        public async Task<ChartOfAccounts?> GetChartOfAccountsByCodeAsync(string accountCode)
        {
            try
            {
                return await _context.ChartOfAccounts
                    .FirstOrDefaultAsync(a => a.AccountCode == accountCode);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving chart of accounts by code: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, decimal>> GetAccountBalancesSummaryAsync()
        {
            try
            {
                var summary = new Dictionary<string, decimal>();
                var accountTypes = new[] { "Asset", "Liability", "Equity", "Revenue", "Expense" };

                foreach (var type in accountTypes)
                {
                    var balance = await _context.ChartOfAccounts
                        .Where(a => a.AccountType == type && a.IsActive)
                        .SumAsync(a => a.CurrentBalance);
                    summary[type] = balance;
                }

                return summary;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving account balances summary: {ex.Message}", ex);
            }
        }

        public async Task UpdateAccountBalanceAsync(string accountCode, decimal amount, bool isDebit)
        {
            try
            {
                var account = await GetChartOfAccountsByCodeAsync(accountCode);
                if (account != null)
                {
                    // Update balance based on normal balance type
                    if (account.NormalBalance == "Debit")
                    {
                        account.CurrentBalance += isDebit ? amount : -amount;
                    }
                    else // Credit
                    {
                        account.CurrentBalance += isDebit ? -amount : amount;
                    }

                    _context.ChartOfAccounts.Update(account);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating account balance: {ex.Message}", ex);
            }
        }

        // Journal Entries
        public async Task<List<JournalEntry>> GetAllJournalEntriesAsync()
        {
            try
            {
                return await _context.JournalEntries.Include(j => j.Lines).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving journal entries: {ex.Message}", ex);
            }
        }

        public async Task<JournalEntry?> GetJournalEntryByIdAsync(int id)
        {
            try
            {
                return await _context.JournalEntries.Include(j => j.Lines).FirstOrDefaultAsync(j => j.JournalId == id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving journal entry: {ex.Message}", ex);
            }
        }

        public async Task<JournalEntry> CreateJournalEntryAsync(JournalEntry entry)
        {
            try
            {
                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating journal entry: {ex.Message}", ex);
            }
        }

        public async Task<JournalEntry> UpdateJournalEntryAsync(JournalEntry entry)
        {
            try
            {
                _context.JournalEntries.Update(entry);
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating journal entry: {ex.Message}", ex);
            }
        }

        // General Ledger
        public async Task<List<GeneralLedgerEntry>> GetAllGeneralLedgerEntriesAsync()
        {
            try
            {
                return await _context.GeneralLedgerEntries.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving general ledger entries: {ex.Message}", ex);
            }
        }

        public async Task<GeneralLedgerEntry?> GetGeneralLedgerEntryByIdAsync(int id)
        {
            try
            {
                return await _context.GeneralLedgerEntries.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving general ledger entry: {ex.Message}", ex);
            }
        }

        // Trial Balance
        public async Task<List<TrialBalanceEntry>> GetAllTrialBalancesAsync()
        {
            try
            {
                return await _context.TrialBalances.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving trial balances: {ex.Message}", ex);
            }
        }

        public async Task<TrialBalanceEntry?> GetTrialBalanceByIdAsync(int id)
        {
            try
            {
                return await _context.TrialBalances.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving trial balance: {ex.Message}", ex);
            }
        }

        // Financial Statements
        public async Task<List<FinancialStatement>> GetAllFinancialStatementsAsync()
        {
            try
            {
                return await _context.FinancialStatements.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving financial statements: {ex.Message}", ex);
            }
        }

        public async Task<FinancialStatement?> GetFinancialStatementByIdAsync(int id)
        {
            try
            {
                return await _context.FinancialStatements.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving financial statement: {ex.Message}", ex);
            }
        }

        public async Task<FinancialStatement> CreateFinancialStatementAsync(FinancialStatement statement)
        {
            try
            {
                _context.FinancialStatements.Add(statement);
                await _context.SaveChangesAsync();
                return statement;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating financial statement: {ex.Message}", ex);
            }
        }

        // Accounts Payable
        public async Task<List<AccountsPayable>> GetAllAccountsPayableAsync()
        {
            try
            {
                return await _context.AccountsPayables.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving accounts payable: {ex.Message}", ex);
            }
        }

        public async Task<AccountsPayable?> GetAccountsPayableByIdAsync(int id)
        {
            try
            {
                return await _context.AccountsPayables.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving accounts payable: {ex.Message}", ex);
            }
        }

        // Accounts Receivable
        public async Task<List<AccountsReceivable>> GetAllAccountsReceivableAsync()
        {
            try
            {
                return await _context.AccountsReceivables.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving accounts receivable: {ex.Message}", ex);
            }
        }

        public async Task<AccountsReceivable?> GetAccountsReceivableByIdAsync(int id)
        {
            try
            {
                return await _context.AccountsReceivables.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving accounts receivable: {ex.Message}", ex);
            }
        }
    }
}
