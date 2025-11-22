# ACCOUNTANT ROLE SYSTEM - VISUAL OVERVIEW

**FinanceBank - FINSYS Accounting System**  
**Complete System Architecture & Workflow**

---

## 🏗️ SYSTEM ARCHITECTURE

```
┌─────────────────────────────────────────────────────────────────────┐
│                     FINSYS ACCOUNTING SYSTEM                        │
│                        (FinanceBank)                                │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│   BANKING OPERATIONS    │
├─────────────────────────┤
│ • Deposits/Withdrawals  │
│ • Fund Transfers        │
│ • Loan Disbursements    │
│ • Loan Repayments       │
│ • Card Transactions     │
│ • Fees & Charges        │
│ • Interest Accruals     │
└────────────────┬────────┘
                 │
                 ↓
        ╔════════════════╗
        ║   ACCOUNTANT   ║
        ║   (YOU ARE     ║
        ║    HERE)       ║
        ╚════════┬═══════╝
                 │
         ┌───────┴────────┐
         ↓                ↓
    ┌─────────┐     ┌──────────┐
    │  CREATE │     │  REVIEW  │
    │JOURNAL  │────→│ENTRIES   │
    │ENTRIES  │     │  BALANCE │
    └────┬────┘     └──────┬───┘
         │                 │
         ↓                 ↓
    ┌──────────────────────────┐
    │   POST TO GL             │
    │   (Update Balances)      │
    └────────────┬─────────────┘
                 │
                 ↓
    ┌──────────────────────────┐
    │   GENERAL LEDGER         │
    │   (Master Books)         │
    │                          │
    │  Account Balances:       │
    │  • Assets: $1.7M         │
    │  • Liabilities: $630K    │
    │  • Equity: $1.07M        │
    └────────────┬─────────────┘
                 │
                 ↓
    ┌──────────────────────────┐
    │   TRIAL BALANCE          │
    │   (Validation)           │
    │                          │
    │  Debits = Credits?       │
    │  ✓ YES → Proceed         │
    │  ✗ NO → Investigate      │
    └────────────┬─────────────┘
                 │
                 ↓
    ┌──────────────────────────┐
    │ FINANCIAL STATEMENTS     │
    │ • Balance Sheet          │
    │ • Income Statement       │
    │ • Cash Flow Statement    │
    └────────────┬─────────────┘
                 │
                 ↓
    ┌──────────────────────────┐
    │  ACCOUNTING REPORTS      │
    │  • GL Detail             │
    │  • Account Analysis      │
    │  • Reconciliations       │
    │  • Management Reports    │
    └──────────────────────────┘
```

---

## 🔄 ACCOUNTING CYCLE (Monthly)

```
MONTH BEGINS
     │
     ├─→ ┌─────────────────────────────────────┐
     │   │ DAY 1-27: DAILY OPERATIONS         │
     │   │ • Receive transactions              │
     │   │ • Create journal entries            │
     │   │ • Post to GL                        │
     │   │ • Daily trial balance check         │
     │   └─────────────────────────────────────┘
     │
     ├─→ ┌─────────────────────────────────────┐
     │   │ DAY 28: ADJUSTMENTS                 │
     │   │ • Interest accruals                 │
     │   │ • Depreciation                      │
     │   │ • Loan loss provisions              │
     │   │ • Accrued expenses                  │
     │   └─────────────────────────────────────┘
     │
     ├─→ ┌─────────────────────────────────────┐
     │   │ DAY 29: VALIDATION                  │
     │   │ • Generate trial balance            │
     │   │ • Reconcile accounts                │
     │   │ • Investigate imbalances            │
     │   │ • Approve TB                        │
     │   └─────────────────────────────────────┘
     │
     ├─→ ┌─────────────────────────────────────┐
     │   │ DAY 30: FINANCIAL STATEMENTS        │
     │   │ • Balance sheet                     │
     │   │ • Income statement                  │
     │   │ • Cash flow statement               │
     │   │ • Notes & disclosures               │
     │   └─────────────────────────────────────┘
     │
     └─→ ┌─────────────────────────────────────┐
         │ DAY 31: REPORTING & CLOSING         │
         │ • Generate reports                  │
         │ • Distribute to mgmt & auditors     │
         │ • Lock period                       │
         │ • Backup files                      │
         │ • Open new month                    │
         └─────────────────────────────────────┘

MONTH CLOSED & READY FOR NEXT CYCLE
```

