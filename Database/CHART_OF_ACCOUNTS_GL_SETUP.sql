-- =====================================================================
-- CHART OF ACCOUNTS - GL POSTING REQUIRED ACCOUNTS
-- Run this script to ensure all required accounts exist for automatic GL posting
-- Matches actual table schema: AccountId, AccountCode, AccountName, AccountType, 
--                              ParentAccountCode, Level, IsActive, CreatedAt, CreatedBy
-- =====================================================================

-- =====================================================================
-- ASSET ACCOUNTS (1xxx)
-- =====================================================================

-- 1010: Cash on Hand
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1010')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('1010', 'Cash on Hand', 'Asset', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 1010: Cash on Hand';
END
ELSE
    PRINT 'Account 1010: Cash on Hand already exists';

-- 1100: Cash in Bank
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1100')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('1100', 'Cash in Bank', 'Asset', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 1100: Cash in Bank';
END
ELSE
    PRINT 'Account 1100: Cash in Bank already exists';

-- 1110: Loans Receivable
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1110')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('1110', 'Loans Receivable', 'Asset', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 1110: Loans Receivable';
END
ELSE
    PRINT 'Account 1110: Loans Receivable already exists';

-- 1200: Accounts Receivable
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '1200')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('1200', 'Accounts Receivable', 'Asset', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 1200: Accounts Receivable';
END
ELSE
    PRINT 'Account 1200: Accounts Receivable already exists';

-- =====================================================================
-- LIABILITY ACCOUNTS (2xxx)
-- =====================================================================

-- 2000: Accounts Payable
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '2000')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('2000', 'Accounts Payable', 'Liability', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 2000: Accounts Payable';
END
ELSE
    PRINT 'Account 2000: Accounts Payable already exists';

-- 2010: Customer Deposits (Savings Liability)
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '2010')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('2010', 'Customer Deposits', 'Liability', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 2010: Customer Deposits';
END
ELSE
    PRINT 'Account 2010: Customer Deposits already exists';

-- =====================================================================
-- EQUITY ACCOUNTS (3xxx)
-- =====================================================================

-- 3000: Common Stock
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '3000')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('3000', 'Common Stock', 'Equity', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 3000: Common Stock';
END
ELSE
    PRINT 'Account 3000: Common Stock already exists';

-- 3100: Retained Earnings
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '3100')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('3100', 'Retained Earnings', 'Equity', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 3100: Retained Earnings';
END
ELSE
    PRINT 'Account 3100: Retained Earnings already exists';

-- =====================================================================
-- REVENUE ACCOUNTS (4xxx)
-- =====================================================================

-- 4010: Interest Income
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4010')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('4010', 'Interest Income', 'Revenue', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 4010: Interest Income';
END
ELSE
    PRINT 'Account 4010: Interest Income already exists';

-- 4020: Service Fee Income
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4020')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('4020', 'Service Fee Income', 'Revenue', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 4020: Service Fee Income';
END
ELSE
    PRINT 'Account 4020: Service Fee Income already exists';

-- 4030: Penalty Income
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4030')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('4030', 'Penalty Income', 'Revenue', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 4030: Penalty Income';
END
ELSE
    PRINT 'Account 4030: Penalty Income already exists';

-- 4040: Loan Processing Fee Income
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '4040')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('4040', 'Loan Processing Fee Income', 'Revenue', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 4040: Loan Processing Fee Income';
END
ELSE
    PRINT 'Account 4040: Loan Processing Fee Income already exists';

-- =====================================================================
-- EXPENSE ACCOUNTS (5xxx)
-- =====================================================================

-- 5000: Salaries Expense
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5000')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('5000', 'Salaries Expense', 'Expense', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 5000: Salaries Expense';
END
ELSE
    PRINT 'Account 5000: Salaries Expense already exists';

-- 5010: Bill Payment Expense
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5010')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('5010', 'Bill Payment Expense', 'Expense', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 5010: Bill Payment Expense';
END
ELSE
    PRINT 'Account 5010: Bill Payment Expense already exists';

-- 5020: Interest Expense
IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = '5020')
BEGIN
    INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountCode, [Level], IsActive, CreatedAt, CreatedBy)
    VALUES ('5020', 'Interest Expense', 'Expense', NULL, 1, 1, GETDATE(), 'System');
    PRINT 'Created account 5020: Interest Expense';
END
ELSE
    PRINT 'Account 5020: Interest Expense already exists';

-- =====================================================================
-- VERIFICATION
-- =====================================================================
PRINT '';
PRINT '=== Chart of Accounts Summary ===';

SELECT 
    AccountCode,
    AccountName,
    AccountType,
    IsActive
FROM ChartOfAccounts
WHERE AccountCode IN ('1010', '1100', '1110', '1200', '2000', '2010', '3000', '3100', '4010', '4020', '4030', '4040', '5000', '5010', '5020')
ORDER BY AccountCode;

PRINT '';
PRINT 'Chart of Accounts setup complete!';
PRINT 'These accounts are required for automatic General Ledger posting.';
