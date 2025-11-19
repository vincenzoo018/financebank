# 📋 STEP-BY-STEP DEPLOYMENT INSTRUCTIONS

## ⚠️ IMPORTANT: Follow these steps EXACTLY in order

---

## PHASE 1: DATABASE MIGRATION (SQL Server)

### **Step 1: Open SQL Server Management Studio**
1. Click Start → Search for "SQL Server Management Studio"
2. Open SQL Server Management Studio
3. Connect to your SQL Server instance
4. Select "BFASdatabase" in the Object Explorer

### **Step 2: Execute First Migration Script**
1. File → Open → File
2. Navigate to: `c:\Users\MECHREVO\source\repos\FinanceBank\Database\`
3. Select: `MIGRATION_Employee_Table.sql`
4. Click Open
5. Click "Execute" (or press F5)
6. Wait for completion (should see "Migration completed successfully!")
7. Check Messages tab for any errors

### **Step 3: Verify First Migration**
Run these verification queries in a new query window:

```sql
-- Check Employee table exists
SELECT COUNT(*) as EmployeeCount FROM Employees;

-- Check Users updated
SELECT UserId, Username, Role, EmployeeId_FK FROM Users LIMIT 5;

-- Check FK created
SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Users' AND CONSTRAINT_TYPE = 'FOREIGN KEY';
```

All should return results without errors.

### **Step 4: Execute Second Migration Script**
1. File → Open → File
2. Select: `COMPLETE_MIGRATION_Script.sql`
3. Click Open
4. Click "Execute" (F5)
5. Wait for completion
6. Check Messages tab for success message

### **Step 5: Verify Second Migration**
Run these queries:

```sql
-- Check Invoices table
SELECT COUNT(*) as InvoiceCount FROM Invoices;

-- Check account numbers
SELECT TOP 5 AccountNumber FROM CustomerAccounts;

-- Check all tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Employees', 'Invoices', 'CustomerAccounts');
```

All should return results.

### **Step 6: Verify Data Integrity**
Run this comprehensive check:

```sql
-- Check foreign keys
SELECT 
    CONSTRAINT_NAME,
    TABLE_NAME,
    REFERENCED_TABLE_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE TABLE_NAME IN ('Users', 'Employees', 'CustomerAccounts', 'Invoices');

-- Check indexes
SELECT TABLE_NAME, INDEX_NAME FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_NAME IN ('Employees', 'Invoices');

-- Count records
SELECT 
    'Employees' as TableName, COUNT(*) as RecordCount FROM Employees
