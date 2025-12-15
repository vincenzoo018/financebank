-- =============================================
-- ASSET HISTORY TABLE - For tracking workflow events
-- Run this script on your BFASdatabase
-- =============================================

USE [BFASdatabase];
GO

-- Drop table if exists (for fresh start)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetHistory]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetHistory];
GO

PRINT 'Creating AssetHistory table...';
GO

-- =============================================
-- Table: AssetHistory
-- Tracks all workflow events for asset applications
-- =============================================
CREATE TABLE [dbo].[AssetHistory](
    [HistoryId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ApplicationId] INT NOT NULL,
    
    -- Action details
    [ActionType] NVARCHAR(50) NOT NULL,
    -- SUBMITTED, VERIFIED, FORWARDED_TO_ACCOUNTANT, ASSESSED, 
    -- FORWARDED_TO_FINANCEMANAGER, APPROVED, REJECTED, RELEASED, CANCELLED
    
    [ActionBy] NVARCHAR(200) NULL,           -- Username who performed the action
    [ActionByRole] NVARCHAR(100) NULL,       -- Role of user (Teller, Accountant, FinanceManager)
    [ActionDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Remarks] NVARCHAR(500) NULL,
    
    -- Status tracking
    [PreviousStatus] NVARCHAR(50) NULL,
    [NewStatus] NVARCHAR(50) NULL,
    
    -- Contract Information (captured at release)
    [ContractNumber] NVARCHAR(100) NULL,
    [ContractDate] DATETIME NULL,
    [DeedOfSaleNumber] NVARCHAR(500) NULL,
    [DeedOfSaleDate] DATETIME NULL,
    [ORNumber] NVARCHAR(100) NULL,
    [CRNumber] NVARCHAR(100) NULL,
    
    -- Financial Snapshot (captured at time of action)
    [AssetPrice] DECIMAL(18,2) NULL,
    [DownPaymentAmount] DECIMAL(18,2) NULL,
    [LoanAmount] DECIMAL(18,2) NULL,
    [TermMonths] INT NULL,
    [InterestRate] DECIMAL(5,2) NULL,
    [MonthlyPayment] DECIMAL(18,2) NULL,
    
    -- Assessment Snapshot (for accountant actions)
    [DebtToIncomeRatio] DECIMAL(5,2) NULL,
    [CreditRiskLevel] NVARCHAR(50) NULL,
    [Recommendation] NVARCHAR(50) NULL,
    
    -- Linked loan info (for release)
    [LinkedLoanId] INT NULL,
    
    CONSTRAINT [FK_AssetHistory_AssetApplications] FOREIGN KEY ([ApplicationId]) 
        REFERENCES [dbo].[AssetApplications]([ApplicationId]) ON DELETE CASCADE
);
GO

-- Create indexes for better query performance
CREATE INDEX [IX_AssetHistory_ApplicationId] ON [dbo].[AssetHistory]([ApplicationId]);
CREATE INDEX [IX_AssetHistory_ActionType] ON [dbo].[AssetHistory]([ActionType]);
CREATE INDEX [IX_AssetHistory_ActionDate] ON [dbo].[AssetHistory]([ActionDate]);
CREATE INDEX [IX_AssetHistory_ActionBy] ON [dbo].[AssetHistory]([ActionBy]);
GO

PRINT '';
PRINT '========================================';
PRINT 'AssetHistory Table Created Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'ActionTypes that will be logged:';
PRINT '  - SUBMITTED: Customer submits application';
PRINT '  - FORWARDED_TO_ACCOUNTANT: Teller verifies and forwards';
PRINT '  - FORWARDED_TO_FINANCEMANAGER: Accountant assesses and forwards';
PRINT '  - APPROVED: Finance Manager approves';
PRINT '  - REJECTED: Any role rejects';
PRINT '  - RELEASED: Teller releases with contract';
PRINT '  - CANCELLED: Application cancelled';
PRINT '';
GO

-- Verify table was created
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name = 'AssetHistory'
ORDER BY c.column_id;
GO
