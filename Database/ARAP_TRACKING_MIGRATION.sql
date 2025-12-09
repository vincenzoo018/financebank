-- =============================================
-- AR/AP TRACKING SYSTEM MIGRATION
-- =============================================
-- This migration adds unified transaction history 
-- and enhanced AR/AP tracking for the banking system
-- =============================================
-- Run Date: 2024-12-10
-- =============================================

USE BFASdatabase;
GO

-- =============================================
-- STEP 1: Add new columns to AccountsPayable
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'TransactionType')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [TransactionType] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'SourceTransactionId')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [SourceTransactionId] INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'SourceTable')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [SourceTable] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'CustomerAccountId')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [CustomerAccountId] INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'ReferenceNumber')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [ReferenceNumber] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'ReviewStatus')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [ReviewStatus] NVARCHAR(50) NULL DEFAULT 'Pending';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'ReviewedBy')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [ReviewedBy] NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'ReviewedAt')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [ReviewedAt] DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'ReviewNotes')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [ReviewNotes] NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsPayable]') AND name = 'JournalEntryId')
BEGIN
    ALTER TABLE [dbo].[AccountsPayable]
    ADD [JournalEntryId] INT NULL;
END
GO

-- =============================================
-- STEP 2: Add new columns to AccountsReceivable
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'TransactionType')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [TransactionType] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'SourceTransactionId')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [SourceTransactionId] INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'SourceTable')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [SourceTable] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'CustomerAccountId')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [CustomerAccountId] INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'ReferenceNumber')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [ReferenceNumber] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'ReviewStatus')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [ReviewStatus] NVARCHAR(50) NULL DEFAULT 'Pending';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'ReviewedBy')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [ReviewedBy] NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'ReviewedAt')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [ReviewedAt] DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'ReviewNotes')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [ReviewNotes] NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccountsReceivable]') AND name = 'JournalEntryId')
BEGIN
    ALTER TABLE [dbo].[AccountsReceivable]
    ADD [JournalEntryId] INT NULL;
END
GO

