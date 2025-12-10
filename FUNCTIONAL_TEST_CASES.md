# FINEBANK - Functional Test Cases
**Application:** FINEBANK Financial Management System  
**Version:** 1.0  
**Date:** December 10, 2025  
**Test Type:** Functional Testing  
**Tester:** QA Team

---

## Table of Contents
1. [Authentication & Security](#authentication--security)
2. [Admin Module](#admin-module)
3. [Teller Module](#teller-module)
4. [Accountant Module](#accountant-module)
5. [Finance Manager Module](#finance-manager-module)
6. [Registrar Module](#registrar-module)
7. [Customer Portal Module](#customer-portal-module)

---

## 1. Authentication & Security

### TC-AUTH-001: User Login
**Objective:** Verify user can successfully log in with valid credentials  
**Prerequisites:** User account exists in the database  
**Test Steps:**
1. Navigate to login page
2. Enter valid username
3. Enter valid password
4. Click "Sign In" button

**Expected Result:**
- User is authenticated successfully
- User is redirected to appropriate dashboard based on role
- Session is created

**Test Data:**
- Username: admin / Password: admin123 (Admin)
- Username: teller / Password: teller123 (Teller)
- Username: accountant / Password: accountant123 (Accountant)
- Username: finance / Password: finance123 (Finance Manager)

---

### TC-AUTH-002: Invalid Login Credentials
**Objective:** Verify system rejects invalid credentials  
**Prerequisites:** None  
**Test Steps:**
1. Navigate to login page
2. Enter invalid username or password
3. Click "Sign In" button

**Expected Result:**
- Error message displayed: "Invalid username or password"
- User remains on login page
- No session created

---

### TC-AUTH-003: Account Recovery with PIN
**Objective:** Verify PIN-based account recovery for locked accounts  
**Prerequisites:** User account is locked due to failed login attempts  
**Test Steps:**
1. Attempt login with locked account
2. System displays PIN verification modal
3. Enter 4-digit PIN
4. Select recovery option (Change Password or Continue)
5. Complete selected action

**Expected Result:**
- PIN modal appears for locked accounts
- Valid PIN unlocks account
- User can change password or continue with session
- Invalid PIN shows error with remaining attempts

---

### TC-AUTH-004: Logout Functionality
**Objective:** Verify user can successfully log out  
**Prerequisites:** User is logged in  
**Test Steps:**
1. Click user profile menu
2. Select "Logout" option
3. Confirm logout

**Expected Result:**
- User session is terminated
- User is redirected to login page
- Cached data is cleared

---

## 2. Admin Module

### TC-ADMIN-001: Dashboard Overview
**Objective:** Verify admin dashboard displays all system metrics  
**Prerequisites:** Admin user is logged in  
**Test Steps:**
1. Navigate to Admin Dashboard
2. Verify all widgets load correctly
3. Check real-time statistics

**Expected Result:**
- Total users count displayed
- Active sessions count displayed
- Transaction statistics visible
- System health indicators shown
- Charts and graphs render correctly

---

### TC-ADMIN-002: User Registration
**Objective:** Verify admin can register new users  
**Prerequisites:** Admin user is logged in  
**Test Steps:**
1. Navigate to "User Registration" page
2. Fill in all required fields:
   - Username
   - Full Name
   - Email
   - Password
   - Role (Admin/Teller/Accountant/Finance Manager/Customer)
3. Click "Register User" button

**Expected Result:**
- User account is created successfully
- Confirmation message displayed
- New user appears in user list
- Login credentials are active

---

### TC-ADMIN-003: View All Users
**Objective:** Verify admin can view and manage all system users  
**Prerequisites:** Admin user is logged in, multiple users exist  
**Test Steps:**
1. Navigate to "All Users" page
2. Review user list
3. Use search/filter options
4. Test pagination if applicable

**Expected Result:**
- All users displayed in table format
- User details visible (username, role, email, status)
- Search filters work correctly
- Pagination functions properly

---

### TC-ADMIN-004: Audit Trail Tracking
**Objective:** Verify audit trail logs all system activities  
**Prerequisites:** Admin user is logged in, system has activity logs  
**Test Steps:**
1. Navigate to "Audit Trail" page
2. View logged activities
3. Apply date filters
4. Search for specific events
5. Export audit logs

**Expected Result:**
- All activities logged with timestamp
- User, action, and details recorded
- Filters work correctly
- Export generates CSV/PDF file

---

### TC-ADMIN-005: Login History
**Objective:** Verify admin can view user login history  
**Prerequisites:** Admin user is logged in, login attempts exist  
**Test Steps:**
1. Navigate to "Login History" page
2. View login records
3. Filter by date range
4. Filter by user
5. Identify failed login attempts

**Expected Result:**
- All login attempts displayed
- Shows username, timestamp, IP address, status
- Failed attempts highlighted
- Filters function correctly

---

### TC-ADMIN-006: Session Management
**Objective:** Verify admin can manage active user sessions  
**Prerequisites:** Admin user is logged in, active sessions exist  
**Test Steps:**
1. Navigate to "Session Management" page
2. View all active sessions
3. Select a session
4. Terminate selected session

**Expected Result:**
- All active sessions displayed
- Session details visible (user, start time, IP)
- Terminate action logs out selected user immediately
- Terminated user must re-login

---

### TC-ADMIN-007: Database Synchronization
**Objective:** Verify admin can sync database records  
**Prerequisites:** Admin user is logged in  
**Test Steps:**
1. Navigate to "Database Sync" page
2. View sync status
3. Click "Sync All" button
4. Monitor sync progress
5. Review sync results

**Expected Result:**
- Sync status displayed for each table
- Sync process completes successfully
- Success/failure count shown
- Error messages displayed if sync fails

---

### TC-ADMIN-008: Security Center
**Objective:** Verify security settings and monitoring  
**Prerequisites:** Admin user is logged in  
**Test Steps:**
1. Navigate to "Security Center" page
2. Review security alerts
3. Check suspicious activity
4. Update security settings
5. Test two-factor authentication toggle

**Expected Result:**
- Security dashboard displays alerts
- Suspicious activities flagged
- Settings can be updated
- Changes saved successfully

---

### TC-ADMIN-009: Transaction Monitoring
**Objective:** Verify admin can monitor all transactions  
**Prerequisites:** Admin user is logged in, transactions exist  
**Test Steps:**
1. Navigate to "Transactions" page
2. View all transactions
3. Apply filters (date, type, amount)
4. View transaction details
5. Export transaction report

**Expected Result:**
- All transactions listed
- Filters work correctly
- Transaction details accessible
- Export generates report file

---

### TC-ADMIN-010: System Settings
**Objective:** Verify admin can configure system settings  
**Prerequisites:** Admin user is logged in  
**Test Steps:**
1. Navigate to "Settings" page
2. Update system configurations
3. Set interest rates
4. Configure transaction limits
5. Save changes

**Expected Result:**
- All settings editable
- Changes saved successfully
- Validation works for invalid inputs
- Confirmation message displayed

---

### TC-ADMIN-011: Admin Analytics
**Objective:** Verify analytics dashboard displays insights  
**Prerequisites:** Admin user is logged in, sufficient data exists  
**Test Steps:**
1. Navigate to "Admin Analytics" page
2. View transaction trends
3. Check user activity charts
4. Review financial metrics
5. Change date range

**Expected Result:**
- Charts and graphs display correctly
- Data updates based on filters
- Metrics calculated accurately
- Interactive elements function properly

---

### TC-ADMIN-012: Loan Violations Monitoring
**Objective:** Verify admin can monitor loan violations  
**Prerequisites:** Admin user is logged in, loan violations exist  
**Test Steps:**
1. Navigate to "Loan Violations" page
2. View all violations
3. Filter by severity
4. View violation details
5. Take action on violation

**Expected Result:**
- All violations listed
- Severity levels indicated
- Details accessible
- Action buttons functional

---

### TC-ADMIN-013: Savings Transaction History
**Objective:** Verify admin can view all savings transactions  
**Prerequisites:** Admin user is logged in, savings transactions exist  
**Test Steps:**
1. Navigate to "Savings Transaction History" page
2. View transaction list
3. Filter by date/customer
4. View transaction details
5. Export report

**Expected Result:**
- All savings transactions displayed
- Filters work correctly
- Details show customer, amount, type, date
- Export generates file

---

## 3. Teller Module

### TC-TELLER-001: Teller Dashboard
**Objective:** Verify teller dashboard displays daily metrics  
**Prerequisites:** Teller user is logged in  
**Test Steps:**
1. Navigate to Teller Dashboard
2. View daily transaction summary
3. Check cash position
4. Review pending tasks

**Expected Result:**
- Dashboard loads successfully
- Today's transaction count displayed
- Cash balance shown
- Pending approvals listed

---

### TC-TELLER-002: Process Deposits
**Objective:** Verify teller can process customer deposits  
**Prerequisites:** Teller logged in, customer account exists  
**Test Steps:**
1. Navigate to "Process Deposits" page
2. Search for customer account
3. Enter deposit amount
4. Select deposit type (Cash/Check)
5. Add reference/notes
6. Submit deposit

**Expected Result:**
- Customer account found successfully
- Deposit amount accepted
- Transaction recorded
- Account balance updated
- Receipt generated

---

### TC-TELLER-003: Process Withdrawals
**Objective:** Verify teller can process customer withdrawals  
**Prerequisites:** Teller logged in, customer has sufficient balance  
**Test Steps:**
1. Navigate to "Process Withdrawals" page
2. Search for customer account
3. Verify customer identity
4. Enter withdrawal amount
5. Validate sufficient balance
6. Submit withdrawal

**Expected Result:**
- Customer account found
- Identity verification successful
- Withdrawal processed if balance sufficient
- Account balance reduced
- Transaction logged

---

### TC-TELLER-004: Account Verification
**Objective:** Verify teller can verify customer accounts  
**Prerequisites:** Teller logged in, customer account exists  
**Test Steps:**
1. Navigate to "Account Verification" page
2. Enter account number
3. View account details
4. Verify customer information
5. Check account status

**Expected Result:**
- Account details displayed
- Customer information shown
- Account status visible (Active/Inactive/Frozen)
- Balance information accurate

---

### TC-TELLER-005: Verify Customer Identity
**Objective:** Verify teller can verify customer identity  
**Prerequisites:** Teller logged in, customer present  
**Test Steps:**
1. Navigate to "Verify Identity" page
2. Enter customer ID or account number
3. Review customer photo and details
4. Verify ID documents
5. Confirm identity verification

**Expected Result:**
- Customer profile displayed
- Photo and details visible
- Verification status can be updated
- Verification logged in system

---

### TC-TELLER-006: Loan Disbursals
**Objective:** Verify teller can process loan disbursals  
**Prerequisites:** Teller logged in, approved loan exists  
**Test Steps:**
1. Navigate to "Loan Disbursals" page
2. View approved loans
3. Select loan for disbursal
4. Verify loan details
5. Process disbursal
6. Generate disbursement receipt

**Expected Result:**
- Approved loans listed
- Loan details accurate
- Disbursal processed successfully
- Funds transferred to customer account
- Receipt generated

---

### TC-TELLER-007: Loan Payments
**Objective:** Verify teller can process loan payments  
**Prerequisites:** Teller logged in, active loan with payment due  
**Test Steps:**
1. Navigate to "Loan Payments" page
2. Search for customer loan
3. View payment schedule
4. Enter payment amount
5. Process payment
6. Update loan balance

**Expected Result:**
- Loan account found
- Payment schedule visible
- Payment processed successfully
- Loan balance updated
- Payment receipt generated

---

### TC-TELLER-008: Daily Cash Summary
**Objective:** Verify teller can view and reconcile daily cash  
**Prerequisites:** Teller logged in, transactions processed today  
**Test Steps:**
1. Navigate to "Cash Summary" page
2. View opening balance
3. Review all transactions
4. Calculate closing balance
5. Submit daily cash report

**Expected Result:**
- Opening balance displayed
- All transactions listed
- Closing balance calculated automatically
- Cash over/short identified
- Report submitted successfully

---

### TC-TELLER-009: Transaction History
**Objective:** Verify teller can view transaction history  
**Prerequisites:** Teller logged in, transactions exist  
**Test Steps:**
1. Navigate to "Transaction History" page
2. View all transactions
3. Filter by date range
4. Filter by transaction type
5. View transaction details

**Expected Result:**
- All transactions displayed
- Filters work correctly
- Transaction details accessible
- Search functionality works

---

### TC-TELLER-010: Customer Account Inquiry
**Objective:** Verify teller can inquire customer account details  
**Prerequisites:** Teller logged in, customer accounts exist  
**Test Steps:**
1. Navigate to "Customer Accounts" page
2. Search for customer
3. View account details
4. Check transaction history
5. Review account status

**Expected Result:**
- Customer found successfully
- All account details visible
- Transaction history displayed
- Account status shown

---

### TC-TELLER-011: Savings Account Opening
**Objective:** Verify teller can open new savings accounts  
**Prerequisites:** Teller logged in, customer profile exists  
**Test Steps:**
1. Navigate to "Savings Account Opening" page
2. Select customer
3. Choose savings account type
4. Enter initial deposit
5. Set account preferences
6. Submit application

**Expected Result:**
- Customer selected successfully
- Account type options available
- Initial deposit validated
- Account created successfully
- Account number generated

---

### TC-TELLER-012: Daily Report Generation
**Objective:** Verify teller can generate daily reports  
**Prerequisites:** Teller logged in, transactions processed  
**Test Steps:**
1. Navigate to "Daily Report" page
2. Select report date
3. View report summary
4. Generate PDF report
5. Export to Excel

**Expected Result:**
- Report displays all daily activities
- Summary totals calculated correctly
- PDF generated successfully
- Excel export works

---

## 4. Accountant Module

### TC-ACCT-001: Accountant Dashboard
**Objective:** Verify accountant dashboard displays financial overview  
**Prerequisites:** Accountant user is logged in  
**Test Steps:**
1. Navigate to Accountant Dashboard
2. View pending approvals
3. Check recent journal entries
4. Review account balances

**Expected Result:**
- Dashboard loads with financial metrics
- Pending tasks displayed
- Recent activities shown
- Quick access to key functions

---

### TC-ACCT-002: Chart of Accounts Management
**Objective:** Verify accountant can manage chart of accounts  
**Prerequisites:** Accountant logged in  
**Test Steps:**
1. Navigate to "Accounts Chart" page
2. View all accounts
3. Add new account
4. Edit existing account
5. Deactivate account

**Expected Result:**
- All accounts listed by category
- New account created successfully
- Edits saved correctly
- Deactivation updates status

---

### TC-ACCT-003: Journal Entries Creation
**Objective:** Verify accountant can create journal entries  
**Prerequisites:** Accountant logged in, accounts exist  
**Test Steps:**
1. Navigate to "Journal Entries" page
2. Click "New Entry"
3. Enter journal details
4. Add debit accounts and amounts
5. Add credit accounts and amounts
6. Ensure debits = credits
7. Submit entry

**Expected Result:**
- Entry form loads correctly
- Account selection works
- Debit/Credit validation works
- Entry saved and posted
- Journal number generated

---

### TC-ACCT-004: General Ledger Review
**Objective:** Verify accountant can view general ledger  
**Prerequisites:** Accountant logged in, transactions exist  
**Test Steps:**
1. Navigate to "General Ledger" page
2. View all ledger entries
3. Filter by account
4. Filter by date range
5. View entry details
6. Export ledger

**Expected Result:**
- All GL transactions displayed
- Includes auto-generated and manual entries
- Filters work correctly
- Running balance calculated
- Export generates file

---

### TC-ACCT-005: Trial Balance Generation
**Objective:** Verify accountant can generate trial balance  
**Prerequisites:** Accountant logged in, GL entries exist  
**Test Steps:**
1. Navigate to "Trial Balance" page
2. Select date range
3. Generate trial balance
4. Verify debits = credits
5. Export report

**Expected Result:**
- Trial balance displays all accounts
- Debit and credit totals shown
- Totals must balance
- Export generates PDF/Excel

---

### TC-ACCT-006: Financial Statements
**Objective:** Verify accountant can generate financial statements  
**Prerequisites:** Accountant logged in, financial data exists  
**Test Steps:**
1. Navigate to "Financial Statements" page
2. Select statement type (Balance Sheet/Income Statement/Cash Flow)
3. Select reporting period
4. Generate statement
5. Export to PDF

**Expected Result:**
- Statement generates correctly
- All accounts categorized properly
- Calculations accurate
- Professional formatting
- Export works

---

### TC-ACCT-007: Savings Interest Posting
**Objective:** Verify accountant can post savings interest  
**Prerequisites:** Accountant logged in, savings accounts with balances exist  
**Test Steps:**
1. Navigate to "Savings Interest Posting" page
2. Select posting period
3. Review calculated interest
4. Verify interest rates applied
5. Post interest to accounts
6. Generate posting report

**Expected Result:**
- Interest calculated correctly for all accounts
- Interest rates applied accurately
- Posting updates account balances
- Journal entries created automatically
- Report generated

---

### TC-ACCT-008: Interest Payouts Management
**Objective:** Verify accountant can manage interest payouts  
**Prerequisites:** Accountant logged in, interest posted  
**Test Steps:**
1. Navigate to "Interest Payouts" page
2. View pending payouts
3. Process payout batch
4. Generate payout report
5. Update payout status

**Expected Result:**
- All pending payouts listed
- Batch processing works
- Payouts recorded in GL
- Report shows all payouts
- Status updated

---

### TC-ACCT-009: All Approvals Review
**Objective:** Verify accountant can review and approve transactions  
**Prerequisites:** Accountant logged in, pending approvals exist  
**Test Steps:**
1. Navigate to "All Approvals" page
2. View pending items
3. Review approval details
4. Approve or reject item
5. Add comments

**Expected Result:**
- All pending approvals displayed
- Details accessible
- Approve/Reject actions work
- Comments saved
- Status updated

---

### TC-ACCT-010: Financial Reports Generation
**Objective:** Verify accountant can generate financial reports  
**Prerequisites:** Accountant logged in, data exists  
**Test Steps:**
1. Navigate to "Reports" page
2. Select report type
3. Set date range
4. Apply filters
5. Generate report
6. Export report

**Expected Result:**
- Report types available
- Filters work correctly
- Report generates accurately
- Export to multiple formats
- Professional formatting

---

### TC-ACCT-011: My Tasks Dashboard
**Objective:** Verify accountant can view assigned tasks  
**Prerequisites:** Accountant logged in, tasks assigned  
**Test Steps:**
1. Navigate to "My Tasks" page
2. View all tasks
3. Filter by status
4. Mark task as complete
5. View task details

**Expected Result:**
- All assigned tasks displayed
- Status filters work
- Tasks can be updated
- Completion logged
- Details accessible

---

## 5. Finance Manager Module

### TC-FIN-001: Finance Manager Dashboard
**Objective:** Verify finance manager dashboard displays key metrics  
**Prerequisites:** Finance Manager logged in  
**Test Steps:**
1. Navigate to Finance Manager Dashboard
2. View financial summary
3. Check budget vs actual
4. Review cashflow status
5. View pending approvals

**Expected Result:**
- Dashboard loads with green theme
- Key metrics displayed
- Charts render correctly
- Quick actions available

---

### TC-FIN-002: Budget Management
**Objective:** Verify finance manager can create and manage budgets  
**Prerequisites:** Finance Manager logged in  
**Test Steps:**
1. Navigate to "Budget Management" page
2. Create new budget
3. Set budget categories
4. Allocate amounts
5. Set period (monthly/quarterly/annual)
6. Save budget

**Expected Result:**
- Budget creation form works
- Categories selectable
- Amounts validated
- Budget saved successfully
- Budget appears in list

---

### TC-FIN-003: Budget Reports
**Objective:** Verify finance manager can generate budget reports  
**Prerequisites:** Finance Manager logged in, budgets exist  
**Test Steps:**
1. Navigate to "Budget Reports" page
2. Select budget period
3. View budget vs actual
4. Analyze variances
5. Export report

**Expected Result:**
- Budget comparison displayed
- Variances calculated
- Visual indicators for over/under budget
- Export generates report

---

### TC-FIN-004: Cashflow Analysis
**Objective:** Verify finance manager can analyze cashflow  
**Prerequisites:** Finance Manager logged in, transactions exist  
**Test Steps:**
1. Navigate to "Cashflow Analysis" page
2. Select analysis period
3. View inflows and outflows
4. Review net cashflow
5. Analyze trends

**Expected Result:**
- Cashflow data displayed
- Inflows and outflows categorized
- Net cashflow calculated (positive values in green)
- Charts show trends
- Period comparison available

---

### TC-FIN-005: Cashflow Reports
**Objective:** Verify finance manager can generate cashflow reports  
**Prerequisites:** Finance Manager logged in, data exists  
**Test Steps:**
1. Navigate to "Cashflow Reports" page
2. Select reporting period
3. Generate report
4. View recommendations
5. Export report

**Expected Result:**
- Report displays detailed cashflow
- Recommendations shown in green cards
- Professional formatting
- Export to PDF/Excel works

---

### TC-FIN-006: Financial Forecasting
**Objective:** Verify finance manager can create financial forecasts  
**Prerequisites:** Finance Manager logged in, historical data exists  
**Test Steps:**
1. Navigate to "Financial Forecasting" page
2. Select forecast period
3. Review historical trends
4. Adjust forecast assumptions
5. Generate forecast
6. Save forecast

**Expected Result:**
- Historical data displayed (green theme)
- Forecast calculations work
- Assumptions editable
- Forecast saved successfully
- Charts show projections

---

### TC-FIN-007: Accounts Payable Management
**Objective:** Verify finance manager can manage AP  
**Prerequisites:** Finance Manager logged in, AP records exist  
**Test Steps:**
1. Navigate to "Accounts Payable" page
2. View all payables
3. Filter by status
4. View payable details
5. Record payment
6. Update status

**Expected Result:**
- All payables listed
- Filters work (green buttons)
- Details accessible (green view buttons)
- Payment recorded
- Status updated

---

### TC-FIN-008: Accounts Receivable Management
**Objective:** Verify finance manager can manage AR  
**Prerequisites:** Finance Manager logged in, AR records exist  
**Test Steps:**
1. Navigate to "Accounts Receivable" page
2. View pending review items
3. Review AR details
4. Record payment receipt
5. Update status to Paid

**Expected Result:**
- All receivables displayed (green theme)
- Tabs work (Pending Review/History)
- Receive Payment button functional
- Payment amount auto-fills correctly
- Status updated to Paid

---

### TC-FIN-009: Loan Approval Center
**Objective:** Verify finance manager can approve loans  
**Prerequisites:** Finance Manager logged in, loan applications exist  
**Test Steps:**
1. Navigate to "Loan Approval Center" page
2. View pending loan applications
3. Review loan details
4. Check credit assessment
5. Approve or reject loan
6. Add approval comments

**Expected Result:**
- All pending loans displayed
- Details accessible
- Date filters work (green buttons)
- Approve/Reject actions work
- Comments saved

---

### TC-FIN-010: Savings Approvals
**Objective:** Verify finance manager can approve savings transactions  
**Prerequisites:** Finance Manager logged in, savings transactions pending  
**Test Steps:**
1. Navigate to "Savings Approvals" page
2. View pending transactions (green tabs)
3. Review transaction details
4. Approve or reject
5. Add notes

**Expected Result:**
- Pending transactions displayed
- Tabs work (Pending/Approved/Rejected) (green theme)
- Details accessible
- Actions work
- Notes saved

---

### TC-FIN-011: Expenses Management
**Objective:** Verify finance manager can manage expenses  
**Prerequisites:** Finance Manager logged in  
**Test Steps:**
1. Navigate to "Expenses" page
2. View all expenses
3. Add new expense
4. Categorize expense
5. Attach receipt
6. Submit for approval

**Expected Result:**
- Expenses listed
- New expense form works
- Categories available
- File upload works
- Expense submitted

---

### TC-FIN-012: Approval Decision Workflow
**Objective:** Verify finance manager approval workflow  
**Prerequisites:** Finance Manager logged in, approval requests exist  
**Test Steps:**
1. Navigate to "Approval Decision" page
2. View pending approvals
3. Review approval details
4. Make decision (Approve/Reject)
5. Add decision notes

**Expected Result:**
- Approvals displayed
- Details comprehensive
- Decision actions work
- Notes saved
- Requester notified

---

## 6. Registrar Module

### TC-REG-001: Registrar Dashboard
**Objective:** Verify registrar dashboard displays customer statistics  
**Prerequisites:** Registrar user is logged in  
**Test Steps:**
1. Navigate to Registrar Dashboard
2. View total customers
3. Check recent registrations
4. Review pending verifications

**Expected Result:**
- Dashboard displays customer metrics
- Recent activities shown
- Quick access to registration

---

### TC-REG-002: Register New Customer
**Objective:** Verify registrar can register new customers  
**Prerequisites:** Registrar logged in  
**Test Steps:**
1. Navigate to "Register Customer" page
2. Fill in personal information
3. Upload customer photo
4. Enter contact details
5. Set initial account details
6. Submit registration

**Expected Result:**
- Form validation works
- Photo upload successful
- Customer created in database
- Customer ID generated
- Account can be created

---

### TC-REG-003: Customer List Management
**Objective:** Verify registrar can view and manage customer list  
**Prerequisites:** Registrar logged in, customers exist  
**Test Steps:**
1. Navigate to "Customer List" page
2. View all customers
3. Search for specific customer
4. Filter by status
5. View customer details
6. Edit customer information

**Expected Result:**
- All customers displayed
- Search works correctly
- Filters function properly
- Details accessible
- Edits saved successfully

---

## 7. Customer Portal Module

### TC-CUST-001: Customer Dashboard
**Objective:** Verify customer dashboard displays account overview  
**Prerequisites:** Customer user is logged in  
**Test Steps:**
1. Navigate to Customer Dashboard
2. View account balance
3. Check recent transactions
4. Review upcoming payments

**Expected Result:**
- Dashboard displays account summary
- Balance shown accurately
- Recent transactions listed
- Loan/savings info visible

---

### TC-CUST-002: View Accounts
**Objective:** Verify customer can view their accounts  
**Prerequisites:** Customer logged in, accounts exist  
**Test Steps:**
1. Navigate to "Accounts" page
2. View all accounts
3. Check account balances
4. View account details
5. View transaction history

**Expected Result:**
- All accounts listed
- Balances accurate
- Details comprehensive
- Transaction history accessible

---

### TC-CUST-003: View Transactions
**Objective:** Verify customer can view transaction history  
**Prerequisites:** Customer logged in, transactions exist  
**Test Steps:**
1. Navigate to "Transactions" page
2. View all transactions
3. Filter by date range
4. Filter by transaction type
5. View transaction details

**Expected Result:**
- All transactions displayed
- Filters work correctly
- Details include: date, type, amount, balance
- Search works

---

### TC-CUST-004: Deposit Money
**Objective:** Verify customer can view deposit options  
**Prerequisites:** Customer logged in  
**Test Steps:**
1. Navigate to "Deposit Money" page
2. View deposit instructions
3. View deposit methods
4. Check branch locations

**Expected Result:**
- Deposit options displayed
- Instructions clear
- Methods listed
- Branch info available

---

### TC-CUST-005: Withdraw Money
**Objective:** Verify customer can view withdrawal options  
**Prerequisites:** Customer logged in, sufficient balance  
**Test Steps:**
1. Navigate to "Withdraw Money" page
2. View withdrawal methods
3. Check withdrawal limits
4. View ATM locations

**Expected Result:**
- Withdrawal options displayed
- Limits shown clearly
- Methods listed
- ATM locations available

---

### TC-CUST-006: Transfer Money
**Objective:** Verify customer can transfer funds  
**Prerequisites:** Customer logged in, sufficient balance  
**Test Steps:**
1. Navigate to "Transfer Money" page
2. Select source account
3. Enter destination account
4. Enter transfer amount
5. Add notes/reference
6. Submit transfer

**Expected Result:**
- Account selection works
- Amount validated against balance
- Transfer processed successfully
- Both accounts updated
- Confirmation displayed

---

### TC-CUST-007: Pay Bills
**Objective:** Verify customer can pay bills  
**Prerequisites:** Customer logged in, sufficient balance  
**Test Steps:**
1. Navigate to "Bills" page
2. Select biller
3. Enter account number
4. Enter payment amount
5. Confirm payment
6. Submit payment

**Expected Result:**
- Billers listed
- Payment form works
- Balance checked
- Payment processed
- Receipt generated

---

### TC-CUST-008: View Loans
**Objective:** Verify customer can view loan details  
**Prerequisites:** Customer logged in, loans exist  
**Test Steps:**
1. Navigate to "Loans" page
2. View all loans
3. Check loan balances
4. View payment schedule
5. View payment history

**Expected Result:**
- All loans displayed
- Current balance shown
- Payment schedule accessible
- History detailed

---

### TC-CUST-009: View Cards
**Objective:** Verify customer can view card details  
**Prerequisites:** Customer logged in, cards issued  
**Test Steps:**
1. Navigate to "Cards" page
2. View all cards
3. Check card status
4. View card limits
5. View card transactions

**Expected Result:**
- All cards displayed
- Status shown (Active/Blocked)
- Limits visible
- Transactions listed

---

### TC-CUST-010: Manage Savings Accounts
**Objective:** Verify customer can manage savings accounts  
**Prerequisites:** Customer logged in, savings accounts exist  
**Test Steps:**
1. Navigate to "My Savings Accounts" page
2. View all savings accounts
3. Check interest rates
4. View balance and interest earned
5. View transaction history

**Expected Result:**
- All savings accounts displayed
- Interest rates shown
- Balance and earned interest visible
- Transaction history accessible

---

### TC-CUST-011: Savings Withdrawal
**Objective:** Verify customer can withdraw from savings  
**Prerequisites:** Customer logged in, savings balance sufficient  
**Test Steps:**
1. Navigate to "Savings Withdrawal" page
2. Select savings account
3. Enter withdrawal amount
4. Confirm withdrawal
5. Submit request

**Expected Result:**
- Account selection works
- Amount validated
- Withdrawal processed or queued for approval
- Balance updated
- Confirmation shown

---

### TC-CUST-012: Manage Savings Goals
**Objective:** Verify customer can set and track savings goals  
**Prerequisites:** Customer logged in  
**Test Steps:**
1. Navigate to "Savings" page (Goals)
2. Create new savings goal
3. Set target amount and date
4. Link to savings account
5. Track progress

**Expected Result:**
- Goal creation works
- Target amount and date saved
- Progress calculated correctly
- Visual indicators shown

---

### TC-CUST-013: View Rewards
**Objective:** Verify customer can view reward points  
**Prerequisites:** Customer logged in, reward points exist  
**Test Steps:**
1. Navigate to "Rewards" page
2. View total points
3. View points history
4. Check available rewards
5. Redeem points

**Expected Result:**
- Points balance displayed
- History detailed
- Rewards catalog shown
- Redemption works

---

### TC-CUST-014: Profile Management
**Objective:** Verify customer can manage profile  
**Prerequisites:** Customer logged in  
**Test Steps:**
1. Navigate to "Profile" page
2. View current information
3. Update contact details
4. Change password
5. Upload profile picture
6. Save changes

**Expected Result:**
- Profile displays current data
- Edits allowed
- Password change works
- Photo upload successful
- Changes saved

---

### TC-CUST-015: Security Logs
**Objective:** Verify customer can view security activity  
**Prerequisites:** Customer logged in, activity exists  
**Test Steps:**
1. Navigate to "Security Logs" page
2. View login history
3. Check recent activities
4. Review suspicious activity alerts

**Expected Result:**
- All activities logged
- Login history shown with timestamp and IP
- Alerts highlighted
- Recent activities listed

---

### TC-CUST-016: Settings Management
**Objective:** Verify customer can manage account settings  
**Prerequisites:** Customer logged in  
**Test Steps:**
1. Navigate to "Settings" page
2. Update preferences
3. Configure notifications
4. Set security options
5. Save changes

**Expected Result:**
- Settings displayed
- Preferences editable
- Notifications configurable
- Security options available
- Changes saved

---

## Test Execution Summary

### Test Coverage
- **Authentication & Security:** 4 test cases
- **Admin Module:** 13 test cases
- **Teller Module:** 12 test cases
- **Accountant Module:** 11 test cases
- **Finance Manager Module:** 12 test cases
- **Registrar Module:** 3 test cases
- **Customer Portal Module:** 16 test cases

**Total Test Cases:** 71 Functional Test Cases

---

## Testing Notes

### General Testing Guidelines:
1. **Test Environment:** Use test database with sample data
2. **Browser Compatibility:** Test on Chrome, Firefox, Edge
3. **Responsive Design:** Test on desktop and mobile views
4. **Data Validation:** Verify all form validations work
5. **Error Handling:** Test invalid inputs and edge cases
6. **Performance:** Monitor page load times
7. **Security:** Ensure role-based access control works

### Priority Levels:
- **Critical (P0):** Login, Core transactions, Payment processing
- **High (P1):** Account management, Reports, Approvals
- **Medium (P2):** Dashboard widgets, Filters, Search
- **Low (P3):** UI enhancements, Export features

### Test Data Requirements:
- Multiple user accounts for each role
- Customer accounts with various balances
- Transaction history data
- Loan applications and active loans
- Savings accounts with balances
- Journal entries and GL transactions
- Pending approvals in various modules

---

## Sign-Off

**Prepared By:** QA Team  
**Approved By:** _________________  
**Date:** December 10, 2025

---

*END OF DOCUMENT*
