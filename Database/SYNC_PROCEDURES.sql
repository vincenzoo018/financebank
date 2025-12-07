-- =============================================
-- BFAS DATABASE SYNC SCRIPT
-- LOCAL (BFASdatabase) <-> CLOUD (db34283)
-- =============================================
-- 
-- This script provides stored procedures and syntax
-- for synchronizing data between LOCAL and CLOUD databases.
-- 
-- Both databases have IDENTICAL schemas.
-- 
-- =============================================

-- =============================================
-- SECTION 1: SYNC TRACKING TABLE
-- Run this on BOTH LOCAL and CLOUD databases
-- =============================================

-- Create SyncLog table to track synchronization
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SyncLog')
BEGIN
    CREATE TABLE [dbo].[SyncLog](
        [SyncId] [int] IDENTITY(1,1) NOT NULL,
        [TableName] [nvarchar](100) NOT NULL,
        [RecordId] [int] NOT NULL,
        [Operation] [nvarchar](20) NOT NULL,  -- INSERT, UPDATE, DELETE
        [SyncStatus] [nvarchar](20) NOT NULL, -- PENDING, SYNCED, FAILED
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [SyncedAt] [datetime] NULL,
        [ErrorMessage] [nvarchar](500) NULL,
        PRIMARY KEY CLUSTERED ([SyncId] ASC)
    );
END
GO

-- Create SyncMetadata table to track last sync times
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SyncMetadata')
BEGIN
    CREATE TABLE [dbo].[SyncMetadata](
        [MetadataId] [int] IDENTITY(1,1) NOT NULL,
        [TableName] [nvarchar](100) NOT NULL,
        [LastSyncTime] [datetime] NOT NULL,
        [SyncDirection] [nvarchar](20) NOT NULL, -- LOCAL_TO_CLOUD, CLOUD_TO_LOCAL
        [RecordsSynced] [int] NOT NULL DEFAULT 0,
        PRIMARY KEY CLUSTERED ([MetadataId] ASC)
    );
END
GO

-- =============================================
-- SECTION 2: STORED PROCEDURES FOR SYNC OPERATIONS
-- =============================================

-- ---------------------------------------------
-- SP: Sync Users Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncUsers')
    DROP PROCEDURE sp_SyncUsers;
GO

CREATE PROCEDURE [dbo].[sp_SyncUsers]
    @Direction NVARCHAR(20) = 'LOCAL_TO_CLOUD'  -- or 'CLOUD_TO_LOCAL'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This is a template - for actual cloud sync, you'd use linked server
    -- For now, this generates the INSERT/UPDATE statements
    
    IF @Direction = 'LOCAL_TO_CLOUD'
    BEGIN
        -- Generate INSERT statements for new users
        SELECT 
            'IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = ''' + REPLACE(Username, '''', '''''') + ''')
            INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, CreatedAt, LastLoginAt, EmployeeId_FK, TransferPinHash)
            VALUES (''' + REPLACE(Username, '''', '''''') + ''', 
                    ''' + REPLACE(PasswordHash, '''', '''''') + ''', 
                    ''' + Role + ''', 
                    ' + ISNULL('''' + REPLACE(FullName, '''', '''''') + '''', 'NULL') + ', 
                    ' + ISNULL('''' + REPLACE(Email, '''', '''''') + '''', 'NULL') + ', 
                    ' + ISNULL('''' + PhoneNumber + '''', 'NULL') + ', 
                    ' + CAST(IsActive AS VARCHAR(1)) + ', 
                    ''' + CONVERT(VARCHAR, CreatedAt, 120) + ''', 
                    ' + ISNULL('''' + CONVERT(VARCHAR, LastLoginAt, 120) + '''', 'NULL') + ',
                    ' + ISNULL(CAST(EmployeeId_FK AS VARCHAR), 'NULL') + ',
                    ' + ISNULL('''' + REPLACE(CAST(TransferPinHash AS NVARCHAR(MAX)), '''', '''''') + '''', 'NULL') + ');' AS SyncSQL
        FROM Users;
    END
END
GO

-- ---------------------------------------------
-- SP: Sync CustomerAccounts Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncCustomerAccounts')
    DROP PROCEDURE sp_SyncCustomerAccounts;
GO

