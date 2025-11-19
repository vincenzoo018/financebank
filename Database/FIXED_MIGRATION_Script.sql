-- =============================================
-- FIXED MIGRATION SCRIPT
-- Database: BFASdatabase
-- Purpose: Fix all migration issues
-- =============================================

USE BFASdatabase;
GO

PRINT '========================================================';
PRINT 'STARTING FIXED MIGRATION';
PRINT '========================================================';
GO

-- =============================================
-- STEP 1: CHECK AND REMOVE OLD COLUMNS FROM USERS
-- =============================================
PRINT 'STEP 1: Cleaning up Users table...';
GO

-- Remove old EmployeeId column if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmployeeId')
BEGIN
    ALTER TABLE Users DROP COLUMN EmployeeId;
    PRINT '✓ Removed old EmployeeId column';
END
ELSE
BEGIN
    PRINT '✓ Old EmployeeId column already removed';
END
GO

-- Remove old Department column if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Department')
BEGIN
    ALTER TABLE Users DROP COLUMN Department;
    PRINT '✓ Removed old Department column';
END
ELSE
BEGIN
    PRINT '✓ Old Department column already removed';
END
GO

-- =============================================
-- STEP 2: CREATE EMPLOYEE TABLE
-- =============================================
PRINT 'STEP 2: Creating Employee table...';
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
-- STEP 3: ADD EmployeeId_FK TO USERS TABLE
-- =============================================
PRINT 'STEP 3: Adding EmployeeId_FK to Users table...';
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
-- STEP 4: ADD FK CONSTRAINT TO USERS (NO ACTION to avoid cascade issues)
-- =============================================
PRINT 'STEP 4: Adding foreign key constraint...';
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'Users' AND CONSTRAINT_NAME = 'FK_Users_Employees')
BEGIN
    ALTER TABLE Users
    ADD CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId_FK) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION ON UPDATE NO ACTION;
    
    PRINT '✓ Foreign key constraint added';
END
ELSE
BEGIN
    PRINT '✓ Foreign key constraint already exists';
END
GO

-- =============================================
-- STEP 5: SEED EMPLOYEE DATA FOR EXISTING EMPLOYEES
-- =============================================
PRINT 'STEP 5: Seeding employee data...';
GO

-- Insert employees for SuperAdmin, Accountant, FinanceManager, Teller roles
IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = 'EMP-001')
BEGIN
    INSERT INTO Employees (EmployeeId, UserId, FirstName, LastName, Department, IsActive, CreatedAt)
    SELECT 
        'EMP-' + RIGHT('00' + CAST(ROW_NUMBER() OVER (ORDER BY UserId) AS NVARCHAR(3)), 3) AS EmployeeId,
        UserId,
        SUBSTRING(FullName, 1, ISNULL(NULLIF(CHARINDEX(' ', FullName) - 1, -1), LEN(FullName))) AS FirstName,
        CASE 
            WHEN CHARINDEX(' ', FullName) > 0 THEN SUBSTRING(FullName, CHARINDEX(' ', FullName) + 1, LEN(FullName))
            ELSE ''
        END AS LastName,
        'General' AS Department,
        IsActive,
        CreatedAt
    FROM Users
    WHERE Role IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Teller')
      AND UserId NOT IN (SELECT UserId FROM Employees);
    
    PRINT '✓ Employee data seeded successfully';
END
ELSE
BEGIN
    PRINT '✓ Employee data already seeded';
END
GO

-- =============================================
-- STEP 6: CREATE INVOICES TABLE
-- =============================================
PRINT 'STEP 6: Creating Invoices table...';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
        InvoiceType NVARCHAR(50) NOT NULL,
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
        
        CONSTRAINT FK_Invoices_CustomerAccounts FOREIGN KEY (AccountId) REFERENCES CustomerAccounts(AccountId) ON DELETE NO ACTION,
        CONSTRAINT FK_Invoices_CustomerTransactions FOREIGN KEY (TransactionId) REFERENCES CustomerTransactions(TransactionId) ON DELETE NO ACTION
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
-- STEP 7: AUTO-GENERATE ACCOUNT NUMBERS
-- =============================================
PRINT 'STEP 7: Auto-generating account numbers...';
GO

UPDATE CustomerAccounts
SET AccountNumber = 'ACC-' + FORMAT(GETDATE(), 'yyyy') + '-' + RIGHT('00000' + CAST(AccountId AS NVARCHAR(5)), 5)
WHERE AccountNumber IS NULL OR AccountNumber = '' OR AccountNumber LIKE 'ACC-0000%';

PRINT '✓ Account numbers auto-generated';
GO

-- =============================================
-- STEP 8: ENSURE CUSTOMERACCOUNTS HAS FK TO USERS
-- =============================================
PRINT 'STEP 8: Verifying CustomerAccounts foreign key...';
GO

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'CustomerAccounts' AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME = 'FK_CustomerAccounts_Users'
)
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.foreign_keys 
        WHERE parent_object_id = OBJECT_ID('CustomerAccounts') 
        AND referenced_object_id = OBJECT_ID('Users')
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
