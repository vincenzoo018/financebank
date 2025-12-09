using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FinanceBank.Services;

/// <summary>
/// Service for generating teller reports with ERP accounting details
/// Supports daily, weekly, and monthly reports in PDF and Excel formats
/// </summary>
public class TellerReportService
{
    private readonly BFASDbContext _context;

    public TellerReportService(BFASDbContext context)
    {
        _context = context;
    }

    #region Dashboard Analytics Data

    /// <summary>
    /// Get comprehensive dashboard analytics for the teller
    /// </summary>
    public async Task<TellerDashboardAnalytics> GetDashboardAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today;
        var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

        var analytics = new TellerDashboardAnalytics
        {
            StartDate = start,
            EndDate = end,
            GeneratedAt = DateTime.Now
        };

        // Get all transactions within the date range
        var transactions = await _context.CustomerTransactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.CreatedAt >= start && t.CreatedAt <= end)
            .ToListAsync();

        // Get loan payments within the date range
        var loanPayments = await _context.LoanPayments
            .Include(p => p.Loan)
            .ThenInclude(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
            .ToListAsync();

        // Get loan disbursements within the date range
        var loanDisbursements = await _context.LoanDisbursals
            .Include(d => d.Loan)
            .ThenInclude(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Where(d => d.DisbursalDate >= start && d.DisbursalDate <= end)
            .ToListAsync();

        // Calculate deposit metrics
        var deposits = transactions.Where(t => t.TransactionType == "Deposit").ToList();
        analytics.TotalDeposits = deposits.Sum(t => t.Amount);
        analytics.DepositCount = deposits.Count;
        analytics.DepositFees = deposits.Sum(t => t.Fee);

        // Calculate withdrawal metrics
        var withdrawals = transactions.Where(t => t.TransactionType == "Withdrawal").ToList();
        analytics.TotalWithdrawals = withdrawals.Sum(t => t.Amount);
        analytics.WithdrawalCount = withdrawals.Count;
        analytics.WithdrawalFees = withdrawals.Sum(t => t.Fee);

        // Calculate loan disbursement metrics
        analytics.TotalLoanDisbursements = loanDisbursements.Sum(d => d.DisbursalAmount);
        analytics.LoanDisbursementCount = loanDisbursements.Count;

        // Calculate loan payment metrics
        analytics.TotalLoanPayments = loanPayments.Sum(p => p.PaymentAmount);
        analytics.LoanPaymentCount = loanPayments.Count;
        analytics.TotalPenaltiesCollected = loanPayments.Sum(p => p.PenaltyPaid);

        // Calculate savings account metrics
        analytics.SavingsAccountsOpened = await _context.SavingsAccounts
            .CountAsync(sa => sa.OpenedDate >= start && sa.OpenedDate <= end);

        // Calculate net cash flow
        analytics.NetCashFlow = analytics.TotalDeposits - analytics.TotalWithdrawals + analytics.TotalLoanPayments - analytics.TotalLoanDisbursements;

        // Calculate total fees/income
        analytics.TotalFeeIncome = analytics.DepositFees + analytics.WithdrawalFees + analytics.TotalPenaltiesCollected;

        // Total transaction count
        analytics.TotalTransactionCount = analytics.DepositCount + analytics.WithdrawalCount + analytics.LoanDisbursementCount + analytics.LoanPaymentCount;

        // Hourly breakdown for chart
        analytics.HourlyBreakdown = GetHourlyBreakdown(transactions, loanPayments, loanDisbursements);

        // Recent transactions
        analytics.RecentTransactions = await GetRecentTransactionsAsync(20);

        return analytics;
    }

    private List<HourlyTransactionData> GetHourlyBreakdown(
        List<CustomerTransaction> transactions,
        List<LoanPayment> loanPayments,
        List<LoanDisbursal> loanDisbursements)
    {
        var hourlyData = new List<HourlyTransactionData>();

        for (int hour = 8; hour <= 17; hour++) // 8 AM to 5 PM
        {
            var hourStart = DateTime.Today.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);

            var hourDeposits = transactions
                .Where(t => t.TransactionType == "Deposit" && t.CreatedAt >= hourStart && t.CreatedAt < hourEnd)
                .Sum(t => t.Amount);

            var hourWithdrawals = transactions
                .Where(t => t.TransactionType == "Withdrawal" && t.CreatedAt >= hourStart && t.CreatedAt < hourEnd)
                .Sum(t => t.Amount);

            var hourLoanPayments = loanPayments
                .Where(p => p.PaymentDate >= hourStart && p.PaymentDate < hourEnd)
                .Sum(p => p.PaymentAmount);

            var hourDisbursements = loanDisbursements
                .Where(d => d.DisbursalDate >= hourStart && d.DisbursalDate < hourEnd)
                .Sum(d => d.DisbursalAmount);

            hourlyData.Add(new HourlyTransactionData
            {
                Hour = hour > 12 ? $"{hour - 12}PM" : (hour == 12 ? "12PM" : $"{hour}AM"),
                Deposits = hourDeposits,
                Withdrawals = hourWithdrawals,
                LoanPayments = hourLoanPayments,
                LoanDisbursements = hourDisbursements
            });
        }

        return hourlyData;
    }

    #endregion

    #region Transaction History

    /// <summary>
    /// Get all teller transactions including loans, deposits, withdrawals
    /// </summary>
    public async Task<List<TellerTransactionRecord>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

        var records = new List<TellerTransactionRecord>();

        // Get regular transactions (deposits/withdrawals)
        var transactions = await _context.CustomerTransactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Customer)
            .Where(t => t.CreatedAt >= start && t.CreatedAt <= end)
            .Where(t => t.TransactionType == "Deposit" || t.TransactionType == "Withdrawal")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        foreach (var txn in transactions)
        {
            records.Add(new TellerTransactionRecord
            {
                TransactionId = txn.TransactionId,
                TransactionNumber = txn.TransactionNumber,
                TransactionType = txn.TransactionType,
                AccountNumber = txn.Account?.AccountNumber ?? "",
                CustomerName = txn.Account?.Customer?.FullName ?? "",
                Amount = txn.Amount,
                Fee = txn.Fee,
                Status = txn.Status,
                Description = txn.Description ?? "",
                Reference = txn.Reference ?? "",
                TransactionDate = txn.CreatedAt,
                ProcessedBy = "Teller" // Could be enhanced with actual employee name
            });
        }

        // Get loan payments
        var loanPayments = await _context.LoanPayments
            .Include(p => p.Loan)
            .ThenInclude(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        foreach (var payment in loanPayments)
        {
            records.Add(new TellerTransactionRecord
            {
                TransactionId = payment.PaymentId,
                TransactionNumber = $"LPAY-{payment.PaymentId:D6}",
                TransactionType = "Loan Payment",
                AccountNumber = payment.Loan?.Account?.AccountNumber ?? "",
                CustomerName = payment.Loan?.Account?.Customer?.FullName ?? "",
                Amount = payment.PaymentAmount,
                Fee = payment.PenaltyPaid,
                Status = "Completed",
                Description = $"Loan payment for {payment.Loan?.LoanNumber}",
                Reference = payment.Reference ?? "",
                TransactionDate = payment.PaymentDate,
                ProcessedBy = payment.ProcessedBy
            });
        }

        // Get loan disbursements
        var disbursements = await _context.LoanDisbursals
            .Include(d => d.Loan)
            .ThenInclude(l => l.Account)
            .ThenInclude(a => a.Customer)
            .Where(d => d.DisbursalDate >= start && d.DisbursalDate <= end)
            .OrderByDescending(d => d.DisbursalDate)
            .ToListAsync();

        foreach (var disbursal in disbursements)
        {
            records.Add(new TellerTransactionRecord
            {
                TransactionId = disbursal.DisbursalId,
                TransactionNumber = $"LREL-{disbursal.DisbursalId:D6}",
                TransactionType = "Loan Release",
                AccountNumber = disbursal.Loan?.Account?.AccountNumber ?? "",
                CustomerName = disbursal.Loan?.Account?.Customer?.FullName ?? "",
                Amount = disbursal.DisbursalAmount,
                Fee = 0,
                Status = disbursal.DisbursalStatus,
                Description = $"Loan disbursement: {disbursal.Loan?.LoanNumber}",
                Reference = disbursal.Reference ?? "",
                TransactionDate = disbursal.DisbursalDate,
                ProcessedBy = disbursal.ProcessedBy
            });
        }

        // Filter by type if specified
        if (!string.IsNullOrEmpty(transactionType))
        {
            records = records.Where(r => r.TransactionType == transactionType).ToList();
        }

        return records.OrderByDescending(r => r.TransactionDate).ToList();
    }

    /// <summary>
    /// Get recent transactions for dashboard
    /// </summary>
    public async Task<List<TellerTransactionRecord>> GetRecentTransactionsAsync(int count = 10)
    {
        var records = await GetAllTransactionsAsync(DateTime.Today.AddDays(-7), DateTime.Now);
        return records.Take(count).ToList();
    }

    #endregion

    #region Report Generation

    /// <summary>
    /// Generate CSV report data
    /// </summary>
    public async Task<string> GenerateCsvReportAsync(DateTime startDate, DateTime endDate, string reportType = "all")
    {
        var analytics = await GetDashboardAnalyticsAsync(startDate, endDate);
        var transactions = await GetAllTransactionsAsync(startDate, endDate);

        var sb = new StringBuilder();

        // Report Header
        sb.AppendLine("FINANCEBANK TELLER REPORT");
        sb.AppendLine($"Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Summary Section
        sb.AppendLine("=== SUMMARY ===");
        sb.AppendLine($"Total Transactions,{analytics.TotalTransactionCount}");
        sb.AppendLine();

        // Deposits Summary
        sb.AppendLine("--- DEPOSITS ---");
        sb.AppendLine($"Total Deposit Amount,₱{analytics.TotalDeposits:N2}");
        sb.AppendLine($"Deposit Count,{analytics.DepositCount}");
        sb.AppendLine($"Deposit Fees,₱{analytics.DepositFees:N2}");
        sb.AppendLine();

        // Withdrawals Summary
        sb.AppendLine("--- WITHDRAWALS ---");
        sb.AppendLine($"Total Withdrawal Amount,₱{analytics.TotalWithdrawals:N2}");
        sb.AppendLine($"Withdrawal Count,{analytics.WithdrawalCount}");
        sb.AppendLine($"Withdrawal Fees,₱{analytics.WithdrawalFees:N2}");
        sb.AppendLine();

        // Loan Disbursements Summary
        sb.AppendLine("--- LOAN DISBURSEMENTS ---");
        sb.AppendLine($"Total Disbursed,₱{analytics.TotalLoanDisbursements:N2}");
        sb.AppendLine($"Disbursement Count,{analytics.LoanDisbursementCount}");
        sb.AppendLine();

        // Loan Payments Summary
        sb.AppendLine("--- LOAN PAYMENTS ---");
        sb.AppendLine($"Total Payments Received,₱{analytics.TotalLoanPayments:N2}");
        sb.AppendLine($"Payment Count,{analytics.LoanPaymentCount}");
        sb.AppendLine($"Penalties Collected,₱{analytics.TotalPenaltiesCollected:N2}");
        sb.AppendLine();

        // ERP Accounting Summary
        sb.AppendLine("=== ERP ACCOUNTING SUMMARY ===");
        sb.AppendLine($"Net Cash Flow,₱{analytics.NetCashFlow:N2}");
        sb.AppendLine($"Total Fee Income,₱{analytics.TotalFeeIncome:N2}");
        sb.AppendLine($"Cash In (Deposits + Loan Payments),₱{(analytics.TotalDeposits + analytics.TotalLoanPayments):N2}");
        sb.AppendLine($"Cash Out (Withdrawals + Loan Disbursements),₱{(analytics.TotalWithdrawals + analytics.TotalLoanDisbursements):N2}");
        sb.AppendLine();

        // Transaction Details
        sb.AppendLine("=== TRANSACTION DETAILS ===");
        sb.AppendLine("Transaction Number,Type,Account Number,Customer Name,Amount,Fee,Status,Date,Processed By");

        foreach (var txn in transactions)
        {
            sb.AppendLine($"{txn.TransactionNumber},{txn.TransactionType},{txn.AccountNumber},{txn.CustomerName},₱{txn.Amount:N2},₱{txn.Fee:N2},{txn.Status},{txn.TransactionDate:yyyy-MM-dd HH:mm},{txn.ProcessedBy}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generate HTML report for PDF conversion
    /// </summary>
    public async Task<string> GenerateHtmlReportAsync(DateTime startDate, DateTime endDate, string reportType = "all")
    {
        var analytics = await GetDashboardAnalyticsAsync(startDate, endDate);
        var transactions = await GetAllTransactionsAsync(startDate, endDate);

        var sb = new StringBuilder();

        sb.AppendLine(@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Teller Report - FinanceBank</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; color: #333; }
        .header { background: linear-gradient(135deg, #059669, #10b981); color: white; padding: 20px; border-radius: 10px; margin-bottom: 30px; }
        .header h1 { margin: 0; font-size: 24px; }
        .header p { margin: 5px 0 0 0; opacity: 0.9; }
        .summary-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin-bottom: 30px; }
        .summary-card { background: #f8fafc; border-radius: 10px; padding: 15px; border-left: 4px solid #059669; }
        .summary-card.red { border-left-color: #dc2626; }
        .summary-card.blue { border-left-color: #3b82f6; }
        .summary-card.yellow { border-left-color: #f59e0b; }
        .summary-card h3 { margin: 0; font-size: 12px; color: #6b7280; text-transform: uppercase; }
        .summary-card .value { font-size: 24px; font-weight: 700; color: #1f2937; margin-top: 5px; }
        .summary-card .sub { font-size: 12px; color: #6b7280; }
        .section { margin-bottom: 30px; }
        .section h2 { font-size: 18px; color: #1f2937; border-bottom: 2px solid #e5e7eb; padding-bottom: 10px; }
        table { width: 100%; border-collapse: collapse; font-size: 12px; }
        th { background: #f8fafc; padding: 12px; text-align: left; font-weight: 600; color: #374151; border-bottom: 2px solid #e5e7eb; }
        td { padding: 10px 12px; border-bottom: 1px solid #e5e7eb; }
        .amount { font-weight: 600; }
        .deposit { color: #059669; }
        .withdrawal { color: #dc2626; }
        .loan-payment { color: #3b82f6; }
        .loan-release { color: #f59e0b; }
        .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; font-size: 11px; color: #6b7280; text-align: center; }
        .accounting-box { background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 10px; padding: 20px; margin-bottom: 30px; }
        .accounting-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #dcfce7; }
        .accounting-row:last-child { border-bottom: none; font-weight: 700; font-size: 16px; }
        @media print { body { margin: 20px; } }
    </style>
</head>
<body>");

        // Header
        sb.AppendLine($@"
    <div class='header'>
        <h1>📊 Teller Transaction Report</h1>
        <p>Report Period: {startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}</p>
        <p>Generated: {DateTime.Now:MMMM dd, yyyy HH:mm:ss}</p>
    </div>");

        // Summary Cards
        sb.AppendLine($@"
    <div class='summary-grid'>
        <div class='summary-card'>
            <h3>Total Deposits</h3>
            <div class='value'>₱{analytics.TotalDeposits:N2}</div>
            <div class='sub'>{analytics.DepositCount} transactions</div>
        </div>
        <div class='summary-card red'>
            <h3>Total Withdrawals</h3>
            <div class='value'>₱{analytics.TotalWithdrawals:N2}</div>
            <div class='sub'>{analytics.WithdrawalCount} transactions</div>
        </div>
        <div class='summary-card blue'>
            <h3>Loan Payments</h3>
            <div class='value'>₱{analytics.TotalLoanPayments:N2}</div>
            <div class='sub'>{analytics.LoanPaymentCount} payments</div>
        </div>
        <div class='summary-card yellow'>
            <h3>Loan Disbursements</h3>
            <div class='value'>₱{analytics.TotalLoanDisbursements:N2}</div>
            <div class='sub'>{analytics.LoanDisbursementCount} releases</div>
        </div>
    </div>");

        // ERP Accounting Summary
        sb.AppendLine($@"
    <div class='section'>
        <h2>💰 ERP Accounting Summary</h2>
        <div class='accounting-box'>
            <div class='accounting-row'>
                <span>Cash Inflows (Deposits)</span>
                <span class='deposit'>+ ₱{analytics.TotalDeposits:N2}</span>
            </div>
            <div class='accounting-row'>
                <span>Cash Inflows (Loan Payments)</span>
                <span class='deposit'>+ ₱{analytics.TotalLoanPayments:N2}</span>
            </div>
            <div class='accounting-row'>
                <span>Cash Outflows (Withdrawals)</span>
                <span class='withdrawal'>- ₱{analytics.TotalWithdrawals:N2}</span>
            </div>
            <div class='accounting-row'>
                <span>Cash Outflows (Loan Disbursements)</span>
                <span class='withdrawal'>- ₱{analytics.TotalLoanDisbursements:N2}</span>
            </div>
            <div class='accounting-row'>
                <span>Fee Income (Transaction Fees)</span>
                <span class='deposit'>+ ₱{(analytics.DepositFees + analytics.WithdrawalFees):N2}</span>
            </div>
            <div class='accounting-row'>
                <span>Fee Income (Loan Penalties)</span>
                <span class='deposit'>+ ₱{analytics.TotalPenaltiesCollected:N2}</span>
            </div>
            <div class='accounting-row'>
                <span><strong>Net Cash Flow</strong></span>
                <span style='color: {(analytics.NetCashFlow >= 0 ? "#059669" : "#dc2626")}'><strong>₱{analytics.NetCashFlow:N2}</strong></span>
            </div>
        </div>
    </div>");

        // Transaction Details Table
        sb.AppendLine(@"
    <div class='section'>
        <h2>📋 Transaction Details</h2>
        <table>
            <thead>
                <tr>
                    <th>Transaction #</th>
                    <th>Type</th>
                    <th>Account</th>
                    <th>Customer</th>
                    <th>Amount</th>
                    <th>Fee</th>
                    <th>Status</th>
                    <th>Date</th>
                </tr>
            </thead>
            <tbody>");

        foreach (var txn in transactions)
        {
            var typeClass = txn.TransactionType switch
            {
                "Deposit" => "deposit",
                "Withdrawal" => "withdrawal",
                "Loan Payment" => "loan-payment",
                "Loan Release" => "loan-release",
                _ => ""
            };

            var prefix = txn.TransactionType switch
            {
                "Deposit" or "Loan Payment" => "+",
                "Withdrawal" or "Loan Release" => "-",
                _ => ""
            };

            sb.AppendLine($@"
                <tr>
                    <td>{txn.TransactionNumber}</td>
                    <td>{txn.TransactionType}</td>
                    <td>{txn.AccountNumber}</td>
                    <td>{txn.CustomerName}</td>
                    <td class='amount {typeClass}'>{prefix}₱{txn.Amount:N2}</td>
                    <td>₱{txn.Fee:N2}</td>
                    <td>{txn.Status}</td>
                    <td>{txn.TransactionDate:yyyy-MM-dd HH:mm}</td>
                </tr>");
        }

        sb.AppendLine(@"
            </tbody>
        </table>
    </div>");

        // Footer
        sb.AppendLine($@"
    <div class='footer'>
        <p>This is an official document generated by FinanceBank System</p>
        <p>Total Transactions: {analytics.TotalTransactionCount} | Total Fee Income: ₱{analytics.TotalFeeIncome:N2}</p>
    </div>
</body>
</html>");

        return sb.ToString();
    }

    #endregion
}

#region Analytics Models

public class TellerDashboardAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }

    // Deposit Metrics
    public decimal TotalDeposits { get; set; }
    public int DepositCount { get; set; }
    public decimal DepositFees { get; set; }

    // Withdrawal Metrics
    public decimal TotalWithdrawals { get; set; }
    public int WithdrawalCount { get; set; }
    public decimal WithdrawalFees { get; set; }

    // Loan Disbursement Metrics
    public decimal TotalLoanDisbursements { get; set; }
    public int LoanDisbursementCount { get; set; }

    // Loan Payment Metrics
    public decimal TotalLoanPayments { get; set; }
    public int LoanPaymentCount { get; set; }
    public decimal TotalPenaltiesCollected { get; set; }

    // Savings Account Metrics
    public int SavingsAccountsOpened { get; set; }

    // Calculated Metrics
    public decimal NetCashFlow { get; set; }
    public decimal TotalFeeIncome { get; set; }
    public int TotalTransactionCount { get; set; }

    // Chart Data
    public List<HourlyTransactionData> HourlyBreakdown { get; set; } = new();

    // Recent Transactions
    public List<TellerTransactionRecord> RecentTransactions { get; set; } = new();
}

public class HourlyTransactionData
{
    public string Hour { get; set; } = "";
    public decimal Deposits { get; set; }
    public decimal Withdrawals { get; set; }
    public decimal LoanPayments { get; set; }
    public decimal LoanDisbursements { get; set; }
}

public class TellerTransactionRecord
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = "";
    public string TransactionType { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public string Status { get; set; } = "";
    public string Description { get; set; } = "";
    public string Reference { get; set; } = "";
    public DateTime TransactionDate { get; set; }
    public string ProcessedBy { get; set; } = "";
}

#endregion
