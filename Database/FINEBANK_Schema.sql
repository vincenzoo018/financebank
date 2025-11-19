-- FINEBANK database schema
-- Generated to align with existing Customer/Admin/Teller UI

IF DB_ID(N'FINEBANK') IS NULL
BEGIN
    CREATE DATABASE [FINEBANK];
END
GO

USE [FINEBANK];
GO

/*
    CORE LOOKUP TABLES
*/

CREATE TABLE [Roles]
(
    [RoleId]     INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    [RoleName]   NVARCHAR(50)  NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [IsSystemRole] BIT         NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT (1),
    [CreatedAt]  DATETIME2(0)  NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX IX_Roles_RoleName ON [Roles]([RoleName]);
GO

-- Seed required roles with fixed IDs 1–5
SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([RoleId], [RoleName], [Description], [IsSystemRole])
VALUES
(1, N'SuperAdmin',    N'System administrator (Admin UI)', 1),
(2, N'Customer',      N'Customer using the customer portal', 1),
(3, N'Accountant',    N'Accounting staff user', 1),
(4, N'FinanceManager',N'Finance manager / approver', 1),
(5, N'Teller',        N'Teller / cashier user', 1);
SET IDENTITY_INSERT [Roles] OFF;
GO

CREATE TABLE [Departments]
(
    [DepartmentId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    [Name]         NVARCHAR(100)    NOT NULL,
    [Description]  NVARCHAR(200)    NULL,
    [CreatedAt]    DATETIME2(0)     NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE [Branches]
(
    [BranchId]   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Branches PRIMARY KEY,
    [BranchCode] NVARCHAR(20)      NOT NULL,
    [Name]       NVARCHAR(150)     NOT NULL,
    [AddressLine1] NVARCHAR(200)   NULL,
    [AddressLine2] NVARCHAR(200)   NULL,
    [City]       NVARCHAR(100)     NULL,
    [Province]   NVARCHAR(100)     NULL,
    [PostalCode] NVARCHAR(20)      NULL,
    [CreatedAt]  DATETIME2(0)      NOT NULL CONSTRAINT DF_Branches_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX IX_Branches_BranchCode ON [Branches]([BranchCode]);
GO

/*
    USERS / EMPLOYEES / CUSTOMERS
*/

CREATE TABLE [Users]
(
    [UserId]        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    [Username]      NVARCHAR(100)    NOT NULL,
    [PasswordHash]  NVARCHAR(256)    NOT NULL,
    [Email]         NVARCHAR(200)    NULL,
    [PhoneNumber]   NVARCHAR(50)     NULL,
    [RoleId]        INT              NOT NULL,
    [IsActive]      BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    [IsLockedOut]   BIT              NOT NULL CONSTRAINT DF_Users_IsLockedOut DEFAULT (0),
    [LastLoginAt]   DATETIME2(0)     NULL,
    [CreatedAt]     DATETIME2(0)     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2(0)     NULL
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
    [UserId]          INT              NOT NULL,
    [EmployeeNumber]  NVARCHAR(50)     NOT NULL,
    [FirstName]       NVARCHAR(100)    NOT NULL,
    [LastName]        NVARCHAR(100)    NOT NULL,
    [Email]           NVARCHAR(200)    NULL,
    [PhoneNumber]     NVARCHAR(50)     NULL,
    [DepartmentId]    INT              NULL,
    [BranchId]        INT              NULL,
    [PositionTitle]   NVARCHAR(100)    NULL,
    [DateHired]       DATE             NULL,
    [IsActive]        BIT              NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
    [CreatedAt]       DATETIME2(0)     NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]       DATETIME2(0)     NULL
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

ALTER TABLE [Employees]
ADD CONSTRAINT FK_Employees_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
GO

CREATE UNIQUE INDEX IX_Employees_EmployeeNumber ON [Employees]([EmployeeNumber]);
GO

CREATE TABLE [Customers]
(
    [CustomerId]     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    [UserId]         INT              NULL,
    [CustomerNumber] NVARCHAR(50)     NOT NULL,
    [FirstName]      NVARCHAR(100)    NOT NULL,
    [LastName]       NVARCHAR(100)    NOT NULL,
    [MiddleName]     NVARCHAR(100)    NULL,
    [Email]          NVARCHAR(200)    NULL,
    [PhoneNumber]    NVARCHAR(50)     NULL,
    [AddressLine1]   NVARCHAR(200)    NULL,
    [AddressLine2]   NVARCHAR(200)    NULL,
    [City]           NVARCHAR(100)    NULL,
    [Province]       NVARCHAR(100)    NULL,
    [PostalCode]     NVARCHAR(20)     NULL,
    [DateOfBirth]    DATE             NULL,
    [BranchId]       INT              NULL,
    [IsActive]       BIT              NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1),
    [CreatedAt]      DATETIME2(0)     NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]      DATETIME2(0)     NULL
);
GO

ALTER TABLE [Customers]
ADD CONSTRAINT FK_Customers_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

ALTER TABLE [Customers]
ADD CONSTRAINT FK_Customers_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
GO

CREATE UNIQUE INDEX IX_Customers_CustomerNumber ON [Customers]([CustomerNumber]);
GO

/*
    ACCOUNTS & TRANSACTIONS
*/

CREATE TABLE [AccountTypes]
(
    [AccountTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountTypes PRIMARY KEY,
    [Code]          NVARCHAR(50)     NOT NULL,
    [Name]          NVARCHAR(100)    NOT NULL,
    [Description]   NVARCHAR(200)    NULL,
    [IsActive]      BIT              NOT NULL CONSTRAINT DF_AccountTypes_IsActive DEFAULT (1)
);
GO

CREATE UNIQUE INDEX IX_AccountTypes_Code ON [AccountTypes]([Code]);
GO

CREATE TABLE [CustomerAccounts]
(
    [AccountId]       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAccounts PRIMARY KEY,
    [CustomerId]      INT              NOT NULL,
    [AccountTypeId]   INT              NOT NULL,
    [AccountNumber]   NVARCHAR(30)     NOT NULL,
    [Currency]        NVARCHAR(3)      NOT NULL CONSTRAINT DF_CustomerAccounts_Currency DEFAULT N'PHP',
    [Balance]         DECIMAL(18,2)    NOT NULL CONSTRAINT DF_CustomerAccounts_Balance DEFAULT (0),
    [AvailableBalance] DECIMAL(18,2)   NOT NULL CONSTRAINT DF_CustomerAccounts_AvailableBalance DEFAULT (0),
    [Status]          NVARCHAR(30)     NOT NULL CONSTRAINT DF_CustomerAccounts_Status DEFAULT N'Active',
    [OpenedDate]      DATE             NOT NULL CONSTRAINT DF_CustomerAccounts_OpenedDate DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    [BranchId]        INT              NULL,
    [LastTransactionAt] DATETIME2(0)   NULL,
    [CreatedAt]       DATETIME2(0)     NOT NULL CONSTRAINT DF_CustomerAccounts_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]       DATETIME2(0)     NULL
);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_AccountTypes
FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountTypes]([AccountTypeId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
GO

CREATE UNIQUE INDEX IX_CustomerAccounts_AccountNumber ON [CustomerAccounts]([AccountNumber]);
GO

CREATE TABLE [TransactionTypes]
(
    [TransactionTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionTypes PRIMARY KEY,
    [Code]              NVARCHAR(50)     NOT NULL,
    [Name]              NVARCHAR(100)    NOT NULL,
    [IsCredit]          BIT              NOT NULL,
    [Description]       NVARCHAR(200)    NULL
);
GO

CREATE UNIQUE INDEX IX_TransactionTypes_Code ON [TransactionTypes]([Code]);
GO

CREATE TABLE [TransactionStatuses]
(
    [TransactionStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionStatuses PRIMARY KEY,
    [Name]                NVARCHAR(50)     NOT NULL
);
GO

CREATE TABLE [TransactionChannels]
(
    [ChannelId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionChannels PRIMARY KEY,
    [Name]      NVARCHAR(50)      NOT NULL
);
GO

CREATE TABLE [CustomerTransactions]
(
    [TransactionId]        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerTransactions PRIMARY KEY,
    [AccountId]            INT              NOT NULL,
    [CustomerId]           INT              NOT NULL,
    [TransactionTypeId]    INT              NOT NULL,
    [TransactionStatusId]  INT              NOT NULL,
    [ChannelId]            INT              NULL,
    [Amount]               DECIMAL(18,2)    NOT NULL,
    [RunningBalance]       DECIMAL(18,2)    NULL,
    [Description]          NVARCHAR(250)    NULL,
    [ReferenceNumber]      NVARCHAR(100)    NULL,
    [ExternalReference]    NVARCHAR(100)    NULL,
    [TransactionDate]      DATETIME2(0)     NOT NULL CONSTRAINT DF_CustomerTransactions_TransactionDate DEFAULT SYSUTCDATETIME(),
    [PerformedByUserId]    INT              NULL,
    [CreatedAt]            DATETIME2(0)     NOT NULL CONSTRAINT DF_CustomerTransactions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_TransactionTypes
FOREIGN KEY ([TransactionTypeId]) REFERENCES [TransactionTypes]([TransactionTypeId]);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_TransactionStatuses
FOREIGN KEY ([TransactionStatusId]) REFERENCES [TransactionStatuses]([TransactionStatusId]);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_Channels
FOREIGN KEY ([ChannelId]) REFERENCES [TransactionChannels]([ChannelId]);
GO

ALTER TABLE [CustomerTransactions]
ADD CONSTRAINT FK_CustomerTransactions_Users
FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users]([UserId]);
GO

CREATE INDEX IX_CustomerTransactions_AccountId_TransactionDate
    ON [CustomerTransactions]([AccountId], [TransactionDate]);
GO

/*
    BILLS PAYMENT
*/

CREATE TABLE [BillCategories]
(
    [BillCategoryId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BillCategories PRIMARY KEY,
    [Name]           NVARCHAR(100)    NOT NULL,
    [Description]    NVARCHAR(200)    NULL
);
GO

CREATE TABLE [Billers]
(
    [BillerId]       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Billers PRIMARY KEY,
    [BillCategoryId] INT              NOT NULL,
    [Name]           NVARCHAR(150)    NOT NULL,
    [ShortCode]      NVARCHAR(50)     NULL,
    [AccountNumberFormat] NVARCHAR(100) NULL,
    [IsActive]       BIT              NOT NULL CONSTRAINT DF_Billers_IsActive DEFAULT (1)
);
GO

ALTER TABLE [Billers]
ADD CONSTRAINT FK_Billers_BillCategories
FOREIGN KEY ([BillCategoryId]) REFERENCES [BillCategories]([BillCategoryId]);
GO

CREATE TABLE [BillPayments]
(
    [BillPaymentId]       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BillPayments PRIMARY KEY,
    [CustomerTransactionId] BIGINT          NOT NULL,
    [CustomerId]          INT               NOT NULL,
    [AccountId]           INT               NOT NULL,
    [BillerId]            INT               NOT NULL,
    [BillerAccountNumber] NVARCHAR(100)     NOT NULL,
    [Amount]              DECIMAL(18,2)     NOT NULL,
    [DueDate]             DATE              NULL,
    [PaymentDate]         DATETIME2(0)      NOT NULL CONSTRAINT DF_BillPayments_PaymentDate DEFAULT SYSUTCDATETIME(),
    [Remarks]             NVARCHAR(250)     NULL
);
GO

ALTER TABLE [BillPayments]
ADD CONSTRAINT FK_BillPayments_Transactions
FOREIGN KEY ([CustomerTransactionId]) REFERENCES [CustomerTransactions]([TransactionId]);
GO

ALTER TABLE [BillPayments]
ADD CONSTRAINT FK_BillPayments_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [BillPayments]
ADD CONSTRAINT FK_BillPayments_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [BillPayments]
ADD CONSTRAINT FK_BillPayments_Billers
FOREIGN KEY ([BillerId]) REFERENCES [Billers]([BillerId]);
GO

/*
    LOANS
*/

CREATE TABLE [LoanProducts]
(
    [LoanProductId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanProducts PRIMARY KEY,
    [Code]          NVARCHAR(50)      NOT NULL,
    [Name]          NVARCHAR(150)     NOT NULL,
    [Description]   NVARCHAR(250)     NULL,
    [InterestRate]  DECIMAL(9,4)      NOT NULL,
    [MinTermMonths] INT               NULL,
    [MaxTermMonths] INT               NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT DF_LoanProducts_IsActive DEFAULT (1)
);
GO

CREATE UNIQUE INDEX IX_LoanProducts_Code ON [LoanProducts]([Code]);
GO

CREATE TABLE [LoanApplications]
(
    [LoanApplicationId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanApplications PRIMARY KEY,
    [CustomerId]        INT               NOT NULL,
    [LoanProductId]     INT               NOT NULL,
    [AccountId]         INT               NULL,
    [RequestedAmount]   DECIMAL(18,2)     NOT NULL,
    [TermMonths]        INT               NOT NULL,
    [Status]            NVARCHAR(30)      NOT NULL CONSTRAINT DF_LoanApplications_Status DEFAULT N'Pending',
    [Purpose]           NVARCHAR(250)     NULL,
    [SubmittedAt]       DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanApplications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    [ReviewedByEmployeeId] INT            NULL,
    [ReviewedAt]        DATETIME2(0)      NULL
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
ADD CONSTRAINT FK_LoanApplications_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [LoanApplications]
ADD CONSTRAINT FK_LoanApplications_Employees
FOREIGN KEY ([ReviewedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE TABLE [Loans]
(
    [LoanId]          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
    [LoanApplicationId] BIGINT           NULL,
    [CustomerId]      INT                NOT NULL,
    [LoanProductId]   INT                NOT NULL,
    [AccountId]       INT                NULL,
    [PrincipalAmount] DECIMAL(18,2)      NOT NULL,
    [InterestRate]    DECIMAL(9,4)       NOT NULL,
    [TermMonths]      INT                NOT NULL,
    [StartDate]       DATE               NOT NULL,
    [MaturityDate]    DATE               NULL,
    [Status]          NVARCHAR(30)       NOT NULL CONSTRAINT DF_Loans_Status DEFAULT N'Active',
    [CreatedAt]       DATETIME2(0)       NOT NULL CONSTRAINT DF_Loans_CreatedAt DEFAULT SYSUTCDATETIME()
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
ADD CONSTRAINT FK_Loans_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

CREATE TABLE [LoanRepayments]
(
    [RepaymentId]        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanRepayments PRIMARY KEY,
    [LoanId]             BIGINT           NOT NULL,
    [ScheduledDate]      DATE             NOT NULL,
    [AmountDue]          DECIMAL(18,2)    NOT NULL,
    [AmountPaid]         DECIMAL(18,2)    NULL,
    [PaidDate]           DATETIME2(0)     NULL,
    [CustomerTransactionId] BIGINT        NULL,
    [Status]             NVARCHAR(30)     NOT NULL CONSTRAINT DF_LoanRepayments_Status DEFAULT N'Pending'
);
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_Loans
FOREIGN KEY ([LoanId]) REFERENCES [Loans]([LoanId]);
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_Transactions
FOREIGN KEY ([CustomerTransactionId]) REFERENCES [CustomerTransactions]([TransactionId]);
GO

/*
    CARDS
*/

CREATE TABLE [CardTypes]
(
    [CardTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CardTypes PRIMARY KEY,
    [Code]       NVARCHAR(50)      NOT NULL,
    [Name]       NVARCHAR(100)     NOT NULL,
    [Description] NVARCHAR(200)    NULL
);
GO

CREATE UNIQUE INDEX IX_CardTypes_Code ON [CardTypes]([Code]);
GO

CREATE TABLE [Cards]
(
    [CardId]        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cards PRIMARY KEY,
    [CustomerId]    INT              NOT NULL,
    [AccountId]     INT              NULL,
    [CardTypeId]    INT              NOT NULL,
    [CardNumber]    NVARCHAR(30)     NOT NULL,
    [ExpiryMonth]   TINYINT          NOT NULL,
    [ExpiryYear]    SMALLINT         NOT NULL,
    [CvvHash]       NVARCHAR(100)    NULL,
    [CreditLimit]   DECIMAL(18,2)    NULL,
    [Status]        NVARCHAR(30)     NOT NULL CONSTRAINT DF_Cards_Status DEFAULT N'Active',
    [CreatedAt]     DATETIME2(0)     NOT NULL CONSTRAINT DF_Cards_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [Cards]
ADD CONSTRAINT FK_Cards_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [Cards]
ADD CONSTRAINT FK_Cards_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

ALTER TABLE [Cards]
ADD CONSTRAINT FK_Cards_CardTypes
FOREIGN KEY ([CardTypeId]) REFERENCES [CardTypes]([CardTypeId]);
GO

CREATE UNIQUE INDEX IX_Cards_CardNumber ON [Cards]([CardNumber]);
GO

CREATE TABLE [CardApplications]
(
    [CardApplicationId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CardApplications PRIMARY KEY,
    [CustomerId]        INT               NOT NULL,
    [CardTypeId]        INT               NOT NULL,
    [RequestedLimit]    DECIMAL(18,2)     NULL,
    [Status]            NVARCHAR(30)      NOT NULL CONSTRAINT DF_CardApplications_Status DEFAULT N'Pending',
    [SubmittedAt]       DATETIME2(0)      NOT NULL CONSTRAINT DF_CardApplications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    [ReviewedByEmployeeId] INT           NULL,
    [ReviewedAt]        DATETIME2(0)      NULL
);
GO

ALTER TABLE [CardApplications]
ADD CONSTRAINT FK_CardApplications_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [CardApplications]
ADD CONSTRAINT FK_CardApplications_CardTypes
FOREIGN KEY ([CardTypeId]) REFERENCES [CardTypes]([CardTypeId]);
GO

ALTER TABLE [CardApplications]
ADD CONSTRAINT FK_CardApplications_Employees
FOREIGN KEY ([ReviewedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

/*
    SAVINGS GOALS & REWARDS
*/

CREATE TABLE [SavingsGoals]
(
    [SavingsGoalId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SavingsGoals PRIMARY KEY,
    [CustomerId]    INT              NOT NULL,
    [AccountId]     INT              NOT NULL,
    [Name]          NVARCHAR(150)    NOT NULL,
    [TargetAmount]  DECIMAL(18,2)    NOT NULL,
    [CurrentAmount] DECIMAL(18,2)    NOT NULL CONSTRAINT DF_SavingsGoals_CurrentAmount DEFAULT (0),
    [TargetDate]    DATE             NULL,
    [Status]        NVARCHAR(30)     NOT NULL CONSTRAINT DF_SavingsGoals_Status DEFAULT N'Active',
    [CreatedAt]     DATETIME2(0)     NOT NULL CONSTRAINT DF_SavingsGoals_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [SavingsGoals]
ADD CONSTRAINT FK_SavingsGoals_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [SavingsGoals]
ADD CONSTRAINT FK_SavingsGoals_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

CREATE TABLE [RewardPoints]
(
    [RewardPointsId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RewardPoints PRIMARY KEY,
    [CustomerId]     INT              NOT NULL,
    [AccountId]      INT              NULL,
    [TotalPoints]    DECIMAL(18,2)    NOT NULL CONSTRAINT DF_RewardPoints_TotalPoints DEFAULT (0),
    [LastUpdatedAt]  DATETIME2(0)     NOT NULL CONSTRAINT DF_RewardPoints_LastUpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [RewardPoints]
ADD CONSTRAINT FK_RewardPoints_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [RewardPoints]
ADD CONSTRAINT FK_RewardPoints_Accounts
FOREIGN KEY ([AccountId]) REFERENCES [CustomerAccounts]([AccountId]);
GO

CREATE TABLE [RewardPointTransactions]
(
    [RewardPointTransactionId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RewardPointTransactions PRIMARY KEY,
    [RewardPointsId]           INT               NOT NULL,
    [CustomerTransactionId]    BIGINT            NULL,
    [PointsChange]             DECIMAL(18,2)     NOT NULL,
    [Reason]                   NVARCHAR(200)     NULL,
    [CreatedAt]                DATETIME2(0)      NOT NULL CONSTRAINT DF_RewardPointTransactions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [RewardPointTransactions]
ADD CONSTRAINT FK_RewardPointTransactions_RewardPoints
FOREIGN KEY ([RewardPointsId]) REFERENCES [RewardPoints]([RewardPointsId]);
GO

ALTER TABLE [RewardPointTransactions]
ADD CONSTRAINT FK_RewardPointTransactions_Transactions
FOREIGN KEY ([CustomerTransactionId]) REFERENCES [CustomerTransactions]([TransactionId]);
GO

/*
    APPROVAL QUEUE & EMPLOYEE TASKS
*/

CREATE TABLE [ApprovalStatuses]
(
    [ApprovalStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApprovalStatuses PRIMARY KEY,
    [Name]             NVARCHAR(50)     NOT NULL
);
GO

CREATE TABLE [ApprovalQueueItems]
(
    [ApprovalQueueItemId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApprovalQueueItems PRIMARY KEY,
    [EntityType]          NVARCHAR(50)     NOT NULL,  -- e.g., 'LoanApplication', 'CardApplication', 'HighValueTransaction'
    [EntityId]            BIGINT           NOT NULL,
    [RequestedByUserId]   INT              NOT NULL,
    [AssignedToUserId]    INT              NULL,
    [ApprovalStatusId]    INT              NOT NULL,
    [Priority]            NVARCHAR(20)     NOT NULL CONSTRAINT DF_ApprovalQueueItems_Priority DEFAULT N'Normal',
    [Remarks]             NVARCHAR(250)    NULL,
    [RequestedAt]         DATETIME2(0)     NOT NULL CONSTRAINT DF_ApprovalQueueItems_RequestedAt DEFAULT SYSUTCDATETIME(),
    [CompletedAt]         DATETIME2(0)     NULL,
    [ApprovedByUserId]    INT              NULL
);
GO

ALTER TABLE [ApprovalQueueItems]
ADD CONSTRAINT FK_ApprovalQueueItems_RequestedBy
FOREIGN KEY ([RequestedByUserId]) REFERENCES [Users]([UserId]);
GO

ALTER TABLE [ApprovalQueueItems]
ADD CONSTRAINT FK_ApprovalQueueItems_AssignedTo
FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users]([UserId]);
GO

ALTER TABLE [ApprovalQueueItems]
ADD CONSTRAINT FK_ApprovalQueueItems_ApprovalStatuses
FOREIGN KEY ([ApprovalStatusId]) REFERENCES [ApprovalStatuses]([ApprovalStatusId]);
GO

ALTER TABLE [ApprovalQueueItems]
ADD CONSTRAINT FK_ApprovalQueueItems_ApprovedBy
FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users]([UserId]);
GO

CREATE TABLE [EmployeeTasks]
(
    [EmployeeTaskId]  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeTasks PRIMARY KEY,
    [AssignedToEmployeeId] INT          NOT NULL,
    [CreatedByUserId] INT               NOT NULL,
    [Title]           NVARCHAR(150)     NOT NULL,
    [Description]     NVARCHAR(500)     NULL,
    [Status]          NVARCHAR(30)      NOT NULL CONSTRAINT DF_EmployeeTasks_Status DEFAULT N'Pending',
    [Priority]        NVARCHAR(20)      NOT NULL CONSTRAINT DF_EmployeeTasks_Priority DEFAULT N'Normal',
    [DueDate]         DATE              NULL,
    [CreatedAt]       DATETIME2(0)      NOT NULL CONSTRAINT DF_EmployeeTasks_CreatedAt DEFAULT SYSUTCDATETIME(),
    [CompletedAt]     DATETIME2(0)      NULL,
    [RelatedEntityType] NVARCHAR(50)    NULL,
    [RelatedEntityId]   BIGINT          NULL
);
GO

ALTER TABLE [EmployeeTasks]
ADD CONSTRAINT FK_EmployeeTasks_Employees
FOREIGN KEY ([AssignedToEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

ALTER TABLE [EmployeeTasks]
ADD CONSTRAINT FK_EmployeeTasks_Users
FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([UserId]);
GO

/*
    USER SETTINGS, NOTIFICATIONS & AUDIT
*/

CREATE TABLE [NotificationPreferences]
(
    [NotificationPreferenceId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationPreferences PRIMARY KEY,
    [UserId]         INT              NOT NULL,
    [EmailEnabled]   BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_EmailEnabled DEFAULT (1),
    [SmsEnabled]     BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_SmsEnabled DEFAULT (0),
    [PushEnabled]    BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_PushEnabled DEFAULT (0),
    [Language]       NVARCHAR(10)     NOT NULL CONSTRAINT DF_NotificationPreferences_Language DEFAULT N'en',
    [DarkMode]       BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_DarkMode DEFAULT (0),
    [HighContrast]   BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_HighContrast DEFAULT (0),
    [ReduceMotion]   BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_ReduceMotion DEFAULT (0),
    [ColorBlindMode] BIT              NOT NULL CONSTRAINT DF_NotificationPreferences_ColorBlindMode DEFAULT (0),
    [UpdatedAt]      DATETIME2(0)     NOT NULL CONSTRAINT DF_NotificationPreferences_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [NotificationPreferences]
ADD CONSTRAINT FK_NotificationPreferences_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

CREATE TABLE [AuditLogs]
(
    [AuditLogId]  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    [UserId]      INT               NULL,
    [Action]      NVARCHAR(100)     NOT NULL,
    [EntityName]  NVARCHAR(100)     NULL,
    [EntityId]    BIGINT            NULL,
    [OldValues]   NVARCHAR(MAX)     NULL,
    [NewValues]   NVARCHAR(MAX)     NULL,
    [CreatedAt]   DATETIME2(0)      NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
    [IpAddress]   NVARCHAR(50)      NULL,
    [UserAgent]   NVARCHAR(200)     NULL
);
GO

ALTER TABLE [AuditLogs]
ADD CONSTRAINT FK_AuditLogs_Users
FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]);
GO

-- END OF FINEBANK SCHEMA
