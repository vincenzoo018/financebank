-- Migration: Add ValidId columns to Users table
-- Date: December 3, 2025
-- Description: Adds ValidIdDocument, ValidIdFileName, and ValidIdContentType columns to store customer valid ID documents

USE BFASDatabase;
GO

-- Check if columns already exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ValidIdDocument')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD ValidIdDocument VARBINARY(MAX) NULL;
    
    PRINT 'Column ValidIdDocument added successfully';
END
ELSE
BEGIN
    PRINT 'Column ValidIdDocument already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ValidIdFileName')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD ValidIdFileName NVARCHAR(50) NULL;
    
    PRINT 'Column ValidIdFileName added successfully';
END
ELSE
BEGIN
    PRINT 'Column ValidIdFileName already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ValidIdContentType')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD ValidIdContentType NVARCHAR(100) NULL;
    
    PRINT 'Column ValidIdContentType added successfully';
END
ELSE
BEGIN
    PRINT 'Column ValidIdContentType already exists';
END
GO

PRINT 'Migration completed: ValidId columns added to Users table';
GO
