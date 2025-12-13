-- =====================================================================
-- LOAN PROCESS ENHANCEMENT - COMPREHENSIVE DATABASE SCHEMA UPDATE
-- Philippine Banking Industry Standard Implementation
-- Created: December 13, 2025
-- =====================================================================

-- =====================================================================
-- PART 1: ENHANCE USERS TABLE (Employment Validation for Account Opening)
-- =====================================================================

-- Add Employment and Income Verification Fields to Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentStatus')
BEGIN
    ALTER TABLE Users ADD EmploymentStatus NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerName')
BEGIN
    ALTER TABLE Users ADD EmployerName NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerAddress')
BEGIN
    ALTER TABLE Users ADD EmployerAddress NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmployerContactNumber')
BEGIN
    ALTER TABLE Users ADD EmployerContactNumber NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'JobTitle')
BEGIN
    ALTER TABLE Users ADD JobTitle NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Department')
BEGIN
    ALTER TABLE Users ADD Department NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'YearsAtCurrentJob')
BEGIN
    ALTER TABLE Users ADD YearsAtCurrentJob INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MonthsAtCurrentJob')
BEGIN
    ALTER TABLE Users ADD MonthsAtCurrentJob INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MonthlyGrossIncome')
BEGIN
    ALTER TABLE Users ADD MonthlyGrossIncome DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AnnualIncome')
BEGIN
    ALTER TABLE Users ADD AnnualIncome DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'OtherIncomeSource')
BEGIN
    ALTER TABLE Users ADD OtherIncomeSource NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'OtherIncomeAmount')
BEGIN
    ALTER TABLE Users ADD OtherIncomeAmount DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TaxIdentificationNumber')
BEGIN
    ALTER TABLE Users ADD TaxIdentificationNumber NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SSSNumber')
BEGIN
    ALTER TABLE Users ADD SSSNumber NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PagIbigNumber')
BEGIN
    ALTER TABLE Users ADD PagIbigNumber NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PhilHealthNumber')
BEGIN
    ALTER TABLE Users ADD PhilHealthNumber NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DateOfBirth')
BEGIN
    ALTER TABLE Users ADD DateOfBirth DATE NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Gender')
BEGIN
    ALTER TABLE Users ADD Gender NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CivilStatus')
BEGIN
    ALTER TABLE Users ADD CivilStatus NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Nationality')
BEGIN
    ALTER TABLE Users ADD Nationality NVARCHAR(50) NULL DEFAULT 'Filipino';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PermanentAddress')
BEGIN
    ALTER TABLE Users ADD PermanentAddress NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PresentAddress')
BEGIN
    ALTER TABLE Users ADD PresentAddress NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'YearsAtCurrentAddress')
BEGIN
    ALTER TABLE Users ADD YearsAtCurrentAddress INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'HomeOwnership')
BEGIN
    ALTER TABLE Users ADD HomeOwnership NVARCHAR(50) NULL; -- Owned, Rented, Living with Parents, etc.
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'NumberOfDependents')
BEGIN
    ALTER TABLE Users ADD NumberOfDependents INT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EducationalAttainment')
BEGIN
    ALTER TABLE Users ADD EducationalAttainment NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseName')
BEGIN
    ALTER TABLE Users ADD SpouseName NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseEmployer')
BEGIN
    ALTER TABLE Users ADD SpouseEmployer NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SpouseIncome')
BEGIN
    ALTER TABLE Users ADD SpouseIncome DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'MotherMaidenName')
BEGIN
    ALTER TABLE Users ADD MotherMaidenName NVARCHAR(200) NULL;
END
GO

-- Employment Verification Status
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerified')
BEGIN
    ALTER TABLE Users ADD EmploymentVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerifiedBy')
BEGIN
    ALTER TABLE Users ADD EmploymentVerifiedBy NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmploymentVerifiedAt')
BEGIN
    ALTER TABLE Users ADD EmploymentVerifiedAt DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerified')
BEGIN
    ALTER TABLE Users ADD IncomeVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerifiedBy')
BEGIN
    ALTER TABLE Users ADD IncomeVerifiedBy NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IncomeVerifiedAt')
BEGIN
    ALTER TABLE Users ADD IncomeVerifiedAt DATETIME NULL;
END
GO

