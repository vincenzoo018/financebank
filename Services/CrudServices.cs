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
    }
    public async Task<List<BankAccount>> GetAllAsync()
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
            new() { BankAccountId = 1, AccountNumber = "ACC-001", AccountName = "Primary Operating Account", AccountType = "Checking", BankName = "FINEBANK", Balance = 1500000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-30) },
            new() { BankAccountId = 2, AccountNumber = "ACC-002", AccountName = "Savings Account", AccountType = "Savings", BankName = "FINEBANK", Balance = 750000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-15) },
            new() { BankAccountId = 3, AccountNumber = "ACC-003", AccountName = "Investment Account", AccountType = "Investment", BankName = "FINEBANK", Balance = 2250000.00m, Currency = "PHP", IsActive = true, CreatedAt = DateTime.Now.AddDays(-7) }
        };
    }
}

public class FundTransferService
{
    private readonly BFASDbContext _context;

    public FundTransferService(BFASDbContext context)
    {
        _context = context;
    }
    public async Task<List<FundTransfer>> GetAllAsync()
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
    }
    public async Task<List<Biller>> GetAllAsync()
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
    }
    public async Task<List<BankingReport>> GetAllAsync()
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
    }
    public async Task<List<LoanManagementEntity>> GetAllAsync()
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
    }
    public async Task<List<CardManagementEntity>> GetAllAsync()
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
    private readonly AutomaticGLPostingService? _glPostingService;

    public AccountsPayableService(BFASDbContext context, AutomaticGLPostingService? glPostingService = null)
    {
        _context = context;
        _glPostingService = glPostingService;
    }
    public async Task<List<AccountsPayable>> GetAllAsync()
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

        // Post to General Ledger - AP Creation (expense recognition)
        try
        {
            if (_glPostingService != null)
            {
                await _glPostingService.PostAccountsPayableCreationAsync(
                    payable.PayableId,
                    payable.VendorName,
                    payable.InvoiceNumber,
                    payable.Amount,
                    payable.Description ?? "General Expense",
                    payable.InvoiceDate);
            }
        }
        catch
        {
            // GL posting failure should not prevent AP creation
        }

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

        // Post to General Ledger - AP Payment
        try
        {
            if (_glPostingService != null)
            {
                await _glPostingService.PostAccountsPayablePaymentAsync(
                    payableId,
                    payable.VendorName,
                    payable.InvoiceNumber,
                    paymentAmount,
                    DateTime.Now);
            }
        }
        catch
        {
            // GL posting failure should not prevent payment recording
        }

        return payable;
    }

    /// <summary>
    /// Get all FM-approved interest payouts waiting for release by accountant
    /// </summary>
    public async Task<List<AccountsPayable>> GetFMApprovedInterestPayoutsAsync()
    {
        return await _context.AccountsPayables
            .Where(p => p.Status == "Approved" &&
                       p.Description != null &&
                       p.Description.Contains("Savings Interest"))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Release an interest payout (mark as paid)
    /// </summary>
    public async Task ReleaseInterestPayoutAsync(int payableId, string releasedBy)
    {
        var payable = await _context.AccountsPayables.FindAsync(payableId);
        if (payable == null)
            throw new KeyNotFoundException($"Payable {payableId} not found");

        payable.Status = "Paid";
        payable.PaidAmount = payable.Amount;
        payable.OutstandingAmount = 0;
        // Update description to include release info
        payable.Description = $"{payable.Description} | Released by {releasedBy} on {DateTime.Now:yyyy-MM-dd HH:mm}";

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Sync savings interest to accounts payable
    /// </summary>
    public async Task<(int created, int skipped, string message)> SyncSavingsInterestToAPAsync(string? processedBy = null)
    {
        // This is a placeholder implementation
        // In a real scenario, this would calculate interest from savings accounts
        // and create corresponding accounts payable entries
        await Task.CompletedTask; // Placeholder to make it async
        return (0, 0, "Sync completed. No new interest payouts to create.");
    }
}

public class AccountsReceivableService
{
    private readonly BFASDbContext _context;
    private readonly AutomaticGLPostingService? _glPostingService;

    public AccountsReceivableService(BFASDbContext context, AutomaticGLPostingService? glPostingService = null)
    {
        _context = context;
        _glPostingService = glPostingService;
    }
    public async Task<List<AccountsReceivable>> GetAllAsync()
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

        // Post to General Ledger - AR Creation (revenue recognition)
        try
        {
            if (_glPostingService != null)
            {
                await _glPostingService.PostAccountsReceivableCreationAsync(
                    receivable.ReceivableId,
                    receivable.CustomerName,
                    receivable.InvoiceNumber,
                    receivable.Amount,
                    receivable.Description ?? "Service Income",
                    receivable.InvoiceDate);
            }
        }
        catch
        {
            // GL posting failure should not prevent AR creation
        }

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

        // Post to General Ledger - AR Receipt
        try
        {
            if (_glPostingService != null)
            {
                await _glPostingService.PostAccountsReceivableReceiptAsync(
                    receivableId,
                    receivable.CustomerName,
                    receivable.InvoiceNumber,
                    receiptAmount,
                    DateTime.Now);
            }
        }
        catch
        {
            // GL posting failure should not prevent receipt recording
        }

        return receivable;
    }

    /// <summary>
    /// Create AR entry from loan payment
    /// </summary>
    public async Task<AccountsReceivable> CreateFromLoanPaymentAsync(
        string customerName,
        int loanId,
        int scheduleId,
        decimal paymentAmount,
        decimal interestAmount,
        decimal penaltyAmount,
        string createdBy)
    {
        var invoiceNumber = $"LN-{loanId:D6}-{scheduleId:D3}-{DateTime.Now:yyyyMMddHHmmss}";

        var receivable = new AccountsReceivable
        {
            CustomerName = customerName,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(1), // Payment already made, due is immediate
            Amount = paymentAmount,
            ReceivedAmount = paymentAmount, // Already received since this is a completed payment
            OutstandingAmount = 0,
            InterestAmount = interestAmount,
            PenaltyAmount = penaltyAmount,
            TaxAmount = 0,
            Status = "Pending Review", // Needs FM approval
            Description = $"Loan#{loanId} Payment | Schedule #{scheduleId} | Principal: ₱{(paymentAmount - interestAmount - penaltyAmount):N2} | Interest: ₱{interestAmount:N2} | Penalty: ₱{penaltyAmount:N2}",
            CreatedAt = DateTime.Now,
            CreatedBy = createdBy
        };

        _context.AccountsReceivables.Add(receivable);
        await _context.SaveChangesAsync();
        return receivable;
    }

    /// <summary>
    /// Get pending loan payment AR entries
    /// </summary>
    public async Task<List<AccountsReceivable>> GetPendingLoanPaymentsAsync()
    {
        return await _context.AccountsReceivables
            .Where(ar => ar.Status == "Pending Review" &&
                        ar.Description != null &&
                        ar.Description.Contains("Loan#"))
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();
    }
}

public class JournalEntryService
{
    private readonly BFASDbContext _context;

