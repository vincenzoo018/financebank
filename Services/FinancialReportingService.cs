using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;
using System.Text.Json;

namespace FinanceBank.Services
{
    /// <summary>
    /// Enhanced Financial Reporting Service for Chart of Accounts and Financial Statements
    /// Integrates with Loan data from CustomerLoans table
    /// </summary>
    public class FinancialReportingService
    {
        private readonly BFASDbContext _context;

        public FinancialReportingService(BFASDbContext context)
        {
            _context = context;
        }

        #region Chart of Accounts Enhanced Methods

        /// <summary>
        /// Get Chart of Accounts with real-time balances from transactions
        /// </summary>
        public async Task<List<ChartOfAccountsViewModel>> GetChartOfAccountsWithBalancesAsync()
        {
            var accounts = await _context.ChartOfAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync();

            var viewModels = new List<ChartOfAccountsViewModel>();

            foreach (var account in accounts)
            {
                var balance = await CalculateAccountBalanceAsync(account.AccountCode);
                viewModels.Add(new ChartOfAccountsViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType,
                    NormalBalance = account.NormalBalance,
                    CurrentBalance = balance,
                    Description = account.Description,
                    IsActive = account.IsActive
                });
            }

            return viewModels;
        }

        /// <summary>
        /// Calculate real-time account balance from all transactions
        /// </summary>
        private async Task<decimal> CalculateAccountBalanceAsync(string accountCode)
        {
            // Get all journal entry lines for this account
            var journalLines = await _context.JournalEntryLines
                .Where(l => l.AccountCode == accountCode)
                .ToListAsync();

            // Get all general ledger entries for this account
            var glEntries = await _context.GeneralLedgerEntries
                .Where(g => g.AccountCode == accountCode)
                .ToListAsync();

            decimal totalDebit = journalLines.Sum(l => l.DebitAmount) + glEntries.Sum(g => g.DebitAmount);
            decimal totalCredit = journalLines.Sum(l => l.CreditAmount) + glEntries.Sum(g => g.CreditAmount);

            // Return net balance
            return totalDebit - totalCredit;
        }

        /// <summary>
        /// Get accounts by type with loan integration
        /// </summary>
        public async Task<Dictionary<string, List<ChartOfAccountsViewModel>>> GetAccountsByTypeAsync()
        {
            var accounts = await GetChartOfAccountsWithBalancesAsync();

            return new Dictionary<string, List<ChartOfAccountsViewModel>>
            {
                ["Asset"] = accounts.Where(a => a.AccountType == "Asset").ToList(),
                ["Liability"] = accounts.Where(a => a.AccountType == "Liability").ToList(),
                ["Equity"] = accounts.Where(a => a.AccountType == "Equity").ToList(),
                ["Revenue"] = accounts.Where(a => a.AccountType == "Revenue").ToList(),
                ["Expense"] = accounts.Where(a => a.AccountType == "Expense").ToList()
            };
        }

        #endregion

        #region Financial Statements with Loan Integration

        /// <summary>
        /// Generate Income Statement with loan interest income
        /// </summary>
        public async Task<IncomeStatementReport> GenerateIncomeStatementWithLoansAsync(DateTime periodStart, DateTime periodEnd)
        {
            // Get interest income from loan payments
            var loanPayments = await _context.LoanPayments
                .Include(p => p.PaymentSchedule)
                .Where(p => p.PaymentDate >= periodStart && p.PaymentDate <= periodEnd)
                .ToListAsync();

            var interestIncome = loanPayments.Sum(p => p.PaymentSchedule?.InterestAmount ?? 0);
            var penaltyIncome = loanPayments.Sum(p => p.PenaltyPaid);

            // Get service fees from transactions
            var serviceFees = await _context.CustomerTransactions
                .Where(t => t.Status == "Completed" &&
                           t.CreatedAt >= periodStart &&
                           t.CreatedAt <= periodEnd)
                .SumAsync(t => t.Fee);

            // Get other revenues from general ledger
            var otherRevenues = await _context.GeneralLedgerEntries
                .Where(e => e.AccountType == "Revenue" &&
                           e.TransactionDate >= periodStart &&
                           e.TransactionDate <= periodEnd)
                .GroupBy(e => e.AccountName)
                .Select(g => new RevenueLineItem
                {
                    AccountName = g.Key,
                    Amount = g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount)
                })
                .ToListAsync();