-- Risk Profile
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CustomerRiskRating')
BEGIN
    ALTER TABLE Users ADD CustomerRiskRating NVARCHAR(20) NULL; -- LOW, MEDIUM, HIGH
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'InternalCreditScore')
BEGIN
    ALTER TABLE Users ADD InternalCreditScore INT NULL; -- 300-850 simulated score
END
GO

PRINT 'Users table enhancement completed.';
GO

-- =====================================================================
-- PART 2: ENHANCE CUSTOMER ACCOUNTS TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'AccountType')
BEGIN
    ALTER TABLE CustomerAccounts ADD AccountType NVARCHAR(50) NULL DEFAULT 'Savings';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'AccountPurpose')
BEGIN
    ALTER TABLE CustomerAccounts ADD AccountPurpose NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'InitialDeposit')
BEGIN
    ALTER TABLE CustomerAccounts ADD InitialDeposit DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'EligibilityStatus')
BEGIN
    ALTER TABLE CustomerAccounts ADD EligibilityStatus NVARCHAR(50) NULL DEFAULT 'PENDING'; -- PENDING, ELIGIBLE, NOT_ELIGIBLE
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'EligibilityRemarks')
BEGIN
    ALTER TABLE CustomerAccounts ADD EligibilityRemarks NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'EligibilityCheckedBy')
BEGIN
    ALTER TABLE CustomerAccounts ADD EligibilityCheckedBy NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'EligibilityCheckedAt')
BEGIN
    ALTER TABLE CustomerAccounts ADD EligibilityCheckedAt DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'CanApplyForLoan')
BEGIN
    ALTER TABLE CustomerAccounts ADD CanApplyForLoan BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'LoanEligibilityReason')
BEGIN
    ALTER TABLE CustomerAccounts ADD LoanEligibilityReason NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAccounts') AND name = 'MaxLoanAmount')
BEGIN
    ALTER TABLE CustomerAccounts ADD MaxLoanAmount DECIMAL(18,2) NULL;
END
GO

PRINT 'CustomerAccounts table enhancement completed.';
GO

-- =====================================================================
-- PART 3: ENHANCE LOAN APPLICATIONS TABLE
-- =====================================================================

-- Remove EmploymentStatus and MonthlyIncome from LoanApplication (already in User)
-- Add new fields for enhanced loan application

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'RequestedTermMonths')
BEGIN
    ALTER TABLE LoanApplications ADD RequestedTermMonths INT NULL DEFAULT 12;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'RequestedInterestRate')
BEGIN
    ALTER TABLE LoanApplications ADD RequestedInterestRate DECIMAL(5,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'LoanCategory')
BEGIN
    ALTER TABLE LoanApplications ADD LoanCategory NVARCHAR(50) NULL DEFAULT 'UNSECURED'; -- SECURED, UNSECURED
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PurposeCategory')
BEGIN
    ALTER TABLE LoanApplications ADD PurposeCategory NVARCHAR(100) NULL; -- Medical, Education, Business Capital, etc.
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PurposeDetails')
BEGIN
    ALTER TABLE LoanApplications ADD PurposeDetails NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ExistingDebtsTotal')
BEGIN
    ALTER TABLE LoanApplications ADD ExistingDebtsTotal DECIMAL(18,2) NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ExistingMonthlyPayments')
BEGIN
    ALTER TABLE LoanApplications ADD ExistingMonthlyPayments DECIMAL(18,2) NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ExistingDebtsDetails')
BEGIN
    ALTER TABLE LoanApplications ADD ExistingDebtsDetails NVARCHAR(1000) NULL; -- JSON or description
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasCollateral')
BEGIN
    ALTER TABLE LoanApplications ADD HasCollateral BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasCoBorrower')
BEGIN
    ALTER TABLE LoanApplications ADD HasCoBorrower BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasGuarantor')
BEGIN
    ALTER TABLE LoanApplications ADD HasGuarantor BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BankStatementSubmitted')
BEGIN
    ALTER TABLE LoanApplications ADD BankStatementSubmitted BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BankStatementMonths')
BEGIN
    ALTER TABLE LoanApplications ADD BankStatementMonths INT NULL; -- How many months of statements
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PayslipsSubmitted')
BEGIN
    ALTER TABLE LoanApplications ADD PayslipsSubmitted BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ITRSubmitted')
