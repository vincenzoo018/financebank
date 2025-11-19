-- =============================================
-- COMPLETE MIGRATION SCRIPT
-- Database: BFASdatabase
-- Purpose: Restructure Users/Employees, Auto-generate Account Numbers, Create Invoices
-- =============================================

USE BFASdatabase;
GO

PRINT '========================================================';
PRINT 'STARTING COMPLETE MIGRATION';
PRINT '========================================================';
GO

-- =============================================
-- STEP 1: CREATE EMPLOYEE TABLE
-- =============================================
PRINT 'STEP 1: Creating Employee table...';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        EmployeeId NVARCHAR(50) PRIMARY KEY,
        UserId INT NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Department NVARCHAR(100) NOT NULL,
        Position NVARCHAR(100) NULL,
        HireDate DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        
        CONSTRAINT FK_Employees_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Employees_UserId ON Employees(UserId);
    CREATE INDEX IX_Employees_Department ON Employees(Department);
    CREATE INDEX IX_Employees_IsActive ON Employees(IsActive);
    
    PRINT '✓ Employee table created successfully';
END
ELSE
BEGIN
    PRINT '✓ Employee table already exists';
END
GO

-- =============================================
-- STEP 2: ADD EmployeeId_FK TO USERS TABLE
-- =============================================
PRINT 'STEP 2: Adding EmployeeId_FK to Users table...';
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmployeeId_FK')
BEGIN
    ALTER TABLE Users
    ADD EmployeeId_FK NVARCHAR(50) NULL;
    
    PRINT '✓ EmployeeId_FK column added to Users table';
END
ELSE
BEGIN
    PRINT '✓ EmployeeId_FK column already exists';
END
GO

-- =============================================
-- STEP 3: ADD FK CONSTRAINT TO USERS
-- =============================================
PRINT 'STEP 3: Adding foreign key constraint...';
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'Users' AND CONSTRAINT_NAME = 'FK_Users_Employees')
BEGIN
    ALTER TABLE Users
    ADD CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId_FK) REFERENCES Employees(EmployeeId) ON DELETE SET NULL;
    
    PRINT '✓ Foreign key constraint added';
END
ELSE
BEGIN
    PRINT '✓ Foreign key constraint already exists';
END
GO

-- =============================================
-- STEP 4: MIGRATE EXISTING EMPLOYEE DATA
-- =============================================
PRINT 'STEP 4: Migrating existing employee data...';
GO

-- Only migrate if there are employees without Employee records
IF EXISTS (SELECT 1 FROM Users WHERE Role IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Teller') AND EmployeeId IS NOT NULL AND UserId NOT IN (SELECT UserId FROM Employees))
BEGIN
    INSERT INTO Employees (EmployeeId, UserId, FirstName, LastName, Department, IsActive, CreatedAt)
    SELECT 
        EmployeeId,
        UserId,
        SUBSTRING(FullName, 1, ISNULL(NULLIF(CHARINDEX(' ', FullName) - 1, -1), LEN(FullName))) AS FirstName,
        CASE 
            WHEN CHARINDEX(' ', FullName) > 0 THEN SUBSTRING(FullName, CHARINDEX(' ', FullName) + 1, LEN(FullName))
            ELSE ''
        END AS LastName,
        ISNULL(Department, 'General') AS Department,
        IsActive,
        CreatedAt
    FROM Users
    WHERE Role IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Teller')
      AND EmployeeId IS NOT NULL
      AND UserId NOT IN (SELECT UserId FROM Employees);
    
    PRINT '✓ Employee data migrated successfully';
END
ELSE
BEGIN
    PRINT '✓ No new employee data to migrate';
END
GO

-- =============================================
-- STEP 5: CREATE INVOICES TABLE
-- =============================================
PRINT 'STEP 5: Creating Invoices table...';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
        InvoiceType NVARCHAR(50) NOT NULL, -- Deposit, Withdrawal
        AccountId INT NOT NULL,
        TransactionId INT NOT NULL,
        CustomerName NVARCHAR(100) NOT NULL,
        AccountNumber NVARCHAR(50) NOT NULL,
        BalanceBefore DECIMAL(18,2) NOT NULL,
        TransactionAmount DECIMAL(18,2) NOT NULL,
        BalanceAfter DECIMAL(18,2) NOT NULL,
        TransactionMethod NVARCHAR(50) NOT NULL,
        Reference NVARCHAR(100) NULL,
        Notes NVARCHAR(500) NULL,
        ProcessedBy NVARCHAR(100) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Completed',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        PrintedAt DATETIME NULL,
        DownloadedAt DATETIME NULL,
        
        CONSTRAINT FK_Invoices_CustomerAccounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId) ON DELETE RESTRICT,
        CONSTRAINT FK_Invoices_CustomerTransactions FOREIGN KEY (TransactionId) REFERENCES CustomerTransactions(TransactionId) ON DELETE RESTRICT
    );
    
    CREATE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
    CREATE INDEX IX_Invoices_AccountId ON Invoices(AccountId);
    CREATE INDEX IX_Invoices_TransactionId ON Invoices(TransactionId);
    CREATE INDEX IX_Invoices_CreatedAt ON Invoices(CreatedAt);
    
    PRINT '✓ Invoices table created successfully';
