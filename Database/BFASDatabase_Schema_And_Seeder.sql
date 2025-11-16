-- =============================================
-- BFAS Database - Authentication System
-- Database Name: BFASdatabase
-- Created: 2025
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BFASdatabase')
BEGIN
    CREATE DATABASE BFASdatabase;
END
GO

USE BFASdatabase;
GO

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
        CONSTRAINT CK_Users_Role CHECK (Role IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Customer'))
    );
    
    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_Role ON Users(Role);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);
END
GO

-- =============================================
-- 2. LOGIN HISTORY TABLE
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
        FailureReason NVARCHAR(255) NULL,
        CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT CK_LoginHistory_Status CHECK (LoginStatus IN ('Success', 'Failed', 'Locked'))
    );
    
    CREATE INDEX IX_LoginHistory_UserId ON LoginHistory(UserId);
    CREATE INDEX IX_LoginHistory_LoginTime ON LoginHistory(LoginTime);
END
GO

-- =============================================
-- 3. USER SESSIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSessions')
BEGIN
    CREATE TABLE UserSessions (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        SessionToken NVARCHAR(255) NOT NULL UNIQUE,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(255) NULL,
        CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
    CREATE INDEX IX_UserSessions_SessionToken ON UserSessions(SessionToken);
    CREATE INDEX IX_UserSessions_IsActive ON UserSessions(IsActive);
END
GO

-- =============================================
-- 4. ROLE PERMISSIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        PermissionId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL,
        ModuleName NVARCHAR(100) NOT NULL,
        Permission NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT CK_RolePermissions_Role CHECK (RoleName IN ('SuperAdmin', 'Accountant', 'FinanceManager', 'Customer')),
        CONSTRAINT CK_RolePermissions_Permission CHECK (Permission IN ('Read', 'Write', 'Delete', 'Full'))
    );
    
    CREATE INDEX IX_RolePermissions_RoleName ON RolePermissions(RoleName);
    CREATE INDEX IX_RolePermissions_ModuleName ON RolePermissions(ModuleName);
END
GO

-- =============================================
-- SEEDER DATA - 4 TEST ACCOUNTS
-- =============================================

-- Clear existing data (for fresh install)
DELETE FROM UserSessions;
DELETE FROM LoginHistory;
DELETE FROM RolePermissions;
DELETE FROM Users;
GO

-- Reset identity seeds
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('LoginHistory', RESEED, 0);
DBCC CHECKIDENT ('UserSessions', RESEED, 0);
DBCC CHECKIDENT ('RolePermissions', RESEED, 0);
GO

-- =============================================
-- INSERT TEST USERS
-- Note: In production, use proper password hashing (BCrypt, PBKDF2, etc.)
-- For demo purposes, we're using simple hashing
-- =============================================

-- 1. SuperAdmin Account
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, Department, EmployeeId)
VALUES (
    'admin',
    'admin123', -- In production: Use BCrypt.HashPassword("admin123")
    'SuperAdmin',
    'System Administrator',
    'admin@bfas.com',
    '+63-917-123-4567',
    1,
    'IT Department',
    'EMP-001'
);

-- 2. Accountant Account
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, Department, EmployeeId)
VALUES (
    'accountant',
    'accountant123', -- In production: Use BCrypt.HashPassword("accountant123")
    'Accountant',
    'Juan Dela Cruz',
    'accountant@bfas.com',
    '+63-917-234-5678',
    1,
    'Accounting Department',
    'EMP-002'
);

-- 3. Finance Manager Account
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, Department, EmployeeId)
VALUES (
    'fmanager',
    'fmanager123', -- In production: Use BCrypt.HashPassword("fmanager123")
    'FinanceManager',
    'Maria Santos',
    'fmanager@bfas.com',
    '+63-917-345-6789',
    1,
    'Finance Department',
    'EMP-003'
);

-- 4. Customer Account
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, Department, EmployeeId)
VALUES (
    'customer',
    'customer123', -- In production: Use BCrypt.HashPassword("customer123")
    'Customer',
    'Pedro Garcia',
    'customer@gmail.com',
    '+63-917-456-7890',
    1,
    NULL,
    NULL
);
GO

-- =============================================
-- INSERT ROLE PERMISSIONS
-- =============================================