BEGIN
    ALTER TABLE LoanApplications ADD ITRSubmitted BIT NULL DEFAULT 0; -- Income Tax Return
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'COESubmitted')
BEGIN
    ALTER TABLE LoanApplications ADD COESubmitted BIT NULL DEFAULT 0; -- Certificate of Employment
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BusinessPermitSubmitted')
BEGIN
    ALTER TABLE LoanApplications ADD BusinessPermitSubmitted BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedTerms')
BEGIN
    ALTER TABLE LoanApplications ADD AcknowledgedTerms BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedPenaltyTerms')
BEGIN
    ALTER TABLE LoanApplications ADD AcknowledgedPenaltyTerms BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedLegalAction')
BEGIN
    ALTER TABLE LoanApplications ADD AcknowledgedLegalAction BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ApplicantSignature')
BEGIN
    ALTER TABLE LoanApplications ADD ApplicantSignature NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ApplicantSignatureDate')
BEGIN
    ALTER TABLE LoanApplications ADD ApplicantSignatureDate DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerReviewedBy')
BEGIN
    ALTER TABLE LoanApplications ADD TellerReviewedBy NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerReviewedAt')
BEGIN
    ALTER TABLE LoanApplications ADD TellerReviewedAt DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerRemarks')
BEGIN
    ALTER TABLE LoanApplications ADD TellerRemarks NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerRecommendation')
BEGIN
    ALTER TABLE LoanApplications ADD TellerRecommendation NVARCHAR(50) NULL; -- RECOMMEND_APPROVE, RECOMMEND_DECLINE
END
GO

PRINT 'LoanApplications table enhancement completed.';
GO

-- =====================================================================
-- PART 4: CREATE LOAN COLLATERAL TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanCollaterals') AND type = 'U')
BEGIN
    CREATE TABLE LoanCollaterals (
        CollateralId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        CollateralType NVARCHAR(50) NOT NULL, -- Real Estate, Vehicle, Equipment, Jewelry, Securities, Deposit
        CollateralDescription NVARCHAR(500) NOT NULL,
        CollateralLocation NVARCHAR(500) NULL,
        
        -- Valuation
        EstimatedValue DECIMAL(18,2) NOT NULL,
        AppraisedValue DECIMAL(18,2) NULL,
        AppraisalDate DATE NULL,
        AppraisedBy NVARCHAR(200) NULL,
        LoanToValueRatio DECIMAL(5,2) NULL, -- LTV%
        
        -- Ownership
        OwnerName NVARCHAR(200) NOT NULL,
        OwnerRelationship NVARCHAR(50) NULL, -- Self, Spouse, Parent, etc.
        
        -- Documents
        TitleNumber NVARCHAR(100) NULL,
        RegistrationNumber NVARCHAR(100) NULL,
        DocumentsSubmitted NVARCHAR(500) NULL, -- JSON list
        
        -- Status
        VerificationStatus NVARCHAR(50) DEFAULT 'PENDING', -- PENDING, VERIFIED, REJECTED
        VerifiedBy NVARCHAR(100) NULL,
        VerifiedAt DATETIME NULL,
        VerificationRemarks NVARCHAR(500) NULL,
        
        -- Insurance
        IsInsured BIT DEFAULT 0,
        InsuranceCompany NVARCHAR(200) NULL,
        InsurancePolicyNumber NVARCHAR(100) NULL,
        InsuranceExpiryDate DATE NULL,
        
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        
        FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(ApplicationId)
    );
    PRINT 'LoanCollaterals table created.';
END
GO

-- =====================================================================
-- PART 5: CREATE CO-BORROWER/GUARANTOR TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanCoBorrowers') AND type = 'U')
BEGIN
    CREATE TABLE LoanCoBorrowers (
        CoBorrowerId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        RelationshipType NVARCHAR(50) NOT NULL, -- CO_BORROWER, GUARANTOR
        
        -- Personal Information
        FullName NVARCHAR(200) NOT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(20) NULL,
        CivilStatus NVARCHAR(20) NULL,
        Nationality NVARCHAR(50) DEFAULT 'Filipino',
        
        -- Contact
        Address NVARCHAR(500) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        
        -- Employment
        EmploymentStatus NVARCHAR(50) NULL,
        EmployerName NVARCHAR(200) NULL,
        EmployerAddress NVARCHAR(500) NULL,
        JobTitle NVARCHAR(100) NULL,
        YearsEmployed INT NULL,
        MonthlyIncome DECIMAL(18,2) NULL,
        
        -- Government IDs
        TIN NVARCHAR(20) NULL,
        SSSNumber NVARCHAR(20) NULL,
        ValidIdType NVARCHAR(50) NULL,
        ValidIdNumber NVARCHAR(50) NULL,
        
        -- Relationship to Borrower
        RelationshipToBorrower NVARCHAR(50) NULL, -- Spouse, Parent, Sibling, Friend, etc.
        
        -- Verification
        VerificationStatus NVARCHAR(50) DEFAULT 'PENDING',
        VerifiedBy NVARCHAR(100) NULL,
        VerifiedAt DATETIME NULL,
        VerificationRemarks NVARCHAR(500) NULL,
        
        -- Acknowledgment
        HasAcknowledged BIT DEFAULT 0,
        AcknowledgmentDate DATETIME NULL,
        Signature NVARCHAR(200) NULL,
        
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        
        FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(ApplicationId)
    );
    PRINT 'LoanCoBorrowers table created.';