---

## 👥 ROLES & PERMISSIONS

```
┌──────────────────────────────────────────────────────────────────┐
│                    SYSTEM USERS                                 │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────┐         ┌────────────────────┐          │
│  │   ADMIN            │         │   ACCOUNTANT       │          │
│  │   (FULL ACCESS)    │         │   (LIMITED ACCESS) │          │
│  ├────────────────────┤         ├────────────────────┤          │
│  │ ✅ Everything      │         │ ✅ Accounting Only │          │
│  │ ├─ Users           │         │ ├─ Journal Entries │          │
│  │ ├─ Roles           │         │ ├─ Trial Balance   │          │
│  │ ├─ Settings        │         │ ├─ Statements     │          │
│  │ ├─ Accounting      │         │ ├─ Reports        │          │
│  │ ├─ Banking         │         │ └─ GL (read-only) │          │
│  │ └─ System          │         │                    │          │
│  │                    │         │ ⚠️ Can View        │          │
│  │ Security Role:     │         │ ├─ Bank Accounts  │          │
│  │ Restricted to IT   │         │ ├─ Transfers      │          │
│  │ Only               │         │ ├─ Loans          │          │
│  │                    │         │ └─ Cards          │          │
│  │                    │         │                    │          │
│  │                    │         │ ❌ Cannot Access   │          │
│  │                    │         │ ├─ Admin Panel    │          │
│  │                    │         │ ├─ Settings      │          │
│  │                    │         │ ├─ User Mgmt     │          │
│  │                    │         │ └─ System Access │          │
│  └────────────────────┘         └────────────────────┘          │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 📊 DATA FLOW - JOURNAL ENTRY TO FINANCIAL STATEMENTS

```
STEP 1: BANKING OPERATION
┌─────────────────────────────────┐
│ Customer Deposit: $5,000        │
└────────────┬────────────────────┘
             │
             ↓
STEP 2: CREATE JOURNAL ENTRY
┌─────────────────────────────────┐
│ JournalEntries Record:          │
│ • JournalId: 1001               │
│ • JournalNumber: JE-20251122-01 │
│ • Description: Customer deposit │
│ • TotalDebit: 5000.00           │
│ • TotalCredit: 5000.00 ✓        │
│ • Status: Draft                 │
└────────────┬────────────────────┘
             │
             ↓
STEP 3: CREATE DETAIL LINES
┌─────────────────────────────────┐
│ JournalEntryLines Records:      │
│                                 │
│ Line 1:                         │
│ • AccountCode: 1010             │
│ • AccountName: Bank Account     │
│ • DebitAmount: 5000.00          │
│ • CreditAmount: 0.00            │
│                                 │
│ Line 2:                         │
│ • AccountCode: 2100             │
│ • AccountName: Cust Deposits    │
│ • DebitAmount: 0.00             │
│ • CreditAmount: 5000.00         │
└────────────┬────────────────────┘
             │
             ↓
STEP 4: VERIFY & POST
┌─────────────────────────────────┐
│ Debit 5000 = Credit 5000 ✓      │
│ Accounts valid ✓                │
│ Status: Draft → Posted          │
└────────────┬────────────────────┘
             │
             ↓
STEP 5: UPDATE GENERAL LEDGER
┌─────────────────────────────────┐
│ GeneralLedger Updates:          │
│                                 │
│ Account 1010 (Bank):            │
│ Balance: 1,000,000              │
│ + 5,000 deposit                 │
│ = 1,005,000 (NEW BALANCE)       │
│                                 │
│ Account 2100 (Deposit Liab):    │
│ Balance: 500,000                │
│ + 5,000 liability               │
│ = 505,000 (NEW BALANCE)         │
└────────────┬────────────────────┘
             │
             ↓
STEP 6: CREATE GL TRANSACTION RECORD
┌─────────────────────────────────┐
│ GeneralLedgerTransactions:      │
│ • Account: 1010                 │
│ • Debit: 5000.00                │
│ • Credit: 0.00                  │
│ • RunningBalance: 1,005,000     │
│                                 │
│ • Account: 2100                 │
│ • Debit: 0.00                   │
│ • Credit: 5000.00               │
│ • RunningBalance: 505,000       │
└────────────┬────────────────────┘
             │
             ↓
