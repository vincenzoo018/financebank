# Employee-Transaction Relationship Implementation

## Overview
Changed from storing employee name as a string to storing employee ID as a foreign key. This allows:
- ✅ Joining with Employees table to get the teller/employee name
- ✅ Referential integrity (can't reference non-existent employees)
- ✅ Audit trail (track which employee processed each transaction)
- ✅ Data consistency (no duplicate/misspelled names)

## Architecture

### Before (String-Based)
```
CustomerTransaction
├── ProcessedBy: "Teller" (string)
└── ProcessedByEmployeeName: "John Doe" (string)
    ❌ Duplicate data
    ❌ No referential integrity
    ❌ Prone to typos
```

### After (Foreign Key-Based)
```
CustomerTransaction
├── ProcessedByEmployeeId: 5 (FK)
│   └── JOIN Employees WHERE EmployeeId = 5
│       └── Get: FirstName, LastName, Department, etc.
✅ Single source of truth
✅ Referential integrity
✅ Consistent data
```

## Database Schema

### CustomerTransactions Table (Updated)
```sql
CREATE TABLE [CustomerTransactions]
(
    [TransactionId]              BIGINT IDENTITY(1,1) PRIMARY KEY,
    [TransactionNumber]          NVARCHAR(50) NOT NULL,
    [AccountId]                  INT NOT NULL (FK),
    [TransactionType]            NVARCHAR(50) NOT NULL,
    [Amount]                     DECIMAL(18,2) NOT NULL,
    [Fee]                        DECIMAL(18,2) DEFAULT 0,
    [Status]                     NVARCHAR(50) NOT NULL,
    [Description]                NVARCHAR(500) NULL,
    [Reference]                  NVARCHAR(100) NULL,
    [ToAccountId]                INT NULL (FK),
    [ToAccountName]              NVARCHAR(100) NULL,
    [BillerName]                 NVARCHAR(100) NULL,
    [BillerAccountNumber]        NVARCHAR(50) NULL,
    [CreatedAt]                  DATETIME2(0) NOT NULL,
    [ProcessedAt]                DATETIME2(0) NULL,
    [ProcessedByEmployeeId]      INT NULL (FK → Employees),  ← NEW
    -- REMOVED: ProcessedBy, ProcessedByEmployeeName
);
```

### Employees Table (Reference)
```sql
CREATE TABLE [Employees]
(
    [EmployeeId]                 INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]                     INT NOT NULL (FK → Users),
    [FirstName]                  NVARCHAR(50) NOT NULL,
    [LastName]                   NVARCHAR(50) NOT NULL,
    [Department]                 NVARCHAR(50) NOT NULL,
    [Position]                   NVARCHAR(50) NULL,
    [IsActive]                   BIT DEFAULT 1,
    [CreatedAt]                  DATETIME2(0) NOT NULL
);
```

## C# Model Changes

### CustomerTransaction Model (Updated)
```csharp
[Table("CustomerTransactions")]
public class CustomerTransaction
{
    [Key]
    public int TransactionId { get; set; }
    
    [Required, MaxLength(50)]
    public string TransactionNumber { get; set; } = "";
    
    [Required]
    public int AccountId { get; set; }
    
    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = "";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; } = 0;
    
    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    public int? ToAccountId { get; set; }
    [MaxLength(100)]
    public string? ToAccountName { get; set; }
    
    [MaxLength(100)]
    public string? BillerName { get; set; }
    [MaxLength(50)]
    public string? BillerAccountNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }
    
    public int? ProcessedByEmployeeId { get; set; }  // ← FK to Employees
    
    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
    
    [ForeignKey("ToAccountId")]
    public virtual CustomerAccount? ToAccount { get; set; }
    
    [ForeignKey("ProcessedByEmployeeId")]
    public virtual Employee? ProcessedByEmployee { get; set; }  // ← NEW
}
```

## Service Implementation

### TellerBankingService (Updated)

#### GetEmployeeIdForCurrentUserAsync()
```csharp
private async Task<int?> GetEmployeeIdForCurrentUserAsync()
{
    try
    {
        if (!_authService.IsAuthenticated || !_authService.CurrentUserId.HasValue)
            return null;

        var userId = _authService.CurrentUserId.Value;

        // Join Users → Employees to get EmployeeId
        var employee = await _context.Employees
            .Where(e => e.UserId == userId)
            .Select(e => e.EmployeeId)
            .FirstOrDefaultAsync();

        return employee > 0 ? employee : null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting employee ID: {ex.Message}");
        return null;
    }
}
```

#### ProcessDepositAsync() (Updated)
```csharp
try
{
    decimal balanceBefore = account.Balance;
    
    // Update account balance
    account.Balance += amount;
    account.AvailableBalance += amount;
    account.LastTransactionAt = DateTime.Now;
    
    string receiptNumber = GenerateReceiptNumber("DEP");
    
    // Get employee ID for current teller
    int? employeeId = await GetEmployeeIdForCurrentUserAsync();
    
    // Create transaction with ProcessedByEmployeeId
    var customerTransaction = new CustomerTransaction
    {
        AccountId = account.AccountId,
        TransactionNumber = receiptNumber,
        TransactionType = "Deposit",
        Amount = amount,
        Fee = 0,
        Status = "Completed",
        Description = $"Deposit via {depositMethod}. {notes}",
        Reference = reference ?? GenerateReference("DEPREF"),
        CreatedAt = DateTime.Now,
        ProcessedAt = DateTime.Now,
        ProcessedByEmployeeId = employeeId  // ← FK instead of name
    };
    
    _context.CustomerTransactions.Add(customerTransaction);
    await _context.SaveChangesAsync();
    
    // Create invoice
    await _invoiceService.CreateDepositInvoiceAsync(...);
    
    await transaction.CommitAsync();
    return (true, $"Deposit of ₱{amount:N2} processed successfully", customerTransaction, receiptNumber);
}
```

## Querying Employee Name

### Method 1: Include Navigation Property
```csharp
var transaction = await _context.CustomerTransactions
    .Include(ct => ct.ProcessedByEmployee)
    .FirstOrDefaultAsync(ct => ct.TransactionId == id);

string employeeName = transaction.ProcessedByEmployee?.FirstName + " " + 
                      transaction.ProcessedByEmployee?.LastName;
```

### Method 2: Join with Employees Table
```csharp
var transactions = await _context.CustomerTransactions
    .Join(
        _context.Employees,
        ct => ct.ProcessedByEmployeeId,
        e => e.EmployeeId,
        (ct, e) => new
        {
            ct.TransactionId,
            ct.TransactionNumber,
            ct.Amount,
            ct.Status,
            EmployeeName = e.FirstName + " " + e.LastName,
            ct.CreatedAt
        }
    )
    .Where(x => x.TransactionId == id)
    .ToListAsync();
```

### Method 3: Select with Navigation
```csharp
var transactions = await _context.CustomerTransactions
    .Where(ct => ct.AccountId == accountId)
    .Select(ct => new
    {
        ct.TransactionId,
        ct.TransactionNumber,
        ct.Amount,
        ct.Status,
        EmployeeName = ct.ProcessedByEmployee.FirstName + " " + 
                       ct.ProcessedByEmployee.LastName,
        ct.CreatedAt
    })
    .OrderByDescending(ct => ct.CreatedAt)
    .ToListAsync();
```

## Data Flow

### Deposit Processing
```
1. Teller logs in (UserId = 5)
   ↓
2. TellerBankingService.ProcessDepositAsync() called
   ↓
3. GetEmployeeIdForCurrentUserAsync() executes:
   - Query: SELECT EmployeeId FROM Employees WHERE UserId = 5
   - Result: EmployeeId = 12
   ↓
4. Create CustomerTransaction with ProcessedByEmployeeId = 12
   ↓
5. Save to database
   ↓
6. When displaying transaction:
   - Query: SELECT * FROM CustomerTransactions WHERE TransactionId = X
   - Include: ProcessedByEmployee
   - Display: "Processed by: John Doe" (from Employees table)
```

## Benefits

✅ **Referential Integrity**
- Can't set ProcessedByEmployeeId to non-existent employee
- Database enforces FK constraint

✅ **Data Consistency**
- Single source of truth (Employees table)
- No duplicate/misspelled names

✅ **Audit Trail**
- Track exactly which employee processed each transaction
- Can query all transactions by employee

✅ **Flexibility**
- Can get employee details (department, position, etc.)
- Can filter transactions by department

✅ **Performance**
- Indexed FK lookup
- Efficient joins

## Migration Steps

### 1. Run Database Migration
```bash
sqlcmd -S localhost\SQLEXPRESS -d BFASdatabase -i "Database/ADD_PROCESSEDBYEMPLOYEENAME_COLUMN.sql"
```

This script:
- ✅ Adds ProcessedByEmployeeId column
- ✅ Removes ProcessedBy column
- ✅ Removes ProcessedByEmployeeName column

### 2. Rebuild Solution
```bash
dotnet build
```

### 3. Test Deposits/Withdrawals
- Log in as Teller
- Process deposit/withdrawal
- Verify no errors
- Check transaction history shows employee name

## Example Queries

### Get Transaction with Employee Name
```sql
SELECT 
    ct.TransactionId,
    ct.TransactionNumber,
    ct.TransactionType,
    ct.Amount,
    ct.Status,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    e.Department,
    ct.CreatedAt
FROM CustomerTransactions ct
LEFT JOIN Employees e ON ct.ProcessedByEmployeeId = e.EmployeeId
WHERE ct.AccountId = 1
ORDER BY ct.CreatedAt DESC;
```

### Get All Transactions by Specific Teller
```sql
SELECT 
    ct.TransactionId,
    ct.TransactionNumber,
    ct.TransactionType,
    ct.Amount,
    ca.AccountNumber,
    ca.Customer.FullName AS CustomerName,
    ct.CreatedAt
FROM CustomerTransactions ct
JOIN Employees e ON ct.ProcessedByEmployeeId = e.EmployeeId
JOIN CustomerAccounts ca ON ct.AccountId = ca.AccountId
WHERE e.EmployeeId = 12
ORDER BY ct.CreatedAt DESC;
```

### Get Transactions by Department
```sql
SELECT 
    ct.TransactionId,
    ct.TransactionNumber,
    ct.TransactionType,
    ct.Amount,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    e.Department,
    ct.CreatedAt
FROM CustomerTransactions ct
JOIN Employees e ON ct.ProcessedByEmployeeId = e.EmployeeId
WHERE e.Department = 'Teller'
ORDER BY ct.CreatedAt DESC;
```

## Summary

✅ **Model Updated** - ProcessedByEmployeeId added, ProcessedBy/ProcessedByEmployeeName removed
✅ **Service Updated** - GetEmployeeIdForCurrentUserAsync() joins Users→Employees
✅ **Migration Ready** - Script adds FK column, removes old columns
✅ **Referential Integrity** - FK constraint ensures valid employees only
✅ **Flexible Querying** - Can join/include to get employee details

**Status: READY FOR MIGRATION & TESTING** ✅
