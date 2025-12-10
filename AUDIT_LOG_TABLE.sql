-- =============================================
-- COMPREHENSIVE AUDIT LOG TABLE FOR BANKING COMPLIANCE
-- SOX (Sarbanes-Oxley), BSA/AML (Bank Secrecy Act), PCI DSS Compliance
-- =============================================
-- Created: For FineBank Financial System
-- Purpose: Track ALL banking transactions for audit trail
-- =============================================

-- Drop existing table if needed (CAUTION: This will delete existing data)
-- IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;

CREATE TABLE [dbo].[AuditLogs] (
    -- Primary Key
    [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    -- ==================== CORE AUDIT FIELDS ====================
    -- Who performed the action (User/System ID)
    [UserId] NVARCHAR(50) NOT NULL,
    
    -- What action was performed
    [Action] NVARCHAR(100) NOT NULL,
    
    -- Which module/system area
    [Module] NVARCHAR(100) NOT NULL,
    
    -- Human-readable description of the action
    [Description] NVARCHAR(500) NULL,
    
    -- Before/After values for data changes (JSON format)
    [OldValues] NVARCHAR(2000) NULL,
    [NewValues] NVARCHAR(2000) NULL,
    
    -- Client Information
    [IpAddress] NVARCHAR(50) NULL,
    [UserAgent] NVARCHAR(255) NULL,
    
    -- Timestamp
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),

    -- ==================== FINANCIAL TRANSACTION FIELDS ====================
    -- Transaction amount
    [Amount] DECIMAL(18,2) NULL,
    
    -- Balance tracking for audit
    [BalanceBefore] DECIMAL(18,2) NULL,
    [BalanceAfter] DECIMAL(18,2) NULL,
    
    -- Fee charged (if any)
    [Fee] DECIMAL(18,2) NULL,

    -- ==================== SECURITY FIELDS ====================
    -- Flag for malicious activity
    [IsMalicious] BIT NOT NULL DEFAULT 0,
    
    -- Customer account reference
    [CustomerAccountId] INT NULL,
    
    -- Account status at time of action
    [AccountStatus] NVARCHAR(50) NULL,
    
    -- Failed attempt tracking
    [FailedAttemptCount] INT NULL DEFAULT 0,
    [PinAttemptCount] INT NULL DEFAULT 0,
    
    -- Account lock status
    [IsAccountLocked] BIT NOT NULL DEFAULT 0,
    [LockReason] NVARCHAR(255) NULL,
    
    -- User role at time of action
    [UserRole] NVARCHAR(50) NULL,

    -- ==================== EMPLOYEE/ACTOR INFORMATION ====================
    -- Employee who performed the transaction
    [EmployeeId] INT NULL,
    [EmployeeName] NVARCHAR(100) NULL,
    [EmployeeRole] NVARCHAR(50) NULL,

    -- ==================== CUSTOMER INFORMATION ====================
    -- Customer whose account is affected
    [CustomerId] INT NULL,
    [CustomerName] NVARCHAR(100) NULL,

    -- ==================== TRANSACTION DETAILS ====================
    -- Type of transaction
    [TransactionType] NVARCHAR(50) NULL,  -- Deposit, Withdrawal, Transfer, LoanDisbursement, LoanPayment, SavingsDeposit, SavingsWithdrawal, InterestPosting
    
    -- Transaction status
    [TransactionStatus] NVARCHAR(50) NULL,  -- Pending, Completed, Failed, Cancelled, Reversed
    
    -- Reference numbers for tracking
    [ReferenceNumber] NVARCHAR(50) NULL,
    [TransactionNumber] NVARCHAR(50) NULL,

    -- ==================== SOURCE ACCOUNT INFORMATION ====================
    [AccountNumber] NVARCHAR(50) NULL,
    [AccountType] NVARCHAR(50) NULL,  -- Savings, Checking, Loan

    -- ==================== TARGET ACCOUNT (FOR TRANSFERS) ====================
    [TargetAccountNumber] NVARCHAR(50) NULL,
    [TargetAccountName] NVARCHAR(100) NULL,

    -- ==================== TRANSACTION METHOD ====================
    [TransactionMethod] NVARCHAR(50) NULL,  -- Cash, Check, Online, InterBank, ATM
    [TransactionChannel] NVARCHAR(100) NULL,  -- Teller Window, Online Banking, Mobile App, ATM

    -- ==================== LOAN-SPECIFIC INFORMATION ====================
    [LoanId] INT NULL,
    [LoanNumber] NVARCHAR(50) NULL,
    [LoanType] NVARCHAR(50) NULL,

    -- ==================== APPROVAL/AUTHORIZATION ====================
    [ApprovedBy] NVARCHAR(100) NULL,
    [ApprovedAt] DATETIME NULL,
    [ApprovalRemarks] NVARCHAR(255) NULL,

    -- ==================== RISK ASSESSMENT ====================
    [RiskLevel] NVARCHAR(50) NULL,  -- Low, Medium, High, Critical
    [RequiresReview] BIT NOT NULL DEFAULT 0,

    -- ==================== SESSION & LOCATION ====================
    [SessionId] NVARCHAR(100) NULL,
    [BranchCode] NVARCHAR(50) NULL,
    [BranchName] NVARCHAR(100) NULL,

    -- ==================== CONSTRAINTS ====================
    CONSTRAINT [CK_AuditLogs_TransactionType] CHECK (
        [TransactionType] IS NULL OR [TransactionType] IN (
            'Deposit', 'Withdrawal', 'Transfer', 'LoanDisbursement', 
            'LoanPayment', 'SavingsDeposit', 'SavingsWithdrawal', 
            'InterestPosting', 'FeeCharge', 'Reversal', 'Adjustment',
            'AccountOpening', 'AccountClosure', 'LoanApplication', 'LoanApproval'
        )
    ),
    CONSTRAINT [CK_AuditLogs_TransactionStatus] CHECK (
        [TransactionStatus] IS NULL OR [TransactionStatus] IN (
            'Pending', 'Processing', 'Completed', 'Failed', 
            'Cancelled', 'Reversed', 'OnHold', 'RequiresApproval'
        )
    ),
    CONSTRAINT [CK_AuditLogs_RiskLevel] CHECK (
        [RiskLevel] IS NULL OR [RiskLevel] IN ('Low', 'Medium', 'High', 'Critical')
    )
);
GO

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Index for quick lookup by user
CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] 
ON [dbo].[AuditLogs] ([UserId]) 
INCLUDE ([Action], [Module], [CreatedAt]);

