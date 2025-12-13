-- =====================================================================
-- ADD MISSING LOAN DOCUMENT AND ACKNOWLEDGMENT COLUMNS
-- Run this script to add document tracking fields to LoanApplications table
-- =====================================================================

USE BFASDatabase;
GO

-- =====================================================================
-- LOAN APPLICATION - DOCUMENT SUBMISSION TRACKING COLUMNS
-- =====================================================================

-- Add document submission flags
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BankStatementSubmitted')
    ALTER TABLE LoanApplications ADD BankStatementSubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BankStatementMonths')
    ALTER TABLE LoanApplications ADD BankStatementMonths INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PayslipsSubmitted')
    ALTER TABLE LoanApplications ADD PayslipsSubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ITRSubmitted')
    ALTER TABLE LoanApplications ADD ITRSubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'COESubmitted')
    ALTER TABLE LoanApplications ADD COESubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'BusinessPermitSubmitted')
    ALTER TABLE LoanApplications ADD BusinessPermitSubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ValidIdSubmitted')
    ALTER TABLE LoanApplications ADD ValidIdSubmitted BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ProofOfBillingSubmitted')
    ALTER TABLE LoanApplications ADD ProofOfBillingSubmitted BIT NOT NULL DEFAULT 0;
GO

-- =====================================================================
-- LOAN APPLICATION - LEGAL ACKNOWLEDGMENT COLUMNS
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedTerms')
    ALTER TABLE LoanApplications ADD AcknowledgedTerms BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedPenaltyTerms')
    ALTER TABLE LoanApplications ADD AcknowledgedPenaltyTerms BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedLegalAction')
    ALTER TABLE LoanApplications ADD AcknowledgedLegalAction BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'AcknowledgedInfoAccuracy')
    ALTER TABLE LoanApplications ADD AcknowledgedInfoAccuracy BIT NOT NULL DEFAULT 0;
GO

-- =====================================================================
-- LOAN APPLICATION - ADDITIONAL FIELDS
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'RequestedTermMonths')
    ALTER TABLE LoanApplications ADD RequestedTermMonths INT NOT NULL DEFAULT 12;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'LoanCategory')
    ALTER TABLE LoanApplications ADD LoanCategory NVARCHAR(50) NULL DEFAULT 'UNSECURED';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasCollateral')
    ALTER TABLE LoanApplications ADD HasCollateral BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasCoBorrower')
    ALTER TABLE LoanApplications ADD HasCoBorrower BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'HasGuarantor')
    ALTER TABLE LoanApplications ADD HasGuarantor BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'OtherIncome')
    ALTER TABLE LoanApplications ADD OtherIncome DECIMAL(18,2) NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ExistingDebtsTotal')
    ALTER TABLE LoanApplications ADD ExistingDebtsTotal DECIMAL(18,2) NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ExistingMonthlyPayments')
    ALTER TABLE LoanApplications ADD ExistingMonthlyPayments DECIMAL(18,2) NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerReviewedBy')
    ALTER TABLE LoanApplications ADD TellerReviewedBy NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerReviewedAt')
    ALTER TABLE LoanApplications ADD TellerReviewedAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerRemarks')
    ALTER TABLE LoanApplications ADD TellerRemarks NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'TellerRecommendation')
    ALTER TABLE LoanApplications ADD TellerRecommendation NVARCHAR(50) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PurposeCategory')
    ALTER TABLE LoanApplications ADD PurposeCategory NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'PurposeDetails')
    ALTER TABLE LoanApplications ADD PurposeDetails NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'Documents')
    ALTER TABLE LoanApplications ADD Documents NVARCHAR(2000) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ApplicantSignature')
    ALTER TABLE LoanApplications ADD ApplicantSignature NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoanApplications') AND name = 'ApplicantSignatureDate')
    ALTER TABLE LoanApplications ADD ApplicantSignatureDate DATETIME NULL;
GO

-- =====================================================================
-- LOAN DOCUMENTS TABLE - For storing uploaded document files
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('LoanDocuments') AND type = 'U')
BEGIN
    CREATE TABLE LoanDocuments (
        DocumentId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        DocumentType NVARCHAR(50) NOT NULL, -- BankStatement, Payslip, ITR, COE, ValidID, ProofOfBilling
        FileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NULL,
        FileData VARBINARY(MAX) NOT NULL,
        FileSize BIGINT NULL,
        UploadedBy NVARCHAR(100) NOT NULL,
        UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),
        VerifiedBy NVARCHAR(100) NULL,
        VerifiedAt DATETIME NULL,
        VerificationStatus NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING, VERIFIED, REJECTED
        VerificationNotes NVARCHAR(500) NULL,
        FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(ApplicationId)
    );
    PRINT 'Table LoanDocuments created successfully.';
END
GO

-- Add index for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoanDocuments_ApplicationId')
    CREATE INDEX IX_LoanDocuments_ApplicationId ON LoanDocuments(ApplicationId);
GO

PRINT 'Loan document and acknowledgment columns have been added successfully!';
PRINT 'Please restart the application for changes to take effect.';
GO
