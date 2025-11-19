using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services;

// =============================================
// BANKING MODULE CRUD SERVICES
// =============================================

public class BankAccountService
{
    private readonly BFASDbContext? _context;
    
    public BankAccountService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<BankAccount>> GetAllAsync()
    {
        if (_context == null) return GetMockData();
        return await _context.BankAccounts.ToListAsync();
    }

    public async Task<BankAccount?> GetByIdAsync(int id)
    {
        if (_context == null) return GetMockData().FirstOrDefault(x => x.BankAccountId == id);
        return await _context.BankAccounts.FindAsync(id);
    }

    public async Task<BankAccount> CreateAsync(BankAccount bankAccount)
    {
        if (_context == null)
        {
            bankAccount.BankAccountId = GetMockData().Count + 1;
            return bankAccount;
        }

        bankAccount.CreatedAt = DateTime.Now;
        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();
        return bankAccount;
    }

    public async Task<BankAccount> UpdateAsync(BankAccount bankAccount)
    {
        if (_context == null) return bankAccount;

        bankAccount.LastModifiedAt = DateTime.Now;
        _context.BankAccounts.Update(bankAccount);
        await _context.SaveChangesAsync();
        return bankAccount;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (_context == null) return true;

        var account = await _context.BankAccounts.FindAsync(id);
        if (account == null) return false;

        account.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<BankAccount> GetMockData()
    {
        return new List<BankAccount>
        {
            new() { BankAccountId = 1, AccountNumber = "ACC-001", AccountName = "Primary Operating Account", AccountType = "Checking", BankName = "FINSYS Bank", Balance = 1500000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-30) },
            new() { BankAccountId = 2, AccountNumber = "ACC-002", AccountName = "Savings Account", AccountType = "Savings", BankName = "FINSYS Bank", Balance = 750000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-15) },
            new() { BankAccountId = 3, AccountNumber = "ACC-003", AccountName = "Investment Account", AccountType = "Investment", BankName = "FINSYS Bank", Balance = 2250000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-7) }
        };
    }
}

public class FundTransferService
{
    private readonly BFASDbContext? _context;
    
    public FundTransferService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<FundTransfer>> GetAllAsync()
    {
        if (_context == null) return GetMockData();
        return await _context.FundTransfers
            .Include(ft => ft.FromAccount)
            .Include(ft => ft.ToAccount)
            .OrderByDescending(ft => ft.CreatedAt)
            .ToListAsync();
    }

    public async Task<FundTransfer?> GetByIdAsync(int id)
    {
        if (_context == null) return GetMockData().FirstOrDefault(x => x.TransferId == id);
        return await _context.FundTransfers
            .Include(ft => ft.FromAccount)
            .Include(ft => ft.ToAccount)
            .FirstOrDefaultAsync(ft => ft.TransferId == id);
    }

