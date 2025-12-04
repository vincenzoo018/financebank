# Quick Reference Guide - Financial Reporting Service

## How to Use the New Financial Services

### 1. Inject the Service

```csharp
@inject FinancialReportingService FinancialReporting
```

---

## 2. Common Usage Patterns

### A. Get Loan Portfolio Analysis (with Status from CustomerLoans)

```csharp
var portfolio = await FinancialReporting.GetLoanPortfolioAnalysisAsync();

// Access properties:
var activeCount = portfolio.ActiveLoans;           // Loans with Status = "ACTIVE"
var completedCount = portfolio.CompletedLoans;     // Status = "COMPLETED" or "PAID"
var defaultedCount = portfolio.DefaultedLoans;     // Status = "DEFAULTED"
var overdueCount = portfolio.OverdueLoans;         // Status = "OVERDUE"
var totalOutstanding = portfolio.TotalOutstandingBalance;

// Breakdown by Status
foreach (var status in portfolio.LoansByStatus)
{
    <p>@status.Status: @status.Count loans, ₱@status.TotalAmount.ToString("N2")</p>
}

// Breakdown by Type
foreach (var type in portfolio.LoansByType)
{
    <p>@type.LoanType: @type.Count loans, ₱@type.TotalAmount.ToString("N2")</p>
}
```

---

### B. Generate Income Statement with Loan Interest

```csharp
var startDate = new DateTime(2025, 1, 1);
var endDate = new DateTime(2025, 12, 31);

var incomeStmt = await FinancialReporting.GenerateIncomeStatementWithLoansAsync(
    startDate, 
    endDate
);

// Display revenue breakdown
<h3>Revenue</h3>
<p>Interest Income from Loans: ₱@incomeStmt.InterestIncome.ToString("N2")</p>
<p>Penalty Income: ₱@incomeStmt.PenaltyIncome.ToString("N2")</p>
<p>Service Fees: ₱@incomeStmt.ServiceFeeIncome.ToString("N2")</p>
<p>Total Revenue: ₱@incomeStmt.TotalRevenue.ToString("N2")</p>

// Display expenses
<h3>Expenses</h3>
@foreach (var expense in incomeStmt.Expenses)
{
    <p>@expense.AccountName: ₱@expense.Amount.ToString("N2")</p>
}
<p>Total Expenses: ₱@incomeStmt.TotalExpenses.ToString("N2")</p>

// Display net income
<h3>Net Income: ₱@incomeStmt.NetIncome.ToString("N2")</h3>
```

---

### C. Generate Balance Sheet with Loans Receivable

```csharp
var asOfDate = DateTime.Today;

var balanceSheet = await FinancialReporting.GenerateBalanceSheetWithLoansAsync(asOfDate);

// Display assets (includes Loans Receivable from CustomerLoans)
<h3>Assets</h3>
@foreach (var asset in balanceSheet.Assets)
{
    <p>@asset.AccountName: ₱@asset.Amount.ToString("N2")</p>
}
<p>Total Assets: ₱@balanceSheet.TotalAssets.ToString("N2")</p>

// Display liabilities
<h3>Liabilities</h3>
@foreach (var liability in balanceSheet.Liabilities)
{
    <p>@liability.AccountName: ₱@liability.Amount.ToString("N2")</p>
}
<p>Total Liabilities: ₱@balanceSheet.TotalLiabilities.ToString("N2")</p>

// Display equity
<h3>Equity: ₱@balanceSheet.TotalEquity.ToString("N2")</h3>
```

---

### D. Get Dashboard Metrics (All-in-One)

```csharp
var metrics = await FinancialReporting.GetFinancialDashboardMetricsAsync();

// Display comprehensive metrics
<div class="dashboard">
    <div class="metric-card">
        <h4>Current Month Revenue</h4>
        <p>₱@metrics.CurrentMonthRevenue.ToString("N2")</p>
    </div>
    
    <div class="metric-card">
        <h4>Net Income</h4>
        <p>₱@metrics.CurrentMonthNetIncome.ToString("N2")</p>
    </div>
    
    <div class="metric-card">
        <h4>Total Assets</h4>
        <p>₱@metrics.TotalAssets.ToString("N2")</p>
    </div>
    
    <div class="metric-card">
        <h4>Loans Portfolio</h4>
        <p>₱@metrics.TotalLoansPortfolio.ToString("N2")</p>
    </div>
    
    <div class="metric-card">
        <h4>Outstanding Loans</h4>
        <p>₱@metrics.OutstandingLoansBalance.ToString("N2")</p>
    </div>
    
    <div class="metric-card">
        <h4>Active Loans</h4>
        <p>@metrics.ActiveLoansCount loans</p>
    </div>
    
    <div class="metric-card">
        <h4>Defaulted Loans</h4>
        <p style="color: red;">@metrics.DefaultedLoansCount loans</p>
    </div>
</div>
```

