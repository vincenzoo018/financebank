using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services;

// =============================================
// BANKING MODULE CRUD SERVICES
// =============================================

public class BankAccountService
    {
        private readonly BFASDbContext _context;
        
        public BankAccountService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<BankAccount>> GetAllAsync()
    {
        return await _context.BankAccounts.ToListAsync();
    }

    public async Task<BankAccount?> GetByIdAsync(int id)
    {
        return await _context.BankAccounts.FindAsync(id);
    }

    public async Task<BankAccount> CreateAsync(BankAccount bankAccount)
    {
        

        bankAccount.CreatedAt = DateTime.Now;
        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();
        return bankAccount;
    }

    public async Task<BankAccount> UpdateAsync(BankAccount bankAccount)
    {
        bankAccount.LastModifiedAt = DateTime.Now;
        _context.BankAccounts.Update(bankAccount);
        await _context.SaveChangesAsync();
        return bankAccount;
    }

    public async Task<bool> DeleteAsync(int id)
    {
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
        private readonly BFASDbContext _context;
        
        public FundTransferService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<FundTransfer>> GetAllAsync()
    {
        return await _context.FundTransfers
            .Include(ft => ft.FromAccount)
            .Include(ft => ft.ToAccount)
            .OrderByDescending(ft => ft.CreatedAt)
            .ToListAsync();
    }

    public async Task<FundTransfer?> GetByIdAsync(int id)
    {
        return await _context.FundTransfers
            .Include(ft => ft.FromAccount)
            .Include(ft => ft.ToAccount)
            .FirstOrDefaultAsync(ft => ft.TransferId == id);
    }

    public async Task<FundTransfer> CreateAsync(FundTransfer transfer)
    {
        transfer.TransferNumber = $"TRF-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        transfer.CreatedAt = DateTime.Now;
        _context.FundTransfers.Add(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<FundTransfer> UpdateAsync(FundTransfer transfer)
    {
        _context.FundTransfers.Update(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<bool> ApproveAsync(int id, string approvedBy)
    {
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

public class BillerService
    {
        private readonly BFASDbContext _context;
        
        public BillerService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<Biller>> GetAllAsync()
    {
        return await _context.Billers
            .OrderBy(b => b.Category)
            .ThenBy(b => b.BillerName)
            .ToListAsync();
    }

    public async Task<Biller?> GetByIdAsync(int id)
    {
        return await _context.Billers.FindAsync(id);
    }

    public async Task<Biller> CreateAsync(Biller biller)
    {
        

        biller.CreatedAt = DateTime.Now;
        _context.Billers.Add(biller);
        await _context.SaveChangesAsync();
        return biller;
    }

    public async Task<Biller> UpdateAsync(Biller biller)
    {
        _context.Billers.Update(biller);
        await _context.SaveChangesAsync();
        return biller;
    }

    public async Task<bool> ToggleActiveAsync(int id, bool isActive)
    {
        var biller = await _context.Billers.FindAsync(id);
        if (biller == null) return false;

        biller.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    private List<Biller> GetMockData()
    {
        return new List<Biller>
        {
            new() { BillerId = 1, BillerName = "Meralco", BillerCode = "MERALCO", Category = "Utilities", Description = "Electricity provider", ProcessingFee = 10.00m, IsActive = true, CreatedAt = DateTime.Now.AddMonths(-6) },
            new() { BillerId = 2, BillerName = "PLDT", BillerCode = "PLDT", Category = "Telecommunications", Description = "Landline and internet", ProcessingFee = 15.00m, IsActive = true, CreatedAt = DateTime.Now.AddMonths(-4) },
            new() { BillerId = 3, BillerName = "SSS", BillerCode = "SSS", Category = "Government", Description = "Social Security System", ProcessingFee = 5.00m, IsActive = true, CreatedAt = DateTime.Now.AddMonths(-2) }
        };
    }
}

public class BankingReportService
    {
        private readonly BFASDbContext _context;
        
        public BankingReportService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<BankingReport>> GetAllAsync()
    {
        return await _context.BankingReports
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();
    }

    public async Task<BankingReport?> GetByIdAsync(int id)
    {
        return await _context.BankingReports.FindAsync(id);
    }

    /// <summary>
    /// Generates a banking summary report for the given period using real transactional data
    /// and saves it into the BankingReports table.
    /// </summary>
    public async Task<BankingReport> GenerateSummaryAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
    {
        // Normalize dates to inclusive day range
        var startDate = periodStart.Date;
        var endDate = periodEnd.Date;

                var inclusiveEnd = endDate.AddDays(1).AddTicks(-1);

        // Customer transactions (deposits, withdrawals, bills)
        var totalDeposits = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Deposit" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var totalWithdrawals = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Withdrawal" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var totalBills = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Bills" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var depositCount = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Deposit" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .CountAsync();

        var withdrawalCount = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Withdrawal" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .CountAsync();

        var billsCount = await _context.CustomerTransactions
            .Where(t => t.TransactionType == "Bills" && t.CreatedAt >= startDate && t.CreatedAt <= inclusiveEnd)
            .CountAsync();

        // Fund transfers between internal bank accounts
        var totalFundTransfers = await _context.FundTransfers
            .Where(ft => ft.CreatedAt >= startDate && ft.CreatedAt <= inclusiveEnd)
            .SumAsync(ft => (decimal?)ft.Amount) ?? 0m;

        var transferCount = await _context.FundTransfers
            .Where(ft => ft.CreatedAt >= startDate && ft.CreatedAt <= inclusiveEnd)
            .CountAsync();

        // New customer accounts opened in the period
        var newAccounts = await _context.CustomerAccounts
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= inclusiveEnd)
            .CountAsync();

        // Current total balances in customer accounts and bank operating accounts
        var totalCustomerBalances = await _context.CustomerAccounts
            .SumAsync(a => (decimal?)a.Balance) ?? 0m;

        var totalBankBalances = await _context.BankAccounts
            .SumAsync(a => (decimal?)a.Balance) ?? 0m;

        var netCustomerFlow = totalDeposits - (totalWithdrawals + totalBills);

        var lines = new List<string>
        {
            $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            $"TotalDeposits={totalDeposits:N2}",
            $"TotalWithdrawals={totalWithdrawals:N2}",
            $"TotalBillsPayments={totalBills:N2}",
            $"NetCustomerCashFlow={netCustomerFlow:N2}",
            $"DepositCount={depositCount}",
            $"WithdrawalCount={withdrawalCount}",
            $"BillsPaymentCount={billsCount}",
            $"FundTransferTotal={totalFundTransfers:N2}",
            $"FundTransferCount={transferCount}",
            $"NewCustomerAccounts={newAccounts}",
            $"TotalCustomerBalances={totalCustomerBalances:N2}",
            $"TotalBankOperatingBalances={totalBankBalances:N2}"
        };

        var report = new BankingReport
        {
            ReportType = "BankingSummary",
            ReportName = "Banking Summary Report",
            PeriodStart = startDate,
            PeriodEnd = endDate,
            GeneratedAt = DateTime.Now,
            GeneratedBy = string.IsNullOrWhiteSpace(generatedBy) ? "system" : generatedBy,
            Data = string.Join("\n", lines)
        };

        _context.BankingReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }
}

public class LoanManagementService
    {
        private readonly BFASDbContext _context;
        
        public LoanManagementService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<LoanManagementEntity>> GetAllAsync()
    {
        return await _context.LoanManagementRecords
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<LoanManagementEntity?> GetByIdAsync(int id)
    {
        return await _context.LoanManagementRecords.FindAsync(id);
    }

    public async Task<LoanManagementEntity?> ApproveAsync(int loanMgmtId, string processedBy, string? notes)
    {
        var entity = await _context.LoanManagementRecords.FindAsync(loanMgmtId);
        if (entity == null) return null;

        entity.ApprovalStatus = "Approved";
        entity.Status = string.IsNullOrWhiteSpace(entity.Status) ? "Active" : entity.Status;
        entity.ApprovedBy = processedBy;
        entity.ApprovedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            entity.ProcessingNotes = notes;
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<LoanManagementEntity?> RejectAsync(int loanMgmtId, string processedBy, string rejectionReason, string? notes)
    {
        var entity = await _context.LoanManagementRecords.FindAsync(loanMgmtId);
        if (entity == null) return null;

        entity.ApprovalStatus = "Rejected";
        entity.Status = "Rejected";
        entity.ApprovedBy = processedBy;
        entity.ApprovedAt = DateTime.Now;
        entity.RejectionReason = rejectionReason;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            entity.ProcessingNotes = notes;
        }

        await _context.SaveChangesAsync();
        return entity;
    }
}

public class CardManagementService
    {
        private readonly BFASDbContext _context;
        
        public CardManagementService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<CardManagementEntity>> GetAllAsync()
    {
        return await _context.CardManagementRecords
            .OrderByDescending(c => c.ActionAt)
            .ToListAsync();
    }

    public async Task<CardManagementEntity?> GetByIdAsync(int id)
    {
        return await _context.CardManagementRecords.FindAsync(id);
    }

    public async Task<CardManagementEntity> RecordActionAsync(int customerCardId, string action, string actionBy, string? reason)
    {
        var entity = new CardManagementEntity
        {
            CustomerCardId = customerCardId,
            Action = action,
            ActionBy = actionBy,
            Reason = reason,
            ActionAt = DateTime.Now
        };

        _context.CardManagementRecords.Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }
}

// =============================================
// ACCOUNTING MODULE CRUD SERVICES
// =============================================

public class AccountsPayableService
    {
        private readonly BFASDbContext _context;
        
        public AccountsPayableService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<AccountsPayable>> GetAllAsync()
    {
        return await _context.AccountsPayables
            .OrderByDescending(p => p.InvoiceDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<AccountsPayable?> GetByIdAsync(int id)
    {
        return await _context.AccountsPayables.FindAsync(id);
    }

    public async Task<AccountsPayable> CreateAsync(AccountsPayable payable)
    {
        payable.CreatedAt = DateTime.Now;
        payable.OutstandingAmount = payable.Amount - payable.PaidAmount;
        payable.Status = payable.OutstandingAmount <= 0 ? "Paid" : "Pending";

        _context.AccountsPayables.Add(payable);
        await _context.SaveChangesAsync();
        return payable;
    }

    public async Task<AccountsPayable> UpdateAsync(AccountsPayable payable)
    {
        payable.OutstandingAmount = payable.Amount - payable.PaidAmount;
        if (payable.OutstandingAmount <= 0)
            payable.Status = "Paid";
        else if (payable.PaidAmount > 0)
            payable.Status = "Partially Paid";

        _context.AccountsPayables.Update(payable);
        await _context.SaveChangesAsync();
        return payable;
    }

    public async Task<AccountsPayable?> RecordPaymentAsync(int payableId, decimal paymentAmount, string processedBy)
    {
        var payable = await _context.AccountsPayables.FindAsync(payableId);
        if (payable == null) return null;

        payable.PaidAmount += paymentAmount;
        payable.OutstandingAmount = payable.Amount - payable.PaidAmount;

        if (payable.OutstandingAmount <= 0)
            payable.Status = "Paid";
        else
            payable.Status = "Partially Paid";

        await _context.SaveChangesAsync();
        return payable;
    }
}

public class AccountsReceivableService
    {
        private readonly BFASDbContext _context;
        
        public AccountsReceivableService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<AccountsReceivable>> GetAllAsync()
    {
        return await _context.AccountsReceivables
            .OrderByDescending(r => r.InvoiceDate)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<AccountsReceivable?> GetByIdAsync(int id)
    {
        return await _context.AccountsReceivables.FindAsync(id);
    }

    public async Task<AccountsReceivable> CreateAsync(AccountsReceivable receivable)
    {
        receivable.CreatedAt = DateTime.Now;
        receivable.OutstandingAmount = receivable.Amount - receivable.ReceivedAmount;
        receivable.Status = receivable.OutstandingAmount <= 0 ? "Paid" : "Pending";

        _context.AccountsReceivables.Add(receivable);
        await _context.SaveChangesAsync();
        return receivable;
    }

    public async Task<AccountsReceivable> UpdateAsync(AccountsReceivable receivable)
    {
        receivable.OutstandingAmount = receivable.Amount - receivable.ReceivedAmount;
        if (receivable.OutstandingAmount <= 0)
            receivable.Status = "Paid";
        else if (receivable.ReceivedAmount > 0)
            receivable.Status = "Partially Paid";

        _context.AccountsReceivables.Update(receivable);
        await _context.SaveChangesAsync();
        return receivable;
    }

    public async Task<AccountsReceivable?> RecordReceiptAsync(int receivableId, decimal receiptAmount, string processedBy)
    {
        var receivable = await _context.AccountsReceivables.FindAsync(receivableId);
        if (receivable == null) return null;

        receivable.ReceivedAmount += receiptAmount;
        receivable.OutstandingAmount = receivable.Amount - receivable.ReceivedAmount;

        if (receivable.OutstandingAmount <= 0)
            receivable.Status = "Paid";
        else
            receivable.Status = "Partially Paid";

        await _context.SaveChangesAsync();
        return receivable;
    }
}

public class JournalEntryService
    {
        private readonly BFASDbContext _context;
        
        public JournalEntryService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<JournalEntry>> GetAllAsync()
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .OrderByDescending(je => je.CreatedAt)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetByIdAsync(int id)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.JournalId == id);
    }

    public async Task<JournalEntry> CreateAsync(JournalEntry journalEntry)
    {
                journalEntry.JournalNumber = $"JE-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        journalEntry.CreatedAt = DateTime.Now;
        _context.JournalEntries.Add(journalEntry);
        await _context.SaveChangesAsync();
        return journalEntry;
    }

    public async Task<JournalEntry> UpdateAsync(JournalEntry journalEntry)
    {
        _context.JournalEntries.Update(journalEntry);
        await _context.SaveChangesAsync();
        return journalEntry;
    }

    public async Task<bool> PostAsync(int id, string postedBy)
    {
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
// GENERAL LEDGER & TRIAL BALANCE SERVICES
// =============================================

public class GeneralLedgerService
    {
        private readonly BFASDbContext _context;
        
        public GeneralLedgerService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<FinanceBank.Models.GeneralLedgerEntry>> GetAllAsync()
    {
        return await _context.GeneralLedgerEntries
            .OrderByDescending(g => g.TransactionDate)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<FinanceBank.Models.GeneralLedgerEntry?> GetByIdAsync(int id)
    {
        return await _context.GeneralLedgerEntries.FindAsync(id);
    }

    public async Task<FinanceBank.Models.GeneralLedgerEntry> CreateAsync(FinanceBank.Models.GeneralLedgerEntry entry)
    {
        entry.EntryNumber = $"GL-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
        entry.CreatedAt = DateTime.Now;
        _context.GeneralLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }
}

public class TrialBalanceService
    {
        private readonly BFASDbContext _context;
        
        public TrialBalanceService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<FinanceBank.Models.TrialBalanceEntry>> GetAllAsync()
    {
        return await _context.TrialBalances
            .OrderBy(t => t.AccountCode)
            .ToListAsync();
    }

    public async Task<FinanceBank.Models.TrialBalanceEntry?> GetByIdAsync(int id)
    {
        return await _context.TrialBalances.FindAsync(id);
    }

    public async Task<FinanceBank.Models.TrialBalanceEntry> CreateAsync(FinanceBank.Models.TrialBalanceEntry entry)
    {
        entry.CreatedAt = DateTime.Now;
        _context.TrialBalances.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<(decimal TotalDebit, decimal TotalCredit)> GetTotalsAsync()
    {
        var entries = await _context.TrialBalances.ToListAsync();
        return (entries.Sum(t => t.DebitBalance), entries.Sum(t => t.CreditBalance));
    }
}

public class FinancialStatementService
    {
        private readonly BFASDbContext _context;
        
        public FinancialStatementService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<FinanceBank.Models.FinancialStatement>> GetAllAsync()
    {
        return await _context.FinancialStatements
            .OrderByDescending(f => f.GeneratedAt)
            .ToListAsync();
    }

    public async Task<FinanceBank.Models.FinancialStatement?> GetByIdAsync(int id)
    {
        return await _context.FinancialStatements.FindAsync(id);
    }

    public async Task<FinanceBank.Models.FinancialStatement> CreateAsync(FinanceBank.Models.FinancialStatement statement)
    {
        statement.GeneratedAt = DateTime.Now;
        _context.FinancialStatements.Add(statement);
        await _context.SaveChangesAsync();
        return statement;
    }

    public async Task<List<FinanceBank.Models.FinancialStatement>> GetByTypeAsync(string statementType)
    {
        return await _context.FinancialStatements
            .Where(f => f.StatementType == statementType)
            .OrderByDescending(f => f.GeneratedAt)
            .ToListAsync();
    }
}

// =============================================
// FINANCE MODULE CRUD SERVICES
// =============================================

public class BudgetManagementService
    {
        private readonly BFASDbContext _context;
        
        public BudgetManagementService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<Budget>> GetAllAsync()
    {
        return await _context.Budgets
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        return await _context.Budgets.FindAsync(id);
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        

        budget.CreatedAt = DateTime.Now;
        budget.RemainingAmount = budget.AllocatedAmount - budget.SpentAmount;
        _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateAsync(Budget budget)
    {
        budget.RemainingAmount = budget.AllocatedAmount - budget.SpentAmount;
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<bool> DeleteAsync(int id)
    {
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

public class CashflowAnalysisService
    {
        private readonly BFASDbContext _context;
        
        public CashflowAnalysisService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<CashflowEntry>> GetAllAsync()
    {
        return await _context.CashflowEntries
            .OrderByDescending(e => e.TransactionDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<CashflowEntry?> GetByIdAsync(int id)
    {
        return await _context.CashflowEntries.FindAsync(id);
    }

    public async Task<CashflowEntry> CreateAsync(CashflowEntry entry)
    {
        

        entry.CreatedAt = DateTime.Now;
        _context.CashflowEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<CashflowEntry> UpdateAsync(CashflowEntry entry)
    {
        _context.CashflowEntries.Update(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.CashflowEntries.FindAsync(id);
        if (entry == null) return false;
        _context.CashflowEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    private List<CashflowEntry> GetMockData()
    {
        var today = DateTime.Today;
        return new List<CashflowEntry>
        {
            new() { CashflowId = 1, EntryType = "Inflow", Category = "Customer Deposits", Amount = 150000.00m, Description = "Daily branch deposits", TransactionDate = today.AddDays(-1), CreatedAt = DateTime.Now.AddDays(-1), CreatedBy = "teller" },
            new() { CashflowId = 2, EntryType = "Outflow", Category = "Loan Disbursements", Amount = 75000.00m, Description = "Approved loan releases", TransactionDate = today.AddDays(-1), CreatedAt = DateTime.Now.AddDays(-1), CreatedBy = "admin" },
            new() { CashflowId = 3, EntryType = "Inflow", Category = "Interest Income", Amount = 25000.00m, Description = "Monthly interest collections", TransactionDate = today.AddDays(-7), CreatedAt = DateTime.Now.AddDays(-7), CreatedBy = "system" }
        };
    }
}

public class FinancialForecastService
    {
        private readonly BFASDbContext _context;
        
        public FinancialForecastService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<FinancialForecast>> GetAllAsync()
    {
        return await _context.FinancialForecasts
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FinancialForecast?> GetByIdAsync(int id)
    {
        return await _context.FinancialForecasts.FindAsync(id);
    }

    public async Task<FinancialForecast> CreateAsync(FinancialForecast forecast)
    {
        

        forecast.CreatedAt = DateTime.Now;
        forecast.Variance = (forecast.ActualAmount) - forecast.ProjectedAmount;
        _context.FinancialForecasts.Add(forecast);
        await _context.SaveChangesAsync();
        return forecast;
    }

    public async Task<FinancialForecast> UpdateAsync(FinancialForecast forecast)
    {
        forecast.Variance = (forecast.ActualAmount) - forecast.ProjectedAmount;
        _context.FinancialForecasts.Update(forecast);
        await _context.SaveChangesAsync();
        return forecast;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var forecast = await _context.FinancialForecasts.FindAsync(id);
        if (forecast == null) return false;
        _context.FinancialForecasts.Remove(forecast);
        await _context.SaveChangesAsync();
        return true;
    }

    private List<FinancialForecast> GetMockData()
    {
        var today = DateTime.Today;
        var start = new DateTime(today.Year, 1, 1);
        var end = new DateTime(today.Year, 12, 31);

        return new List<FinancialForecast>
        {
            new() { ForecastId = 1, ForecastName = "Annual Revenue Forecast", ForecastType = "Revenue", PeriodStart = start, PeriodEnd = end, ProjectedAmount = 5000000.00m, ActualAmount = 0, Variance = -5000000.00m, Assumptions = "Based on historical growth and new product launches", CreatedAt = DateTime.Now.AddDays(-30), CreatedBy = "fmanager" },
            new() { ForecastId = 2, ForecastName = "Operating Expenses Forecast", ForecastType = "Expense", PeriodStart = start, PeriodEnd = end, ProjectedAmount = 3200000.00m, ActualAmount = 0, Variance = -3200000.00m, Assumptions = "Includes salary adjustments and new hires", CreatedAt = DateTime.Now.AddDays(-25), CreatedBy = "accountant" }
        };
    }
}

// =============================================
// CUSTOMER PORTAL CRUD SERVICES
// =============================================

public class CustomerAccountService
    {
        private readonly BFASDbContext _context;
        
        public CustomerAccountService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<CustomerAccount>> GetAllAsync()
    {
        return await _context.CustomerAccounts
            .Include(ca => ca.Customer)
            .Where(ca => ca.IsActive)
            .ToListAsync();
    }

    public async Task<List<CustomerAccount>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.CustomerAccounts
            .Where(ca => ca.CustomerId == customerId && ca.IsActive)
            .ToListAsync();
    }

    public async Task<CustomerAccount?> GetByIdAsync(int id)
    {
        return await _context.CustomerAccounts
            .Include(ca => ca.Customer)
            .FirstOrDefaultAsync(ca => ca.AccountId == id);
    }

    public async Task<CustomerAccount> CreateAsync(CustomerAccount account)
    {
                account.AccountNumber = $"CUST-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        account.CreatedAt = DateTime.Now;
        account.AvailableBalance = account.Balance;
        _context.CustomerAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> UpdateBalanceAsync(int accountId, decimal amount, string transactionType)
    {
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
        private readonly BFASDbContext _context;
        
        public CustomerTransactionService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<CustomerTransaction>> GetByAccountIdAsync(int accountId)
    {
        return await _context.CustomerTransactions
            .Include(ct => ct.Account)
            .Where(ct => ct.AccountId == accountId)
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomerTransaction> CreateAsync(CustomerTransaction transaction)
    {
                transaction.TransactionNumber = $"TXN-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        transaction.CreatedAt = DateTime.Now;
        _context.CustomerTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> ProcessAsync(int transactionId, string processedBy)
    {
        var transaction = await _context.CustomerTransactions.FindAsync(transactionId);
        if (transaction == null) return false;

        transaction.Status = "Completed";
        transaction.ProcessedAt = DateTime.Now;
        // Note: ProcessedBy has been replaced with ProcessedByEmployeeId (FK to Employees)
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CustomerTransaction>> GetDepositsByAccountIdAsync(int accountId)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId && ct.TransactionType == "Deposit")
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetWithdrawalsByAccountIdAsync(int accountId)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId && ct.TransactionType == "Withdrawal")
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetBillsByAccountIdAsync(int accountId)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId && ct.TransactionType == "Bills")
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetByTypeAsync(int accountId, string transactionType)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId && ct.TransactionType == transactionType)
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomerTransaction?> GetByIdAsync(int transactionId)
    {
        return await _context.CustomerTransactions.FirstOrDefaultAsync(ct => ct.TransactionId == transactionId);
    }

    public async Task<List<CustomerTransaction>> GetRecentAsync(int accountId, int limit = 10)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId)
            .OrderByDescending(ct => ct.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.AccountId == accountId && ct.CreatedAt >= startDate && ct.CreatedAt <= endDate)
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    private List<CustomerTransaction> GetMockData()
    {
        return new List<CustomerTransaction>
        {
            new() { TransactionId = 1, TransactionNumber = "TXN-20241115-001", AccountId = 1, TransactionType = "Deposit", Amount = 10000.00m, Fee = 0.00m, Status = "Completed", Description = "Cash deposit", CreatedAt = DateTime.Now.AddHours(-2), ProcessedAt = DateTime.Now.AddHours(-2), ProcessedByEmployeeId = 1 },
            new() { TransactionId = 2, TransactionNumber = "TXN-20241115-002", AccountId = 1, TransactionType = "Transfer", Amount = 5000.00m, Fee = 25.00m, Status = "Completed", Description = "Fund transfer", ToAccountId = 2, ToAccountName = "Checking Account", CreatedAt = DateTime.Now.AddHours(-1), ProcessedAt = DateTime.Now.AddHours(-1), ProcessedByEmployeeId = 1 },
            new() { TransactionId = 3, TransactionNumber = "TXN-20241115-003", AccountId = 2, TransactionType = "Bills", Amount = 2500.00m, Fee = 15.00m, Status = "Pending", Description = "Electricity bill payment", BillerName = "Meralco", BillerAccountNumber = "1234567890", CreatedAt = DateTime.Now.AddMinutes(-30), ProcessedByEmployeeId = 1 }
        };
    }
}

public class CardService
    {
        private readonly BFASDbContext _context;
        
        public CardService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<Card>> GetByAccountIdAsync(int accountId)
    {
        // Query from CustomerCardEntity and map to Card
        var cardEntities = await _context.CustomerCards
            .Where(c => c.AccountId == accountId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        // Map CustomerCardEntity to Card
        return cardEntities.Select(ce => new Card
        {
            CardId = ce.CardId,
            CardNumber = ce.CardNumber,
            AccountId = ce.AccountId,
            CardType = ce.CardType,
            CardName = ce.CardName,
            CreditLimit = ce.CreditLimit,
            AvailableCredit = ce.AvailableCredit,
            OutstandingBalance = ce.OutstandingBalance,
            ExpiryDate = ce.ExpiryDate ?? DateTime.Now.AddYears(5),
            DueDate = ce.DueDate,
            InterestRate = ce.InterestRate,
            IsActive = ce.IsActive,
            IsLocked = ce.IsLocked,
            CreatedAt = ce.CreatedAt
        }).ToList();
    }

    public async Task<Card?> GetByIdAsync(int cardId)
    {
        var cardEntity = await _context.CustomerCards
            .FirstOrDefaultAsync(c => c.CardId == cardId);

        if (cardEntity == null) return null;

        return new Card
        {
            CardId = cardEntity.CardId,
            CardNumber = cardEntity.CardNumber,
            AccountId = cardEntity.AccountId,
            CardType = cardEntity.CardType,
            CardName = cardEntity.CardName,
            CreditLimit = cardEntity.CreditLimit,
            AvailableCredit = cardEntity.AvailableCredit,
            OutstandingBalance = cardEntity.OutstandingBalance,
            ExpiryDate = cardEntity.ExpiryDate ?? DateTime.Now.AddYears(5),
            DueDate = cardEntity.DueDate,
            InterestRate = cardEntity.InterestRate,
            IsActive = cardEntity.IsActive,
            IsLocked = cardEntity.IsLocked,
            CreatedAt = cardEntity.CreatedAt
        };
    }

    public async Task<Card> CreateAsync(Card card)
    {
        var cardEntity = new CustomerCardEntity
        {
            AccountId = card.AccountId,
            CardNumber = card.CardNumber,
            CardType = card.CardType,
            CardName = card.CardName,
            CreditLimit = card.CreditLimit,
            AvailableCredit = card.AvailableCredit,
            OutstandingBalance = card.OutstandingBalance,
            ExpiryDate = card.ExpiryDate,
            DueDate = card.DueDate,
            InterestRate = card.InterestRate,
            IsActive = card.IsActive,
            IsLocked = card.IsLocked,
            CreatedAt = DateTime.Now
        };

        _context.CustomerCards.Add(cardEntity);
        await _context.SaveChangesAsync();
        
        card.CardId = cardEntity.CardId;
        return card;
    }

    public async Task<Card> UpdateAsync(Card card)
    {
        var existing = await _context.CustomerCards
            .FirstOrDefaultAsync(c => c.CardId == card.CardId);

        if (existing != null)
        {
            existing.IsLocked = card.IsLocked;
            existing.OutstandingBalance = card.OutstandingBalance;
            existing.AvailableCredit = card.AvailableCredit;
            await _context.SaveChangesAsync();
        }

        return card;
    }
}

public class LoanService
    {
        private readonly BFASDbContext _context;
        
        public LoanService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<Loan>> GetByAccountIdAsync(int accountId)
    {
        System.Diagnostics.Debug.WriteLine($"[LoanService] Fetching loans for AccountId: {accountId}");
        
        var loans = await _context.Loans
            .Where(l => l.AccountId == accountId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        System.Diagnostics.Debug.WriteLine($"[LoanService] Found {loans.Count} loans");
        return loans;
    }

    public async Task<Loan?> GetByIdAsync(int loanId)
    {
        return await _context.Loans
            .FirstOrDefaultAsync(l => l.LoanId == loanId);
    }

    public async Task<Loan> CreateAsync(Loan loan)
    {
        loan.CreatedAt = DateTime.Now;
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task<Loan> UpdateAsync(Loan loan)
    {
        var existing = await _context.Loans
            .FirstOrDefaultAsync(l => l.LoanId == loan.LoanId);

        if (existing != null)
        {
            existing.OutstandingBalance = loan.OutstandingBalance;
            existing.Status = loan.Status;
            existing.NextDueDate = loan.NextDueDate;
            await _context.SaveChangesAsync();
        }

        return loan;
    }
}

public class SavingsGoalService
    {
        private readonly BFASDbContext _context;
        
        public SavingsGoalService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<SavingsGoalEntity>> GetByAccountIdAsync(int accountId)
    {
        return await _context.CustomerAccounts
            .Where(ca => ca.AccountId == accountId)
            .SelectMany(ca => ca.SavingsGoals ?? new List<SavingsGoalEntity>())
            .OrderByDescending(sg => sg.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavingsGoalEntity?> GetByIdAsync(int goalId)
    {
        return await _context.CustomerAccounts
            .SelectMany(ca => ca.SavingsGoals ?? new List<SavingsGoalEntity>())
            .FirstOrDefaultAsync(sg => sg.GoalId == goalId);
    }

    public async Task<SavingsGoalEntity> CreateAsync(SavingsGoalEntity goal)
    {
        goal.CreatedAt = DateTime.Now;
        _context.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<SavingsGoalEntity> UpdateAsync(SavingsGoalEntity goal)
    {
        _context.Update(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<(bool success, string message)> CreateGoalAsync(
        int accountId,
        string goalName,
        decimal targetAmount,
        DateTime targetDate,
        string category = "Other")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(goalName) || targetAmount <= 0 || targetDate <= DateTime.Today)
                return (false, "Invalid goal data");

            var goal = new SavingsGoalEntity
            {
                AccountId = accountId,
                GoalName = goalName,
                TargetAmount = targetAmount,
                CurrentAmount = 0,
                StartDate = DateTime.Today,
                TargetDate = targetDate,
                Status = "Active",
                Description = category,
                CreatedAt = DateTime.Now
            };

            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();
            
            return (true, "Savings goal created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating savings goal: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }
}

public class RewardPointsService
    {
        private readonly BFASDbContext _context;
        
        public RewardPointsService(BFASDbContext context)
        {
            _context = context;
        }    public async Task<List<RewardPointsEntity>> GetByAccountIdAsync(int accountId)
    {
        return await _context.CustomerAccounts
            .Where(ca => ca.AccountId == accountId)
            .SelectMany(ca => ca.RewardPoints ?? new List<RewardPointsEntity>())
            .OrderByDescending(rp => rp.EarnedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalPointsAsync(int accountId)
    {
        return await _context.CustomerAccounts
            .Where(ca => ca.AccountId == accountId)
            .SelectMany(ca => ca.RewardPoints ?? new List<RewardPointsEntity>())
            .Where(rp => !rp.IsRedeemed)
            .SumAsync(rp => rp.Points);
    }

    public async Task<RewardPointsEntity> AddPointsAsync(int accountId, int points, string earnedFrom, string? description)
    {
        var reward = new RewardPointsEntity
        {
            AccountId = accountId,
            Points = points,
            EarnedFrom = earnedFrom,
            Description = description,
            EarnedAt = DateTime.Now,
            IsRedeemed = false
        };

        _context.Add(reward);
        await _context.SaveChangesAsync();

        return reward;
    }

    public async Task<bool> RedeemPointsAsync(int rewardId)
    {
        var reward = await _context.RewardPoints.FindAsync(rewardId);
        if (reward == null) return false;

        reward.IsRedeemed = true;
        reward.RedeemedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }
}

public class AuditLogService
{
    private readonly BFASDbContext? _context;

    public AuditLogService(BFASDbContext? context = null)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetRecentAsync(int limit = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<AuditLog> CreateAsync(AuditLog log)
    {
        log.CreatedAt = DateTime.Now;
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task ClearAllAsync()
    {
        var logs = await _context.AuditLogs.ToListAsync();
        if (logs.Count == 0) return;

        _context.AuditLogs.RemoveRange(logs);
        await _context.SaveChangesAsync();
    }
}


