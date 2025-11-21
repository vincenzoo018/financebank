-- ============================================================================
-- MIGRATION: Add ProcessedByEmployeeId Column to CustomerTransactions
-- ============================================================================
-- This script adds the ProcessedByEmployeeId column (FK to Employees table)
-- to track which teller/employee processed the transaction
-- The employee name can be retrieved by joining with the Employees table

USE [BFASdatabase];
GO

-- Check if column exists before adding it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerTransactions' AND COLUMN_NAME = 'ProcessedByEmployeeId')
BEGIN
    ALTER TABLE [CustomerTransactions]
    ADD [ProcessedByEmployeeId] INT NULL;
    PRINT '✓ Added ProcessedByEmployeeId column';
END
ELSE
BEGIN
    PRINT '✓ ProcessedByEmployeeId column already exists';
END
GO

-- Remove old ProcessedBy and ProcessedByEmployeeName columns if they exist
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerTransactions' AND COLUMN_NAME = 'ProcessedBy')
BEGIN
    ALTER TABLE [CustomerTransactions]
    DROP COLUMN [ProcessedBy];
    PRINT '✓ Removed ProcessedBy column (replaced by ProcessedByEmployeeId)';
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerTransactions' AND COLUMN_NAME = 'ProcessedByEmployeeName')
BEGIN
    ALTER TABLE [CustomerTransactions]
    DROP COLUMN [ProcessedByEmployeeName];
    PRINT '✓ Removed ProcessedByEmployeeName column (use JOIN with Employees table instead)';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '========================================================';
PRINT 'MIGRATION COMPLETE - CustomerTransactions Table Structure';
PRINT '========================================================';
PRINT '';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CustomerTransactions'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '✓ Migration completed successfully!';
GO
