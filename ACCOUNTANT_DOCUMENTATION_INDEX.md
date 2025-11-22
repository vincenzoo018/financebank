# ACCOUNTANT ROLE COMPREHENSIVE DOCUMENTATION INDEX

**FinanceBank - FINSYS Accounting System**  
**Complete Reference Guide for Accountant Operations & System Architecture**

**Created:** November 22, 2025  
**Version:** 1.0  
**Classification:** Internal Use

---

## QUICK NAVIGATION

### 📋 Core Documentation Files

| Document | Purpose | Length | Audience |
|----------|---------|--------|----------|
| **ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md** | Full workflow, responsibilities, module interactions, daily operations | 50+ pages | Accountants, Managers, Auditors |
| **ROLE_BASED_ACCESS_CONTROL_GUIDE.md** | Admin vs Accountant permissions, RBAC implementation, security policies | 40+ pages | IT, Admins, Security Officers |
| **ACCOUNTANT_DATABASE_MAPPING_GUIDE.md** | Database schema, table relationships, data flow, SQL queries | 35+ pages | Developers, DBAs, Advanced Users |

---

## EXECUTIVE SUMMARY

### What is the Accountant Role?

The **Accountant** is the financial record-keeper who:
- ✅ Records financial transactions via Journal Entries
- ✅ Maintains the General Ledger (the "books")
- ✅ Validates accuracy through Trial Balance
- ✅ Prepares Financial Statements
- ✅ Generates Accounting Reports
- ✅ Coordinates with Banking Operations
- ❌ Cannot access Admin functions or system settings
- ❌ Cannot modify system-critical configurations

### Accountant Responsibilities Summary

```
┌─────────────────────────────────────────────────────────────┐
│                  ACCOUNTANT WORKFLOW                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. RECEIVE → Daily banking transactions from operations   │
│      ↓                                                      │
│  2. RECORD → Create journal entries for each transaction   │
│      ↓                                                      │
│  3. POST → Move entries to General Ledger                 │
│      ↓                                                      │
│  4. VALIDATE → Run Trial Balance (Debits = Credits?)       │
│      ↓                                                      │
│  5. REPORT → Generate Financial Statements                 │
│      ↓                                                      │
│  6. ANALYZE → Create Accounting Reports                    │
│      ↓                                                      │
│  7. COORDINATE → Work with other departments               │
│      ↓                                                      │
│  8. CLOSE → End-of-period closing procedures              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Key Access Rights

```
✅ FULL ACCESS:
   • Journal Entries (Create, Edit, Post, Reverse)
   • Trial Balance (Generate, Review, Approve)
   • Financial Statements (Prepare, Generate, Distribute)
   • Accounting Reports (All types)

⚠️  READ-ONLY ACCESS:
   • General Ledger (View balances, review transactions)
   • Bank Accounts, Fund Transfers, Loans, Cards (Verify data)
   • Banking Reports (Understand operations)
   • Audit Logs (View own entries)

❌ NO ACCESS:
   • Admin Panel
   • System Settings
   • User Management
   • Role & Permissions
   • Backup/Restore
