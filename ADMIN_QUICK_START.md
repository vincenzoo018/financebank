# 🎯 FinanceBank Admin System - Quick Start Guide

## 🚀 Getting Started

### Step 1: Start the Application
```powershell
cd c:\Users\MECHREVO\source\repos\financebank
dotnet run -f net9.0-windows10.0.19041.0
```

### Step 2: Open in Browser
Navigate to: `http://localhost:5000` (or check terminal for exact URL)

### Step 3: Login
Choose a test credential:
- **SuperAdmin:** admin / admin123
- **Admin:** admin2 / admin123  
- **Accountant:** accountant / accountant123
- **Finance Manager:** fmanager / fmanager123
- **Teller:** teller / teller123
- **Customer:** customer / customer123

---

## 📊 Dashboard Overview

### What You'll See as Admin
After logging in with admin credentials, you'll see:

1. **Green Header** with system title and time period filter (Hour, Day, Month, Year)
2. **6 KPI Cards** showing:
   - 👥 Total Users
   - 💳 Transactions
   - ⏳ Pending Items
   - 💵 Revenue
   - 🟢 Active Sessions
   - ⚙️ System Health

3. **Charts** showing:
   - Transaction volume over time
   - Revenue trends
   - Account growth

---

## 🗂️ Main Modules

### Module 1: USER MANAGEMENT
**Go to:** Sidebar → "Users & Roles" dropdown

#### Pages Available:
1. **Register New User** (`/admin/user-registration`)
   - Create new user accounts
   - Assign role and department
   - Auto-generate Employee ID
   
2. **All Users** (`/admin/system/user-management`)
   - View all users
   - Search and filter
   - Edit user details
   - Activate/Deactivate users

3. **Activity Logs** (`/admin/system/user-activity-logs`)
   - Monitor all user actions
   - Filter by user, module, action, date
   - View detailed activity

4. **Login History** (`/admin/system/login-history`)
   - Track all logins
   - See IP addresses
   - Monitor session times

5. **Session Management** (`/admin/system/session-management`)
   - View active sessions
   - Manage user sessions
   - Monitor concurrent users

---

### Module 2: APPROVALS
**Go to:** Sidebar → "Approve Transactions" dropdown

#### Pages Available:
1. **Deposit Approvals** (`/admin/approvals/deposit-approvals`)
   - Approve pending deposits
   - View deposit details
   - Bulk approval option

2. **Withdrawal Approvals** (`/admin/approvals/withdrawal-approvals`)
   - Approve withdrawal requests
   - Check available balance
   - Process confirmations

3. **Transfer Approvals** (`/admin/approvals/transfer-approvals`)
   - Approve fund transfers
   - View transfer type
   - Track transfer status

4. **Loan Application Approvals** (`/admin/approvals/loans`)
   - Review loan applications
   - Check applicant details
   - Approve or reject

5. **Card Application Approvals** (`/admin/approvals/cards`)
   - Review card applications
   - View card type requested
   - Process approval

6. **Bill Payment Approvals** (`/admin/approvals/bill-approvals`)
   - Approve bill payments
   - Verify biller details
   - Track payment status

---

### Module 3: BANKING
**Go to:** Sidebar → "Banking" dropdown

#### Pages Available:
1. **Bank Accounts** (`/admin/banking/accounts`)
   - View all customer accounts
   - Manage account details
   - Track balances

2. **Fund Transfer** (`/admin/banking/fund-transfer`)
   - Initiate transfers
   - Set up beneficiaries
   - Monitor transfer status

3. **Loan Management** (`/admin/banking/loan-management`)
   - Manage active loans
   - Track payments
   - View loan details

4. **Billers Management** (`/admin/banking/billers-management`)
   - Add/Edit billers
   - Manage biller categories
   - Track biller information

5. **Card Management** (`/admin/banking/card-management`)
   - Issue new cards
   - Manage card types
   - Track card status

6. **Bill Payment** (`/admin/banking/bill-payment`)
   - Process bill payments
   - Create new payments
   - Approve payments

7. **Manage Deposits/Withdrawals** (`/admin/banking/manage-deposits-withdrawals`)
   - Process deposits
   - Process withdrawals
   - View transaction history

---

### Module 4: ACCOUNTING
**Go to:** Sidebar → "Accounting" dropdown

#### Pages Available:
1. **Journal Entries** (`/admin/accounting/journal-entries`)
   - Create journal entries
   - Post entries
   - View entry details

2. **General Ledger** (`/admin/accounting/general-ledger`)
   - View general ledger
   - Track debits/credits
   - Monitor account balances