END
GO

-- =====================================================================
-- PART 6: ENHANCE LOAN ASSESSMENTS TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'CreditScore')
BEGIN
    ALTER TABLE LoanAssessments ADD CreditScore INT NULL; -- 300-850
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'CreditScoreSource')
BEGIN
    ALTER TABLE LoanAssessments ADD CreditScoreSource NVARCHAR(50) NULL; -- INTERNAL, CIBI, TransUnion
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'DebtToIncomeRatio')
BEGIN
    ALTER TABLE LoanAssessments ADD DebtToIncomeRatio DECIMAL(5,2) NULL; -- Percentage
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'RiskScore')
BEGIN
    ALTER TABLE LoanAssessments ADD RiskScore INT NULL; -- 1-100
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'RiskCategory')
BEGIN
    ALTER TABLE LoanAssessments ADD RiskCategory NVARCHAR(20) NULL; -- LOW, MEDIUM, HIGH, VERY_HIGH
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'RiskAssessmentDetails')
BEGIN
    ALTER TABLE LoanAssessments ADD RiskAssessmentDetails NVARCHAR(2000) NULL; -- JSON with detailed breakdown
END
GO

-- Verification Checklist Results
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'IdentityVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD IdentityVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'IncomeVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD IncomeVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'AddressVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD AddressVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'EmploymentVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD EmploymentVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'CollateralVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD CollateralVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'CoBorrowerVerified')
BEGIN
    ALTER TABLE LoanAssessments ADD CoBorrowerVerified BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'BankStatementsReviewed')
BEGIN
    ALTER TABLE LoanAssessments ADD BankStatementsReviewed BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'AverageMonthlyBalance')
BEGIN
    ALTER TABLE LoanAssessments ADD AverageMonthlyBalance DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'VerificationNotes')
BEGIN
    ALTER TABLE LoanAssessments ADD VerificationNotes NVARCHAR(2000) NULL;
END
GO

-- Recommendation
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'AccountantRecommendation')
BEGIN
    ALTER TABLE LoanAssessments ADD AccountantRecommendation NVARCHAR(50) NULL; -- RECOMMEND_APPROVE, RECOMMEND_DECLINE, NEEDS_REVIEW
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'RecommendationReason')
BEGIN
    ALTER TABLE LoanAssessments ADD RecommendationReason NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'SuggestedAmount')
BEGIN
    ALTER TABLE LoanAssessments ADD SuggestedAmount DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'SuggestedTerm')
BEGIN
    ALTER TABLE LoanAssessments ADD SuggestedTerm INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanAssessments') AND name = 'SuggestedRate')
BEGIN
    ALTER TABLE LoanAssessments ADD SuggestedRate DECIMAL(5,2) NULL;
END
GO

PRINT 'LoanAssessments table enhancement completed.';
GO

-- =====================================================================
-- PART 7: ENHANCE LOAN APPROVALS TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ApprovalReason')
BEGIN
    ALTER TABLE LoanApprovals ADD ApprovalReason NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ReviewedCreditScore')
BEGIN
    ALTER TABLE LoanApprovals ADD ReviewedCreditScore BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ReviewedDTI')
BEGIN
    ALTER TABLE LoanApprovals ADD ReviewedDTI BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ReviewedRiskAssessment')
BEGIN
    ALTER TABLE LoanApprovals ADD ReviewedRiskAssessment BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ReviewedCollateral')
BEGIN
    ALTER TABLE LoanApprovals ADD ReviewedCollateral BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'ReviewedCoBorrower')