```

---

## DETAILED DOCUMENTATION BREAKDOWN

### 1. ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md

**📖 What's Inside:**

This document provides the complete operational guide for the Accountant role.

**Contents:**
- ✅ Executive Overview (What is an Accountant?)
- ✅ 9-Step Workflow with detailed explanations:
  1. Banking Operations Generate Transactions
  2. Accountant Receives Daily Financial Activity
  3. Accountant Posts/Reviews Journal Entries
  4. Entries Flow into General Ledger
  5. Accountant Reviews Trial Balance
  6. Accountant Generates Financial Statements
  7. Accountant Reviews and Produces Reports
  8. Accountant Coordinates With Other Banking Units
  9. Period-End Closing
- ✅ Module Interactions (complete map of all modules)
- ✅ Database Mapping (high-level table overview)
- ✅ Role-Based Access (permissions matrix)
- ✅ Daily Operations Checklist
- ✅ Period-End Close Process (5-day procedure)
- ✅ Coordination & Escalation Guidelines

**Best For:**
- New accountants learning the system
- Managers understanding accountability
- Auditors reviewing procedures
- System designers understanding requirements

**Key Sections to Read:**
1. Start with "FLOW 1: Banking Operations Generate Transactions"
2. Follow through "FLOW 9: Period-End Closing"
3. Reference "DAILY OPERATIONS CHECKLIST" for daily work

---

### 2. ROLE_BASED_ACCESS_CONTROL_GUIDE.md

**🔐 What's Inside:**

Complete guide to role-based access control and permissions.

**Contents:**
- ✅ System Overview (RBAC principles)
- ✅ Role Definitions:
  - Admin (Full Access)
  - Accountant (Limited, Accounting-Focused)
- ✅ Comprehensive Permission Matrix (30+ modules/functions)
- ✅ Detailed Access Specifications:
  - What Admin Can Do (15+ categories)
  - What Accountant Can Do (6+ capability areas)
  - What Neither Can Do (restrictions)
- ✅ Implementation Code:
  - SQL: RolePermissions setup
  - C# Authorization Service
  - ASP.NET Core Attributes
- ✅ Security Policies:
  - Principle of Least Privilege
  - Separation of Duties
  - Need-to-Know Basis
  - Access Reviews
  - Audit Trails
  - Password & Session Security
  - Data Encryption
- ✅ Audit & Compliance

**Best For:**
- IT/Admin teams implementing access control
- Security officers reviewing permissions
- Compliance auditors verifying controls
- Developers implementing authorization

**Key Sections to Read:**
1. "PERMISSION MATRIX" for quick reference
2. "DETAILED ACCESS SPECIFICATIONS" for deep dive
3. "IMPLEMENTATION CODE" for development
4. "SECURITY POLICIES" for controls

---

### 3. ACCOUNTANT_DATABASE_MAPPING_GUIDE.md

**🗄️ What's Inside:**

Complete database schema reference for accounting operations.

**Contents:**
- ✅ Overview of data model and accounting cycle flow
- ✅ Core Accounting Tables (7 tables):
  1. JournalEntries (header info)
  2. JournalEntryLines (detail debits/credits)
  3. GeneralLedger (account master)
  4. GeneralLedgerTransactions (running balance)
  5. TrialBalance (validation report)
  6. FinancialStatements (statement archive)
  7. ChartOfAccounts (account reference)
- ✅ Supporting Tables (6 tables for source data):
  1. CustomerTransactions
  2. FundTransfers
  3. BankAccounts (reconciliation)
  4. CustomerLoans (interest calculation)
  5. CustomerCards (fees & interest)
  6. AuditLogs (compliance trail)
- ✅ Integration Tables (RolePermissions, Users)
- ✅ Data Flow Diagrams (ASCII visualization)
- ✅ 8 Complete SQL Query Examples:
  - Daily transaction summary
  - Account transaction detail
  - Balance reconciliation
  - Trial balance generation
  - Unposted entries
  - Audit trail review
  - Interest accrual
  - Month-end checklist
- ✅ Accounting Cycle Support Map

**Best For:**
- Database administrators
- Developers building accounting features
- Data analysts querying financial data
- DBAs designing backups/recovery

**Key Sections to Read:**
1. "CORE ACCOUNTING TABLES" for schema
2. "DATA FLOW DIAGRAMS" for visualization
3. "QUERY REFERENCE" for SQL examples
4. "ACCOUNTING CYCLE SUPPORT" for relationships

---

## HOW TO USE THIS DOCUMENTATION

### Scenario 1: New Accountant Starting Work

```
1. Read: ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
   ├─ Executive Overview (understand role)
   ├─ Flow 1-3 (understand daily tasks)
   └─ Daily Operations Checklist (your daily guide)

2. Reference: ROLE_BASED_ACCESS_CONTROL_GUIDE.md
   ├─ What Accountant Can Do section
   └─ Understand your permissions

3. Use: ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
   └─ As reference while using the system
       (what table stores what data?)