    public JournalEntryService(BFASDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all journal entries including auto-generated ones from real transactions
    /// </summary>
    public async Task<List<JournalEntry>> GetAllAsync()
    {
        // Get manually created journal entries
        var manualEntries = await _context.JournalEntries
            .Include(je => je.Lines)
            .OrderByDescending(je => je.CreatedAt)
            .ToListAsync();

        return manualEntries;
    }

    /// <summary>
    /// Get all journal entries with auto-generated entries from CustomerTransactions, LoanPayments, LoanDisbursals
    /// </summary>
    public async Task<List<JournalEntry>> GetAllWithTransactionsAsync()
    {
        var entries = new List<JournalEntry>();

        try
        {
            // Get manual journal entries
            var manualEntries = await _context.JournalEntries
                .Include(je => je.Lines)
                .OrderByDescending(je => je.CreatedAt)
                .ToListAsync();
            entries.AddRange(manualEntries);

            // Auto-generate journal entries from CustomerTransactions (Deposits, Withdrawals, Transfers)
            var customerTransactions = await _context.CustomerTransactions
                .Include(t => t.Account)
                .ThenInclude(a => a!.Customer)
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            foreach (var txn in customerTransactions)
            {
                try
                {
                    var journalEntry = CreateJournalFromTransaction(txn);
                    if (journalEntry != null)
                        entries.Add(journalEntry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating journal from transaction {txn.TransactionId}: {ex.Message}");
                }
            }

            // Auto-generate journal entries from Loan Payments
            var loanPayments = await _context.LoanPayments
                .Include(p => p.Loan)
                .Include(p => p.PaymentSchedule)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            foreach (var payment in loanPayments)
            {
                try
                {
                    var journalEntry = CreateJournalFromLoanPayment(payment);
                    if (journalEntry != null)
                        entries.Add(journalEntry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating journal from loan payment {payment.PaymentId}: {ex.Message}");
                }
            }

            // Auto-generate journal entries from Loan Disbursals
            var loanDisbursals = await _context.LoanDisbursals
                .Include(d => d.Loan)
                .Where(d => d.DisbursalStatus == "Completed")
                .OrderByDescending(d => d.DisbursalDate)
                .ToListAsync();

            foreach (var disbursal in loanDisbursals)
            {
                try
                {
                    var journalEntry = CreateJournalFromLoanDisbursal(disbursal);
                    if (journalEntry != null)
                        entries.Add(journalEntry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating journal from disbursal {disbursal.DisbursalId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetAllWithTransactionsAsync: {ex.Message}");
            throw; // Re-throw so the caller can see the error
        }

        return entries.OrderByDescending(e => e.TransactionDate).ThenByDescending(e => e.CreatedAt).ToList();
    }

    /// <summary>
    /// Create journal entry from customer transaction (Deposit, Withdrawal, Transfer)
    /// </summary>
    private JournalEntry? CreateJournalFromTransaction(CustomerTransaction txn)
    {
        var customerName = txn.Account?.Customer?.FullName ?? $"Account #{txn.AccountId}";
        var accountNumber = txn.Account?.AccountNumber ?? "";

        var entry = new JournalEntry
        {
            JournalId = -txn.TransactionId, // Negative ID for auto-generated
            JournalNumber = $"AUTO-TXN-{txn.TransactionNumber}",
            TransactionDate = txn.ProcessedAt ?? txn.CreatedAt,
            Description = $"{txn.TransactionType}: {customerName} ({accountNumber}) - {txn.Description}",
            Reference = txn.TransactionNumber,
            Status = "Posted",
            CreatedAt = txn.CreatedAt,
            CreatedBy = "System",
            PostedAt = txn.ProcessedAt,
            PostedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        switch (txn.TransactionType.ToUpper())
        {
            case "DEPOSIT":
                // Debit: Cash on Hand, Credit: Customer Deposits
                entry.TotalDebit = txn.Amount;
                entry.TotalCredit = txn.Amount;
                entry.Lines.Add(new JournalEntryLine { AccountCode = "1010", AccountName = "Cash on Hand", DebitAmount = txn.Amount, CreditAmount = 0, Description = $"Cash received - {customerName}" });
                entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits", DebitAmount = 0, CreditAmount = txn.Amount, Description = $"Deposit liability - {customerName}" });
                break;

            case "WITHDRAWAL":
                // Debit: Customer Deposits, Credit: Cash on Hand
                entry.TotalDebit = txn.Amount;
                entry.TotalCredit = txn.Amount;
                entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits", DebitAmount = txn.Amount, CreditAmount = 0, Description = $"Withdrawal - {customerName}" });
                entry.Lines.Add(new JournalEntryLine { AccountCode = "1010", AccountName = "Cash on Hand", DebitAmount = 0, CreditAmount = txn.Amount, Description = $"Cash disbursed - {customerName}" });
                break;

            case "TRANSFER":
                // Internal transfer - Debit: Sender's liability, Credit: Receiver's liability
                entry.TotalDebit = txn.Amount;
                entry.TotalCredit = txn.Amount;
                entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits (Sender)", DebitAmount = txn.Amount, CreditAmount = 0, Description = $"Transfer out - {customerName}" });
                entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits (Receiver)", DebitAmount = 0, CreditAmount = txn.Amount, Description = $"Transfer in - {txn.ToAccountName}" });
                if (txn.Fee > 0)
                {
                    entry.TotalDebit += txn.Fee;
                    entry.TotalCredit += txn.Fee;
                    entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits", DebitAmount = txn.Fee, CreditAmount = 0, Description = $"Transfer fee charged" });
                    entry.Lines.Add(new JournalEntryLine { AccountCode = "4020", AccountName = "Service Fee Income", DebitAmount = 0, CreditAmount = txn.Fee, Description = $"Transfer fee income" });
                }
                break;

            case "BILLS":
            case "BILLSPAYMENT":
                // Debit: Customer Deposits, Credit: Cash/Due to Biller
                entry.TotalDebit = txn.Amount;
                entry.TotalCredit = txn.Amount;
                entry.Lines.Add(new JournalEntryLine { AccountCode = "2010", AccountName = "Customer Deposits", DebitAmount = txn.Amount, CreditAmount = 0, Description = $"Bills payment - {txn.BillerName}" });
                entry.Lines.Add(new JournalEntryLine { AccountCode = "1010", AccountName = "Cash on Hand", DebitAmount = 0, CreditAmount = txn.Amount, Description = $"Cash paid to biller - {txn.BillerName}" });
                break;

            default:
                return null;
        }

        return entry;
    }

    /// <summary>
    /// Create journal entry from loan payment
    /// </summary>
    private JournalEntry? CreateJournalFromLoanPayment(LoanPayment payment)
    {
        var loanNumber = payment.Loan?.LoanNumber ?? $"Loan #{payment.LoanId}";

        // Get interest from payment schedule if available
        var interestAmount = payment.PaymentSchedule?.InterestAmount ?? 0;
        var principalAmount = payment.PaymentSchedule?.PrincipalAmount ?? (payment.PaymentAmount - payment.PenaltyPaid);

        var entry = new JournalEntry
        {
            JournalId = -(100000 + payment.PaymentId), // Negative ID for auto-generated
            JournalNumber = $"AUTO-LP-{payment.PaymentId:D6}",
            TransactionDate = payment.PaymentDate,
            Description = $"Loan Payment: {loanNumber} - Principal: ₱{principalAmount:N2}, Interest: ₱{interestAmount:N2}, Penalty: ₱{payment.PenaltyPaid:N2}",
            Reference = $"LP-{payment.PaymentId:D6}",
            Status = "Posted",
            CreatedAt = payment.PaymentDate,
            CreatedBy = "System",
            PostedAt = payment.PaymentDate,
            PostedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Cash on Hand
        entry.Lines.Add(new JournalEntryLine { AccountCode = "1010", AccountName = "Cash on Hand", DebitAmount = payment.PaymentAmount, CreditAmount = 0, Description = $"Cash received - Loan payment" });

        // Credit: Loans Receivable (Principal)
        if (principalAmount > 0)
            entry.Lines.Add(new JournalEntryLine { AccountCode = "1110", AccountName = "Loans Receivable", DebitAmount = 0, CreditAmount = principalAmount, Description = $"Principal collection - {loanNumber}" });

        // Credit: Interest Income
        if (interestAmount > 0)
            entry.Lines.Add(new JournalEntryLine { AccountCode = "4010", AccountName = "Interest Income", DebitAmount = 0, CreditAmount = interestAmount, Description = $"Interest collection - {loanNumber}" });

        // Credit: Penalty Income
        if (payment.PenaltyPaid > 0)
            entry.Lines.Add(new JournalEntryLine { AccountCode = "4030", AccountName = "Penalty Income", DebitAmount = 0, CreditAmount = payment.PenaltyPaid, Description = $"Penalty collection - {loanNumber}" });

        entry.TotalDebit = payment.PaymentAmount;
        entry.TotalCredit = payment.PaymentAmount;

        return entry;
    }

    /// <summary>
    /// Create journal entry from loan disbursal
    /// </summary>
    private JournalEntry? CreateJournalFromLoanDisbursal(LoanDisbursal disbursal)
    {
        var loanNumber = disbursal.Loan?.LoanNumber ?? $"Loan #{disbursal.LoanId}";

        var entry = new JournalEntry
        {
            JournalId = -(200000 + disbursal.DisbursalId), // Negative ID for auto-generated
            JournalNumber = $"AUTO-LD-{disbursal.DisbursalId:D6}",
            TransactionDate = disbursal.DisbursalDate,
            Description = $"Loan Disbursal: {loanNumber} - Amount: ₱{disbursal.DisbursalAmount:N2} to {disbursal.DisbursedTo}",
            Reference = disbursal.Reference ?? $"LD-{disbursal.DisbursalId:D6}",
            Status = "Posted",
            CreatedAt = disbursal.DisbursalDate,
            CreatedBy = disbursal.ProcessedBy,
            PostedAt = disbursal.DisbursalDate,
            PostedBy = disbursal.ProcessedBy,
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Loans Receivable
        entry.Lines.Add(new JournalEntryLine { AccountCode = "1110", AccountName = "Loans Receivable", DebitAmount = disbursal.DisbursalAmount, CreditAmount = 0, Description = $"Loan released - {loanNumber}" });

        // Credit: Cash on Hand
        entry.Lines.Add(new JournalEntryLine { AccountCode = "1010", AccountName = "Cash on Hand", DebitAmount = 0, CreditAmount = disbursal.DisbursalAmount, Description = $"Cash disbursed for loan - {loanNumber}" });

        entry.TotalDebit = disbursal.DisbursalAmount;
        entry.TotalCredit = disbursal.DisbursalAmount;

        return entry;
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
        var entry = await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.JournalId == id);
        if (entry == null) return false;

        // Ensure entry is balanced before posting
        if (entry.TotalDebit != entry.TotalCredit)
            return false;

        entry.Status = "Posted";
        entry.PostedBy = postedBy;
        entry.PostedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        // Create GL entries for each line in the journal entry
        if (entry.Lines != null && entry.Lines.Any())
        {
            foreach (var line in entry.Lines)
            {
                // Get account type from chart of accounts
                var chartAccount = await _context.ChartOfAccounts
                    .FirstOrDefaultAsync(c => c.AccountCode == line.AccountCode);

                var glEntry = new GeneralLedgerTransaction
                {
                    JournalLineId = line.LineId,
                    AccountCode = line.AccountCode,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    Description = line.Description ?? entry.Description,
                    TransactionDate = entry.TransactionDate,
                    RunningBalance = 0 // Will be calculated
                };

                _context.GeneralLedgerTransactions.Add(glEntry);
            }
            await _context.SaveChangesAsync();
        }

        return true;
    }

    /// <summary>
    /// Get all chart of accounts for dropdown
    /// </summary>
    public async Task<List<ChartOfAccounts>> GetChartOfAccountsAsync()
    {
        return await _context.ChartOfAccounts
            .Where(c => c.IsActive)
            .OrderBy(c => c.AccountCode)
            .ToListAsync();
    }

    /// <summary>
    /// Get transaction summary statistics
    /// </summary>
    public async Task<(int TotalDeposits, decimal DepositAmount, int TotalWithdrawals, decimal WithdrawalAmount, int TotalLoanPayments, decimal LoanPaymentAmount, int TotalLoanDisbursals, decimal LoanDisbursalAmount)> GetTransactionStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.MinValue;
        var to = toDate ?? DateTime.MaxValue;

        var transactions = await _context.CustomerTransactions
            .Where(t => t.Status == "Completed")
            .ToListAsync();

        var deposits = transactions.Where(t => t.TransactionType.ToUpper() == "DEPOSIT").ToList();
        var withdrawals = transactions.Where(t => t.TransactionType.ToUpper() == "WITHDRAWAL").ToList();

        var loanPayments = await _context.LoanPayments
            .ToListAsync();

        var loanDisbursals = await _context.LoanDisbursals
            .Where(d => d.DisbursalStatus == "Completed")
            .ToListAsync();

        return (
            deposits.Count,
            deposits.Sum(d => d.Amount),
            withdrawals.Count,
            withdrawals.Sum(w => w.Amount),
            loanPayments.Count,
            loanPayments.Sum(p => p.PaymentAmount),
            loanDisbursals.Count,
            loanDisbursals.Sum(d => d.DisbursalAmount)
        );
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
    }

    /// <summary>
    /// Get all general ledger transactions - from DB table plus auto-generated from transactions
    /// Includes journal line details and account information
    /// </summary>
    public async Task<List<GeneralLedgerTransaction>> GetAllAsync()
    {
        var allEntries = new List<GeneralLedgerTransaction>();

        // Get existing GL transactions from database
        var dbTransactions = await _context.GeneralLedgerTransactions
            .Include(t => t.Account)
            .Include(t => t.JournalLine)
                .ThenInclude(jl => jl!.Journal)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.GLTransactionId)
            .ToListAsync();

        // Populate NotMapped properties from related entities
        foreach (var trans in dbTransactions)
        {
            if (trans.Account != null)
            {
                trans.AccountName = trans.Account.AccountName;
                trans.AccountType = trans.Account.AccountType;
            }

            if (trans.JournalLine?.Journal != null)
            {
                trans.Reference = trans.JournalLine.Journal.Reference;
            }
        }
        allEntries.AddRange(dbTransactions);

        // If no DB entries, auto-generate from existing transactions
        if (dbTransactions.Count == 0)
        {
            var autoEntries = await GenerateGLFromTransactionsAsync();
            allEntries.AddRange(autoEntries);
        }

        return allEntries.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.GLTransactionId).ToList();
    }

    /// <summary>
    /// Generate GL entries from existing CustomerTransactions, LoanPayments, LoanDisbursals, SavingsTransactions
    /// </summary>
    private async Task<List<GeneralLedgerTransaction>> GenerateGLFromTransactionsAsync()
    {
        var entries = new List<GeneralLedgerTransaction>();
        int tempId = -1;

        try
        {
            // Generate from Customer Transactions (Deposits, Withdrawals, Bills)
            var customerTransactions = await _context.CustomerTransactions
                .Include(t => t.Account)
                .ThenInclude(a => a!.Customer)
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            foreach (var txn in customerTransactions)
            {
                var customerName = txn.Account?.Customer?.FullName ?? $"Account #{txn.AccountId}";
                var txnDate = txn.ProcessedAt ?? txn.CreatedAt;
                var reference = txn.TransactionNumber ?? $"TXN-{txn.TransactionId}";

                switch (txn.TransactionType.ToUpper())
                {
                    case "DEPOSIT":
                        // Debit Cash, Credit Customer Deposits
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "1010",
                            AccountName = "Cash on Hand",
                            AccountType = "Asset",
                            DebitAmount = txn.Amount,
                            CreditAmount = 0,
                            Description = $"Cash received - {customerName}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "2010",
                            AccountName = "Customer Deposits",
                            AccountType = "Liability",
                            DebitAmount = 0,
                            CreditAmount = txn.Amount,
                            Description = $"Deposit liability - {customerName}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        break;

                    case "WITHDRAWAL":
                        // Debit Customer Deposits, Credit Cash
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "2010",
                            AccountName = "Customer Deposits",
                            AccountType = "Liability",
                            DebitAmount = txn.Amount,
                            CreditAmount = 0,
                            Description = $"Withdrawal - {customerName}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "1010",
                            AccountName = "Cash on Hand",
                            AccountType = "Asset",
                            DebitAmount = 0,
                            CreditAmount = txn.Amount,
                            Description = $"Cash disbursed - {customerName}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        break;

                    case "BILLS":
                    case "BILLSPAYMENT":
                        // Debit Customer Deposits, Credit Cash
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "2010",
                            AccountName = "Customer Deposits",
                            AccountType = "Liability",
                            DebitAmount = txn.Amount,
                            CreditAmount = 0,
                            Description = $"Bills Payment - {txn.BillerName ?? "Biller"}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "1010",
                            AccountName = "Cash on Hand",
                            AccountType = "Asset",
                            DebitAmount = 0,
                            CreditAmount = txn.Amount,
                            Description = $"Cash paid to biller - {txn.BillerName ?? "Biller"}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        break;

                    case "TRANSFER":
                        // Internal transfer
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "2010",
                            AccountName = "Customer Deposits (Sender)",
                            AccountType = "Liability",
                            DebitAmount = txn.Amount,
                            CreditAmount = 0,
                            Description = $"Transfer out - {customerName}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        entries.Add(new GeneralLedgerTransaction
                        {
                            GLTransactionId = tempId--,
                            AccountCode = "2010",
                            AccountName = "Customer Deposits (Receiver)",
                            AccountType = "Liability",
                            DebitAmount = 0,
                            CreditAmount = txn.Amount,
                            Description = $"Transfer in - {txn.ToAccountName ?? "Receiver"}",
                            TransactionDate = txnDate,
                            Reference = reference
                        });
                        break;
                }
            }

            // Generate from Loan Payments
            var loanPayments = await _context.LoanPayments
                .Include(p => p.Loan)
                .Include(p => p.PaymentSchedule)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            foreach (var payment in loanPayments)
            {
                var loanNumber = payment.Loan?.LoanNumber ?? $"Loan #{payment.LoanId}";
                var principalAmount = payment.PaymentSchedule?.PrincipalAmount ?? (payment.PaymentAmount - payment.PenaltyPaid);
                var interestAmount = payment.PaymentSchedule?.InterestAmount ?? 0;
                var reference = $"LP-{payment.PaymentId:D6}";

                // Debit Cash
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1010",
                    AccountName = "Cash on Hand",
                    AccountType = "Asset",
                    DebitAmount = payment.PaymentAmount,
                    CreditAmount = 0,
                    Description = $"Loan payment received - {loanNumber}",
                    TransactionDate = payment.PaymentDate,
                    Reference = reference
                });

                // Credit Loans Receivable (Principal)
                if (principalAmount > 0)
                {
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "1110",
                        AccountName = "Loans Receivable",
                        AccountType = "Asset",
                        DebitAmount = 0,
                        CreditAmount = principalAmount,
                        Description = $"Principal collection - {loanNumber}",
                        TransactionDate = payment.PaymentDate,
                        Reference = reference
                    });
                }

                // Credit Interest Income
                if (interestAmount > 0)
                {
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "4010",
                        AccountName = "Interest Income",
                        AccountType = "Revenue",
                        DebitAmount = 0,
                        CreditAmount = interestAmount,
                        Description = $"Interest collection - {loanNumber}",
                        TransactionDate = payment.PaymentDate,
                        Reference = reference
                    });
                }

                // Credit Penalty Income
                if (payment.PenaltyPaid > 0)
                {
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "4030",
                        AccountName = "Penalty Income",
                        AccountType = "Revenue",
                        DebitAmount = 0,
                        CreditAmount = payment.PenaltyPaid,
                        Description = $"Penalty collection - {loanNumber}",
                        TransactionDate = payment.PaymentDate,
                        Reference = reference
                    });
                }
            }

            // Generate from Loan Disbursals
            var loanDisbursals = await _context.LoanDisbursals
                .Include(d => d.Loan)
                .Where(d => d.DisbursalStatus == "Completed")
                .OrderByDescending(d => d.DisbursalDate)
                .ToListAsync();

            foreach (var disbursal in loanDisbursals)
            {
                var loanNumber = disbursal.Loan?.LoanNumber ?? $"Loan #{disbursal.LoanId}";
                var reference = disbursal.Reference ?? $"LD-{disbursal.DisbursalId:D6}";

                // Debit Loans Receivable
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1110",
                    AccountName = "Loans Receivable",
                    AccountType = "Asset",
                    DebitAmount = disbursal.DisbursalAmount,
                    CreditAmount = 0,
                    Description = $"Loan disbursed - {loanNumber} to {disbursal.DisbursedTo}",
                    TransactionDate = disbursal.DisbursalDate,
                    Reference = reference
                });

                // Credit Cash
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1010",
                    AccountName = "Cash on Hand",
                    AccountType = "Asset",
                    DebitAmount = 0,
                    CreditAmount = disbursal.DisbursalAmount,
                    Description = $"Cash disbursed for loan - {loanNumber}",
                    TransactionDate = disbursal.DisbursalDate,
                    Reference = reference
                });
            }

            // Generate from Savings Transactions
            var savingsTransactions = await _context.SavingsTransactions
                .Include(s => s.SavingsAccount)
                .Where(s => s.Status == "Completed")
                .OrderByDescending(s => s.TransactionDate)
                .ToListAsync();

            foreach (var stxn in savingsTransactions)
            {
                var reference = stxn.TransactionNumber ?? $"SAV-{stxn.TransactionId}";

                if (stxn.TransactionType == "Deposit")
                {
                    // Debit Cash, Credit Savings Deposits
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "1010",
                        AccountName = "Cash on Hand",
                        AccountType = "Asset",
                        DebitAmount = stxn.Amount,
                        CreditAmount = 0,
                        Description = $"Savings deposit received - {reference}",
                        TransactionDate = stxn.TransactionDate,
                        Reference = reference
                    });
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "2101",
                        AccountName = "Savings Deposits",
                        AccountType = "Liability",
                        DebitAmount = 0,
                        CreditAmount = stxn.Amount,
                        Description = $"Savings liability - {reference}",
                        TransactionDate = stxn.TransactionDate,
                        Reference = reference
                    });
                }
                else if (stxn.TransactionType == "Withdrawal")
                {
                    // Debit Savings Deposits, Credit Cash
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "2101",
                        AccountName = "Savings Deposits",
                        AccountType = "Liability",
                        DebitAmount = stxn.Amount,
                        CreditAmount = 0,
                        Description = $"Savings withdrawal - {reference}",
                        TransactionDate = stxn.TransactionDate,
                        Reference = reference
                    });
                    entries.Add(new GeneralLedgerTransaction
                    {
                        GLTransactionId = tempId--,
                        AccountCode = "1010",
                        AccountName = "Cash on Hand",
                        AccountType = "Asset",
                        DebitAmount = 0,
                        CreditAmount = stxn.Amount,
                        Description = $"Cash paid out - {reference}",
                        TransactionDate = stxn.TransactionDate,
                        Reference = reference
                    });
                }
            }

            // Generate from Accounts Payable payments
            var apPayments = await _context.AccountsPayables
                .Where(ap => ap.PaidAmount > 0)
                .OrderByDescending(ap => ap.CreatedAt)
                .ToListAsync();

            foreach (var ap in apPayments)
            {
                var reference = ap.InvoiceNumber ?? $"AP-{ap.PayableId}";

                // Debit AP, Credit Cash
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "2000",
                    AccountName = "Accounts Payable",
                    AccountType = "Liability",
                    DebitAmount = ap.PaidAmount,
                    CreditAmount = 0,
                    Description = $"AP Payment - {ap.VendorName}",
                    TransactionDate = ap.DueDate,
                    Reference = reference
                });
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1010",
                    AccountName = "Cash on Hand",
                    AccountType = "Asset",
                    DebitAmount = 0,
                    CreditAmount = ap.PaidAmount,
                    Description = $"Cash paid for AP - {ap.VendorName}",
                    TransactionDate = ap.DueDate,
                    Reference = reference
                });
            }

            // Generate from Accounts Receivable receipts
            var arReceipts = await _context.AccountsReceivables
                .Where(ar => ar.ReceivedAmount > 0)
                .OrderByDescending(ar => ar.CreatedAt)
                .ToListAsync();

            foreach (var ar in arReceipts)
            {
                var reference = ar.InvoiceNumber ?? $"AR-{ar.ReceivableId}";

                // Debit Cash, Credit AR
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1010",
                    AccountName = "Cash on Hand",
                    AccountType = "Asset",
                    DebitAmount = ar.ReceivedAmount,
                    CreditAmount = 0,
                    Description = $"AR Receipt - {ar.CustomerName}",
                    TransactionDate = ar.DueDate,
                    Reference = reference
                });
                entries.Add(new GeneralLedgerTransaction
                {
                    GLTransactionId = tempId--,
                    AccountCode = "1200",
                    AccountName = "Accounts Receivable",
                    AccountType = "Asset",
                    DebitAmount = 0,
                    CreditAmount = ar.ReceivedAmount,
                    Description = $"AR collected - {ar.CustomerName}",
                    TransactionDate = ar.DueDate,
                    Reference = reference
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating GL from transactions: {ex.Message}");
        }

        return entries;
    }

    /// <summary>
    /// Get GL transaction by ID with full details
    /// </summary>
    public async Task<GeneralLedgerTransaction?> GetByIdWithDetailsAsync(int id)
    {
        var transaction = await _context.GeneralLedgerTransactions
            .Include(t => t.Account)
            .Include(t => t.JournalLine)
                .ThenInclude(jl => jl!.Journal)
                    .ThenInclude(je => je!.Lines)
            .FirstOrDefaultAsync(t => t.GLTransactionId == id);

        if (transaction != null)
        {
            if (transaction.Account != null)
            {
                transaction.AccountName = transaction.Account.AccountName;
                transaction.AccountType = transaction.Account.AccountType;
            }

            if (transaction.JournalLine?.Journal != null)
            {
                transaction.Reference = transaction.JournalLine.Journal.Reference;
            }
        }

        return transaction;
    }

    /// <summary>
    /// Get GL transactions by account code
    /// </summary>
    public async Task<List<GeneralLedgerTransaction>> GetByAccountAsync(string accountCode, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.GeneralLedgerTransactions
            .Include(t => t.Account)
            .Where(t => t.AccountCode == accountCode);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        var transactions = await query.OrderBy(t => t.TransactionDate).ToListAsync();

        // Populate NotMapped properties
        foreach (var trans in transactions)
        {
            if (trans.Account != null)
            {
                trans.AccountName = trans.Account.AccountName;
                trans.AccountType = trans.Account.AccountType;
            }
        }

        return transactions;
    }

    /// <summary>
    /// Get account balances - from GeneralLedger table or computed from transactions
    /// </summary>
    public async Task<List<(string AccountCode, string AccountName, string AccountType, decimal Balance)>> GetAccountBalancesAsync()
    {
        // First try to get from GeneralLedger table
        var accounts = await _context.GeneralLedger
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountCode)
            .Select(a => new ValueTuple<string, string, string, decimal>(
                a.AccountCode,
                a.AccountName,
                a.AccountType,
                a.Balance
            ))
            .ToListAsync();

        // If no accounts with balances, compute from auto-generated entries
        if (!accounts.Any() || accounts.All(a => a.Item4 == 0))
        {
            var allEntries = await GetAllAsync();
            var computedBalances = allEntries
                .GroupBy(e => new { e.AccountCode, e.AccountName, e.AccountType })
                .Select(g => new ValueTuple<string, string, string, decimal>(
                    g.Key.AccountCode,
                    g.Key.AccountName ?? "Unknown",
                    g.Key.AccountType ?? "Asset",
                    // For Assets/Expenses: Debit increases, Credit decreases
                    // For Liabilities/Revenue: Credit increases, Debit decreases
                    (g.Key.AccountType == "Asset" || g.Key.AccountType == "Expense")
                        ? g.Sum(e => e.DebitAmount) - g.Sum(e => e.CreditAmount)
                        : g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount)
                ))
                .OrderBy(b => b.Item1)
                .ToList();

            if (computedBalances.Any())
                return computedBalances;
        }

        return accounts;
    }

    public async Task<GeneralLedgerTransaction?> GetByIdAsync(int id)
    {
        return await _context.GeneralLedgerTransactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.GLTransactionId == id);
    }

    /// <summary>
    /// Get all chart of accounts
    /// </summary>
    public async Task<List<GeneralLedger>> GetChartOfAccountsAsync()
    {
        return await _context.GeneralLedger
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    /// <summary>
    /// Get account by code
    /// </summary>
    public async Task<GeneralLedger?> GetAccountByCodeAsync(string accountCode)
    {
        return await _context.GeneralLedger
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode);
    }

    /// <summary>
    /// Create GL entries from a posted journal entry
    /// </summary>
    public async Task CreateGLEntriesFromJournalAsync(JournalEntry journal)
    {
        if (journal.Lines == null || !journal.Lines.Any())
            return;

        foreach (var line in journal.Lines)
        {
            // Get account type from chart of accounts
            var chartAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.AccountCode == line.AccountCode);

            var glEntry = new GeneralLedgerTransaction
            {
                JournalLineId = line.LineId,
                AccountCode = line.AccountCode,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Description = line.Description ?? journal.Description,
                TransactionDate = journal.TransactionDate,
                RunningBalance = 0 // Will be calculated
            };

            _context.GeneralLedgerTransactions.Add(glEntry);
        }

        await _context.SaveChangesAsync();
    }
}

