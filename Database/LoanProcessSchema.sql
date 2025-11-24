-- =====================================================================
-- LOAN PROCESS DATABASE SCHEMA - COMPLETE WORKFLOW IMPLEMENTATION
-- =====================================================================

-- 1. LOAN APPLICATIONS TABLE (Customer Submission Stage)
CREATE TABLE [dbo].[LoanApplications](
    [ApplicationId] [int] IDENTITY(1,1) NOT NULL,
    [ApplicationNumber] [nvarchar](50) NOT NULL UNIQUE,
    [AccountId] [int] NOT NULL,
    [LoanType] [nvarchar](50) NOT NULL,  -- Personal, Home, Auto, Education
    [RequestedAmount] [decimal](18, 2) NOT NULL,
    [Purpose] [nvarchar](500) NULL,
    [EmploymentStatus] [nvarchar](50) NULL,
    [MonthlyIncome] [decimal](18, 2) NULL,
    [ExistingLoans] [int] DEFAULT 0,
    [Documents] [nvarchar](max) NULL,  -- JSON array of document names
    [Status] [nvarchar](50) NOT NULL DEFAULT 'SUBMITTED',  -- SUBMITTED, VERIFIED, REJECTED
    [ApplicationDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [SubmittedBy] [nvarchar](50) NOT NULL,
    [RejectionReason] [nvarchar](500) NULL,
    [RejectedAt] [datetime] NULL,
    [RejectedBy] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CustomerAccounts]([AccountId])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- 2. LOAN ASSESSMENT TABLE (Accountant Review & Computation)
CREATE TABLE [dbo].[LoanAssessments](
    [AssessmentId] [int] IDENTITY(1,1) NOT NULL,
    [ApplicationId] [int] NOT NULL,
    [AccountId] [int] NOT NULL,
    [LoanAmount] [decimal](18, 2) NOT NULL,
    [InterestRate] [decimal](5, 2) NOT NULL,
    [TermMonths] [int] NOT NULL,
    [ComputedMonthlyPayment] [decimal](18, 2) NOT NULL,
    [TotalPayable] [decimal](18, 2) NOT NULL,
    [AssessmentDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [AssessmentStatus] [nvarchar](50) NOT NULL DEFAULT 'ASSESSED',  -- ASSESSED, FORWARDED
    [AssessedBy] [nvarchar](50) NOT NULL,
    [Remarks] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([AssessmentId] ASC),
FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[LoanApplications]([ApplicationId]),
FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CustomerAccounts]([AccountId])
) ON [PRIMARY]
GO

-- 3. LOAN APPROVAL TABLE (Finance Manager Decision)
CREATE TABLE [dbo].[LoanApprovals](
    [ApprovalId] [int] IDENTITY(1,1) NOT NULL,
    [AssessmentId] [int] NOT NULL,
    [ApplicationId] [int] NOT NULL,
    [AccountId] [int] NOT NULL,
    [ApprovalStatus] [nvarchar](50) NOT NULL,  -- APPROVED, DECLINED
    [ApprovalDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [ApprovedBy] [nvarchar](50) NOT NULL,
    [ApprovedAmount] [decimal](18, 2) NULL,
    [ApprovedInterestRate] [decimal](5, 2) NULL,
    [ApprovedTermMonths] [int] NULL,
    [SpecialConditions] [nvarchar](500) NULL,
    [DeclinationReason] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([ApprovalId] ASC),
FOREIGN KEY ([AssessmentId]) REFERENCES [dbo].[LoanAssessments]([AssessmentId]),
FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[LoanApplications]([ApplicationId]),
FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CustomerAccounts]([AccountId])
) ON [PRIMARY]
GO

-- 4. LOAN PAYMENT SCHEDULE TABLE (Auto-generated Installments)
CREATE TABLE [dbo].[LoanPaymentSchedules](
    [ScheduleId] [int] IDENTITY(1,1) NOT NULL,
    [LoanId] [int] NOT NULL,
    [PaymentNumber] [int] NOT NULL,
    [DueDate] [date] NOT NULL,
    [MinimumPayment] [decimal](18, 2) NOT NULL,
    [PrincipalAmount] [decimal](18, 2) NOT NULL,
    [InterestAmount] [decimal](18, 2) NOT NULL,
    [PaymentStatus] [nvarchar](50) NOT NULL DEFAULT 'PENDING',  -- PENDING, PAID, OVERDUE, PARTIAL
    [DaysOverdue] [int] DEFAULT 0,
    [Penalty] [decimal](18, 2) DEFAULT 0,
PRIMARY KEY CLUSTERED ([ScheduleId] ASC),
FOREIGN KEY ([LoanId]) REFERENCES [dbo].[CustomerLoans]([LoanId])
) ON [PRIMARY]
GO

-- 5. LOAN PAYMENTS TABLE (Actual Payment Records)
CREATE TABLE [dbo].[LoanPayments](
    [PaymentId] [int] IDENTITY(1,1) NOT NULL,
    [ScheduleId] [int] NOT NULL,
    [LoanId] [int] NOT NULL,
    [PaymentAmount] [decimal](18, 2) NOT NULL,
    [PenaltyPaid] [decimal](18, 2) DEFAULT 0,
    [PaymentDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [PaymentMethod] [nvarchar](50) NOT NULL,  -- Cash, Check, Transfer, etc.
    [Reference] [nvarchar](100) NULL,
    [ProcessedBy] [nvarchar](50) NOT NULL,
    [IsLatePayment] [bit] DEFAULT 0,
PRIMARY KEY CLUSTERED ([PaymentId] ASC),
FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LoanPaymentSchedules]([ScheduleId]),
FOREIGN KEY ([LoanId]) REFERENCES [dbo].[CustomerLoans]([LoanId])
) ON [PRIMARY]
GO

-- 6. LOAN VIOLATIONS TABLE (Late/Missed Payments & Penalties)
CREATE TABLE [dbo].[LoanViolations](
    [ViolationId] [int] IDENTITY(1,1) NOT NULL,
    [LoanId] [int] NOT NULL,
    [ScheduleId] [int] NOT NULL,
    [ViolationType] [nvarchar](50) NOT NULL,  -- LATE_PAYMENT, MISSED_PAYMENT, REPEATED_VIOLATION
    [DaysOverdue] [int] NOT NULL,
    [PenaltyRate] [decimal](5, 2) NOT NULL,  -- BPI Standard Rate
    [PenaltyAmount] [decimal](18, 2) NOT NULL,
    [OutstandingBalance] [decimal](18, 2) NOT NULL,
    [ViolationDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [ResolvedAt] [datetime] NULL,
    [IsResolved] [bit] DEFAULT 0,
    [Remarks] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([ViolationId] ASC),
FOREIGN KEY ([LoanId]) REFERENCES [dbo].[CustomerLoans]([LoanId]),
FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LoanPaymentSchedules]([ScheduleId])
) ON [PRIMARY]
GO

-- 7. LOAN DISBURSALS TABLE (Funds Release Record)
CREATE TABLE [dbo].[LoanDisbursals](
    [DisbursalId] [int] IDENTITY(1,1) NOT NULL,
    [LoanId] [int] NOT NULL,
    [ApprovalId] [int] NOT NULL,
    [DisbursalAmount] [decimal](18, 2) NOT NULL,
    [DisbursalDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [DisbursalStatus] [nvarchar](50) NOT NULL DEFAULT 'RELEASED',  -- RELEASED, PENDING
    [DisbursedTo] [nvarchar](50) NOT NULL,  -- Account or Cash
    [ProcessedBy] [nvarchar](50) NOT NULL,
    [Reference] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED ([DisbursalId] ASC),
FOREIGN KEY ([LoanId]) REFERENCES [dbo].[CustomerLoans]([LoanId]),
FOREIGN KEY ([ApprovalId]) REFERENCES [dbo].[LoanApprovals]([ApprovalId])
) ON [PRIMARY]
GO

-- =====================================================================
-- ENHANCE EXISTING TABLES FOR LOAN PROCESS
-- =====================================================================

-- Modify CustomerLoans table to track complete loan lifecycle
ALTER TABLE [dbo].[CustomerLoans] ADD 
    [ApplicationId] [int] NULL,
    [AssessmentId] [int] NULL,
    [ApprovalId] [int] NULL,
    [DisbursalId] [int] NULL,
    [CumulativeLateDays] [int] DEFAULT 0,
    [TotalPenalties] [decimal](18, 2) DEFAULT 0,
    [LastPaymentDate] [datetime] NULL,
    [IsBlacklisted] [bit] DEFAULT 0,
    [BlacklistedAt] [datetime] NULL,
    [BlacklistedReason] [nvarchar](500) NULL
GO

-- Modify LoanManagement table to include approval workflow status
ALTER TABLE [dbo].[LoanManagement] ADD 
    [CurrentStage] [nvarchar](50) DEFAULT 'APPLICATION',  -- APPLICATION, VERIFICATION, ASSESSMENT, APPROVAL, DISBURSEMENT, PAYMENT, COMPLETED
    [DisbursalStatus] [nvarchar](50) DEFAULT 'PENDING',  -- PENDING, RELEASED
    [PaymentStatus] [nvarchar](50) DEFAULT 'ACTIVE'  -- ACTIVE, OVERDUE, PARTIAL, COMPLETED
GO

-- =====================================================================
-- INDEXES FOR PERFORMANCE
-- =====================================================================

CREATE NONCLUSTERED INDEX [IX_LoanApplications_AccountId] 
    ON [dbo].[LoanApplications]([AccountId])
GO

CREATE NONCLUSTERED INDEX [IX_LoanAssessments_ApplicationId] 
    ON [dbo].[LoanAssessments]([ApplicationId])
GO

CREATE NONCLUSTERED INDEX [IX_LoanApprovals_AssessmentId] 
    ON [dbo].[LoanApprovals]([AssessmentId])
GO

CREATE NONCLUSTERED INDEX [IX_LoanPaymentSchedules_LoanId] 
    ON [dbo].[LoanPaymentSchedules]([LoanId], [DueDate])
GO

CREATE NONCLUSTERED INDEX [IX_LoanPayments_LoanId] 
    ON [dbo].[LoanPayments]([LoanId], [PaymentDate])
GO

CREATE NONCLUSTERED INDEX [IX_LoanViolations_LoanId] 
    ON [dbo].[LoanViolations]([LoanId], [IsResolved])
GO

-- =====================================================================
-- CONSTANTS & DEFAULT VALUES
-- =====================================================================

-- Insert system settings for loan processing
INSERT INTO [dbo].[SystemSettings] ([SettingKey], [SettingValue], [Description], [Category], [IsActive], [LastModifiedAt], [LastModifiedBy])
VALUES 
    ('LOAN_DAILY_PENALTY_RATE', '0.05', 'Daily penalty rate as percentage (BPI Standard)', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_MAX_TERM_MONTHS', '360', 'Maximum loan term in months', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_MIN_MONTHLY_INCOME', '15000', 'Minimum monthly income requirement', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_VIOLATION_THRESHOLD', '3', 'Number of violations before blacklist', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_INTEREST_RATE_PERSONAL', '12.5', 'Interest rate for personal loans', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_INTEREST_RATE_HOME', '8.5', 'Interest rate for home loans', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM'),
    ('LOAN_INTEREST_RATE_AUTO', '9.5', 'Interest rate for auto loans', 'LOAN_SETTINGS', 1, GETDATE(), 'SYSTEM')
GO

PRINT 'Loan Process Database Schema Created Successfully!'
