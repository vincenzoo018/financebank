-- =============================================
-- SECURITY ENHANCEMENT: Add Security Columns to AuditLogs Table
-- Date: December 10, 2025
-- Description: Adds columns for malicious attempt tracking and account security
-- =============================================

USE [BFASdatabase]
GO

-- Add IsMalicious column to flag malicious login attempts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'IsMalicious')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [IsMalicious] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsMalicious column to AuditLogs table';
END
GO

-- Add CustomerAccountId column to link audit logs to customer accounts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'CustomerAccountId')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [CustomerAccountId] INT NULL;
    PRINT 'Added CustomerAccountId column to AuditLogs table';
END
GO

-- Add AccountStatus column to track account status at time of event
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'AccountStatus')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [AccountStatus] VARCHAR(50) NULL;
    PRINT 'Added AccountStatus column to AuditLogs table';
END
GO

-- Add FailedAttemptCount column to track consecutive failed attempts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'FailedAttemptCount')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [FailedAttemptCount] INT NULL DEFAULT 0;
    PRINT 'Added FailedAttemptCount column to AuditLogs table';
END
GO

-- Add PinAttemptCount column to track PIN verification attempts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'PinAttemptCount')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [PinAttemptCount] INT NULL DEFAULT 0;
    PRINT 'Added PinAttemptCount column to AuditLogs table';
END
GO

-- Add IsAccountLocked column to indicate if account was locked
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'IsAccountLocked')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [IsAccountLocked] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsAccountLocked column to AuditLogs table';
END
GO

-- Add LockReason column to track why account was locked
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND name = 'LockReason')
BEGIN
    ALTER TABLE [dbo].[AuditLogs]
    ADD [LockReason] VARCHAR(255) NULL;
    PRINT 'Added LockReason column to AuditLogs table';
END
GO

-- Create index for faster querying of malicious attempts
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_IsMalicious' AND object_id = OBJECT_ID('dbo.AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_IsMalicious] 
    ON [dbo].[AuditLogs] ([IsMalicious]) 
    INCLUDE ([UserId], [Action], [CreatedAt], [CustomerAccountId]);
    PRINT 'Created index IX_AuditLogs_IsMalicious';
END
GO

-- Create index for customer account lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CustomerAccountId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_CustomerAccountId] 
    ON [dbo].[AuditLogs] ([CustomerAccountId]) 
    INCLUDE ([UserId], [Action], [CreatedAt], [IsMalicious]);
    PRINT 'Created index IX_AuditLogs_CustomerAccountId';
END
GO

-- Create index for locked accounts
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_IsAccountLocked' AND object_id = OBJECT_ID('dbo.AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_IsAccountLocked] 
    ON [dbo].[AuditLogs] ([IsAccountLocked]) 
    WHERE [IsAccountLocked] = 1;
    PRINT 'Created index IX_AuditLogs_IsAccountLocked';
END
GO

PRINT '========================================';
PRINT 'AuditLogs Security Columns Added Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'New Columns Added:';
PRINT '  - IsMalicious (BIT) - Flags malicious login attempts';
PRINT '  - CustomerAccountId (INT) - Links to customer account';
PRINT '  - AccountStatus (VARCHAR) - Tracks account status';
PRINT '  - FailedAttemptCount (INT) - Tracks consecutive failed attempts';
PRINT '  - PinAttemptCount (INT) - Tracks PIN verification attempts';
PRINT '  - IsAccountLocked (BIT) - Indicates if account was locked';
PRINT '  - LockReason (VARCHAR) - Reason for account lock';
PRINT '';
PRINT 'Security Rules:';
PRINT '  - 5 failed password attempts = Show PIN verification modal';
PRINT '  - 2 failed PIN attempts = Lock account (requires SuperAdmin)';
PRINT '  - Failed attempt counter resets after successful login';
PRINT '  - Failed attempt counter resets after 24 hours';
GO