STEP 7: GENERATE TRIAL BALANCE
┌─────────────────────────────────┐
│ TrialBalance Record:            │
│                                 │
│ Account 1010:                   │
│ DebitBalance: 1,005,000         │
│ CreditBalance: 0.00             │
│                                 │
│ Account 2100:                   │
│ DebitBalance: 0.00              │
│ CreditBalance: 505,000          │
│                                 │
│ Totals:                         │
│ Total Debits: 1,809,900         │
│ Total Credits: 1,809,900 ✓      │
│ Status: BALANCED                │
└────────────┬────────────────────┘
             │
             ↓
STEP 8: GENERATE FINANCIAL STATEMENTS
┌─────────────────────────────────┐
│ Balance Sheet:                  │
│                                 │
│ ASSETS:                         │
│ Bank Account 1010:              │
│   From GL: 1,005,000            │
│   Display in Statement          │
│                                 │
│ LIABILITIES:                    │
│ Cust Deposits Liability:        │
│   From GL: 505,000              │
│   Display in Statement          │
│                                 │
│ FinancialStatements:            │
│ • StatementType: BalanceSheet   │
│ • StatementData: [Full data]    │
│ • GeneratedAt: [Today]          │
│ • GeneratedBy: [Accountant]     │
└─────────────────────────────────┘
```

---

## 🗄️ CRITICAL DATABASE TABLES

```
┌────────────────────────────────────────────────────────────────┐
│                 ACCOUNTING DATABASE SCHEMA                    │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────────────┐  ┌──────────────────────┐           │
│  │   JournalEntries     │  │ JournalEntryLines    │           │
│  │  (Header)            │  │  (Detail)            │           │
│  ├──────────────────────┤  ├──────────────────────┤           │
│  │ • JournalId (PK)     │  │ • LineId (PK)        │           │
│  │ • JournalNumber      │  │ • JournalId (FK) ──→ │           │
│  │ • TransactionDate    │  │ • AccountCode        │           │
│  │ • Description        │  │ • DebitAmount        │           │
│  │ • TotalDebit         │  │ • CreditAmount       │           │
│  │ • TotalCredit        │  └──────────────────────┘           │
│  │ • Status             │           │                         │
│  │ • CreatedAt/By       │           │                         │
│  │ • PostedAt/By        │           ↓                         │
│  └──────────────────────┘  ┌──────────────────────┐           │
│           │                │GeneralLedgerTxns     │           │
│           │                │  (Detail)            │           │
│           │                ├──────────────────────┤           │
│           │                │ • GLTransactionId(PK)│           │
│           │                │ • JournalLineId(FK) ─┘           │
│           └───────────────→│ • AccountCode        │           │
│                            │ • DebitAmount        │           │
│                            │ • CreditAmount       │           │
│                            │ • RunningBalance     │           │
│                            └──────────┬───────────┘           │
│                                       │                        │
│                                       ↓                        │
│                            ┌──────────────────────┐           │
│                            │  GeneralLedger       │           │
│                            │  (Master)            │           │
│                            ├──────────────────────┤           │
│                            │ • LedgerId (PK)      │           │
│                            │ • AccountCode        │           │
│                            │ • AccountName        │           │
│                            │ • Balance (CURRENT)  │           │
│                            │ • IsActive           │           │
│                            └──────────┬───────────┘           │
│                                       │                        │
│                            ┌──────────┴───────────┐           │
│                            ↓                      ↓            │
│                 ┌──────────────────┐  ┌──────────────────┐   │
│                 │  TrialBalance    │  │ Financial        │   │
│                 │  (Validation)    │  │ Statements       │   │
│                 ├──────────────────┤  │ (Reporting)      │   │
│                 │ • TB Id          │  ├──────────────────┤   │
│                 │ • PeriodEnd      │  │ • StatementId    │   │
│                 │ • AccountCode    │  │ • StatementType  │   │
│                 │ • DebitBalance   │  │ • StatementData  │   │
│                 │ • CreditBalance  │  │ • GeneratedAt    │   │
│                 └──────────────────┘  └──────────────────┘   │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## 📋 ACCOUNTANT DAILY WORKFLOW

