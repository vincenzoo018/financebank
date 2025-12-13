-- =====================================================================
-- ADD EMPLOYMENT AND INCOME COLUMNS TO USERS TABLE
-- Run this script on your local database to enable employment/income storage
-- =====================================================================

USE bfasdatabase;
GO

PRINT 'Adding Employment and Income columns to Users table...';

-- Employment Information
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentStatus')
BEGIN
    ALTER TABLE Users ADD EmploymentStatus NVARCHAR(50) NULL;
    PRINT 'Added EmploymentStatus column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerName')
BEGIN
    ALTER TABLE Users ADD EmployerName NVARCHAR(200) NULL;
    PRINT 'Added EmployerName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerAddress')
BEGIN
    ALTER TABLE Users ADD EmployerAddress NVARCHAR(500) NULL;
    PRINT 'Added EmployerAddress column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerContactNumber')
BEGIN
    ALTER TABLE Users ADD EmployerContactNumber NVARCHAR(20) NULL;
    PRINT 'Added EmployerContactNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'JobTitle')
BEGIN
    ALTER TABLE Users ADD JobTitle NVARCHAR(100) NULL;
    PRINT 'Added JobTitle column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Department')
BEGIN
    ALTER TABLE Users ADD Department NVARCHAR(100) NULL;
    PRINT 'Added Department column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'YearsAtCurrentJob')
BEGIN
    ALTER TABLE Users ADD YearsAtCurrentJob INT NULL;
    PRINT 'Added YearsAtCurrentJob column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MonthsAtCurrentJob')
BEGIN
    ALTER TABLE Users ADD MonthsAtCurrentJob INT NULL;
    PRINT 'Added MonthsAtCurrentJob column';
END

-- Income Information
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MonthlyGrossIncome')
BEGIN
    ALTER TABLE Users ADD MonthlyGrossIncome DECIMAL(18,2) NULL;
    PRINT 'Added MonthlyGrossIncome column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AnnualIncome')
BEGIN
    ALTER TABLE Users ADD AnnualIncome DECIMAL(18,2) NULL;
    PRINT 'Added AnnualIncome column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'OtherIncomeSource')
BEGIN
    ALTER TABLE Users ADD OtherIncomeSource NVARCHAR(200) NULL;
    PRINT 'Added OtherIncomeSource column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'OtherIncomeAmount')
BEGIN
    ALTER TABLE Users ADD OtherIncomeAmount DECIMAL(18,2) NULL;
    PRINT 'Added OtherIncomeAmount column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TotalMonthlyIncome')
BEGIN
    ALTER TABLE Users ADD TotalMonthlyIncome DECIMAL(18,2) NULL;
    PRINT 'Added TotalMonthlyIncome column';
END

-- Government IDs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TaxIdentificationNumber')
BEGIN
    ALTER TABLE Users ADD TaxIdentificationNumber NVARCHAR(20) NULL;
    PRINT 'Added TaxIdentificationNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SSSNumber')
BEGIN
    ALTER TABLE Users ADD SSSNumber NVARCHAR(20) NULL;
    PRINT 'Added SSSNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PagIbigNumber')
BEGIN
    ALTER TABLE Users ADD PagIbigNumber NVARCHAR(20) NULL;
    PRINT 'Added PagIbigNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PhilHealthNumber')
BEGIN
    ALTER TABLE Users ADD PhilHealthNumber NVARCHAR(20) NULL;
    PRINT 'Added PhilHealthNumber column';
END

-- Personal Details
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DateOfBirth')
BEGIN
    ALTER TABLE Users ADD DateOfBirth DATETIME NULL;
    PRINT 'Added DateOfBirth column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Gender')
BEGIN
    ALTER TABLE Users ADD Gender NVARCHAR(20) NULL;
    PRINT 'Added Gender column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CivilStatus')
BEGIN
    ALTER TABLE Users ADD CivilStatus NVARCHAR(20) NULL;
    PRINT 'Added CivilStatus column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Nationality')
