-- =============================================
-- COMPLETE DATABASE SCHEMA FOR ALL PAGES
-- Part 1: Customer Portal & Banking Module
-- =============================================

-- =============================================
-- CUSTOMER PORTAL TABLES
-- =============================================

-- Customer Accounts
CREATE TABLE CustomerAccounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    AccountNumber NVARCHAR(50) NOT NULL UNIQUE,
    AccountType NVARCHAR(50) NOT NULL, -- Savings, Checking, Credit
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    AvailableBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'PHP',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastTransactionAt DATETIME NULL,
    CONSTRAINT FK_CustomerAccounts_Users FOREIGN KEY (CustomerId) REFERENCES Users(UserId)
);

-- Customer Transactions
CREATE TABLE CustomerTransactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    TransactionNumber NVARCHAR(50) NOT NULL UNIQUE,
    AccountId INT NOT NULL,
    TransactionType NVARCHAR(50) NOT NULL, -- Transfer, Deposit, Withdrawal, Bills
    Amount DECIMAL(18,2) NOT NULL,
    Fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Description NVARCHAR(500) NULL,
    Reference NVARCHAR(100) NULL,
    ToAccountId INT NULL,
    ToAccountName NVARCHAR(100) NULL,
    BillerName NVARCHAR(100) NULL,
    BillerAccountNumber NVARCHAR(50) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy NVARCHAR(50) NULL,
    CONSTRAINT FK_CustomerTransactions_Accounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId),
    CONSTRAINT FK_CustomerTransactions_ToAccounts FOREIGN KEY (ToAccountId) REFERENCES CustomerAccounts(AccountId)
);

-- Customer Cards
CREATE TABLE CustomerCards (
    CardId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    CardNumber NVARCHAR(20) NOT NULL UNIQUE,
    CardType NVARCHAR(50) NOT NULL, -- Debit, Credit
    CardName NVARCHAR(100) NOT NULL,
    CreditLimit DECIMAL(18,2) NULL DEFAULT 0,
    AvailableCredit DECIMAL(18,2) NULL DEFAULT 0,
    OutstandingBalance DECIMAL(18,2) NULL DEFAULT 0,
    ExpiryDate DATE NOT NULL,
    DueDate DATE NULL,
    InterestRate DECIMAL(5,2) NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsLocked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CustomerCards_Accounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId)
);

-- Customer Loans
CREATE TABLE CustomerLoans (
    LoanId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    LoanNumber NVARCHAR(50) NOT NULL UNIQUE,
    LoanType NVARCHAR(50) NOT NULL, -- Personal, Home, Auto, Education
    LoanAmount DECIMAL(18,2) NOT NULL,
    OutstandingBalance DECIMAL(18,2) NOT NULL,
    InterestRate DECIMAL(5,2) NOT NULL,
    TermMonths INT NOT NULL,
    MonthlyPayment DECIMAL(18,2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    NextDueDate DATE NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    Purpose NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CustomerLoans_Accounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId)
);

-- Customer Savings Goals
CREATE TABLE CustomerSavingsGoals (
    GoalId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    GoalName NVARCHAR(100) NOT NULL,
    TargetAmount DECIMAL(18,2) NOT NULL,
    CurrentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartDate DATE NOT NULL DEFAULT GETDATE(),
    TargetDate DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CustomerSavingsGoals_Accounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId)
);

-- Customer Reward Points
CREATE TABLE CustomerRewardPoints (
    RewardId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    Points INT NOT NULL DEFAULT 0,
    CashbackAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    EarnedFrom NVARCHAR(50) NOT NULL,
    Description NVARCHAR(100) NULL,
    EarnedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME NULL,
    IsRedeemed BIT NOT NULL DEFAULT 0,
    RedeemedAt DATETIME NULL,
    CONSTRAINT FK_CustomerRewardPoints_Accounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId)
);

-- =============================================
-- BANKING MODULE TABLES
-- =============================================

-- Bank Accounts Management
CREATE TABLE BankAccounts (
    BankAccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountNumber NVARCHAR(50) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,
    BankName NVARCHAR(100) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'PHP',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    LastModifiedAt DATETIME NULL,
    LastModifiedBy NVARCHAR(50) NULL
);

-- Fund Transfers
CREATE TABLE FundTransfers (
    TransferId INT IDENTITY(1,1) PRIMARY KEY,
    TransferNumber NVARCHAR(50) NOT NULL UNIQUE,
    FromAccountId INT NOT NULL,
    ToAccountId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy NVARCHAR(50) NULL,
    ApprovedBy NVARCHAR(50) NULL,
    ApprovedAt DATETIME NULL,
    CONSTRAINT FK_FundTransfers_FromAccount FOREIGN KEY (FromAccountId) REFERENCES BankAccounts(BankAccountId),
    CONSTRAINT FK_FundTransfers_ToAccount FOREIGN KEY (ToAccountId) REFERENCES BankAccounts(BankAccountId)
);

-- Billers Management
CREATE TABLE BillersManagement (
    BillerId INT IDENTITY(1,1) PRIMARY KEY,
    BillerName NVARCHAR(100) NOT NULL,
    BillerCode NVARCHAR(50) NOT NULL UNIQUE,
    Category NVARCHAR(50) NOT NULL, -- Utilities, Telecom, Government, etc.
    Description NVARCHAR(500) NULL,
    ProcessingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL
);

-- Loan Management (Admin Side)
CREATE TABLE LoanManagement (
    LoanMgmtId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerLoanId INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ApprovedBy NVARCHAR(50) NULL,
    ApprovedAt DATETIME NULL,
    RejectionReason NVARCHAR(500) NULL,
    ProcessingNotes NVARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LoanManagement_CustomerLoans FOREIGN KEY (CustomerLoanId) REFERENCES CustomerLoans(LoanId)
);

-- Card Management (Admin Side)
CREATE TABLE CardManagement (
    CardMgmtId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerCardId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Issue, Block, Unblock, Close
    Reason NVARCHAR(500) NULL,
    ActionBy NVARCHAR(50) NOT NULL,
    ActionAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CardManagement_CustomerCards FOREIGN KEY (CustomerCardId) REFERENCES CustomerCards(CardId)
);

-- Banking Reports
CREATE TABLE BankingReports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ReportType NVARCHAR(50) NOT NULL,
    ReportName NVARCHAR(100) NOT NULL,
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    Data NVARCHAR(MAX) NULL, -- JSON data
    GeneratedAt DATETIME NOT NULL DEFAULT GETDATE(),
    GeneratedBy NVARCHAR(50) NOT NULL
);