BEGIN
    ALTER TABLE LoanApprovals ADD ReviewedCoBorrower BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'FinalDecisionNotes')
BEGIN
    ALTER TABLE LoanApprovals ADD FinalDecisionNotes NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'RequiresAdditionalDocuments')
BEGIN
    ALTER TABLE LoanApprovals ADD RequiresAdditionalDocuments BIT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApprovals') AND name = 'AdditionalDocumentsRequired')
BEGIN
    ALTER TABLE LoanApprovals ADD AdditionalDocumentsRequired NVARCHAR(500) NULL;
END
GO

PRINT 'LoanApprovals table enhancement completed.';
GO

-- =====================================================================
-- PART 8: CREATE LOAN CONTRACT TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanContracts') AND type = 'U')
BEGIN
    CREATE TABLE LoanContracts (
        ContractId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        ApplicationId INT NOT NULL,
        ApprovalId INT NOT NULL,
        
        -- Contract Identification
        ContractNumber NVARCHAR(50) NOT NULL,
        ContractDate DATE NOT NULL,
        EffectiveDate DATE NOT NULL,
        MaturityDate DATE NOT NULL,
        
        -- Loan Terms
        PrincipalAmount DECIMAL(18,2) NOT NULL,
        InterestRate DECIMAL(5,2) NOT NULL,
        TermMonths INT NOT NULL,
        MonthlyPayment DECIMAL(18,2) NOT NULL,
        TotalInterest DECIMAL(18,2) NOT NULL,
        TotalPayable DECIMAL(18,2) NOT NULL,
        
        -- Payment Terms
        PaymentDueDay INT NOT NULL DEFAULT 5, -- Day of month payment is due
        FirstPaymentDate DATE NOT NULL,
        LastPaymentDate DATE NOT NULL,
        
        -- Grace Period and Penalties (Industry Standard)
        GracePeriodDays INT NOT NULL DEFAULT 5, -- 5 days grace period
        LatePenaltyRate DECIMAL(5,4) NOT NULL DEFAULT 0.0005, -- 0.05% per day (industry standard)
        LatePenaltyType NVARCHAR(20) NOT NULL DEFAULT 'DAILY', -- DAILY, MONTHLY, FIXED
        MaxPenaltyRate DECIMAL(5,2) NULL DEFAULT 25.00, -- Max 25% of outstanding
        
        -- Legal Action Thresholds
        FirstWarningDays INT NOT NULL DEFAULT 7, -- Days overdue for first reminder
        SecondWarningDays INT NOT NULL DEFAULT 15, -- Days overdue for second warning
        DemandLetterDays INT NOT NULL DEFAULT 30, -- Days overdue for demand letter
        LegalActionDays INT NOT NULL DEFAULT 60, -- Days overdue for legal action
        CollectionAgencyDays INT NOT NULL DEFAULT 90, -- Days overdue for collection agency
        
        -- Legal Terms Text
        LegalWarningText NVARCHAR(MAX) NULL,
        PenaltyClauseText NVARCHAR(MAX) NULL,
        DefaultClauseText NVARCHAR(MAX) NULL,
        CollectionClauseText NVARCHAR(MAX) NULL,
        
        -- Acknowledgments
        BorrowerFullName NVARCHAR(200) NOT NULL,
        BorrowerAddress NVARCHAR(500) NOT NULL,
        BorrowerSignature NVARCHAR(200) NULL,
        BorrowerSignedAt DATETIME NULL,
        
        CoBorrowerFullName NVARCHAR(200) NULL,
        CoBorrowerSignature NVARCHAR(200) NULL,
        CoBorrowerSignedAt DATETIME NULL,
        
        GuarantorFullName NVARCHAR(200) NULL,
        GuarantorSignature NVARCHAR(200) NULL,
        GuarantorSignedAt DATETIME NULL,
        
        -- Witness
        WitnessName NVARCHAR(200) NULL,
        WitnessSignature NVARCHAR(200) NULL,
        
        -- Bank Representative
        BankRepresentativeName NVARCHAR(200) NOT NULL,
        BankRepresentativeTitle NVARCHAR(100) NOT NULL,
        BankRepresentativeSignature NVARCHAR(200) NULL,
        
        -- Contract Status
        ContractStatus NVARCHAR(50) NOT NULL DEFAULT 'DRAFT', -- DRAFT, PENDING_SIGNATURE, ACTIVE, COMPLETED, DEFAULTED, LEGAL_ACTION
        
        -- Notarization (Optional)
        IsNotarized BIT DEFAULT 0,
        NotaryName NVARCHAR(200) NULL,
        NotaryNumber NVARCHAR(50) NULL,
        NotarizationDate DATE NULL,
        
        -- Document
        ContractDocument VARBINARY(MAX) NULL, -- PDF of signed contract
        ContractDocumentName NVARCHAR(200) NULL,
        
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        
        FOREIGN KEY (LoanId) REFERENCES Loans(LoanId),
        FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(ApplicationId),
        FOREIGN KEY (ApprovalId) REFERENCES LoanApprovals(ApprovalId)
    );
    
    CREATE INDEX IX_LoanContracts_LoanId ON LoanContracts(LoanId);
    CREATE INDEX IX_LoanContracts_ContractNumber ON LoanContracts(ContractNumber);
    
    PRINT 'LoanContracts table created.';