END
ELSE
BEGIN
    PRINT '✓ Invoices table already exists';
END
GO

-- =============================================
-- STEP 6: AUTO-GENERATE ACCOUNT NUMBERS
-- =============================================
PRINT 'STEP 6: Auto-generating account numbers...';
GO

UPDATE CustomerAccounts
SET AccountNumber = 'ACC-' + FORMAT(GETDATE(), 'yyyy') + '-' + RIGHT('00000' + CAST(AccountId AS NVARCHAR(5)), 5)
WHERE AccountNumber IS NULL OR AccountNumber = '' OR AccountNumber LIKE 'ACC-0000%';

PRINT '✓ Account numbers auto-generated';
GO

-- =============================================
-- STEP 7: ENSURE CUSTOMERACCOUNTS HAS FK TO USERS
-- =============================================
PRINT 'STEP 7: Verifying CustomerAccounts foreign key...';
GO

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'CustomerAccounts' AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME = 'FK_CustomerAccounts_Users'
)
BEGIN
    -- Check if FK already exists with different name
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
        WHERE TABLE_NAME = 'CustomerAccounts' AND REFERENCED_TABLE_NAME = 'Users'
    )
    BEGIN
        ALTER TABLE CustomerAccounts
        ADD CONSTRAINT FK_CustomerAccounts_Users FOREIGN KEY (CustomerId) REFERENCES Users(UserId) ON DELETE CASCADE;
        
        PRINT '✓ Foreign key added to CustomerAccounts';
    END
    ELSE
    BEGIN
        PRINT '✓ Foreign key already exists';
    END
END
ELSE
BEGIN
    PRINT '✓ Foreign key already exists';
END
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================
PRINT '';
PRINT '========================================================';
PRINT 'MIGRATION COMPLETE - VERIFICATION';
PRINT '========================================================';
PRINT '';

PRINT 'Employees Table:';
SELECT 
    EmployeeId,
    UserId,
    FirstName,
    LastName,
    Department,
    Position,
    IsActive,
    HireDate
FROM Employees
ORDER BY EmployeeId;
GO

PRINT '';
PRINT 'Users Table (Updated):';
SELECT 
    UserId,
    Username,
    Role,
    FullName,
    Email,
    PhoneNumber,
    IsActive,
    CreatedAt,
    EmployeeId_FK
FROM Users
ORDER BY UserId;
GO

PRINT '';
PRINT 'Customer Accounts with Auto-Generated Numbers:';
SELECT 
    AccountId,
    AccountNumber,
    AccountType,
    CustomerId,
    Balance,
    AvailableBalance,
    IsActive,
    CreatedAt
FROM CustomerAccounts
ORDER BY AccountId;
GO

PRINT '';
PRINT 'Invoices Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Invoices'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '========================================================';
PRINT 'Migration completed successfully!';
PRINT '========================================================';