BEGIN
    ALTER TABLE Users ADD Nationality NVARCHAR(50) NULL;
    PRINT 'Added Nationality column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PermanentAddress')
BEGIN
    ALTER TABLE Users ADD PermanentAddress NVARCHAR(500) NULL;
    PRINT 'Added PermanentAddress column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PresentAddress')
BEGIN
    ALTER TABLE Users ADD PresentAddress NVARCHAR(500) NULL;
    PRINT 'Added PresentAddress column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'YearsAtCurrentAddress')
BEGIN
    ALTER TABLE Users ADD YearsAtCurrentAddress INT NULL;
    PRINT 'Added YearsAtCurrentAddress column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'HomeOwnership')
BEGIN
    ALTER TABLE Users ADD HomeOwnership NVARCHAR(50) NULL;
    PRINT 'Added HomeOwnership column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'NumberOfDependents')
BEGIN
    ALTER TABLE Users ADD NumberOfDependents INT NULL;
    PRINT 'Added NumberOfDependents column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EducationalAttainment')
BEGIN
    ALTER TABLE Users ADD EducationalAttainment NVARCHAR(100) NULL;
    PRINT 'Added EducationalAttainment column';
END

-- Spouse Information
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseName')
BEGIN
    ALTER TABLE Users ADD SpouseName NVARCHAR(100) NULL;
    PRINT 'Added SpouseName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseEmployer')
BEGIN
    ALTER TABLE Users ADD SpouseEmployer NVARCHAR(200) NULL;
    PRINT 'Added SpouseEmployer column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseIncome')
BEGIN
    ALTER TABLE Users ADD SpouseIncome DECIMAL(18,2) NULL;
    PRINT 'Added SpouseIncome column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MotherMaidenName')
BEGIN
    ALTER TABLE Users ADD MotherMaidenName NVARCHAR(100) NULL;
    PRINT 'Added MotherMaidenName column';
END

-- Verification Flags
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerified')
BEGIN
    ALTER TABLE Users ADD EmploymentVerified BIT NOT NULL DEFAULT 0;
    PRINT 'Added EmploymentVerified column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerifiedBy')
BEGIN
    ALTER TABLE Users ADD EmploymentVerifiedBy NVARCHAR(100) NULL;
    PRINT 'Added EmploymentVerifiedBy column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerifiedAt')
BEGIN
    ALTER TABLE Users ADD EmploymentVerifiedAt DATETIME NULL;
    PRINT 'Added EmploymentVerifiedAt column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerified')
BEGIN
    ALTER TABLE Users ADD IncomeVerified BIT NOT NULL DEFAULT 0;
    PRINT 'Added IncomeVerified column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerifiedBy')
BEGIN
    ALTER TABLE Users ADD IncomeVerifiedBy NVARCHAR(100) NULL;
    PRINT 'Added IncomeVerifiedBy column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerifiedAt')
BEGIN
    ALTER TABLE Users ADD IncomeVerifiedAt DATETIME NULL;
    PRINT 'Added IncomeVerifiedAt column';
END

-- Risk Rating
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CustomerRiskRating')
BEGIN
    ALTER TABLE Users ADD CustomerRiskRating NVARCHAR(20) NULL;
    PRINT 'Added CustomerRiskRating column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'InternalCreditScore')
BEGIN
    ALTER TABLE Users ADD InternalCreditScore INT NULL;
    PRINT 'Added InternalCreditScore column';
END

-- Add index for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_EmploymentStatus')
BEGIN
    CREATE INDEX IX_Users_EmploymentStatus ON Users(EmploymentStatus);
    PRINT 'Added index IX_Users_EmploymentStatus';
END

PRINT '';
PRINT '=====================================================================';
PRINT 'Users table Employment/Income columns added successfully!';
PRINT 'You may now register customers with employment information.';
PRINT '=====================================================================';
GO
