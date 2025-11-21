# Required Database Migrations

## Overview
The C# models have been updated to match the business logic, but the database schema is missing some columns. This document lists all required migrations.

## Migrations Required

### 1. Add ProcessedByEmployeeName to CustomerTransactions
**File:** `Database/ADD_PROCESSEDBYEMPLOYEENAME_COLUMN.sql`

**What it does:**
- Adds `ProcessedByEmployeeName` column (NVARCHAR(100), NULL)
- Stores the full name of the teller/employee who processed the transaction
- Used for audit trail and receipt display

**Why it's needed:**
- TellerBankingService sets `ProcessedByEmployeeName = _authService.FullName`
- CustomerBankingService sets `ProcessedByEmployeeName = _authService.FullName`
- Database was missing this column

**SQL:**
```sql
ALTER TABLE [CustomerTransactions]
ADD [ProcessedByEmployeeName] NVARCHAR(100) NULL;
```

---

## How to Run Migrations

### Option 1: SQL Server Management Studio (SSMS) - RECOMMENDED

1. **Open SQL Server Management Studio**
2. **Connect to:** `localhost\SQLEXPRESS` → Database: `BFASdatabase`
3. **Open migration script:**
   - File → Open → File
   - Navigate to: `Database/ADD_PROCESSEDBYEMPLOYEENAME_COLUMN.sql`
4. **Execute:**
   - Click "Execute" button or press F5
   - Check "Messages" tab for "✓ Migration completed successfully!"

### Option 2: Command Line (sqlcmd)

```bash
sqlcmd -S localhost\SQLEXPRESS -d BFASdatabase -i "c:\Users\MECHREVO\source\repos\FinanceBank\Database\ADD_PROCESSEDBYEMPLOYEENAME_COLUMN.sql"
```

### Option 3: Visual Studio

1. **Open Visual Studio**
2. **View → SQL Server Object Explorer**
3. **Connect to database** (if not already connected)
4. **Right-click database → New Query**
5. **Copy and paste migration script**
6. **Execute** (Ctrl+Shift+E)

---

## Verification

After running the migration, verify the column was added:

```sql
-- Check CustomerTransactions table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CustomerTransactions'
ORDER BY ORDINAL_POSITION;
```

**Expected output should include:**
```
ProcessedByEmployeeName | nvarchar | YES | NULL
```

---

## What Each Column Does

### ProcessedByEmployeeName
- **Type:** NVARCHAR(100), NULL
- **Purpose:** Stores the full name of the teller/employee who processed the transaction
- **Example values:** "John Doe", "Jane Smith", "Admin User"
- **Used in:**
  - Receipt/Invoice display
  - Audit trail
  - Transaction history
  - Teller dashboard

---

## Database Schema After Migration

### CustomerTransactions Table
```sql
CREATE TABLE [CustomerTransactions]
(
    [TransactionId]              BIGINT IDENTITY(1,1) PRIMARY KEY,
    [TransactionNumber]          NVARCHAR(50) NOT NULL,
    [AccountId]                  INT NOT NULL,
    [TransactionType]            NVARCHAR(50) NOT NULL,
    [Amount]                     DECIMAL(18,2) NOT NULL,
    [Fee]                        DECIMAL(18,2) DEFAULT 0,
    [Status]                     NVARCHAR(50) NOT NULL,
    [Description]                NVARCHAR(500) NULL,
    [Reference]                  NVARCHAR(100) NULL,
    [ToAccountId]                INT NULL,
    [ToAccountName]              NVARCHAR(100) NULL,
    [BillerName]                 NVARCHAR(100) NULL,
    [BillerAccountNumber]        NVARCHAR(50) NULL,
    [CreatedAt]                  DATETIME2(0) NOT NULL,
    [ProcessedAt]                DATETIME2(0) NULL,
    [ProcessedBy]                NVARCHAR(50) NULL,
    [ProcessedByEmployeeName]    NVARCHAR(100) NULL  ← NEW COLUMN
);
```

---

## Rollback (if needed)

If you need to rollback the migration:

```sql
-- Rollback script
ALTER TABLE [CustomerTransactions] DROP COLUMN [ProcessedByEmployeeName];
```

---

## Testing After Migration

1. **Build the solution** in Visual Studio
2. **Log in as Teller**
3. **Process a deposit:**
   - Search for customer
   - Enter deposit amount
   - Select deposit method
   - Click "Process Deposit"
4. **Verify:**
   - No SQL errors
   - Deposit appears in customer history
   - Teller name appears in transaction details

---

## Troubleshooting

### Error: "Invalid column name 'ProcessedByEmployeeName'"
- **Cause:** Migration script hasn't been run yet
- **Solution:** Run the migration script above

### Error: "Column 'ProcessedByEmployeeName' does not exist"
- **Cause:** Migration script hasn't been run yet
- **Solution:** Run the migration script above

### Error: "The column name is not valid"
- **Cause:** Typo in column name or table name
- **Solution:** Check the migration script spelling

### Error: "Cannot add column, column already exists"
- **Cause:** The column was already added in a previous run
- **Solution:** This is normal, the script checks for this and skips if it exists

---

## Summary

✅ **Migration script created:** `ADD_PROCESSEDBYEMPLOYEENAME_COLUMN.sql`
✅ **Safe to run:** Checks if column exists before adding
✅ **Idempotent:** Can be run multiple times without errors
✅ **Complete:** Adds the missing column needed for deposits/withdrawals

**Next Steps:**
1. Run the migration script using one of the methods above
2. Verify the column was added
3. Rebuild the solution
4. Test deposits and withdrawals

**Status: READY TO MIGRATE** ✅