CREATE PROCEDURE [dbo].[sp_SyncCustomerAccounts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM CustomerAccounts WHERE AccountNumber = ''' + AccountNumber + ''')
        INSERT INTO CustomerAccounts (CustomerId, AccountNumber, Balance, AvailableBalance, Currency, IsActive, CreatedAt, LastTransactionAt, Status)
        VALUES (' + CAST(CustomerId AS VARCHAR) + ', 
                ''' + AccountNumber + ''', 
                ' + CAST(Balance AS VARCHAR) + ', 
                ' + CAST(AvailableBalance AS VARCHAR) + ', 
                ''' + Currency + ''', 
                ' + CAST(IsActive AS VARCHAR(1)) + ', 
                ''' + CONVERT(VARCHAR, CreatedAt, 120) + ''', 
                ' + ISNULL('''' + CONVERT(VARCHAR, LastTransactionAt, 120) + '''', 'NULL') + ',
                ''' + Status + ''');
        ELSE
        UPDATE CustomerAccounts SET 
            Balance = ' + CAST(Balance AS VARCHAR) + ',
            AvailableBalance = ' + CAST(AvailableBalance AS VARCHAR) + ',
            Status = ''' + Status + ''',
            LastTransactionAt = ' + ISNULL('''' + CONVERT(VARCHAR, LastTransactionAt, 120) + '''', 'NULL') + '
        WHERE AccountNumber = ''' + AccountNumber + ''';' AS SyncSQL
    FROM CustomerAccounts;
END
GO

-- ---------------------------------------------
-- SP: Sync CustomerTransactions Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncCustomerTransactions')
    DROP PROCEDURE sp_SyncCustomerTransactions;
GO

CREATE PROCEDURE [dbo].[sp_SyncCustomerTransactions]
    @LastSyncTime DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LastSyncTime IS NULL
        SET @LastSyncTime = '1900-01-01';
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM CustomerTransactions WHERE TransactionNumber = ''' + TransactionNumber + ''')
        INSERT INTO CustomerTransactions (TransactionNumber, AccountId, TransactionType, Amount, Fee, Status, Description, Reference, ToAccountId, ToAccountName, BillerName, BillerAccountNumber, CreatedAt, ProcessedAt, ProcessedByEmployeeId, Tax, BalanceAfter, Category, SenderName, SenderAccount, RecipientName, RecipientAccount)
        VALUES (''' + TransactionNumber + ''', 
                ' + CAST(AccountId AS VARCHAR) + ', 
                ''' + TransactionType + ''', 
                ' + CAST(Amount AS VARCHAR) + ', 
                ' + CAST(Fee AS VARCHAR) + ', 
                ''' + Status + ''', 
                ' + ISNULL('''' + REPLACE(Description, '''', '''''') + '''', 'NULL') + ', 
                ' + ISNULL('''' + Reference + '''', 'NULL') + ', 
                ' + ISNULL(CAST(ToAccountId AS VARCHAR), 'NULL') + ', 
                ' + ISNULL('''' + REPLACE(ToAccountName, '''', '''''') + '''', 'NULL') + ', 
                ' + ISNULL('''' + REPLACE(BillerName, '''', '''''') + '''', 'NULL') + ', 
                ' + ISNULL('''' + BillerAccountNumber + '''', 'NULL') + ', 
                ''' + CONVERT(VARCHAR, CreatedAt, 120) + ''', 
                ' + ISNULL('''' + CONVERT(VARCHAR, ProcessedAt, 120) + '''', 'NULL') + ',
                ' + ISNULL(CAST(ProcessedByEmployeeId AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(Tax AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(BalanceAfter AS VARCHAR), 'NULL') + ',
                ' + ISNULL('''' + Category + '''', 'NULL') + ',
                ' + ISNULL('''' + REPLACE(SenderName, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + SenderAccount + '''', 'NULL') + ',
                ' + ISNULL('''' + REPLACE(RecipientName, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + RecipientAccount + '''', 'NULL') + ');' AS SyncSQL
    FROM CustomerTransactions
    WHERE CreatedAt > @LastSyncTime;
END
GO

-- ---------------------------------------------
-- SP: Sync LoanApplications Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncLoanApplications')
    DROP PROCEDURE sp_SyncLoanApplications;
GO