```
8:00 AM - START DAY
  ┌─────────────────────┐
  │ 1. Log in           │
  │ 2. Check messages   │
  │ 3. Review overnight │
  │    banking activity │
  └────────────┬────────┘
               │
               ↓
11:00 AM - CREATE ENTRIES
  ┌─────────────────────┐
  │ 1. Review all daily │
  │    transactions     │
  │ 2. Create journal   │
  │    entries          │
  │ 3. Verify balance   │
  │    (DR = CR)        │
  │ 4. Post to GL       │
  └────────────┬────────┘
               │
               ↓
2:00 PM - RECONCILIATION
  ┌─────────────────────┐
  │ 1. Review GL        │
  │    accounts         │
  │ 2. Reconcile bank   │
  │    accounts         │
  │ 3. Check for       │
  │    errors           │
  └────────────┬────────┘
               │
               ↓
4:00 PM - END-OF-DAY CLOSE
  ┌─────────────────────┐
  │ 1. Generate daily   │
  │    trial balance    │
  │ 2. Verify balance   │
  │ 3. Document any     │
  │    findings         │
  │ 4. Report to mgmt   │
  │ 5. Save & lock      │
  │    transactions     │
  └─────────────────────┘

END OF DAY
```

---

## 📈 MONTH-END CLOSE PROCESS (5 Days)

```
Day 1: ADJUSTMENTS
┌─────────────────────────────────┐
│ • Post interest accruals        │
│ • Post depreciation             │
│ • Record loan loss provisions   │
│ • Accrue outstanding expenses   │
└────────────┬────────────────────┘
             │
             ↓
Day 2: VALIDATION
┌─────────────────────────────────┐
│ • Reconcile all accounts        │
│ • Generate trial balance        │
│ • Investigate any imbalances    │
│ • Verify source data matches    │
└────────────┬────────────────────┘
             │
             ↓
Day 3: STATEMENTS
┌─────────────────────────────────┐
│ • Prepare balance sheet         │
│ • Prepare income statement      │
│ • Prepare cash flow statement   │
│ • Add notes & disclosures       │
└────────────┬────────────────────┘
             │
             ↓
Day 4: REVIEW
┌─────────────────────────────────┐
│ • Manager review                │
│ • Get necessary approvals       │
│ • Make adjustments if needed    │
│ • Final sign-off                │
└────────────┬────────────────────┘
             │
             ↓
Day 5: CLOSE & DISTRIBUTE
┌─────────────────────────────────┐
│ • Distribute statements         │
│ • Lock the period               │
│ • Complete backups              │
│ • Prepare for next period       │
└─────────────────────────────────┘

MONTH OFFICIALLY CLOSED & COMPLETE
```

---

## 🔐 PERMISSION MATRIX AT A GLANCE

```
                    ADMIN          ACCOUNTANT
════════════════════════════════════════════════════
Journal Entries     Full           Full
General Ledger      Full           Read-Only
Trial Balance       Full           Full
Fin. Statements     Full           Full
Acctg Reports       Full           Full
Chart of Accounts   Full           Read-Only
Bank Accounts       Full           Read-Only
Fund Transfers      Full           Read-Only
Loans               Full           Read-Only
Cards               Full           Read-Only
Audit Logs          Full           Own Only
Users               Full           None
Roles               Full           None
Settings            Full           None
System Access       Full           None
════════════════════════════════════════════════════
TOTAL:              100%            60%
```

---

## 🎯 YOUR RESPONSIBILITIES

