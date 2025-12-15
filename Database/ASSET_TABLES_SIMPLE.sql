-- =============================================
-- ASSET MARKETPLACE DATABASE TABLES
-- Run this script on your BFAS database
-- =============================================

-- Drop tables if they exist (for fresh start)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetApplications]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetApplications];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetImages]') AND type in (N'U'))
    DROP TABLE [dbo].[AssetImages];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assets]') AND type in (N'U'))
    DROP TABLE [dbo].[Assets];
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
-- Table 3: AssetApplications
-- =============================================
CREATE TABLE [dbo].[AssetApplications](
    [ApplicationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ApplicationNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [AssetId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [CustomerAccountId] INT NULL,
    [PurchaseType] NVARCHAR(50) NOT NULL,
    
    -- Financial terms
    [AssetPrice] DECIMAL(18,2) NOT NULL,
    [DownPaymentAmount] DECIMAL(18,2) NOT NULL,
    [LoanAmount] DECIMAL(18,2) NOT NULL,
    [TermMonths] INT NOT NULL,
    [InterestRate] DECIMAL(5,2) NOT NULL,
    [MonthlyPayment] DECIMAL(18,2) NOT NULL,
    
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

-- Create indexes
CREATE INDEX [IX_Assets_AssetType] ON [dbo].[Assets]([AssetType]);
CREATE INDEX [IX_Assets_Status] ON [dbo].[Assets]([Status]);
CREATE INDEX [IX_AssetImages_AssetId] ON [dbo].[AssetImages]([AssetId]);
CREATE INDEX [IX_AssetApplications_CustomerId] ON [dbo].[AssetApplications]([CustomerId]);
CREATE INDEX [IX_AssetApplications_AssetId] ON [dbo].[AssetApplications]([AssetId]);
CREATE INDEX [IX_AssetApplications_Status] ON [dbo].[AssetApplications]([Status]);
GO

PRINT 'Table [AssetApplications] created successfully.';
PRINT '';
PRINT '========================================';
PRINT 'Asset Marketplace Tables Ready!';
PRINT '========================================';
GO
