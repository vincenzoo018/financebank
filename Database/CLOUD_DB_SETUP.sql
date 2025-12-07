-- =============================================
-- CLOUD DATABASE SETUP SCRIPT
-- For MonsterASP: db34283.public.databaseasp.net
-- Database: db34283
-- =============================================
-- 
-- HOW TO USE:
-- 1. Connect to the cloud server in SSMS using:
--    Server: db34283.public.databaseasp.net,1433
--    Login: db34283
--    Password: Zx6=2+fXCm8!
--    
-- 2. Make sure you're connected to database: db34283
-- 3. Run this entire script
-- =============================================

-- Note: DO NOT use "USE BFASdatabase" - the cloud database is named "db34283"
-- The connection should already be to db34283

-- =============================================
-- 1. USERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(100) NULL,
        Email NVARCHAR(100) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        LastLoginAt DATETIME NULL,
        Department NVARCHAR(50) NULL,
        EmployeeId NVARCHAR(50) NULL,
        TransferPinHash NVARCHAR(255) NULL,
        ProfilePicture NVARCHAR(MAX) NULL,
        ProfilePictureType NVARCHAR(50) NULL
    );
    
    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_Role ON Users(Role);
    PRINT 'Users table created successfully';
END
ELSE
    PRINT 'Users table already exists';
GO

-- =============================================
-- 2. EMPLOYEES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Department NVARCHAR(50) NULL,
        Position NVARCHAR(50) NULL,
        HireDate DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        UserId INT NULL,
        ValidIdType NVARCHAR(50) NULL,
        ValidIdNumber NVARCHAR(100) NULL,
        ValidIdImage NVARCHAR(MAX) NULL
    );
    PRINT 'Employees table created successfully';
END
GO

-- =============================================
-- 3. LOGIN HISTORY TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginHistory')
BEGIN
    CREATE TABLE LoginHistory (
        LoginId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        LoginTime DATETIME NOT NULL DEFAULT GETDATE(),
        LogoutTime DATETIME NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(255) NULL,
        LoginStatus NVARCHAR(20) NOT NULL DEFAULT 'Success',
        FailureReason NVARCHAR(255) NULL
    );
    PRINT 'LoginHistory table created successfully';
END
GO

-- =============================================
-- 4. USER SESSIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSessions')
BEGIN
    CREATE TABLE UserSessions (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        SessionToken NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(255) NULL
    );
    PRINT 'UserSessions table created successfully';
END
GO

-- =============================================
-- 5. ROLE PERMISSIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        PermissionId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL,
        ModuleName NVARCHAR(100) NOT NULL,
        Permission NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
    PRINT 'RolePermissions table created successfully';
END
GO

-- =============================================
-- 6. CUSTOMER ACCOUNTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerAccounts')
BEGIN
    CREATE TABLE CustomerAccounts (
        AccountId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
        AccountName NVARCHAR(100) NULL,
        AccountType NVARCHAR(50) NOT NULL DEFAULT 'Savings',
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
        AvailableBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'PHP',
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL
    );
    PRINT 'CustomerAccounts table created successfully';
END
GO

-- =============================================
-- 7. CUSTOMERS TABLE (for Customer details)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
        AccountId INT NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Address NVARCHAR(500) NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(10) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Customers table created successfully';
END
GO

-- =============================================
-- 8. CUSTOMER TRANSACTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerTransactions')
BEGIN
    CREATE TABLE CustomerTransactions (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        AccountId INT NOT NULL,
        TransactionNumber NVARCHAR(50) NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL,
        Description NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Completed',
        ProcessedBy NVARCHAR(100) NULL,
        ProcessedByEmployeeId INT NULL,
        ProcessedByEmployeeName NVARCHAR(100) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ProcessedAt DATETIME NULL,
        ReferenceNumber NVARCHAR(100) NULL
    );
    PRINT 'CustomerTransactions table created successfully';
END
GO