3. **Trial Balance** (`/admin/accounting/trial-balance`)
   - Generate trial balance
   - Check balance equality
   - Export report

4. **Financial Statements** (`/admin/accounting/financial-statements`)
   - View income statement
   - View balance sheet
   - View cash flow statement

5. **Chart of Accounts** (`/admin/accounting/chart-of-accounts`)
   - Manage account structure
   - Add/Edit accounts
   - Track account types

---

### Module 5: FINANCE
**Go to:** Sidebar → "Finance" dropdown

#### Pages Available:
1. **Accounts Payable** (`/admin/finance/payables`)
   - Manage vendor invoices
   - Track payment status
   - Record payments

2. **Accounts Receivable** (`/admin/finance/receivables`)
   - Manage customer invoices
   - Track collections
   - Send reminders

3. **Budget Management** (`/admin/finance/budget-management`)
   - Create budgets
   - Compare actual vs. planned
   - Adjust allocations

4. **Financial Forecasting** (`/admin/finance/financial-forecasting`)
   - Project future performance
   - Analyze trends
   - Generate forecasts

5. **Cashflow Analysis** (`/admin/finance/cashflow-analysis`)
   - Monitor cash flow
   - Project cash needs
   - Track inflows/outflows

6. **Budget Reports** (`/admin/finance/budget-reports`)
   - View budget performance
   - Compare to targets
   - Export reports

7. **Cashflow Reports** (`/admin/finance/cashflow-reports`)
   - View cashflow reports
   - Analyze variations
   - Generate forecasts

---

### Module 6: SYSTEM SETTINGS
**Go to:** Sidebar → "System" dropdown (SuperAdmin only)

#### Pages Available:
1. **Security Center** (`/admin/system/security-center`)
   - Configure security settings
   - Manage passwords
   - Set security policies

2. **Audit Trail** (`/admin/system/audit-trail`)
   - View system audit logs
   - Track all changes
   - Monitor compliance

3. **Settings** (`/admin/system/settings`)
   - Configure system parameters
   - Set defaults
   - Manage configuration

4. **Reports** (`/admin/system/reports`)
   - Generate system reports
   - View statistics
   - Export data

---

### Module 7: REPORTS
**Go to:** Sidebar → "Reports" dropdown

#### Pages Available:
1. **Accountant Reports** (`/admin/system/accountant-reports`)
   - Accounting reports
   - Financial analysis
   - Compliance reports

2. **Finance Manager Reports** (`/admin/system/finance-manager-reports`)
   - Finance reports
   - Budget analysis
   - Cash flow reports

3. **Banking Reports** (`/admin/reports/banking-reports`)
   - Banking statistics
   - Transaction reports
   - Customer reports

---

## 🔍 Common Tasks

### Task 1: Create a New User
1. Go to **User Management → Register New User**
2. Fill in: Name, Email, Phone, Address
3. Create login: Username, Password
4. Select Role (Accountant, Teller, etc.)
5. Select Department (Finance, Operations, etc.)
6. Click "Create Account"
7. User will be logged in immediately

### Task 2: Approve a Deposit
1. Go to **Approvals → Deposit Approvals**
2. View pending deposits
3. Click on a deposit to view details
4. Click "Approve" button
5. Deposit status changes to "Completed"

### Task 3: Create a Journal Entry
1. Go to **Accounting → Journal Entries**
2. Click "New Entry"
3. Select date
4. Add line items (Account, Debit/Credit)
5. Add description
6. Click "Post Entry"
7. Entry appears in General Ledger

### Task 4: Monitor User Activity
1. Go to **User Management → Activity Logs**
2. Use filters: User, Module, Action, Date Range
3. Click "Apply Filters"
4. View filtered results
5. Click "View" on any log to see details

### Task 5: View Dashboard
1. Go to **Dashboard**
2. Select time period (Hour, Day, Month, Year)
3. View KPI cards
4. View charts
5. Check pending items

---

## 🎨 User Interface Guide

