-- ============================================
-- GL ACCOUNTS MIGRATION SCRIPT
-- Ensures all required GL accounts exist for automatic posting
-- Run this script to add missing accounts
-- ============================================

-- Check and create Chart of Accounts entries
INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '1010', 'Cash on Hand', 'Asset', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1010');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '1100', 'Cash in Bank', 'Asset', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1100');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '1110', 'Loans Receivable', 'Asset', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1110');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '1200', 'Accounts Receivable', 'Asset', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1200');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '2000', 'Accounts Payable', 'Liability', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '2000');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '2010', 'Customer Deposits', 'Liability', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '2010');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '4010', 'Interest Income', 'Revenue', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4010');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '4020', 'Service Fee Income', 'Revenue', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4020');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '4030', 'Penalty Income', 'Revenue', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4030');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '4040', 'Loan Processing Fee Income', 'Revenue', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4040');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5010', 'Bill Payment Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5010');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5020', 'Interest Expense (Savings)', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5020');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5030', 'Utilities Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5030');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5040', 'Rent Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5040');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5050', 'Office Supplies Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5050');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5060', 'Salaries and Wages Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5060');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5070', 'Maintenance Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5070');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5080', 'Insurance Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5080');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5090', 'Professional Fees Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5090');

INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, Level, IsActive, CreatedBy, CreatedAt)
SELECT '5100', 'Tax Expense', 'Expense', 1, 1, 'System', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5100');

-- Check and create GeneralLedger entries (for GL transactions)
INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1010', 'Cash on Hand', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1100', 'Cash in Bank', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1100');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1110', 'Loans Receivable', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1110');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1200', 'Accounts Receivable', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1200');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '2000', 'Accounts Payable', 'Liability', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '2000');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '2010', 'Customer Deposits', 'Liability', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '2010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4010', 'Interest Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4020', 'Service Fee Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4020');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4030', 'Penalty Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4030');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4040', 'Loan Processing Fee Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4040');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5010', 'Bill Payment Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5020', 'Interest Expense (Savings)', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5020');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5030', 'Utilities Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5030');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5040', 'Rent Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5040');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5050', 'Office Supplies Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5050');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5060', 'Salaries and Wages Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5060');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5070', 'Maintenance Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5070');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5080', 'Insurance Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5080');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5090', 'Professional Fees Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5090');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '5100', 'Tax Expense', 'Expense', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '5100');

-- Verify the results
SELECT 'ChartOfAccounts' as TableName, COUNT(*) as AccountCount FROM ChartOfAccounts WHERE IsActive = 1;
SELECT 'GeneralLedger' as TableName, COUNT(*) as AccountCount FROM GeneralLedger WHERE IsActive = 1;

PRINT 'GL Accounts Migration Completed Successfully';
