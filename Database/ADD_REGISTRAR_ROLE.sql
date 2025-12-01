-- =====================================================
-- ADD REGISTRAR ROLE TO DATABASE (BFASdatabase)
-- Run this script to allow the Registrar role in the database
-- =====================================================

USE BFASdatabase;
GO

-- First, let's see what tables and constraints exist
PRINT 'Checking database structure...';
PRINT '';

-- Check if Users table exists and show its constraints
PRINT 'Looking for Users table and its CHECK constraints:';
SELECT 
    t.name AS TableName,
    c.name AS ConstraintName,
    c.definition AS ConstraintDefinition
FROM sys.check_constraints c
INNER JOIN sys.tables t ON c.parent_object_id = t.object_id
WHERE t.name = 'Users' OR c.name LIKE '%Role%';
GO

-- Drop constraint if it exists (handles both dbo.Users and just Users)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Role')
BEGIN
    DECLARE @TableName NVARCHAR(255);
    DECLARE @SQL NVARCHAR(MAX);
    
    SELECT @TableName = OBJECT_NAME(parent_object_id) 
    FROM sys.check_constraints 
    WHERE name = 'CK_Users_Role';
    
    SET @SQL = 'ALTER TABLE [' + @TableName + '] DROP CONSTRAINT [CK_Users_Role]';
    EXEC sp_executesql @SQL;
    PRINT 'Dropped existing CK_Users_Role constraint from table: ' + @TableName;
    
    SET @SQL = 'ALTER TABLE [' + @TableName + '] WITH CHECK ADD CONSTRAINT [CK_Users_Role] CHECK (([Role]=''Customer'' OR [Role]=''Teller'' OR [Role]=''FinanceManager'' OR [Role]=''Accountant'' OR [Role]=''SuperAdmin'' OR [Role]=''Admin'' OR [Role]=''Registrar''))';
    EXEC sp_executesql @SQL;
    PRINT 'Created new CK_Users_Role constraint with Registrar role on table: ' + @TableName;
END
ELSE
BEGIN
    PRINT 'No CK_Users_Role constraint found - Role column may not have any restrictions.';
END
GO

-- Do the same for RolePermissions if it exists
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_RolePermissions_Role')
BEGIN
    DECLARE @TableName2 NVARCHAR(255);
    DECLARE @SQL2 NVARCHAR(MAX);
    
    SELECT @TableName2 = OBJECT_NAME(parent_object_id) 
    FROM sys.check_constraints 
    WHERE name = 'CK_RolePermissions_Role';
    
    SET @SQL2 = 'ALTER TABLE [' + @TableName2 + '] DROP CONSTRAINT [CK_RolePermissions_Role]';
    EXEC sp_executesql @SQL2;
    
    SET @SQL2 = 'ALTER TABLE [' + @TableName2 + '] WITH CHECK ADD CONSTRAINT [CK_RolePermissions_Role] CHECK (([RoleName]=''Customer'' OR [RoleName]=''Teller'' OR [RoleName]=''FinanceManager'' OR [RoleName]=''Accountant'' OR [RoleName]=''SuperAdmin'' OR [RoleName]=''Admin'' OR [RoleName]=''Registrar''))';
    EXEC sp_executesql @SQL2;
    PRINT 'Updated CK_RolePermissions_Role constraint.';
END
GO

PRINT '';
PRINT 'Script completed!';
GO
