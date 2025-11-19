-- =====================================================
-- CUSTOMER ACCOUNTS TABLE UPDATE
-- Remove AccountType column (not needed)
-- Update account number format to 6-digit unique
-- =====================================================

USE [BFASdatabase];
GO

-- Step 1: Drop AccountType column if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'CustomerAccounts' AND COLUMN_NAME = 'AccountType')
BEGIN
    ALTER TABLE CustomerAccounts DROP COLUMN AccountType;
    PRINT '✓ AccountType column removed from CustomerAccounts';
END
ELSE
BEGIN
    PRINT '✓ AccountType column does not exist (already removed)';
END
GO

-- Step 2: Verify table structure
PRINT '';
PRINT '========== CUSTOMER ACCOUNTS TABLE STRUCTURE ==========';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CustomerAccounts'
ORDER BY ORDINAL_POSITION;
GO

-- Step 3: Verify existing accounts
PRINT '';
PRINT '========== EXISTING CUSTOMER ACCOUNTS ==========';
SELECT TOP 10 
    AccountId,
    AccountNumber,
    CustomerId,
    Balance,
    AvailableBalance,
    Currency,
    IsActive,
    CreatedAt
FROM CustomerAccounts
ORDER BY CreatedAt DESC;
GO

-- Step 4: Summary
PRINT '';
PRINT '========== UPDATE COMPLETE ==========';
PRINT '✓ AccountType column removed';
PRINT '✓ Table ready for new account creation';
PRINT '✓ Account numbers will be auto-generated as ACC-XXXXXX (6 digits)';
PRINT '✓ Initial balance for new accounts: 0.00';
GO
