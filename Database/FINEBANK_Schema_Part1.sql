-- ============================================================================
-- FINEBANK DATABASE SCHEMA - PART 1
-- Process-Based Design: Deposits, Withdrawals, Transfers, Loans, Accounts, Finance
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
(3, N'Accountant',     N'Accounting staff for finance management', 1),
(4, N'FinanceManager', N'Finance manager for approvals', 1),
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

CREATE TABLE [Branches]
(
    [BranchId]    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Branches PRIMARY KEY,
    [BranchCode]  NVARCHAR(20)      NOT NULL,
    [BranchName]  NVARCHAR(150)     NOT NULL,
    [AddressLine1] NVARCHAR(200)    NULL,
    [AddressLine2] NVARCHAR(200)    NULL,
    [City]        NVARCHAR(100)     NULL,
    [Province]    NVARCHAR(100)     NULL,
    [PostalCode]  NVARCHAR(20)      NULL,
    [PhoneNumber] NVARCHAR(50)      NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT DF_Branches_IsActive DEFAULT (1),
    [CreatedAt]   DATETIME2(0)      NOT NULL CONSTRAINT DF_Branches_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX IX_Branches_BranchCode ON [Branches]([BranchCode]);
GO

CREATE TABLE [AccountTypes]
(
    [AccountTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountTypes PRIMARY KEY,
    [Code]          NVARCHAR(50)      NOT NULL,
    [Name]          NVARCHAR(100)     NOT NULL,
    [Description]   NVARCHAR(200)     NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT DF_AccountTypes_IsActive DEFAULT (1)
);
GO

CREATE UNIQUE INDEX IX_AccountTypes_Code ON [AccountTypes]([Code]);
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
    [BranchId]        INT               NULL,
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

ALTER TABLE [Employees]
ADD CONSTRAINT FK_Employees_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
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
    [BranchId]       INT               NULL,
    [IsActive]       BIT               NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1),
    [CreatedAt]      DATETIME2(0)      NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]      DATETIME2(0)      NULL
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

-- ============================================================================
-- SECTION 3: KYC & CUSTOMER VERIFICATION
-- ============================================================================

CREATE TABLE [ValidIDTypes]
(
    [ValidIDTypeId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ValidIDTypes PRIMARY KEY,
    [IDTypeName]    NVARCHAR(100)     NOT NULL,
    [Description]   NVARCHAR(200)     NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT DF_ValidIDTypes_IsActive DEFAULT (1)
);
GO

CREATE TABLE [CustomerValidIDs]
(
    [CustomerValidIDId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerValidIDs PRIMARY KEY,
    [CustomerId]        INT               NOT NULL,
    [ValidIDTypeId]     INT               NOT NULL,
    [IDNumber]          NVARCHAR(100)     NOT NULL,
    [IssuedDate]        DATE              NULL,
    [ExpiryDate]        DATE              NULL,
    [DocumentPath]      NVARCHAR(500)     NULL,
    [IsVerified]        BIT               NOT NULL CONSTRAINT DF_CustomerValidIDs_IsVerified DEFAULT (0),
    [VerifiedByEmployeeId] INT            NULL,
    [VerifiedAt]        DATETIME2(0)      NULL,
    [CreatedAt]         DATETIME2(0)      NOT NULL CONSTRAINT DF_CustomerValidIDs_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [CustomerValidIDs]
ADD CONSTRAINT FK_CustomerValidIDs_Customers
FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]);
GO

ALTER TABLE [CustomerValidIDs]
ADD CONSTRAINT FK_CustomerValidIDs_ValidIDTypes
FOREIGN KEY ([ValidIDTypeId]) REFERENCES [ValidIDTypes]([ValidIDTypeId]);
GO

ALTER TABLE [CustomerValidIDs]
ADD CONSTRAINT FK_CustomerValidIDs_Employees
FOREIGN KEY ([VerifiedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

-- ============================================================================
-- SECTION 4: ACCOUNTS & ACCOUNT MANAGEMENT
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
    [AccountTypeId]     INT               NOT NULL,
    [AccountNumber]     NVARCHAR(30)      NOT NULL,
    [Currency]          NVARCHAR(3)       NOT NULL CONSTRAINT DF_CustomerAccounts_Currency DEFAULT N'PHP',
    [Balance]           DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CustomerAccounts_Balance DEFAULT (0),
    [AvailableBalance]  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CustomerAccounts_AvailableBalance DEFAULT (0),
    [AccountStatusId]   INT               NOT NULL,
    [OpenedDate]        DATE              NOT NULL CONSTRAINT DF_CustomerAccounts_OpenedDate DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    [ClosedDate]        DATE              NULL,
    [BranchId]          INT               NULL,
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
ADD CONSTRAINT FK_CustomerAccounts_AccountTypes
FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountTypes]([AccountTypeId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_AccountStatuses
FOREIGN KEY ([AccountStatusId]) REFERENCES [AccountStatuses]([AccountStatusId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Branches
FOREIGN KEY ([BranchId]) REFERENCES [Branches]([BranchId]);
GO

ALTER TABLE [CustomerAccounts]
ADD CONSTRAINT FK_CustomerAccounts_Employees
FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE UNIQUE INDEX IX_CustomerAccounts_AccountNumber ON [CustomerAccounts]([AccountNumber]);
GO

-- ============================================================================
-- SECTION 5: WITHDRAWAL LIMITS
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