public class TrialBalanceService
{
    private readonly BFASDbContext _context;
    private readonly GeneralLedgerService _glService;

    public TrialBalanceService(BFASDbContext context)
    {
        _context = context;
        _glService = new GeneralLedgerService(context);
    }

    /// <summary>
    /// Get trial balance computed from real transaction data
    /// </summary>
    public async Task<List<FinanceBank.Models.TrialBalanceEntry>> GetAllAsync()
    {
        // Get all GL entries and compute balances
        var glEntries = await _glService.GetAllAsync();

        // Group by account and compute totals
        var accountGroups = glEntries
            .GroupBy(e => new { e.AccountCode, e.AccountName, e.AccountType })
            .Select(g => new FinanceBank.Models.TrialBalanceEntry
            {
                TrialBalanceId = g.Key.AccountCode.GetHashCode(),
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                AccountType = g.Key.AccountType,
                DebitBalance = g.Sum(e => e.DebitAmount),
                CreditBalance = g.Sum(e => e.CreditAmount),
                AsOfDate = DateTime.Today,
                CreatedAt = DateTime.Now
            })
            .OrderBy(t => t.AccountCode)
            .ToList();

        // Adjust balances based on normal balance (Assets/Expenses = Debit, Liabilities/Equity/Revenue = Credit)
        foreach (var entry in accountGroups)
        {
            var netBalance = entry.DebitBalance - entry.CreditBalance;

            if (entry.AccountType == "Asset" || entry.AccountType == "Expense")
            {
                // Normal debit balance
                if (netBalance >= 0)
                {
                    entry.DebitBalance = netBalance;
                    entry.CreditBalance = 0;
                }
                else
                {
                    entry.DebitBalance = 0;
                    entry.CreditBalance = Math.Abs(netBalance);
                }
            }
            else // Liability, Equity, Revenue
            {
                // Normal credit balance
                if (netBalance <= 0)
                {
                    entry.CreditBalance = Math.Abs(netBalance);
                    entry.DebitBalance = 0;
                }
                else
                {
                    entry.CreditBalance = 0;
                    entry.DebitBalance = netBalance;
                }
            }
        }

        return accountGroups;
    }

