USE [BFASdatabase]
GO

-- =============================================
-- FIX MISSING TABLES - ADD TO EXISTING DATABASE
-- =============================================

-- Check and create Budgets table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Budgets')
BEGIN
    CREATE TABLE [dbo].[Budgets](
        [BudgetId] [int] IDENTITY(1,1) NOT NULL,
        [BudgetName] [nvarchar](100) NOT NULL,
        [Department] [nvarchar](50) NOT NULL,
        [Category] [nvarchar](50) NOT NULL,
        [AllocatedAmount] [decimal](18, 2) NOT NULL,
        [SpentAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [RemainingAmount] [decimal](18, 2) NOT NULL,
        [StartDate] [datetime2] NOT NULL DEFAULT GETDATE(),
        [EndDate] [datetime2] NOT NULL DEFAULT GETDATE(),
        [Status] [nvarchar](50) NOT NULL DEFAULT 'Active',
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [nvarchar](50) NULL,
    PRIMARY KEY CLUSTERED ([BudgetId] ASC)
    ) ON [PRIMARY]
    PRINT 'Budgets table created successfully'
END
ELSE
    PRINT 'Budgets table already exists'
GO

-- Check and create CashflowEntries table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CashflowEntries')
BEGIN
    CREATE TABLE [dbo].[CashflowEntries](
        [CashflowId] [int] IDENTITY(1,1) NOT NULL,
        [EntryDate] [datetime2] NOT NULL,
        [Category] [nvarchar](50) NOT NULL,
        [InAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [OutAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [NetCashflow] [decimal](18, 2) NOT NULL DEFAULT 0,
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2] NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([CashflowId] ASC)
    ) ON [PRIMARY]
    PRINT 'CashflowEntries table created successfully'
END
ELSE
    PRINT 'CashflowEntries table already exists'
GO

-- Check and create FinancialForecasts table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FinancialForecasts')
BEGIN
    CREATE TABLE [dbo].[FinancialForecasts](
        [ForecastId] [int] IDENTITY(1,1) NOT NULL,
        [ForecastName] [nvarchar](100) NOT NULL,
        [ForecastDate] [datetime2] NOT NULL,
        [Category] [nvarchar](50) NOT NULL,
        [ForecastAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [ActualAmount] [decimal](18, 2) NULL DEFAULT 0,
        [Variance] [decimal](18, 2) NULL,
        [Status] [nvarchar](50) NOT NULL DEFAULT 'Pending',
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [nvarchar](50) NULL,
    PRIMARY KEY CLUSTERED ([ForecastId] ASC)
    ) ON [PRIMARY]
    PRINT 'FinancialForecasts table created successfully'
END
ELSE
    PRINT 'FinancialForecasts table already exists'
GO

-- Create indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Budgets_Department')
    CREATE INDEX [IX_Budgets_Department] ON [dbo].[Budgets] ([Department])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Budgets_Status')
    CREATE INDEX [IX_Budgets_Status] ON [dbo].[Budgets] ([Status])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashflowEntries_EntryDate')
    CREATE INDEX [IX_CashflowEntries_EntryDate] ON [dbo].[CashflowEntries] ([EntryDate])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashflowEntries_Category')
    CREATE INDEX [IX_CashflowEntries_Category] ON [dbo].[CashflowEntries] ([Category])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FinancialForecasts_ForecastDate')
    CREATE INDEX [IX_FinancialForecasts_ForecastDate] ON [dbo].[FinancialForecasts] ([ForecastDate])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FinancialForecasts_Category')
    CREATE INDEX [IX_FinancialForecasts_Category] ON [dbo].[FinancialForecasts] ([Category])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FinancialForecasts_Status')
    CREATE INDEX [IX_FinancialForecasts_Status] ON [dbo].[FinancialForecasts] ([Status])
GO

PRINT '========================================='
PRINT 'Database fix completed successfully!'
PRINT '========================================='
PRINT 'Tables added/verified:'
PRINT '  - Budgets'
PRINT '  - CashflowEntries'
PRINT '  - FinancialForecasts'
PRINT 'Indexes created for optimal performance'
PRINT '========================================='