END
GO

-- =====================================================================
-- PART 9: CREATE VERIFICATION CHECKLIST TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanVerificationChecklists') AND type = 'U')
BEGIN
    CREATE TABLE LoanVerificationChecklists (
        ChecklistId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        
        -- Stage
        VerificationStage NVARCHAR(50) NOT NULL, -- TELLER, ACCOUNTANT, FINANCE_MANAGER
        
        -- Identity Verification
        ValidIdChecked BIT DEFAULT 0,
        ValidIdType NVARCHAR(50) NULL,
        ValidIdNumber NVARCHAR(50) NULL,
        ValidIdExpiry DATE NULL,
        ValidIdRemarks NVARCHAR(200) NULL,
        
        SecondaryIdChecked BIT DEFAULT 0,
        SecondaryIdType NVARCHAR(50) NULL,
        
        PhotoMatchesId BIT DEFAULT 0,
        SignatureVerified BIT DEFAULT 0,
        
        -- Income Verification
        PayslipVerified BIT DEFAULT 0,
        PayslipMonths INT NULL,
        PayslipRemarks NVARCHAR(200) NULL,
        
        COEVerified BIT DEFAULT 0,
        COEDate DATE NULL,
        COERemarks NVARCHAR(200) NULL,
        
        ITRVerified BIT DEFAULT 0,
        ITRYear INT NULL,
        ITRRemarks NVARCHAR(200) NULL,
        
        BankStatementVerified BIT DEFAULT 0,
        BankStatementMonths INT NULL,
        BankStatementRemarks NVARCHAR(200) NULL,
        
        -- Business Verification (for self-employed)
        BusinessPermitVerified BIT DEFAULT 0,
        DTIRegistrationVerified BIT DEFAULT 0,
        SECRegistrationVerified BIT DEFAULT 0,
        AuditedFSVerified BIT DEFAULT 0,
        
        -- Address Verification
        ProofOfAddressChecked BIT DEFAULT 0,
        ProofOfAddressType NVARCHAR(50) NULL, -- Utility Bill, Barangay Clearance, etc.
        AddressMatchesRecord BIT DEFAULT 0,
        
        -- Employment Verification
        EmployerContactVerified BIT DEFAULT 0,
        EmploymentCallMade BIT DEFAULT 0,
        EmploymentCallDate DATE NULL,
        EmploymentCallNotes NVARCHAR(500) NULL,
        
        -- Credit Check
        CreditCheckPerformed BIT DEFAULT 0,
        CreditCheckDate DATE NULL,
        CreditCheckSource NVARCHAR(50) NULL,
        CreditCheckResult NVARCHAR(50) NULL, -- PASS, FAIL, NEEDS_REVIEW
        
        -- Collateral Verification
        CollateralDocumentsChecked BIT DEFAULT 0,
        CollateralAppraisalDone BIT DEFAULT 0,
        CollateralInsuranceVerified BIT DEFAULT 0,
        
        -- Co-Borrower/Guarantor Verification
        CoBorrowerIdVerified BIT DEFAULT 0,
        CoBorrowerIncomeVerified BIT DEFAULT 0,
        GuarantorIdVerified BIT DEFAULT 0,
        GuarantorIncomeVerified BIT DEFAULT 0,
        
        -- Overall
        OverallStatus NVARCHAR(50) DEFAULT 'PENDING', -- PENDING, COMPLETE, INCOMPLETE, FAILED
        CompletionPercentage INT DEFAULT 0,
        MissingItems NVARCHAR(1000) NULL,
        
        VerifiedBy NVARCHAR(100) NOT NULL,
        VerifiedAt DATETIME DEFAULT GETDATE(),
        Remarks NVARCHAR(2000) NULL,
        
        FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(ApplicationId)
    );
    
    CREATE INDEX IX_LoanVerificationChecklists_ApplicationId ON LoanVerificationChecklists(ApplicationId);
    
    PRINT 'LoanVerificationChecklists table created.';
