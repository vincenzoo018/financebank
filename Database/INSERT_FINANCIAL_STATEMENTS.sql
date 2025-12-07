-- Insert Sample Financial Statements
-- Run this script to populate the FinancialStatements table with test data

USE BFASdatabase;
GO

-- Clear existing test data (optional)
-- DELETE FROM FinancialStatements WHERE GeneratedBy = 'System';

-- Insert Income Statement for November 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'IncomeStatement',
    '2025-11-01',
    '2025-11-30',
    '{"Revenues":[{"Account":"Interest Income","Amount":125000.00},{"Account":"Service Fee Income","Amount":45000.00},{"Account":"Penalty Income","Amount":8500.00}],"TotalRevenue":178500.00,"Expenses":[{"Account":"Salary Expense","Amount":65000.00},{"Account":"Utilities Expense","Amount":12000.00},{"Account":"Office Supplies","Amount":5500.00}],"TotalExpenses":82500.00,"NetIncome":96000.00}',
    GETDATE(),
    'System'
);

-- Insert Income Statement for October 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'IncomeStatement',
    '2025-10-01',
    '2025-10-31',
    '{"Revenues":[{"Account":"Interest Income","Amount":118000.00},{"Account":"Service Fee Income","Amount":42000.00},{"Account":"Penalty Income","Amount":6200.00}],"TotalRevenue":166200.00,"Expenses":[{"Account":"Salary Expense","Amount":65000.00},{"Account":"Utilities Expense","Amount":11500.00},{"Account":"Office Supplies","Amount":4800.00}],"TotalExpenses":81300.00,"NetIncome":84900.00}',
    GETDATE(),
    'System'
);

-- Insert Balance Sheet for November 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'BalanceSheet',
    '2025-11-30',
    '2025-11-30',
    '{"Assets":[{"Account":"Cash on Hand","Balance":2500000.00},{"Account":"Loans Receivable","Balance":8750000.00},{"Account":"Office Equipment","Balance":350000.00}],"TotalAssets":11600000.00,"Liabilities":[{"Account":"Customer Deposits","Balance":9500000.00},{"Account":"Accounts Payable","Balance":125000.00}],"TotalLiabilities":9625000.00,"Equity":[{"Account":"Capital Stock","Balance":1500000.00}],"RetainedEarnings":475000.00,"TotalEquity":1975000.00}',
    GETDATE(),
    'System'
);

-- Insert Balance Sheet for October 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'BalanceSheet',
    '2025-10-31',
    '2025-10-31',
    '{"Assets":[{"Account":"Cash on Hand","Balance":2255000.00},{"Account":"Loans Receivable","Balance":8425000.00},{"Account":"Office Equipment","Balance":350000.00}],"TotalAssets":11030000.00,"Liabilities":[{"Account":"Customer Deposits","Balance":9125000.00},{"Account":"Accounts Payable","Balance":118000.00}],"TotalLiabilities":9243000.00,"Equity":[{"Account":"Capital Stock","Balance":1500000.00}],"RetainedEarnings":287000.00,"TotalEquity":1787000.00}',
    GETDATE(),
    'System'
);

-- Insert Cash Flow Statement for November 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'CashFlow',
    '2025-11-01',
    '2025-11-30',
    '{"OperatingActivities":{"CustomerDeposits":1250000.00,"CustomerWithdrawals":875000.00,"ServiceFees":45000.00,"LoanPaymentsReceived":325000.00,"NetCashFromOperating":745000.00},"FinancingActivities":{"LoanDisbursements":500000.00,"NetCashFromFinancing":-500000.00},"NetCashChange":245000.00}',
    GETDATE(),
    'System'
);

-- Insert Cash Flow Statement for October 2025
INSERT INTO FinancialStatements (StatementType, PeriodStart, PeriodEnd, StatementData, GeneratedAt, GeneratedBy)
VALUES (
    'CashFlow',
    '2025-10-01',
    '2025-10-31',
    '{"OperatingActivities":{"CustomerDeposits":1180000.00,"CustomerWithdrawals":920000.00,"ServiceFees":42000.00,"LoanPaymentsReceived":298000.00,"NetCashFromOperating":600000.00},"FinancingActivities":{"LoanDisbursements":450000.00,"NetCashFromFinancing":-450000.00},"NetCashChange":150000.00}',
    GETDATE(),
    'System'
);

-- Verify the inserts
SELECT StatementId, StatementType, PeriodStart, PeriodEnd, GeneratedAt, GeneratedBy 
FROM FinancialStatements 
ORDER BY GeneratedAt DESC;

PRINT 'Successfully inserted 6 financial statement records!';
GO
