-- =============================================
-- ASSET MARKETPLACE DATABASE TABLES
-- Created: December 15, 2025
-- Purpose: Support for Property, Vehicle, and Other Asset Sales/Loans
-- =============================================

-- NOTE: Run this script on your existing database
-- The script will check if tables exist before creating them
GO

-- =============================================
-- Table: Assets
-- Description: Main asset table for properties, vehicles, and other assets
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Assets](
        [AssetId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AssetType] NVARCHAR(50) NOT NULL, -- Property, Vehicle, Other
        [AssetName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(2000) NULL,
        [TotalPrice] DECIMAL(18,2) NOT NULL,
        [DownPaymentPercent] DECIMAL(5,2) NOT NULL DEFAULT 20,
        [DownPaymentAmount] DECIMAL(18,2) NULL,
        [InterestRate] DECIMAL(5,2) NOT NULL DEFAULT 12,
        [DefaultTermMonths] INT NOT NULL DEFAULT 60,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Available', -- Available, Reserved, Sold, Inactive
        
        -- PROPERTY-SPECIFIC FIELDS
        [PropertyType] NVARCHAR(100) NULL,
        [PropertyLocation] NVARCHAR(500) NULL,
        [LandAreaSqm] DECIMAL(10,2) NULL,
        [FloorAreaSqm] DECIMAL(10,2) NULL,
        [Bedrooms] INT NULL,
        [Bathrooms] INT NULL,
        [ParkingSlots] INT NULL,
        [DeveloperName] NVARCHAR(200) NULL,
        [TitleStatus] NVARCHAR(100) NULL,
        
        -- VEHICLE-SPECIFIC FIELDS
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
        
        -- OTHER ASSET FIELDS
        [OtherAssetCategory] NVARCHAR(100) NULL,
        [OtherAssetBrand] NVARCHAR(200) NULL,
        [OtherAssetModel] NVARCHAR(200) NULL,
        [OtherAssetCondition] NVARCHAR(100) NULL,
        [OtherAssetSpecifications] NVARCHAR(500) NULL,
        
        -- COMMON FIELDS
        [SellerName] NVARCHAR(200) NULL,
        [SellerContact] NVARCHAR(100) NULL,
        [SellerAddress] NVARCHAR(500) NULL,
        [CreatedBy] NVARCHAR(200) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedBy] NVARCHAR(200) NULL,
        [UpdatedAt] DATETIME NULL,
        
        CONSTRAINT [CK_Assets_AssetType] CHECK ([AssetType] IN ('Property', 'Vehicle', 'Other')),
        CONSTRAINT [CK_Assets_Status] CHECK ([Status] IN ('Available', 'Reserved', 'Sold', 'Inactive'))
    );

    CREATE INDEX [IX_Assets_AssetType] ON [dbo].[Assets]([AssetType]);
    CREATE INDEX [IX_Assets_Status] ON [dbo].[Assets]([Status]);
    CREATE INDEX [IX_Assets_CreatedAt] ON [dbo].[Assets]([CreatedAt]);

    PRINT 'Table [Assets] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Assets] already exists.';
END
GO

-- =============================================
-- Table: AssetImages
-- Description: Stores multiple images for each asset
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetImages]') AND type in (N'U'))
BEGIN
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

    CREATE INDEX [IX_AssetImages_AssetId] ON [dbo].[AssetImages]([AssetId]);
    CREATE INDEX [IX_AssetImages_IsPrimary] ON [dbo].[AssetImages]([IsPrimary]);

    PRINT 'Table [AssetImages] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [AssetImages] already exists.';
END
GO