END
GO

-- =====================================================================
-- PART 10: CREATE LOAN DEFAULT/DELINQUENCY TRACKING TABLE
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanDelinquencies') AND type = 'U')
BEGIN
    CREATE TABLE LoanDelinquencies (
        DelinquencyId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        ContractId INT NULL,
        
        -- Delinquency Details
        DelinquencyType NVARCHAR(50) NOT NULL, -- LATE_PAYMENT, MISSED_PAYMENT, CHRONIC_DELINQUENT, DEFAULT
        DaysOverdue INT NOT NULL,
        OverdueAmount DECIMAL(18,2) NOT NULL,
        PenaltyAmount DECIMAL(18,2) NOT NULL,
        TotalAmountDue DECIMAL(18,2) NOT NULL,
        
        -- Action Stage
        ActionStage NVARCHAR(50) NOT NULL, -- REMINDER, WARNING, DEMAND_LETTER, LEGAL_NOTICE, LEGAL_ACTION, COLLECTION
        ActionDate DATE NOT NULL,
        ActionTakenBy NVARCHAR(100) NOT NULL,
        
        -- Communication
        CommunicationType NVARCHAR(50) NULL, -- SMS, EMAIL, PHONE, LETTER, LEGAL_NOTICE
        CommunicationSentAt DATETIME NULL,
        CommunicationContent NVARCHAR(MAX) NULL,
        CustomerResponse NVARCHAR(1000) NULL,
        CustomerResponseDate DATETIME NULL,
        
        -- Legal Action Details
        LawyerAssigned NVARCHAR(200) NULL,
        LegalCaseNumber NVARCHAR(100) NULL,
        LegalFilingDate DATE NULL,
        LegalStatus NVARCHAR(50) NULL,
        LegalNotes NVARCHAR(2000) NULL,
        
        -- Collection Agency Details
        CollectionAgencyName NVARCHAR(200) NULL,
        CollectionAgencyContact NVARCHAR(100) NULL,
        CollectionReferralDate DATE NULL,
        CollectionStatus NVARCHAR(50) NULL,
        
        -- Resolution
        IsResolved BIT DEFAULT 0,
        ResolutionType NVARCHAR(50) NULL, -- PAID, RESTRUCTURED, SETTLED, WRITTEN_OFF
        ResolutionDate DATE NULL,
        ResolutionNotes NVARCHAR(1000) NULL,
        ResolvedBy NVARCHAR(100) NULL,
        
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        
        FOREIGN KEY (LoanId) REFERENCES Loans(LoanId),
        FOREIGN KEY (ContractId) REFERENCES LoanContracts(ContractId)
    );
    
    CREATE INDEX IX_LoanDelinquencies_LoanId ON LoanDelinquencies(LoanId);
    CREATE INDEX IX_LoanDelinquencies_ActionStage ON LoanDelinquencies(ActionStage);
    
    PRINT 'LoanDelinquencies table created.';
END
GO

