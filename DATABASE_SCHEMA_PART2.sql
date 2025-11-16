-- =============================================
-- COMPLETE DATABASE SCHEMA FOR ALL PAGES
-- Part 2: Accounting, Finance & System Modules
-- =============================================

-- =============================================
-- ACCOUNTING MODULE TABLES
-- =============================================

-- Journal Entries
CREATE TABLE JournalEntries (
    JournalId INT IDENTITY(1,1) PRIMARY KEY,
    JournalNumber NVARCHAR(50) NOT NULL UNIQUE,
    TransactionDate DATE NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Reference NVARCHAR(100) NULL,
    TotalDebit DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCredit DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Posted, Reversed
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL,
    PostedAt DATETIME NULL,
    PostedBy NVARCHAR(50) NULL
);

-- Journal Entry Lines
CREATE TABLE JournalEntryLines (
    LineId INT IDENTITY(1,1) PRIMARY KEY,
    JournalId INT NOT NULL,
    AccountCode NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_JournalEntryLines_Journal FOREIGN KEY (JournalId) REFERENCES JournalEntries(JournalId)
);

-- General Ledger
CREATE TABLE GeneralLedger (
    LedgerId INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL, -- Asset, Liability, Equity, Revenue, Expense
    ParentAccountCode NVARCHAR(50) NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- General Ledger Transactions
CREATE TABLE GeneralLedgerTransactions (
    GLTransactionId INT IDENTITY(1,1) PRIMARY KEY,
    JournalLineId INT NOT NULL,
    AccountCode NVARCHAR(50) NOT NULL,
    TransactionDate DATE NOT NULL,
    Description NVARCHAR(500) NULL,
    DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    RunningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_GLTransactions_JournalLines FOREIGN KEY (JournalLineId) REFERENCES JournalEntryLines(LineId)
);

-- Trial Balance
CREATE TABLE TrialBalance (
    TrialBalanceId INT IDENTITY(1,1) PRIMARY KEY,
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    AccountCode NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    DebitBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    GeneratedAt DATETIME NOT NULL DEFAULT GETDATE(),
    GeneratedBy NVARCHAR(50) NOT NULL
);

-- Financial Statements
CREATE TABLE FinancialStatements (
    StatementId INT IDENTITY(1,1) PRIMARY KEY,
    StatementType NVARCHAR(50) NOT NULL, -- Income Statement, Balance Sheet, Cash Flow
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    StatementData NVARCHAR(MAX) NULL, -- JSON data
    GeneratedAt DATETIME NOT NULL DEFAULT GETDATE(),
    GeneratedBy NVARCHAR(50) NOT NULL
);

-- Chart of Accounts
CREATE TABLE ChartOfAccounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(50) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,
    ParentAccountCode NVARCHAR(50) NULL,
    Level INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- =============================================
-- FINANCE MODULE TABLES
-- =============================================

-- Accounts Payable
CREATE TABLE AccountsPayable (
    PayableId INT IDENTITY(1,1) PRIMARY KEY,
    VendorName NVARCHAR(100) NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    OutstandingAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Partial, Paid, Overdue
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- Accounts Receivable
CREATE TABLE AccountsReceivable (
    ReceivableId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ReceivedAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    OutstandingAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Partial, Received, Overdue
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- Budget Management
CREATE TABLE BudgetManagement (
    BudgetId INT IDENTITY(1,1) PRIMARY KEY,
    BudgetName NVARCHAR(100) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    AllocatedAmount DECIMAL(18,2) NOT NULL,
    SpentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    RemainingAmount DECIMAL(18,2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- Financial Forecasting
CREATE TABLE FinancialForecasting (
    ForecastId INT IDENTITY(1,1) PRIMARY KEY,
    ForecastName NVARCHAR(100) NOT NULL,
    ForecastType NVARCHAR(50) NOT NULL, -- Revenue, Expense, Cashflow
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    ProjectedAmount DECIMAL(18,2) NOT NULL,
    ActualAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Variance DECIMAL(5,2) NOT NULL DEFAULT 0,
    Assumptions NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- Cash Flow Analysis
CREATE TABLE CashflowAnalysis (
    CashflowId INT IDENTITY(1,1) PRIMARY KEY,
    EntryType NVARCHAR(50) NOT NULL, -- Inflow, Outflow
    Category NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    TransactionDate DATE NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- =============================================
-- SYSTEM MODULE TABLES
-- =============================================

-- User Registration (Admin)
CREATE TABLE UserRegistrations (
    RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Department NVARCHAR(50) NULL,
    EmployeeId NVARCHAR(50) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NOT NULL
);

-- Audit Logs
CREATE TABLE AuditLogs (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Module NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- System Settings
CREATE TABLE SystemSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(1000) NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastModifiedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastModifiedBy NVARCHAR(50) NOT NULL
);

-- =============================================
-- APPROVAL MODULE TABLES
-- =============================================

-- Approval Queue
CREATE TABLE ApprovalQueue (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestType NVARCHAR(50) NOT NULL, -- Transfer, Deposit, Withdrawal, Loan, Card
    RequestId INT NOT NULL,
    RequestedBy NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
    Priority NVARCHAR(50) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Urgent
    Description NVARCHAR(500) NULL,
    RequestedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy NVARCHAR(50) NULL,
    ApprovalNotes NVARCHAR(500) NULL
);

-- =============================================
-- REPORTS MODULE TABLES
-- =============================================

-- System Reports
CREATE TABLE SystemReports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ReportType NVARCHAR(50) NOT NULL, -- Banking, Accounting, Finance, User Activity
    ReportName NVARCHAR(100) NOT NULL,
    ReportData NVARCHAR(MAX) NULL, -- JSON data
    Parameters NVARCHAR(MAX) NULL, -- JSON parameters
    GeneratedAt DATETIME NOT NULL DEFAULT GETDATE(),
    GeneratedBy NVARCHAR(50) NOT NULL,
    AccessedCount INT NOT NULL DEFAULT 0,
    LastAccessedAt DATETIME NULL
);
