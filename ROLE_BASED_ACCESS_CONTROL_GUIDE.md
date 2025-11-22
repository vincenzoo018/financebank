# ROLE-BASED ACCESS CONTROL (RBAC) - ADMIN vs ACCOUNTANT

**FinanceBank - FINSYS Accounting System**  
**Role-Based Access Control Implementation Guide**

---

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Role Definitions](#role-definitions)
3. [Permission Matrix](#permission-matrix)
4. [Detailed Access Specifications](#detailed-access-specifications)
5. [Implementation Code](#implementation-code)
6. [Security Policies](#security-policies)
7. [Audit & Compliance](#audit--compliance)

---

## SYSTEM OVERVIEW

### What is Role-Based Access Control (RBAC)?

**RBAC** is a security mechanism that restricts system access based on a user's assigned role. Each role has specific permissions for different modules and operations.

### Current System Roles

```
┌─────────────────────────────────────────────────────────────┐
│                    SYSTEM ROLES                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  🔴 ADMIN                                                  │
│     └─ Highest authority                                   │
│     └─ Full system access                                  │
│     └─ Can create/modify users and roles                   │
│     └─ Can change system settings                          │
│                                                             │
│  🟡 FINANCE MANAGER                                        │
│     └─ Management authority                                │
│     └─ Can review all reports                              │
│     └─ Can approve transactions                            │
│     └─ Cannot change system settings                       │
│                                                             │
│  🟢 ACCOUNTANT                                             │
│     └─ Accounting operations only                          │
│     └─ Can record and post entries                         │
│     └─ Can view banking operations (read-only)             │
│     └─ Cannot access admin functions                       │
│                                                             │
│  🔵 TELLER                                                 │
│     └─ Customer-facing transactions                        │
│     └─ Can process deposits/withdrawals                    │
│     └─ Cannot access accounting systems                    │
│                                                             │
│  🟣 CUSTOMER                                               │
│     └─ Minimal access                                      │
│     └─ Can view own accounts                               │
│     └─ Can view own transactions                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## ROLE DEFINITIONS

### Admin - Full Access

#### What is an Admin?

An **Admin** is a system administrator with complete authority over all aspects of the FinanceBank system. Admin is the highest role and should be restricted to authorized IT and management personnel only.

#### Admin Responsibilities

| Responsibility | Description |
|---|---|
| **System Management** | Install patches, manage backups, monitor performance |
| **User Management** | Create, modify, deactivate user accounts |
| **Role Management** | Create roles, assign roles, modify permissions |
| **Settings Configuration** | System settings, feature toggles, integrations |
| **Security** | Password resets, access reviews, security policies |
| **Compliance** | Audit logging, regulatory compliance, reporting |
| **Troubleshooting** | Resolve technical issues, database management |
| **Financial Oversight** | View all financial data, modify if necessary |

#### Admin Access Summary

```
✅ FULL ACCESS to:
  ├─ All Modules
  ├─ All Functions
  ├─ All Data
  ├─ Admin Panel
  ├─ System Settings
  ├─ User Accounts
  ├─ Role Permissions
  └─ All Reports

❌ RESTRICTED from:
  └─ Nothing (except physical access to servers)
```

---

### Accountant - Limited Access

#### What is an Accountant?

An **Accountant** is a financial professional who records, validates, and reports financial transactions. The Accountant has focused access to accounting modules with read-only access to banking operations.

#### Accountant Responsibilities

| Responsibility | Description |
|---|---|
| **Transaction Recording** | Create journal entries for all financial events |
| **Ledger Maintenance** | Manage general ledger accounts and balances |
| **Validation** | Review trial balance, verify accuracy |
| **Financial Reporting** | Prepare balance sheet, income statement, cash flow |
| **Analysis** | Generate accounting reports for management |
| **Reconciliation** | Reconcile accounts to source data |
| **Compliance** | Ensure accounting standards are followed |
| **Coordination** | Work with operations, loan, and card teams |

#### Accountant Access Summary

```
✅ FULL ACCESS to:
  ├─ Journal Entries (Create, Edit, Post, Reverse)
  ├─ Trial Balance (Generate, Review, Approve)
  ├─ Financial Statements (Prepare, Generate)
  ├─ Accounting Reports (All types)
  └─ Chart of Accounts (View/Reference)

⚠️  READ-ONLY ACCESS to:
  ├─ General Ledger
  ├─ Bank Accounts
  ├─ Fund Transfers
  ├─ Loan Management
  ├─ Card Management
  ├─ Customer Transactions
  ├─ Customer Accounts
  ├─ Banking Reports
  └─ Audit Logs

❌ NO ACCESS to:
  ├─ Admin Panel
  ├─ System Settings
  ├─ User Management
  ├─ Role Management
  ├─ Approval Queue (Admin-level)
  ├─ User Accounts
  ├─ Role Permissions
  ├─ Backup/Restore
  ├─ System Configuration
  └─ Security Settings
```

---

## PERMISSION MATRIX

### Comprehensive Access Matrix

```
MODULE                           | ADMIN  | ACCOUNTANT | NOTES
═════════════════════════════════════════════════════════════════════
JOURNAL ENTRIES
  Create                         |  ✅    |   ✅       | Core function
  View All                       |  ✅    |   ✅       | All or own
  Edit Own (Draft)               |  ✅    |   ✅       | Before posting
  Edit Others' (Draft)           |  ✅    |   ❌       | Security
  Edit (Posted)                  |  ✅    |   ❌       | Use reversals
  Post to GL                     |  ✅    |   ✅       | Create impact
  Reverse Entry                  |  ✅    |   ✅       | For corrections
  Delete Entry                   |  ✅    |   ❌       | Archive only
─────────────────────────────────|────────|────────────|──────────────
GENERAL LEDGER
  View Accounts                  |  ✅    |   ✅       | Reference
  View Transactions              |  ✅    |   ✅       | Read-only
  View Balances                  |  ✅    |   ✅       | Current state
  Edit Balances Direct           |  ✅    |   ❌       | Via JE only
  Delete Records                 |  ✅    |   ❌       | No deletion
─────────────────────────────────|────────|────────────|──────────────
TRIAL BALANCE
  Generate Report                |  ✅    |   ✅       | Monthly
  View Report                    |  ✅    |   ✅       | Analysis
  Approve Trial Balance          |  ✅    |   ✅       | Key step
  Modify Period Closed           |  ✅    |   ❌       | Admin only
  Lock Period                    |  ✅    |   ✅       | Prevent changes
─────────────────────────────────|────────|────────────|──────────────
FINANCIAL STATEMENTS
  Generate Statements            |  ✅    |   ✅       | Monthly
  View Statements                |  ✅    |   ✅       | Analysis
  Edit Statements                |  ✅    |   ✅       | Add notes
  Add Disclosures                |  ✅    |   ✅       | Required
  Approve for Distribution       |  ✅    |   ✅       | With mgmt
  Delete Statements              |  ✅    |   ❌       | Archive only
  Distribute Statements          |  ✅    |   ✅       | Authorized users
─────────────────────────────────|────────|────────────|──────────────
ACCOUNTING REPORTS
  View All Reports               |  ✅    |   ✅       | All types
  Generate Reports               |  ✅    |   ✅       | Custom reports
  Export Reports                 |  ✅    |   ✅       | CSV, Excel, PDF
  Schedule Reports               |  ✅    |   ⚠️       | Own only
  Delete Reports                 |  ✅    |   ❌       | Archive
  Email Reports                  |  ✅    |   ✅       | To authorized
─────────────────────────────────|────────|────────────|──────────────
CHART OF ACCOUNTS
  View Accounts                  |  ✅    |   ✅       | Reference
  Search Accounts                |  ✅    |   ✅       | Find codes
  Create New Account             |  ✅    |   ❌       | Structure
  Edit Account Details           |  ✅    |   ❌       | Structure
  Delete Account                 |  ✅    |   ❌       | Structure
  Activate/Deactivate            |  ✅    |   ❌       | Use in entries
─────────────────────────────────|────────|────────────|──────────────
BANK ACCOUNTS
  View Accounts                  |  ✅    |   ✅       | Read-only
  View Balances                  |  ✅    |   ✅       | Reconciliation
  View Transactions              |  ✅    |   ✅       | Verify posting
  Edit Account Details           |  ✅    |   ❌       | Operations
  Transfer Funds                 |  ✅    |   ❌       | Operations
  Reconcile Accounts             |  ✅    |   ✅       | GL verification
─────────────────────────────────|────────|────────────|──────────────
FUND TRANSFERS
  View Transfers                 |  ✅    |   ✅       | Read-only
  View Transfer Details          |  ✅    |   ✅       | Amounts, dates
  Create Transfer                |  ✅    |   ❌       | Operations
  Approve Transfer               |  ✅    |   ❌       | Manager
  Cancel Transfer                |  ✅    |   ❌       | Operations
  Post GL Entries                |  ✅    |   ✅       | Journal entries
─────────────────────────────────|────────|────────────|──────────────
LOAN MANAGEMENT
  View Loans                     |  ✅    |   ✅       | Read-only
  View Loan Details              |  ✅    |   ✅       | Terms, balance
  Create Loan                    |  ✅    |   ❌       | Loan officer
  Calculate Interest             |  ✅    |   ✅       | Accrual entries
  Record Payment                 |  ✅    |   ❌       | Operations
  Post GL Entries                |  ✅    |   ✅       | Journal entries
─────────────────────────────────|────────|────────────|──────────────
CARD MANAGEMENT
  View Cards                     |  ✅    |   ✅       | Read-only
  View Card Activity             |  ✅    |   ✅       | Transactions
  Issue Card                     |  ✅    |   ❌       | Card officer
  Block/Unblock Card             |  ✅    |   ❌       | Card officer
  Charge Fees                    |  ✅    |   ✅       | Journal entries
  Post GL Entries                |  ✅    |   ✅       | Fees & interest
─────────────────────────────────|────────|────────────|──────────────
CUSTOMER TRANSACTIONS
  View Transactions              |  ✅    |   ✅       | Read-only
  View Transaction Details       |  ✅    |   ✅       | Amounts, dates
  Process Transactions           |  ✅    |   ❌       | Teller/Ops
  Reverse Transaction            |  ✅    |   ❌       | Teller/Ops
  Post GL Entries                |  ✅    |   ✅       | Journal entries
─────────────────────────────────|────────|────────────|──────────────
CUSTOMER ACCOUNTS
  View Accounts                  |  ✅    |   ✅       | Read-only
  View Account Details           |  ✅    |   ✅       | Balance, info
  Create Account                 |  ✅    |   ❌       | Customer service
  Edit Account                   |  ✅    |   ❌       | Customer service
  Close Account                  |  ✅    |   ❌       | Operations
  Reconcile Accounts             |  ✅    |   ✅       | GL verification
─────────────────────────────────|────────|────────────|──────────────
BANKING REPORTS
  View Reports                   |  ✅    |   ✅       | Read-only
  Generate Reports               |  ✅    |   ✅       | Ad-hoc
  Schedule Reports               |  ✅    |   ⚠️       | Own schedules
  Email Reports                  |  ✅    |   ✅       | To authorized
  Export Reports                 |  ✅    |   ✅       | CSV, Excel
─────────────────────────────────|────────|────────────|──────────────
AUDIT LOGS
  View Logs                      |  ✅    |   ⚠️       | Own entries
  Search Logs                    |  ✅    |   ⚠️       | Own entries
  Export Logs                    |  ✅    |   ❌       | Security
  Modify Logs                    |  ✅    |   ❌       | Never
  Delete Logs                    |  ✅    |   ❌       | Never
─────────────────────────────────|────────|────────────|──────────────
SYSTEM REPORTS
  View Reports                   |  ✅    |   ✅       | Read-only
  Generate Reports               |  ✅    |   ✅       | Ad-hoc reports
  Schedule Reports               |  ✅    |   ⚠️       | Own schedules
  Email Reports                  |  ✅    |   ✅       | Authorized
  Delete Reports                 |  ✅    |   ❌       | Archive
─────────────────────────────────|────────|────────────|──────────────
ADMIN FUNCTIONS
  User Management                |  ✅    |   ❌       | Create/Edit users
  Role Management                |  ✅    |   ❌       | Roles/Permissions
  View Admin Panel               |  ✅    |   ❌       | Restricted
  System Settings                |  ✅    |   ❌       | Configuration
  Backup/Restore                 |  ✅    |   ❌       | System
  Database Management            |  ✅    |   ❌       | Direct access
  Security Settings              |  ✅    |   ❌       | Policies
  Approval Queue (Admin)         |  ✅    |   ❌       | Critical approvals
─────────────────────────────────|────────|────────────|──────────────
```

**Legend:**
- ✅ = Full Access (Create, Read, Update, Delete as appropriate)
- ⚠️ = Limited Access (Own entries only, read-only, or restricted actions)
- ❌ = No Access (Denied)

---

## DETAILED ACCESS SPECIFICATIONS

### 1. Admin - Complete Authority

#### Admin Capabilities

**A. System Administration**
```
✅ Can:
  • Install software updates and patches
  • Configure system settings
  • Manage system integrations
  • Monitor system performance
  • Manage database backups
  • Perform data recovery
  • Configure security policies
  • Enable/disable features
```

**B. User Management**
```
✅ Can:
  • Create new user accounts
  • Assign roles to users
  • Deactivate user accounts
  • Reset user passwords
  • Unlock locked accounts
  • Modify user details (email, phone, etc.)
  • View all user information
  • Audit user activity
```

**C. Role and Permission Management**
```
✅ Can:
  • Create new roles
  • Modify role names and descriptions
  • Assign permissions to roles
  • Create custom permission sets
  • View all role assignments
  • Audit permission changes
  • Create temporary permission elevations
  • Enforce permission policies
```

**D. Financial System Access**
```
✅ Can:
  • Access ALL financial data
  • View confidential reports
  • Modify transaction records (if needed for recovery)
  • Override business rules
  • Approve critical transactions
  • Audit financial operations
  • Modify accounting entries (recovery only)
  • Access audit trails
```

**E. Security Functions**
```
✅ Can:
  • Change password policies
  • Configure authentication methods
  • Enable/disable MFA
  • Manage session timeouts
  • Configure IP whitelisting
  • Manage API keys
  • Audit login attempts
  • Respond to security breaches
```

---

### 2. Accountant - Accounting Operations Only

#### Accountant Capabilities

**A. Journal Entry Management**

```
✅ Can Do:
  • CREATE new journal entries
    └─ Provide description
    └─ Select accounts from Chart of Accounts
    └─ Enter debit and credit amounts
    └─ Add supporting documentation
  
  • REVIEW draft entries
    └─ Check for balancing
    └─ Verify account coding
    └─ Validate amounts
    └─ Request corrections
  
  • POST entries to General Ledger
    └─ After verification
    └─ Mark as "Posted"
    └─ Creates GL impact
  
  • REVERSE entries (for corrections)
    └─ Creates reversing entry
    └─ Documents reason
    └─ Maintains audit trail
  
  • SEARCH and FILTER entries
    └─ By date range
    └─ By account
    └─ By description
    └─ By status

❌ Cannot Do:
  • Delete entries (must reverse)
  • Modify posted entries (must reverse)
  • Edit others' draft entries
  • Force-post unbalanced entries
  • Access draft entries from other periods
```

**Example Workflow:**

```
1. Accountant receives daily banking summary
2. Creates Journal Entry JE-001
   ├─ Debit: Bank Account (1010) - $5,000
   └─ Credit: Customer Deposit Liability (2100) - $5,000
3. Saves as "Draft"
4. Reviews for:
   ├─ Balancing: $5,000 = $5,000 ✓
   ├─ Accounts: Both valid ✓
   ├─ Description: Clear ✓
5. Changes status to "Posted"
6. GL is automatically updated
   ├─ Account 1010 balance increases $5,000
   └─ Account 2100 balance increases $5,000
7. GL transaction record created
```

**B. General Ledger Management**

```
✅ Can Do:
  • VIEW accounts and balances
    └─ Read current account balances
    └─ View account definitions
    └─ See account classifications
  
  • VIEW transactions and history
    └─ See all posted GL entries
    └─ Track running balance
    └─ Sort by date/account
  
  • ANALYZE accounts
    └─ Identify unusual transactions
    └─ Calculate ratios and trends
    └─ Prepare for reporting

❌ Cannot Do:
  • Directly edit account balances
  • Delete GL records
  • Modify account hierarchy
  • Create new accounts
  • Deactivate accounts
```

**C. Trial Balance Management**

```
✅ Can Do:
  • GENERATE trial balance report
    └─ For any accounting period
    └─ Shows all account balances
    └─ Debit/Credit summary
  
  • REVIEW for accuracy
    └─ Verify Debits = Credits
    └─ Investigate imbalances
    └─ Search for posting errors
  
  • APPROVE trial balance
    └─ Once verified
    └─ Sign off for financial statements
    └─ Lock if needed
  
  • INVESTIGATE issues
    └─ Check recent entries
    └─ Review reconciliations
    └─ Request corrections

❌ Cannot Do:
  • Force balance if unequal
  • Delete trial balance records
  • Modify prior periods' trial balances
  • Lock without approval
```

**Example Workflow:**

```
1. End of day, generate Trial Balance
2. Review Report:
   ├─ Total Debits: $2,500,000
   ├─ Total Credits: $2,495,000
   └─ Imbalance: -$5,000
3. Investigate:
   ├─ Check recent journal entries
   ├─ Review GL for errors
   ├─ Found: Entry JE-045 not fully posted
4. Create correction entry JE-046
   ├─ Debit: Correction Account
   └─ Credit: Cash
5. Re-run Trial Balance:
   ├─ Total Debits: $2,500,000
   ├─ Total Credits: $2,500,000
   └─ Balanced! ✓
6. Approve Trial Balance
```

**D. Financial Statement Generation**

```
✅ Can Do:
  • PREPARE financial statements
    └─ Balance Sheet (Assets, Liabilities, Equity)
    └─ Income Statement (Revenue, Expenses, Net Income)
    └─ Cash Flow Statement (Operating, Investing, Financing)
  
  • ADD notes and disclosures
    └─ Accounting policy notes
    └─ Significant transaction details
    └─ Contingencies or risks
  
  • REVIEW for accuracy
    └─ All amounts from GL
    └─ Proper classification
    └─ Arithmetically correct
  
  • DISTRIBUTE to authorized users
    └─ Management
    └─ Auditors
    └─ Board of directors
  
  • ARCHIVE statements
    └─ Save to repository
    └─ Maintain audit trail

❌ Cannot Do:
  • Modify accounts used for statements
  • Delete statements
  • Change accounting policies
  • Bypass audit procedures
  • Distribute to unauthorized recipients
```

**Example Balance Sheet:**

```
Accountant Prepares:
FINSYS BANK - BALANCE SHEET
As of November 22, 2025

ASSETS:
  Bank Accounts (from GL 1010)          $1,009,900
  Investments (from GL 1020)              $200,000
  Loans Receivable (from GL 1030)         $450,000
TOTAL ASSETS                            $1,659,900

LIABILITIES:
  Customer Deposits (from GL 2100)        $505,000
  Accounts Payable (from GL 2200)         $125,000
TOTAL LIABILITIES                         $630,000

EQUITY:
  Capital Stock (from GL 3100)            $800,000
  Retained Earnings (from GL 3200)        $229,900
TOTAL EQUITY                            $1,029,900

TOTAL LIABILITIES + EQUITY              $1,659,900

Accountant Notes:
✓ All amounts from approved GL
✓ Trial Balance balanced
✓ Ratios reviewed and reasonable
✓ Ready for approval
```

**E. Accounting Report Generation**

```
✅ Can Do:
  • GENERATE standard reports
    └─ Journal Entry Register
    └─ GL Detail Report
    └─ Account Analysis
    └─ Period Summary
  
  • GENERATE custom reports
    └─ Ad-hoc analysis
    └─ Specific account ranges
    └─ Date range filtering
  
  • EXPORT reports
    └─ PDF format
    └─ Excel format
    └─ CSV for analysis
  
  • SCHEDULE recurring reports
    └─ Daily
    └─ Weekly
    └─ Monthly
    └─ Auto-email to recipients
  
  • ARCHIVE reports
    └─ For audit trail
    └─ Historical comparison

❌ Cannot Do:
  • Delete reports
  • Modify underlying data for reports
  • Distribute without authorization
  • Change report parameters after release
```

**F. Audit Logs - Limited Access**

```
✅ Can Do:
  • VIEW own entries
    └─ Entries created by this accountant
    └─ Changes made by this accountant
    └─ Date and time stamps
  
  • SEARCH own entries
    └─ By date range
    └─ By module
    └─ By action type

❌ Cannot Do:
  • View other users' entries
  • Export audit logs
  • Delete any audit records
  • Modify audit records
  • Search across all users' activities
```

---

## IMPLEMENTATION CODE

### SQL: Setup RolePermissions for Accountant

```sql
-- Create or verify Accountant role permissions
-- Execute as Admin or Database Administrator

USE [BFASdatabase]
GO

-- Clear existing Accountant permissions (if updating)
DELETE FROM RolePermissions WHERE RoleName = 'Accountant';

-- Insert Accountant Full Access Permissions
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES
  -- Full Control: Accounting Operations
  ('Accountant', 'JournalEntries', 'Full', 1),
  ('Accountant', 'TrialBalance', 'Full', 1),
  ('Accountant', 'FinancialStatements', 'Full', 1),
  ('Accountant', 'AccountingReports', 'Full', 1),
  
  -- Read-Only: General Ledger & Chart of Accounts
  ('Accountant', 'GeneralLedger', 'Read', 1),
  ('Accountant', 'ChartOfAccounts', 'Read', 1),
  
  -- Read-Only: Banking Operations (for verification)
  ('Accountant', 'BankAccounts', 'Read', 1),
  ('Accountant', 'FundTransfers', 'Read', 1),
  ('Accountant', 'LoanManagement', 'Read', 1),
  ('Accountant', 'CardManagement', 'Read', 1),
  ('Accountant', 'CustomerTransactions', 'Read', 1),
  ('Accountant', 'CustomerAccounts', 'Read', 1),
  ('Accountant', 'BankingReports', 'Read', 1),
  
  -- Read-Only: Audit & System Reports
  ('Accountant', 'AuditLogs', 'Read', 1),
  ('Accountant', 'SystemReports', 'Read', 1);

-- Verify permissions were inserted
SELECT * FROM RolePermissions WHERE RoleName = 'Accountant' ORDER BY ModuleName;
```

### SQL: Setup Admin Role Permissions

```sql
-- Full access for Admin (typically set at system setup)

USE [BFASdatabase]
GO

-- Insert Admin Full Access (All Permissions)
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES
  ('Admin', 'JournalEntries', 'Full', 1),
  ('Admin', 'GeneralLedger', 'Full', 1),
  ('Admin', 'TrialBalance', 'Full', 1),
  ('Admin', 'FinancialStatements', 'Full', 1),
  ('Admin', 'AccountingReports', 'Full', 1),
  ('Admin', 'ChartOfAccounts', 'Full', 1),
  ('Admin', 'BankAccounts', 'Full', 1),
  ('Admin', 'FundTransfers', 'Full', 1),
  ('Admin', 'LoanManagement', 'Full', 1),
  ('Admin', 'CardManagement', 'Full', 1),
  ('Admin', 'CustomerTransactions', 'Full', 1),
  ('Admin', 'CustomerAccounts', 'Full', 1),
  ('Admin', 'BankingReports', 'Full', 1),
  ('Admin', 'AuditLogs', 'Full', 1),
  ('Admin', 'SystemReports', 'Full', 1),
  ('Admin', 'UserManagement', 'Full', 1),
  ('Admin', 'RoleManagement', 'Full', 1),
  ('Admin', 'SystemSettings', 'Full', 1),
  ('Admin', 'Security', 'Full', 1),
  ('Admin', 'BackupRestore', 'Full', 1);
```

### C# Code: Check User Permissions

```csharp
// Example: Authorization service to check permissions

public class AuthorizationService
{
    private readonly IDbContext _dbContext;
    
    public AuthorizationService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Check if user has permission to access a module
    /// </summary>
    public bool HasModuleAccess(int userId, string moduleName)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return false;
        
        var permission = _dbContext.RolePermissions.FirstOrDefault(p =>
            p.RoleName == user.Role &&
            p.ModuleName == moduleName &&
            p.IsActive);
        
        return permission != null;
    }
    
    /// <summary>
    /// Check if user can perform specific action in module
    /// </summary>
    public bool CanPerformAction(int userId, string moduleName, string action)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return false;
        
        var permission = _dbContext.RolePermissions.FirstOrDefault(p =>
            p.RoleName == user.Role &&
            p.ModuleName == moduleName &&
            p.IsActive);
        
        if (permission == null) return false;
        
        // Check if permission level allows action
        return HasPermission(permission.Permission, action);
    }
    
    private bool HasPermission(string permissionLevel, string requiredAction)
    {
        return permissionLevel switch
        {
            "Full" => true,
            "Write" => requiredAction == "Read" || requiredAction == "Write",
            "Read" => requiredAction == "Read",
            "Delete" => true, // Can do anything if delete is granted
            _ => false
        };
    }
    
    /// <summary>
    /// Audit log the access attempt
    /// </summary>
    public void LogAccess(int userId, string module, string action, bool success)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Module = module,
            Description = $"{action} on {module}",
            CreatedAt = DateTime.Now
        };
        
        _dbContext.AuditLogs.Add(auditLog);
        _dbContext.SaveChanges();
    }
}

// Usage in Controller:
[HttpGet("journal-entries")]
public IActionResult GetJournalEntries()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier);
    
    if (!_authService.CanPerformAction(userId, "JournalEntries", "Read"))
    {
        _authService.LogAccess(userId, "JournalEntries", "Read", false);
        return Forbid("You do not have access to Journal Entries");
    }
    
    var entries = _dbContext.JournalEntries.ToList();
    _authService.LogAccess(userId, "JournalEntries", "Read", true);
    return Ok(entries);
}
```

### ASP.NET Core Authorization Attribute

```csharp
// Custom attribute for role-based access control

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _moduleName;
    private readonly string _requiredPermission;
    
    public RequireModuleAccessAttribute(string moduleName, string requiredPermission = "Read")
    {
        _moduleName = moduleName;
        _requiredPermission = requiredPermission;
    }
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var authService = context.HttpContext.RequestServices
            .GetService<AuthorizationService>();
        
        if (!authService.CanPerformAction(int.Parse(userId), _moduleName, _requiredPermission))
        {
            authService.LogAccess(int.Parse(userId), _moduleName, _requiredPermission, false);
            context.Result = new ForbidResult("Access Denied");
            return;
        }
        
        authService.LogAccess(int.Parse(userId), _moduleName, _requiredPermission, true);
    }
}

