-- =============================================
-- ASSET MARKETPLACE DATABASE TABLES - COMPLETE VERSION
-- Run this script on your BFASdatabase
-- This includes all fields for full workflow support
-- =============================================

USE [BFASdatabase];
GO

-- Drop tables in correct order (child tables first due to foreign key constraints)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetHistory]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetHistory];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetApplications]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetApplications];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetImages]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetImages];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assets]') AND type in (N'U'))
    DROP TABLE [dbo].[Assets];
GO

PRINT 'Existing tables dropped (if any).';
GO

-- =============================================
-- Table 1: Assets
-- =============================================
CREATE TABLE [dbo].[Assets](
    [AssetId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AssetType] NVARCHAR(50) NOT NULL,
    [AssetName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [TotalPrice] DECIMAL(18,2) NOT NULL,
    [DownPaymentPercent] DECIMAL(5,2) NOT NULL DEFAULT 20,
    [DownPaymentAmount] DECIMAL(18,2) NULL,
    [InterestRate] DECIMAL(5,2) NOT NULL DEFAULT 12,
    [DefaultTermMonths] INT NOT NULL DEFAULT 60,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Available',
    
    -- Property fields
    [PropertyType] NVARCHAR(100) NULL,
    [PropertyLocation] NVARCHAR(500) NULL,
    [LandAreaSqm] DECIMAL(10,2) NULL,
    [FloorAreaSqm] DECIMAL(10,2) NULL,
    [Bedrooms] INT NULL,
    [Bathrooms] INT NULL,
    [ParkingSlots] INT NULL,
    [DeveloperName] NVARCHAR(200) NULL,
    [TitleStatus] NVARCHAR(100) NULL,
    
    -- Vehicle fields
    [VehicleBrand] NVARCHAR(100) NULL,
    [VehicleModel] NVARCHAR(100) NULL,
    [VehicleYear] INT NULL,
    [VehicleCondition] NVARCHAR(50) NULL,
    [Mileage] INT NULL,
    [Transmission] NVARCHAR(50) NULL,
    [FuelType] NVARCHAR(50) NULL,
    [Color] NVARCHAR(50) NULL,
    [EngineNumber] NVARCHAR(100) NULL,
    [ChassisNumber] NVARCHAR(100) NULL,
    [PlateNumber] NVARCHAR(50) NULL,
    
    -- Other asset fields
    [OtherAssetCategory] NVARCHAR(100) NULL,
    [OtherAssetBrand] NVARCHAR(200) NULL,
    [OtherAssetModel] NVARCHAR(200) NULL,
    [OtherAssetCondition] NVARCHAR(100) NULL,
    [OtherAssetSpecifications] NVARCHAR(500) NULL,
    
    -- Common fields
    [SellerName] NVARCHAR(200) NULL,
    [SellerContact] NVARCHAR(100) NULL,
    [SellerAddress] NVARCHAR(500) NULL,
    [CreatedBy] NVARCHAR(200) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedBy] NVARCHAR(200) NULL,
    [UpdatedAt] DATETIME NULL
);
GO

PRINT 'Table [Assets] created successfully.';
GO

