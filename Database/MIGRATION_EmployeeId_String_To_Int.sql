-- =====================================================
-- MIGRATION: Convert EmployeeId from NVARCHAR(50) to INT
-- =====================================================
-- This script converts the Employee.EmployeeId column from string to integer
-- and updates the foreign key in the Users table accordingly.
-- =====================================================

USE [BFASdatabase];
GO

PRINT '========================================';
PRINT 'MIGRATION: EmployeeId String to Int';
PRINT '========================================';
GO

-- =====================================================
-- STEP 1: Drop existing foreign key constraints
-- =====================================================
PRINT 'STEP 1: Dropping existing foreign key constraints...';
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'Users' AND CONSTRAINT_NAME = 'FK_Users_Employees')
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Employees];
    PRINT '✓ Dropped FK_Users_Employees constraint';
END
GO

-- =====================================================
-- STEP 2: Drop the old EmployeeId column and recreate as INT
-- =====================================================
PRINT 'STEP 2: Recreating EmployeeId column as INT...';
GO

-- Create a temporary table to backup Employees data
CREATE TABLE [Employees_Backup] (
    [EmployeeId_Old] NVARCHAR(50),
    [UserId] INT NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Department] NVARCHAR(100) NOT NULL,
    [Position] NVARCHAR(100) NULL,
    [HireDate] DATETIME NOT NULL,
    [IsActive] BIT NOT NULL,
    [CreatedAt] DATETIME NOT NULL,
    [UpdatedAt] DATETIME NULL
);

-- Backup existing data
INSERT INTO [Employees_Backup]
SELECT [EmployeeId], [UserId], [FirstName], [LastName], [Department], [Position], [HireDate], [IsActive], [CreatedAt], [UpdatedAt]
FROM [Employees];

PRINT '✓ Backed up Employees data';
GO

-- Drop the old Employees table
DROP TABLE [Employees];
PRINT '✓ Dropped old Employees table';
GO

-- Recreate Employees table with INT EmployeeId
CREATE TABLE [Employees] (
    [EmployeeId] INT PRIMARY KEY,
    [UserId] INT NOT NULL UNIQUE,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Department] NVARCHAR(100) NOT NULL,
    [Position] NVARCHAR(100) NULL,
    [HireDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NULL,
    CONSTRAINT FK_Employees_Users FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]) ON DELETE CASCADE
);

CREATE INDEX IX_Employees_UserId ON [Employees]([UserId]);
CREATE INDEX IX_Employees_Department ON [Employees]([Department]);
CREATE INDEX IX_Employees_IsActive ON [Employees]([IsActive]);

PRINT '✓ Created new Employees table with INT EmployeeId';
GO

-- =====================================================
-- STEP 3: Restore data with new INT IDs
-- =====================================================
PRINT 'STEP 3: Restoring employee data with sequential INT IDs...';
GO

INSERT INTO [Employees] ([EmployeeId], [UserId], [FirstName], [LastName], [Department], [Position], [HireDate], [IsActive], [CreatedAt], [UpdatedAt])
SELECT 
    ROW_NUMBER() OVER (ORDER BY [UserId]) AS [EmployeeId],
    [UserId],
    [FirstName],
    [LastName],
    [Department],
    [Position],
    [HireDate],
    [IsActive],
    [CreatedAt],
    [UpdatedAt]
FROM [Employees_Backup];

PRINT '✓ Restored employee data with sequential INT IDs';
GO

-- =====================================================
-- STEP 4: Update Users.EmployeeId_FK to INT
-- =====================================================
PRINT 'STEP 4: Updating Users.EmployeeId_FK to INT...';
GO

-- Drop the old EmployeeId_FK column if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmployeeId_FK')
BEGIN
    ALTER TABLE [Users] DROP COLUMN [EmployeeId_FK];
    PRINT '✓ Dropped old EmployeeId_FK column';
END
GO

-- Add new EmployeeId_FK as INT
ALTER TABLE [Users]
ADD [EmployeeId_FK] INT NULL;

PRINT '✓ Added EmployeeId_FK as INT';
GO

-- =====================================================
-- STEP 5: Recreate foreign key constraint
-- =====================================================
PRINT 'STEP 5: Creating foreign key constraint...';
GO

ALTER TABLE [Users]
ADD CONSTRAINT [FK_Users_Employees] FOREIGN KEY ([EmployeeId_FK]) REFERENCES [Employees]([EmployeeId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

PRINT '✓ Created FK_Users_Employees constraint';
GO

-- =====================================================
-- STEP 6: Populate EmployeeId_FK for existing users
-- =====================================================
PRINT 'STEP 6: Populating EmployeeId_FK for existing users...';
GO

UPDATE [Users]
SET [EmployeeId_FK] = (SELECT [EmployeeId] FROM [Employees] WHERE [Employees].[UserId] = [Users].[UserId])
WHERE [UserId] IN (SELECT [UserId] FROM [Employees]);

PRINT '✓ Populated EmployeeId_FK for existing users';
GO

-- =====================================================
-- STEP 7: Cleanup backup table
-- =====================================================
PRINT 'STEP 7: Cleaning up backup table...';
GO

DROP TABLE [Employees_Backup];
PRINT '✓ Dropped backup table';
GO

-- =====================================================
-- VERIFICATION
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION';
PRINT '========================================';
GO

PRINT 'Employees Table:';
SELECT 
    [EmployeeId],
    [UserId],
    [FirstName],
    [LastName],
    [Department],
    [IsActive]
FROM [Employees]
ORDER BY [EmployeeId];
GO

PRINT '';
PRINT 'Users with EmployeeId_FK:';
SELECT 
    [UserId],
    [Username],
    [Role],
    [EmployeeId_FK],
    [FullName]
FROM [Users]
WHERE [EmployeeId_FK] IS NOT NULL
ORDER BY [UserId];
GO

PRINT '';
PRINT '✓ Migration completed successfully!';
PRINT '========================================';
GO
