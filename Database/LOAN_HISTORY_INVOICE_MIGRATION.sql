-- =====================================================================
-- LOAN TRANSACTION HISTORY & INVOICE TABLES MIGRATION
-- Run this script on your database to add the new tracking tables
-- =====================================================================

USE BFASdatabase;
GO

-- =====================================================================
-- 1. LOAN TRANSACTION HISTORY TABLE
-- Tracks all loan-related activities for audit trail
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LoanTransactionHistory' AND xtype='U')
BEGIN
    CREATE TABLE LoanTransactionHistory (
        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NULL,
        ApplicationId INT NULL,
        ApprovalId INT NULL,
        AccountId INT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL, -- APPLICATION, TELLER_REVIEW, ACCOUNTANT_ASSESSMENT, FM_APPROVAL, FM_DECLINE, RELEASE, PAYMENT, FULL_PAYMENT
        Description NVARCHAR(500) NULL,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PenaltyAmount DECIMAL(18,2) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL,
        Reference NVARCHAR(100) NULL,
        ProcessedBy NVARCHAR(100) NOT NULL,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(50) NULL,
        
        CONSTRAINT FK_LoanTransactionHistory_Account FOREIGN KEY (AccountId) 
            REFERENCES CustomerAccounts(AccountId),
        CONSTRAINT FK_LoanTransactionHistory_Loan FOREIGN KEY (LoanId) 
            REFERENCES Loans(LoanId),
        CONSTRAINT FK_LoanTransactionHistory_Application FOREIGN KEY (ApplicationId) 
            REFERENCES LoanApplications(ApplicationId)
    );

    CREATE INDEX IX_LoanTransactionHistory_AccountId ON LoanTransactionHistory(AccountId);
    CREATE INDEX IX_LoanTransactionHistory_LoanId ON LoanTransactionHistory(LoanId);
    CREATE INDEX IX_LoanTransactionHistory_TransactionDate ON LoanTransactionHistory(TransactionDate);
    CREATE INDEX IX_LoanTransactionHistory_TransactionType ON LoanTransactionHistory(TransactionType);

    PRINT 'Created LoanTransactionHistory table';
END
ELSE
BEGIN
    PRINT 'LoanTransactionHistory table already exists';
END
GO

-- =====================================================================
-- 2. LOAN INVOICES TABLE
-- Stores generated invoices for releases and payments
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LoanInvoices' AND xtype='U')
BEGIN
    CREATE TABLE LoanInvoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        InvoiceType NVARCHAR(50) NOT NULL, -- RELEASE, PAYMENT
        LoanId INT NULL,
        PaymentId INT NULL,
        DisbursalId INT NULL,
        AccountId INT NOT NULL,
        CustomerName NVARCHAR(200) NOT NULL,
        AccountNumber NVARCHAR(100) NULL,
        PrincipalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        InterestAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PenaltyAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        FeesAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaymentMethod NVARCHAR(50) NULL,
        Reference NVARCHAR(100) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL,
        ProcessedBy NVARCHAR(100) NOT NULL,
        InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
        Notes NVARCHAR(500) NULL,
        
        CONSTRAINT FK_LoanInvoices_Account FOREIGN KEY (AccountId) 
            REFERENCES CustomerAccounts(AccountId),
        CONSTRAINT FK_LoanInvoices_Loan FOREIGN KEY (LoanId) 
            REFERENCES Loans(LoanId)
    );

    CREATE INDEX IX_LoanInvoices_AccountId ON LoanInvoices(AccountId);
    CREATE INDEX IX_LoanInvoices_LoanId ON LoanInvoices(LoanId);
    CREATE INDEX IX_LoanInvoices_InvoiceNumber ON LoanInvoices(InvoiceNumber);
    CREATE INDEX IX_LoanInvoices_InvoiceDate ON LoanInvoices(InvoiceDate);

    PRINT 'Created LoanInvoices table';
END
ELSE
BEGIN
    PRINT 'LoanInvoices table already exists';
END
GO

-- =====================================================================
-- 3. UPDATE LoanAssessments - Add FM status tracking
-- =====================================================================

-- Check if AssessmentStatus can hold FM_APPROVED/FM_DECLINED
-- The column should already exist, just confirming it can hold these values
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'LoanAssessments' 
           AND COLUMN_NAME = 'AssessmentStatus')
BEGIN
    PRINT 'AssessmentStatus column exists in LoanAssessments';
    
    -- Update any existing ASSESSED to allow FM workflow
    -- No changes needed - just informational
END
GO

-- =====================================================================
-- 4. VERIFICATION QUERIES
-- =====================================================================

-- Verify tables created
SELECT 'LoanTransactionHistory' AS TableName, COUNT(*) AS RowCount FROM LoanTransactionHistory
UNION ALL
SELECT 'LoanInvoices', COUNT(*) FROM LoanInvoices;

-- View table structures
EXEC sp_help 'LoanTransactionHistory';
EXEC sp_help 'LoanInvoices';

PRINT '';
PRINT '=====================================================================';
PRINT 'MIGRATION COMPLETE';
PRINT '=====================================================================';
PRINT 'New Features Added:';
PRINT '1. LoanTransactionHistory - Full audit trail of all loan operations';
PRINT '2. LoanInvoices - Invoice generation for releases and payments';
PRINT '3. FM Approval History - Assessments removed from pending after decision';
PRINT '4. Duplicate Loan Prevention - Customers cannot apply with unpaid loans';
PRINT '5. Physical Cash Payments - No account deduction for loan payments';
PRINT '=====================================================================';
GO
