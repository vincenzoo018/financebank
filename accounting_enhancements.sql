-- Add accounting enhancements to existing tables
USE BFASdatabase;
GO

-- Create ChartOfAccounts table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChartOfAccounts')
BEGIN
    CREATE TABLE [dbo].[ChartOfAccounts] (
        [AccountId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [AccountCode] NVARCHAR(50) NOT NULL UNIQUE,
        [AccountName] NVARCHAR(100) NOT NULL,
        [AccountType] NVARCHAR(50) NOT NULL,
        [AccountSubType] NVARCHAR(50) NOT NULL,
        [NormalBalance] NVARCHAR(10) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsHeader] BIT NOT NULL DEFAULT 0,
        [ParentAccountId] INT NULL REFERENCES [ChartOfAccounts]([AccountId]) ON DELETE RESTRICT,
        [Level] INT NOT NULL DEFAULT 1,
        [ControlAccount] NVARCHAR(50) NULL,
        [CurrencyCode] NVARCHAR(3) NOT NULL DEFAULT 'PHP',
        [Department] NVARCHAR(50) NULL,
        [CostCenter] NVARCHAR(50) NULL,
        [AllowManualEntry] BIT NOT NULL DEFAULT 1,
        [RequiresApproval] BIT NOT NULL DEFAULT 0,
        [OpeningBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [CurrentBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [YearToDateBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(50) NOT NULL,
        [ModifiedAt] DATETIME2 NULL,
        [ModifiedBy] NVARCHAR(50) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [TaxAccount] BIT NOT NULL DEFAULT 0,
        [BankAccount] BIT NOT NULL DEFAULT 0,
        [CashAccount] BIT NOT NULL DEFAULT 0,
        [ReceivablesAccount] BIT NOT NULL DEFAULT 0,
        [PayablesAccount] BIT NOT NULL DEFAULT 0,
        [StatementSectionId] INT NULL,
        [ReportingCode] NVARCHAR(20) NULL,
        [ExternalRefCode] NVARCHAR(50) NULL
    );

    CREATE INDEX [IX_ChartOfAccounts_AccountCode] ON [ChartOfAccounts]([AccountCode]);
    CREATE INDEX [IX_ChartOfAccounts_AccountType] ON [ChartOfAccounts]([AccountType]);
    PRINT 'ChartOfAccounts table created successfully'
END
ELSE
    PRINT 'ChartOfAccounts table already exists'
GO

-- Add columns to JournalEntry if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'JournalNumber')
    ALTER TABLE [dbo].[JournalEntry] ADD [JournalNumber] NVARCHAR(50) NOT NULL DEFAULT ''
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'ReversedJournalId')
    ALTER TABLE [dbo].[JournalEntry] ADD [ReversedJournalId] INT NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'ReversalStatus')
    ALTER TABLE [dbo].[JournalEntry] ADD [ReversalStatus] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'Department')
    ALTER TABLE [dbo].[JournalEntry] ADD [Department] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'CostCenter')
    ALTER TABLE [dbo].[JournalEntry] ADD [CostCenter] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'ModifiedBy')
    ALTER TABLE [dbo].[JournalEntry] ADD [ModifiedBy] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntry' AND COLUMN_NAME = 'ModifiedAt')
    ALTER TABLE [dbo].[JournalEntry] ADD [ModifiedAt] DATETIME2 NULL

-- Add unique index on JournalNumber if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JournalEntry_JournalNumber')
    CREATE UNIQUE INDEX [IX_JournalEntry_JournalNumber] ON [JournalEntry]([JournalNumber])

PRINT 'JournalEntry columns added successfully'
GO

-- Add columns to GeneralLedgerEntry if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'JournalId')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [JournalId] INT NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'AccountType')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [AccountType] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'Balance')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'Department')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [Department] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'CostCenter')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [CostCenter] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralLedgerEntry' AND COLUMN_NAME = 'Status')
    ALTER TABLE [dbo].[GeneralLedgerEntry] ADD [Status] NVARCHAR(50) NULL

PRINT 'GeneralLedgerEntry columns added successfully'
GO

-- Add columns to TrialBalanceEntry if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TrialBalanceEntry' AND COLUMN_NAME = 'AccountType')
    ALTER TABLE [dbo].[TrialBalanceEntry] ADD [AccountType] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TrialBalanceEntry' AND COLUMN_NAME = 'Status')
    ALTER TABLE [dbo].[TrialBalanceEntry] ADD [Status] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TrialBalanceEntry' AND COLUMN_NAME = 'TotalDebits')
    ALTER TABLE [dbo].[TrialBalanceEntry] ADD [TotalDebits] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TrialBalanceEntry' AND COLUMN_NAME = 'TotalCredits')
    ALTER TABLE [dbo].[TrialBalanceEntry] ADD [TotalCredits] DECIMAL(18,2) NOT NULL DEFAULT 0

PRINT 'TrialBalanceEntry columns added successfully'
GO

-- Add columns to FinancialStatement if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'TotalAssets')
    ALTER TABLE [dbo].[FinancialStatement] ADD [TotalAssets] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'TotalLiabilities')
    ALTER TABLE [dbo].[FinancialStatement] ADD [TotalLiabilities] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'TotalEquity')
    ALTER TABLE [dbo].[FinancialStatement] ADD [TotalEquity] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'TotalRevenue')
    ALTER TABLE [dbo].[FinancialStatement] ADD [TotalRevenue] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'TotalExpenses')
    ALTER TABLE [dbo].[FinancialStatement] ADD [TotalExpenses] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'NetIncome')
    ALTER TABLE [dbo].[FinancialStatement] ADD [NetIncome] DECIMAL(18,2) NOT NULL DEFAULT 0
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FinancialStatement' AND COLUMN_NAME = 'Status')
    ALTER TABLE [dbo].[FinancialStatement] ADD [Status] NVARCHAR(50) NULL

PRINT 'FinancialStatement columns added successfully'
GO

-- Add columns to JournalEntryLine if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntryLine' AND COLUMN_NAME = 'AccountType')
    ALTER TABLE [dbo].[JournalEntryLine] ADD [AccountType] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntryLine' AND COLUMN_NAME = 'Department')
    ALTER TABLE [dbo].[JournalEntryLine] ADD [Department] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntryLine' AND COLUMN_NAME = 'CostCenter')
    ALTER TABLE [dbo].[JournalEntryLine] ADD [CostCenter] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntryLine' AND COLUMN_NAME = 'Status')
    ALTER TABLE [dbo].[JournalEntryLine] ADD [Status] NVARCHAR(50) NULL
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JournalEntryLine' AND COLUMN_NAME = 'CreatedAt')
    ALTER TABLE [dbo].[JournalEntryLine] ADD [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()

PRINT 'JournalEntryLine columns added successfully'
GO

PRINT 'All accounting enhancements completed successfully!'