UNION ALL
SELECT 'Invoices', COUNT(*) FROM Invoices
UNION ALL
SELECT 'CustomerAccounts', COUNT(*) FROM CustomerAccounts;
```

---

## PHASE 2: CODE COMPILATION (Visual Studio)

### **Step 7: Open Visual Studio Solution**
1. Open File Explorer
2. Navigate to: `c:\Users\MECHREVO\source\repos\FinanceBank\`
3. Find `FinanceBank.sln`
4. Double-click to open in Visual Studio
5. Wait for solution to load (may take 1-2 minutes)

### **Step 8: Verify All Files Present**
In Solution Explorer, verify these files exist:
- [ ] `Models/AuthModels.cs` (should have Employee class)
- [ ] `Models/DatabaseModels.cs` (should have GenerateAccountNumber method)
- [ ] `Models/InvoiceModels.cs` (NEW file)
- [ ] `Services/TellerBankingService.cs` (should have invoice methods)
- [ ] `Data/BFASDbContext.cs` (should have Employees and Invoices DbSets)

### **Step 9: Clean Solution**
1. Build → Clean Solution
2. Wait for completion
3. Check Output window for "Clean complete"

### **Step 10: Build Solution**
1. Build → Build Solution (or Ctrl+Shift+B)
2. Wait for build to complete
3. Check Output window for "Build succeeded"

### **Step 11: Check for Errors**
1. View → Error List
2. Should show 0 Errors
3. Warnings are OK (can ignore)

If there are errors:
- [ ] Check that all files are present
- [ ] Verify no syntax errors in models
- [ ] Check DbContext has all DbSets
- [ ] Verify namespaces are correct

---

## PHASE 3: ENTITY FRAMEWORK MIGRATION (Optional but Recommended)

### **Step 12: Open Package Manager Console**
1. Tools → NuGet Package Manager → Package Manager Console
2. Wait for console to load

### **Step 13: Create EF Migration**
In Package Manager Console, type:
```powershell
Add-Migration AddEmployeeAndInvoiceTables
```

Press Enter and wait for completion.

### **Step 14: Update Database**
In Package Manager Console, type:
```powershell
Update-Database
```

Press Enter and wait for completion.

Check for success message: "Done."

---

## PHASE 4: TESTING

### **Step 15: Start Application**
1. Press F5 (or Debug → Start Debugging)
2. Wait for application to start
3. Browser should open to login page

### **Step 16: Test Teller Deposit**

**Login as Teller:**
1. Username: `teller`
2. Password: `teller123`
3. Click Login
4. Should redirect to `/teller/dashboard`

**Process Deposit:**
1. Click "Process Deposits" or go to `/teller/deposits`
2. In "Account Number" field, enter: `ACC-2025-00001` (or any valid account number)
3. Press Tab or click elsewhere to auto-fill
4. Verify customer name appears
5. Verify current balance displays
6. Enter Deposit Amount: `1000`
7. Select Deposit Method: `OTC`
8. Click "Process Deposit"
9. Should see success message with receipt number
10. Click "Print Receipt" to download invoice

**Verify in Database:**
```sql
SELECT TOP 1 * FROM Invoices ORDER BY CreatedAt DESC;
SELECT TOP 1 * FROM CustomerTransactions ORDER BY CreatedAt DESC;
SELECT * FROM CustomerAccounts WHERE AccountNumber = 'ACC-2025-00001';
```

### **Step 17: Test Teller Withdrawal**

**Process Withdrawal:**
1. Go to `/teller/withdrawals`
2. Enter Account Number: `ACC-2025-00001`
3. Verify customer details auto-fill
4. Verify available balance displays
5. Enter Withdrawal Amount: `500`
6. Select Withdrawal Method: `OTC`
7. Click "Process Withdrawal"
8. Should see success message
9. Click "Print Receipt"

**Verify in Database:**
```sql
SELECT TOP 1 * FROM Invoices WHERE InvoiceType = 'Withdrawal' ORDER BY CreatedAt DESC;
SELECT * FROM CustomerAccounts WHERE AccountNumber = 'ACC-2025-00001';
```

### **Step 18: Test Customer Dashboard**

**Login as Customer:**
1. Logout from Teller account
2. Username: `customer`
3. Password: `customer123`
4. Click Login
5. Should redirect to `/customer/dashboard`

**Verify Updates:**
1. Check account balance (should reflect deposits/withdrawals)
2. Check "Recent Transactions" section
3. Should show deposit and withdrawal transactions
4. Verify amounts are correct
5. Verify balance calculation is correct

---

## PHASE 5: VERIFICATION

### **Step 19: Database Verification**
Run these queries to verify everything:

```sql
-- 1. Check Employee table
SELECT COUNT(*) as EmployeeCount FROM Employees;
SELECT * FROM Employees LIMIT 1;

-- 2. Check Invoices table
SELECT COUNT(*) as InvoiceCount FROM Invoices;
SELECT * FROM Invoices LIMIT 1;

-- 3. Check account numbers
SELECT COUNT(*) as AccountsWithNumbers FROM CustomerAccounts 
WHERE AccountNumber LIKE 'ACC-%';

-- 4. Check foreign keys
SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Invoices' AND CONSTRAINT_TYPE = 'FOREIGN KEY';

-- 5. Check recent transactions
SELECT TOP 5 
    TransactionNumber, 
    TransactionType, 
    Amount, 
    CreatedAt 