    /// <summary>
    /// Generate trial balance for a specific date range
    /// </summary>
    public async Task<List<FinanceBank.Models.TrialBalanceEntry>> GenerateAsync(DateTime asOfDate)
    {
        var entries = await GetAllAsync();
        foreach (var entry in entries)
        {
            entry.AsOfDate = asOfDate;
        }
        return entries;
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
        var entries = await GetAllAsync();
        return (entries.Sum(t => t.DebitBalance), entries.Sum(t => t.CreditBalance));
    }

    /// <summary>
    /// Check if trial balance is balanced (Debits = Credits)
    /// </summary>
    public async Task<bool> IsBalancedAsync()
    {
        var (debit, credit) = await GetTotalsAsync();
        return Math.Abs(debit - credit) < 0.01m;
    }
}

public class FinancialStatementService
{
    private readonly BFASDbContext _context;
    private readonly GeneralLedgerService _glService;

    public FinancialStatementService(BFASDbContext context)
    {
        _context = context;
        _glService = new GeneralLedgerService(context);
    }

    public async Task<List<FinanceBank.Models.FinancialStatement>> GetAllAsync()
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

    /// <summary>
    /// Generate Income Statement from real transaction data
    /// </summary>
    public async Task<FinanceBank.Models.FinancialStatement> GenerateIncomeStatementAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
    {
        var glEntries = await _glService.GetAllAsync();
        var periodEntries = glEntries.Where(e => e.TransactionDate >= periodStart && e.TransactionDate <= periodEnd).ToList();

        // Calculate Revenue (Credit balances in Revenue accounts)
        var revenues = periodEntries.Where(e => e.AccountType == "Revenue")
            .GroupBy(e => e.AccountName)
            .Select(g => new { Account = g.Key, Amount = g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount) })
            .ToList();