// Usage on Controller Methods:

[HttpPost("journal-entries")]
[RequireModuleAccessAttribute("JournalEntries", "Write")]
public IActionResult CreateJournalEntry([FromBody] JournalEntryDto dto)
{
    // Only Accountant and Admin reach here
    // Teller, Customer get 403 Forbidden
    
    var journalEntry = new JournalEntry { /* map from dto */ };
    _dbContext.JournalEntries.Add(journalEntry);
    _dbContext.SaveChanges();
    
    return Created($"/api/journal-entries/{journalEntry.JournalId}", journalEntry);
}

[HttpGet("trial-balance")]
[RequireModuleAccessAttribute("TrialBalance", "Read")]
public IActionResult GetTrialBalance([FromQuery] DateTime periodEnd)
{
    // Accountant and Admin can view
    // Teller, Customer get 403 Forbidden
    
    var trialBalance = _dbContext.TrialBalances
        .Where(tb => tb.PeriodEnd == periodEnd)
        .ToList();
    
    return Ok(trialBalance);
}

[HttpGet("admin/users")]
[Authorize(Roles = "Admin")]
public IActionResult GetAllUsers()
{
    // Only Admin reaches here
    // All other roles get 403 Forbidden
    
    var users = _dbContext.Users.ToList();
    return Ok(users);
}
```

---

## SECURITY POLICIES

### 1. Principle of Least Privilege

**Policy:** Each user receives the MINIMUM permissions required to perform their job.

```
✅ Correct:
  Accountant Role:
  • Full Access: JournalEntries, TrialBalance, FinancialStatements
  • Read-Only: GeneralLedger, BankAccounts, FundTransfers
  • No Access: Admin functions, User management, System settings
  
  Result: Accountant can do job, cannot cause system damage

