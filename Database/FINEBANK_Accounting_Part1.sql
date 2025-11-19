-- ============================================================================
-- FINEBANK ACCOUNTING & FINANCE SCHEMA - PART 1
-- Focused on: Accounting Operations & Finance Operations (Single Branch)
-- ============================================================================

IF DB_ID(N'FINEBANK') IS NULL
BEGIN
    CREATE DATABASE [FINEBANK];
END
GO

USE [FINEBANK];
GO

-- ============================================================================
-- SECTION 1: CORE LOOKUP TABLES
-- ============================================================================

CREATE TABLE [Roles]
(
    [RoleId]         INT           NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    [RoleName]       NVARCHAR(50)  NOT NULL,
    [Description]    NVARCHAR(200) NULL,
    [IsActive]       BIT           NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
    [CreatedAt]      DATETIME2(0)  NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX IX_Roles_RoleName ON [Roles]([RoleName]);
GO

INSERT INTO [Roles] ([RoleId], [RoleName], [Description], [IsActive])
VALUES
(1, N'SuperAdmin',     N'System administrator with full access', 1),
(2, N'Customer',       N'Customer using the customer portal', 1),
(3, N'Accountant',     N'Accounting staff for journal entries, general ledger', 1),
(4, N'FinanceManager', N'Finance manager for budgets, forecasting, reports', 1),
(5, N'Teller',         N'Teller/cashier for deposits and withdrawals', 1);
GO

CREATE TABLE [Departments]
(
    [DepartmentId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    [Name]         NVARCHAR(100)    NOT NULL,
    [Description]  NVARCHAR(200)    NULL,
    [IsActive]     BIT              NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
    [CreatedAt]    DATETIME2(0)     NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================================
-- SECTION 2: USERS, EMPLOYEES, CUSTOMERS
-- ============================================================================

CREATE TABLE [Users]
(
    [UserId]        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    [Username]      NVARCHAR(100)     NOT NULL,
    [PasswordHash]  NVARCHAR(256)     NOT NULL,
    [Email]         NVARCHAR(200)     NULL,
    [PhoneNumber]   NVARCHAR(50)      NULL,
    [RoleId]        INT               NOT NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    [IsLockedOut]   BIT               NOT NULL CONSTRAINT DF_Users_IsLockedOut DEFAULT (0),
    [LastLoginAt]   DATETIME2(0)      NULL,
    [CreatedAt]     DATETIME2(0)      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2(0)      NULL
);
GO

ALTER TABLE [Users]
ADD CONSTRAINT FK_Users_Roles
FOREIGN KEY ([RoleId]) REFERENCES [Roles]([RoleId]);
GO

CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);
GO

CREATE TABLE [Employees]
(
    [EmployeeId]      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    [UserId]          INT               NOT NULL,
    [EmployeeNumber]  NVARCHAR(50)      NOT NULL,
    [FirstName]       NVARCHAR(100)     NOT NULL,
    [LastName]        NVARCHAR(100)     NOT NULL,
    [MiddleName]      NVARCHAR(100)     NULL,
    [Email]           NVARCHAR(200)     NULL,
    [PhoneNumber]     NVARCHAR(50)      NULL,
    [DepartmentId]    INT               NULL,
    [PositionTitle]   NVARCHAR(100)     NULL,
    [DateHired]       DATE              NULL,
    [IsActive]        BIT               NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
    [CreatedAt]       DATETIME2(0)      NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]       DATETIME2(0)      NULL
);
GO

ALTER TABLE [Employees]
ADD CONSTRAINT FK_Employees_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

ALTER TABLE [Employees]
ADD CONSTRAINT FK_Employees_Departments
FOREIGN KEY ([DepartmentId]) REFERENCES [Departments]([DepartmentId]);
GO

CREATE UNIQUE INDEX IX_Employees_EmployeeNumber ON [Employees]([EmployeeNumber]);
GO

CREATE TABLE [Customers]
(
    [CustomerId]     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    [UserId]         INT               NULL,
    [CustomerNumber] NVARCHAR(50)      NOT NULL,
    [FirstName]      NVARCHAR(100)     NOT NULL,
    [LastName]       NVARCHAR(100)     NOT NULL,
    [MiddleName]     NVARCHAR(100)     NULL,
    [Email]          NVARCHAR(200)     NULL,
    [PhoneNumber]    NVARCHAR(50)      NULL,
    [AddressLine1]   NVARCHAR(200)     NULL,
    [AddressLine2]   NVARCHAR(200)     NULL,
    [City]           NVARCHAR(100)     NULL,
    [Province]       NVARCHAR(100)     NULL,
    [PostalCode]     NVARCHAR(20)      NULL,
    [DateOfBirth]    DATE              NULL,
    [IsActive]       BIT               NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1),
    [CreatedAt]      DATETIME2(0)      NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]      DATETIME2(0)      NULL
);
GO

