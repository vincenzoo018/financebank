-- =============================================
-- Security and ERP Enhancement Migration
-- Adds AccountingEntries and AuditLogs tables
-- Updates User table password field to support longer hashes
-- =============================================

USE BFASdatabase;
GO

-- Create AccountingEntries table for double-entry bookkeeping
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AccountingEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AccountingEntries] (
        [EntryId] INT IDENTITY(1,1) PRIMARY KEY,
        [AccountId] INT NULL,
        [TransactionType] NVARCHAR(50) NOT NULL,
        [AccountType] NVARCHAR(50) NOT NULL,
        [DebitAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [CreditAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Reference] NVARCHAR(100) NOT NULL DEFAULT '',
        [Description] NVARCHAR(500) NOT NULL DEFAULT '',
        [ProcessedBy] NVARCHAR(50) NOT NULL DEFAULT '',
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_AccountingEntries_CustomerAccounts] FOREIGN KEY ([AccountId]) 
            REFERENCES [dbo].[CustomerAccounts]([AccountId]) ON DELETE SET NULL
    );

    CREATE INDEX [IX_AccountingEntries_AccountId] ON [dbo].[AccountingEntries]([AccountId]);
    CREATE INDEX [IX_AccountingEntries_Reference] ON [dbo].[AccountingEntries]([Reference]);
    CREATE INDEX [IX_AccountingEntries_CreatedAt] ON [dbo].[AccountingEntries]([CreatedAt]);
    
    PRINT '✓ AccountingEntries table created successfully';
END
ELSE
BEGIN
    PRINT '⚠ AccountingEntries table already exists';
END
GO

-- Create AuditLogs table if it doesn't exist or update if needed
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [AuditId] INT IDENTITY(1,1) PRIMARY KEY,
        [AccountId] INT NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [ActionType] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [BalanceBefore] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [BalanceAfter] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Success] BIT NOT NULL DEFAULT 1,
        [Details] NVARCHAR(1000) NOT NULL DEFAULT '',
        [PerformedBy] NVARCHAR(50) NOT NULL DEFAULT '',
        [IpAddress] NVARCHAR(50) NOT NULL DEFAULT '',
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_AuditLogs_CustomerAccounts] FOREIGN KEY ([AccountId]) 
            REFERENCES [dbo].[CustomerAccounts]([AccountId]) ON DELETE SET NULL
    );

    CREATE INDEX [IX_AuditLogs_AccountId] ON [dbo].[AuditLogs]([AccountId]);
    CREATE INDEX [IX_AuditLogs_Action] ON [dbo].[AuditLogs]([Action]);
    CREATE INDEX [IX_AuditLogs_PerformedBy] ON [dbo].[AuditLogs]([PerformedBy]);
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs]([CreatedAt]);
    CREATE INDEX [IX_AuditLogs_Success] ON [dbo].[AuditLogs]([Success]);
    
    PRINT '✓ AuditLogs table created successfully';
END
ELSE
BEGIN
    PRINT '⚠ AuditLogs table already exists';
    
    -- Check if we need to add new columns
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'Amount')
    BEGIN
        ALTER TABLE [dbo].[AuditLogs] ADD [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Added Amount column to AuditLogs';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'BalanceBefore')
    BEGIN
        ALTER TABLE [dbo].[AuditLogs] ADD [BalanceBefore] DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Added BalanceBefore column to AuditLogs';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'BalanceAfter')
    BEGIN
        ALTER TABLE [dbo].[AuditLogs] ADD [BalanceAfter] DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Added BalanceAfter column to AuditLogs';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'IpAddress')
    BEGIN
        ALTER TABLE [dbo].[AuditLogs] ADD [IpAddress] NVARCHAR(50) NOT NULL DEFAULT '';
        PRINT '✓ Added IpAddress column to AuditLogs';
    END
END
GO

-- Update Users table to support longer password hashes (BCrypt needs 60+ characters)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    -- Check current length of PasswordHash column
    DECLARE @CurrentLength INT;
    SELECT @CurrentLength = MAX_LENGTH 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordHash';
    
    IF @CurrentLength < 255
    BEGIN
        ALTER TABLE [dbo].[Users] ALTER COLUMN [PasswordHash] NVARCHAR(255) NOT NULL;
        PRINT '✓ Users.PasswordHash column updated to NVARCHAR(255) for BCrypt support';
    END
    ELSE
    BEGIN
        PRINT '⚠ Users.PasswordHash already supports long hashes';
    END
END
GO

-- Update CustomerAccounts table to add Status column if it doesn't exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerAccounts]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CustomerAccounts]') AND name = 'Status')
    BEGIN
        ALTER TABLE [dbo].[CustomerAccounts] ADD [Status] NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE';
        PRINT '✓ Added Status column to CustomerAccounts';
    END
    ELSE
    BEGIN
        PRINT '⚠ CustomerAccounts.Status column already exists';
    END
END
GO

-- Create sample data for testing (optional)
PRINT '';
PRINT '==============================================';
PRINT 'Security and ERP Enhancement Migration Complete';
PRINT '==============================================';
PRINT '';
PRINT 'New Tables Created:';
PRINT '  ✓ AccountingEntries - Double-entry bookkeeping';
PRINT '  ✓ AuditLogs - Comprehensive audit trail';
PRINT '';
PRINT 'Updated Tables:';
PRINT '  ✓ Users - Password field updated for BCrypt';
PRINT '  ✓ CustomerAccounts - Status field added';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run: dotnet restore (to install BCrypt.Net-Next package)';
PRINT '  2. Run: dotnet build';
PRINT '  3. All passwords will now be hashed with BCrypt';
PRINT '  4. All transactions will create accounting entries';
PRINT '  5. All activities will be audit logged';
PRINT '';
GO
