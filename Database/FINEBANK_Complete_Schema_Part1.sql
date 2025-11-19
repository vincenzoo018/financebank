-- ============================================================================
-- FINEBANK COMPLETE DATABASE SCHEMA - PART 1
-- All Core & Banking Tables
-- ============================================================================

IF DB_ID(N'FINEBANK') IS NULL
BEGIN
    CREATE DATABASE [FINEBANK];
END
GO

USE [FINEBANK];
GO

-- ============================================================================
-- SECTION 1: ROLES
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

-- ============================================================================
-- SECTION 2: DEPARTMENTS
-- ============================================================================

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
-- SECTION 3: USERS
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

-- ============================================================================
-- SECTION 4: EMPLOYEES
-- ============================================================================

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

-- ============================================================================
-- SECTION 5: CUSTOMERS
-- ============================================================================

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
-- SECTION 6: ACCOUNT STATUSES
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

-- ============================================================================
-- SECTION 7: CUSTOMER ACCOUNTS
-- ============================================================================

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
-- SECTION 8: WITHDRAWAL LIMITS
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
-- SECTION 9: TRANSACTION TYPES
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

-- ============================================================================
-- SECTION 10: TRANSACTION CHANNELS
-- ============================================================================

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

-- ============================================================================
-- SECTION 11: TRANSACTION STATUSES
-- ============================================================================

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
-- SECTION 12: DEPOSIT METHODS
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

-- ============================================================================
-- SECTION 13: DEPOSITS
-- ============================================================================

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

-- ============================================================================
-- SECTION 14: WITHDRAWAL METHODS
-- ============================================================================

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

-- ============================================================================
-- SECTION 15: WITHDRAWALS
-- ============================================================================

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
-- SECTION 16: TRANSFERS (INTERNAL)
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

-- ============================================================================
-- SECTION 17: LOAN PRODUCTS
-- ============================================================================