---

### E. Get Chart of Accounts with Real-Time Balances

```csharp
var accounts = await FinancialReporting.GetChartOfAccountsWithBalancesAsync();

<table>
    <thead>
        <tr>
            <th>Code</th>
            <th>Name</th>
            <th>Type</th>
            <th>Balance</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var account in accounts)
        {
            <tr>
                <td>@account.AccountCode</td>
                <td>@account.AccountName</td>
                <td>@account.AccountType</td>
                <td style="text-align: right;">₱@account.CurrentBalance.ToString("N2")</td>
            </tr>
        }
    </tbody>
</table>
```

---

### F. Get Accounts Grouped by Type

```csharp
var accountsByType = await FinancialReporting.GetAccountsByTypeAsync();

<h3>Assets</h3>
@foreach (var account in accountsByType["Asset"])
{
    <p>@account.AccountName: ₱@account.CurrentBalance.ToString("N2")</p>
}

<h3>Liabilities</h3>
@foreach (var account in accountsByType["Liability"])
{
    <p>@account.AccountName: ₱@account.CurrentBalance.ToString("N2")</p>
}

<h3>Equity</h3>
@foreach (var account in accountsByType["Equity"])
{
    <p>@account.AccountName: ₱@account.CurrentBalance.ToString("N2")</p>
}

<h3>Revenue</h3>
@foreach (var account in accountsByType["Revenue"])
{
    <p>@account.AccountName: ₱@account.CurrentBalance.ToString("N2")</p>
}

<h3>Expenses</h3>
@foreach (var account in accountsByType["Expense"])
{
    <p>@account.AccountName: ₱@account.CurrentBalance.ToString("N2")</p>
}
```

---

## 3. Enhanced AccountingService Methods

### A. Get Accounts by Type

```csharp
@inject AccountingService AccountingService

var assetAccounts = await AccountingService.GetChartOfAccountsByTypeAsync("Asset");
var liabilityAccounts = await AccountingService.GetChartOfAccountsByTypeAsync("Liability");
```

### B. Get Account by Code

```csharp
var cashAccount = await AccountingService.GetChartOfAccountsByCodeAsync("1010");
if (cashAccount != null)
{
    <p>@cashAccount.AccountName: ₱@cashAccount.CurrentBalance.ToString("N2")</p>
}
```

### C. Get Balance Summary

```csharp
var summary = await AccountingService.GetAccountBalancesSummaryAsync();

<p>Total Assets: ₱@summary["Asset"].ToString("N2")</p>
<p>Total Liabilities: ₱@summary["Liability"].ToString("N2")</p>
<p>Total Equity: ₱@summary["Equity"].ToString("N2")</p>
<p>Total Revenue: ₱@summary["Revenue"].ToString("N2")</p>
<p>Total Expenses: ₱@summary["Expense"].ToString("N2")</p>
```

### D. Update Account Balance

```csharp
// Debit cash account (increase asset)
await AccountingService.UpdateAccountBalanceAsync("1010", 5000, true);

// Credit cash account (decrease asset)
await AccountingService.UpdateAccountBalanceAsync("1010", 3000, false);
```

---

## 4. Enhanced FinancialStatementService Methods

### A. Get Financial Summary

```csharp
@inject FinancialStatementService FinancialStatementService

var summary = await FinancialStatementService.GetFinancialSummaryAsync(DateTime.Today);

<div class="financial-summary">
    <p>Total Assets: ₱@summary["TotalAssets"].ToString("N2")</p>
    <p>Total Liabilities: ₱@summary["TotalLiabilities"].ToString("N2")</p>
    <p>Total Equity: ₱@summary["TotalEquity"].ToString("N2")</p>
    <p>Total Revenue: ₱@summary["TotalRevenue"].ToString("N2")</p>
    <p>Total Expenses: ₱@summary["TotalExpenses"].ToString("N2")</p>
    <p>Net Income: ₱@summary["NetIncome"].ToString("N2")</p>
</div>
```

### B. Get Loan Portfolio Summary