CREATE PROCEDURE [dbo].[sp_SyncLoanApplications]
    @LastSyncTime DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LastSyncTime IS NULL
        SET @LastSyncTime = '1900-01-01';
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM LoanApplications WHERE ApplicationNumber = ''' + ApplicationNumber + ''')
        INSERT INTO LoanApplications (ApplicationNumber, AccountId, LoanType, RequestedAmount, Purpose, EmploymentStatus, MonthlyIncome, ExistingLoans, Documents, Status, ApplicationDate, SubmittedBy, RejectionReason, RejectedAt, RejectedBy)
        VALUES (''' + ApplicationNumber + ''', 
                ' + CAST(AccountId AS VARCHAR) + ', 
                ''' + LoanType + ''', 
                ' + CAST(RequestedAmount AS VARCHAR) + ', 
                ' + ISNULL('''' + REPLACE(Purpose, '''', '''''') + '''', 'NULL') + ', 
                ' + ISNULL('''' + EmploymentStatus + '''', 'NULL') + ', 
                ' + ISNULL(CAST(MonthlyIncome AS VARCHAR), 'NULL') + ', 
                ' + ISNULL(CAST(ExistingLoans AS VARCHAR), 'NULL') + ', 
                ' + ISNULL('''' + REPLACE(CAST(Documents AS NVARCHAR(MAX)), '''', '''''') + '''', 'NULL') + ', 
                ''' + Status + ''', 
                ''' + CONVERT(VARCHAR, ApplicationDate, 120) + ''', 
                ''' + SubmittedBy + ''', 
                ' + ISNULL('''' + REPLACE(RejectionReason, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + CONVERT(VARCHAR, RejectedAt, 120) + '''', 'NULL') + ',
                ' + ISNULL('''' + RejectedBy + '''', 'NULL') + ');
        ELSE
        UPDATE LoanApplications SET 
            Status = ''' + Status + ''',
            RejectionReason = ' + ISNULL('''' + REPLACE(RejectionReason, '''', '''''') + '''', 'NULL') + ',
            RejectedAt = ' + ISNULL('''' + CONVERT(VARCHAR, RejectedAt, 120) + '''', 'NULL') + ',
            RejectedBy = ' + ISNULL('''' + RejectedBy + '''', 'NULL') + '
        WHERE ApplicationNumber = ''' + ApplicationNumber + ''';' AS SyncSQL
    FROM LoanApplications
    WHERE ApplicationDate > @LastSyncTime;
END
GO

-- ---------------------------------------------
-- SP: Sync LoanAssessments Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncLoanAssessments')
    DROP PROCEDURE sp_SyncLoanAssessments;
GO

CREATE PROCEDURE [dbo].[sp_SyncLoanAssessments]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM LoanAssessments WHERE AssessmentId = ' + CAST(AssessmentId AS VARCHAR) + ')
        SET IDENTITY_INSERT LoanAssessments ON;
        INSERT INTO LoanAssessments (AssessmentId, ApplicationId, AccountId, LoanAmount, InterestRate, TermMonths, ComputedMonthlyPayment, TotalPayable, AssessmentDate, AssessmentStatus, AssessedBy, Remarks, RejectionReason, RejectedAt, RejectedBy)
        VALUES (' + CAST(AssessmentId AS VARCHAR) + ', 
                ' + CAST(ApplicationId AS VARCHAR) + ', 
                ' + CAST(AccountId AS VARCHAR) + ', 
                ' + CAST(LoanAmount AS VARCHAR) + ', 
                ' + CAST(InterestRate AS VARCHAR) + ', 
                ' + CAST(TermMonths AS VARCHAR) + ', 
                ' + CAST(ComputedMonthlyPayment AS VARCHAR) + ', 
                ' + CAST(TotalPayable AS VARCHAR) + ', 
                ''' + CONVERT(VARCHAR, AssessmentDate, 120) + ''', 
                ''' + AssessmentStatus + ''', 
                ''' + AssessedBy + ''', 
                ' + ISNULL('''' + REPLACE(Remarks, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + REPLACE(RejectionReason, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + CONVERT(VARCHAR, RejectedAt, 120) + '''', 'NULL') + ',
                ' + ISNULL('''' + RejectedBy + '''', 'NULL') + ');
        SET IDENTITY_INSERT LoanAssessments OFF;' AS SyncSQL
    FROM LoanAssessments;
