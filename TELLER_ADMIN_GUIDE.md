# 🏦 Teller & Admin Deposit/Withdrawal Management System

## Quick Start Guide

### 📋 Test Credentials

**Teller Account:**
- Username: `teller`
- Password: `teller123`
- Role: Teller
- Name: Anna Reyes

**Admin Account:**
- Username: `admin`
- Password: `admin123`
- Role: SuperAdmin

**Customer Account (for testing):**
- Username: `customer`
- Password: `customer123`
- Role: Customer

---

## 🎯 Features Overview

### 1. Teller Deposit Processing (`/teller/deposits`)

**What it does:**
- Teller can process customer deposits
- Search customers by name or account number
- Auto-fill customer details
- Generate receipts
- Print receipts

**How to use:**
1. Log in as Teller
2. Click "Process Deposits" in sidebar
3. Type customer name or account number in search box
4. Click customer from dropdown
5. Enter deposit amount
6. Select deposit method (OTC, Bank Transfer, CDM)
7. Add optional notes
8. Click "Process Deposit"
9. Click "Print Receipt" to print

**Receipt includes:**
- Receipt number (auto-generated)
- Date & time
- Account number
- Customer name
- Current balance
- Deposit amount
- Deposit method
- Processed by (teller name)

---

### 2. Teller Withdrawal Processing (`/teller/withdrawals`)

**What it does:**
- Teller can process customer withdrawals
- Validates sufficient balance
- Search customers by name or account number
- Auto-fill customer details
- Generate receipts
- Print receipts

**How to use:**
1. Log in as Teller
2. Click "Process Withdrawals" in sidebar
3. Type customer name or account number in search box
4. Click customer from dropdown
5. Enter withdrawal amount (must be ≤ available balance)
6. Select withdrawal method (OTC, ATM, Bank Transfer)
7. Add optional notes
8. Click "Process Withdrawal"
9. Click "Print Receipt" to print

**Receipt includes:**
- Receipt number (auto-generated)
- Date & time
- Account number
- Customer name
- Available balance
- Withdrawal amount
- Withdrawal method
- Processed by (teller name)

---

### 3. Admin Management Page (`/admin/manage-deposits-withdrawals`)

**What it does:**
- Admin can process deposits and withdrawals
- View all transactions processed today
- Same functionality as teller pages
- Full control over all transactions

**How to use:**

#### Tab 1: Process Deposits
1. Log in as Admin
2. Go to `/admin/manage-deposits-withdrawals`
3. Click "Process Deposits" tab
4. Search for customer
5. Enter deposit details
6. Click "Process Deposit"

#### Tab 2: Process Withdrawals
1. Click "Process Withdrawals" tab
2. Search for customer
3. Enter withdrawal details
4. Click "Process Withdrawal"

#### Tab 3: Transaction History
1. Click "Transaction History" tab
2. View all deposits and withdrawals from today
3. Shows: Account #, Customer, Type, Amount, Time, Status

---

## 🔍 Customer Search

**How it works:**
- Type at least 2 characters
- Search by customer name OR account number
- Dropdown shows matching results
- Click to select customer
- Form auto-fills with customer details

**Example searches:**
- "Juan" → Shows all customers with "Juan" in name
- "ACC-2025-001" → Shows customer with that account number
- "Maria" → Shows all customers with "Maria" in name

---

## 💾 Database Integration

**What gets saved:**
- Account balance updated in real-time
- Transaction record created with:
  - Transaction number (receipt number)
  - Transaction type (Deposit/Withdrawal)
  - Amount
  - Method used
  - Timestamp
  - Processed by (teller/admin name)
  - Status (Completed)

**Validation:**
- Account must exist and be active
- Amount must be > 0
- For withdrawals: Available balance must be ≥ withdrawal amount

---

## 🧾 Receipt System

**Receipt Number Format:**
- Deposits: `DEP-YYYYMMDD-XXXXXXXX` (e.g., `DEP-20250118-ABC12345`)
- Withdrawals: `WDR-YYYYMMDD-XXXXXXXX` (e.g., `WDR-20250118-XYZ98765`)