    public async Task<FundTransfer> CreateAsync(FundTransfer transfer)
    {
        if (_context == null)
        {
            transfer.TransferId = GetMockData().Count + 1;
            transfer.TransferNumber = $"TRF-{DateTime.Now:yyyyMMdd}-{transfer.TransferId:000}";
            return transfer;
        }

        transfer.TransferNumber = $"TRF-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        transfer.CreatedAt = DateTime.Now;
        _context.FundTransfers.Add(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<FundTransfer> UpdateAsync(FundTransfer transfer)
    {
        if (_context == null) return transfer;

        _context.FundTransfers.Update(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<bool> ApproveAsync(int id, string approvedBy)
    {
        if (_context == null) return true;

        var transfer = await _context.FundTransfers.FindAsync(id);
        if (transfer == null) return false;

        transfer.Status = "Approved";
        transfer.ApprovedBy = approvedBy;
        transfer.ApprovedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<FundTransfer> GetMockData()
    {
        return new List<FundTransfer>
        {
            new() { TransferId = 1, TransferNumber = "TRF-20241115-001", FromAccountId = 1, ToAccountId = 2, Amount = 50000.00m, Fee = 25.00m, Status = "Pending", Description = "Monthly allocation", CreatedAt = DateTime.Now.AddHours(-2) },
            new() { TransferId = 2, TransferNumber = "TRF-20241115-002", FromAccountId = 2, ToAccountId = 3, Amount = 100000.00m, Fee = 50.00m, Status = "Approved", Description = "Investment transfer", CreatedAt = DateTime.Now.AddHours(-1), ApprovedBy = "admin", ApprovedAt = DateTime.Now.AddMinutes(-30) },
            new() { TransferId = 3, TransferNumber = "TRF-20241115-003", FromAccountId = 3, ToAccountId = 1, Amount = 25000.00m, Fee = 15.00m, Status = "Processing", Description = "Operational funding", CreatedAt = DateTime.Now.AddMinutes(-30) }
        };
    }
}

// =============================================
// ACCOUNTING MODULE CRUD SERVICES
// =============================================

public class JournalEntryService
{
    private readonly BFASDbContext? _context;
    
    public JournalEntryService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<JournalEntry>> GetAllAsync()
    {
        if (_context == null) return GetMockData();
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .OrderByDescending(je => je.CreatedAt)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetByIdAsync(int id)
    {
        if (_context == null) return GetMockData().FirstOrDefault(x => x.JournalId == id);
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.JournalId == id);
    }

    public async Task<JournalEntry> CreateAsync(JournalEntry journalEntry)
    {
        if (_context == null)
        {
            journalEntry.JournalId = GetMockData().Count + 1;
            journalEntry.JournalNumber = $"JE-{DateTime.Now:yyyyMMdd}-{journalEntry.JournalId:000}";
            return journalEntry;
        }

        journalEntry.JournalNumber = $"JE-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        journalEntry.CreatedAt = DateTime.Now;
        _context.JournalEntries.Add(journalEntry);
        await _context.SaveChangesAsync();
        return journalEntry;
    }

    public async Task<JournalEntry> UpdateAsync(JournalEntry journalEntry)
    {
        if (_context == null) return journalEntry;

        _context.JournalEntries.Update(journalEntry);
        await _context.SaveChangesAsync();
        return journalEntry;
    }

    public async Task<bool> PostAsync(int id, string postedBy)
    {
        if (_context == null) return true;

        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null) return false;

        entry.Status = "Posted";
        entry.PostedBy = postedBy;
        entry.PostedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<JournalEntry> GetMockData()
    {
        return new List<JournalEntry>
        {
            new() { JournalId = 1, JournalNumber = "JE-20241115-001", TransactionDate = DateTime.Today, Description = "Monthly expense accrual", Reference = "REF-001", TotalDebit = 50000.00m, TotalCredit = 50000.00m, Status = "Draft", CreatedAt = DateTime.Now.AddHours(-3), CreatedBy = "accountant" },
            new() { JournalId = 2, JournalNumber = "JE-20241115-002", TransactionDate = DateTime.Today, Description = "Revenue recognition", Reference = "REF-002", TotalDebit = 75000.00m, TotalCredit = 75000.00m, Status = "Posted", CreatedAt = DateTime.Now.AddHours(-2), CreatedBy = "accountant", PostedBy = "admin", PostedAt = DateTime.Now.AddHours(-1) }
        };
    }
}

// =============================================
// FINANCE MODULE CRUD SERVICES
// =============================================

public class BudgetManagementService
{
    private readonly BFASDbContext? _context;
    
    public BudgetManagementService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<Budget>> GetAllAsync()
    {
        if (_context == null) return GetMockData();
        return await _context.Budgets
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        if (_context == null) return GetMockData().FirstOrDefault(x => x.BudgetId == id);
        return await _context.Budgets.FindAsync(id);
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        if (_context == null)
        {
            budget.BudgetId = GetMockData().Count + 1;
            budget.RemainingAmount = budget.AllocatedAmount - budget.SpentAmount;
            return budget;
        }

        budget.CreatedAt = DateTime.Now;
        budget.RemainingAmount = budget.AllocatedAmount - budget.SpentAmount;
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateAsync(Budget budget)
    {
        if (_context == null) return budget;

        budget.RemainingAmount = budget.AllocatedAmount - budget.SpentAmount;
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (_context == null) return true;

        var budget = await _context.Budgets.FindAsync(id);
        if (budget == null) return false;

        budget.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    private List<Budget> GetMockData()
    {
        return new List<Budget>
        {
            new() { BudgetId = 1, BudgetName = "IT Department Q4 2024", Department = "IT Department", Category = "Operations", AllocatedAmount = 500000.00m, SpentAmount = 275000.00m, RemainingAmount = 225000.00m, StartDate = new DateTime(2024, 10, 1), EndDate = new DateTime(2024, 12, 31), Status = "Active", CreatedAt = DateTime.Now.AddDays(-45), CreatedBy = "fmanager" },
            new() { BudgetId = 2, BudgetName = "Marketing Campaign 2024", Department = "Marketing", Category = "Advertising", AllocatedAmount = 750000.00m, SpentAmount = 600000.00m, RemainingAmount = 150000.00m, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 12, 31), Status = "Active", CreatedAt = DateTime.Now.AddDays(-90), CreatedBy = "fmanager" },
            new() { BudgetId = 3, BudgetName = "Training & Development", Department = "HR", Category = "Training", AllocatedAmount = 300000.00m, SpentAmount = 125000.00m, RemainingAmount = 175000.00m, StartDate = new DateTime(2024, 6, 1), EndDate = new DateTime(2025, 5, 31), Status = "Active", CreatedAt = DateTime.Now.AddDays(-60), CreatedBy = "fmanager" }
        };
    }
}

// =============================================
// CUSTOMER PORTAL CRUD SERVICES
// =============================================

public class CustomerAccountService
{
    private readonly BFASDbContext? _context;
    
    public CustomerAccountService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<CustomerAccount>> GetAllAsync()
    {
        if (_context == null) return GetMockData();
        return await _context.CustomerAccounts
            .Include(ca => ca.Customer)
            .Where(ca => ca.IsActive)
            .ToListAsync();
    }

    public async Task<List<CustomerAccount>> GetByCustomerIdAsync(int customerId)
    {
        if (_context == null) return GetMockData().Where(x => x.CustomerId == customerId).ToList();
        return await _context.CustomerAccounts
            .Where(ca => ca.CustomerId == customerId && ca.IsActive)
            .ToListAsync();
    }

    public async Task<CustomerAccount?> GetByIdAsync(int id)
    {
        if (_context == null) return GetMockData().FirstOrDefault(x => x.AccountId == id);
        return await _context.CustomerAccounts
            .Include(ca => ca.Customer)
            .FirstOrDefaultAsync(ca => ca.AccountId == id);
    }

    public async Task<CustomerAccount> CreateAsync(CustomerAccount account)
    {
        if (_context == null)
        {
            account.AccountId = GetMockData().Count + 1;
            account.AccountNumber = $"CUST-{DateTime.Now:yyyyMMdd}-{account.AccountId:000}";
            return account;
        }

        account.AccountNumber = $"CUST-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        account.CreatedAt = DateTime.Now;
        account.AvailableBalance = account.Balance;
        _context.CustomerAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> UpdateBalanceAsync(int accountId, decimal amount, string transactionType)
    {
        if (_context == null) return true;

        var account = await _context.CustomerAccounts.FindAsync(accountId);
        if (account == null) return false;

        if (transactionType == "Deposit")
        {
            account.Balance += amount;
            account.AvailableBalance += amount;
        }
        else if (transactionType == "Withdrawal" || transactionType == "Transfer")
        {
            if (account.AvailableBalance < amount) return false;
            account.Balance -= amount;
            account.AvailableBalance -= amount;
        }

        account.LastTransactionAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<CustomerAccount> GetMockData()
    {
        return new List<CustomerAccount>
        {
            new() { AccountId = 1, CustomerId = 4, AccountNumber = "ACC-123456", Balance = 75000.00m, AvailableBalance = 75000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-30), LastTransactionAt = DateTime.Now.AddHours(-2) },
            new() { AccountId = 2, CustomerId = 4, AccountNumber = "ACC-654321", Balance = 25000.00m, AvailableBalance = 25000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-15), LastTransactionAt = DateTime.Now.AddDays(-1) }
        };
    }
}

public class CustomerTransactionService
{
    private readonly BFASDbContext? _context;
    
    public CustomerTransactionService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<CustomerTransaction>> GetByAccountIdAsync(int accountId)
    {
        if (_context == null) return GetMockData().Where(x => x.AccountId == accountId).ToList();
        return await _context.CustomerTransactions
            .Include(ct => ct.Account)
            .Where(ct => ct.AccountId == accountId)
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomerTransaction> CreateAsync(CustomerTransaction transaction)
    {
        if (_context == null)
        {
            transaction.TransactionId = GetMockData().Count + 1;
            transaction.TransactionNumber = $"TXN-{DateTime.Now:yyyyMMdd}-{transaction.TransactionId:000}";
            return transaction;
        }

        transaction.TransactionNumber = $"TXN-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        transaction.CreatedAt = DateTime.Now;
        _context.CustomerTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> ProcessAsync(int transactionId, string processedBy)
    {
        if (_context == null) return true;

        var transaction = await _context.CustomerTransactions.FindAsync(transactionId);
        if (transaction == null) return false;

        transaction.Status = "Completed";
        transaction.ProcessedAt = DateTime.Now;
        transaction.ProcessedBy = processedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<CustomerTransaction> GetMockData()
    {
        return new List<CustomerTransaction>
        {
            new() { TransactionId = 1, TransactionNumber = "TXN-20241115-001", AccountId = 1, TransactionType = "Deposit", Amount = 10000.00m, Fee = 0.00m, Status = "Completed", Description = "Cash deposit", CreatedAt = DateTime.Now.AddHours(-2), ProcessedAt = DateTime.Now.AddHours(-2), ProcessedBy = "system" },
            new() { TransactionId = 2, TransactionNumber = "TXN-20241115-002", AccountId = 1, TransactionType = "Transfer", Amount = 5000.00m, Fee = 25.00m, Status = "Completed", Description = "Fund transfer", ToAccountId = 2, ToAccountName = "Checking Account", CreatedAt = DateTime.Now.AddHours(-1), ProcessedAt = DateTime.Now.AddHours(-1), ProcessedBy = "system" },
            new() { TransactionId = 3, TransactionNumber = "TXN-20241115-003", AccountId = 2, TransactionType = "Bills", Amount = 2500.00m, Fee = 15.00m, Status = "Pending", Description = "Electricity bill payment", BillerName = "Meralco", BillerAccountNumber = "1234567890", CreatedAt = DateTime.Now.AddMinutes(-30) }
        };
    }
}
