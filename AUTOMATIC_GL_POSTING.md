# Automatic General Ledger Posting System

## Overview

This document describes the implementation of automatic General Ledger (GL) posting for all cash-related transactions in the FinanceBank system. All transactions are now automatically recorded in the General Ledger using double-entry accounting principles in real-time.

## Transaction Types Covered

The following transactions automatically post to the General Ledger:

### 1. Customer Deposits
**Accounting Entry:**
- **Debit**: Cash on Hand (1010) - Asset increases
- **Credit**: Customer Deposits (2010) - Liability increases

### 2. Customer Withdrawals
**Accounting Entry:**
- **Debit**: Customer Deposits (2010) - Liability decreases
- **Credit**: Cash on Hand (1010) - Asset decreases

### 3. Fund Transfers (Fee portion only)
**Accounting Entry:**
- **Debit**: Customer Deposits (2010) - Fee deducted from customer
- **Credit**: Service Fee Income (4020) - Revenue recognized

### 4. Bill Payments
**Accounting Entry:**
- **Debit**: Customer Deposits (2010) - Liability decreases (customer pays)
- **Credit**: Cash on Hand (1010) - Asset decreases (cash paid to biller)

### 5. Loan Disbursements
**Accounting Entry:**
- **Debit**: Loans Receivable (1110) - Asset increases (bank has a receivable)
- **Credit**: Cash on Hand (1010) - Asset decreases (cash given to customer)

### 6. Loan Payments
**Accounting Entry:**
- **Debit**: Cash on Hand (1010) - Asset increases (cash received)
- **Credit**: Loans Receivable (1110) - Principal portion (asset decreases)
- **Credit**: Interest Income (4010) - Interest portion (revenue recognized)
- **Credit**: Penalty Income (4030) - Penalty portion if applicable (revenue recognized)

## Chart of Accounts

The following accounts are required for automatic GL posting:

| Code | Account Name | Type | Normal Balance |
|------|-------------|------|----------------|
| 1010 | Cash on Hand | Asset | Debit |
| 1100 | Cash in Bank | Asset | Debit |
| 1110 | Loans Receivable | Asset | Debit |
| 1200 | Accounts Receivable | Asset | Debit |
| 2000 | Accounts Payable | Liability | Credit |
| 2010 | Customer Deposits | Liability | Credit |
| 3000 | Common Stock | Equity | Credit |
| 3100 | Retained Earnings | Equity | Credit |
| 4010 | Interest Income | Revenue | Credit |
| 4020 | Service Fee Income | Revenue | Credit |
| 4030 | Penalty Income | Revenue | Credit |
| 4040 | Loan Processing Fee Income | Revenue | Credit |
| 5000 | Salaries Expense | Expense | Debit |
| 5010 | Bill Payment Expense | Expense | Debit |
| 5020 | Interest Expense | Expense | Debit |

## Implementation Details

### Services Modified

1. **AutomaticGLPostingService** (NEW)
   - Location: `Services/AutomaticGLPostingService.cs`
   - Purpose: Centralized GL posting logic for all transaction types
   - Methods:
     - `PostDepositAsync()`
     - `PostWithdrawalAsync()`
     - `PostTransferAsync()`
     - `PostBillPaymentAsync()`
     - `PostLoanDisbursementAsync()`
     - `PostLoanPaymentAsync()`
     - `PostLoanProcessingFeeAsync()`
     - `EnsureChartOfAccountsAsync()` - Ensures required accounts exist

2. **CustomerBankingService**
   - Modified to call AutomaticGLPostingService after deposits, withdrawals, transfers, and bill payments

3. **TellerBankingService**
   - Modified to call AutomaticGLPostingService after teller-processed deposits and withdrawals

4. **LoanPaymentService**
   - Modified to call AutomaticGLPostingService after loan payments

5. **LoanProcessService**
   - Modified to call AutomaticGLPostingService after loan disbursements

### Database Tables Used

1. **JournalEntries** - Stores journal entry headers
2. **GeneralLedger** - Stores individual GL entries (debits and credits)
3. **ChartOfAccounts** - Stores account definitions and current balances

### Journal Entry Format

Each transaction creates:
1. A **JournalEntry** header with:
   - Unique journal number (format: `JE-{TYPE}-{YYYYMMDD}-{SEQUENCE}`)
   - Transaction date
   - Description
   - Reference (links to original transaction)
   - Total debits and credits (must be equal)
   - Status: "Posted"

2. Multiple **GeneralLedgerEntry** records (one for each debit/credit) with:
   - Entry number (format: `GL-{TYPE}-{TRANSACTION_ID}`)
   - Account code, name, and type
   - Debit or credit amount
   - Running balance
   - Reference and description
   - Status: "Posted"

## Setup Instructions

### 1. Run the Chart of Accounts Setup Script

Execute the SQL script to ensure all required accounts exist:

```sql
-- Run this in SQL Server Management Studio
USE BFASdatabase;
GO

-- Execute the setup script
:r "Database/CHART_OF_ACCOUNTS_GL_SETUP.sql"
```

Or run manually from the Database folder:
```
Database/CHART_OF_ACCOUNTS_GL_SETUP.sql
```

### 2. Verify Service Registration

The `AutomaticGLPostingService` is registered in `MauiProgram.cs`:

```csharp
// Register Automatic GL Posting Service (real-time General Ledger posting)
builder.Services.AddScoped<AutomaticGLPostingService>();
```

### 3. Test the Implementation

1. Log in as a Teller or Customer
2. Process a deposit or withdrawal
3. Log in as Accountant
4. Navigate to General Ledger page
5. Verify the transaction appears with correct debits and credits

## Viewing General Ledger

### Accountant Dashboard
- Navigate to: **Accountant → General Ledger**
- View all GL entries sorted by date
- Filter by account code or date range
- View account balances summary

### Admin Dashboard
- Navigate to: **Admin → Accounting → General Ledger**
- Full access to all accounting data

## Error Handling

- GL posting failures do NOT affect the original transaction
- All GL posting is wrapped in try-catch blocks
- Failed GL entries can be manually corrected by Accountant
- System logs GL posting failures for review

## Double-Entry Verification

The system ensures:
1. Every journal entry has equal debits and credits
2. Account types are correctly applied:
   - Assets and Expenses have normal Debit balances
   - Liabilities, Equity, and Revenue have normal Credit balances
3. Running balances are calculated per account

## Integration with Dual-Write Sync

When Dual-Write is enabled:
1. Original transactions sync to both LOCAL and CLOUD databases
2. GL entries also sync to both databases automatically
3. Both databases maintain consistent accounting records

---

**Last Updated:** $(Get-Date -Format "yyyy-MM-dd")
**Version:** 1.0
**Author:** System Implementation
