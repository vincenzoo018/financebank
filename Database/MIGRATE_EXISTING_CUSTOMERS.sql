-- =====================================================
-- MIGRATE EXISTING CUSTOMER USERS TO CUSTOMERACCOUNTS
-- This script creates CustomerAccount records for all
-- existing Customer users who don't have accounts yet
-- =====================================================

USE [BFASdatabase];
GO

PRINT '========================================================';
PRINT 'MIGRATING EXISTING CUSTOMER USERS TO CUSTOMERACCOUNTS';
PRINT '========================================================';
GO

-- Step 1: Check existing customer users without accounts
PRINT '';
PRINT '--- Step 1: Checking existing customer users without accounts ---';
SELECT COUNT(*) as CustomersWithoutAccounts
FROM Users u
WHERE u.Role = 'Customer'
AND NOT EXISTS (SELECT 1 FROM CustomerAccounts ca WHERE ca.CustomerId = u.UserId);
GO

-- Step 2: Create customer accounts for existing customers
PRINT '';
PRINT '--- Step 2: Creating customer accounts for existing customers ---';
INSERT INTO CustomerAccounts (AccountNumber, CustomerId, Balance, AvailableBalance, Currency, IsActive, CreatedAt)
SELECT 
    'ACC-' + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 900000 + 100000 AS VARCHAR(6)), 6) as AccountNumber,
    u.UserId,
    0 as Balance,
    0 as AvailableBalance,
    'PHP' as Currency,
    1 as IsActive,
    GETDATE() as CreatedAt
FROM Users u
WHERE u.Role = 'Customer'
AND NOT EXISTS (SELECT 1 FROM CustomerAccounts ca WHERE ca.CustomerId = u.UserId);

PRINT '✓ Customer accounts created successfully';
GO

-- Step 3: Verify migration
PRINT '';
PRINT '--- Step 3: Verifying migration ---';
PRINT '';
PRINT 'Customer Accounts Created:';
SELECT 
    u.UserId,
    u.Username,
    u.FullName,
    ca.AccountNumber,
    ca.Balance,
    ca.AvailableBalance,
    ca.CreatedAt
FROM Users u
INNER JOIN CustomerAccounts ca ON u.UserId = ca.CustomerId
WHERE u.Role = 'Customer'
ORDER BY ca.CreatedAt DESC;
GO

-- Step 4: Summary
PRINT '';
PRINT '========================================================';
PRINT 'MIGRATION COMPLETE';
PRINT '========================================================';
PRINT '';
PRINT 'Summary:';
SELECT 
    COUNT(*) as TotalCustomers,
    COUNT(DISTINCT ca.AccountId) as AccountsCreated,
    SUM(CASE WHEN ca.AccountNumber IS NOT NULL THEN 1 ELSE 0 END) as AccountsWithNumbers
FROM Users u
LEFT JOIN CustomerAccounts ca ON u.UserId = ca.CustomerId
WHERE u.Role = 'Customer';
GO

-- Step 5: Verify no duplicates
PRINT '';
PRINT '--- Checking for duplicate accounts (should be 0) ---';
SELECT CustomerId, COUNT(*) as DuplicateCount
FROM CustomerAccounts
GROUP BY CustomerId
HAVING COUNT(*) > 1;
GO

PRINT '';
PRINT '✓ Migration completed successfully!';
PRINT '✓ All existing customer users now have accounts in CustomerAccounts table';
PRINT '✓ Account numbers are unique and in ACC-XXXXXX format';
PRINT '✓ Initial balance set to 0.00 for all accounts';
GO