ALTER TABLE [Customers]
ADD CONSTRAINT FK_Customers_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

CREATE UNIQUE INDEX IX_Customers_CustomerNumber ON [Customers]([CustomerNumber]);
GO

-- ============================================================================
-- SECTION 3: ACCOUNTS & ACCOUNT MANAGEMENT
-- ============================================================================

CREATE TABLE [AccountStatuses]
(
    [AccountStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountStatuses PRIMARY KEY,
    [StatusName]      NVARCHAR(50)      NOT NULL,
    [Description]     NVARCHAR(200)     NULL
);
GO

INSERT INTO [AccountStatuses] ([StatusName], [Description])
VALUES
(N'Pending', N'Account pending activation'),
(N'Active', N'Account is active'),
(N'Suspended', N'Account is suspended'),
(N'Closed', N'Account is closed');
GO

CREATE TABLE [CustomerAccounts]
(
    [AccountId]         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAccounts PRIMARY KEY,
    [CustomerId]        INT               NOT NULL,
    [AccountName]       NVARCHAR(100)     NOT NULL,
    [AccountNumber]     NVARCHAR(30)      NOT NULL,
    [Currency]          NVARCHAR(3)       NOT NULL CONSTRAINT DF_CustomerAccounts_Currency DEFAULT N'PHP',
    [Balance]           DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CustomerAccounts_Balance DEFAULT (0),
    [AvailableBalance]  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CustomerAccounts_AvailableBalance DEFAULT (0),
    [AccountStatusId]   INT               NOT NULL,
    [OpenedDate]        DATE              NOT NULL CONSTRAINT DF_CustomerAccounts_OpenedDate DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    [ClosedDate]        DATE              NULL,
    [CreatedByEmployeeId] INT             NULL,
    [LastTransactionAt] DATETIME2(0)      NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_CustomerAccounts_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_AccountStatuses
FOREIGN KEY ([AccountStatusId]) REFERENCES [AccountStatuses]([AccountStatusId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_CustomerAccounts_AccountNumber ON [CustomerAccounts]([AccountNumber]);
GO

-- ============================================================================
-- SECTION 4: WITHDRAWAL LIMITS
-- ============================================================================

CREATE TABLE [WithdrawalLimits]
(
    [WithdrawalLimitId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WithdrawalLimits PRIMARY KEY,
    [CustomerId]        INT               NOT NULL,
    [DailyLimit]        DECIMAL(18,2)     NOT NULL,
    [MonthlyLimit]      DECIMAL(18,2)     NOT NULL,
    [DailyWithdrawnAmount] DECIMAL(18,2)  NOT NULL CONSTRAINT DF_WithdrawalLimits_DailyWithdrawnAmount DEFAULT (0),
    [MonthlyWithdrawnAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_WithdrawalLimits_MonthlyWithdrawnAmount DEFAULT (0),
    [LastResetDate]     DATE              NOT NULL CONSTRAINT DF_WithdrawalLimits_LastResetDate DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_WithdrawalLimits_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

ALTER TABLE [WithdrawalLimits]
ADD CONSTRAINT FK_WithdrawalLimits_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

-- ============================================================================
-- SECTION 5: TRANSACTION TYPES & CHANNELS
-- ============================================================================

CREATE TABLE [TransactionTypes]
(
    [TransactionTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionTypes PRIMARY KEY,
    [Code]              NVARCHAR(50)      NOT NULL,
    [Name]              NVARCHAR(100)     NOT NULL,
    [Description]       NVARCHAR(200)     NULL,
    [IsCredit]          BIT               NOT NULL
);
GO

INSERT INTO [TransactionTypes] ([Code], [Name], [IsCredit], [Description])
VALUES
(N'DEP', N'Deposit', 1, N'Money deposited into account'),
(N'WTH', N'Withdrawal', 0, N'Money withdrawn from account'),
(N'TRF', N'Transfer', 0, N'Money transferred to another account'),
(N'FEE', N'Fee', 0, N'Transaction or service fee'),
(N'INT', N'Interest', 1, N'Interest earned on account'),
(N'LN_DISB', N'Loan Disbursement', 1, N'Loan amount disbursed'),
(N'LN_REPAY', N'Loan Repayment', 0, N'Loan repayment');
GO

CREATE TABLE [TransactionChannels]
(
    [ChannelId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionChannels PRIMARY KEY,
    [ChannelName] NVARCHAR(50)    NOT NULL,
    [Description] NVARCHAR(200)   NULL
);
GO

INSERT INTO [TransactionChannels] ([ChannelName], [Description])
VALUES
(N'Teller', N'Over-the-counter teller transaction'),
(N'ATM', N'Automated Teller Machine'),
(N'Online', N'Online banking portal'),
(N'Mobile', N'Mobile app transaction');
GO

CREATE TABLE [TransactionStatuses]
(
    [TransactionStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionStatuses PRIMARY KEY,
    [StatusName]          NVARCHAR(50)      NOT NULL,
    [Description]         NVARCHAR(200)     NULL
);
GO

INSERT INTO [TransactionStatuses] ([StatusName], [Description])
VALUES
(N'Pending', N'Transaction is pending'),
(N'Completed', N'Transaction completed successfully'),
(N'Failed', N'Transaction failed'),
(N'Reversed', N'Transaction has been reversed'),
(N'Cancelled', N'Transaction was cancelled');
GO

-- ============================================================================
-- SECTION 6: DEPOSITS & WITHDRAWALS
-- ============================================================================

CREATE TABLE [DepositMethods]
(
    [DepositMethodId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DepositMethods PRIMARY KEY,
    [MethodName]      NVARCHAR(50)      NOT NULL,
    [Description]     NVARCHAR(200)     NULL
);
GO

INSERT INTO [DepositMethods] ([MethodName], [Description])
VALUES
(N'Cash', N'Cash deposit'),
(N'Check', N'Check deposit'),
(N'Online Transfer', N'Online bank transfer');
GO

CREATE TABLE [Deposits]
(
    [DepositId]          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Deposits PRIMARY KEY,
    [AccountId]          INT               NOT NULL,
    [CustomerId]         INT               NOT NULL,
    [DepositMethodId]    INT               NOT NULL,
    [Amount]             DECIMAL(18,2)     NOT NULL,
    [DepositSlipNumber]  NVARCHAR(100)     NULL,
    [DepositSlipPath]    NVARCHAR(500)     NULL,
    [ReferenceNumber]    NVARCHAR(100)     NOT NULL,
    [Description]        NVARCHAR(250)     NULL,
    [ProcessedByEmployeeId] INT            NOT NULL,
    [ProcessedByEmployeeName] NVARCHAR(200) NULL,
    [TransactionStatusId] INT              NOT NULL,
    [DepositDate]        DATETIME2(0)      NOT NULL CONSTRAINT DF_Deposits_DepositDate DEFAULT SYSUTCDATETIME(),
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_Deposits_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [Deposits]
ADD CONSTRAINT FK_Deposits_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [Deposits]
ADD CONSTRAINT FK_Deposits_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [Deposits]
ADD CONSTRAINT FK_Deposits_DepositMethods
FOREIGN KEY ([DepositMethodId]) REFERENCES [DepositMethods]([DepositMethodId]);
GO

ALTER TABLE [Deposits]
ADD CONSTRAINT FK_Deposits_Employees
FOREIGN KEY ([ProcessedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [Deposits]
ADD CONSTRAINT FK_Deposits_TransactionStatuses
FOREIGN KEY ([TransactionStatusId]) REFERENCES [TransactionStatuses]([TransactionStatusId]);
GO

CREATE INDEX IX_Deposits_AccountId_DepositDate ON [Deposits]([AccountId], [DepositDate]);
GO

CREATE TABLE [WithdrawalMethods]
(
    [WithdrawalMethodId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WithdrawalMethods PRIMARY KEY,
    [MethodName]         NVARCHAR(50)      NOT NULL,
    [Description]        NVARCHAR(200)     NULL
);
GO

INSERT INTO [WithdrawalMethods] ([MethodName], [Description])
VALUES
(N'Teller', N'Over-the-counter withdrawal'),
(N'ATM', N'ATM withdrawal'),
(N'Online Transfer', N'Online transfer withdrawal');
GO

CREATE TABLE [Withdrawals]
(
    [WithdrawalId]       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Withdrawals PRIMARY KEY,
    [AccountId]          INT               NOT NULL,
    [CustomerId]         INT               NOT NULL,
    [WithdrawalMethodId] INT               NOT NULL,
    [Amount]             DECIMAL(18,2)     NOT NULL,
    [ReferenceNumber]    NVARCHAR(100)     NOT NULL,
    [Description]        NVARCHAR(250)     NULL,
    [ProcessedByEmployeeId] INT            NULL,
    [ProcessedByEmployeeName] NVARCHAR(200) NULL,
    [TransactionStatusId] INT              NOT NULL,
    [WithdrawalDate]     DATETIME2(0)      NOT NULL CONSTRAINT DF_Withdrawals_WithdrawalDate DEFAULT SYSUTCDATETIME(),
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_Withdrawals_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [Withdrawals]
ADD CONSTRAINT FK_Withdrawals_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [Withdrawals]
ADD CONSTRAINT FK_Withdrawals_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [Withdrawals]
ADD CONSTRAINT FK_Withdrawals_WithdrawalMethods
FOREIGN KEY ([WithdrawalMethodId]) REFERENCES [WithdrawalMethods]([WithdrawalMethodId]);
GO

ALTER TABLE [Withdrawals]
ADD CONSTRAINT FK_Withdrawals_Employees
FOREIGN KEY ([ProcessedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [Withdrawals]
ADD CONSTRAINT FK_Withdrawals_TransactionStatuses
FOREIGN KEY ([TransactionStatusId]) REFERENCES [TransactionStatuses]([TransactionStatusId]);
GO

CREATE INDEX IX_Withdrawals_AccountId_WithdrawalDate ON [Withdrawals]([AccountId], [WithdrawalDate]);
GO

-- ============================================================================
-- SECTION 7: TRANSFERS (INTERNAL)
-- ============================================================================

CREATE TABLE [Transfers]
(
    [TransferId]         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Transfers PRIMARY KEY,
    [FromAccountId]      INT               NOT NULL,
    [ToAccountId]        INT               NOT NULL,
    [CustomerId]         INT               NOT NULL,
    [Amount]             DECIMAL(18,2)     NOT NULL,
    [ReferenceNumber]    NVARCHAR(100)     NOT NULL,
    [Description]        NVARCHAR(250)     NULL,
    [TransactionStatusId] INT              NOT NULL,
    [TransferDate]       DATETIME2(0)      NOT NULL CONSTRAINT DF_Transfers_TransferDate DEFAULT SYSUTCDATETIME(),
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_Transfers_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [Transfers]
ADD CONSTRAINT FK_Transfers_FromAccount
FOREIGN KEY ([FromAccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [Transfers]
ADD CONSTRAINT FK_Transfers_ToAccount
FOREIGN KEY ([ToAccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [Transfers]
ADD CONSTRAINT FK_Transfers_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [Transfers]
ADD CONSTRAINT FK_Transfers_TransactionStatuses
FOREIGN KEY ([TransactionStatusId]) REFERENCES [TransactionStatuses]([TransactionStatusId]);
GO

CREATE INDEX IX_Transfers_FromAccountId_TransferDate ON [Transfers]([FromAccountId], [TransferDate]);
GO