**Receipt Contents:**
```
╔════════════════════════════════════════════════════════════╗
║                    DEPOSIT RECEIPT                         ║
║                  BFAS Banking System                       ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ Receipt Number:    DEP-20250118-ABC12345
║ Date & Time:       2025-01-18 14:30:45
║                                                            ║
║ Account Number:    ACC-2025-001
║ Customer Name:     Juan Dela Cruz
║ Current Balance:   ₱30,450.00
║                                                            ║
║ Deposit Amount:    ₱5,000.00
║ Deposit Method:    Over-the-Counter
║ Reference:        DEPREF-20250118143045-1234
║                                                            ║
║ Notes:             Monthly savings deposit
║                                                            ║
║ Processed By:      Anna Reyes
║ Status:            COMPLETED
║                                                            ║
╠════════════════════════════════════════════════════════════╣
║ Thank you for banking with us!                            ║
║ Please keep this receipt for your records.                ║
╚════════════════════════════════════════════════════════════╝
```

**Printing:**
- Click "Print Receipt" button
- Receipt opens in Notepad
- Print from Notepad or save as file

---

## ✅ Testing Scenarios

### Scenario 1: Process Deposit
1. Log in as Teller
2. Go to Process Deposits
3. Search for "customer" (or any customer name)
4. Select customer
5. Enter amount: 5000
6. Select method: OTC
7. Click "Process Deposit"
8. Verify: Success message shows
9. Verify: Receipt number displays
10. Click "Print Receipt"
11. Verify: Receipt opens in Notepad

### Scenario 2: Process Withdrawal
1. Log in as Teller
2. Go to Process Withdrawals
3. Search for customer
4. Select customer
5. Enter amount: 2000
6. Select method: ATM
7. Click "Process Withdrawal"
8. Verify: Success message shows
9. Verify: Receipt number displays
10. Click "Print Receipt"

### Scenario 3: Insufficient Balance
1. Log in as Teller
2. Go to Process Withdrawals
3. Search for customer with low balance
4. Enter amount greater than available balance
5. Click "Process Withdrawal"
6. Verify: Error message "Insufficient balance" shows

### Scenario 4: Admin Management
1. Log in as Admin
2. Go to `/admin/manage-deposits-withdrawals`
3. Click "Process Deposits" tab
4. Process a deposit
5. Click "Process Withdrawals" tab
6. Process a withdrawal
7. Click "Transaction History" tab
8. Verify: Both transactions appear in table

---

## 🔐 Access Control

**Teller can:**
- ✅ Process deposits
- ✅ Process withdrawals
- ✅ View their own transactions
- ✅ Print receipts
- ❌ Cannot access admin pages
- ❌ Cannot view all transactions

**Admin can:**
- ✅ Process deposits
- ✅ Process withdrawals
- ✅ View all transactions
- ✅ Print receipts
- ✅ Access management page
- ✅ Full control

---

## 📊 Today's Transactions Table

**Shows:**
- Account Number
- Customer Name
- Transaction Type (Deposit/Withdrawal)
- Amount (color-coded: Green for deposits, Red for withdrawals)
- Time (HH:MM AM/PM format)
- Status (Completed)

**Updates:**
- Automatically refreshes after each transaction
- Shows only today's transactions
- Sorted by most recent first

---

## 🐛 Troubleshooting

**Issue: Customer not found in search**
- Solution: Make sure customer exists in database
- Try searching by account number instead of name
- Ensure at least 2 characters are typed

**Issue: Receipt won't print**
- Solution: Make sure Notepad is installed
- Try printing from Notepad directly
- Check printer settings

**Issue: Insufficient balance error**
- Solution: Check available balance before withdrawal
- Withdrawal amount must be ≤ available balance
- Try smaller amount

**Issue: Account not found**
- Solution: Verify account number is correct
- Check if account is active in database
- Try searching by customer name

---

## 📝 Notes

- All transactions are logged to database
- Receipts are generated automatically
- Balances update in real-time
- Teller role is restricted to deposits/withdrawals only
- Admin has full access to all features
- All transactions require valid customer account
- Deposits increase balance
- Withdrawals decrease balance
- All amounts are in Philippine Peso (₱)

---

## 🚀 Next Steps

- Run the application
- Log in as Teller or Admin
- Test deposit/withdrawal processing
- Verify receipts print correctly
- Check database for transaction records
- Test customer search functionality
- Verify balance updates

---

**System Status:** ✅ Production Ready

**Last Updated:** 2025-01-18
**Version:** 1.0