❌ Incorrect:
  Accountant Role:
  • Full Access: ALL modules including Admin Panel
  
  Result: Accountant could delete critical data, change permissions
```

### 2. Separation of Duties

**Policy:** No single person has complete control over a transaction from start to finish.

```
✅ Correct:
  Transaction Recording:
  • Teller creates/initiates transaction
  • Operations team verifies
  • Accountant posts to GL
  • Manager reviews
  • Auditor confirms
  
  Result: Multiple checkpoints prevent fraud

❌ Incorrect:
  • Same person: Creates, verifies, posts, approves, audits
  
  Result: Easy to commit fraud undetected
```

### 3. Need-to-Know Basis

**Policy:** Users can only access data relevant to their role.

```
✅ Correct:
  Accountant: Can see all financial data (needs for GL)
  Teller: Can only see own transactions
  Loan Officer: Can see loan data but not depositor information
  
  Result: Protects customer privacy, reduces information leakage

❌ Incorrect:
  All employees: Can see all customer account data
  
  Result: Privacy violations, information leaks
```

### 4. Regular Access Reviews

**Policy:** Audit and verify permissions quarterly.

```
Quarterly Review Procedure:
1. Generate list of all users and their roles
2. Verify each person still needs their current role
3. Remove access for:
   ├─ Terminated employees
   ├─ Transferred employees
   ├─ Promoted employees (update role)
   └─ On extended leave
