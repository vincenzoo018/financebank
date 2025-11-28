-- =============================================
-- Check Users Table for Authentication Issues
-- =============================================

USE BFASdatabase;
GO

-- Check if the users exist
SELECT 
    UserId,
    Username,
    Role,
    FullName,
    Email,
    IsActive,
    LEN(PasswordHash) as PasswordHashLength,
    LEFT(PasswordHash, 20) as HashPrefix,
    CASE 
        WHEN PasswordHash LIKE '$2a$12$%' THEN 'BCrypt Valid Format'
        WHEN PasswordHash LIKE '$2b$%' THEN 'BCrypt 2b Format'
        WHEN PasswordHash LIKE '$2y$%' THEN 'BCrypt 2y Format'
        ELSE 'Unknown Hash Format'
    END as HashFormat,
    CreatedAt,
    LastLoginAt
FROM Users
WHERE Username IN ('superadmin2024', 'accountant2024', 'fmanager2024', 'teller2024', 'admin', 'accountant', 'financemanager', 'teller')
ORDER BY Username;
GO

-- Count users by role
SELECT 
    Role,
    COUNT(*) as UserCount
FROM Users
WHERE IsActive = 1
GROUP BY Role
ORDER BY Role;
GO

-- Check for any customer accounts (to compare)
SELECT TOP 3
    UserId,
    Username,
    Role,
    LEN(PasswordHash) as PasswordHashLength,
    LEFT(PasswordHash, 20) as HashPrefix
FROM Users
WHERE Role = 'Customer'
ORDER BY CreatedAt DESC;
GO
