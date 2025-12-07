-- =============================================
-- DATA SYNC: LOCAL TO CLOUD
-- =============================================
-- 
-- This script helps you export data from LOCAL and import to CLOUD
-- 
-- STEP 1: Run EXPORT queries on LOCAL database (localhost\SQLEXPRESS -> BFASdatabase)
-- STEP 2: Copy the INSERT statements generated
-- STEP 3: Run the INSERT statements on CLOUD database (db34283.public.databaseasp.net -> db34283)
--
-- =============================================

-- =============================================
-- OPTION 1: GENERATE INSERT STATEMENTS FROM LOCAL
-- =============================================
-- Run this on your LOCAL database to generate INSERT statements

-- Connect to LOCAL first:
-- Server: localhost\SQLEXPRESS
-- Database: BFASdatabase

USE BFASdatabase;
GO

-- Generate INSERT for Users (run on local, copy results to cloud)
SELECT 
    'INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, CreatedAt, Department) VALUES (' +
    '''' + REPLACE(Username, '''', '''''') + ''', ' +
    '''' + REPLACE(PasswordHash, '''', '''''') + ''', ' +
    '''' + Role + ''', ' +
    ISNULL('''' + REPLACE(FullName, '''', '''''') + '''', 'NULL') + ', ' +
    ISNULL('''' + REPLACE(Email, '''', '''''') + '''', 'NULL') + ', ' +
    ISNULL('''' + PhoneNumber + '''', 'NULL') + ', ' +
    CAST(IsActive AS VARCHAR(1)) + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''', ' +
    ISNULL('''' + Department + '''', 'NULL') + ');'
AS InsertStatement
FROM Users;
GO

-- Generate INSERT for CustomerAccounts
SELECT 
    'INSERT INTO CustomerAccounts (UserId, AccountNumber, AccountName, AccountType, Balance, AvailableBalance, Currency, Status, CreatedAt) VALUES (' +
    CAST(UserId AS VARCHAR(10)) + ', ' +
    '''' + AccountNumber + ''', ' +
    ISNULL('''' + REPLACE(AccountName, '''', '''''') + '''', 'NULL') + ', ' +
    '''' + AccountType + ''', ' +
    CAST(Balance AS VARCHAR(20)) + ', ' +
    CAST(AvailableBalance AS VARCHAR(20)) + ', ' +
    '''' + Currency + ''', ' +
    '''' + Status + ''', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
AS InsertStatement
FROM CustomerAccounts;
GO

-- Generate INSERT for Customers
SELECT 
    'INSERT INTO Customers (AccountId, FullName, Email, PhoneNumber, Address, CreatedAt) VALUES (' +
    CAST(AccountId AS VARCHAR(10)) + ', ' +
    '''' + REPLACE(FullName, '''', '''''') + ''', ' +
    ISNULL('''' + REPLACE(Email, '''', '''''') + '''', 'NULL') + ', ' +
    ISNULL('''' + PhoneNumber + '''', 'NULL') + ', ' +
    ISNULL('''' + REPLACE(Address, '''', '''''') + '''', 'NULL') + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
AS InsertStatement
FROM Customers;
GO

-- =============================================
-- OPTION 2: USE LINKED SERVER (Advanced)
-- =============================================
-- This requires setting up a linked server connection

-- Step 1: Create linked server to cloud (run on local)
/*
EXEC sp_addlinkedserver 
    @server = 'CloudDB',
    @srvproduct = '',
    @provider = 'SQLNCLI',
    @datasrc = 'db34283.public.databaseasp.net,1433';

EXEC sp_addlinkedsrvlogin 
    @rmtsrvname = 'CloudDB',
    @useself = 'FALSE',
    @locallogin = NULL,
    @rmtuser = 'db34283',
    @rmtpassword = 'Zx6=2+fXCm8!';
*/

-- Step 2: Copy data using linked server
/*
INSERT INTO [CloudDB].[db34283].[dbo].[Users]
SELECT * FROM [BFASdatabase].[dbo].[Users];
*/

-- =============================================
-- OPTION 3: USE SSMS IMPORT/EXPORT WIZARD
-- =============================================
-- 1. Right-click on LOCAL database -> Tasks -> Export Data
-- 2. Source: localhost\SQLEXPRESS, BFASdatabase
-- 3. Destination: db34283.public.databaseasp.net,1433, db34283
-- 4. Select tables to copy
-- 5. Run immediately

PRINT 'See instructions above for data sync options';
