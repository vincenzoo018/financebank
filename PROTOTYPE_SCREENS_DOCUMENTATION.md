# FINSYS Banking System - Prototype Screens Documentation

> **Complete Screenshot & Description Guide for All Transaction and UI Screens**
> 
> This document provides a comprehensive guide to all existing screens in the FINSYS Banking System sidebars, organized by user role. Each screen includes a label name, description, and guidance on functionality and user interaction.

---

## Table of Contents

1. [Customer Portal Screens](#1-customer-portal-screens)
2. [Teller Operations Screens](#2-teller-operations-screens)
3. [Accountant Operations Screens](#3-accountant-operations-screens)
4. [Registrar Screens](#4-registrar-screens)
5. [Admin/SuperAdmin Screens](#5-adminsuperadmin-screens)

---

## 1. Customer Portal Screens

### 1.1 Customer Dashboard
**Route:** `/customer/dashboard`  
**Label:** `SCREEN_CUSTOMER_DASHBOARD`

**Description:**  
The Customer Dashboard is the main landing page for customers after login. It provides a comprehensive overview of the customer's financial status and account information.

**Key Features:**
- **Welcome Section:** Displays personalized greeting with customer name and current date/time
- **Account Overview Card:** Shows the primary account balance with a gradient blue card design, including:
  - Account number (masked for security)
  - Available balance (large display)
  - Pending transactions amount
  - Rewards points
  - Account status (Active/Inactive)
- **KPI Analytics Section:** Monthly financial summary with:
  - Total Income (deposits this month)
  - Total Expenses (withdrawals this month)
- **Offline Mode Banner:** Yellow warning banner appears when database connection is unavailable

**User Interaction:**
- View real-time account balance
- Navigate to other sections via sidebar
- Monitor monthly income vs expenses
- View recent transaction summary

---

### 1.2 Send Money (Transfer Money)
**Route:** `/customer/transfer-money`  
**Label:** `SCREEN_CUSTOMER_SEND_MONEY`

**Description:**  
The Send Money screen allows customers to transfer funds to other FINSYS account holders. Features a modern interface with account lookup and transaction confirmation.

**Key Features:**
- **Header Section:** Gradient teal banner with "Send Money" title and History button
- **From Account Display:** Shows sender's account details:
  - Account number (masked)
  - Current balance
  - Account type (FINSYS Savings Account)
- **Recipient Lookup:** Enter recipient account number with auto-lookup feature
- **Transfer Form:**
  - Amount input field
  - Description/notes field
  - Transaction PIN verification
- **Transaction Confirmation Modal:** Requires PIN verification before processing

**User Interaction:**
1. View your account balance in "From Account" section
2. Enter recipient's account number
3. System auto-fills recipient name after validation
4. Enter transfer amount and optional notes
5. Enter transaction PIN to confirm
6. View transaction success/failure message

**Offline Handling:**  
Red banner displays "Database Connection Required" message when offline. Transfers are not possible without database connection.

---

### 1.3 Deposit History
**Route:** `/customer/deposit`  
**Label:** `SCREEN_CUSTOMER_DEPOSIT_HISTORY`

**Description:**  
Displays a comprehensive history of all deposits processed by tellers for the customer's account.

**Key Features:**
- **Header Section:** Green gradient banner with deposit icon
- **Account Info Card:** Grid showing:
  - Account Number
  - Account Holder name
  - Current Balance
- **Deposit History Table:**
  - Date & Time column
  - Reference/Transaction Number
  - Transaction Type badge
  - Description
  - Amount (right-aligned)
  - Status badge (Completed/Pending)
  - Processed By (Teller name)
  - Action button (View Receipt)

**User Interaction:**
- View all deposit transactions
- Click on any row to view detailed receipt
- Sort/filter deposits by date
- Export deposit history

---

### 1.4 Withdrawal History
**Route:** `/customer/withdraw`  
**Label:** `SCREEN_CUSTOMER_WITHDRAWAL_HISTORY`

**Description:**  
Displays a comprehensive history of all withdrawals processed by tellers for the customer's account.

**Key Features:**
- **Header Section:** Gradient banner with ATM/withdrawal icon
- **Account Info Card:** Grid showing current account details and balance
- **Withdrawal History Table:**
  - Date & Time
  - Reference Number
  - Transaction Type (Withdrawal badge)
  - Description
  - Amount
  - Status
  - Processed By
  - Action (View Receipt)

**User Interaction:**
- Review all withdrawal transactions
- Track withdrawal patterns
- View individual transaction receipts

---

### 1.5 Pay Bills
**Route:** `/customer/bills`  
**Label:** `SCREEN_CUSTOMER_PAY_BILLS`

**Description:**  
Allows customers to pay utility bills and other billers through their FINSYS account.

**Key Features:**
- **Header Section:** Green gradient with "Pay Bills" title and History button
- **Biller Categories:** Grid of category buttons:
  - Utilities
  - Telecommunications
  - Credit Cards
  - Insurance
  - Government
  - Others
- **Biller Selection:** After selecting category, displays available billers
- **Payment Details Form:**
  - Account Number field
  - Amount to Pay field
  - Convenience Fee display (₱10.00)
- **Action Buttons:** Clear and Process Payment buttons

**User Interaction:**
1. Select biller category (e.g., Utilities)
2. Choose specific biller (e.g., Meralco, Manila Water)
3. Enter biller account number
4. Enter payment amount
5. Review convenience fee
6. Confirm and process payment

---

### 1.6 Transaction History
**Route:** `/customer/transactions`  
**Label:** `SCREEN_CUSTOMER_ALL_TRANSACTIONS`

**Description:**  
Comprehensive view of all customer transactions including deposits, withdrawals, transfers, and bill payments.

**Key Features:**
- **Header Section:** Gradient banner with Filters button
- **Sidebar Filters:**
  - Filter by Type (All, Deposit, Withdrawal, Transfer, Bills)
  - Monthly Summary showing Income vs Expenses
- **Transaction List:**
  - Visual icon for each transaction type
  - Transaction description
  - Date and time
  - Amount (green for income, red for expenses)
  - Status indicator

**User Interaction:**
- Filter transactions by type
- View income vs expenses summary
- Click transaction for detailed view
- Export transaction history

---

### 1.7 My Savings Accounts
**Route:** `/customer/savings-accounts`  
**Label:** `SCREEN_CUSTOMER_SAVINGS_ACCOUNTS`

**Description:**  
Displays all savings accounts owned by the customer with detailed information and interest tracking.

**Key Features:**
- **Header Section:** Green gradient with total savings balance
- **Savings Account Cards:** For each account:
  - Account type icon (💰 Regular, 🔒 TimeDeposit, ⭐ Premium)
  - Account number
  - Status badge (Active/Dormant/Closed)
  - Grid of account details:
    - Current Balance
    - Interest Earned
    - Interest Rate (APY)
    - Opened Date
  - Lock-in period indicator for time deposits
  - Maturity date countdown

**User Interaction:**
- View all savings accounts
- Track interest earnings
- Monitor maturity dates for time deposits
- Navigate to withdrawal request

---

### 1.8 Savings Withdrawal Request
**Route:** `/customer/savings-withdrawal`  
**Label:** `SCREEN_CUSTOMER_SAVINGS_WITHDRAWAL`

**Description:**  
Allows customers to submit withdrawal requests from their savings accounts for approval by bank staff.

**Key Features:**
- **Header Section:** Amber/orange gradient with withdrawal icon
- **Savings Account Selection:** List of customer's active savings accounts:
  - Account number and type
  - Available balance
  - Interest rate
  - Lock-in days remaining
  - Maturity date
  - Early withdrawal penalty warning
- **Withdrawal Form:**
  - Amount input
  - Purpose/reason field
  - Withdrawal method selection

**User Interaction:**
1. Select savings account to withdraw from
2. Review balance and any early withdrawal penalties
3. Enter withdrawal amount
4. Provide reason for withdrawal
5. Submit request for approval

---

### 1.9 My Loans
**Route:** `/customer/loans`  
**Label:** `SCREEN_CUSTOMER_LOANS`

**Description:**  
Comprehensive loan management screen showing active loans, applications, payment history, and loan calculator.

**Key Features:**
- **Header Section:** Green gradient with tab buttons:
  - My Loans tab
  - Transaction History tab
  - Apply for Loan button
- **Stats Cards:**
  - Active Loans count
  - Total Outstanding Balance
  - Next Payment Due
- **Active Loans List:** For each loan:
  - Loan number and type
  - Outstanding balance
  - Monthly payment amount
  - Next due date
  - Progress bar (principal vs interest paid)
  - Pay Now button
- **Loan Application Modal:** Multi-step form for new loan applications

**User Interaction:**
- View active loans and balances
- Check payment schedules
- Apply for new loans
- Make loan payments
- View payment history

---

### 1.10 My Profile
**Route:** `/customer/profile`  
**Label:** `SCREEN_CUSTOMER_PROFILE`

**Description:**  
Personal profile management page where customers can view and update their information.

**Key Features:**
- **Header Section:** Green gradient with user icon
- **Profile Card (Center):**
  - Profile picture or initials avatar
  - Full name
  - Username
  - Verified/Active status badge
  - Edit Profile button
  - Change Photo button
- **Personal Information Section:**
  - Email address
  - Phone number
  - Address
  - Date of birth
  - Editable fields when in edit mode

**User Interaction:**
- View personal information
- Click "Edit Profile" to enable editing
- Update phone number, email, address
- Upload new profile picture
- Save changes

**Offline Handling:**  
Yellow banner displays "Offline Mode - viewing cached data" when connection is unavailable.

---

### 1.11 Security Logs
**Route:** `/customer/security-logs`  
**Label:** `SCREEN_CUSTOMER_SECURITY_LOGS`

**Description:**  
Security monitoring page showing login history, security events, and account access records.

**Key Features:**
- **Header Section:** Red gradient (security theme) with shield icon
- **Security Status Cards (4 columns):**
  - Account Status (SECURE/WARNING/LOCKED)
  - Total Logins (last 30 days)
  - Failed Attempts (last 24 hours)
  - Last Login time and IP
- **Tab Navigation:**
  - Recent Activity tab
  - Login History tab
  - Security Events tab
- **Activity Log List:**
  - Icon based on activity type
  - Activity description
  - Timestamp
  - IP address
  - Device/browser info

**User Interaction:**
- Monitor account security status
- Review login history
- Identify suspicious activity
- Track failed login attempts

---

### 1.12 Settings
**Route:** `/customer/settings`  
**Label:** `SCREEN_CUSTOMER_SETTINGS`

**Description:**  
Account settings page for customizing banking experience and security preferences.

**Key Features:**
- **Header Section:** Green gradient with gear icon
- **Appearance & Accessibility:**
  - Dark Mode toggle switch
  - Font Size selector (Small/Medium/Large/Extra Large)
- **Security Settings:**
  - Change Password form
  - Change Transaction PIN form
  - Two-Factor Authentication toggle
- **Notification Preferences:**
  - Email notifications toggle
  - SMS alerts toggle
  - Push notifications toggle

**User Interaction:**
- Toggle dark mode on/off
- Adjust font size for accessibility
- Change password (requires current password)
- Update transaction PIN
- Configure notification preferences

**Offline Handling:**  
Yellow banner warns that password/PIN changes require active connection.

---

## 2. Teller Operations Screens

### 2.1 Teller Dashboard
**Route:** `/teller/dashboard`  
**Label:** `SCREEN_TELLER_DASHBOARD`

**Description:**  
Real-time ERP analytics dashboard for tellers showing transaction processing statistics and performance metrics.

**Key Features:**
- **Header Section:** Light green gradient with:
  - Title "Teller Dashboard"
  - Period filter buttons (Day/Week/Month)
  - Download Report button
- **Quick Stats Cards (4 columns):**
  - Total Deposits (green border) - amount and count
  - Total Withdrawals (red border) - amount and count
  - Loan Payments (blue border) - amount and count
  - Loan Releases (amber border) - amount and count
- **Transaction Charts:** Visual representation of daily/weekly/monthly trends
- **Recent Transactions Table:** Latest processed transactions

**User Interaction:**
- Switch between Day/Week/Month views
- Download transaction reports
- View real-time transaction statistics
- Click on cards to drill down into details

---

### 2.2 Process Deposits
**Route:** `/teller/deposits`  
**Label:** `SCREEN_TELLER_PROCESS_DEPOSITS`

**Description:**  
Transaction processing screen for recording customer cash deposits.

**Key Features:**
- **Header Section:** Green gradient with deposit icon
- **New Deposit Transaction Form:**
  - Account Number (with auto-lookup)
  - Customer Name (auto-filled)
  - Current Balance (auto-filled)
  - Deposit Amount input
  - Deposit Method dropdown:
    - Over-the-Counter (OTC)
    - Bank Transfer
    - Cash Deposit Machine (CDM)
  - Reference Number (auto-generated)
  - Notes textarea
- **Teller PIN Verification:** Required before processing
- **Confirmation Modal:** Shows transaction summary before final processing

**User Interaction:**
1. Enter customer's account number
2. System auto-fills customer name and balance
3. Enter deposit amount
4. Select deposit method
5. Add optional notes
6. Enter Teller PIN for verification
7. Review and confirm deposit
8. Print receipt

---

### 2.3 Process Withdrawals
**Route:** `/teller/withdrawals`  
**Label:** `SCREEN_TELLER_PROCESS_WITHDRAWALS`

**Description:**  
Transaction processing screen for recording customer cash withdrawals.

**Key Features:**
- **Header Section:** Red gradient with withdrawal icon
- **New Withdrawal Transaction Form:**
  - Account Number (with auto-lookup)
  - Customer Name (auto-filled)
  - Available Balance (auto-filled)
  - Withdrawal Amount input
  - Insufficient balance warning
  - Withdrawal Method dropdown:
    - Over-the-Counter (OTC)
    - ATM Withdrawal
    - Bank Transfer
  - Reference Number (auto-generated)
  - Notes textarea
- **Balance Validation:** Prevents withdrawal exceeding available balance
- **Teller PIN Verification:** Required before processing

**User Interaction:**
1. Enter customer's account number
2. Verify customer identity
3. Check available balance
4. Enter withdrawal amount
5. Select withdrawal method
6. Verify Teller PIN
7. Process withdrawal
8. Print receipt and dispense cash

---

### 2.4 Loan Application Review
**Route:** `/teller/loan-review`  
**Label:** `SCREEN_TELLER_LOAN_REVIEW`

**Description:**  
Review pending loan applications and forward them to the Accountant for approval.

**Key Features:**
- **Header Section:** Green gradient with document review icon
- **Success/Error Messages:** Alert banners for operation feedback
- **Pending Applications Table:**
  - Application Number (blue badge)
  - Customer Name and Account
  - Loan Type
  - Requested Amount
  - Monthly Income
  - Status (Pending Review badge)
  - Action Buttons (Review/Reject)
- **Review Modal:** Detailed application view with:
  - Customer information
  - Employment details
  - Financial assessment
  - Notes field for teller comments
  - Forward to Accountant button

**User Interaction:**
1. View list of pending loan applications
2. Click "Review" to open application details
3. Verify customer information and documents
4. Add review notes
5. Forward to Accountant for approval OR Reject with reason

---

### 2.5 Loan Release
**Route:** `/teller/loan-release`  
**Label:** `SCREEN_TELLER_LOAN_RELEASE`

**Description:**  
Disburse approved loan funds to customer accounts.

**Key Features:**
- **Header Section:** With Release History button
- **Stats Cards (3 columns):**
  - Pending Release count
  - Total Amount to be released
  - Today's Releases count
- **Approved Loans Table:**
  - Application number
  - Customer details
  - Loan type
  - Approved amount
  - Approval date
  - Release button
- **Release Modal:**
  - Disbursement details
  - Account selection for fund deposit
  - Confirmation checkbox
  - Release confirmation

**User Interaction:**
1. View approved loans pending release
2. Click "Release" on specific loan
3. Verify disbursement account
4. Confirm fund release
5. Print release confirmation receipt

---

### 2.6 Loan Payments
**Route:** `/teller/loan-payments`  
**Label:** `SCREEN_TELLER_LOAN_PAYMENTS`

**Description:**  
Process physical cash/check payments for customer loans with automatic penalty calculation.

**Key Features:**
- **Important Notice Banner:** Blue info box explaining physical payment mode
- **Search Section:** Account number lookup
- **Customer Information Card:** Displays customer details after search
- **Active Loans List:** For each loan:
  - Loan number and type
  - Outstanding balance
  - Monthly payment
  - Next due date
  - Overdue warning with penalty amount
- **Payment Form:**
  - Loan selection
  - Payment amount
  - Penalty calculation (if overdue)
  - Payment method
  - Receipt preview

**User Interaction:**
1. Enter customer account number
2. Search and verify customer
3. Select loan to pay
4. View any penalties for late payment
5. Enter payment amount
6. Process payment
7. Print payment receipt

---

### 2.7 Open Savings Account
**Route:** `/teller/savings/open`  
**Label:** `SCREEN_TELLER_SAVINGS_OPEN`

**Description:**  
Create new savings accounts for existing customers.

**Key Features:**
- **Header Section:** Green gradient with plus icon
- **Customer Search:** Account number lookup
- **Account Type Selection:**
  - Regular Savings
  - Time Deposit
  - Premium Savings
- **Account Details Form:**
  - Initial Deposit amount
  - Interest Rate (auto-filled based on type)
  - Lock-in Period (for time deposits)
  - Terms and conditions checkbox
- **Confirmation Modal:** Summary of new account details

**User Interaction:**
1. Search for customer by account number
2. Select savings account type
3. Enter initial deposit amount
4. Review interest rate and terms
5. Confirm account opening
6. Print account opening receipt

---

### 2.8 Savings Account History
**Route:** `/teller/savings/history`  
**Label:** `SCREEN_TELLER_SAVINGS_HISTORY`

**Description:**  
View and track all savings accounts opened at the branch.

**Key Features:**
- **Header Section:** With Export to CSV button
- **Summary Statistics Cards:**
  - Total Accounts (purple gradient)
  - Total Deposits (green gradient)
  - Active Accounts (blue gradient)
  - Average Balance (amber gradient)
- **Filters Section:**
  - Search by account number/name
  - Account Type filter
  - Status filter (Active/Closed/Dormant)
- **Accounts Table:**
  - Account number
  - Customer name
  - Account type
  - Balance
  - Interest rate
  - Status
  - Opened date

**User Interaction:**
- Search and filter accounts
- Export data to CSV
- Click account for detailed view
- Monitor account activity

---

### 2.9 Transaction History
**Route:** `/teller/transactions`  
**Label:** `SCREEN_TELLER_TRANSACTION_HISTORY`

**Description:**  
Comprehensive view of all processed transactions including deposits, withdrawals, loan payments, and disbursements.

**Key Features:**
- **Header Section:** Blue gradient with Export button
- **Summary Stats (4 cards):**
  - Deposits count and total
  - Withdrawals count and total
  - Loan Payments count and total
  - Loan Releases count and total
- **Filters:**
  - Transaction Type dropdown
  - Status filter
  - Date Range selector
  - Refresh button
- **Transactions Table:**
  - Date/Time
  - Transaction number
  - Type
  - Customer
  - Amount
  - Status
  - Processed By

**User Interaction:**
- Filter by transaction type, status, or date
- Export filtered data
- View individual transaction details
- Generate reports

---

### 2.10 Customer Accounts
**Route:** `/teller/accounts`  
**Label:** `SCREEN_TELLER_CUSTOMER_ACCOUNTS`

**Description:**  
View and verify customer account information.

**Key Features:**
- Account search functionality
- Customer profile view
- Account balance verification
- Transaction history quick view

---

### 2.11 Account Verification
**Route:** `/teller/verification`  
**Label:** `SCREEN_TELLER_ACCOUNT_VERIFICATION`

**Description:**  
Verify customer identity and account ownership for transactions.

**Key Features:**
- Identity verification checklist
- Document verification
- Photo ID matching
- Signature verification

---

### 2.12 Daily Report
**Route:** `/teller/daily-report`  
**Label:** `SCREEN_TELLER_DAILY_REPORT`

**Description:**  
Generate end-of-day transaction summary report.

**Key Features:**
- Daily transaction summary
- Cash count reconciliation
- Transaction breakdown by type
- Discrepancy reporting

---

### 2.13 Cash Summary
**Route:** `/teller/cash-summary`  
**Label:** `SCREEN_TELLER_CASH_SUMMARY`

**Description:**  
Track cash drawer balance and reconciliation.

**Key Features:**
- Opening balance entry
- Running cash balance
- Denomination breakdown
- Closing balance verification

---

## 3. Accountant Operations Screens

### 3.1 Accountant Dashboard
**Route:** `/accountant/dashboard`  
**Label:** `SCREEN_ACCOUNTANT_DASHBOARD`

**Description:**  
Central dashboard for accountants showing real-time financial records and accounting operations overview.

**Key Features:**
- **Header Section:** Green gradient with period filters (Today/Week/Month)
- **Stats Cards (First Row):**
  - Journal Entries count with pending review
  - Accounts Payable total
  - Accounts Receivable total
  - GL Entries count
- **Stats Cards (Second Row):**
  - Total Deposits
  - Total Withdrawals
  - Loan Payments received
  - Loan Disbursements
- **Quick Actions:** Links to common accounting tasks
- **Recent Activity Feed:** Latest journal entries and transactions

**User Interaction:**
- Toggle between time periods
- Refresh dashboard data
- Navigate to specific accounting modules
- Review pending approvals

---

### 3.2 All Approvals
**Route:** `/accountant/all-approvals`  
**Label:** `SCREEN_ACCOUNTANT_ALL_APPROVALS`

**Description:**  
Central hub for reviewing and processing pending approvals from Teller and Finance Manager.

**Key Features:**
- **Header Section:** Green gradient with description
- **Statistics Cards:**
  - Pending count
  - Approved count
  - Declined count
  - Total count
- **Tab Navigation:**
  - Pending Approvals tab
  - Approval History tab
- **Filter Bar:** Status, type, and date filters
- **Approvals Table:**
  - Reference number
  - Request type
  - Requestor
  - Amount
  - Status
  - Action buttons (Approve/Decline)
- **Approval Modal:**
  - Detailed request information
  - Approval notes
  - Approve/Decline buttons

**User Interaction:**
1. View pending approvals list
2. Filter by type or status
3. Click to review details
4. Add approval notes
5. Approve or decline with reason
6. View approval history

---

### 3.3 Journal Entries
**Route:** `/accounting/journal-entries`  
**Label:** `SCREEN_ACCOUNTANT_JOURNAL_ENTRIES`

**Description:**  
Real-time accounting entries from all transactions with ability to create manual entries.

**Key Features:**
- **Header Section:** With toggle for showing auto-generated entries
- **Summary Cards:**
  - Total Entries
  - Deposits count and amount
  - Withdrawals count and amount
  - Loan Payments count and amount
  - Posted This Month count
- **Journal Entry Table:**
  - Entry date
  - Entry number
  - Description
  - Debit amount
  - Credit amount
  - Source (Auto/Manual)
  - Status
- **Create Entry Modal:** For manual journal entries

**User Interaction:**
- View all journal entries
- Toggle between auto-generated and manual entries
- Create new manual entries
- Post entries to General Ledger
- Export journal data

---

### 3.4 General Ledger
**Route:** `/accounting/general-ledger`  
**Label:** `SCREEN_ACCOUNTANT_GENERAL_LEDGER`

**Description:**  
Real-time view of all account transactions from deposits, withdrawals, fund transfers, loan payments, and savings.

**Key Features:**
- **Header Section:** Description of data sources
- **Account Balances Summary:** Quick view cards for top accounts
- **Filters:**
  - Account selection
  - Account Type (Asset/Liability/Revenue/Expense/Equity)
  - Date From/To
  - Filter/Reset/Export buttons
- **Ledger Table:**
  - Date
  - Account Code
  - Account Name
  - Description
  - Debit
  - Credit
  - Balance
  - Reference

**User Interaction:**
- Filter by account, type, or date range
- Export to CSV
- View running balances
- Drill down into specific accounts

---

### 3.5 Trial Balance
**Route:** `/accountant/trial-balance`  
**Label:** `SCREEN_ACCOUNTANT_TRIAL_BALANCE`

**Description:**  
Generate and verify that total debits equal total credits.

**Key Features:**
- **Header Section:** With description
- **Action Bar:**
  - Generate Trial Balance button
  - Export CSV button
  - Print button
  - As-of Date selector
- **Summary Cards:**
  - Total Debits
  - Total Credits
  - Balance Status (BALANCED/UNBALANCED)
- **Trial Balance Table:**
  - Account Code
  - Account Name
  - Account Type
  - Debit Balance
  - Credit Balance
- **Footer Totals:** Sum of debits and credits

**User Interaction:**
1. Select as-of date
2. Click Generate Trial Balance
3. Review for balance status
4. Export or print report
5. Investigate any imbalances

---

### 3.6 Financial Statements
**Route:** `/accountant/financial-statements`  
**Label:** `SCREEN_ACCOUNTANT_FINANCIAL_STATEMENTS`

**Description:**  
Generate Income Statement, Balance Sheet, and Cash Flow Statement from real transaction data.

**Key Features:**
- **Header Section:** With data source info banner
- **Action Buttons:**
  - Income Statement
  - Balance Sheet
  - Cash Flow Statement
  - Refresh
- **Summary Cards:**
  - Income Statements count
  - Balance Sheets count
  - Cash Flow Statements count
- **Statement Modal:** Displays selected financial statement with:
  - Period selection
  - Statement data
  - Print/Export options

**User Interaction:**
1. Click desired statement type
2. Select reporting period
3. Generate statement
4. Review financial data
5. Print or export PDF

---

### 3.7 Chart of Accounts
**Route:** `/accountant/chart-of-accounts`  
**Label:** `SCREEN_ACCOUNTANT_CHART_OF_ACCOUNTS`

**Description:**  
Manage the financial accounts structure of the organization.

**Key Features:**
- **Header Section:** With description
- **Action Bar:**
  - Add Account button
  - Search input
  - Type filter dropdown
- **Summary Cards (5):**
  - Assets count
  - Liabilities count
  - Equity count
  - Revenue count
  - Expenses count
- **Accounts Table:**
  - Account Code
  - Account Name
  - Account Type
  - Normal Balance
  - Status
  - Actions (Edit/Delete)
- **Add/Edit Modal:** Form for account details

**User Interaction:**
- Search accounts
- Filter by type
- Add new accounts
- Edit existing accounts
- Deactivate accounts

---

### 3.8 Savings Interest Posting
**Route:** `/accountant/savings-interest-posting`  
**Label:** `SCREEN_ACCOUNTANT_SAVINGS_INTEREST`

**Description:**  
Release approved interest to customer savings accounts.

**Key Features:**
- **Header Section:** With pending release count and total amount
- **Daily Operations Card:**
  - Daily Interest Computation button
  - Create Monthly Batch button
  - Last computation info
- **Pending Release Batches:** List of FM-approved batches
- **Batch Processing:**
  - Batch details
  - Account list
  - Interest amounts
  - Release button
- **Processing History:** Previously released batches

**User Interaction:**
1. Run daily interest computation
2. Create monthly posting batch
3. Review FM-approved batches
4. Release interest to accounts
5. View posting history

---

## 4. Registrar Screens

### 4.1 Registrar Dashboard
**Route:** `/registrar/dashboard`  
**Label:** `SCREEN_REGISTRAR_DASHBOARD`

**Description:**  
Overview dashboard for customer registration management.

**Key Features:**
- **Header Section:** Green gradient with welcome message and date
- **Quick Stats Cards:**
  - Total Customers
  - New This Month
  - Today's Registrations
- **Quick Actions Section:**
  - Register New Customer (green button with icon)
  - View Customer List (outlined button)
- **Recent Registrations Table:** Latest customer registrations

**User Interaction:**
- View registration statistics
- Quick navigate to registration form
- Access customer list
- Review recent registrations

---

### 4.2 Register New Customer
**Route:** `/registrar/register-customer`  
**Label:** `SCREEN_REGISTRAR_REGISTER_CUSTOMER`

**Description:**  
Complete customer registration form for creating new banking accounts.

**Key Features:**
- **Header Section:** Green gradient with registration icon
- **Personal Information Section:**
  - Last Name, First Name, Middle Name
  - Email Address
  - Phone Number (11 digits validation)
- **Address Information:**
  - Street Address
  - City
  - Province
  - Postal Code
- **Account Setup:**
  - Username (auto-generated option)
  - Initial Password
  - Initial Deposit amount
- **Confirmation:** Terms acceptance and submit button

**User Interaction:**
1. Fill in personal information
2. Enter address details
3. Set up account credentials
4. Enter initial deposit amount
5. Review and submit
6. Print account opening documents

**Validation:**
- Required fields highlighted in red if empty
- Phone number must be 11 digits
- Email format validation
- Password strength requirements

---

### 4.3 Customer List
**Route:** `/registrar/customers`  
**Label:** `SCREEN_REGISTRAR_CUSTOMER_LIST`

**Description:**  
Comprehensive list of all registered customers with search and management features.

**Key Features:**
- **Header Section:** With Register New Customer button
- **Stats Cards:**
  - Total Customers
  - Active Accounts
  - Pending Accounts
  - New This Month
- **Search and Filter:**
  - Search by name, account number, email
  - Status filter
  - Date range filter
- **Customer Table:**
  - Customer ID
  - Full Name
  - Account Number
  - Email
  - Phone
  - Status
  - Registration Date
  - Actions (View/Edit)

**User Interaction:**
- Search for specific customers
- Filter by status or date
- View customer details
- Edit customer information
- Navigate to new registration

---

## 5. Admin/SuperAdmin Screens

### 5.1 Admin Dashboard
**Route:** `/admin/dashboard`  
**Label:** `SCREEN_ADMIN_DASHBOARD`

**Description:**  
Executive dashboard for administrators with comprehensive system overview and analytics.

**Key Features:**
- **Header Section:** 
  - Welcome message with admin name
  - Date and time
  - System health indicator (circular progress)
  - Refresh button
- **Financial Health Alert Banner:** Color-coded status indicator
- **KPI Cards:**
  - Total Assets
  - Total Liabilities
  - Net Worth
  - Daily Transactions
  - Active Users
  - Pending Approvals
- **Charts Section:**
  - Transaction trends
  - User activity
  - Revenue breakdown
- **System Health Metrics:**
  - Database status
  - Sync status
  - Performance metrics

**User Interaction:**
- Monitor system health
- View financial KPIs
- Access quick links to management functions
- Generate executive reports
- Refresh dashboard data

---

### 5.2 User Registration
**Route:** `/admin/user-registration`  
**Label:** `SCREEN_ADMIN_USER_REGISTRATION`

**Description:**  
Create employee or customer accounts with role-based access control.

**Key Features:**
- **Header Section:** Green gradient with user add icon
- **User Type Selection:**
  - Employee card (Staff member account)
  - Customer card (Personal banking account)
- **Registration Form:**
  - Personal Information (Name, Email, Phone, Address)
  - Account Credentials (Username, Password)
  - For Employees:
    - Department selection
    - Employee ID
    - Role selection (Teller, Accountant, etc.)
  - For Customers:
    - Account type
    - Initial deposit
- **Submit Section:** Create Account button

**User Interaction:**
1. Select user type (Employee/Customer)
2. Fill personal information
3. Set account credentials
4. For employees: assign department and role
5. Submit and create account
6. Print credentials document

---

### 5.3 All Users
**Route:** `/admin/all-users`  
**Label:** `SCREEN_ADMIN_ALL_USERS`

**Description:**  
View and manage all registered users with their profile details.

**Key Features:**
- **Header Section:** With description
- **Summary Cards:**
  - Total Users
  - Active users
  - Employees count
  - Customers count
- **Filters:**
  - Search box
  - Role filter (SuperAdmin, Accountant, Teller, etc.)
  - Status filter (Active/Inactive)
  - Filter and Reset buttons
- **Users Table:**
  - User ID
  - Full Name
  - Username
  - Email
  - Role (colored badge)
  - Status
  - Created Date
  - Last Login
  - Actions (View/Edit/Disable)
- **Pagination:** Page size selector and navigation

**User Interaction:**
- Search users by name, email, or username
- Filter by role or status
- View user details
- Edit user information
- Enable/Disable user accounts
- Reset user passwords

---

### 5.4 Additional Admin Screens

The Admin portal also includes access to various operational screens under dropdown menus:

**Banking Operations:**
- Transaction Management
- Account Management
- Loan Management
- Reports Generation

**Accountant Operations:**
- Journal Entry Review
- GL Management
- Financial Reporting

**Finance Manager Operations:**
- Budget Management
- Interest Rate Settings
- Audit Trail

**System Settings:**
- Database Sync
- Security Center
- Session Management
- Audit Logs

---

## Screenshot Label Reference Guide

| Screen Name | Label | Route |
|------------|-------|-------|
| Customer Dashboard | `SCREEN_CUSTOMER_DASHBOARD` | /customer/dashboard |
| Send Money | `SCREEN_CUSTOMER_SEND_MONEY` | /customer/transfer-money |
| Deposit History | `SCREEN_CUSTOMER_DEPOSIT_HISTORY` | /customer/deposit |
| Withdrawal History | `SCREEN_CUSTOMER_WITHDRAWAL_HISTORY` | /customer/withdraw |
| Pay Bills | `SCREEN_CUSTOMER_PAY_BILLS` | /customer/bills |
| All Transactions | `SCREEN_CUSTOMER_ALL_TRANSACTIONS` | /customer/transactions |
| My Savings | `SCREEN_CUSTOMER_SAVINGS_ACCOUNTS` | /customer/savings-accounts |
| Savings Withdrawal | `SCREEN_CUSTOMER_SAVINGS_WITHDRAWAL` | /customer/savings-withdrawal |
| My Loans | `SCREEN_CUSTOMER_LOANS` | /customer/loans |
| My Profile | `SCREEN_CUSTOMER_PROFILE` | /customer/profile |
| Security Logs | `SCREEN_CUSTOMER_SECURITY_LOGS` | /customer/security-logs |
| Settings | `SCREEN_CUSTOMER_SETTINGS` | /customer/settings |
| Teller Dashboard | `SCREEN_TELLER_DASHBOARD` | /teller/dashboard |
| Process Deposits | `SCREEN_TELLER_PROCESS_DEPOSITS` | /teller/deposits |
| Process Withdrawals | `SCREEN_TELLER_PROCESS_WITHDRAWALS` | /teller/withdrawals |
| Loan Review | `SCREEN_TELLER_LOAN_REVIEW` | /teller/loan-review |
| Loan Release | `SCREEN_TELLER_LOAN_RELEASE` | /teller/loan-release |
| Loan Payments | `SCREEN_TELLER_LOAN_PAYMENTS` | /teller/loan-payments |
| Open Savings | `SCREEN_TELLER_SAVINGS_OPEN` | /teller/savings/open |
| Savings History | `SCREEN_TELLER_SAVINGS_HISTORY` | /teller/savings/history |
| Transaction History | `SCREEN_TELLER_TRANSACTION_HISTORY` | /teller/transactions |
| Customer Accounts | `SCREEN_TELLER_CUSTOMER_ACCOUNTS` | /teller/accounts |
| Account Verification | `SCREEN_TELLER_ACCOUNT_VERIFICATION` | /teller/verification |
| Daily Report | `SCREEN_TELLER_DAILY_REPORT` | /teller/daily-report |
| Cash Summary | `SCREEN_TELLER_CASH_SUMMARY` | /teller/cash-summary |
| Accountant Dashboard | `SCREEN_ACCOUNTANT_DASHBOARD` | /accountant/dashboard |
| All Approvals | `SCREEN_ACCOUNTANT_ALL_APPROVALS` | /accountant/all-approvals |
| Journal Entries | `SCREEN_ACCOUNTANT_JOURNAL_ENTRIES` | /accounting/journal-entries |
| General Ledger | `SCREEN_ACCOUNTANT_GENERAL_LEDGER` | /accounting/general-ledger |
| Trial Balance | `SCREEN_ACCOUNTANT_TRIAL_BALANCE` | /accountant/trial-balance |
| Financial Statements | `SCREEN_ACCOUNTANT_FINANCIAL_STATEMENTS` | /accountant/financial-statements |
| Chart of Accounts | `SCREEN_ACCOUNTANT_CHART_OF_ACCOUNTS` | /accountant/chart-of-accounts |
| Savings Interest | `SCREEN_ACCOUNTANT_SAVINGS_INTEREST` | /accountant/savings-interest-posting |
| Registrar Dashboard | `SCREEN_REGISTRAR_DASHBOARD` | /registrar/dashboard |
| Register Customer | `SCREEN_REGISTRAR_REGISTER_CUSTOMER` | /registrar/register-customer |
| Customer List | `SCREEN_REGISTRAR_CUSTOMER_LIST` | /registrar/customers |
| Admin Dashboard | `SCREEN_ADMIN_DASHBOARD` | /admin/dashboard |
| User Registration | `SCREEN_ADMIN_USER_REGISTRATION` | /admin/user-registration |
| All Users | `SCREEN_ADMIN_ALL_USERS` | /admin/all-users |

---

## Design System Notes

### Color Palette Used:
- **Primary Green:** #059669, #047857, #10b981
- **Success Green:** #d1fae5, #6ee7b7
- **Warning Amber:** #f59e0b, #fef3c7
- **Error Red:** #dc2626, #fee2e2
- **Info Blue:** #3b82f6, #dbeafe
- **Text Dark:** #1f2937, #374151
- **Text Gray:** #6b7280, #9ca3af
- **Background:** #f9fafb, #ffffff

### Common UI Components:
- **Cards:** White background, 16px border-radius, subtle shadow
- **Buttons:** Gradient backgrounds, 8-10px border-radius, bold text
- **Tables:** Header with gray background, hover effects, alternating row colors
- **Forms:** 12-16px padding, 8-10px border-radius, clear labels
- **Modals:** Centered overlay, white background, rounded corners
- **Alerts:** Color-coded banners with icons

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**System:** FINSYS Banking System - .NET MAUI Blazor Hybrid Application