-- =============================================
-- Table: AssetApplications
-- Description: Customer applications to purchase/loan assets
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetApplications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetApplications](
        [ApplicationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ApplicationNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [AssetId] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [CustomerAccountId] INT NULL,
        [PurchaseType] NVARCHAR(50) NOT NULL, -- Cash, Loan
        
        -- FINANCIAL TERMS
        [AssetPrice] DECIMAL(18,2) NOT NULL,
        [DownPaymentAmount] DECIMAL(18,2) NOT NULL,
        [LoanAmount] DECIMAL(18,2) NOT NULL,
        [TermMonths] INT NOT NULL,
        [InterestRate] DECIMAL(5,2) NOT NULL,
        [MonthlyPayment] DECIMAL(18,2) NOT NULL,
        
        -- STATUS
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'SUBMITTED',
        -- SUBMITTED, PENDING_TELLER_REVIEW, VERIFIED, PENDING_ACCOUNTANT_REVIEW,
        -- ASSESSED, PENDING_FINANCEMANAGER_APPROVAL, APPROVED, REJECTED, RELEASED, CANCELLED
        
        -- CUSTOMER INFORMATION
        [CustomerName] NVARCHAR(200) NULL,
        [CustomerAddress] NVARCHAR(500) NULL,
        [CustomerEmail] NVARCHAR(100) NULL,
        [CustomerPhone] NVARCHAR(50) NULL,
        [EmploymentStatus] NVARCHAR(100) NULL,
        [MonthlyIncome] DECIMAL(18,2) NULL,
        
        -- WORKFLOW TRACKING
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
        
        -- CONTRACT INFORMATION
        [ContractNumber] NVARCHAR(100) NULL,
        [ContractDate] DATETIME NULL,
        [DeedOfSaleNumber] NVARCHAR(500) NULL,
        [DeedOfSaleDate] DATETIME NULL,
        [ORNumber] NVARCHAR(100) NULL, -- Official Receipt (for vehicles)
        [CRNumber] NVARCHAR(100) NULL, -- Certificate of Registration (for vehicles)
        
        -- LINKED LOAN
        [LinkedLoanId] INT NULL
        
        -- Note: Foreign key to CustomerLoans is optional - add manually if needed:
        -- ALTER TABLE [dbo].[AssetApplications] ADD CONSTRAINT [FK_AssetApplications_Loans] 
        --     FOREIGN KEY ([LinkedLoanId]) REFERENCES [dbo].[CustomerLoans]([LoanId])
    );

    -- Add FK to Assets (required)
    ALTER TABLE [dbo].[AssetApplications] ADD CONSTRAINT [FK_AssetApplications_Assets] 
        FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Assets]([AssetId]);
    
    -- Add check constraint for PurchaseType
    ALTER TABLE [dbo].[AssetApplications] ADD CONSTRAINT [CK_AssetApplications_PurchaseType] 
        CHECK ([PurchaseType] IN ('Cash', 'Loan'));

    CREATE INDEX [IX_AssetApplications_CustomerId] ON [dbo].[AssetApplications]([CustomerId]);
    CREATE INDEX [IX_AssetApplications_AssetId] ON [dbo].[AssetApplications]([AssetId]);
    CREATE INDEX [IX_AssetApplications_Status] ON [dbo].[AssetApplications]([Status]);
    CREATE INDEX [IX_AssetApplications_SubmittedAt] ON [dbo].[AssetApplications]([SubmittedAt]);
    CREATE INDEX [IX_AssetApplications_ApplicationNumber] ON [dbo].[AssetApplications]([ApplicationNumber]);

    PRINT 'Table [AssetApplications] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [AssetApplications] already exists.';
END
GO

-- =============================================
-- Sample Data: Asset Types
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Asset Marketplace Tables Created Successfully!';
PRINT '========================================';
PRINT 'Tables created:';
PRINT '  - Assets (Properties, Vehicles, Other Assets)';
PRINT '  - AssetImages (Multiple images per asset)';
PRINT '  - AssetApplications (Customer purchase/loan applications)';
PRINT '';
PRINT 'You can now:';
PRINT '  1. Add assets via Admin -> Asset Management';
PRINT '  2. Upload multiple images per asset';
PRINT '  3. Customers can browse and apply for assets';
PRINT '  4. Process follows same workflow as regular loans';
PRINT '========================================';
GO