### Colors & Meanings
- **Green (#059669):** Primary actions, important information
- **Dark Green (#047857):** Secondary actions, hover states
- **Light Green (#10b981):** Success, approved, active
- **Red (#dc2626):** Warnings, rejected, inactive
- **Yellow/Orange:** Pending, needs attention
- **Blue:** Information, update actions

### Common Button States
- **Green Button:** Primary action (Create, Save, Approve)
- **Gray Button:** Secondary action (Cancel, Reset)
- **Green (Disabled):** Action not available
- **Red Button:** Danger action (Delete, Reject)

### Status Badges
- **Green:** Completed, Active, Approved
- **Yellow:** Pending, Awaiting approval
- **Red:** Rejected, Inactive, Failed
- **Blue:** Processing, In progress

---

## 🔐 Access Levels by Role

### What Each Role Can Access:

**SuperAdmin/Admin:**
- ✅ All 42 admin pages
- ✅ All modules
- ✅ All approvals
- ✅ System settings
- ✅ User management

**Accountant:**
- ✅ Banking module (7 pages)
- ✅ Accounting module (5 pages)
- ✅ Reports (accounting specific)
- ❌ Finance operations
- ❌ User management

**Finance Manager:**
- ✅ Finance module (7 pages)
- ✅ Reports (finance specific)
- ✅ Budget management
- ❌ Banking operations
- ❌ Accounting operations

**Teller:**
- ✅ Deposit processing
- ✅ Withdrawal processing
- ✅ Transaction history
- ❌ Approvals
- ❌ Other operations

**Customer:**
- ✅ Personal dashboard
- ✅ Own transaction history
- ✅ Account information
- ❌ Admin features
- ❌ Other users' data

---

## 📱 Sidebar Navigation

### How to Use the Sidebar
1. **Click Section Title:** Expands/collapses menu
2. **Click Menu Item:** Navigates to page
3. **Active Item:** Highlighted in green
4. **Green Color:** You have access
5. **Hidden Section:** You don't have permission

### Sidebar Sections (Admin)
```
Dashboard
├─ Dashboard (always visible)

User Management
├─ Register New User
├─ All Users
├─ Activity Logs
├─ Login History
├─ Session Management

Approvals
├─ Deposit Approvals
├─ Withdrawal Approvals
├─ Transfer Approvals
├─ Loan Applications
├─ Card Applications
├─ Bill Payments

Banking
├─ Bank Accounts
├─ Fund Transfer
├─ Loan Management
├─ Billers Management
├─ Card Management
├─ Bill Payment
├─ Manage Deposits/Withdrawals

Accounting
├─ Journal Entries
├─ General Ledger
├─ Trial Balance
├─ Financial Statements
├─ Chart of Accounts

Finance
├─ Accounts Payable
├─ Accounts Receivable
├─ Budget Management
├─ Financial Forecasting
├─ Cashflow Analysis
├─ Budget Reports
├─ Cashflow Reports

System Settings
├─ Security Center
├─ Audit Trail
├─ System Settings
├─ Reports

Reports
├─ Accountant Reports
├─ Finance Manager Reports
├─ Banking Reports
```

---

## 🆘 Troubleshooting

### Issue: Can't see a module
**Solution:** Check your role. Admin see all, Accountant see Banking+Accounting, Finance Manager see Finance only.

### Issue: Can't save changes
**Solution:** Check if all required fields are filled (marked with *). Check for error messages.

### Issue: Page shows "Unauthorized"
**Solution:** You don't have permission for this page. Contact admin to add your role.

### Issue: Data not loading
**Solution:** Refresh page (F5). Check network connection. Check if database is running.

### Issue: Sidebar looks wrong
**Solution:** Clear browser cache. Close and reopen browser. Check screen size (responsive design).

---

## 📞 Quick Links

### Important URLs
```
Login:           http://localhost:5000/login
Admin Dashboard: http://localhost:5000/admin/dashboard
User List:       http://localhost:5000/admin/system/user-management
Activity Logs:   http://localhost:5000/admin/system/user-activity-logs
Banking:         http://localhost:5000/admin/banking/accounts
Accounting:      http://localhost:5000/admin/accounting/journal-entries
Finance:         http://localhost:5000/admin/finance/payables
Approvals:       http://localhost:5000/admin/approvals/deposit-approvals
```

---

## ✅ Checklist: First Time Using Admin System

- [ ] Build project (Ctrl+Shift+B)
- [ ] Run project (F5)
- [ ] Navigate to login page
- [ ] Login with admin credentials
- [ ] View dashboard
- [ ] Check all KPI cards display
- [ ] Navigate to User Management
- [ ] View all users
- [ ] Navigate to Banking module
- [ ] View bank accounts
- [ ] Navigate to Accounting module
- [ ] View journal entries
- [ ] Try time period filter on dashboard
- [ ] Click on a user to view activity
- [ ] Check sidebar navigation
- [ ] Verify role-based access

---

**Status:** ✅ Ready to Use  
**Version:** 1.0  
**Date:** November 23, 2025