END
GO

-- ---------------------------------------------
-- SP: Sync LoanApprovals Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncLoanApprovals')
    DROP PROCEDURE sp_SyncLoanApprovals;
GO

CREATE PROCEDURE [dbo].[sp_SyncLoanApprovals]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM LoanApprovals WHERE ApprovalId = ' + CAST(ApprovalId AS VARCHAR) + ')
        SET IDENTITY_INSERT LoanApprovals ON;
        INSERT INTO LoanApprovals (ApprovalId, AssessmentId, ApplicationId, AccountId, ApprovalStatus, ApprovalDate, ApprovedBy, ApprovedAmount, ApprovedInterestRate, ApprovedTermMonths, SpecialConditions, DeclinationReason)
        VALUES (' + CAST(ApprovalId AS VARCHAR) + ', 
                ' + CAST(AssessmentId AS VARCHAR) + ', 
                ' + CAST(ApplicationId AS VARCHAR) + ', 
                ' + CAST(AccountId AS VARCHAR) + ', 
                ''' + ApprovalStatus + ''', 
                ''' + CONVERT(VARCHAR, ApprovalDate, 120) + ''', 
                ''' + ApprovedBy + ''', 
                ' + ISNULL(CAST(ApprovedAmount AS VARCHAR), 'NULL') + ', 
                ' + ISNULL(CAST(ApprovedInterestRate AS VARCHAR), 'NULL') + ', 
                ' + ISNULL(CAST(ApprovedTermMonths AS VARCHAR), 'NULL') + ', 
                ' + ISNULL('''' + REPLACE(SpecialConditions, '''', '''''') + '''', 'NULL') + ',
                ' + ISNULL('''' + REPLACE(DeclinationReason, '''', '''''') + '''', 'NULL') + ');
        SET IDENTITY_INSERT LoanApprovals OFF;' AS SyncSQL
    FROM LoanApprovals;
END
GO

-- ---------------------------------------------
-- SP: Sync CustomerLoans Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncCustomerLoans')
    DROP PROCEDURE sp_SyncCustomerLoans;
GO

CREATE PROCEDURE [dbo].[sp_SyncCustomerLoans]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM CustomerLoans WHERE LoanNumber = ''' + LoanNumber + ''')
        INSERT INTO CustomerLoans (AccountId, LoanNumber, LoanType, LoanAmount, OutstandingBalance, InterestRate, TermMonths, MonthlyPayment, StartDate, EndDate, NextDueDate, Status, Purpose, CreatedAt, ApplicationId, AssessmentId, ApprovalId, DisbursalId, CumulativeLateDays, TotalPenalties, LastPaymentDate, IsBlacklisted, BlacklistedAt, BlacklistedReason)
        VALUES (' + CAST(AccountId AS VARCHAR) + ', 
                ''' + LoanNumber + ''', 
                ''' + LoanType + ''', 
                ' + CAST(LoanAmount AS VARCHAR) + ', 
                ' + CAST(OutstandingBalance AS VARCHAR) + ', 
                ' + CAST(InterestRate AS VARCHAR) + ', 
                ' + CAST(TermMonths AS VARCHAR) + ', 
                ' + CAST(MonthlyPayment AS VARCHAR) + ', 
                ''' + CONVERT(VARCHAR, StartDate, 120) + ''', 
                ''' + CONVERT(VARCHAR, EndDate, 120) + ''', 
                ' + ISNULL('''' + CONVERT(VARCHAR, NextDueDate, 120) + '''', 'NULL') + ', 
                ''' + Status + ''', 
                ' + ISNULL('''' + REPLACE(Purpose, '''', '''''') + '''', 'NULL') + ', 
                ''' + CONVERT(VARCHAR, CreatedAt, 120) + ''',
                ' + ISNULL(CAST(ApplicationId AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(AssessmentId AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(ApprovalId AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(DisbursalId AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(CumulativeLateDays AS VARCHAR), 'NULL') + ',
                ' + ISNULL(CAST(TotalPenalties AS VARCHAR), 'NULL') + ',
                ' + ISNULL('''' + CONVERT(VARCHAR, LastPaymentDate, 120) + '''', 'NULL') + ',
                ' + ISNULL(CAST(IsBlacklisted AS VARCHAR), 'NULL') + ',
                ' + ISNULL('''' + CONVERT(VARCHAR, BlacklistedAt, 120) + '''', 'NULL') + ',
                ' + ISNULL('''' + REPLACE(BlacklistedReason, '''', '''''') + '''', 'NULL') + ');
        ELSE
        UPDATE CustomerLoans SET 
            OutstandingBalance = ' + CAST(OutstandingBalance AS VARCHAR) + ',
            Status = ''' + Status + ''',
            NextDueDate = ' + ISNULL('''' + CONVERT(VARCHAR, NextDueDate, 120) + '''', 'NULL') + ',
            CumulativeLateDays = ' + ISNULL(CAST(CumulativeLateDays AS VARCHAR), 'NULL') + ',
            TotalPenalties = ' + ISNULL(CAST(TotalPenalties AS VARCHAR), 'NULL') + ',
            LastPaymentDate = ' + ISNULL('''' + CONVERT(VARCHAR, LastPaymentDate, 120) + '''', 'NULL') + '
        WHERE LoanNumber = ''' + LoanNumber + ''';' AS SyncSQL
    FROM CustomerLoans;