-- =============================================
-- Table 2: AssetImages
-- =============================================
CREATE TABLE [dbo].[AssetImages](
    [ImageId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AssetId] INT NOT NULL,
    [FileName] NVARCHAR(200) NULL,
    [ContentType] NVARCHAR(100) NULL,
    [ImageData] VARBINARY(MAX) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [UploadedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [FK_AssetImages_Assets] FOREIGN KEY ([AssetId]) 
        REFERENCES [dbo].[Assets]([AssetId]) ON DELETE CASCADE
);
GO

PRINT 'Table [AssetImages] created successfully.';
GO

-- =============================================
-- Table 3: AssetApplications (Complete with all workflow fields)
-- =============================================
CREATE TABLE [dbo].[AssetApplications](
    [ApplicationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ApplicationNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [AssetId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [CustomerAccountId] INT NULL,
    [PurchaseType] NVARCHAR(50) NOT NULL DEFAULT 'Loan',
    
    -- Financial terms
    [AssetPrice] DECIMAL(18,2) NOT NULL,
    [DownPaymentAmount] DECIMAL(18,2) NOT NULL,
    [LoanAmount] DECIMAL(18,2) NOT NULL,
    [TermMonths] INT NOT NULL,
    [InterestRate] DECIMAL(5,2) NOT NULL,
    [MonthlyPayment] DECIMAL(18,2) NOT NULL,
    
    -- ========== REQUIREMENT CHECKBOXES (Customer must check these) ==========
    [AgreedTermsAndConditions] BIT NOT NULL DEFAULT 0,
    [AgreedPaymentTerms] BIT NOT NULL DEFAULT 0,
    [AgreedPenaltyTerms] BIT NOT NULL DEFAULT 0,
    [AgreedLegalAction] BIT NOT NULL DEFAULT 0,
    [CertifiedInfoAccuracy] BIT NOT NULL DEFAULT 0,
    
    -- ========== DOCUMENT SUBMISSION TRACKING ==========
    [ValidIdRequired] BIT NOT NULL DEFAULT 0,
    [ValidIdSubmitted] BIT NOT NULL DEFAULT 0,
    
    [ProofOfIncomeRequired] BIT NOT NULL DEFAULT 0,
    [ProofOfIncomeSubmitted] BIT NOT NULL DEFAULT 0,
    
    [ProofOfBillingRequired] BIT NOT NULL DEFAULT 0,
    [ProofOfBillingSubmitted] BIT NOT NULL DEFAULT 0,
    
    [BankStatementRequired] BIT NOT NULL DEFAULT 0,
    [BankStatementSubmitted] BIT NOT NULL DEFAULT 0,
    
    [COERequired] BIT NOT NULL DEFAULT 0,
    [COESubmitted] BIT NOT NULL DEFAULT 0,
    
    [BusinessPermitRequired] BIT NOT NULL DEFAULT 0,
    [BusinessPermitSubmitted] BIT NOT NULL DEFAULT 0,
    
    [CollateralDocsRequired] BIT NOT NULL DEFAULT 0,
    [CollateralDocsSubmitted] BIT NOT NULL DEFAULT 0,
    
    [TitleDeedRequired] BIT NOT NULL DEFAULT 0,
    [TitleDeedSubmitted] BIT NOT NULL DEFAULT 0,
    
    [TaxDeclarationRequired] BIT NOT NULL DEFAULT 0,
    [TaxDeclarationSubmitted] BIT NOT NULL DEFAULT 0,
    
    [ORCRRequired] BIT NOT NULL DEFAULT 0,
    [ORCRSubmitted] BIT NOT NULL DEFAULT 0,
    
    -- Uploaded documents JSON
    [UploadedDocuments] NVARCHAR(4000) NULL,
    
    -- ========== TELLER VERIFICATION CHECKLIST ==========
    [TellerVerifiedId] BIT NOT NULL DEFAULT 0,
    [TellerVerifiedIncome] BIT NOT NULL DEFAULT 0,
    [TellerVerifiedEmployment] BIT NOT NULL DEFAULT 0,
    [TellerVerifiedDocuments] BIT NOT NULL DEFAULT 0,
    [TellerVerifiedAddress] BIT NOT NULL DEFAULT 0,
    
    -- Status
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'SUBMITTED',
    
    -- Customer information
    [CustomerName] NVARCHAR(200) NULL,
    [CustomerAddress] NVARCHAR(500) NULL,
    [CustomerEmail] NVARCHAR(100) NULL,
    [CustomerPhone] NVARCHAR(50) NULL,
    [EmploymentStatus] NVARCHAR(100) NULL,
    [MonthlyIncome] DECIMAL(18,2) NULL,
    
    -- Workflow tracking
    [SubmittedBy] NVARCHAR(200) NULL,
    [SubmittedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    
    [TellerReviewedBy] NVARCHAR(200) NULL,
    [TellerReviewedAt] DATETIME NULL,
    [TellerRemarks] NVARCHAR(500) NULL,
    
    [AccountantAssessedBy] NVARCHAR(200) NULL,
    [AccountantAssessedAt] DATETIME NULL,
    [AccountantRemarks] NVARCHAR(500) NULL,
    
    -- ========== ACCOUNTANT ASSESSMENT FIELDS ==========
    [AssessedMonthlyPayment] DECIMAL(18,2) NULL,
    [AssessedInterestRate] DECIMAL(5,2) NULL,
    [AssessedTermMonths] INT NULL,
    [DebtToIncomeRatio] DECIMAL(5,2) NULL,
    [CreditRiskLevel] NVARCHAR(50) NULL,
    [AccountantRecommendation] NVARCHAR(50) NULL,
    
    [FinanceManagerApprovedBy] NVARCHAR(200) NULL,
    [FinanceManagerApprovedAt] DATETIME NULL,
    [FinanceManagerRemarks] NVARCHAR(500) NULL,
    
    [ReleasedBy] NVARCHAR(200) NULL,
    [ReleasedAt] DATETIME NULL,
    [ReleaseRemarks] NVARCHAR(500) NULL,
    
    -- Contract information
    [ContractNumber] NVARCHAR(100) NULL,
    [ContractDate] DATETIME NULL,
    [DeedOfSaleNumber] NVARCHAR(500) NULL,
    [DeedOfSaleDate] DATETIME NULL,
    [ORNumber] NVARCHAR(100) NULL,
    [CRNumber] NVARCHAR(100) NULL,
    
    -- Linked loan
    [LinkedLoanId] INT NULL,
    
    CONSTRAINT [FK_AssetApplications_Assets] FOREIGN KEY ([AssetId]) 
        REFERENCES [dbo].[Assets]([AssetId])
);
GO

-- Create indexes for better query performance
CREATE INDEX [IX_Assets_AssetType] ON [dbo].[Assets]([AssetType]);
CREATE INDEX [IX_Assets_Status] ON [dbo].[Assets]([Status]);
CREATE INDEX [IX_AssetImages_AssetId] ON [dbo].[AssetImages]([AssetId]);
CREATE INDEX [IX_AssetApplications_CustomerId] ON [dbo].[AssetApplications]([CustomerId]);
CREATE INDEX [IX_AssetApplications_AssetId] ON [dbo].[AssetApplications]([AssetId]);
CREATE INDEX [IX_AssetApplications_Status] ON [dbo].[AssetApplications]([Status]);
GO

PRINT 'Table [AssetApplications] created successfully.';
GO

-- =============================================
-- Table 4: AssetHistory (Audit Trail)
-- Tracks all workflow events for asset applications
-- =============================================
CREATE TABLE [dbo].[AssetHistory](
    [HistoryId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ApplicationId] INT NOT NULL,
    
    -- Action details
    [ActionType] NVARCHAR(50) NOT NULL,
    [ActionBy] NVARCHAR(200) NULL,
    [ActionByRole] NVARCHAR(100) NULL,
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
    
    -- Financial Snapshot
    [AssetPrice] DECIMAL(18,2) NULL,
    [DownPaymentAmount] DECIMAL(18,2) NULL,
    [LoanAmount] DECIMAL(18,2) NULL,
    [TermMonths] INT NULL,
    [InterestRate] DECIMAL(5,2) NULL,
    [MonthlyPayment] DECIMAL(18,2) NULL,
    
    -- Assessment Snapshot
    [DebtToIncomeRatio] DECIMAL(5,2) NULL,
    [CreditRiskLevel] NVARCHAR(50) NULL,
    [Recommendation] NVARCHAR(50) NULL,
    
    -- Linked loan info
    [LinkedLoanId] INT NULL,
    
    CONSTRAINT [FK_AssetHistory_AssetApplications] FOREIGN KEY ([ApplicationId]) 
        REFERENCES [dbo].[AssetApplications]([ApplicationId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AssetHistory_ApplicationId] ON [dbo].[AssetHistory]([ApplicationId]);
CREATE INDEX [IX_AssetHistory_ActionType] ON [dbo].[AssetHistory]([ActionType]);
CREATE INDEX [IX_AssetHistory_ActionDate] ON [dbo].[AssetHistory]([ActionDate]);
GO

PRINT 'Table [AssetHistory] created successfully.';
GO

PRINT '';
PRINT '========================================';
PRINT 'Asset Marketplace Tables Ready!';
PRINT 'Tables created: Assets, AssetImages, AssetApplications, AssetHistory';
PRINT '========================================';
GO

-- Verify tables were created
SELECT 
    t.name AS TableName,
    SUM(p.rows) AS [RowCount]
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE t.name IN ('Assets', 'AssetImages', 'AssetApplications', 'AssetHistory')
  AND p.index_id IN (0, 1)
GROUP BY t.name;
GO