        // Calculate Expenses (Debit balances in Expense accounts)
        var expenses = periodEntries.Where(e => e.AccountType == "Expense")
            .GroupBy(e => e.AccountName)
            .Select(g => new { Account = g.Key, Amount = g.Sum(e => e.DebitAmount) - g.Sum(e => e.CreditAmount) })
            .ToList();

        var totalRevenue = revenues.Sum(r => r.Amount);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var netIncome = totalRevenue - totalExpenses;

        var statement = new FinanceBank.Models.FinancialStatement
        {
            StatementType = "IncomeStatement",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            StatementData = System.Text.Json.JsonSerializer.Serialize(new
            {
                Revenues = revenues,
                Expenses = expenses,
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetIncome = netIncome
            }),
            GeneratedAt = DateTime.Now,
            GeneratedBy = generatedBy
        };

        _context.FinancialStatements.Add(statement);
        await _context.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Generate Balance Sheet from real transaction data
    /// </summary>
    public async Task<FinanceBank.Models.FinancialStatement> GenerateBalanceSheetAsync(DateTime asOfDate, string generatedBy)
    {
        var glEntries = await _glService.GetAllAsync();
        var periodEntries = glEntries.Where(e => e.TransactionDate <= asOfDate).ToList();

        // Assets
        var assets = periodEntries.Where(e => e.AccountType == "Asset")
            .GroupBy(e => e.AccountName)
            .Select(g => new { Account = g.Key, Balance = g.Sum(e => e.DebitAmount) - g.Sum(e => e.CreditAmount) })
            .Where(a => a.Balance != 0)
            .ToList();

        // Liabilities
        var liabilities = periodEntries.Where(e => e.AccountType == "Liability")
            .GroupBy(e => e.AccountName)
            .Select(g => new { Account = g.Key, Balance = g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount) })
            .Where(l => l.Balance != 0)
            .ToList();