FROM CustomerTransactions 
ORDER BY CreatedAt DESC;
```

### **Step 20: Code Verification**
1. Open `Models/InvoiceModels.cs`
2. Verify Invoice class has all properties
3. Open `Services/TellerBankingService.cs`
4. Verify GenerateInvoiceAsync method exists
5. Open `Data/BFASDbContext.cs`
6. Verify `public DbSet<Invoice> Invoices { get; set; }` exists
7. Verify `public DbSet<Employee> Employees { get; set; }` exists

### **Step 21: Functional Verification**
- [ ] Teller can log in
- [ ] Teller can access deposit page
- [ ] Teller can access withdrawal page
- [ ] Customer search works
- [ ] Account details auto-fill
- [ ] Deposit processes successfully
- [ ] Withdrawal processes successfully
- [ ] Invoice generates
- [ ] Invoice can be downloaded
- [ ] Customer dashboard updates
- [ ] Balance reflects transactions
- [ ] Recent transactions show updates

---

## TROUBLESHOOTING

### **Issue: "Foreign key constraint failed" during migration**
**Solution:**
1. Check that Users table exists
2. Check that CustomerAccounts table exists
3. Run migration scripts again
4. Verify all previous tables created successfully

### **Issue: Compilation error "Employee not found"**
**Solution:**
1. Verify `Models/InvoiceModels.cs` exists
2. Check that Employee class is in `Models/AuthModels.cs`
3. Verify using statements at top of files
4. Rebuild solution

### **Issue: "DbSet<Invoice> not found"**
**Solution:**
1. Open `Data/BFASDbContext.cs`
2. Add: `public DbSet<Invoice> Invoices { get; set; }`
3. Add: `public DbSet<Employee> Employees { get; set; }`
4. Rebuild solution

### **Issue: Account numbers are NULL**
**Solution:**
1. Run COMPLETE_MIGRATION_Script.sql again
2. Check that script executed successfully
3. Verify account numbers updated:
   ```sql
   SELECT COUNT(*) FROM CustomerAccounts WHERE AccountNumber IS NULL;
   ```

### **Issue: Invoice not generating**
**Solution:**
1. Check Invoices table exists: `SELECT * FROM Invoices;`
2. Check TellerBankingService has GenerateInvoiceAsync method
3. Check DbContext has Invoices DbSet
4. Check application logs for errors

### **Issue: Customer dashboard not updating**
**Solution:**
1. Refresh page (F5)
2. Log out and log back in
3. Check database transaction was committed
4. Check application logs

---

## ✅ FINAL CHECKLIST

Before declaring deployment complete:

- [ ] Database migration scripts executed successfully
- [ ] All tables created (Employees, Invoices)
- [ ] All foreign keys created
- [ ] Account numbers auto-generated
- [ ] Solution compiles without errors
- [ ] Teller can log in
- [ ] Teller can process deposits
- [ ] Teller can process withdrawals
- [ ] Invoices are generated
- [ ] Invoices can be downloaded
- [ ] Customer dashboard updates
- [ ] Balance reflects transactions
- [ ] All verification queries return results
- [ ] No errors in application logs

---

## 🎉 DEPLOYMENT COMPLETE!

If all steps completed successfully, your system is ready for production use!

**Summary of Changes:**
- ✅ Employee table created and linked to Users
- ✅ Invoices table created for transaction receipts
- ✅ Account numbers auto-generated
- ✅ Teller can process deposits with invoices
- ✅ Teller can process withdrawals with invoices
- ✅ Customer dashboard updates in real-time
- ✅ All transactions tracked and invoiced

---

**Need Help?**
- Check IMPLEMENTATION_GUIDE.md for detailed information
- Check DEPLOYMENT_SUMMARY.md for overview
- Check database migration scripts for SQL details
- Check TellerBankingService for code implementation

**Status:** ✅ READY FOR PRODUCTION

**Date:** 2025-01-19

**Version:** 1.0
