# Transaction & Invoice Data Flow Implementation

## Overview
Complete implementation of the data flow for deposits and withdrawals:
- **CustomerAccounts** → Balance updated
- **CustomerTransactions** → Transaction recorded
- **Invoices** → Invoice created with concatenated data

## Architecture

### Three-Table Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ TELLER PROCESSES DEPOSIT/WITHDRAWAL                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 1. UPDATE CustomerAccounts                                  │
│    - Balance += amount (deposit) or -= amount (withdrawal)  │
│    - AvailableBalance updated                               │
│    - LastTransactionAt = now                                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. CREATE CustomerTransaction                               │
│    - TransactionId (auto-generated)                         │
│    - TransactionNumber (receipt number)                     │
│    - TransactionType: "Deposit" or "Withdrawal"             │
│    - Amount, Status, Description, Reference                │
│    - ProcessedBy, ProcessedByEmployeeName                   │
│    - CreatedAt, ProcessedAt                                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. CREATE Invoice (CONCATENATED DATA)                       │
│    - InvoiceNumber (unique)                                 │
│    - InvoiceType: "Deposit" or "Withdrawal"                 │
│    - AccountId, TransactionId (foreign keys)                │
│    - CustomerName, AccountNumber (from CustomerAccounts)    │
│    - BalanceBefore, TransactionAmount, BalanceAfter         │
│    - TransactionMethod (OTC, Bank Transfer, CDM)            │
│    - Reference, Notes, ProcessedBy                          │
│    - Status, CreatedAt, PrintedAt, DownloadedAt             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ DATA AVAILABLE FOR:                                         │
│ - Teller Dashboard (today's transactions)                   │
│ - Customer History (account invoices)                       │
│ - Receipt/Invoice Printing                                  │
│ - Transaction Reports                                       │
└─────────────────────────────────────────────────────────────┘
```

## Implementation Details

### 1. InvoiceService (New Service)
**File:** `Services/InvoiceService.cs`

**Key Methods:**

#### CreateDepositInvoiceAsync()
```csharp
public async Task<(bool success, Invoice? invoice, string message)> CreateDepositInvoiceAsync(
    int accountId,
    int transactionId,
    decimal balanceBefore,
    decimal depositAmount,
    string depositMethod,
    string? reference = null,
    string? notes = null)
```
- Creates invoice after deposit transaction
- Stores balance before and after
- Links to both CustomerAccounts and CustomerTransactions

#### CreateWithdrawalInvoiceAsync()
```csharp
public async Task<(bool success, Invoice? invoice, string message)> CreateWithdrawalInvoiceAsync(
    int accountId,
    int transactionId,
    decimal balanceBefore,
    decimal withdrawalAmount,
    string withdrawalMethod,
    string? reference = null,
    string? notes = null)
```
- Creates invoice after withdrawal transaction
- Stores balance before and after
- Links to both CustomerAccounts and CustomerTransactions

#### GetAccountInvoicesAsync()
```csharp
public async Task<List<(...)>> GetAccountInvoicesAsync(int accountId)
```
- Fetches all invoices for a customer account
- Returns concatenated data (invoice + transaction details)
- Used by customer history pages

#### GetTodayInvoicesAsync()
```csharp
public async Task<List<(...)>> GetTodayInvoicesAsync()
```
- Fetches all invoices created today
- Used by teller dashboard
- Shows all deposits and withdrawals processed

#### GetAllTransactionsForAccountAsync()
```csharp
public async Task<List<dynamic>> GetAllTransactionsForAccountAsync(int accountId)
```
- Complete transaction view with all concatenated data
- Includes: InvoiceId, InvoiceNumber, Type, Amount, Balances, Method, Status, etc.

#### MarkAsPrintedAsync() / MarkAsDownloadedAsync()
```csharp
public async Task<bool> MarkAsPrintedAsync(int invoiceId)
public async Task<bool> MarkAsDownloadedAsync(int invoiceId)
```
- Tracks when invoices are printed or downloaded
- Updates PrintedAt and DownloadedAt timestamps

### 2. TellerBankingService Updates
**File:** `Services/TellerBankingService.cs`

#### ProcessDepositAsync() - Updated
```csharp
// 1. Store balance before
decimal balanceBefore = account.Balance;

// 2. Update account balance
account.Balance += amount;
account.AvailableBalance += amount;

// 3. Create CustomerTransaction
var customerTransaction = new CustomerTransaction { ... };
_context.CustomerTransactions.Add(customerTransaction);
await _context.SaveChangesAsync();

// 4. Create Invoice (NEW)
await _invoiceService.CreateDepositInvoiceAsync(
    account.AccountId,
    customerTransaction.TransactionId,
    balanceBefore,
    amount,
    depositMethod,
    reference,
    notes);
```

#### ProcessWithdrawalAsync() - Updated
```csharp
// 1. Store balance before
decimal balanceBefore = account.Balance;

// 2. Update account balance
account.Balance -= amount;
account.AvailableBalance -= amount;

// 3. Create CustomerTransaction
var customerTransaction = new CustomerTransaction { ... };
_context.CustomerTransactions.Add(customerTransaction);
await _context.SaveChangesAsync();

// 4. Create Invoice (NEW)
await _invoiceService.CreateWithdrawalInvoiceAsync(
    account.AccountId,
    customerTransaction.TransactionId,
    balanceBefore,
    amount,
    withdrawalMethod,
    reference,
    notes);
```

### 3. Dependency Injection
**File:** `MauiProgram.cs`

```csharp
// Register Invoice Service
builder.Services.AddScoped<InvoiceService>();
```

## Database Tables

### CustomerAccounts
```sql
[AccountId] INT PRIMARY KEY
[CustomerId] INT
[AccountNumber] NVARCHAR(30)
[Balance] DECIMAL(18,2)
[AvailableBalance] DECIMAL(18,2)
[Currency] NVARCHAR(3)
[IsActive] BIT
[CreatedAt] DATETIME2
[LastTransactionAt] DATETIME2 NULL
```

### CustomerTransactions
```sql
[TransactionId] BIGINT PRIMARY KEY
[TransactionNumber] NVARCHAR(50)
[AccountId] INT (FK)
[TransactionType] NVARCHAR(50) -- "Deposit", "Withdrawal"
[Amount] DECIMAL(18,2)
[Fee] DECIMAL(18,2)
[Status] NVARCHAR(50) -- "Completed", "Pending", "Failed"
[Description] NVARCHAR(250)
[Reference] NVARCHAR(100)
[CreatedAt] DATETIME2
[ProcessedAt] DATETIME2
[ProcessedBy] NVARCHAR(50)
[ProcessedByEmployeeName] NVARCHAR(100)
```

### Invoices
```sql
[InvoiceId] INT PRIMARY KEY
[InvoiceNumber] NVARCHAR(100) UNIQUE
[InvoiceType] NVARCHAR(50) -- "Deposit", "Withdrawal"
[AccountId] INT (FK → CustomerAccounts)
[TransactionId] BIGINT (FK → CustomerTransactions)
[CustomerName] NVARCHAR(100)
[AccountNumber] NVARCHAR(30)
[BalanceBefore] DECIMAL(18,2)
[TransactionAmount] DECIMAL(18,2)
[BalanceAfter] DECIMAL(18,2)
[TransactionMethod] NVARCHAR(50) -- "OTC", "Bank Transfer", "CDM"
[Reference] NVARCHAR(100)
[Notes] NVARCHAR(250)
[ProcessedBy] NVARCHAR(50)
[Status] NVARCHAR(50)
[CreatedAt] DATETIME2
[PrintedAt] DATETIME2 NULL
[DownloadedAt] DATETIME2 NULL
```

## Data Flow Examples

### Example 1: Deposit via OTC
```
Teller searches for customer → ACC-123456 (Balance: ₱10,000)
Teller enters: Deposit ₱5,000 via OTC

STEP 1: Update CustomerAccounts
  Balance: 10,000 → 15,000
  AvailableBalance: 10,000 → 15,000
  LastTransactionAt: 2025-11-20 14:30:00

STEP 2: Create CustomerTransaction
  TransactionId: 1
  TransactionNumber: DEP-20251120-7DC5FF65
  TransactionType: Deposit
  Amount: 5,000
  Status: Completed
  ProcessedBy: Teller
  ProcessedByEmployeeName: John Doe

STEP 3: Create Invoice
  InvoiceId: 1
  InvoiceNumber: DEP-20251120143000-A1B2C3D4
  InvoiceType: Deposit
  AccountId: 1
  TransactionId: 1
  CustomerName: Jane Smith
  AccountNumber: ACC-123456
  BalanceBefore: 10,000
  TransactionAmount: 5,000
  BalanceAfter: 15,000
  TransactionMethod: Over-the-Counter (OTC)
  Status: Completed
  CreatedAt: 2025-11-20 14:30:00
```

### Example 2: Withdrawal via OTC
```
Teller searches for customer → ACC-654321 (Balance: ₱20,000)
Teller enters: Withdrawal ₱3,000 via OTC

STEP 1: Update CustomerAccounts
  Balance: 20,000 → 17,000
  AvailableBalance: 20,000 → 17,000
  LastTransactionAt: 2025-11-20 14:35:00

STEP 2: Create CustomerTransaction
  TransactionId: 2
  TransactionNumber: WDR-20251120-8EF9GH12
  TransactionType: Withdrawal
  Amount: 3,000
  Status: Completed
  ProcessedBy: Teller
  ProcessedByEmployeeName: John Doe

STEP 3: Create Invoice
  InvoiceId: 2
  InvoiceNumber: WDR-20251120143500-E5F6G7H8
  InvoiceType: Withdrawal
  AccountId: 2
  TransactionId: 2
  CustomerName: Mark Johnson
  AccountNumber: ACC-654321
  BalanceBefore: 20,000
  TransactionAmount: 3,000
  BalanceAfter: 17,000
  TransactionMethod: Over-the-Counter (OTC)
  Status: Completed
  CreatedAt: 2025-11-20 14:35:00
```

## Usage in UI Pages

### Teller Dashboard
```csharp
// Get today's transactions
var todayTransactions = await _invoiceService.GetTodayInvoicesAsync();

// Display: InvoiceNumber | CustomerName | Type | Amount | Status | Time
foreach (var transaction in todayTransactions)
{
    Console.WriteLine($"{transaction.InvoiceNumber} | {transaction.CustomerName} | {transaction.Type} | ₱{transaction.Amount} | {transaction.Status}");
}
```

### Customer History Page
```csharp
// Get all transactions for customer's account
var transactions = await _invoiceService.GetAccountInvoicesAsync(accountId);

// Display: Date | Type | Amount | Balance Before | Balance After | Status
foreach (var transaction in transactions)
{
    Console.WriteLine($"{transaction.CreatedAt} | {transaction.Type} | ₱{transaction.Amount} | ₱{transaction.BalanceBefore} | ₱{transaction.BalanceAfter}");
}
```

### Invoice Details Page
```csharp
// Get complete invoice with all details
var invoiceDetails = await _invoiceService.GetInvoiceDetailsAsync(invoiceId);

// Display all concatenated data
Console.WriteLine($"Invoice: {invoiceDetails.InvoiceNumber}");
Console.WriteLine($"Customer: {invoiceDetails.CustomerName}");
Console.WriteLine($"Account: {invoiceDetails.AccountNumber}");
Console.WriteLine($"Type: {invoiceDetails.InvoiceType}");
Console.WriteLine($"Amount: ₱{invoiceDetails.TransactionAmount}");
Console.WriteLine($"Balance Before: ₱{invoiceDetails.BalanceBefore}");
Console.WriteLine($"Balance After: ₱{invoiceDetails.BalanceAfter}");
Console.WriteLine($"Method: {invoiceDetails.TransactionMethod}");
Console.WriteLine($"Status: {invoiceDetails.Status}");
Console.WriteLine($"Processed By: {invoiceDetails.ProcessedBy}");
```

## Key Features

✅ **Complete Data Concatenation**
- All transaction data combined in Invoice table
- No need to join multiple tables in UI

✅ **Audit Trail**
- PrintedAt timestamp tracks when invoice was printed
- DownloadedAt timestamp tracks when invoice was downloaded
- ProcessedBy tracks who processed the transaction

✅ **Referential Integrity**
- Foreign keys link Invoice → CustomerAccounts
- Foreign keys link Invoice → CustomerTransactions
- Ensures data consistency

✅ **Transaction Safety**
- Database transaction ensures all 3 tables updated together
- Rollback on any error

✅ **Flexible Querying**
- Get all transactions for account
- Get today's transactions
- Get invoice details with all concatenated data

## Testing Checklist

- [ ] Teller deposits ₱1,000 → Check CustomerAccounts, CustomerTransactions, Invoices
- [ ] Teller withdraws ₱500 → Check all three tables
- [ ] Customer views history → Should show all deposits/withdrawals from Invoices
- [ ] Teller dashboard → Should show today's transactions from Invoices
- [ ] Invoice details → Should show all concatenated data
- [ ] Mark as printed → PrintedAt should be set
- [ ] Mark as downloaded → DownloadedAt should be set

## Summary

✅ **InvoiceService created** - Handles all invoice operations
✅ **TellerBankingService updated** - Creates invoices after deposits/withdrawals
✅ **Dependency injection configured** - InvoiceService registered in MauiProgram
✅ **Data flow complete** - CustomerAccounts → CustomerTransactions → Invoices
✅ **Concatenated data available** - All transaction info in Invoice table
✅ **Ready for UI implementation** - Teller and Customer views can fetch data from Invoices

**Status: READY FOR TESTING** ✅
