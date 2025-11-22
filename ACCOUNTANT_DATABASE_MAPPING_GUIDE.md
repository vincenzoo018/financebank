# ACCOUNTANT MODULE - DATABASE MAPPING & SCHEMA GUIDE

**FinanceBank - FINSYS Accounting System**  
**Complete Database Reference for Accountant Operations**

---

## TABLE OF CONTENTS

1. [Overview](#overview)
2. [Core Accounting Tables](#core-accounting-tables)
3. [Supporting Reference Tables](#supporting-reference-tables)
4. [Integration Tables](#integration-tables)
5. [Data Flow Diagrams](#data-flow-diagrams)
6. [Query Reference](#query-reference)
7. [Accounting Cycle Support](#accounting-cycle-support)

---

## OVERVIEW

### Accounting Data Model

The FinanceBank system uses a relational database structure that supports the complete accounting cycle:

```
SOURCE TRANSACTIONS (Banking Operations)
        ↓
    JOURNAL ENTRIES (Recording)
        ↓
    GENERAL LEDGER (Maintenance)
        ↓
    TRIAL BALANCE (Validation)
        ↓
    FINANCIAL STATEMENTS (Reporting)
        ↓
    ACCOUNTING REPORTS (Analysis)
```

### Key Principle

**Every accounting action flows through Journal Entries and into the General Ledger. The GL is the single source of truth for all financial data.**

---

## CORE ACCOUNTING TABLES

### 1. JournalEntries

**Purpose:** Store all journal entries with header-level information

**Table Definition:**
```sql
CREATE TABLE JournalEntries (
    JournalId INT IDENTITY(1,1) PRIMARY KEY,
    JournalNumber NVARCHAR(50) NOT NULL UNIQUE,
    TransactionDate DATE NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Reference NVARCHAR(100) NULL,
    TotalDebit DECIMAL(18,2) NOT NULL,
    TotalCredit DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,          -- Draft, Posted, Reversed
    CreatedAt DATETIME NOT NULL,
    CreatedBy NVARCHAR(50) NOT NULL,
    PostedAt DATETIME NULL,
    PostedBy NVARCHAR(50) NULL
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| JournalId | Auto-increment PK | 1001 |
| JournalNumber | Human-readable reference | JE-20251122-001 |
| TransactionDate | Date of event (not current date) | 2025-11-22 |
| Description | What happened | "Daily customer deposits reconciliation" |
| Reference | Supporting doc link | "Bank deposit slip #4521" |
| TotalDebit | Sum of all debit amounts | 10000.00 |
| TotalCredit | Sum of all credit amounts | 10000.00 |
| Status | Current state | "Posted" |
| CreatedAt | When entry created | 2025-11-22 09:15:00 |
| CreatedBy | Accountant name | "jsmith" |
| PostedAt | When posted to GL | 2025-11-22 09:30:00 |
| PostedBy | Accountant who posted | "jsmith" |

**Key Constraints:**
- TotalDebit MUST equal TotalCredit (accounting equation)
- JournalNumber must be UNIQUE
- Status: Draft → Posted → (optionally Reversed)
- Cannot modify a Posted entry (must reverse instead)

**Usage Scenarios:**

```
Scenario 1: Recording a Customer Deposit
────────────────────────────────────────
INSERT INTO JournalEntries (
    JournalNumber, TransactionDate, Description, Reference,
    TotalDebit, TotalCredit, Status, CreatedAt, CreatedBy
) VALUES (
    'JE-20251122-001',
    '2025-11-22',
    'Customer deposit recorded',
    'Deposit slip #1234',
    5000.00, 5000.00,
    'Draft',
    GETDATE(),
    'jsmith'
);
-- Status = Draft until reviewed

-- Review and Post Entry
UPDATE JournalEntries
SET Status = 'Posted', PostedAt = GETDATE(), PostedBy = 'jsmith'
WHERE JournalId = 1001;
-- Now GL will be updated with entry details


Scenario 2: Error Correction
────────────────────────────────
-- Original incorrect entry (posted)
INSERT INTO JournalEntries VALUES (...)
UPDATE JournalEntries SET Status = 'Posted' WHERE JournalId = 1001;

-- Create reversing entry
INSERT INTO JournalEntries (
    JournalNumber, TransactionDate, Description, Reference,
    TotalDebit, TotalCrebit, Status, CreatedAt, CreatedBy
) VALUES (
    'JE-20251122-999',
    '2025-11-22',
    'Reversal of JE-1001 - incorrect amount',
    'JE-1001',
    5000.00, 5000.00,  -- REVERSED amounts
    'Draft',
    GETDATE(),
    'jsmith'
);

-- Both entries remain (audit trail) but net effect cancels
```

**Relationships:**
- `1 JournalEntry : Many JournalEntryLines`
- `1 JournalEntry : Many GeneralLedgerTransactions`

---

### 2. JournalEntryLines

**Purpose:** Store detail lines for each journal entry (the actual debits/credits)

**Table Definition:**
```sql
CREATE TABLE JournalEntryLines (
    LineId INT IDENTITY(1,1) PRIMARY KEY,
    JournalId INT NOT NULL,                -- FK to JournalEntries
    AccountCode NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DebitAmount DECIMAL(18,2) NOT NULL,   -- Default 0
    CreditAmount DECIMAL(18,2) NOT NULL,  -- Default 0
    FOREIGN KEY (JournalId) REFERENCES JournalEntries(JournalId)
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| LineId | Unique line identifier | 50001 |
| JournalId | Parent journal entry | 1001 |
| AccountCode | GL account code | "1010" |
| AccountName | Account description | "Bank Account" |
| Description | Optional detail | "Deposit from Customer ABC" |
| DebitAmount | Amount if debit | 5000.00 |
| CreditAmount | Amount if credit | 0.00 |

**Key Rules:**
- Each line has EITHER a debit OR a credit, NOT both
- Sum of DebitAmounts = Sum of CreditAmounts (for the parent entry)
- At least 2 lines per journal entry (debit and credit sides)

**Example: Customer Deposit of $5,000**

```
JournalId: 1001
JournalNumber: JE-20251122-001
Status: Posted

LineId   AccountCode  AccountName              DebitAmount   CreditAmount
─────────────────────────────────────────────────────────────────────────
50001    1010         Bank Account             5000.00       0.00
50002    2100         Customer Deposits Liab.  0.00          5000.00

Effect: 
  Bank Account (Asset) increases by $5,000
  Customer Deposits (Liability) increases by $5,000
  (Bank holds the customer's money)
```

**Relationships:**
- `JournalId` → JournalEntries (Many-to-One)
- Details used to update GL accounts

---

### 3. GeneralLedger

**Purpose:** Master account list with current balances

**Table Definition:**
```sql
CREATE TABLE GeneralLedger (
    LedgerId INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(50) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,    -- Asset, Liability, etc.
    ParentAccountCode NVARCHAR(50) NULL,  -- For hierarchy
    Balance DECIMAL(18,2) NOT NULL,       -- Current balance
    IsActive BIT NOT NULL                 -- Can post to this account?
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| LedgerId | Unique GL account | 1 |
| AccountCode | Code for GL | "1010" |
| AccountName | Description | "Bank Account - Checking" |
| AccountType | Classification | "Asset" |
| ParentAccountCode | For reporting hierarchy | "1000" |
| Balance | Current balance | 1005000.00 |
| IsActive | Can post to account? | 1 (yes) |

**Chart of Accounts Structure:**

```
1000 - ASSETS
  1010 - Bank Accounts                    Balance: $1,005,000
    1011 - Checking                       Balance: $600,000
    1012 - Savings                        Balance: $405,000
  1020 - Investments                      Balance: $200,000
  1030 - Loan Receivables                 Balance: $450,000
  1040 - Cash in Tills                    Balance: $50,000
    Total Assets                          Balance: $1,705,000

2000 - LIABILITIES
  2100 - Customer Deposits                Balance: $505,000
  2200 - Accounts Payable                 Balance: $125,000
    Total Liabilities                     Balance: $630,000

3000 - EQUITY
  3100 - Capital Stock                    Balance: $800,000
  3200 - Retained Earnings                Balance: $275,000
    Total Equity                          Balance: $1,075,000

4000 - INCOME
  4100 - Interest Income                  Balance: $75,000
  4200 - Service Fees                     Balance: $15,000
    Total Income                          Balance: $90,000

5000 - EXPENSES
  5100 - Salary Expense                   Balance: $35,000
  5200 - Operating Expense                Balance: $10,000
    Total Expenses                        Balance: $45,000
```

**Key Characteristics:**
- One record per GL account
- Balance is ALWAYS current (updated when entries posted)
- IsActive = 0 prevents new postings (but historical data remains)
- Structure supports multi-level reporting

**Relationships:**
- Referenced by JournalEntryLines
- Referenced by GeneralLedgerTransactions
- Parent-child relationships via ParentAccountCode

---

### 4. GeneralLedgerTransactions

**Purpose:** Detailed transaction log for each GL account (running balance)

**Table Definition:**
```sql
CREATE TABLE GeneralLedgerTransactions (
    GLTransactionId INT IDENTITY(1,1) PRIMARY KEY,
    JournalLineId INT NOT NULL,          -- FK to JournalEntryLines
    AccountCode NVARCHAR(50) NOT NULL,
    TransactionDate DATE NOT NULL,
    Description NVARCHAR(500) NULL,
    DebitAmount DECIMAL(18,2) NOT NULL,
    CreditAmount DECIMAL(18,2) NOT NULL,
    RunningBalance DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (JournalLineId) REFERENCES JournalEntryLines(LineId)
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| GLTransactionId | Unique transaction | 100001 |
| JournalLineId | Source journal line | 50001 |
| AccountCode | GL account | "1010" |
| TransactionDate | Date of transaction | 2025-11-22 |
| Description | What happened | "Customer deposit" |
| DebitAmount | Debit side amount | 5000.00 |
| CreditAmount | Credit side amount | 0.00 |
| RunningBalance | Account balance after transaction | 1005000.00 |

**Example GL Subsidiary Ledger:**

```
ACCOUNT: 1010 - Bank Account
═══════════════════════════════════════════════════════════════════════

GLTxnId  Date        Description                  Debit       Credit  Balance
─────────────────────────────────────────────────────────────────────────────
100000   2025-11-21  Opening Balance                                1,000,000
100001   2025-11-22  Customer deposit (JE-001)  5,000               1,005,000
100002   2025-11-22  Interest posted (JE-002)                 100     1,004,900
100003   2025-11-22  Fee charged (JE-003)                      25     1,004,875

Purpose:
  • Audit trail - see all changes to account
  • Analysis - find large transactions
  • Verification - match to journal entries
  • Reconciliation - explain variances
```

**Relationships:**
- Links back to JournalEntryLines via JournalLineId
- One record per journal line posting
- Forms complete audit trail

---

### 5. TrialBalance

**Purpose:** Summary report of all GL accounts for validation

**Table Definition:**
```sql
CREATE TABLE TrialBalance (
    TrialBalanceId INT IDENTITY(1,1) PRIMARY KEY,
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    AccountCode NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    DebitBalance DECIMAL(18,2) NOT NULL,
    CreditBalance DECIMAL(18,2) NOT NULL,
    GeneratedAt DATETIME NOT NULL,
    GeneratedBy NVARCHAR(50) NOT NULL
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| TrialBalanceId | Unique report | 1001 |
| PeriodStart | Period beginning | 2025-11-01 |
| PeriodEnd | Period ending | 2025-11-30 |
| AccountCode | GL account code | "1010" |
| AccountName | Account name | "Bank Account" |
| DebitBalance | Total debits to account | 1005000.00 |
| CreditBalance | Total credits to account | 0.00 |
| GeneratedAt | Report creation time | 2025-11-30 17:00:00 |
| GeneratedBy | Accountant name | "jsmith" |

**Example Trial Balance Report:**

```
TRIAL BALANCE
November 22, 2025

Account Code    Account Name                 Debit           Credit
─────────────────────────────────────────────────────────────────
1010            Bank Accounts             1,005,000
1020            Investments                 200,000
1030            Loan Receivables            450,000
1040            Cash in Tills                50,000
2100            Customer Deposits                        505,000
2200            Accounts Payable                        125,000
3100            Capital Stock                          800,000
3200            Retained Earnings                      275,000
4100            Interest Income                         75,000
5100            Operating Expenses          35,000

                TOTALS                  1,740,000      1,780,000

❌ IMBALANCE! Debits ≠ Credits

Accountant Action:
  → Investigate journal entries
  → Check for posting errors
  → Create correction entry
  → Re-run trial balance
```

**Validation Logic:**
```sql
-- This query validates the trial balance balances
SELECT 
    SUM(DebitBalance) AS TotalDebits,
    SUM(CreditBalance) AS TotalCredits,
    CASE 
        WHEN SUM(DebitBalance) = SUM(CreditBalance) THEN 'BALANCED ✓'
        ELSE 'IMBALANCED ✗'
    END AS Status
FROM TrialBalance
WHERE PeriodEnd = '2025-11-22';
```

---

### 6. FinancialStatements

**Purpose:** Store generated financial statements

**Table Definition:**
```sql
CREATE TABLE FinancialStatements (
    StatementId INT IDENTITY(1,1) PRIMARY KEY,
    StatementType NVARCHAR(50) NOT NULL,  -- BalanceSheet, IncomeStatement, CashFlow
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    StatementData NVARCHAR(MAX) NOT NULL, -- Full statement (JSON)
    GeneratedAt DATETIME NOT NULL,
    GeneratedBy NVARCHAR(50) NOT NULL
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| StatementId | Unique statement | 1001 |
| StatementType | Type of statement | "BalanceSheet" |
| PeriodStart | Period start | 2025-11-01 |
| PeriodEnd | Period end | 2025-11-30 |
| StatementData | Full statement content | JSON object |
| GeneratedAt | Creation date | 2025-11-30 17:30:00 |
| GeneratedBy | Creator | "jsmith" |

**StatementData Example (JSON):**

```json
{
  "balanceSheet": {
    "asOfDate": "2025-11-30",
    "assets": {
      "current": {
        "cashAndEquivalents": 1005000,
        "investments": 200000,
        "loansReceivable": 450000,
        "totalCurrentAssets": 1655000
      },
      "fixed": {
        "equipment": 300000,
        "accumulatedDepreciation": -50000,
        "netFixed": 250000
      },
      "totalAssets": 1905000
    },
    "liabilities": {
      "current": {
        "customerDeposits": 505000,
        "accountsPayable": 125000,
        "totalCurrentLiabilities": 630000
      }
    },
    "equity": {
      "capitalStock": 800000,
      "retainedEarnings": 475000,
      "totalEquity": 1275000
    }
  }
}
```

---

### 7. ChartOfAccounts

**Purpose:** Reference table for all valid GL account codes

**Table Definition:**
```sql
CREATE TABLE ChartOfAccounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(50) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,  -- Asset, Liability, Income, Expense, Equity
    ParentAccountCode NVARCHAR(50) NULL,
    Level INT NOT NULL,                 -- 1=Main, 2=Sub, 3=Detail
    IsActive BIT NOT NULL
);
```

**Column Details:**

| Column | Purpose | Example |
|--------|---------|---------|
| AccountId | Unique ID | 1 |
| AccountCode | Account code used in GL | "1010" |
| AccountName | Full name | "Bank Accounts - Checking" |
| AccountType | Classification | "Asset" |
| ParentAccountCode | Parent for hierarchy | "1000" |
| Level | Hierarchy level | 3 |
| IsActive | Available for posting? | 1 (yes) |

**Hierarchy Example:**

```
Level 1 (Main Category)
1000 ASSETS
  Level 2 (Sub-category)
  1010 Bank Accounts
    Level 3 (Detail)
    1011 Checking Account
    1012 Savings Account
  1020 Investments
  1030 Loan Receivables

2000 LIABILITIES
  2100 Customer Deposits
  2200 Accounts Payable

3000 EQUITY
  3100 Capital Stock
  3200 Retained Earnings
```

**Usage:**
- Accountant uses codes when creating journal entries
- Ensures only valid accounts are used
- InActive accounts cannot receive new postings

---

## SUPPORTING REFERENCE TABLES

### 8. CustomerTransactions

**Purpose:** Source transactions from banking operations

**Key Columns for Accountant:**

| Column | Purpose | Used For |
|--------|---------|----------|
| TransactionId | Unique transaction | Cross-reference |
| AccountId | Customer account | GL posting |
| TransactionType | Deposit, Withdrawal, etc. | Determine GL accounts |
| Amount | Transaction amount | Journal entry amount |
| Fee | Fee charged | Separate GL posting |
| Status | Completed, Pending | Timing of posting |
| CreatedAt | Transaction date | Journal entry date |
| ProcessedAt | When posted to GL | Timing verification |

**Accountant Action:**
```
1. Review daily CustomerTransactions report
2. Verify all Completed transactions
3. Create journal entries for each:
   Debit: Bank Account (increase)
   Credit: Customer Liability (increase)
4. Post to GL when ready
```

---

### 9. FundTransfers

**Purpose:** Track inter-account transfers

**Key Columns for Accountant:**

| Column | Purpose | Used For |
|--------|---------|----------|
| TransferId | Unique transfer | Cross-reference |
| FromAccountId | Source bank account | GL posting (decrease) |
| ToAccountId | Destination account | GL posting (increase) |
| Amount | Transfer amount | Journal entry |
| Fee | Transfer fee | Separate entry |
| Status | Completed, Pending | Timing |
| CreatedAt | Transfer date | Journal entry date |

**Accountant Action:**
```
Transfer of $10,000 from Checking to Savings with $25 fee:

JE-001: Inter-account Transfer
  Debit: Savings Account (1012)              $10,000
  Credit: Checking Account (1011)                    $10,000
  
JE-002: Transfer Fee
  Debit: Bank Service Charge Expense        $25
  Credit: Checking Account (1011)                    $25
```

---

### 10. BankAccounts

**Purpose:** Track bank account balances (must reconcile to GL)

**Key Columns for Accountant:**

| Column | Purpose | Used For |
|--------|---------|----------|
| BankAccountId | Unique account | GL account link |
| AccountNumber | Bank account number | Reference |
| AccountName | Account description | GL account name |
| Balance | Current balance | Reconciliation to GL |

**Reconciliation Process:**

```
BankAccounts Table:
  Account 1010 (Checking)
  Balance: $1,000,000

vs.

GeneralLedger Table:
  Account 1010 (Checking)
  Balance: $1,005,000

Difference: $5,000

Investigation:
  → Customer deposit posted to GL but not yet cleared in bank
  → This is normal (timing difference)
  → Create bank reconciliation entry to track difference
```

---

### 11. CustomerLoans

**Purpose:** Loan portfolio (interest calculation & tracking)

**Key Columns for Accountant:**

| Column | Purpose | Used For |
|--------|---------|----------|
| LoanId | Unique loan | Cross-reference |
| LoanAmount | Original amount | Interest basis |
| OutstandingBalance | Amount still owed | GL posting |
| InterestRate | Annual % rate | Interest calculation |
| MonthlyPayment | Expected payment | Accrual amount |
| NextDueDate | Payment date | Accrual timing |
| Status | Active, Paid Off | Account classification |

**Accountant Action - Monthly Interest Accrual:**

```
For each Active loan:
  Monthly Interest = OutstandingBalance × (InterestRate / 12)
  
Example: Loan of $100,000 at 10% annual
  Monthly Interest = $100,000 × (0.10 / 12) = $833.33
  
Journal Entry:
  Debit: Interest Receivable (1050)         $833.33
  Credit: Interest Income (4100)                     $833.33

This accrues interest even before payment received.
```

---

### 12. CustomerCards

**Purpose:** Card portfolio (fee and interest tracking)

**Key Columns for Accountant:**

| Column | Purpose | Used For |
|--------|---------|----------|
| CardId | Unique card | Cross-reference |
| OutstandingBalance | Amount owed | GL posting |
| InterestRate | Card APR | Interest calculation |
| CreditLimit | Maximum balance | Reporting |
| ExpiryDate | Card expiration | Activity cut-off |

**Accountant Action - Monthly Card Processing:**

```
For each Active card with balance:
  Monthly Interest = OutstandingBalance × (InterestRate / 12)
  Monthly Fee = Fixed fee per card
  
Journal Entries:
  1. Interest on Card Balance
     Debit: Interest Receivable (1060)      [Interest amount]
     Credit: Card Interest Income (4110)              [Amount]
  
  2. Card Annual Fee
     Debit: Accounts Receivable (1040)      [Fee amount]
     Credit: Card Fee Income (4200)                   [Amount]
```

---

## INTEGRATION TABLES

### 13. AuditLogs

**Purpose:** Record all system access and modifications

**Table Definition:**
```sql
CREATE TABLE AuditLogs (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,         -- Create, Update, Delete, Post
    Module NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL
);
```

**Accountant Usage:**

```
Query: Who modified which journal entry?

SELECT 
    u.Username,
    a.Action,
    a.Module,
    a.Description,
    a.CreatedAt
FROM AuditLogs a
JOIN Users u ON a.UserId = u.UserId
WHERE a.Module = 'JournalEntries'
  AND a.CreatedAt >= DATEADD(MONTH, -1, GETDATE())
ORDER BY a.CreatedAt DESC;

Result:
  jsmith      | Create     | JournalEntries | Created JE-001 | 2025-11-22 09:15
  jsmith      | Update     | JournalEntries | Posted JE-001  | 2025-11-22 09:30
  msmith      | Update     | JournalEntries | Reversed JE-001| 2025-11-23 08:00
```

---

### 14. RolePermissions

**Purpose:** Control access to modules and functions

**Table Definition:**
```sql
CREATE TABLE RolePermissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    Permission NVARCHAR(50) NOT NULL,      -- Read, Write, Delete, Full
    IsActive BIT NOT NULL
);
```

**Accountant Permissions:**

```sql
SELECT * FROM RolePermissions WHERE RoleName = 'Accountant';

Result:
  RoleName      ModuleName                Permission   IsActive
  ────────────────────────────────────────────────────────────────
  Accountant    JournalEntries            Full         1
  Accountant    TrialBalance              Full         1
  Accountant    FinancialStatements       Full         1
  Accountant    GeneralLedger             Read         1
  Accountant    BankAccounts              Read         1
  Accountant    FundTransfers             Read         1
  Accountant    CustomerTransactions      Read         1
  Accountant    AuditLogs                 Read         1
```

---

## DATA FLOW DIAGRAMS

### Complete Accounting Cycle Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ BANKING OPERATIONS (External Sources)                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  • Customer deposits/withdrawals                           │
│  • Fund transfers                                          │
│  • Loan disbursements/payments                            │
│  • Card transactions & fees                               │
│  • Interest accruals                                      │
│                                                             │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ SOURCE TRANSACTION TABLES                                   │
├─────────────────────────────────────────────────────────────┤
│ CustomerTransactions | FundTransfers | CustomerLoans |      │
│ CustomerCards | BankAccounts                        │
└────────────────────┬────────────────────────────────────────┘
                     ↓
         ┌───────────────────────┐
         │    ACCOUNTANT REVIEW   │
         │  (Manual verification) │
         └───────────┬───────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRIES (Recording)                                 │
├─────────────────────────────────────────────────────────────┤
│ • JournalEntries (header)                                  │
│ • JournalEntryLines (detail lines - debits/credits)        │
│ Status: Draft → Posted                                     │
└────────────────────┬────────────────────────────────────────┘
                     ↓
        ┌──────────────────────────┐
        │ VALIDATION CHECK         │
        │ TotalDebit = TotalCredit? │
        │ (Must balance)           │
        └────────────┬─────────────┘
                     ↓ YES
┌─────────────────────────────────────────────────────────────┐
│ POST TO GENERAL LEDGER (Maintenance)                        │
├─────────────────────────────────────────────────────────────┤
│ • Update GeneralLedger account balances                     │
│ • Create GeneralLedgerTransactions detail records           │
│ • Calculate running balances                               │
│ • Create audit trail entries in AuditLogs                  │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ TRIAL BALANCE (Validation)                                  │
├─────────────────────────────────────────────────────────────┤
│ • Generate TrialBalance report                              │
│ • Verify Debits = Credits                                  │
│ • Investigate imbalances                                   │
│ • Approve when balanced                                    │
└────────────────────┬────────────────────────────────────────┘
                     ↓ BALANCED
┌─────────────────────────────────────────────────────────────┐
│ FINANCIAL STATEMENTS (Reporting)                            │
├─────────────────────────────────────────────────────────────┤
│ • FinancialStatements table                                │
│ • Balance Sheet (GL accounts organized)                     │
│ • Income Statement (Revenue - Expenses)                     │
│ • Cash Flow Statement (Cash movements)                      │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ ACCOUNTING REPORTS & ANALYSIS (Stakeholder Reports)         │
├─────────────────────────────────────────────────────────────┤
│ • GL Detail Reports (by account, date range)                │
│ • Account Analysis (unusual transactions)                   │
│ • Reconciliation Reports (GL vs source)                     │
│ • Compliance Reports (audit trail)                          │
│ • Management Reports (financial metrics)                    │
└─────────────────────────────────────────────────────────────┘
```

---

## QUERY REFERENCE

### Query 1: Daily Transaction Summary

```sql
-- Show all posted journal entries for a date range
SELECT
    je.JournalNumber,
    je.TransactionDate,
    je.Description,
    je.TotalDebit,
    je.TotalCredit,
    je.Status,
    je.CreatedBy,
    je.PostedBy
FROM JournalEntries je
WHERE je.TransactionDate >= '2025-11-22'
  AND je.TransactionDate < '2025-11-23'
  AND je.Status = 'Posted'
ORDER BY je.CreatedAt DESC;
```

### Query 2: Account Transaction Detail

```sql
-- Show all GL transactions for a specific account
SELECT
    gl.GLTransactionId,
    gl.TransactionDate,
    gl.Description,
    gl.DebitAmount,
    gl.CreditAmount,
    gl.RunningBalance
FROM GeneralLedgerTransactions gl
WHERE gl.AccountCode = '1010'  -- Bank Account
  AND gl.TransactionDate >= '2025-11-01'
  AND gl.TransactionDate < '2025-12-01'
ORDER BY gl.TransactionDate, gl.GLTransactionId;
```

### Query 3: Account Balance Reconciliation

```sql
-- Verify GL account balance matches expected
SELECT
    gl.AccountCode,
    gl.AccountName,
    gl.Balance AS GLBalance,
    ba.Balance AS BankAccountBalance,
    (gl.Balance - ba.Balance) AS Difference,
    CASE 
        WHEN gl.Balance = ba.Balance THEN 'Reconciled ✓'
        ELSE 'Difference - Investigate'
    END AS Status
FROM GeneralLedger gl
LEFT JOIN BankAccounts ba ON gl.AccountCode = ba.AccountNumber
WHERE gl.AccountType = 'Asset';
```

### Query 4: Trial Balance Generation

```sql
-- Generate trial balance
SELECT
    gl.AccountCode,
    gl.AccountName,
    gl.AccountType,
    CASE 
        WHEN gl.AccountType IN ('Asset', 'Expense') THEN gl.Balance
        ELSE 0
    END AS DebitBalance,
    CASE 
        WHEN gl.AccountType IN ('Liability', 'Income', 'Equity') THEN gl.Balance
        ELSE 0
    END AS CreditBalance,
    GETDATE() AS GeneratedAt
FROM GeneralLedger gl
WHERE gl.IsActive = 1
ORDER BY gl.AccountCode;

-- Validation
SELECT
    SUM(CASE WHEN gl.AccountType IN ('Asset', 'Expense') THEN gl.Balance ELSE 0 END) AS TotalDebits,
    SUM(CASE WHEN gl.AccountType IN ('Liability', 'Income', 'Equity') THEN gl.Balance ELSE 0 END) AS TotalCredits
FROM GeneralLedger gl
WHERE gl.IsActive = 1;
```

### Query 5: Unposted/Draft Entries

```sql
-- Show entries waiting for posting
SELECT
    je.JournalNumber,
    je.TransactionDate,
    je.Description,
    je.TotalDebit,
    je.TotalCredit,
    je.Status,
    je.CreatedBy,
    CASE 
        WHEN je.TotalDebit = je.TotalCredit THEN 'Ready to post'
        ELSE 'UNBALANCED - Cannot post'
    END AS PostingStatus
FROM JournalEntries je
WHERE je.Status = 'Draft'
ORDER BY je.CreatedAt DESC;
```

### Query 6: Audit Trail for an Entry

```sql
-- Show all changes made to a specific journal entry
SELECT
    a.AuditId,
    u.Username,
    a.Action,
    a.OldValues,
    a.NewValues,
    a.CreatedAt
FROM AuditLogs a
JOIN Users u ON a.UserId = u.UserId
WHERE a.Module = 'JournalEntries'
  AND a.Description LIKE '%JE-20251122-001%'
ORDER BY a.CreatedAt DESC;
```

### Query 7: Interest Accrual Calculation

```sql
-- Calculate monthly interest accruals needed
SELECT
    cl.LoanId,
    cl.LoanNumber,
    cl.OutstandingBalance,
    cl.InterestRate,
    (cl.OutstandingBalance * (cl.InterestRate / 100 / 12)) AS MonthlyInterest
FROM CustomerLoans cl
WHERE cl.Status = 'Active'
  AND MONTH(cl.NextDueDate) = MONTH(GETDATE())
  AND YEAR(cl.NextDueDate) = YEAR(GETDATE());
```

### Query 8: Month-End Close Checklist

```sql
-- Verify all period-end requirements
SELECT
    'Unposted Entries' AS CheckItem,
    COUNT(*) AS Count
FROM JournalEntries
WHERE Status = 'Draft'
  AND TransactionDate < CAST(GETDATE() AS DATE)

UNION ALL

SELECT
    'Unbalanced Trial Balance',
    CASE 
        WHEN (SELECT SUM(CASE WHEN AccountType IN ('Asset', 'Expense') THEN Balance ELSE 0 END) FROM GeneralLedger)
            = (SELECT SUM(CASE WHEN AccountType IN ('Liability', 'Income', 'Equity') THEN Balance ELSE 0 END) FROM GeneralLedger)
        THEN 0
        ELSE 1
    END

UNION ALL

SELECT
    'Unreconciled Bank Accounts',
    COUNT(*)
FROM GeneralLedger gl
LEFT JOIN BankAccounts ba ON gl.AccountCode = ba.AccountNumber
WHERE gl.AccountType = 'Asset'
  AND gl.Balance <> ba.Balance;
```

---

## ACCOUNTING CYCLE SUPPORT

### How Each Table Supports the Cycle

```
PHASE 1: RECORDING (JournalEntries + JournalEntryLines)
────────────────────────────────────────────────────────
Input:  Banking operations data
Action: Accountant records as journal entries
Output: Draft entries awaiting posting
        
Tables: 
  - JournalEntries (header info, status tracking)
  - JournalEntryLines (detail debits/credits)


PHASE 2: POSTING (JournalEntries → GeneralLedger)
──────────────────────────────────────────────────
Input:  Approved journal entries
Action: Post to GL, update account balances
Output: Updated GL account balances, GL transactions
        
Tables:
  - JournalEntries (status = Posted)
  - GeneralLedger (account balances updated)
  - GeneralLedgerTransactions (detail posted)
  - AuditLogs (audit trail created)


PHASE 3: VALIDATION (TrialBalance)
──────────────────────────────────
Input:  Posted GL entries
Action: Generate TB, verify balancing
Output: Approved TB or corrections needed
        
Tables:
  - GeneralLedger (account balances)
  - TrialBalance (TB report)
  - JournalEntries (if corrections needed)


PHASE 4: REPORTING (FinancialStatements)
────────────────────────────────────────
Input:  Approved trial balance
Action: Generate financial statements
Output: Balance sheet, income statement, cash flow
        
Tables:
  - GeneralLedger (balances used for statements)
  - FinancialStatements (statements archived)


PHASE 5: ANALYSIS (Accounting Reports)
──────────────────────────────────────
Input:  GL transactions and statements
Action: Generate various analysis reports
Output: Management reports, compliance reports
        
Tables:
  - GeneralLedgerTransactions (detail analysis)
  - FinancialStatements (comparison)
  - AuditLogs (compliance verification)
  - SystemReports (report archival)
```

---

## SUMMARY

**The Accountant's Database**

| Phase | Primary Tables | Purpose |
|-------|---|---|
| **Recording** | JournalEntries, JournalEntryLines | Create entries for transactions |
| **Reference** | ChartOfAccounts | Verify account codes |
| **Posting** | GeneralLedger, GeneralLedgerTransactions | Maintain account balances |
| **Validation** | TrialBalance | Verify debits = credits |
| **Reporting** | FinancialStatements, SystemReports | Generate statements and reports |
| **Verification** | AuditLogs, BankAccounts, CustomerLoans | Reconcile and audit |
| **Control** | RolePermissions, Users | Access management |

---

**Document Version:** 1.0  
**Last Updated:** November 22, 2025  
**Database:** BFASdatabase  
**Classification:** Internal Use