```
┌──────────────────────────────────────────────────────────────┐
│         AS AN ACCOUNTANT, YOU ARE RESPONSIBLE FOR:          │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ✅ RECORDING TRANSACTIONS                                  │
│     • Receive daily banking activity                        │
│     • Create accurate journal entries                       │
│     • Verify all entries balance                            │
│                                                              │
│  ✅ POSTING TO GENERAL LEDGER                               │
│     • Post entries to GL for account updates                │
│     • Maintain current GL balances                          │
│     • Create audit trail                                    │
│                                                              │
│  ✅ VALIDATING ACCURACY                                     │
│     • Generate trial balance                               │
│     • Verify debits = credits                               │
│     • Investigate imbalances                                │
│                                                              │
│  ✅ PREPARING REPORTS                                       │
│     • Generate financial statements                        │
│     • Prepare accounting reports                            │
│     • Provide analysis to management                        │
│                                                              │
│  ✅ COORDINATING WITH OTHERS                                │
│     • Work with operations team                             │
│     • Communicate with loan officers                        │
│     • Coordinate with card services                         │
│     • Support audit processes                               │
│                                                              │
│  ✅ CLOSING PERIODS                                         │
│     • Month-end reconciliation                              │
│     • Period closing procedures                             │
│     • Maintain data integrity                               │
│                                                              │
│  ✅ MAINTAINING COMPLIANCE                                  │
│     • Follow accounting standards                           │
│     • Maintain audit trail                                  │
│     • Support regulatory requirements                       │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 💡 KEY SUCCESS FACTORS

```
┌────────────────────────────────────────────────────┐
│        SUCCEED BY REMEMBERING:                     │
├────────────────────────────────────────────────────┤
│                                                    │
│ 1. ACCURACY IS EVERYTHING                         │
│    • Every penny must be accounted for            │
│    • Errors compound over time                    │
│    • Check twice, post once                       │
│                                                    │
│ 2. TIMING MATTERS                                 │
│    • Post daily, don't let backlog build          │
│    • Month-end close has strict deadline          │
│    • Management relies on timely reporting        │
│                                                    │
│ 3. DOCUMENTATION IS YOUR FRIEND                   │
│    • Always reference supporting documents       │
│    • Record your reasoning                       │
│    • Support audit trail                          │
│                                                    │
│ 4. ASK WHEN UNSURE                                │
│    • Better to ask than make mistake              │
│    • Your manager is there to help                │
│    • Clarify ambiguities early                    │
│                                                    │
│ 5. RECONCILE REGULARLY                            │
│    • Don't wait for big imbalances                │
│    • Daily reconciliation prevents problems       │
│    • Early detection = easier fix                 │
│                                                    │
│ 6. STAY CURRENT                                   │
│    • Keep documentation updated                   │
│    • Regular training and learning                │
│    • Know your system thoroughly                  │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

## 🎓 LEARNING PATH

```
WEEK 1: UNDERSTANDING
  ├─ Read Accountant Quick Start Guide (1 hour)
  ├─ Read Workflow Complete document (4 hours)
  ├─ Observe experienced accountant (4 hours)
  └─ Complete system tutorial

WEEK 2: HANDS-ON
  ├─ Create first journal entry (supervised)
  ├─ Post to GL (supervised)
  ├─ Generate trial balance (supervised)
  ├─ Run accounting reports (supervised)
  └─ Create sample financial statement

WEEK 3: INDEPENDENT
  ├─ Create journal entries independently
  ├─ Post entries independently
  ├─ Review GL transactions
  ├─ Reconcile sample accounts
  └─ Generate various reports

WEEK 4: PROFICIENCY
  ├─ Handle full daily workload
  ├─ Manage all month-end tasks
  ├─ Resolve issues independently
  └─ Train other new staff members

BY END OF MONTH: READY FOR PRODUCTION WORK
```

---

## 📞 SUPPORT MATRIX

```
Question Type              Contact          Response Time
──────────────────────────────────────────────────────
How do I...?               Your Manager     15 min
System problem             IT Support       1 hour
Accounting question        Finance Mgr      30 min
Access/Permission issue    Admin            2 hours
Compliance question        CFO              1 day
Urgent escalation          Finance Manager  Immediate
```

---

## ✅ SIGN-OFF CHECKLIST

```
Before you start working as an Accountant, verify:

□ You have system login (Accountant role)
□ You can access all accounting modules
□ You cannot access admin functions (expected)
□ You understand the 9-step workflow
□ You know your daily responsibilities
□ You know the month-end close process
□ You know who to contact for issues
□ You have read the documentation
□ You understand the permission structure
□ You are ready to begin production work

READY TO START? ✅ YES
```

---

**Created:** November 22, 2025  
**Status:** ✅ COMPLETE  
**Version:** 1.0

