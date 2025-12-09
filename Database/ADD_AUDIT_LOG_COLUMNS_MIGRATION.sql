-- =====================================================
-- AUDIT LOG TABLE ENHANCEMENT MIGRATION
-- Adds columns for comprehensive audit tracking
-- Run this script if columns don't exist in your database
-- =====================================================

USE BFASdatabase;
GO

PRINT 'Starting AuditLog table migration...';
GO

-- Check if OldValues column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'OldValues')
BEGIN
    PRINT 'Adding OldValues column...';
    ALTER TABLE [dbo].[AuditLogs]
    ADD [OldValues] NVARCHAR(4000) NULL;
    PRINT '✓ OldValues column added';
END
ELSE
BEGIN
    PRINT '✓ OldValues column already exists';
END
GO

-- Check if NewValues column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'NewValues')
BEGIN
    PRINT 'Adding NewValues column...';
    ALTER TABLE [dbo].[AuditLogs]
    ADD [NewValues] NVARCHAR(4000) NULL;
    PRINT '✓ NewValues column added';
END
ELSE
BEGIN
    PRINT '✓ NewValues column already exists';
END
GO

-- Check if Amount column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'Amount')
BEGIN
    PRINT 'Adding Amount column...';
    ALTER TABLE [dbo].[AuditLogs]
    ADD [Amount] DECIMAL(18,2) NULL;
    PRINT '✓ Amount column added';
END
ELSE
BEGIN
    PRINT '✓ Amount column already exists';
END
GO

-- Check if BalanceBefore column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'BalanceBefore')
BEGIN
    PRINT 'Adding BalanceBefore column...';
    ALTER TABLE [dbo].[AuditLogs]
    ADD [BalanceBefore] DECIMAL(18,2) NULL;
    PRINT '✓ BalanceBefore column added';
END
ELSE
BEGIN
    PRINT '✓ BalanceBefore column already exists';
END
GO

-- Check if BalanceAfter column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'BalanceAfter')
BEGIN
    PRINT 'Adding BalanceAfter column...';
    ALTER TABLE [dbo].[AuditLogs]
    ADD [BalanceAfter] DECIMAL(18,2) NULL;
    PRINT '✓ BalanceAfter column added';
END
ELSE
BEGIN
    PRINT '✓ BalanceAfter column already exists';
END
GO

-- Verify all columns exist
PRINT '';
PRINT '=== VERIFICATION ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AuditLogs'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '✅ AuditLog table migration completed successfully!';
PRINT '';
PRINT 'Your AuditLogs table now includes:';
PRINT '  ✓ OldValues (NVARCHAR(4000)) - Stores previous values';
PRINT '  ✓ NewValues (NVARCHAR(4000)) - Stores new values';
PRINT '  ✓ Amount (DECIMAL(18,2)) - Transaction amounts';
PRINT '  ✓ BalanceBefore (DECIMAL(18,2)) - Balance before transaction';
PRINT '  ✓ BalanceAfter (DECIMAL(18,2)) - Balance after transaction';
PRINT '';
PRINT 'Audit logging is now ready for comprehensive tracking!';
GO