-- Index for date range queries
CREATE NONCLUSTERED INDEX [IX_AuditLogs_CreatedAt] 
ON [dbo].[AuditLogs] ([CreatedAt] DESC) 
INCLUDE ([UserId], [Action], [Module]);

-- Index for transaction type filtering
CREATE NONCLUSTERED INDEX [IX_AuditLogs_TransactionType] 
ON [dbo].[AuditLogs] ([TransactionType], [CreatedAt] DESC) 
INCLUDE ([AccountNumber], [Amount], [TransactionStatus]);

-- Index for employee tracking
CREATE NONCLUSTERED INDEX [IX_AuditLogs_EmployeeId] 
ON [dbo].[AuditLogs] ([EmployeeId], [CreatedAt] DESC) 
INCLUDE ([Action], [TransactionType], [Amount]);

-- Index for customer account tracking
CREATE NONCLUSTERED INDEX [IX_AuditLogs_CustomerId] 
ON [dbo].[AuditLogs] ([CustomerId], [CreatedAt] DESC) 
INCLUDE ([TransactionType], [Amount], [AccountNumber]);

-- Index for account number lookup
CREATE NONCLUSTERED INDEX [IX_AuditLogs_AccountNumber] 
ON [dbo].[AuditLogs] ([AccountNumber], [CreatedAt] DESC) 
INCLUDE ([TransactionType], [Amount], [TransactionStatus]);

-- Index for malicious activity monitoring
CREATE NONCLUSTERED INDEX [IX_AuditLogs_IsMalicious] 
ON [dbo].[AuditLogs] ([IsMalicious], [CreatedAt] DESC) 
WHERE [IsMalicious] = 1;