            // Get expenses
            var expenses = await _context.GeneralLedgerEntries
                .Where(e => e.AccountType == "Expense" &&
                           e.TransactionDate >= periodStart &&
                           e.TransactionDate <= periodEnd)
                .GroupBy(e => e.AccountName)
                .Select(g => new ExpenseLineItem
                {
                    AccountName = g.Key,
                    Amount = g.Sum(e => e.DebitAmount) - g.Sum(e => e.CreditAmount)
                })
                .ToListAsync();

            var totalRevenue = interestIncome + penaltyIncome + serviceFees + otherRevenues.Sum(r => r.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);

            return new IncomeStatementReport
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                InterestIncome = interestIncome,
                PenaltyIncome = penaltyIncome,
                ServiceFeeIncome = serviceFees,
                OtherRevenues = otherRevenues,
                TotalRevenue = totalRevenue,
                Expenses = expenses,
                TotalExpenses = totalExpenses,
                NetIncome = totalRevenue - totalExpenses,
                GeneratedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Generate Balance Sheet with loan receivables
        /// </summary>
        public async Task<BalanceSheetReport> GenerateBalanceSheetWithLoansAsync(DateTime asOfDate)
        {
            // Get loan receivables from active loans
            var activeLoans = await _context.Loans
                .Where(l => l.Status == "ACTIVE" && l.StartDate <= asOfDate)
                .ToListAsync();

            var loansReceivable = activeLoans.Sum(l => l.OutstandingBalance);

            // Get cash from customer transactions
            var deposits = await _context.CustomerTransactions
                .Where(t => t.Status == "Completed" &&
                           t.TransactionType.ToUpper() == "DEPOSIT" &&
                           t.CreatedAt <= asOfDate)
                .SumAsync(t => t.Amount);

            var withdrawals = await _context.CustomerTransactions
                .Where(t => t.Status == "Completed" &&
                           t.TransactionType.ToUpper() == "WITHDRAWAL" &&
                           t.CreatedAt <= asOfDate)
                .SumAsync(t => t.Amount);

            var cashOnHand = deposits - withdrawals;

            // Get customer deposits liability
            var customerDepositsLiability = await _context.CustomerAccounts
                .Where(a => a.CreatedAt <= asOfDate)
                .SumAsync(a => a.Balance);

            // Get assets
            var assets = new List<AssetLineItem>
            {
                new() { AccountName = "Cash on Hand", Amount = cashOnHand },
                new() { AccountName = "Loans Receivable", Amount = loansReceivable }
            };

            // Get liabilities
            var liabilities = new List<LiabilityLineItem>
            {
                new() { AccountName = "Customer Deposits", Amount = customerDepositsLiability }
            };

            // Calculate equity (Assets - Liabilities)
            var totalAssets = assets.Sum(a => a.Amount);
            var totalLiabilities = liabilities.Sum(l => l.Amount);
            var totalEquity = totalAssets - totalLiabilities;

            return new BalanceSheetReport
            {
                AsOfDate = asOfDate,
                Assets = assets,
                TotalAssets = totalAssets,
                Liabilities = liabilities,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity,
                GeneratedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Get Loan Portfolio Analysis by Status
        /// </summary>
        public async Task<LoanPortfolioAnalysis> GetLoanPortfolioAnalysisAsync()
        {
            var loans = await _context.Loans
                .Include(l => l.Account)
                .ToListAsync();

            var analysis = new LoanPortfolioAnalysis
            {
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.Status == "ACTIVE"),
                CompletedLoans = loans.Count(l => l.Status == "COMPLETED" || l.Status == "PAID"),
                DefaultedLoans = loans.Count(l => l.Status == "DEFAULTED"),
                OverdueLoans = loans.Count(l => l.Status == "OVERDUE"),
                TotalPrincipalDisbursed = loans.Sum(l => l.LoanAmount),
                TotalOutstandingBalance = loans.Where(l => l.Status == "ACTIVE").Sum(l => l.OutstandingBalance),
                TotalCollected = loans.Sum(l => l.LoanAmount - l.OutstandingBalance),
                LoansByType = loans.GroupBy(l => l.LoanType)
                    .Select(g => new LoanTypeBreakdown
                    {
                        LoanType = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(l => l.LoanAmount),
                        OutstandingBalance = g.Sum(l => l.OutstandingBalance)
                    })
                    .ToList(),
                LoansByStatus = loans.GroupBy(l => l.Status)
                    .Select(g => new LoanStatusBreakdown
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(l => l.LoanAmount),
                        OutstandingBalance = g.Sum(l => l.OutstandingBalance)
                    })
                    .ToList(),
                GeneratedAt = DateTime.Now
            };

            return analysis;
        }