4. Document any changes
5. Have manager sign off on approval
6. Archive for compliance
```

### 5. Audit Trail for All Access

**Policy:** Every system access is logged and can be audited.

```
Logged for Each Access:
  ✓ Who (UserId, Username)
  ✓ What (Module, Action)
  ✓ When (Timestamp)
  ✓ Where (IP Address)
  ✓ Result (Success/Failure)

Example Log Entry:
  11/22/2025 09:15:00 | User 42 (jsmith) | JournalEntries | Create |
  Success | 192.168.1.100 | Chrome on Windows

Auditor Can Use For:
  • Detect unauthorized access attempts
  • Verify legitimate use
  • Investigate suspicious activity
  • Comply with regulations
  • Discipline policy violations
```

### 6. Password and Session Security

**Policy:** Strong authentication and session management.

```
Password Requirements:
  ✓ Minimum 12 characters
  ✓ Mix of Upper, Lower, Numbers, Symbols
  ✓ Change every 90 days
  ✓ Cannot reuse last 5 passwords
  ✓ Lockout after 5 failed attempts

Session Management:
  ✓ 30-minute inactivity timeout
  ✓ Force logout at end of day
  ✓ One active session per user
  ✓ Requires re-authentication for sensitive operations
```

### 7. Data Encryption

**Policy:** Sensitive data is encrypted at rest and in transit.

```
Encryption Standards:
  ✓ AES-256 for data at rest
  ✓ TLS 1.2+ for data in transit
  ✓ All passwords hashed with bcrypt
  ✓ PII fields encrypted in database
  ✓ Secure key management
