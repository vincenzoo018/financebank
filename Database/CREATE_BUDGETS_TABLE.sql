-- Create Budgets Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Budgets')
BEGIN
    CREATE TABLE [dbo].[Budgets] (
        [BudgetId] INT IDENTITY(1,1) PRIMARY KEY,
        [BudgetName] NVARCHAR(100) NOT NULL,
        [Department] NVARCHAR(50) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [AllocatedAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [SpentAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [RemainingAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [StartDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [EndDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(50) NULL
    );
    
    -- Create indexes
    CREATE INDEX [IX_Budgets_Department] ON [dbo].[Budgets] ([Department]);
    CREATE INDEX [IX_Budgets_Status] ON [dbo].[Budgets] ([Status]);
    CREATE INDEX [IX_Budgets_CreatedAt] ON [dbo].[Budgets] ([CreatedAt]);
    
    PRINT 'Budgets table created successfully';
END
ELSE
BEGIN
    PRINT 'Budgets table already exists';
END

-- Create CashflowEntries Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CashflowEntries')
BEGIN
    CREATE TABLE [dbo].[CashflowEntries] (
        [CashflowId] INT IDENTITY(1,1) PRIMARY KEY,
        [EntryDate] DATETIME2 NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [InAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [OutAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [NetCashflow] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX [IX_CashflowEntries_EntryDate] ON [dbo].[CashflowEntries] ([EntryDate]);
    CREATE INDEX [IX_CashflowEntries_Category] ON [dbo].[CashflowEntries] ([Category]);
    
    PRINT 'CashflowEntries table created successfully';
END
ELSE
BEGIN
    PRINT 'CashflowEntries table already exists';
END

-- Create FinancialForecasts Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FinancialForecasts')
BEGIN
    CREATE TABLE [dbo].[FinancialForecasts] (
        [ForecastId] INT IDENTITY(1,1) PRIMARY KEY,
        [ForecastName] NVARCHAR(100) NOT NULL,
        [ForecastDate] DATETIME2 NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [ForecastAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ActualAmount] DECIMAL(18,2) NULL DEFAULT 0,
        [Variance] DECIMAL(18,2) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(50) NULL
    );
    
    CREATE INDEX [IX_FinancialForecasts_ForecastDate] ON [dbo].[FinancialForecasts] ([ForecastDate]);
    CREATE INDEX [IX_FinancialForecasts_Category] ON [dbo].[FinancialForecasts] ([Category]);
    CREATE INDEX [IX_FinancialForecasts_Status] ON [dbo].[FinancialForecasts] ([Status]);
    
    PRINT 'FinancialForecasts table created successfully';
END
ELSE
BEGIN
    PRINT 'FinancialForecasts table already exists';
END