END
GO

-- ---------------------------------------------
-- SP: Sync EmployeeAccounts Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncEmployeeAccounts')
    DROP PROCEDURE sp_SyncEmployeeAccounts;
GO

CREATE PROCEDURE [dbo].[sp_SyncEmployeeAccounts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'IF NOT EXISTS (SELECT 1 FROM EmployeeAccounts WHERE EmployeeNumber = ''' + EmployeeNumber + ''')
        INSERT INTO EmployeeAccounts (UserID, FirstName, LastName, EmployeeNumber, Department, Position, Specialization, DateOfBirth, HireDate, EmploymentStatus, Address, City, State, ZipCode, DirectSupervisorID, WorkPhone, Office, DateModified)
        VALUES (' + CAST(UserID AS VARCHAR) + ', 
                ''' + REPLACE(FirstName, '''', '''''') + ''', 
                ''' + REPLACE(LastName, '''', '''''') + ''', 
                ''' + EmployeeNumber + ''', 
                ''' + Department + ''', 
                ''' + Position + ''', 
                ' + ISNULL('''' + Specialization + '''', 'NULL') + ', 
                ' + ISNULL('''' + CONVERT(VARCHAR, DateOfBirth, 120) + '''', 'NULL') + ', 
                ''' + CONVERT(VARCHAR, HireDate, 120) + ''', 
                ''' + EmploymentStatus + ''', 
                ' + ISNULL('''' + REPLACE(Address, '''', '''''') + '''', 'NULL') + ', 
                ' + ISNULL('''' + City + '''', 'NULL') + ', 
                ' + ISNULL('''' + State + '''', 'NULL') + ', 
                ' + ISNULL('''' + ZipCode + '''', 'NULL') + ',
                ' + ISNULL(CAST(DirectSupervisorID AS VARCHAR), 'NULL') + ',
                ' + ISNULL('''' + WorkPhone + '''', 'NULL') + ',
                ' + ISNULL('''' + Office + '''', 'NULL') + ',
                ''' + CONVERT(VARCHAR, DateModified, 120) + ''');
        ELSE
        UPDATE EmployeeAccounts SET 
            Position = ''' + Position + ''',
            Department = ''' + Department + ''',
            EmploymentStatus = ''' + EmploymentStatus + ''',
            DateModified = ''' + CONVERT(VARCHAR, DateModified, 120) + '''
        WHERE EmployeeNumber = ''' + EmployeeNumber + ''';' AS SyncSQL
    FROM EmployeeAccounts;
END
GO

-- ---------------------------------------------
-- SP: Sync LoginHistory Table
-- ---------------------------------------------
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_SyncLoginHistory')
    DROP PROCEDURE sp_SyncLoginHistory;
GO

CREATE PROCEDURE [dbo].[sp_SyncLoginHistory]
    @LastSyncTime DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LastSyncTime IS NULL
        SET @LastSyncTime = DATEADD(DAY, -7, GETDATE()); -- Only sync last 7 days
    
    SELECT 
        'INSERT INTO LoginHistory (UserId, LoginTime, LogoutTime, IpAddress, UserAgent, LoginStatus, FailureReason)
        VALUES (' + CAST(UserId AS VARCHAR) + ', 
                ''' + CONVERT(VARCHAR, LoginTime, 120) + ''', 
                ' + ISNULL('''' + CONVERT(VARCHAR, LogoutTime, 120) + '''', 'NULL') + ', 
                ' + ISNULL('''' + IpAddress + '''', 'NULL') + ', 
                ' + ISNULL('''' + REPLACE(UserAgent, '''', '''''') + '''', 'NULL') + ', 
                ''' + LoginStatus + ''', 
                ' + ISNULL('''' + REPLACE(FailureReason, '''', '''''') + '''', 'NULL') + ');' AS SyncSQL
    FROM LoginHistory
    WHERE LoginTime > @LastSyncTime;
END
GO

-- =============================================
-- SECTION 3: MASTER SYNC PROCEDURE
-- =============================================

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_FullDatabaseSync')
    DROP PROCEDURE sp_FullDatabaseSync;
GO

CREATE PROCEDURE [dbo].[sp_FullDatabaseSync]
    @Direction NVARCHAR(20) = 'LOCAL_TO_CLOUD',
    @LastSyncTime DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    PRINT '=========================================='
    PRINT 'Starting Full Database Sync'
    PRINT 'Direction: ' + @Direction
    PRINT 'Last Sync Time: ' + ISNULL(CONVERT(VARCHAR, @LastSyncTime, 120), 'FULL SYNC')
    PRINT '=========================================='
    
    -- Sync in proper order (respecting foreign keys)
    PRINT ''
    PRINT '-- STEP 1: USERS --'
    EXEC sp_SyncUsers @Direction;
    
    PRINT ''
    PRINT '-- STEP 2: EMPLOYEE ACCOUNTS --'
    EXEC sp_SyncEmployeeAccounts;
    
    PRINT ''
    PRINT '-- STEP 3: CUSTOMER ACCOUNTS --'
    EXEC sp_SyncCustomerAccounts;
    
    PRINT ''
    PRINT '-- STEP 4: CUSTOMER TRANSACTIONS --'
    EXEC sp_SyncCustomerTransactions @LastSyncTime;
    
    PRINT ''
    PRINT '-- STEP 5: LOAN APPLICATIONS --'
    EXEC sp_SyncLoanApplications @LastSyncTime;
    
    PRINT ''
    PRINT '-- STEP 6: LOAN ASSESSMENTS --'
    EXEC sp_SyncLoanAssessments;
    
    PRINT ''
    PRINT '-- STEP 7: LOAN APPROVALS --'
    EXEC sp_SyncLoanApprovals;
    
    PRINT ''
    PRINT '-- STEP 8: CUSTOMER LOANS --'
    EXEC sp_SyncCustomerLoans;
    
    PRINT ''
    PRINT '-- STEP 9: LOGIN HISTORY --'
    EXEC sp_SyncLoginHistory @LastSyncTime;
    
    -- Update sync metadata
    INSERT INTO SyncMetadata (TableName, LastSyncTime, SyncDirection, RecordsSynced)
    VALUES ('ALL_TABLES', GETDATE(), @Direction, 0);
    
    PRINT ''
    PRINT '=========================================='
    PRINT 'Sync Complete: ' + CONVERT(VARCHAR, GETDATE(), 120)
    PRINT '=========================================='
END
GO

-- =============================================
-- SECTION 4: QUICK SYNC COMMANDS
-- =============================================

-- Example usage:
-- Run on LOCAL database to generate INSERT statements for CLOUD:
-- EXEC sp_FullDatabaseSync 'LOCAL_TO_CLOUD', NULL

-- Run on CLOUD database to generate INSERT statements for LOCAL:
-- EXEC sp_FullDatabaseSync 'CLOUD_TO_LOCAL', NULL

-- Incremental sync (only changes since last sync):
-- EXEC sp_FullDatabaseSync 'LOCAL_TO_CLOUD', '2025-12-01 00:00:00'

PRINT 'Sync procedures installed successfully!'
PRINT ''
PRINT 'Usage:'
PRINT '  EXEC sp_FullDatabaseSync ''LOCAL_TO_CLOUD'', NULL  -- Full sync'
PRINT '  EXEC sp_FullDatabaseSync ''LOCAL_TO_CLOUD'', ''2025-12-01''  -- Incremental'
GO