-- =============================================
-- 9. FUND TRANSFERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FundTransfers')
BEGIN
    CREATE TABLE FundTransfers (
        TransferId INT IDENTITY(1,1) PRIMARY KEY,
        TransferNumber NVARCHAR(50) NOT NULL,
        FromAccountId INT NOT NULL,
        ToAccountNumber NVARCHAR(50) NOT NULL,
        ToAccountName NVARCHAR(100) NULL,
        ToBankName NVARCHAR(100) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Fee DECIMAL(18,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        ProcessedBy NVARCHAR(100) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ProcessedAt DATETIME NULL
    );
    PRINT 'FundTransfers table created successfully';
END
GO

-- =============================================
-- 10. LOAN APPLICATIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanApplications')
BEGIN
    CREATE TABLE LoanApplications (
        ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationNumber NVARCHAR(50) NOT NULL,
        AccountId INT NOT NULL,
        LoanType NVARCHAR(50) NOT NULL,
        RequestedAmount DECIMAL(18,2) NOT NULL,
        Purpose NVARCHAR(500) NULL,
        EmploymentStatus NVARCHAR(50) NULL,
        MonthlyIncome DECIMAL(18,2) NULL,
        ExistingLoans INT NOT NULL DEFAULT 0,
        Documents NVARCHAR(MAX) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'SUBMITTED',
        ApplicationDate DATETIME NOT NULL DEFAULT GETDATE(),
        SubmittedBy NVARCHAR(50) NOT NULL,
        RejectionReason NVARCHAR(500) NULL,
        RejectedAt DATETIME NULL,
        RejectedBy NVARCHAR(50) NULL
    );
    PRINT 'LoanApplications table created successfully';
END
GO

-- =============================================
-- 11. LOAN ASSESSMENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanAssessments')
BEGIN
    CREATE TABLE LoanAssessments (
        AssessmentId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        AccountId INT NOT NULL,
        LoanAmount DECIMAL(18,2) NOT NULL,
        InterestRate DECIMAL(5,2) NOT NULL,
        TermMonths INT NOT NULL,
        ComputedMonthlyPayment DECIMAL(18,2) NOT NULL,
        TotalPayable DECIMAL(18,2) NOT NULL,
        AssessmentDate DATETIME NOT NULL DEFAULT GETDATE(),
        AssessmentStatus NVARCHAR(50) NOT NULL DEFAULT 'ASSESSED',
        AssessedBy NVARCHAR(50) NOT NULL,
        Remarks NVARCHAR(500) NULL
    );
    PRINT 'LoanAssessments table created successfully';
END
GO

-- =============================================
-- 12. LOAN APPROVALS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanApprovals')
BEGIN
    CREATE TABLE LoanApprovals (
        ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
        AssessmentId INT NOT NULL,
        ApplicationId INT NOT NULL,
        AccountId INT NOT NULL,
        ApprovalStatus NVARCHAR(50) NOT NULL,
        ApprovalDate DATETIME NOT NULL DEFAULT GETDATE(),
        ApprovedBy NVARCHAR(50) NOT NULL,
        ApprovedAmount DECIMAL(18,2) NULL,
        ApprovedInterestRate DECIMAL(5,2) NULL,
        ApprovedTermMonths INT NULL,
        SpecialConditions NVARCHAR(500) NULL,
        DeclinationReason NVARCHAR(500) NULL
    );
    PRINT 'LoanApprovals table created successfully';
END
GO

-- =============================================
-- 13. LOANS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Loans')
BEGIN
    CREATE TABLE Loans (
        LoanId INT IDENTITY(1,1) PRIMARY KEY,
        LoanNumber NVARCHAR(50) NOT NULL,
        ApprovalId INT NULL,
        AccountId INT NOT NULL,
        LoanType NVARCHAR(50) NOT NULL,
        LoanAmount DECIMAL(18,2) NOT NULL,
        OutstandingBalance DECIMAL(18,2) NOT NULL,
        InterestRate DECIMAL(5,2) NOT NULL,
        TermMonths INT NOT NULL,
        MonthlyPayment DECIMAL(18,2) NOT NULL,
        StartDate DATETIME NOT NULL,
        EndDate DATETIME NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Loans table created successfully';
END
GO

-- =============================================
-- 14. LOAN DISBURSALS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanDisbursals')
BEGIN
    CREATE TABLE LoanDisbursals (
        DisbursalId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        ApprovalId INT NOT NULL,
        DisbursalAmount DECIMAL(18,2) NOT NULL,
        DisbursalDate DATETIME NOT NULL DEFAULT GETDATE(),
        DisbursalStatus NVARCHAR(50) NOT NULL DEFAULT 'RELEASED',
        DisbursedTo NVARCHAR(100) NOT NULL,
        ProcessedBy NVARCHAR(50) NOT NULL,
        Reference NVARCHAR(100) NULL
    );
    PRINT 'LoanDisbursals table created successfully';
END
GO

-- =============================================
-- 15. LOAN PAYMENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanPayments')
BEGIN
    CREATE TABLE LoanPayments (
        PaymentId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        PaymentAmount DECIMAL(18,2) NOT NULL,
        PrincipalAmount DECIMAL(18,2) NOT NULL,
        InterestAmount DECIMAL(18,2) NOT NULL,
        PenaltyAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
        PaymentMethod NVARCHAR(50) NOT NULL,
        PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED',
        ProcessedBy NVARCHAR(50) NOT NULL,
        ReferenceNumber NVARCHAR(100) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL
    );
    PRINT 'LoanPayments table created successfully';
END
GO

-- =============================================
-- 16. LOAN INVOICES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanInvoices')
BEGIN
    CREATE TABLE LoanInvoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        InvoiceType NVARCHAR(50) NOT NULL,
        LoanId INT NULL,
        DisbursalId INT NULL,
        PaymentId INT NULL,
        AccountId INT NOT NULL,
        CustomerName NVARCHAR(100) NOT NULL,
        PrincipalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        InterestAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PenaltyAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL,
        InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
        ProcessedBy NVARCHAR(50) NOT NULL,
        Reference NVARCHAR(100) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL
    );
    PRINT 'LoanInvoices table created successfully';
END
GO

-- =============================================
-- 17. LOAN TRANSACTION HISTORY TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanTransactionHistory')
BEGIN
    CREATE TABLE LoanTransactionHistory (
        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
        AccountId INT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(500) NULL,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        ProcessedBy NVARCHAR(50) NULL,
        ApplicationId INT NULL,
        ApprovalId INT NULL,
        LoanId INT NULL,
        Reference NVARCHAR(100) NULL,
        Status NVARCHAR(50) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL
    );
    PRINT 'LoanTransactionHistory table created successfully';
END
GO

-- =============================================
-- 18. AUDIT LOGS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        Username NVARCHAR(50) NULL,
        Action NVARCHAR(100) NOT NULL,
        TableName NVARCHAR(100) NULL,
        RecordId INT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(50) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'AuditLogs table created successfully';
END
GO

-- =============================================
-- 19. INVOICES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        AccountId INT NOT NULL,
        TransactionId INT NULL,
        InvoiceType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Fee DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Issued',
        IssuedBy NVARCHAR(100) NULL,
        IssuedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CustomerName NVARCHAR(100) NULL,
        AccountNumber NVARCHAR(50) NULL,
        BalanceBefore DECIMAL(18,2) NULL,
        BalanceAfter DECIMAL(18,2) NULL,
        TransactionReference NVARCHAR(100) NULL,
        PaymentMethod NVARCHAR(50) NULL
    );
    PRINT 'Invoices table created successfully';
END
GO

-- =============================================
-- 20. CARDS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cards')
BEGIN
    CREATE TABLE Cards (
        CardId INT IDENTITY(1,1) PRIMARY KEY,
        AccountId INT NOT NULL,
        CardNumber NVARCHAR(20) NOT NULL,
        CardType NVARCHAR(50) NOT NULL,
        ExpiryDate DATE NOT NULL,
        CVV NVARCHAR(5) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        CreditLimit DECIMAL(18,2) NULL,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Cards table created successfully';
END
GO

-- =============================================
-- INSERT DEFAULT ADMIN USER
-- =============================================
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, IsActive, Department)
    VALUES (
        'admin',
        '$2a$11$rBNr.YkvxEPmMQZQg3qQKuGxKzx7nwLbJkYIqWkQ7HnQwvJKqVtZa', -- BCrypt hash for 'admin123'
        'SuperAdmin',
        'System Administrator',
        'admin@bfas.com',
        1,
        'IT Department'
    );
    PRINT 'Default admin user created';
END
GO

-- =============================================
-- VERIFY TABLES CREATED
-- =============================================
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

PRINT '========================================';
PRINT 'Cloud database setup completed!';
PRINT 'Tables have been created in db34283';
PRINT '========================================';