```

---

## AUDIT & COMPLIANCE

### Audit Trail Report

```sql
-- Query to audit Accountant activities

SELECT
    a.AuditId,
    u.Username,
    u.Role,
    a.Action,
    a.Module,
    a.Description,
    a.CreatedAt,
    CASE 
        WHEN a.OldValues IS NOT NULL THEN 'Modified'
        WHEN a.NewValues IS NOT NULL AND a.OldValues IS NULL THEN 'Created'
        ELSE 'Accessed'
    END AS ActivityType
FROM AuditLogs a
JOIN Users u ON a.UserId = u.UserId
WHERE u.Role = 'Accountant'
  AND a.CreatedAt >= DATEADD(MONTH, -1, GETDATE())
ORDER BY a.CreatedAt DESC;
```

### Compliance Verification

```
Monthly Compliance Checklist:

□ All Accountant entries have proper approval
□ No unauthorized modifications to GL
□ All journal entries balanced
□ Trial Balance reviewed and approved
□ Financial statements distributed only to authorized users
□ Audit logs intact and unmodified
□ No unauthorized access attempts
□ Passwords changed by scheduled date
□ Training up to date for compliance
□ Segregation of duties maintained
□ All transactions traceable to source documents
```

---

## SUMMARY

**Admin Role:**
- Full, unrestricted access to ALL system functions
- Can create/modify users, roles, and permissions
- Can access all financial and administrative data
- Responsible for system security and compliance
- Should be restricted to IT and senior management only

**Accountant Role:**
- Full control over accounting modules (Journal Entries, Trial Balance, Financial Statements)
- Read-only access to operational modules (Bank Accounts, Transfers, Loans, Cards)
- Cannot access admin functions or system settings
- Focused on financial recording, validation, and reporting
- Maintains audit trail through AuditLogs table

**Key Principle:**
Accountants have everything they need to do accounting work, but nothing they can use to damage the system or bypass controls.

---

**Document Version:** 1.0  
**Last Updated:** November 22, 2025  
**Classification:** Internal Use  
**Owner:** IT Security & Compliance

