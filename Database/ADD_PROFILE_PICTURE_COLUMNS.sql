-- =============================================
-- Migration: Add Profile Picture Support to Users Table
-- Description: Adds columns for storing user profile pictures
-- Created: 2024
-- =============================================

USE BFAS;
GO

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ProfilePicture')
BEGIN
    ALTER TABLE Users 
    ADD ProfilePicture varbinary(max) NULL;
    PRINT 'ProfilePicture column added successfully.';
END
ELSE
BEGIN
    PRINT 'ProfilePicture column already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ProfilePictureFileName')
BEGIN
    ALTER TABLE Users 
    ADD ProfilePictureFileName nvarchar(255) NULL;
    PRINT 'ProfilePictureFileName column added successfully.';
END
ELSE
BEGIN
    PRINT 'ProfilePictureFileName column already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ProfilePictureContentType')
BEGIN
    ALTER TABLE Users 
    ADD ProfilePictureContentType nvarchar(100) NULL;
    PRINT 'ProfilePictureContentType column added successfully.';
END
ELSE
BEGIN
    PRINT 'ProfilePictureContentType column already exists.';
END
GO

-- Verify the columns were added
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME IN ('ProfilePicture', 'ProfilePictureFileName', 'ProfilePictureContentType')
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Profile picture migration completed successfully!';
GO
