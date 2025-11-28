-- =============================================
-- BFAS User Seeder with BCrypt Passwords
-- Creates SuperAdmin, Accountant, Finance Manager, and Teller
-- Passwords: 12+ characters with special characters and numbers
-- =============================================

USE BFASdatabase;
GO

-- Note: EmployeeId_FK can be NULL in Users table
-- If you need to link employees later, create them separately

-- Create Users with BCrypt hashed passwords (Work Factor: 12)
-- FRESHLY GENERATED with BCrypt.Net-Next 4.0.3 on 2024-11-28
-- Passwords:
-- Admin@2024!Secure -> $2a$12$YmA.lQ514jqo139qlWTyduX.qDlKZoQKkEa/GM6RSRvQ6alTabFa6
-- Account#2024$Pass -> $2a$12$qA/suEdMk1wtMnnAky32GeUc6PaFOiF5D.G30YOvLPpddq3M4u/wK
-- Finance$2024#Mgr! -> $2a$12$owP4e8uuN/dZ1INvz5lC7OfVEt./ISh/cUxlXLo49R.fYtbr64ezC
-- Teller@2024!Bank# -> $2a$12$5NaW5kLSaJ.6TdHJYf727O2.p2oQEEoVp1/olydyALuRU7U817G6i

-- DELETE ANY EXISTING TEST USERS FIRST
DELETE FROM Users WHERE Username IN ('superadmin2024', 'accountant2024', 'fmanager2024', 'teller2024');

INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, CreatedAt, LastLoginAt, EmployeeId_FK)
VALUES 
('superadmin2024', '$2a$12$YmA.lQ514jqo139qlWTyduX.qDlKZoQKkEa/GM6RSRvQ6alTabFa6', 'SuperAdmin', 'System Administrator', 'superadmin@bfas.com', '555-1001', 1, GETDATE(), NULL, NULL),

('accountant2024', '$2a$12$qA/suEdMk1wtMnnAky32GeUc6PaFOiF5D.G30YOvLPpddq3M4u/wK', 'Accountant', 'John Accountant', 'johnacct@bfas.com', '555-1002', 1, GETDATE(), NULL, NULL),

('fmanager2024', '$2a$12$owP4e8uuN/dZ1INvz5lC7OfVEt./ISh/cUxlXLo49R.fYtbr64ezC', 'FinanceManager', 'Jane Finance', 'jfinance@bfas.com', '555-1003', 1, GETDATE(), NULL, NULL),

('teller2024', '$2a$12$5NaW5kLSaJ.6TdHJYf727O2.p2oQEEoVp1/olydyALuRU7U817G6i', 'Teller', 'Anna Teller', 'ateller@bfas.com', '555-1004', 1, GETDATE(), NULL, NULL);
GO

-- Verify the seeded users
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
WHERE Username IN ('superadmin2024', 'accountant2024', 'fmanager2024', 'teller2024')
ORDER BY UserId;
GO

PRINT '✓ User seeding completed successfully with strong passwords!';
PRINT '  ';
PRINT '  NEW Login Credentials:';
PRINT '  ---------------------';
PRINT '  SuperAdmin:       username = superadmin2024  | password = Admin@2024!Secure';
PRINT '  Accountant:       username = accountant2024  | password = Account#2024$Pass';
PRINT '  Finance Manager:  username = fmanager2024    | password = Finance$2024#Mgr!';
PRINT '  Teller:           username = teller2024      | password = Teller@2024!Bank#';
PRINT '  ';
PRINT '  All passwords are 12+ characters with special characters and numbers.';
GO