```

### Scenario 2: System Administrator Setting Up Access

```
1. Read: ROLE_BASED_ACCESS_CONTROL_GUIDE.md
   ├─ Role Definitions
   ├─ Permission Matrix
   ├─ Implementation Code section
   └─ SQL for RolePermissions setup

2. Execute: SQL Scripts
   ├─ Setup RolePermissions for Accountant
   └─ Verify permissions inserted correctly

3. Verify: Authorization checks
   ├─ Test C# AuthorizationService
   └─ Confirm [RequireModuleAccess] attributes work
```

### Scenario 3: Auditor Reviewing Accounting Procedures

```
1. Read: ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
   ├─ Full workflow overview
   ├─ Daily Operations Checklist
   └─ Period-End Close Process

2. Review: ROLE_BASED_ACCESS_CONTROL_GUIDE.md
   ├─ Audit & Compliance section
   ├─ Security Policies
   └─ Access matrices

3. Query: ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
   ├─ Audit Trail queries
   ├─ Trial Balance queries
   └─ Reconciliation queries
```

### Scenario 4: Developer Building Accounting Module

```
1. Understand: ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
   ├─ What accountants do (requirements)
   ├─ Module interactions (system design)
   └─ Database mapping high-level (architecture)

2. Design: ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
   ├─ Core accounting tables (schema)
   ├─ Table relationships (foreign keys)
   ├─ Data flow diagrams (architecture)
   └─ SQL queries (implementation)

3. Implement: ROLE_BASED_ACCESS_CONTROL_GUIDE.md
   ├─ Authorization Service code
   ├─ ASP.NET Core attributes
   ├─ RolePermissions SQL
   └─ Security policies
```

---

## KEY CONCEPTS

### The Accounting Equation

```
ASSETS = LIABILITIES + EQUITY

Or rearranged:
Assets - Liabilities = Equity

Example:
Bank has $1,000,000 in assets
Owes $300,000 in liabilities
Therefore has $700,000 in equity (for owners)

Every journal entry must maintain this equation!
```

### Double-Entry Bookkeeping

```
Every transaction affects two accounts:

DEPOSIT OF $5,000:
  Debit:   Bank Account (Asset)          +$5,000
  Credit:  Customer Deposit (Liability)          +$5,000
  
Effect: Bank's assets increase, bank's liabilities increase
        (bank holds customer's money, so it's a liability)