        // Equity (including retained earnings from Revenue - Expenses)
        var equity = periodEntries.Where(e => e.AccountType == "Equity")
            .GroupBy(e => e.AccountName)
            .Select(g => new { Account = g.Key, Balance = g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount) })
            .ToList();

        var totalAssets = assets.Sum(a => a.Balance);
        var totalLiabilities = liabilities.Sum(l => l.Balance);
        var totalEquity = equity.Sum(e => e.Balance);

        // Add retained earnings (Net Income = Revenue - Expenses)
        var revenueTotal = periodEntries.Where(e => e.AccountType == "Revenue").Sum(e => e.CreditAmount - e.DebitAmount);
        var expenseTotal = periodEntries.Where(e => e.AccountType == "Expense").Sum(e => e.DebitAmount - e.CreditAmount);
        var retainedEarnings = revenueTotal - expenseTotal;
        totalEquity += retainedEarnings;

        var statement = new FinanceBank.Models.FinancialStatement
        {
            StatementType = "BalanceSheet",
            PeriodStart = asOfDate,
            PeriodEnd = asOfDate,
            StatementData = System.Text.Json.JsonSerializer.Serialize(new
            {
                Assets = assets,
                Liabilities = liabilities,
                Equity = equity,
                RetainedEarnings = retainedEarnings,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity
            }),
            GeneratedAt = DateTime.Now,
            GeneratedBy = generatedBy
        };

        _context.FinancialStatements.Add(statement);
        await _context.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Generate Cash Flow Statement from real transaction data
    /// </summary>
    public async Task<FinanceBank.Models.FinancialStatement> GenerateCashFlowAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
    {
        // Get customer transactions for cash flow
        var transactions = await _context.CustomerTransactions
            .Where(t => t.Status == "Completed" && t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd)
            .ToListAsync();

        var loanPayments = await _context.LoanPayments
            .Where(p => p.PaymentDate >= periodStart && p.PaymentDate <= periodEnd)
            .ToListAsync();

        var loanDisbursals = await _context.LoanDisbursals
            .Where(d => d.DisbursalStatus == "Completed" && d.DisbursalDate >= periodStart && d.DisbursalDate <= periodEnd)
            .ToListAsync();

        // Operating Activities
        var deposits = transactions.Where(t => t.TransactionType.ToUpper() == "DEPOSIT").Sum(t => t.Amount);
        var withdrawals = transactions.Where(t => t.TransactionType.ToUpper() == "WITHDRAWAL").Sum(t => t.Amount);
        var fees = transactions.Sum(t => t.Fee);
        var loanPaymentTotal = loanPayments.Sum(p => p.PaymentAmount);

        // Financing Activities
        var loanDisbursalTotal = loanDisbursals.Sum(d => d.DisbursalAmount);

        var netCashFromOperating = deposits - withdrawals + fees + loanPaymentTotal;
        var netCashFromFinancing = -loanDisbursalTotal;
        var netCashChange = netCashFromOperating + netCashFromFinancing;

        var statement = new FinanceBank.Models.FinancialStatement
        {
            StatementType = "CashFlow",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            StatementData = System.Text.Json.JsonSerializer.Serialize(new
            {
                OperatingActivities = new
                {
                    CustomerDeposits = deposits,
                    CustomerWithdrawals = -withdrawals,
                    ServiceFees = fees,
                    LoanPaymentsReceived = loanPaymentTotal,
                    NetCashFromOperating = netCashFromOperating
                },
                FinancingActivities = new
                {
                    LoanDisbursements = -loanDisbursalTotal,
                    NetCashFromFinancing = netCashFromFinancing
                },
                NetCashChange = netCashChange
            }),
            GeneratedAt = DateTime.Now,
            GeneratedBy = generatedBy
        };

        _context.FinancialStatements.Add(statement);
        await _context.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Get financial summary for dashboard - Total Assets, Liabilities, Equity, Revenue, Expenses
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetFinancialSummaryAsync(DateTime? asOfDate = null)
    {
        var targetDate = asOfDate ?? DateTime.Today;
        var glEntries = await _glService.GetAllAsync();
        var periodEntries = glEntries.Where(e => e.TransactionDate <= targetDate).ToList();

        var summary = new Dictionary<string, decimal>
        {
            ["TotalAssets"] = periodEntries.Where(e => e.AccountType == "Asset")
                .Sum(e => e.DebitAmount - e.CreditAmount),
            ["TotalLiabilities"] = periodEntries.Where(e => e.AccountType == "Liability")
                .Sum(e => e.CreditAmount - e.DebitAmount),
            ["TotalEquity"] = periodEntries.Where(e => e.AccountType == "Equity")
                .Sum(e => e.CreditAmount - e.DebitAmount),
            ["TotalRevenue"] = periodEntries.Where(e => e.AccountType == "Revenue")
                .Sum(e => e.CreditAmount - e.DebitAmount),
            ["TotalExpenses"] = periodEntries.Where(e => e.AccountType == "Expense")
                .Sum(e => e.DebitAmount - e.CreditAmount)
        };

        // Calculate Net Income
        summary["NetIncome"] = summary["TotalRevenue"] - summary["TotalExpenses"];

        // Add retained earnings to equity
        summary["TotalEquity"] += summary["NetIncome"];

        return summary;
    }

    /// <summary>
    /// Get loan portfolio summary for financial analysis
    /// </summary>
    public async Task<Dictionary<string, object>> GetLoanPortfolioSummaryAsync()
    {
        var activeLoans = await _context.Loans
            .Where(l => l.Status == "ACTIVE")
            .ToListAsync();

        var completedLoans = await _context.Loans
            .Where(l => l.Status == "COMPLETED" || l.Status == "PAID")
            .ToListAsync();

        var overdueLoans = await _context.Loans
            .Where(l => l.Status == "OVERDUE" || l.Status == "DEFAULTED")
            .ToListAsync();

        return new Dictionary<string, object>
        {
            ["ActiveLoansCount"] = activeLoans.Count,
            ["ActiveLoansPrincipal"] = activeLoans.Sum(l => l.LoanAmount),
            ["OutstandingBalance"] = activeLoans.Sum(l => l.OutstandingBalance),
            ["CompletedLoansCount"] = completedLoans.Count,
            ["CompletedLoansPrincipal"] = completedLoans.Sum(l => l.LoanAmount),
            ["OverdueLoansCount"] = overdueLoans.Count,
            ["OverdueLoansBalance"] = overdueLoans.Sum(l => l.OutstandingBalance),
            ["TotalLoansPortfolio"] = activeLoans.Sum(l => l.LoanAmount) + completedLoans.Sum(l => l.LoanAmount)
        };
    }

    /// <summary>
    /// Generate comprehensive financial report with all statements combined
    /// </summary>
    public async Task<Dictionary<string, object>> GenerateComprehensiveReportAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
    {
        var incomeStatement = await GenerateIncomeStatementAsync(periodStart, periodEnd, generatedBy);
        var balanceSheet = await GenerateBalanceSheetAsync(periodEnd, generatedBy);
        var cashFlow = await GenerateCashFlowAsync(periodStart, periodEnd, generatedBy);
        var financialSummary = await GetFinancialSummaryAsync(periodEnd);
        var loanPortfolio = await GetLoanPortfolioSummaryAsync();

        return new Dictionary<string, object>
        {
            ["IncomeStatement"] = incomeStatement,
            ["BalanceSheet"] = balanceSheet,
            ["CashFlowStatement"] = cashFlow,
            ["FinancialSummary"] = financialSummary,
            ["LoanPortfolio"] = loanPortfolio,
            ["GeneratedAt"] = DateTime.Now,
            ["GeneratedBy"] = generatedBy,
            ["PeriodStart"] = periodStart,
            ["PeriodEnd"] = periodEnd
        };
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
    }
    public async Task<List<Budget>> GetAllAsync()
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
    }
    public async Task<List<CashflowEntry>> GetAllAsync()
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
    }
    public async Task<List<FinancialForecast>> GetAllAsync()
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
    }
    public async Task<List<CustomerAccount>> GetAllAsync()
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
    }

    public async Task<List<CustomerTransaction>> GetAllAsync()
    {
        return await _context.CustomerTransactions
            .Include(ct => ct.Account)
            .OrderByDescending(ct => ct.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetByAccountIdAsync(int accountId)
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
    }
    public async Task<List<Card>> GetByAccountIdAsync(int accountId)
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
    }
    public async Task<List<Loan>> GetByAccountIdAsync(int accountId)
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

    public async Task<List<Loan>> GetAllOutstandingLoansAsync()
    {
        // Get all loans with outstanding balance > 0 and status Active
        return await _context.Loans
            .Include(l => l.Account)
                .ThenInclude(a => a!.Customer)
            .Where(l => l.OutstandingBalance > 0 && l.Status == "Active")
            .OrderByDescending(l => l.OutstandingBalance)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetAllLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Account)
                .ThenInclude(a => a!.Customer)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetAllAsync()
    {
        return await GetAllLoansAsync();
    }
}