-- Index for security monitoring (failed attempts)
CREATE NONCLUSTERED INDEX [IX_AuditLogs_Security] 
ON [dbo].[AuditLogs] ([Action], [CreatedAt] DESC) 
WHERE [Action] IN ('LOGIN_FAILED', 'MALICIOUS_ATTEMPT', 'SUSPICIOUS_ACTIVITY');

-- Index for risk-based queries
CREATE NONCLUSTERED INDEX [IX_AuditLogs_RiskLevel] 
ON [dbo].[AuditLogs] ([RiskLevel], [CreatedAt] DESC) 
WHERE [RiskLevel] IN ('High', 'Critical');

-- Index for review required items
CREATE NONCLUSTERED INDEX [IX_AuditLogs_RequiresReview] 
ON [dbo].[AuditLogs] ([RequiresReview], [CreatedAt] DESC) 
WHERE [RequiresReview] = 1;

GO

-- =============================================
-- MIGRATION SCRIPT: Add new columns to existing table
-- Use this if the table already exists
-- =============================================

/*
-- Add new columns (run only if table exists and needs updating)

ALTER TABLE [dbo].[AuditLogs] ADD [EmployeeId] INT NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [EmployeeName] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [EmployeeRole] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [CustomerId] INT NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [CustomerName] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TransactionType] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TransactionStatus] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [ReferenceNumber] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TransactionNumber] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [AccountNumber] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [AccountType] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TargetAccountNumber] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TargetAccountName] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TransactionMethod] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [TransactionChannel] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [LoanId] INT NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [LoanNumber] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [LoanType] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [Fee] DECIMAL(18,2) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [ApprovedBy] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [ApprovedAt] DATETIME NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [ApprovalRemarks] NVARCHAR(255) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [RiskLevel] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [RequiresReview] BIT NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[AuditLogs] ADD [SessionId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [BranchCode] NVARCHAR(50) NULL;
ALTER TABLE [dbo].[AuditLogs] ADD [BranchName] NVARCHAR(100) NULL;
GO
*/

-- =============================================
-- SAMPLE QUERIES FOR AUDIT REPORTING
-- =============================================

-- Get all transactions for a specific date range
-- SELECT * FROM AuditLogs WHERE CreatedAt BETWEEN '2024-01-01' AND '2024-01-31' ORDER BY CreatedAt DESC;

-- Get all deposits processed by a specific employee
-- SELECT * FROM AuditLogs WHERE EmployeeId = 1 AND TransactionType = 'Deposit' ORDER BY CreatedAt DESC;

-- Get high-risk transactions requiring review
-- SELECT * FROM AuditLogs WHERE RiskLevel IN ('High', 'Critical') AND RequiresReview = 1 ORDER BY CreatedAt DESC;

-- Get all transfers above a certain amount (for BSA/AML monitoring)
-- SELECT * FROM AuditLogs WHERE TransactionType = 'Transfer' AND Amount >= 10000 ORDER BY Amount DESC;

-- Get customer transaction history
-- SELECT * FROM AuditLogs WHERE CustomerId = 123 ORDER BY CreatedAt DESC;

-- Get failed/cancelled transactions
-- SELECT * FROM AuditLogs WHERE TransactionStatus IN ('Failed', 'Cancelled') ORDER BY CreatedAt DESC;

-- Daily transaction summary by type
-- SELECT TransactionType, COUNT(*) as Count, SUM(Amount) as TotalAmount 
-- FROM AuditLogs 
-- WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE) AND TransactionType IS NOT NULL
-- GROUP BY TransactionType;

-- Employee activity report
-- SELECT EmployeeId, EmployeeName, COUNT(*) as TransactionCount, SUM(Amount) as TotalAmount
-- FROM AuditLogs
-- WHERE EmployeeId IS NOT NULL AND CreatedAt >= DATEADD(day, -30, GETDATE())
-- GROUP BY EmployeeId, EmployeeName
-- ORDER BY TransactionCount DESC;

PRINT 'AuditLogs table created successfully with all banking compliance columns!';
GO