```

### The Accounting Cycle

```
1. RECORD → Journal Entries (What happened)
2. POST → General Ledger (Where it went)
3. VALIDATE → Trial Balance (Does it balance?)
4. REPORT → Financial Statements (What's the result?)
5. CLOSE → Close period, prepare for next
6. REPEAT → Start next period
```

### Chart of Accounts

```
Hierarchical structure of all valid GL accounts:

ASSETS (Increase with Debits)
  1000 - Current Assets
    1010 - Bank Accounts
      1011 - Checking
      1012 - Savings
    1020 - Investments
  2000 - Fixed Assets
    ...

LIABILITIES (Increase with Credits)
  2000 - Current Liabilities
    2100 - Customer Deposits
    ...

EQUITY (Increase with Credits)
  3000 - Owner's Capital
    3100 - Capital Stock
    3200 - Retained Earnings

INCOME (Increase with Credits)
  4000 - Operating Income
    4100 - Interest Income
    4200 - Service Fees

EXPENSES (Increase with Debits)
  5000 - Operating Expenses
    5100 - Salaries
    5200 - Utilities
```

---

## COMMON TASKS & WHERE TO FIND ANSWERS

### Task 1: Create and Post a Journal Entry

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 3: Accountant Posts/Reviews Journal Entries"
- Example: "Recording a Customer Deposit"

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- Section: "JournalEntries Table" & "JournalEntryLines Table"
- Includes exact SQL examples

**Access Rights in:** ROLE_BASED_ACCESS_CONTROL_GUIDE.md
- Section: "Journal Entry Management"
- What you can/cannot do

### Task 2: Generate Trial Balance

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 5: Accountant Reviews Trial Balance"

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- Section: "TrialBalance Table"
- SQL query: "Query 4: Trial Balance Generation"

**Access Rights in:** ROLE_BASED_ACCESS_CONTROL_GUIDE.md
- Section: "Trial Balance Management"

### Task 3: Prepare Financial Statements

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 6: Accountant Generates Financial Statements"
- Shows Balance Sheet, Income Statement, Cash Flow

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- Section: "FinancialStatements Table"
- Includes JSON structure example

**Access Rights in:** ROLE_BASED_ACCESS_CONTROL_GUIDE.md
- Section: "Financial Statement Preparation"

### Task 4: Review Account Reconciliation

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "MODULE INTERACTIONS"
- Subsection: "Bank Account Reconciliation"

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- Section: "BankAccounts Table"
- SQL query: "Query 3: Account Balance Reconciliation"

### Task 5: Calculate Interest Accrual

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "COORDINATION WITH LOAN OFFICERS"

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- Section: "CustomerLoans Table"
- SQL query: "Query 7: Interest Accrual Calculation"

### Task 6: Investigate Posting Error

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 5: Accountant Reviews Trial Balance"
- Subsection: "When Trial Balance DOES NOT Balance"

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- SQL query: "Query 5: Unposted/Draft Entries"
- SQL query: "Query 6: Audit Trail for an Entry"

### Task 7: Month-End Close

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 9: Period-End Closing"
- Includes 5-day procedure

**Implementation Details in:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md
- SQL query: "Query 8: Month-End Close Checklist"

### Task 8: Generate Accounting Reports

**Find in:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- Section: "FLOW 7: Accountant Reviews and Produces Reports"
- Lists 12 types of reports

**Access Rights in:** ROLE_BASED_ACCESS_CONTROL_GUIDE.md
- Section: "Accounting Report Generation"

---

## DATABASE QUICK REFERENCE

### Critical Tables for Accountants

```
TABLE                   PRIMARY USE              READ/WRITE
────────────────────────────────────────────────────────────
JournalEntries          Record transactions     WRITE
JournalEntryLines       Entry details           WRITE
GeneralLedger           Account balances        READ
GeneralLedgerTxns       GL detail audit         READ
TrialBalance            Validation              READ
FinancialStatements     Reporting               WRITE
ChartOfAccounts         Account reference       READ
CustomerTransactions    Source data             READ
AuditLogs               Compliance              READ
```

### Key Relationships

```
JournalEntries
    ↓ (1-to-many)
JournalEntryLines
    ↓ (creates entries in)
GeneralLedgerTransactions
    ↓ (updates)
GeneralLedger
    ↓ (organized for)
TrialBalance
    ↓ (becomes)
FinancialStatements
```

---

## IMPORTANT PHONE NUMBERS & CONTACTS

### Coordination Contacts

**When to Contact Operations:**
- Transaction discrepancies
- Missing deposits/withdrawals
- Timing differences

**When to Contact Loan Officers:**
- Interest calculation questions
- Loan status changes
- Defaults or modifications

**When to Contact Card Services:**
- Chargeback disputes
- Fee discrepancies
- Interest calculations

**When to Escalate to Management:**
- Unusual transactions
- Financial variances > $X
- Compliance issues
- Time-sensitive items

---

## COMPLIANCE & AUDIT

### Regulations This System Supports

```
✅ Double-Entry Bookkeeping
   Every transaction recorded on both sides

✅ Audit Trail
   Every action logged in AuditLogs
   Who, What, When, Where recorded

✅ Segregation of Duties
   Recording ≠ Posting ≠ Approval
   Multiple checkpoints prevent fraud

✅ Access Controls
   Role-based permissions
   Admin ≠ Accountant ≠ Teller

✅ Financial Reporting
   Standardized statements
   Trial balance requirement

✅ Data Retention
   Historical records archived
   Compliance with regulations
```

### Monthly Compliance Checklist

```
□ All journal entries posted and reconciled
□ Trial balance balanced
□ GL balances verified against source data
□ Bank reconciliation completed
□ Financial statements prepared and reviewed
□ Audit logs reviewed for unauthorized access
□ Period properly closed
□ Backups completed
```

---

## TROUBLESHOOTING GUIDE

### Problem: Trial Balance Doesn't Balance

**Solution:**
1. Check for unposted journal entries (Query 5)
2. Verify journal entries are balanced (TotalDebit = TotalCredit)
3. Search for recently reversed entries
4. Review GL for posting errors
5. Create correction entry if needed

**Reference:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md → FLOW 5

---

### Problem: GL Account Balance Doesn't Match Bank Statement

**Solution:**
1. Check for timing differences (deposit in GL but not bank)
2. Verify all bank transactions posted to GL
3. Review uncleared items (Query 3)
4. Create reconciliation entry
5. Document explanation

**Reference:** ACCOUNTANT_DATABASE_MAPPING_GUIDE.md → Query 3

---

### Problem: Can't Access a Module

**Solution:**
1. Verify your user role is "Accountant"
2. Check RolePermissions table for your role
3. Verify IsActive = 1 for your permission
4. Contact Admin if permission is missing

**Reference:** ROLE_BASED_ACCESS_CONTROL_GUIDE.md → Accountant Access Summary

---

### Problem: Need to Reverse a Posted Entry

**Solution:**
1. Create new journal entry with:
   - Opposite debits/credits
   - Reference original entry
   - Status = Draft
2. Review and verify balances
3. Post to GL
4. Both entries remain (audit trail)

**Reference:** ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md → FLOW 3 → Error Correction

---

## SYSTEM CONFIGURATION

### Login Credentials

**Demo User:**
- Username: jsmith
- Role: Accountant
- Permissions: Full access to accounting modules

**Admin User:**
- Username: admin
- Role: Admin
- Permissions: Full system access

### System Settings

```
Period Close Day: 28th of each month (configurable)
Audit Log Retention: 7 years
Backup Frequency: Daily (midnight)
Trial Balance: Run daily
Password Expiration: 90 days
Session Timeout: 30 minutes inactivity
```

---

## APPENDIX

### A. Accounting Terminology

- **Debit:** Left side entry; increases assets/expenses
- **Credit:** Right side entry; increases liabilities/income/equity
- **Trial Balance:** Report showing all GL accounts with debit/credit balances
- **GL (General Ledger):** Master book of all accounts
- **JE (Journal Entry):** Recording of a transaction
- **Post:** To move entry from journal to GL
- **Reconcile:** To verify balance between GL and source data
- **Close:** To end accounting period and lock records

### B. File Locations

```
Documentation:
  ├─ ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
  ├─ ROLE_BASED_ACCESS_CONTROL_GUIDE.md
  └─ ACCOUNTANT_DATABASE_MAPPING_GUIDE.md

Database:
  └─ BFASdatabase (SQL Server)
      ├─ Tables (15 core tables)
      ├─ Stored Procedures
      └─ Views
```

### C. Related Documentation

- 00_FINANCE_MANAGER_START_HERE.md
- BUILD_AND_RUN.md
- DATABASE_SCHEMA_PART1.sql
- DATABASE_SCHEMA_PART2.sql

---

## DOCUMENT CHANGE HISTORY

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Nov 22, 2025 | Initial release with 3 core documents |

---

## SUPPORT

**Questions?**
1. Search this index for your topic
2. Read relevant section in core documentation
3. Check examples and SQL queries
4. Contact your Finance Manager
5. Escalate to IT/Admin if technical issue

---

## ACKNOWLEDGMENTS

**Created By:** System Documentation Team  
**Reviewed By:** Finance Manager, IT Security  
**Approved By:** Chief Financial Officer  

**Special Thanks To:**
- Accounting Team for workflow insights
- IT Team for access control requirements
- Database Team for schema documentation
- Management for process guidance

---

**END OF DOCUMENTATION INDEX**

---

**For Quick Access:**
- 🔍 **Find workflow details:** → ACCOUNTANT_ROLE_WORKFLOW_COMPLETE.md
- 🔐 **Check permissions:** → ROLE_BASED_ACCESS_CONTROL_GUIDE.md
- 🗄️ **Query database:** → ACCOUNTANT_DATABASE_MAPPING_GUIDE.md

**Last Updated:** November 22, 2025  
**Next Review:** February 22, 2026