-- =============================================
-- STEP 3: Create UnifiedTransactionHistory table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UnifiedTransactionHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UnifiedTransactionHistory] (
        [TransactionHistoryId] INT IDENTITY(1,1) PRIMARY KEY,
        [ReferenceNumber] NVARCHAR(50) NOT NULL,
        [TransactionType] NVARCHAR(50) NOT NULL,
        [ARAPClassification] NVARCHAR(10) NOT NULL, -- AR or AP
        [Amount] DECIMAL(18,2) NOT NULL,
        [FeeAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [InterestAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PenaltyAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [CustomerAccountId] INT NULL,
        [AccountNumber] NVARCHAR(50) NULL,
        [CustomerName] NVARCHAR(200) NULL,
        [SecondaryAccountId] INT NULL,
        [SecondaryAccountNumber] NVARCHAR(50) NULL,
        [SourceTable] NVARCHAR(50) NULL,
        [SourceRecordId] INT NULL,
        [AccountsReceivableId] INT NULL,
        [AccountsPayableId] INT NULL,
        [JournalEntryId] INT NULL,
        [Description] NVARCHAR(500) NULL,
        [TransactionMethod] NVARCHAR(50) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Completed',
        [ProcessedBy] NVARCHAR(100) NULL,
        [ProcessedByEmployeeId] INT NULL,
        [TransactionDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [Notes] NVARCHAR(500) NULL,

        -- Foreign Keys
        CONSTRAINT FK_UnifiedTransactionHistory_CustomerAccount 
            FOREIGN KEY ([CustomerAccountId]) REFERENCES [CustomerAccounts]([AccountId]),
        CONSTRAINT FK_UnifiedTransactionHistory_SecondaryAccount 
            FOREIGN KEY ([SecondaryAccountId]) REFERENCES [CustomerAccounts]([AccountId]),
        CONSTRAINT FK_UnifiedTransactionHistory_AR 
            FOREIGN KEY ([AccountsReceivableId]) REFERENCES [AccountsReceivable]([ReceivableId]),
        CONSTRAINT FK_UnifiedTransactionHistory_AP 
            FOREIGN KEY ([AccountsPayableId]) REFERENCES [AccountsPayable]([PayableId]),
        CONSTRAINT FK_UnifiedTransactionHistory_Journal 
            FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries]([JournalId])
    );

    -- Create indexes for better query performance
    CREATE INDEX IX_UnifiedTransactionHistory_ReferenceNumber ON [UnifiedTransactionHistory]([ReferenceNumber]);
    CREATE INDEX IX_UnifiedTransactionHistory_TransactionType ON [UnifiedTransactionHistory]([TransactionType]);
    CREATE INDEX IX_UnifiedTransactionHistory_ARAPClassification ON [UnifiedTransactionHistory]([ARAPClassification]);
    CREATE INDEX IX_UnifiedTransactionHistory_CustomerAccountId ON [UnifiedTransactionHistory]([CustomerAccountId]);
    CREATE INDEX IX_UnifiedTransactionHistory_TransactionDate ON [UnifiedTransactionHistory]([TransactionDate]);
    CREATE INDEX IX_UnifiedTransactionHistory_Status ON [UnifiedTransactionHistory]([Status]);
    CREATE INDEX IX_UnifiedTransactionHistory_CreatedAt ON [UnifiedTransactionHistory]([CreatedAt] DESC);

    PRINT 'Created UnifiedTransactionHistory table successfully';
END
ELSE
BEGIN
    PRINT 'UnifiedTransactionHistory table already exists';
END
GO

-- =============================================
-- STEP 4: Create indexes on AR/AP tables
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsPayable_TransactionType' AND object_id = OBJECT_ID('AccountsPayable'))
BEGIN
    CREATE INDEX IX_AccountsPayable_TransactionType ON [AccountsPayable]([TransactionType]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsPayable_ReferenceNumber' AND object_id = OBJECT_ID('AccountsPayable'))
BEGIN
    CREATE INDEX IX_AccountsPayable_ReferenceNumber ON [AccountsPayable]([ReferenceNumber]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsPayable_ReviewStatus' AND object_id = OBJECT_ID('AccountsPayable'))
BEGIN
    CREATE INDEX IX_AccountsPayable_ReviewStatus ON [AccountsPayable]([ReviewStatus]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsReceivable_TransactionType' AND object_id = OBJECT_ID('AccountsReceivable'))
BEGIN
    CREATE INDEX IX_AccountsReceivable_TransactionType ON [AccountsReceivable]([TransactionType]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsReceivable_ReferenceNumber' AND object_id = OBJECT_ID('AccountsReceivable'))
BEGIN
    CREATE INDEX IX_AccountsReceivable_ReferenceNumber ON [AccountsReceivable]([ReferenceNumber]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccountsReceivable_ReviewStatus' AND object_id = OBJECT_ID('AccountsReceivable'))
BEGIN
    CREATE INDEX IX_AccountsReceivable_ReviewStatus ON [AccountsReceivable]([ReviewStatus]);
END
GO

-- =============================================
-- STEP 5: Ensure Chart of Accounts has required accounts
-- =============================================
IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '1001')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('1001', 'Cash', 'Asset', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '1301')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('1301', 'Loans Receivable', 'Asset', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '2001')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('2001', 'Customer Deposits', 'Liability', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '2101')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('2101', 'Savings Deposits', 'Liability', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '4101')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('4101', 'Transfer Fee Income', 'Revenue', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '4201')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('4201', 'Interest Income', 'Revenue', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '4202')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('4202', 'Penalty Income', 'Revenue', 1, 1, 'System');
END
GO

IF NOT EXISTS (SELECT * FROM [ChartOfAccounts] WHERE [AccountCode] = '5101')
BEGIN
    INSERT INTO [ChartOfAccounts] ([AccountCode], [AccountName], [AccountType], [Level], [IsActive], [CreatedBy])
    VALUES ('5101', 'Interest Expense', 'Expense', 1, 1, 'System');
END
GO

PRINT '=============================================';
PRINT 'AR/AP TRACKING SYSTEM MIGRATION COMPLETE';
PRINT '=============================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Added tracking columns to AccountsPayable';
PRINT '- Added tracking columns to AccountsReceivable';
PRINT '- Created UnifiedTransactionHistory table';
PRINT '- Created required indexes';
PRINT '- Added Chart of Accounts entries';
PRINT '';
PRINT 'Transaction Types:';
PRINT '  AR (Accounts Receivable - Money IN):';
PRINT '    - Deposit, LoanPayment, SavingsDeposit, Transfer (incoming)';
PRINT '  AP (Accounts Payable - Money OUT):';
PRINT '    - Withdrawal, LoanRelease, SavingsWithdrawal, SavingsInterest, Transfer (outgoing)';
PRINT '';
PRINT 'Reference Number Format: {TYPE}-{YYYYMMDD}-{6DIGIT}';
PRINT '  DEP = Deposit, WTH = Withdrawal, TRF = Transfer';
PRINT '  LNP = Loan Payment, LNR = Loan Release';
PRINT '  SVD = Savings Deposit, SVW = Savings Withdrawal, SVI = Savings Interest';
PRINT '=============================================';
GO
