-- ============================================================================
-- FINEBANK COMPLETE DATABASE SCHEMA - PART 2
-- All Accounting & Finance Tables
-- ============================================================================

USE [FINEBANK];
GO

-- ============================================================================
-- SECTION 17: CHART OF ACCOUNTS (ACCOUNTING)
-- ============================================================================

CREATE TABLE [ChartOfAccounts]
(
    [AccountCodeId]     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChartOfAccounts PRIMARY KEY,
    [AccountCode]       NVARCHAR(20)      NOT NULL,
    [AccountName]       NVARCHAR(150)     NOT NULL,
    [AccountType]       NVARCHAR(50)      NOT NULL,
    [Description]       NVARCHAR(250)     NULL,
    [NormalBalance]     NVARCHAR(10)      NOT NULL,
    [IsActive]          BIT               NOT NULL CONSTRAINT DF_ChartOfAccounts_IsActive DEFAULT (1),
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_ChartOfAccounts_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX IX_ChartOfAccounts_AccountCode ON [ChartOfAccounts]([AccountCode]);
GO

-- ============================================================================
-- SECTION 18: JOURNAL ENTRIES (ACCOUNTING)
-- ============================================================================

CREATE TABLE [JournalEntries]
(
    [JournalEntryId]    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_JournalEntries PRIMARY KEY,
    [EntryNumber]       NVARCHAR(50)      NOT NULL,
    [EntryDate]         DATE              NOT NULL,
    [Description]       NVARCHAR(250)     NOT NULL,
    [CreatedByEmployeeId] INT             NOT NULL,
    [ApprovedByEmployeeId] INT            NULL,
    [IsApproved]        BIT               NOT NULL CONSTRAINT DF_JournalEntries_IsApproved DEFAULT (0),
    [ApprovedAt]        DATETIME2(0)      NULL,
    [IsPosted]          BIT               NOT NULL CONSTRAINT DF_JournalEntries_IsPosted DEFAULT (0),
    [PostedAt]          DATETIME2(0)      NULL,
    [Remarks]           NVARCHAR(500)     NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_JournalEntries_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

ALTER TABLE [JournalEntries]
ADD CONSTRAINT FK_JournalEntries_CreatedByEmployee
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [JournalEntries]
ADD CONSTRAINT FK_JournalEntries_ApprovedByEmployee
FOREIGN KEY ([ApprovedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_JournalEntries_EntryNumber ON [JournalEntries]([EntryNumber]);
GO

-- ============================================================================
-- SECTION 19: JOURNAL ENTRY LINES (ACCOUNTING)
-- ============================================================================

CREATE TABLE [JournalEntryLines]
(
    [JournalEntryLineId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_JournalEntryLines PRIMARY KEY,
    [JournalEntryId]     BIGINT            NOT NULL,
    [AccountCodeId]      INT               NOT NULL,
    [DebitAmount]        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_JournalEntryLines_DebitAmount DEFAULT (0),
    [CreditAmount]       DECIMAL(18,2)     NOT NULL CONSTRAINT DF_JournalEntryLines_CreditAmount DEFAULT (0),
    [Description]        NVARCHAR(250)     NULL,
    [LineNumber]         INT               NOT NULL
);
GO

ALTER TABLE [JournalEntryLines]
ADD CONSTRAINT FK_JournalEntryLines_JournalEntries
FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries]([JournalEntryId]);
GO

ALTER TABLE [JournalEntryLines]
ADD CONSTRAINT FK_JournalEntryLines_ChartOfAccounts
FOREIGN KEY ([AccountCodeId]) REFERENCES [ChartOfAccounts]([AccountCodeId]);
GO

CREATE INDEX IX_JournalEntryLines_JournalEntryId ON [JournalEntryLines]([JournalEntryId]);
GO

-- ============================================================================
-- SECTION 20: GENERAL LEDGER (ACCOUNTING)
-- ============================================================================

CREATE TABLE [GeneralLedger]
(
    [GeneralLedgerId]   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GeneralLedger PRIMARY KEY,
    [AccountCodeId]     INT               NOT NULL,
    [TransactionDate]   DATE              NOT NULL,
    [JournalEntryId]    BIGINT            NULL,
    [Description]       NVARCHAR(250)     NULL,
    [DebitAmount]       DECIMAL(18,2)     NOT NULL CONSTRAINT DF_GeneralLedger_DebitAmount DEFAULT (0),
    [CreditAmount]      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_GeneralLedger_CreditAmount DEFAULT (0),
    [RunningBalance]    DECIMAL(18,2)     NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_GeneralLedger_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [GeneralLedger]
ADD CONSTRAINT FK_GeneralLedger_ChartOfAccounts
FOREIGN KEY ([AccountCodeId]) REFERENCES [ChartOfAccounts]([AccountCodeId]);
GO

ALTER TABLE [GeneralLedger]
ADD CONSTRAINT FK_GeneralLedger_JournalEntries
FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries]([JournalEntryId]);
GO

CREATE INDEX IX_GeneralLedger_AccountCodeId_TransactionDate ON [GeneralLedger]([AccountCodeId], [TransactionDate]);
GO

-- ============================================================================
-- SECTION 21: TRIAL BALANCES (ACCOUNTING)
-- ============================================================================

CREATE TABLE [TrialBalances]
(
    [TrialBalanceId]    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrialBalances PRIMARY KEY,
    [TrialBalanceDate]  DATE              NOT NULL,
    [AccountCodeId]     INT               NOT NULL,
    [DebitBalance]      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_TrialBalances_DebitBalance DEFAULT (0),
    [CreditBalance]     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_TrialBalances_CreditBalance DEFAULT (0),
    [CreatedByEmployeeId] INT             NOT NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_TrialBalances_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [TrialBalances]
ADD CONSTRAINT FK_TrialBalances_ChartOfAccounts
FOREIGN KEY ([AccountCodeId]) REFERENCES [ChartOfAccounts]([AccountCodeId]);
GO

ALTER TABLE [TrialBalances]
ADD CONSTRAINT FK_TrialBalances_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_TrialBalances_TrialBalanceDate_AccountCodeId ON [TrialBalances]([TrialBalanceDate], [AccountCodeId]);
GO

-- ============================================================================
-- SECTION 22: FINANCIAL STATEMENTS (ACCOUNTING)
-- ============================================================================

CREATE TABLE [FinancialStatements]
(
    [FinancialStatementId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatements PRIMARY KEY,
    [StatementType]        NVARCHAR(50)      NOT NULL,
    [StatementDate]        DATE              NOT NULL,
    [PeriodStart]          DATE              NOT NULL,
    [PeriodEnd]            DATE              NOT NULL,
    [Content]              NVARCHAR(MAX)     NULL,
    [CreatedByEmployeeId]  INT               NOT NULL,
    [ApprovedByEmployeeId] INT               NULL,
    [IsApproved]           BIT               NOT NULL CONSTRAINT DF_FinancialStatements_IsApproved DEFAULT (0),
    [ApprovedAt]           DATETIME2(0)      NULL,
    [CreatedAt]            DATETIME2(0)      NOT NULL CONSTRAINT DF_FinancialStatements_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [FinancialStatements]
ADD CONSTRAINT FK_FinancialStatements_CreatedByEmployee
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [FinancialStatements]
ADD CONSTRAINT FK_FinancialStatements_ApprovedByEmployee
FOREIGN KEY ([ApprovedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_FinancialStatements_StatementDate ON [FinancialStatements]([StatementDate]);
GO

-- ============================================================================
-- SECTION 23: ACCOUNTING REPORTS (ACCOUNTING)
-- ============================================================================

CREATE TABLE [AccountingReports]
(
    [AccountingReportId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountingReports PRIMARY KEY,
    [ReportName]         NVARCHAR(150)     NOT NULL,
    [ReportType]         NVARCHAR(50)      NOT NULL,
    [ReportDate]         DATE              NOT NULL,
    [PeriodStart]        DATE              NOT NULL,
    [PeriodEnd]          DATE              NOT NULL,
    [Content]            NVARCHAR(MAX)     NULL,
    [CreatedByEmployeeId] INT              NOT NULL,
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_AccountingReports_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [AccountingReports]
ADD CONSTRAINT FK_AccountingReports_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_AccountingReports_ReportDate ON [AccountingReports]([ReportDate]);
GO

-- ============================================================================
-- SECTION 24: ACCOUNTS PAYABLE (FINANCE)
-- ============================================================================

CREATE TABLE [AccountsPayable]
(
    [AccountsPayableId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountsPayable PRIMARY KEY,
    [VendorName]        NVARCHAR(150)     NOT NULL,
    [VendorNumber]      NVARCHAR(50)      NULL,
    [InvoiceNumber]     NVARCHAR(50)      NOT NULL,
    [InvoiceDate]       DATE              NOT NULL,
    [DueDate]           DATE              NOT NULL,
    [Amount]            DECIMAL(18,2)     NOT NULL,
    [PaidAmount]        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_AccountsPayable_PaidAmount DEFAULT (0),
    [RemainingAmount]   DECIMAL(18,2)     NOT NULL,
    [Description]       NVARCHAR(250)     NULL,
    [Status]            NVARCHAR(30)      NOT NULL CONSTRAINT DF_AccountsPayable_Status DEFAULT N'Unpaid',
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_AccountsPayable_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

CREATE INDEX IX_AccountsPayable_DueDate ON [AccountsPayable]([DueDate]);
GO

-- ============================================================================
-- SECTION 25: ACCOUNTS RECEIVABLE (FINANCE)
-- ============================================================================

CREATE TABLE [AccountsReceivable]
(
    [AccountsReceivableId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountsReceivable PRIMARY KEY,
    [CustomerId]           INT               NOT NULL,
    [InvoiceNumber]        NVARCHAR(50)      NOT NULL,
    [InvoiceDate]          DATE              NOT NULL,
    [DueDate]              DATE              NOT NULL,
    [Amount]               DECIMAL(18,2)     NOT NULL,
    [ReceivedAmount]       DECIMAL(18,2)     NOT NULL CONSTRAINT DF_AccountsReceivable_ReceivedAmount DEFAULT (0),
    [RemainingAmount]      DECIMAL(18,2)     NOT NULL,
    [Description]          NVARCHAR(250)     NULL,
    [Status]               NVARCHAR(30)      NOT NULL CONSTRAINT DF_AccountsReceivable_Status DEFAULT N'Outstanding',
    [CreatedAt]            DATETIME2(0)      NOT NULL CONSTRAINT DF_AccountsReceivable_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]            DATETIME2(0)      NULL
);
GO

ALTER TABLE [AccountsReceivable]
ADD CONSTRAINT FK_AccountsReceivable_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

CREATE INDEX IX_AccountsReceivable_DueDate ON [AccountsReceivable]([DueDate]);
GO

-- ============================================================================
-- SECTION 26: BUDGETS (FINANCE)
-- ============================================================================

CREATE TABLE [Budgets]
(
    [BudgetId]          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Budgets PRIMARY KEY,
    [BudgetName]        NVARCHAR(150)     NOT NULL,
    [BudgetYear]        INT               NOT NULL,
    [BudgetMonth]       INT               NOT NULL,
    [AccountCodeId]     INT               NOT NULL,
    [BudgetedAmount]    DECIMAL(18,2)     NOT NULL,
    [ActualAmount]      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Budgets_ActualAmount DEFAULT (0),
    [Variance]          DECIMAL(18,2)     NULL,
    [VariancePercent]   DECIMAL(9,2)      NULL,
    [Status]            NVARCHAR(30)      NOT NULL CONSTRAINT DF_Budgets_Status DEFAULT N'Active',
    [CreatedByEmployeeId] INT             NOT NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_Budgets_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

ALTER TABLE [Budgets]
ADD CONSTRAINT FK_Budgets_ChartOfAccounts
FOREIGN KEY ([AccountCodeId]) REFERENCES [ChartOfAccounts]([AccountCodeId]);
GO

ALTER TABLE [Budgets]
ADD CONSTRAINT FK_Budgets_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_Budgets_BudgetYear_BudgetMonth ON [Budgets]([BudgetYear], [BudgetMonth]);
GO

-- ============================================================================
-- SECTION 27: FINANCIAL FORECASTS (FINANCE)
-- ============================================================================

CREATE TABLE [FinancialForecasts]
(
    [FinancialForecastId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialForecasts PRIMARY KEY,
    [ForecastName]        NVARCHAR(150)     NOT NULL,
    [ForecastType]        NVARCHAR(50)      NOT NULL,
    [ForecastMonth]       INT               NOT NULL,
    [ForecastYear]        INT               NOT NULL,
    [ForecastedAmount]    DECIMAL(18,2)     NOT NULL,
    [ActualAmount]        DECIMAL(18,2)     NULL,
    [Accuracy]            DECIMAL(9,2)      NULL,
    [Description]         NVARCHAR(250)     NULL,
    [CreatedByEmployeeId] INT               NOT NULL,
    [CreatedAt]           DATETIME2(0)      NOT NULL CONSTRAINT DF_FinancialForecasts_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]           DATETIME2(0)      NULL
);
GO

ALTER TABLE [FinancialForecasts]
ADD CONSTRAINT FK_FinancialForecasts_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_FinancialForecasts_ForecastYear_ForecastMonth ON [FinancialForecasts]([ForecastYear], [ForecastMonth]);
GO

-- ============================================================================
-- SECTION 28: CASH FLOW ANALYSIS (FINANCE)
-- ============================================================================

CREATE TABLE [CashFlowAnalysis]
(
    [CashFlowAnalysisId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CashFlowAnalysis PRIMARY KEY,
    [AnalysisDate]       DATE              NOT NULL,
    [AnalysisMonth]      INT               NOT NULL,
    [AnalysisYear]       INT               NOT NULL,
    [CashInflows]        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CashFlowAnalysis_CashInflows DEFAULT (0),
    [CashOutflows]       DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CashFlowAnalysis_CashOutflows DEFAULT (0),
    [NetCashFlow]        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CashFlowAnalysis_NetCashFlow DEFAULT (0),
    [OpeningBalance]     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CashFlowAnalysis_OpeningBalance DEFAULT (0),
    [ClosingBalance]     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CashFlowAnalysis_ClosingBalance DEFAULT (0),
    [Description]        NVARCHAR(250)     NULL,
    [CreatedByEmployeeId] INT              NOT NULL,
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_CashFlowAnalysis_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]          DATETIME2(0)      NULL
);
GO

ALTER TABLE [CashFlowAnalysis]
ADD CONSTRAINT FK_CashFlowAnalysis_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_CashFlowAnalysis_AnalysisDate ON [CashFlowAnalysis]([AnalysisDate]);
GO

-- ============================================================================
-- SECTION 29: FINANCE REPORTS (FINANCE)
-- ============================================================================

CREATE TABLE [FinanceReports]
(
    [FinanceReportId]   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinanceReports PRIMARY KEY,
    [ReportName]        NVARCHAR(150)     NOT NULL,
    [ReportType]        NVARCHAR(50)      NOT NULL,
    [ReportDate]        DATE              NOT NULL,
    [PeriodStart]       DATE              NOT NULL,
    [PeriodEnd]         DATE              NOT NULL,
    [Content]           NVARCHAR(MAX)     NULL,
    [CreatedByEmployeeId] INT             NOT NULL,
    [ApprovedByEmployeeId] INT            NULL,
    [IsApproved]        BIT               NOT NULL CONSTRAINT DF_FinanceReports_IsApproved DEFAULT (0),
    [ApprovedAt]        DATETIME2(0)      NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_FinanceReports_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [FinanceReports]
ADD CONSTRAINT FK_FinanceReports_CreatedByEmployee
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [FinanceReports]
ADD CONSTRAINT FK_FinanceReports_ApprovedByEmployee
FOREIGN KEY ([ApprovedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_FinanceReports_ReportDate ON [FinanceReports]([ReportDate]);
GO

-- ============================================================================
-- SECTION 30: AUDIT LOGS
-- ============================================================================

CREATE TABLE [AuditLogs]
(
    [AuditLogId]   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    [UserId]       INT               NULL,
    [EmployeeId]   INT               NULL,
    [Action]       NVARCHAR(100)     NOT NULL,
    [EntityName]   NVARCHAR(100)     NULL,
    [EntityId]     BIGINT            NULL,
    [OldValues]    NVARCHAR(MAX)     NULL,
    [NewValues]    NVARCHAR(MAX)     NULL,
    [IpAddress]    NVARCHAR(50)      NULL,
    [UserAgent]    NVARCHAR(200)     NULL,
    [CreatedAt]    DATETIME2(0)      NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [AuditLogs]
ADD CONSTRAINT FK_AuditLogs_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

ALTER TABLE [AuditLogs]
ADD CONSTRAINT FK_AuditLogs_Employees
FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_AuditLogs_CreatedAt ON [AuditLogs]([CreatedAt]);
GO

-- ============================================================================
-- SECTION 31: NOTIFICATION PREFERENCES
-- ============================================================================

CREATE TABLE [NotificationPreferences]
(
    [NotificationPreferenceId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationPreferences PRIMARY KEY,
    [UserId]                   INT               NOT NULL,
    [EmailEnabled]             BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_EmailEnabled DEFAULT (1),
    [SmsEnabled]               BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_SmsEnabled DEFAULT (0),
    [PushEnabled]              BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_PushEnabled DEFAULT (0),
    [Language]                 NVARCHAR(10)      NOT NULL CONSTRAINT DF_NotificationPreferences_Language DEFAULT N'en',
    [DarkMode]                 BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_DarkMode DEFAULT (0),
    [HighContrast]             BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_HighContrast DEFAULT (0),
    [ReduceMotion]             BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_ReduceMotion DEFAULT (0),
    [ColorBlindMode]           BIT               NOT NULL CONSTRAINT DF_NotificationPreferences_ColorBlindMode DEFAULT (0),
    [UpdatedAt]                DATETIME2(0)      NOT NULL CONSTRAINT DF_NotificationPreferences_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [NotificationPreferences]
ADD CONSTRAINT FK_NotificationPreferences_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

-- ============================================================================
-- END OF FINEBANK COMPLETE DATABASE SCHEMA
-- ============================================================================

PRINT N'FINEBANK Database schema created successfully!';
PRINT N'Total tables: 31';
PRINT N'All relationships and indexes created.';
GO