-- SuperAdmin - Full Access to Everything
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES 
    ('SuperAdmin', 'Dashboard', 'Full', 1),
    ('SuperAdmin', 'Banking', 'Full', 1),
    ('SuperAdmin', 'Accounting', 'Full', 1),
    ('SuperAdmin', 'Finance', 'Full', 1),
    ('SuperAdmin', 'Users', 'Full', 1),
    ('SuperAdmin', 'Reports', 'Full', 1),
    ('SuperAdmin', 'Settings', 'Full', 1),
    ('SuperAdmin', 'Approvals', 'Full', 1),
    ('SuperAdmin', 'Transactions', 'Full', 1),
    ('SuperAdmin', 'Audit', 'Full', 1),
    ('SuperAdmin', 'Security', 'Full', 1);

-- Accountant - Banking & Accounting Access
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES 
    ('Accountant', 'Dashboard', 'Read', 1),
    ('Accountant', 'Banking', 'Full', 1),
    ('Accountant', 'Accounting', 'Full', 1),
    ('Accountant', 'Reports', 'Full', 1),
    ('Accountant', 'Transactions', 'Read', 1);

-- Finance Manager - Finance & Budget Access
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES 
    ('FinanceManager', 'Dashboard', 'Read', 1),
    ('FinanceManager', 'Finance', 'Full', 1),
    ('FinanceManager', 'Reports', 'Full', 1),
    ('FinanceManager', 'Accounting', 'Read', 1);

-- Customer - Personal Banking Access
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES 
    ('Customer', 'Dashboard', 'Read', 1),
    ('Customer', 'Transactions', 'Write', 1),
    ('Customer', 'Accounts', 'Read', 1),
    ('Customer', 'Cards', 'Read', 1),
    ('Customer', 'Loans', 'Write', 1),
    ('Customer', 'Bills', 'Write', 1),
    ('Customer', 'Settings', 'Write', 1);
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- View all users
SELECT 
    UserId,
    Username,
    Role,
    FullName,
    Email,
    Department,
    EmployeeId,
    IsActive,
    CreatedAt
FROM Users
ORDER BY UserId;
GO

-- View role permissions
SELECT 
    RoleName,
    ModuleName,
    Permission,
    IsActive
FROM RolePermissions
ORDER BY RoleName, ModuleName;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- Procedure to authenticate user
CREATE OR ALTER PROCEDURE sp_AuthenticateUser
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @IpAddress NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    DECLARE @Role NVARCHAR(50);
    DECLARE @IsActive BIT;
    
    -- Check if user exists and password matches
    SELECT 
        @UserId = UserId,
        @Role = Role,
        @IsActive = IsActive
    FROM Users
    WHERE Username = @Username AND PasswordHash = @Password;
    
    IF @UserId IS NULL
    BEGIN
        -- Log failed login attempt
        INSERT INTO LoginHistory (UserId, LoginTime, IpAddress, UserAgent, LoginStatus, FailureReason)
        SELECT TOP 1 UserId, GETDATE(), @IpAddress, @UserAgent, 'Failed', 'Invalid credentials'
        FROM Users WHERE Username = @Username;
        
        SELECT 'Failed' AS Status, 'Invalid username or password' AS Message;
        RETURN;
    END
    
    IF @IsActive = 0
    BEGIN
        -- Log locked account attempt
        INSERT INTO LoginHistory (UserId, LoginTime, IpAddress, UserAgent, LoginStatus, FailureReason)
        VALUES (@UserId, GETDATE(), @IpAddress, @UserAgent, 'Locked', 'Account is inactive');
        
        SELECT 'Locked' AS Status, 'Account is inactive' AS Message;
        RETURN;
    END
    
    -- Successful login
    UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @UserId;
    
    INSERT INTO LoginHistory (UserId, LoginTime, IpAddress, UserAgent, LoginStatus)
    VALUES (@UserId, GETDATE(), @IpAddress, @UserAgent, 'Success');
    
    -- Return user details
    SELECT 
        'Success' AS Status,
        UserId,
        Username,
        Role,
        FullName,
        Email,
        Department,
        EmployeeId
    FROM Users
    WHERE UserId = @UserId;
END
GO

-- =============================================
-- TEST THE AUTHENTICATION
-- =============================================

PRINT '==============================================';
PRINT 'BFAS Database Setup Complete!';
PRINT '==============================================';
PRINT '';
PRINT 'Test Accounts Created:';
PRINT '1. SuperAdmin    - Username: admin       Password: admin123';
PRINT '2. Accountant    - Username: accountant  Password: accountant123';
PRINT '3. Finance Mgr   - Username: fmanager    Password: fmanager123';
PRINT '4. Customer      - Username: customer    Password: customer123';
PRINT '';
PRINT 'Database: BFASdatabase';
PRINT 'Tables Created: Users, LoginHistory, UserSessions, RolePermissions';
PRINT '==============================================';
GO
