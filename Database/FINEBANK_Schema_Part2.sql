-- ============================================================================
-- FINEBANK DATABASE SCHEMA - PART 2
-- Deposits, Withdrawals, Transfers, Loans
-- ============================================================================

USE [FINEBANK];
GO

-- ============================================================================
-- SECTION 6: TRANSACTION TYPES & CHANNELS
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
-- SECTION 7: DEPOSITS
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

-- ============================================================================
-- SECTION 8: WITHDRAWALS
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
-- SECTION 9: TRANSFERS (INTERNAL)
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
-- SECTION 10: LOANS
-- ============================================================================

CREATE TABLE [LoanProducts]
(
    [LoanProductId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanProducts PRIMARY KEY,
    [Code]          NVARCHAR(50)      NOT NULL,
    [Name]          NVARCHAR(150)     NOT NULL,
    [Description]   NVARCHAR(250)     NULL,
    [InterestRate]  DECIMAL(9,4)      NOT NULL,
    [MinAmount]     DECIMAL(18,2)     NULL,
    [MaxAmount]     DECIMAL(18,2)     NULL,
    [MinTermMonths] INT               NULL,
    [MaxTermMonths] INT               NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT DF_LoanProducts_IsActive DEFAULT (1)
);
GO

CREATE UNIQUE INDEX IX_LoanProducts_Code ON [LoanProducts]([Code]);
GO

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
(N'TellerReview', N'Under review by teller'),
(N'ManagerReview', N'Under review by finance manager'),
(N'Approved', N'Application approved'),
(N'Rejected', N'Application rejected'),
(N'Cancelled', N'Application cancelled');
GO

CREATE TABLE [LoanApplications]
(
    [LoanApplicationId]      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanApplications PRIMARY KEY,
    [CustomerId]             INT               NOT NULL,
    [LoanProductId]          INT               NOT NULL,
    [AccountId]              INT               NULL,
    [RequestedAmount]        DECIMAL(18,2)     NOT NULL,
    [TermMonths]             INT               NOT NULL,
    [Purpose]                NVARCHAR(250)     NULL,
    [LoanApplicationStatusId] INT              NOT NULL,
    [SubmittedAt]            DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanApplications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    [SubmittedByEmployeeId]  INT               NULL,
    [ReviewedByEmployeeId]   INT               NULL,
    [ReviewedAt]             DATETIME2(0)      NULL,
    [ApprovalRemarks]        NVARCHAR(500)     NULL,
    [CreatedAt]              DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanApplications_CreatedAt DEFAULT SYSUTCDATETIME()
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

CREATE TABLE [LoanStatuses]
(
    [LoanStatusId] INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanStatuses PRIMARY KEY,
    [StatusName]   NVARCHAR(50)      NOT NULL,
    [Description]  NVARCHAR(200)     NULL
);
GO

INSERT INTO [LoanStatuses] ([StatusName], [Description])
VALUES
(N'Active', N'Loan is active'),
(N'Completed', N'Loan has been fully repaid'),
(N'Defaulted', N'Loan is in default'),
(N'Cancelled', N'Loan was cancelled');
GO

CREATE TABLE [Loans]
(
    [LoanId]              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
    [LoanApplicationId]   BIGINT            NULL,
    [CustomerId]          INT               NOT NULL,
    [LoanProductId]       INT               NOT NULL,
    [AccountId]           INT               NULL,
    [PrincipalAmount]     DECIMAL(18,2)     NOT NULL,
    [InterestRate]        DECIMAL(9,4)      NOT NULL,
    [TermMonths]          INT               NOT NULL,
    [StartDate]           DATE              NOT NULL,
    [MaturityDate]        DATE              NULL,
    [DisbursedAmount]     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Loans_DisbursedAmount DEFAULT (0),
    [RemainingBalance]    DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Loans_RemainingBalance DEFAULT (0),
    [LoanStatusId]        INT               NOT NULL,
    [DisbursedByEmployeeId] INT             NULL,
    [DisbursedAt]         DATETIME2(0)      NULL,
    [CreatedAt]           DATETIME2(0)      NOT NULL CONSTRAINT DF_Loans_CreatedAt DEFAULT SYSUTCDATETIME()
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

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_LoanStatuses
FOREIGN KEY ([LoanStatusId]) REFERENCES [LoanStatuses]([LoanStatusId]);
GO

ALTER TABLE [Loans]
ADD CONSTRAINT FK_Loans_Employees
FOREIGN KEY ([DisbursedByEmployeeId]) REFERENCES [Employees]([EmployeeId]);
GO

CREATE TABLE [LoanRepayments]
(
    [RepaymentId]        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanRepayments PRIMARY KEY,
    [LoanId]             BIGINT            NOT NULL,
    [ScheduledDate]      DATE              NOT NULL,
    [AmountDue]          DECIMAL(18,2)     NOT NULL,
    [AmountPaid]         DECIMAL(18,2)     NULL,
    [PaidDate]           DATETIME2(0)      NULL,
    [RepaymentStatusId]  INT               NOT NULL,
    [CreatedAt]          DATETIME2(0)      NOT NULL CONSTRAINT DF_LoanRepayments_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE [LoanRepayments]
ADD CONSTRAINT FK_LoanRepayments_Loans
FOREIGN KEY ([LoanId]) REFERENCES [Loans]([LoanId]);
GO

CREATE INDEX IX_LoanRepayments_LoanId_ScheduledDate ON [LoanRepayments]([LoanId], [ScheduledDate]);
GO
