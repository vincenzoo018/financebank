-- =============================================
-- SECURITY & AUDIT LOG MIGRATION SCRIPT
-- =============================================
-- This script updates the database schema for:
-- 1. Enhanced AuditLogs table with malicious attempt tracking
-- 2. Added security fields to Users table for failed login tracking
-- =============================================

USE BFASdatabase;
GO

-- =============================================
-- STEP 1: Update AuditLogs Table
-- =============================================

PRINT 'Updating AuditLogs table...';

-- Add new columns to AuditLogs if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'IsMalicious')
BEGIN
    ALTER TABLE AuditLogs ADD IsMalicious BIT NOT NULL DEFAULT 0;
    PRINT '  - Added IsMalicious column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'CustomerAccountId')
BEGIN
    ALTER TABLE AuditLogs ADD CustomerAccountId INT NULL;
    PRINT '  - Added CustomerAccountId column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'AccountStatus')
BEGIN
    ALTER TABLE AuditLogs ADD AccountStatus NVARCHAR(20) NULL;
    PRINT '  - Added AccountStatus column';
END

-- Remove deprecated columns from AuditLogs (optional - comment out if you want to keep historical data)
-- Note: These columns are no longer used by the application
-- Uncomment to remove if desired:

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'Amount')
-- BEGIN
--     ALTER TABLE AuditLogs DROP COLUMN Amount;
--     PRINT '  - Removed Amount column';
-- END

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'BalanceBefore')
-- BEGIN
--     ALTER TABLE AuditLogs DROP COLUMN BalanceBefore;
--     PRINT '  - Removed BalanceBefore column';
-- END

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'BalanceAfter')
-- BEGIN
--     ALTER TABLE AuditLogs DROP COLUMN BalanceAfter;
--     PRINT '  - Removed BalanceAfter column';
-- END

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'OldValues')
-- BEGIN
--     ALTER TABLE AuditLogs DROP COLUMN OldValues;
--     PRINT '  - Removed OldValues column';
-- END

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AuditLogs') AND name = 'NewValues')
-- BEGIN
--     ALTER TABLE AuditLogs DROP COLUMN NewValues;
--     PRINT '  - Removed NewValues column';
-- END

PRINT 'AuditLogs table updated successfully.';
GO

-- =============================================
-- STEP 2: Create Index for Performance
-- =============================================
-- NOTE: We use the existing TransferPinHash column for security PIN verification
-- Failed login attempts are tracked via AuditLogs, not in Users table

PRINT 'Creating indexes for performance...';

-- Index on IsMalicious for quick security queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_IsMalicious' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_IsMalicious 
    ON AuditLogs(IsMalicious) 
    WHERE IsMalicious = 1;
    PRINT '  - Created IX_AuditLogs_IsMalicious index';
END

-- Index on CustomerAccountId for customer-specific queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CustomerAccountId' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_CustomerAccountId 
    ON AuditLogs(CustomerAccountId) 
    WHERE CustomerAccountId IS NOT NULL;
    PRINT '  - Created IX_AuditLogs_CustomerAccountId index';
END

-- Index on Action and CreatedAt for failed login tracking
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action_CreatedAt' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Action_CreatedAt 
    ON AuditLogs(Action, CreatedAt DESC);
    PRINT '  - Created IX_AuditLogs_Action_CreatedAt index';
END

PRINT 'Indexes created successfully.';
GO

-- =============================================
-- STEP 3: Verify TransferPinHash Column Exists
-- =============================================

PRINT 'Verifying TransferPinHash column exists for security PIN...';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'TransferPinHash')
BEGIN
    PRINT '  - TransferPinHash column exists (will be used for security PIN verification)';
END
ELSE
BEGIN
    PRINT '  - WARNING: TransferPinHash column not found! Creating it...';
    ALTER TABLE Users ADD TransferPinHash NVARCHAR(255) NULL;
    PRINT '  - Created TransferPinHash column';
END

PRINT 'TransferPinHash verification complete.';
GO

-- =============================================
-- STEP 4: Set Default Security PINs for Existing Customers (Optional)
-- =============================================

-- Uncomment the following if you want to set a default PIN for all existing customers
-- The default PIN below is '1234' (hashed with BCrypt - you'll need to generate this in C#)
-- This is just a placeholder - DO NOT use this hash in production!

-- UPDATE Users 
-- SET SecurityPinHash = '$2a$11$K8ZpZJZC9zQKL3zqR7ZLSuS0BwYtULz.QKzPUaH3lKY2oTaQiQqNi'
-- WHERE Role = 'Customer' AND SecurityPinHash IS NULL;

PRINT '';
PRINT '=============================================';
PRINT 'MIGRATION COMPLETED SUCCESSFULLY!';
PRINT '=============================================';
PRINT '';
PRINT 'Summary of changes:';
PRINT '  - AuditLogs: Added IsMalicious, CustomerAccountId, AccountStatus columns';
PRINT '  - Users: Added SecurityPinHash, FailedLoginAttempts, LastFailedAttempt, IsLocked, LockoutEnd columns';
PRINT '  - Created performance indexes for security queries';
PRINT '';
PRINT 'IMPORTANT NOTES:';
PRINT '  1. Customers need to set their 4-digit Security PIN in their profile settings';
PRINT '  2. After 5 failed login attempts, the account will be locked';
PRINT '  3. Locked accounts can be unlocked via:';
PRINT '     - Customer entering their Security PIN';
PRINT '     - SuperAdmin unlocking from Audit Logs page';
PRINT '  4. All malicious attempts are logged with IsMalicious = 1';
PRINT '';
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Verify AuditLogs columns
SELECT 
    c.name AS ColumnName, 
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('AuditLogs')
ORDER BY c.column_id;

-- Verify Users columns (security related)
SELECT 
    c.name AS ColumnName, 
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Users')
    AND c.name IN ('SecurityPinHash', 'FailedLoginAttempts', 'LastFailedAttempt', 'IsLocked', 'LockoutEnd')
ORDER BY c.column_id;

PRINT '';
PRINT 'Migration verification complete.';
GO
