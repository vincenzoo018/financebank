-- =============================================
-- MIGRATION: Add Employee Table & Restructure Users
-- Database: BFASdatabase
-- Purpose: Separate Employee data from Users table
-- =============================================

USE BFASdatabase;
GO

-- =============================================
-- STEP 1: CREATE EMPLOYEE TABLE
-- =============================================
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
    
    PRINT 'Employee table created successfully';
END
GO

-- =============================================
-- STEP 2: MIGRATE EXISTING EMPLOYEE DATA
-- =============================================

-- Insert existing employees from Users table
INSERT INTO Employees (EmployeeId, UserId, FirstName, LastName, Department, IsActive, CreatedAt)
SELECT 
    EmployeeId,
    UserId,
    SUBSTRING(FullName, 1, CHARINDEX(' ', FullName + ' ') - 1) AS FirstName,
    SUBSTRING(FullName, CHARINDEX(' ', FullName + ' ') + 1, LEN(FullName)) AS LastName,
    ISNULL(Department, 'General') AS Department,
    IsActive,
    CreatedAt
FROM Users
WHERE Role IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Teller')
  AND EmployeeId IS NOT NULL;

PRINT 'Employee data migrated from Users table';
GO

-- =============================================
-- STEP 3: REMOVE EmployeeId AND Department FROM USERS TABLE
-- =============================================

-- Drop the old column
ALTER TABLE Users
DROP COLUMN EmployeeId, Department;

PRINT 'EmployeeId and Department columns removed from Users table';
GO

-- =============================================
-- STEP 4: ADD FK TO EMPLOYEE IN USERS TABLE (OPTIONAL - for direct reference)
-- =============================================

-- Add EmployeeId as nullable FK for future use
ALTER TABLE Users
ADD EmployeeId_FK NVARCHAR(50) NULL;

ALTER TABLE Users
ADD CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId_FK) REFERENCES Employees(EmployeeId) ON DELETE SET NULL;

PRINT 'Foreign key added to Users table';
GO

-- =============================================
-- STEP 5: ENSURE CUSTOMERACCOUNTS LINKS TO USERS
-- =============================================

-- Check if CustomerAccounts has CustomerId FK to Users
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'CustomerAccounts' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
)
BEGIN
    ALTER TABLE CustomerAccounts
    ADD CONSTRAINT FK_CustomerAccounts_Users FOREIGN KEY (CustomerId) REFERENCES Users(UserId) ON DELETE CASCADE;
    
    PRINT 'Foreign key added to CustomerAccounts table';
END
GO

-- =============================================
-- STEP 6: AUTO-GENERATE ACCOUNT NUMBERS FOR EXISTING CUSTOMERS
-- =============================================

-- Update any NULL account numbers with auto-generated ones
UPDATE CustomerAccounts
SET AccountNumber = 'ACC-' + FORMAT(GETDATE(), 'yyyy') + '-' + RIGHT('00000' + CAST(AccountId AS NVARCHAR(5)), 5)
WHERE AccountNumber IS NULL OR AccountNumber = '';

PRINT 'Account numbers auto-generated for customers';
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

PRINT '';
PRINT '==============================================';
PRINT 'MIGRATION COMPLETE - VERIFICATION';
PRINT '==============================================';
PRINT '';

-- View all employees
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

-- View Users (without EmployeeId and Department)
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
    CreatedAt
FROM Users
ORDER BY UserId;
GO

-- View Customer Accounts with auto-generated numbers
PRINT '';
PRINT 'Customer Accounts:';
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
PRINT '==============================================';
PRINT 'Migration completed successfully!';
PRINT '==============================================';
