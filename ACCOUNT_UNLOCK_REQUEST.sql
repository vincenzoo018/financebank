-- =============================================
-- Account Unlock Request Table
-- For handling locked savings account unlock requests
-- Created: December 14, 2025
-- Updated: December 15, 2025
-- Note: This feature uses the existing SavingsAccounts.IsLocked 
--       and SavingsAccounts.Status columns for locking
-- =============================================

-- Create the AccountUnlockRequests table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AccountUnlockRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AccountUnlockRequests] (
        [RequestId] INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Customer Information
        [CustomerId] INT NOT NULL,                    -- FK to Users table (the account owner)
        [CustomerName] NVARCHAR(200) NOT NULL,
        [CustomerUsername] NVARCHAR(100) NOT NULL,    -- Stores the SavingsAccount.AccountNumber (e.g., SAV-XXXXXX)
        [CustomerEmail] NVARCHAR(200) NULL,
        [CustomerPhone] NVARCHAR(50) NULL,
        
        -- Request Details
        [LockReason] NVARCHAR(500) NOT NULL,          -- Why the account was locked
        [CustomerStatement] NVARCHAR(1000) NULL,      -- Customer's explanation or statement
        [IdentificationVerified] BIT NOT NULL DEFAULT 0, -- Did teller verify customer's ID?
        [IdentificationType] NVARCHAR(100) NULL,      -- Type of ID presented (e.g., "Driver's License", "Passport")
        [IdentificationNumber] NVARCHAR(100) NULL,    -- ID number (last 4 digits or masked)
        
        -- Status Tracking
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
        [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Normal', -- Low, Normal, High, Urgent
        
        -- Teller Information (who created the request)
        [RequestedByTellerId] INT NOT NULL,           -- FK to Users table (Teller)
        [RequestedByTellerName] NVARCHAR(200) NOT NULL,
        [RequestedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [TellerNotes] NVARCHAR(1000) NULL,
        
        -- Admin Processing
        [ProcessedByAdminId] INT NULL,                -- FK to Users table (Admin who processed)
        [ProcessedByAdminName] NVARCHAR(200) NULL,
        [ProcessedAt] DATETIME NULL,
        [AdminNotes] NVARCHAR(1000) NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        
        -- Audit Fields
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME NULL,
        
        -- Foreign Key Constraints
        CONSTRAINT [FK_AccountUnlockRequests_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Users]([UserId]),
        CONSTRAINT [FK_AccountUnlockRequests_Teller] FOREIGN KEY ([RequestedByTellerId]) REFERENCES [dbo].[Users]([UserId]),
        CONSTRAINT [FK_AccountUnlockRequests_Admin] FOREIGN KEY ([ProcessedByAdminId]) REFERENCES [dbo].[Users]([UserId])
    );
    
    PRINT 'AccountUnlockRequests table created successfully.';
END
ELSE
BEGIN
    PRINT 'AccountUnlockRequests table already exists.';
END
GO

-- Create indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountUnlockRequests_CustomerId' AND object_id = OBJECT_ID('AccountUnlockRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AccountUnlockRequests_CustomerId] ON [dbo].[AccountUnlockRequests]([CustomerId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountUnlockRequests_Status' AND object_id = OBJECT_ID('AccountUnlockRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AccountUnlockRequests_Status] ON [dbo].[AccountUnlockRequests]([Status]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountUnlockRequests_RequestedAt' AND object_id = OBJECT_ID('AccountUnlockRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AccountUnlockRequests_RequestedAt] ON [dbo].[AccountUnlockRequests]([RequestedAt] DESC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountUnlockRequests_AccountNumber' AND object_id = OBJECT_ID('AccountUnlockRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AccountUnlockRequests_AccountNumber] ON [dbo].[AccountUnlockRequests]([CustomerUsername]);
END
GO

PRINT '=============================================';
PRINT 'Account Unlock Request schema setup complete!';
PRINT '';
PRINT 'NOTE: This feature uses the existing columns in SavingsAccounts table:';
PRINT '  - SavingsAccounts.IsLocked (BIT) - indicates if account is locked';
PRINT '  - SavingsAccounts.Status (NVARCHAR) - "LOCKED" when account is locked';
PRINT '  - SavingsAccounts.Notes (NVARCHAR) - stores lock reason';
PRINT '';
PRINT 'To lock a savings account for testing:';
PRINT '  UPDATE SavingsAccounts SET IsLocked = 1, Status = ''LOCKED'', Notes = ''Test lock''';
PRINT '  WHERE AccountNumber = ''SAV-XXXXXX''';
PRINT '=============================================';
GO