CREATE TABLE [LoanProducts]
(
    [LoanProductId]     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanProducts PRIMARY KEY,
    [ProductName]       NVARCHAR(100)     NOT NULL,
    [Description]       NVARCHAR(250)     NULL,
    [MinAmount]         DECIMAL(18,2)     NOT NULL,
    [MaxAmount]         DECIMAL(18,2)     NOT NULL,
    [InterestRate]      DECIMAL(9,4)      NOT NULL,
    [TermMonths]        INT               NOT NULL,
    [IsActive]          BIT               NOT NULL CONSTRAINT DF_LoanProducts_IsActive DEFAULT (1),
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanProducts_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================================
-- SECTION 18: LOAN APPLICATION STATUSES
-- ============================================================================

CREATE TABLE [LoanApplicationStatuses]
(
    [LoanApplicationStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanApplicationStatuses PRIMARY KEY,
    [StatusName]              NVARCHAR(50)      NOT NULL,
    [Description]             NVARCHAR(200)     NULL
);
GO

INSERT INTO [LoanApplicationStatuses] ([StatusName], [Description])
VALUES
(N'Submitted', N'Application submitted by customer'),
(N'TellerReview', N'Under teller review'),
(N'ManagerReview', N'Under manager/finance review'),
(N'Approved', N'Application approved'),
(N'Rejected', N'Application rejected'),
(N'Cancelled', N'Application cancelled');
GO

-- ============================================================================
-- SECTION 19: LOAN APPLICATIONS
-- ============================================================================

CREATE TABLE [LoanApplications]
(
    [LoanApplicationId]     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanApplications PRIMARY KEY,
    [CustomerId]            INT               NOT NULL,
    [LoanProductId]         INT               NOT NULL,
    [ApplicationNumber]     NVARCHAR(50)      NOT NULL,
    [RequestedAmount]       DECIMAL(18,2)     NOT NULL,
    [ApprovedAmount]        DECIMAL(18,2)     NULL,
    [Purpose]               NVARCHAR(250)     NULL,
    [LoanApplicationStatusId] INT             NOT NULL,
    [SubmittedByEmployeeId] INT              NULL,
    [SubmittedAt]           DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanApplications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    [ReviewedByEmployeeId]  INT              NULL,
    [ReviewedAt]            DATETIME2(0)      NULL,
    [ApprovalRemarks]       NVARCHAR(500)     NULL,
    [CreatedAt]             DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanApplications_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2(0)      NULL
);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_LoanProducts
FOREIGN KEY ([LoanProductId]) REFERENCES [LoanProducts]([LoanProductId]);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_LoanApplicationStatuses
FOREIGN KEY ([LoanApplicationStatusId]) REFERENCES [LoanApplicationStatuses]([LoanApplicationStatusId]);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_SubmittedByEmployee
FOREIGN KEY ([SubmittedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_ReviewedByEmployee
FOREIGN KEY ([ReviewedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_LoanApplications_ApplicationNumber ON [LoanApplications]([ApplicationNumber]);
GO

-- ============================================================================
-- SECTION 20: LOAN STATUSES
-- ============================================================================

CREATE TABLE [LoanStatuses]
(
    [LoanStatusId]  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanStatuses PRIMARY KEY,
    [StatusName]    NVARCHAR(50)      NOT NULL,
    [Description]   NVARCHAR(200)     NULL
);
GO

INSERT INTO [LoanStatuses] ([StatusName], [Description])
VALUES
(N'Active', N'Loan is active'),
(N'Completed', N'Loan fully repaid'),
(N'Defaulted', N'Loan in default'),
(N'Cancelled', N'Loan cancelled');
GO

-- ============================================================================
-- SECTION 21: LOANS
-- ============================================================================

CREATE TABLE [Loans]
(
    [LoanId]                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
    [LoanApplicationId]     BIGINT            NOT NULL,
    [CustomerId]            INT               NOT NULL,
    [LoanProductId]         INT               NOT NULL,
    [LoanNumber]            NVARCHAR(50)      NOT NULL,
    [PrincipalAmount]       DECIMAL(18,2)     NOT NULL,
    [InterestRate]          DECIMAL(9,4)      NOT NULL,
    [TermMonths]            INT               NOT NULL,
    [StartDate]             DATE              NOT NULL,
    [MaturityDate]          DATE              NOT NULL,
    [DisbursedAmount]       DECIMAL(18,2)     NOT NULL,
    [RemainingBalance]      DECIMAL(18,2)     NOT NULL,
    [LoanStatusId]          INT               NOT NULL,
    [DisbursedByEmployeeId] INT              NOT NULL,
    [DisbursedAt]           DATETIME2(0)      NOT NULL,
    [CreatedAt]             DATETIME2(0)      NOT NULL CONSTRAINT DF_Loans_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2(0)      NULL
);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_LoanApplications
FOREIGN KEY ([LoanApplicationId]) REFERENCES [LoanApplications]([LoanApplicationId]);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_LoanProducts
FOREIGN KEY ([LoanProductId]) REFERENCES [LoanProducts]([LoanProductId]);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_LoanStatuses
FOREIGN KEY ([LoanStatusId]) REFERENCES [LoanStatuses]([LoanStatusId]);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_DisbursedByEmployee
FOREIGN KEY ([DisbursedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_Loans_LoanNumber ON [Loans]([LoanNumber]);
GO

-- ============================================================================
-- SECTION 22: LOAN REPAYMENTS
-- ============================================================================

CREATE TABLE [LoanRepayments]
(
    [LoanRepaymentId]   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanRepayments PRIMARY KEY,
    [LoanId]            BIGINT            NOT NULL,
    [CustomerId]        INT               NOT NULL,
    [ScheduledAmount]   DECIMAL(18,2)     NOT NULL,
    [AmountPaid]        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_LoanRepayments_AmountPaid DEFAULT (0),
    [PaidDate]          DATE              NULL,
    [DueDate]           DATE              NOT NULL,
    [RepaymentNumber]   INT               NOT NULL,
    [RepaymentStatusId] INT               NOT NULL,
    [Notes]             NVARCHAR(250)     NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanRepayments_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2(0)      NULL
);
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_Loans
FOREIGN KEY ([LoanId]) REFERENCES [Loans]([LoanId]);
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

CREATE INDEX IX_LoanRepayments_LoanId_DueDate ON [LoanRepayments]([LoanId], [DueDate]);
GO

-- ============================================================================
-- SECTION 23: REPAYMENT STATUSES
-- ============================================================================

CREATE TABLE [RepaymentStatuses]
(
    [RepaymentStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RepaymentStatuses PRIMARY KEY,
    [StatusName]        NVARCHAR(50)      NOT NULL,
    [Description]       NVARCHAR(200)     NULL
);
GO

INSERT INTO [RepaymentStatuses] ([StatusName], [Description])
VALUES
(N'Pending', N'Repayment pending'),
(N'Paid', N'Repayment paid'),
(N'Overdue', N'Repayment overdue'),
(N'Partial', N'Repayment partially paid');
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_RepaymentStatuses
FOREIGN KEY ([RepaymentStatusId]) REFERENCES [RepaymentStatuses]([RepaymentStatusId]);
GO
