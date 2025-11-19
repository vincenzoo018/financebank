-- ============================================================================
-- FINEBANK DATABASE SCHEMA - PART 3
-- Finance Management, Fees, Interest, Audit, Approvals
-- ============================================================================

USE [FINEBANK];
GO

-- ============================================================================
-- SECTION 11: FINANCE MANAGEMENT - FEES & CHARGES
-- ============================================================================

CREATE TABLE [FeeTypes]
(
    [FeeTypeId]   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeTypes PRIMARY KEY,
    [FeeName]     NVARCHAR(100)     NOT NULL,
    [Description] NVARCHAR(200)     NULL,
    [FeeAmount]   DECIMAL(18,2)     NOT NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT DF_FeeTypes_IsActive DEFAULT (1)
);
GO

CREATE TABLE [TransactionFees]
(
    [TransactionFeeId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionFees PRIMARY KEY,
    [AccountId]        INT               NOT NULL,
    [CustomerId]       INT               NOT NULL,
    [FeeTypeId]        INT               NOT NULL,
    [FeeAmount]        DECIMAL(18,2)     NOT NULL,
    [Description]      NVARCHAR(250)     NULL,
    [ChargedDate]      DATETIME2(0)      NOT NULL CONSTRAINT DF_TransactionFees_ChargedDate DEFAULT SYSUTCDATETIME(),
    [CreatedAt]        DATETIME2(0)      NOT NULL CONSTRAINT DF_TransactionFees_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [TransactionFees]
ADD CONSTRAINT FK_TransactionFees_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [TransactionFees]
ADD CONSTRAINT FK_TransactionFees_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [TransactionFees]
ADD CONSTRAINT FK_TransactionFees_FeeTypes
FOREIGN KEY ([FeeTypeId]) REFERENCES [FeeTypes]([FeeTypeId]);
GO

-- ============================================================================
-- SECTION 12: FINANCE MANAGEMENT - INTEREST ACCRUAL
-- ============================================================================

CREATE TABLE [InterestRates]
(
    [InterestRateId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InterestRates PRIMARY KEY,
    [AccountTypeId]  INT               NOT NULL,
    [AnnualRate]     DECIMAL(9,4)      NOT NULL,
    [EffectiveFrom]  DATE              NOT NULL,
    [EffectiveTo]    DATE              NULL,
    [IsActive]       BIT               NOT NULL CONSTRAINT DF_InterestRates_IsActive DEFAULT (1)
);
GO

ALTER TABLE [InterestRates]
ADD CONSTRAINT FK_InterestRates_AccountTypes
FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountTypes]([AccountTypeId]);
GO

CREATE TABLE [InterestAccruals]
(
    [InterestAccrualId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InterestAccruals PRIMARY KEY,
    [AccountId]         INT               NOT NULL,
    [CustomerId]        INT               NOT NULL,
    [InterestRateId]    INT               NOT NULL,
    [AccrualAmount]     DECIMAL(18,2)     NOT NULL,
    [AccrualDate]       DATE              NOT NULL,
    [IsPosted]          BIT               NOT NULL CONSTRAINT DF_InterestAccruals_IsPosted DEFAULT (0),
    [PostedDate]        DATETIME2(0)      NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_InterestAccruals_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [InterestAccruals]
ADD CONSTRAINT FK_InterestAccruals_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [InterestAccruals]
ADD CONSTRAINT FK_InterestAccruals_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [InterestAccruals]
ADD CONSTRAINT FK_InterestAccruals_InterestRates
FOREIGN KEY ([InterestRateId]) REFERENCES [InterestRates]([InterestRateId]);
GO

-- ============================================================================
-- SECTION 13: FINANCE MANAGEMENT - DAILY CASH FLOW
-- ============================================================================

CREATE TABLE [DailyCashFlows]
(
    [DailyCashFlowId]  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DailyCashFlows PRIMARY KEY,
    [BranchId]         INT               NOT NULL,
    [FlowDate]         DATE              NOT NULL,
    [TotalDeposits]    DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DailyCashFlows_TotalDeposits DEFAULT (0),
    [TotalWithdrawals] DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DailyCashFlows_TotalWithdrawals DEFAULT (0),
    [NetCashFlow]      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DailyCashFlows_NetCashFlow DEFAULT (0),
    [CreatedAt]        DATETIME2(0)      NOT NULL CONSTRAINT DF_DailyCashFlows_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]        DATETIME2(0)      NULL
);
GO

ALTER TABLE [DailyCashFlows]
ADD CONSTRAINT FK_DailyCashFlows_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
GO

CREATE UNIQUE INDEX IX_DailyCashFlows_BranchId_FlowDate ON [DailyCashFlows]([BranchId], [FlowDate]);
GO

-- ============================================================================
-- SECTION 14: FINANCE MANAGEMENT - PROFIT & LOSS
-- ============================================================================

CREATE TABLE [ProfitLossReports]
(
    [ProfitLossReportId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProfitLossReports PRIMARY KEY,
    [ReportDate]         DATE              NOT NULL,
    [ReportPeriod]       NVARCHAR(50)      NOT NULL,  -- e.g., 'Monthly', 'Quarterly', 'Annual'
    [TotalInterestIncome] DECIMAL(18,2)    NOT NULL CONSTRAINT DF_ProfitLossReports_TotalInterestIncome DEFAULT (0),
    [TotalFeeIncome]     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_ProfitLossReports_TotalFeeIncome DEFAULT (0),
    [TotalOperatingExpenses] DECIMAL(18,2) NOT NULL CONSTRAINT DF_ProfitLossReports_TotalOperatingExpenses DEFAULT (0),
    [NetProfit]          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_ProfitLossReports_NetProfit DEFAULT (0),
    [CreatedByEmployeeId] INT              NULL,
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_ProfitLossReports_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [ProfitLossReports]
ADD CONSTRAINT FK_ProfitLossReports_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

-- ============================================================================
-- SECTION 15: APPROVALS & WORKFLOW
-- ============================================================================

CREATE TABLE [ApprovalStatuses]
(
    [ApprovalStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApprovalStatuses PRIMARY KEY,
    [StatusName]       NVARCHAR(50)      NOT NULL,
    [Description]      NVARCHAR(200)     NULL
);
GO

INSERT INTO [ApprovalStatuses] ([StatusName], [Description])
VALUES
(N'Pending', N'Awaiting approval'),
(N'Approved', N'Approved'),
(N'Rejected', N'Rejected'),
(N'Cancelled', N'Cancelled');
GO

CREATE TABLE [ApprovalQueues]
(
    [ApprovalQueueId]    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApprovalQueues PRIMARY KEY,
    [EntityType]         NVARCHAR(50)      NOT NULL,  -- e.g., 'LoanApplication', 'HighValueTransaction'
    [EntityId]           BIGINT            NOT NULL,
    [ApprovalStatusId]   INT               NOT NULL,
    [RequestedByEmployeeId] INT            NULL,
    [ApprovedByEmployeeId] INT             NULL,
    [ApprovalRemarks]    NVARCHAR(500)     NULL,
    [RequestedAt]        DATETIME2(0)      NOT NULL CONSTRAINT DF_ApprovalQueues_RequestedAt DEFAULT SYSUTCDATETIME(),
    [ApprovedAt]         DATETIME2(0)      NULL,
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_ApprovalQueues_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [ApprovalQueues]
ADD CONSTRAINT FK_ApprovalQueues_ApprovalStatuses
FOREIGN KEY ([ApprovalStatusId]) REFERENCES [ApprovalStatuses]([ApprovalStatusId]);
GO

ALTER TABLE [ApprovalQueues]
ADD CONSTRAINT FK_ApprovalQueues_RequestedByEmployee
FOREIGN KEY ([RequestedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [ApprovalQueues]
ADD CONSTRAINT FK_ApprovalQueues_ApprovedByEmployee
FOREIGN KEY ([ApprovedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_ApprovalQueues_EntityType_EntityId ON [ApprovalQueues]([EntityType], [EntityId]);
GO

-- ============================================================================
-- SECTION 16: AUDIT & COMPLIANCE
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
-- SECTION 17: SUSPICIOUS ACTIVITY LOGGING
-- ============================================================================

CREATE TABLE [SuspiciousActivityTypes]
(
    [SuspiciousActivityTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SuspiciousActivityTypes PRIMARY KEY,
    [ActivityName]             NVARCHAR(100)     NOT NULL,
    [Description]              NVARCHAR(200)     NULL,
    [RiskLevel]                NVARCHAR(20)      NOT NULL  -- Low, Medium, High, Critical
);
GO

INSERT INTO [SuspiciousActivityTypes] ([ActivityName], [Description], [RiskLevel])
VALUES
(N'Multiple Failed Withdrawals', N'Customer attempted multiple failed withdrawals', N'Medium'),
(N'Unusual Transaction Amount', N'Transaction amount significantly higher than normal', N'High'),
(N'Rapid Transfers', N'Multiple transfers in short time period', N'High'),
(N'Account Access from New Location', N'Login from unusual geographic location', N'Medium'),
(N'Unusual Loan Request', N'Loan request with unusual parameters', N'Medium'),
(N'Duplicate Transactions', N'Potential duplicate transaction detected', N'Low');
GO

CREATE TABLE [SuspiciousActivities]
(
    [SuspiciousActivityId]     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SuspiciousActivities PRIMARY KEY,
    [CustomerId]               INT               NOT NULL,
    [AccountId]                INT               NULL,
    [SuspiciousActivityTypeId] INT               NOT NULL,
    [Description]              NVARCHAR(500)     NULL,
    [TransactionId]            BIGINT            NULL,
    [IsResolved]               BIT               NOT NULL CONSTRAINT DF_SuspiciousActivities_IsResolved DEFAULT (0),
    [ResolvedByEmployeeId]     INT               NULL,
    [ResolutionNotes]          NVARCHAR(500)     NULL,
    [ResolvedAt]               DATETIME2(0)      NULL,
    [CreatedAt]                DATETIME2(0)      NOT NULL CONSTRAINT DF_SuspiciousActivities_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [SuspiciousActivities]
ADD CONSTRAINT FK_SuspiciousActivities_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [SuspiciousActivities]
ADD CONSTRAINT FK_SuspiciousActivities_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [SuspiciousActivities]
ADD CONSTRAINT FK_SuspiciousActivities_SuspiciousActivityTypes
FOREIGN KEY ([SuspiciousActivityTypeId]) REFERENCES [SuspiciousActivityTypes]([SuspiciousActivityTypeId]);
GO

ALTER TABLE [SuspiciousActivities]
ADD CONSTRAINT FK_SuspiciousActivities_Employees
FOREIGN KEY ([ResolvedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE INDEX IX_SuspiciousActivities_CustomerId_CreatedAt ON [SuspiciousActivities]([CustomerId], [CreatedAt]);
GO

-- ============================================================================
-- SECTION 18: USER SETTINGS & PREFERENCES
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
-- END OF FINEBANK SCHEMA PART 3
-- ============================================================================