```csharp
var loanSummary = await FinancialStatementService.GetLoanPortfolioSummaryAsync();

<div class="loan-summary">
    <p>Active Loans: @loanSummary["ActiveLoansCount"]</p>
    <p>Active Principal: ₱@loanSummary["ActiveLoansPrincipal"]</p>
    <p>Outstanding Balance: ₱@loanSummary["OutstandingBalance"]</p>
    <p>Completed Loans: @loanSummary["CompletedLoansCount"]</p>
    <p>Overdue Loans: @loanSummary["OverdueLoansCount"]</p>
    <p>Overdue Balance: ₱@loanSummary["OverdueLoansBalance"]</p>
    <p>Total Portfolio: ₱@loanSummary["TotalLoansPortfolio"]</p>
</div>
```

### C. Generate Comprehensive Report

```csharp
var report = await FinancialStatementService.GenerateComprehensiveReportAsync(
    new DateTime(2025, 1, 1),
    new DateTime(2025, 12, 31),
    "Accountant"
);

// Access all components
var incomeStatement = report["IncomeStatement"];
var balanceSheet = report["BalanceSheet"];
var cashFlow = report["CashFlowStatement"];
var summary = report["FinancialSummary"];
var portfolio = report["LoanPortfolio"];
```

---

## 5. Loan Recording Helper Methods

### Already implemented in LoanRecording.razor:

```csharp
// Get styled badge for loan type
var style = GetLoanTypeStyle("Personal");  // Returns CSS style string

// Get styled badge for loan status (from CustomerLoans.Status)
var statusStyle = GetLoanStatusStyle("ACTIVE");  // Returns CSS style string
```

### Supported Loan Types:
- `Personal` → Yellow badge
- `Home` → Blue badge
- `Auto` → Indigo badge
- `Education` → Purple badge
- `Business` → Pink badge

### Supported Loan Statuses (from CustomerLoans table):
- `ACTIVE` → Green badge
- `COMPLETED` → Blue badge
- `PAID` → Green badge
- `DEFAULTED` → Red badge
- `OVERDUE` → Yellow badge
- `PENDING` → Indigo badge
- `CLOSED` → Gray badge

---

## 6. Complete Example: Financial Dashboard Component

```csharp
@page "/accountant/financial-dashboard"
@inject FinancialReportingService FinancialReporting
@inject AccountingService AccountingService

<h1>Financial Dashboard</h1>

@if (metrics != null)
{
    <!-- Summary Cards -->
    <div class="metrics-grid">
        <div class="card">
            <h3>Current Month Revenue</h3>
            <p class="amount">₱@metrics.CurrentMonthRevenue.ToString("N2")</p>
        </div>
        
        <div class="card">
            <h3>Net Income</h3>
            <p class="amount">₱@metrics.CurrentMonthNetIncome.ToString("N2")</p>
        </div>
        
        <div class="card">
            <h3>Total Assets</h3>
            <p class="amount">₱@metrics.TotalAssets.ToString("N2")</p>
        </div>
        
        <div class="card">
            <h3>Active Loans</h3>
            <p class="count">@metrics.ActiveLoansCount</p>
            <p class="amount">₱@metrics.OutstandingLoansBalance.ToString("N2")</p>
        </div>
    </div>

    <!-- Loan Portfolio Analysis -->
    <h2>Loan Portfolio</h2>
    @if (portfolio != null)
    {
        <table>
            <thead>
                <tr>
                    <th>Status</th>
                    <th>Count</th>
                    <th>Total Amount</th>
                    <th>Outstanding</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var status in portfolio.LoansByStatus)
                {
                    <tr>
                        <td>@status.Status</td>
                        <td>@status.Count</td>
                        <td>₱@status.TotalAmount.ToString("N2")</td>
                        <td>₱@status.OutstandingBalance.ToString("N2")</td>
                    </tr>
                }
            </tbody>
        </table>
    }
}

@code {
    private FinancialDashboardMetrics? metrics;
    private LoanPortfolioAnalysis? portfolio;

    protected override async Task OnInitializedAsync()
    {
        metrics = await FinancialReporting.GetFinancialDashboardMetricsAsync();
        portfolio = await FinancialReporting.GetLoanPortfolioAnalysisAsync();
    }
}
```

---

## 7. Tips & Best Practices

### ✅ DO:
- Use `FinancialReportingService` for comprehensive reports
- Use `AccountingService` for basic Chart of Accounts operations
- Check for null values before displaying data
- Format currency with `.ToString("N2")`
- Cache results when displaying multiple times

### ❌ DON'T:
- Don't call services multiple times for same data
- Don't modify returned collections directly
- Don't forget to handle exceptions
- Don't display raw decimal values without formatting

---

**Happy Coding! 🚀**