-- =====================================================================
-- PART 11: INSERT DEFAULT LEGAL TEXT TEMPLATES
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LoanLegalTemplates') AND type = 'U')
BEGIN
    CREATE TABLE LoanLegalTemplates (
        TemplateId INT IDENTITY(1,1) PRIMARY KEY,
        TemplateName NVARCHAR(100) NOT NULL,
        TemplateType NVARCHAR(50) NOT NULL, -- PENALTY_CLAUSE, DEFAULT_CLAUSE, LEGAL_WARNING, COLLECTION_WARNING
        TemplateContent NVARCHAR(MAX) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL
    );
    
    -- Insert standard Philippine banking legal templates
    INSERT INTO LoanLegalTemplates (TemplateName, TemplateType, TemplateContent) VALUES
    ('Standard Penalty Clause', 'PENALTY_CLAUSE', 
    'In case of delay in the payment of any installment due, the BORROWER agrees to pay a penalty charge equivalent to 0.05% (five hundredths of one percent) per day of the amount due, computed from the due date until fully paid. The penalty shall not exceed 25% of the total outstanding balance.'),
    
    ('Default Warning - 30 Days', 'DEFAULT_CLAUSE',
    'WARNING: Your loan account is now THIRTY (30) DAYS PAST DUE. Failure to settle your outstanding obligation within SEVEN (7) days from receipt of this notice may result in the following actions: (1) Acceleration of the entire loan balance; (2) Filing of appropriate legal action for collection; (3) Reporting to credit bureaus which may affect your credit standing; (4) Additional penalties and collection costs.'),
    
    ('Legal Action Warning', 'LEGAL_WARNING',
    'FINAL DEMAND NOTICE: This serves as your FINAL NOTICE that your loan account remains unpaid despite previous reminders. The Bank hereby demands payment of your total outstanding obligation within FIVE (5) days from receipt hereof. FAILURE TO COMPLY shall constrain the Bank, without further notice, to: (1) File appropriate CIVIL and/or CRIMINAL cases against you before the proper courts; (2) Engage the services of a collection agency; (3) Report your delinquency to the Credit Information Corporation (CIC) and other credit bureaus. This legal action may result in court proceedings, garnishment of wages, attachment of assets, and other remedies available under Philippine law.'),
    
    ('Collection Agency Notice', 'COLLECTION_WARNING',
    'NOTICE OF ACCOUNT REFERRAL: Please be informed that due to your continued failure to settle your outstanding obligation despite repeated demands, your account has been ENDORSED to a licensed collection agency for appropriate action. The collection agency is authorized to: (1) Contact you at your residence, workplace, or other known addresses; (2) Pursue all available legal remedies for the collection of your debt; (3) Report your delinquent status to credit bureaus. Additional collection costs and legal fees will be charged to your account. To avoid further action, please contact us immediately to arrange payment.'),
    
    ('Criminal Liability Warning', 'CRIMINAL_WARNING',
    'IMPORTANT LEGAL NOTICE: Under Philippine law, specifically Article 315 of the Revised Penal Code (Estafa) and Batas Pambansa Blg. 22 (Bouncing Checks Law), willful failure to pay obligations with intent to defraud may constitute a criminal offense. This is to inform you that the Bank reserves the right to file appropriate criminal charges should you continue to evade your lawful obligation. A police report and/or complaint-affidavit may be filed with the proper authorities.');
    
    PRINT 'LoanLegalTemplates table created and populated.';
END
GO

-- =====================================================================
-- PART 12: ADD INDEXES FOR PERFORMANCE
-- =====================================================================

-- Index for user employment verification
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_EmploymentStatus')
    CREATE INDEX IX_Users_EmploymentStatus ON Users(EmploymentStatus);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_CustomerRiskRating')
    CREATE INDEX IX_Users_CustomerRiskRating ON Users(CustomerRiskRating);
GO

-- Index for loan application status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoanApplications_Status_Date')
    CREATE INDEX IX_LoanApplications_Status_Date ON LoanApplications(Status, ApplicationDate);
GO

-- Index for loan assessments
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoanAssessments_RiskCategory')
    CREATE INDEX IX_LoanAssessments_RiskCategory ON LoanAssessments(RiskCategory);
GO

PRINT 'All indexes created successfully.';
GO

PRINT '=====================================================================';
PRINT 'LOAN PROCESS ENHANCEMENT COMPLETED SUCCESSFULLY!';
PRINT '=====================================================================';
PRINT 'Summary of changes:';
PRINT '1. Users table - Added 30+ employment and personal fields';
PRINT '2. CustomerAccounts table - Added eligibility and loan qualification fields';
PRINT '3. LoanApplications table - Added 25+ enhanced application fields';
PRINT '4. LoanCollaterals table - NEW table for secured loans';
PRINT '5. LoanCoBorrowers table - NEW table for co-borrowers/guarantors';
PRINT '6. LoanAssessments table - Added credit score, DTI, risk assessment fields';
PRINT '7. LoanApprovals table - Added detailed review criteria fields';
PRINT '8. LoanContracts table - NEW comprehensive contract with legal terms';
PRINT '9. LoanVerificationChecklists table - NEW detailed verification tracking';
PRINT '10. LoanDelinquencies table - NEW delinquency and legal action tracking';
PRINT '11. LoanLegalTemplates table - NEW standard legal text templates';
PRINT '=====================================================================';
GO