        /// <summary>
        /// Generate comprehensive dashboard metrics
        /// </summary>
        public async Task<FinancialDashboardMetrics> GetFinancialDashboardMetricsAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var incomeStatement = await GenerateIncomeStatementWithLoansAsync(monthStart, monthEnd);
            var balanceSheet = await GenerateBalanceSheetWithLoansAsync(today);
            var loanPortfolio = await GetLoanPortfolioAnalysisAsync();

            return new FinancialDashboardMetrics
            {
                CurrentMonthRevenue = incomeStatement.TotalRevenue,
                CurrentMonthExpenses = incomeStatement.TotalExpenses,
                CurrentMonthNetIncome = incomeStatement.NetIncome,
                TotalAssets = balanceSheet.TotalAssets,
                TotalLiabilities = balanceSheet.TotalLiabilities,
                TotalEquity = balanceSheet.TotalEquity,
                TotalLoansPortfolio = loanPortfolio.TotalPrincipalDisbursed,
                OutstandingLoansBalance = loanPortfolio.TotalOutstandingBalance,
                ActiveLoansCount = loanPortfolio.ActiveLoans,
                CompletedLoansCount = loanPortfolio.CompletedLoans,
                DefaultedLoansCount = loanPortfolio.DefaultedLoans,
                GeneratedAt = DateTime.Now
            };
        }

        #endregion
    }

    #region View Models and Report Classes

    public class ChartOfAccountsViewModel
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string NormalBalance { get; set; } = "";
        public decimal CurrentBalance { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class IncomeStatementReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal InterestIncome { get; set; }
        public decimal PenaltyIncome { get; set; }
        public decimal ServiceFeeIncome { get; set; }
        public List<RevenueLineItem> OtherRevenues { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public List<ExpenseLineItem> Expenses { get; set; } = new();
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class RevenueLineItem
    {
        public string AccountName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class ExpenseLineItem
    {
        public string AccountName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class BalanceSheetReport
    {
        public DateTime AsOfDate { get; set; }
        public List<AssetLineItem> Assets { get; set; } = new();
        public decimal TotalAssets { get; set; }
        public List<LiabilityLineItem> Liabilities { get; set; } = new();
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class AssetLineItem
    {
        public string AccountName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class LiabilityLineItem
    {
        public string AccountName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class LoanPortfolioAnalysis
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
        public int DefaultedLoans { get; set; }
        public int OverdueLoans { get; set; }
        public decimal TotalPrincipalDisbursed { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalCollected { get; set; }
        public List<LoanTypeBreakdown> LoansByType { get; set; } = new();
        public List<LoanStatusBreakdown> LoansByStatus { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class LoanTypeBreakdown
    {
        public string LoanType { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class LoanStatusBreakdown
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class FinancialDashboardMetrics
    {
        public decimal CurrentMonthRevenue { get; set; }
        public decimal CurrentMonthExpenses { get; set; }
        public decimal CurrentMonthNetIncome { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalLoansPortfolio { get; set; }
        public decimal OutstandingLoansBalance { get; set; }
        public int ActiveLoansCount { get; set; }
        public int CompletedLoansCount { get; set; }
        public int DefaultedLoansCount { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion
}