public class SavingsGoalService
{
    private readonly BFASDbContext _context;

    public SavingsGoalService(BFASDbContext context)
    {
        _context = context;
    }
    public async Task<List<SavingsGoalEntity>> GetByAccountIdAsync(int accountId)
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
    }
    public async Task<List<RewardPointsEntity>> GetByAccountIdAsync(int accountId)
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

// ===== AUDIT LOG SERVICE =====
public class AuditLogService
{
    private readonly BFASDbContext _context;

    public AuditLogService(BFASDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<AuditLog> GetByIdAsync(int id)
    {
        return await _context.AuditLogs.FindAsync(id);
    }

    public async Task CreateAsync(AuditLog auditLog)
    {
        auditLog.CreatedAt = DateTime.Now;
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    // ==================== COMPREHENSIVE AUDIT LOGGING METHODS ====================

    /// <summary>
    /// Log successful login attempts
    /// </summary>
    public async Task LogLoginSuccessAsync(string userId, string userName, string ipAddress = "", string userAgent = "")
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = "LOGIN_SUCCESS",
            Module = "Authentication",
            Description = $"User '{userName}' logged in successfully",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });
    }

    /// <summary>
    /// Log failed login attempts - CRITICAL for security monitoring
    /// </summary>
    public async Task LogLoginFailureAsync(string attemptedUserId, string reason, string ipAddress = "", string userAgent = "")
    {
        await CreateAsync(new AuditLog
        {
            UserId = attemptedUserId ?? "UNKNOWN",
            Action = "LOGIN_FAILED",
            Module = "Authentication",
            Description = $"Failed login attempt. Reason: {reason}",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        // Check for suspicious activity
        await CheckSuspiciousLoginActivity(attemptedUserId, ipAddress);
    }

    /// <summary>
    /// Log logout events
    /// </summary>
    public async Task LogLogoutAsync(string userId, string userName)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = "LOGOUT",
            Module = "Authentication",
            Description = $"User '{userName}' logged out"
        });
    }

    /// <summary>
    /// Log page navigation/access
    /// </summary>
    public async Task LogPageAccessAsync(string userId, string userName, string pageName, string module)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = "PAGE_ACCESS",
            Module = module,
            Description = $"User '{userName}' accessed page: {pageName}"
        });
    }

    /// <summary>
    /// Log financial transactions with amount and balance tracking
    /// </summary>
    public async Task LogTransactionAsync(
        string userId,
        string userName,
        string action,
        string module,
        string description,
        decimal? amount = null,
        decimal? balanceBefore = null,
        decimal? balanceAfter = null,
        string? oldValues = null,
        string? newValues = null)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            Module = module,
            Description = $"{description}" + (amount.HasValue ? $" Amount: ₱{amount.Value:N2}" : "") +
                         (balanceBefore.HasValue ? $" Balance Before: ₱{balanceBefore.Value:N2}" : "") +
                         (balanceAfter.HasValue ? $" Balance After: ₱{balanceAfter.Value:N2}" : "")
        });

        // Check for suspicious transaction patterns
        if (amount.HasValue && amount.Value > 100000) // Large transaction threshold
        {
            await LogSuspiciousActivityAsync(userId, "LARGE_TRANSACTION",
                $"Large transaction detected: ₱{amount.Value:N2}");
        }
    }

    /// <summary>
    /// Log data modifications (Create, Update, Delete)
    /// </summary>
    public async Task LogDataChangeAsync(
        string userId,
        string userName,
        string action, // CREATE, UPDATE, DELETE
        string module,
        string entityName,
        string entityId,
        string? oldValues = null,
        string? newValues = null)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = $"{action}_{entityName.ToUpper()}",
            Module = module,
            Description = $"User '{userName}' {action.ToLower()}d {entityName} (ID: {entityId})"
        });
    }

    /// <summary>
    /// Log approval actions (Loan approvals, withdrawal approvals, etc.)
    /// </summary>
    public async Task LogApprovalActionAsync(
        string userId,
        string userName,
        string action, // APPROVE, REJECT, OVERRIDE
        string module,
        string approvalType,
        string requestId,
        string? reason = null,
        decimal? amount = null)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = $"{action}_{approvalType.ToUpper()}",
            Module = module,
            Description = $"User '{userName}' {action.ToLower()}d {approvalType} (Request ID: {requestId}). Reason: {reason ?? "N/A"}" +
                         (amount.HasValue ? $" Amount: ₱{amount.Value:N2}" : "")
        });

        // Log Super Admin overrides as potentially suspicious
        if (action == "OVERRIDE")
        {
            await LogSuspiciousActivityAsync(userId, "ADMIN_OVERRIDE",
                $"Super Admin override on {approvalType} (Request: {requestId})");
        }
    }

    /// <summary>
    /// Log suspicious activity for security monitoring
    /// </summary>
    public async Task LogSuspiciousActivityAsync(string userId, string activityType, string description)
    {
        await CreateAsync(new AuditLog
        {
            UserId = userId,
            Action = "SUSPICIOUS_ACTIVITY",
            Module = "Security",
            Description = $"[{activityType}] {description}"
        });
    }

    /// <summary>
    /// Check for suspicious login patterns
    /// </summary>
    private async Task CheckSuspiciousLoginActivity(string userId, string ipAddress)
    {
        // Check failed login attempts in last 15 minutes
        var recentFailures = await _context.AuditLogs
            .Where(a => a.UserId == userId
                     && a.Action == "LOGIN_FAILED"
                     && a.CreatedAt >= DateTime.Now.AddMinutes(-15))
            .CountAsync();

        if (recentFailures >= 3)
        {
            await LogSuspiciousActivityAsync(userId, "MULTIPLE_FAILED_LOGINS",
                $"Multiple failed login attempts detected: {recentFailures} attempts in 15 minutes from IP: {ipAddress}");
        }
    }

    /// <summary>
    /// Get recent audit logs for a specific user
    /// </summary>
    public async Task<List<AuditLog>> GetUserAuditLogsAsync(string userId, int limit = 100)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get suspicious activities for security dashboard
    /// </summary>
    public async Task<List<AuditLog>> GetSuspiciousActivitiesAsync(int limit = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == "SUSPICIOUS_ACTIVITY" || a.Action == "LOGIN_FAILED")
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get audit logs by module
    /// </summary>
    public async Task<List<AuditLog>> GetLogsByModuleAsync(string module, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.Where(a => a.Module == module);

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get financial transaction logs (transactions with financial actions)
    /// </summary>
    public async Task<List<AuditLog>> GetFinancialTransactionLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var financialActions = new[] { "DEPOSIT", "WITHDRAWAL", "TRANSFER", "LOAN_DISBURSEMENT", "LOAN_PAYMENT", "INTEREST_POSTING" };
        var query = _context.AuditLogs.Where(a => financialActions.Contains(a.Action) || a.Description.Contains("Amount:"));

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get login history
    /// </summary>
    public async Task<List<AuditLog>> GetLoginHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.Where(a =>
            a.Action == "LOGIN_SUCCESS" ||
            a.Action == "LOGIN_FAILED" ||
            a.Action == "LOGOUT");

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Helper to get client IP address (if available) - Not available in MAUI
    /// </summary>
    public string GetClientIpAddress()
    {
        return "MAUI_APP"; // MAUI apps don't have HTTP context
    }

    /// <summary>
    /// Helper to get user agent (if available) - Not available in MAUI
    /// </summary>
    public string GetUserAgent()
    {
        return "MAUI_APP"; // MAUI apps don't have HTTP context
    }

    public async Task ClearAllAsync()
    {
        _context.AuditLogs.RemoveRange(_context.AuditLogs);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetByModuleAsync(string module)
    {
        return await _context.AuditLogs
            .Where(a => a.Module == module)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserAsync(string userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all malicious attempts (IsMalicious = true)
    /// </summary>
    public async Task<List<AuditLog>> GetMaliciousAttemptsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        // Use Action = "MALICIOUS_ATTEMPT" instead of IsMalicious flag
        var query = _context.AuditLogs.Where(a => a.Action == "MALICIOUS_ATTEMPT");

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get security-related audit logs
    /// </summary>
    public async Task<List<AuditLog>> GetSecurityLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var securityActions = new[] { "LOGIN_SUCCESS", "LOGIN_FAILED", "LOGOUT", "MALICIOUS_ATTEMPT", 
                                      "ACCOUNT_LOCKED", "ACCOUNT_UNLOCKED", "PASSWORD_RESET", 
                                      "FAILED_PASSWORD_VERIFICATION", "SUSPICIOUS_ACTIVITY" };
        var query = _context.AuditLogs.Where(a => securityActions.Contains(a.Action) || a.Module == "Security");

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get customer-specific audit logs by UserId
    /// </summary>
    public async Task<List<AuditLog>> GetCustomerLogsAsync(int customerAccountId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Use UserId field instead of CustomerAccountId
        var query = _context.AuditLogs.Where(a => a.UserId == customerAccountId.ToString());

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get count of malicious attempts in the last 24 hours
    /// </summary>
    public async Task<int> GetRecentMaliciousAttemptsCountAsync()
    {
        return await _context.AuditLogs
            .Where(a => a.Action == "MALICIOUS_ATTEMPT" && a.CreatedAt >= DateTime.Now.AddHours(-24))
            .CountAsync();
    }
}
/// Chart of Accounts Service - Manages account codes for journal entries
/// </summary>
public class ChartOfAccountsService
{
    private readonly IDbContextFactory<BFASDbContext> _contextFactory;

    public ChartOfAccountsService(IDbContextFactory<BFASDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Get all active chart of accounts for dropdowns
    /// </summary>
    public async Task<List<ChartOfAccounts>> GetAllActiveAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts
            .Where(c => c.IsActive)
            .OrderBy(c => c.AccountCode)
            .ToListAsync();
    }

    /// <summary>
    /// Get account by code
    /// </summary>
    public async Task<ChartOfAccounts?> GetByCodeAsync(string accountCode)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.AccountCode == accountCode && c.IsActive);
    }
}



