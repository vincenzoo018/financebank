# ACCOUNTANT ROLE WORKFLOW - COMPLETE SYSTEM GUIDE

**Finance & Accounting Transaction System**  
**FinanceBank - FINSYS Accountant**

---

## TABLE OF CONTENTS

1. [Executive Overview](#executive-overview)
2. [Accountant Workflow - Step by Step](#accountant-workflow---step-by-step)
3. [Module Interactions](#module-interactions)
4. [Database Mapping](#database-mapping)
5. [Role-Based Access Control](#role-based-access-control)
6. [Daily Operations Checklist](#daily-operations-checklist)
7. [Period-End Close Process](#period-end-close-process)
8. [Coordination & Escalation](#coordination--escalation)

---

## EXECUTIVE OVERVIEW

### What is an Accountant?

The **Accountant** is a financial professional who:
- Records all financial transactions using Journal Entries
- Maintains the General Ledger (the "books")
- Validates financial accuracy via Trial Balance
- Prepares Financial Statements (Balance Sheet, Income Statement, Cash Flow)
- Generates and reviews Accounting Reports
- Ensures compliance with accounting standards
- Coordinates with Banking Operations to verify all transactions

### Key Responsibility Areas

| Area | Responsibility | Focus |
|------|-----------------|-------|
| **Recording** | Post Journal Entries | Accuracy, completeness, proper account coding |
| **Maintenance** | Manage General Ledger | All account balances are current and correct |
| **Validation** | Review Trial Balance | Debits = Credits, no posting errors |
| **Reporting** | Create Financial Statements | Provide clear financial position to management |
| **Analysis** | Generate Reports | Identify trends, variances, compliance issues |
| **Coordination** | Work with Operations | Resolve transaction mismatches and discrepancies |

---

## ACCOUNTANT WORKFLOW - STEP BY STEP

### ✅ FLOW 1: Banking Operations Generate Transactions

**Source:** Banking Operations  
**Output:** Financial Events

```
Banking Operations Activities
        ↓
    ┌─────────────────────────────────────────┐
    │ • Customer Deposits & Withdrawals       │
    │ • Fund Transfers (Account to Account)   │
    │ • Loan Disbursements                    │
    │ • Loan Repayments                       │
    │ • Card Transactions & Payments          │
    │ • Service Charges & Penalties           │
    │ • Interest Accruals                     │
    └─────────────────────────────────────────┘
        ↓
    These activities generate financial events
    that must be recorded in the accounting system
```

**Accountant's Role:** Awaits notification and collects all financial movements.

**Related Modules:**
- Bank Accounts (account balances)
- Funds Transfer (inter-account movements)
- Loan Management (loan activities)
- Card Management (card transactions)
- Banking Reports (transaction summaries)

---

### ✅ FLOW 2: Accountant Receives Daily Financial Activity

**Action:** Review All Financial Movements  
**Timeline:** Daily, end-of-day or start of next business day

**Accountant Reviews:**

1. **Bank Accounts Module**
   - Deposits received
   - Withdrawals processed
   - Current account balances
   - Bank reconciliation differences

2. **Funds Transfer Module**
   - Internal account transfers
   - External wire transfers
   - Transfer fees charged
   - Pending vs. completed transfers

3. **Loan Management Module**
   - New loan disbursements
   - Loan repayments received
   - Interest accruals
   - Late payments or defaults

4. **Card Management Module**
   - Card transactions posted
   - Chargebacks received
   - Payment reversals
   - Card fees charged

5. **Banking Reports Module**
   - Daily transaction summaries
   - Cash position reports
   - Activity by account type

**Goal:** Ensure EVERY financial activity is captured and ready for posting.

**Verification Checklist:**
- [ ] All transactions documented
- [ ] Amounts match banking records
- [ ] No missing or duplicate entries
- [ ] All source documents present
- [ ] Authorization codes verified

---

### ✅ FLOW 3: Accountant Posts/Reviews Journal Entries

**Action:** Record Transactions as Journal Entries  
**Frequency:** Daily or weekly, depending on transaction volume

**What Gets Posted:**

#### A. Banking Transactions (Automatic or Manual)
```
Example: Customer Deposit of $5,000

Debit:  Bank Account                    $5,000
Credit: Customer Deposit Liability                $5,000

Effect: Bank balance increases; customer owes the bank nothing
        (it's a liability because bank holds customer funds)
```

#### B. Manual Adjustments

```
Example: Depreciation for Equipment

Debit:  Depreciation Expense           $500
Credit: Accumulated Depreciation              $500

Effect: Records equipment wear and tear; reduces asset value
```

#### C. Accruals

```
Example: Interest Earned on Customer Deposits

Debit:  Interest Receivable            $200
Credit: Interest Income                        $200

Effect: Records earned interest before actual payment
```

#### D. End-of-Day Balancing

```
Example: Reconciliation Entry

Debit:  Cash Variance                  $50
Credit: Correction Account                    $50

Effect: Adjusts for rounding or minor discrepancies
```

#### E. Corrections

```
Example: Reversal of Incorrect Entry

Debit:  Accounts Payable               $1,000
Credit: Cash                                   $1,000

Effect: Reverses a posting error from prior day
```

**Journal Entry Components:**

| Field | Purpose | Example |
|-------|---------|---------|
| Journal Number | Unique identifier | JNL-20251122-001 |
| Transaction Date | When event occurred | 2025-11-22 |
| Description | What happened | "Daily customer deposits reconciliation" |
| Reference | Supporting doc | "Bank deposit slip #4521" |
| Account Code | GL account | 1010 (Bank Account) |
| Debit Amount | Increases assets/expenses | 5000.00 |
| Credit Amount | Decreases assets/increases liabilities/income | 5000.00 |
| Status | Draft → Posted | Draft (waiting review) |

**Accountant's Validation:**
- ✓ Debits = Credits (journal entry balances)
- ✓ Correct accounts used (per Chart of Accounts)
- ✓ Amounts match supporting documents
- ✓ Descriptions are clear and complete
- ✓ All required fields populated

**Output:** Journal Entries stored with Status = "Posted"

---

### ✅ FLOW 4: Entries Flow into the General Ledger

**Automatic Process:** Posted journal entries update the GL

**What Happens:**

```
Posted Journal Entry (JE-001)
        ↓
    Entry Lines Created:
    - Account 1010 (Bank Account) - Debit $5,000
    - Account 2100 (Customer Deposits Liability) - Credit $5,000
        ↓
    General Ledger Updated:
    - GL Account 1010: Balance increases to $1,005,000
    - GL Account 2100: Balance increases to $500,000
        ↓
    General Ledger Transaction Record Created:
    - Running balances calculated
    - Audit trail established
```

**General Ledger Functions:**

1. **Maintains Account Balances**
   - Every account has a current balance
   - Updated in real-time as entries are posted

2. **Records Account Type**
   - Assets (increase with debits)
   - Liabilities (increase with credits)
   - Income (increase with credits)
   - Expenses (increase with debits)
   - Equity (increases with credits)

3. **Creates Audit Trail**
   - Running balance shows progression
   - Transaction sequence preserved
   - Easy to trace corrections and adjustments

4. **Enables Reporting**
   - Financial statements pull from GL
   - Trial Balance compiled from GL
   - Reports based on GL transactions

**General Ledger Structure:**

```
Account 1010 - Bank Account
─────────────────────────────────
Date        Description              Debit       Credit      Balance
2025-11-21  Opening Balance                                  1,000,000
2025-11-22  Customer Deposit         5,000                   1,005,000
2025-11-22  Interest Posted                    100          1,004,900
─────────────────────────────────────────────────────────────────────

Account 2100 - Customer Deposits Liability
─────────────────────────────────
Date        Description              Debit       Credit      Balance
2025-11-21  Opening Balance                                  500,000
2025-11-22  New Deposit                        5,000         505,000
```

**Output:** General Ledger automatically reflects all posted activity

---

### ✅ FLOW 5: Accountant Reviews Trial Balance

**Action:** Validate GL Integrity  
**Frequency:** Daily or weekly before closing  
**Duration:** 1-2 hours typical

**What Trial Balance Shows:**

```
TRIAL BALANCE - As of November 22, 2025
════════════════════════════════════════════════════════════════

Account Code    Account Name                   Debit           Credit
─────────────────────────────────────────────────────────────────
1010            Bank Accounts              1,004,900
1020            Investment Accounts          200,000
1030            Loan Receivables             450,000
1040            Cash in Tills                 50,000
2100            Customer Deposits                        500,000
2200            Accounts Payable                         125,000
3100            Capital Stock                           800,000
4100            Interest Income                          75,000
5100            Operating Expenses           100,000

                TOTALS                     1,804,900       1,500,000
```

**Wait... Totals Don't Match!**

**What Accountant Investigates:**

1. **Check for Posting Errors**
   - Did someone post to the wrong account?
   - Were debits and credits reversed?

2. **Verify Account Balances**
   - Are all GL accounts properly updated?
   - Any pending transactions not posted?

3. **Review Recent Entries**
   - Last few days' journal entries
   - Any corrections or reversals pending?

4. **Check for Unbalanced Entries**
   - Are all journal entries still in balance?
   - Any partial postings?

5. **Investigate Reconciliation Issues**
   - Bank reconciliation differences?
   - Customer account discrepancies?

**Finding the Issue:**

```
Accountant discovers:
- Journal Entry JE-045 was created but not fully posted
- Account 1010 shows debit of 5,000 but account 2100 
  has no corresponding credit
  
Solution:
- Complete posting of JE-045
- OR create correction entry
- Rerun Trial Balance
```

**When Trial Balance DOES Balance:**

```
Corrected Trial Balance:
════════════════════════════════════════════════════════════════

Account Code    Account Name                   Debit           Credit
─────────────────────────────────────────────────────────────────
1010            Bank Accounts              1,009,900
1020            Investment Accounts          200,000
1030            Loan Receivables             450,000
1040            Cash in Tills                 50,000
2100            Customer Deposits                        505,000
2200            Accounts Payable                         125,000
3100            Capital Stock                           800,000
4100            Interest Income                          75,000
5100            Operating Expenses           100,000

                TOTALS                     1,809,900       1,809,900  ✓
```

**Validation Checklist:**
- [ ] Total Debits = Total Credits
- [ ] All accounts have correct signs
- [ ] No accounts missing from GL
- [ ] All posted entries reflected
- [ ] Account balances reasonable
- [ ] No arithmetic errors

**Output:** Approved Trial Balance (ready for financial statements)

---

### ✅ FLOW 6: Accountant Generates Financial Statements

**Action:** Prepare Official Financial Reports  
**Frequency:** Monthly/Quarterly/Annually  
**Based On:** Approved Trial Balance

**Three Main Statements:**

#### A. BALANCE SHEET (Statement of Financial Position)

Shows what the bank OWNS and OWES at a point in time.

```
FINSYS BANK - BALANCE SHEET
As of November 22, 2025
═════════════════════════════════════════════════════

ASSETS (What the Bank Owns)
──────────────────────────────
Current Assets:
  Bank Accounts                          $1,009,900
  Cash in Tills                              50,000
  Customer Deposits (receivable)            15,000
  Investment Accounts                      200,000
    Subtotal Current Assets                        $1,274,900

Non-Current Assets:
  Loan Receivables (long-term)             450,000
  Equipment (net of depreciation)          250,000
    Subtotal Non-Current Assets                      $700,000

TOTAL ASSETS                                      $1,974,900
════════════════════════════════════════════════════

LIABILITIES (What the Bank Owes)
──────────────────────────────────
Current Liabilities:
  Customer Deposits Liability              $505,000
  Accounts Payable                         125,000
  Accrued Expenses                          50,000
    Subtotal Current Liabilities                     $680,000

TOTAL LIABILITIES                                    $680,000
════════════════════════════════════════════════════

EQUITY (What's Left for Owners)
──────────────────────────────────
Capital Stock                              $800,000
Retained Earnings                          494,900
  TOTAL EQUITY                                    $1,294,900

TOTAL LIABILITIES + EQUITY                      $1,974,900
════════════════════════════════════════════════════

Balance Check: Assets = Liabilities + Equity ✓
```

**Purpose:** Shows financial position; used for performance evaluation

#### B. INCOME STATEMENT (Profit & Loss)

Shows how much money the bank made or lost during a period.

```
FINSYS BANK - INCOME STATEMENT
For the Month Ended November 22, 2025
═════════════════════════════════════════════════════

REVENUES (Money Coming In)
──────────────────────────────
Interest Income                           $75,000
  (From customer loans & deposits)
Service Fees                              15,000
  (Monthly account fees, transfer fees)
Card Processing Fees                       8,000
  (From card transactions)
Late Payment Penalties                     2,000

TOTAL REVENUES                                    $100,000
═════════════════════════════════════════════════════

EXPENSES (Money Going Out)
──────────────────────────────
Employee Salaries                        $35,000
Depreciation - Equipment                   5,000
Utilities & Maintenance                    3,000
Loan Loss Provisions                       2,000
Marketing & Advertising                    1,500
Other Operating Expenses                   3,500

TOTAL EXPENSES                                    $50,000
═════════════════════════════════════════════════════

NET INCOME (Profit)                             $50,000
═════════════════════════════════════════════════════

Interpretation: Bank earned $50,000 this month
```

**Purpose:** Shows profitability; used for performance analysis

#### C. CASH FLOW STATEMENT

Shows where cash came from and where it went.

```
FINSYS BANK - CASH FLOW STATEMENT
For the Month Ended November 22, 2025
═════════════════════════════════════════════════════

OPERATING ACTIVITIES (Core Business)
──────────────────────────────────
Net Income                                $50,000
Adjustments:
  Depreciation (non-cash)                  5,000
  Increase in Accounts Payable            10,000

Cash from Operations                             $65,000

INVESTING ACTIVITIES (Buying/Selling Assets)
──────────────────────────────────
Purchase of Equipment                   ($20,000)
Sale of Investments                       25,000

Cash from Investing                             $5,000

FINANCING ACTIVITIES (Raising/Repaying Money)
──────────────────────────────────
Dividend Payments                        ($10,000)

Cash from Financing                           ($10,000)

NET CHANGE IN CASH                             $60,000

Cash Beginning of Month                    40,000
Cash End of Month                          100,000

═════════════════════════════════════════════════════
```

**Purpose:** Shows cash movements; important for liquidity analysis

**Accountant's Role in Statement Prep:**

1. ✓ Compile account balances from GL
2. ✓ Apply accounting adjustments
3. ✓ Classify accounts to statement lines
4. ✓ Calculate subtotals and totals
5. ✓ Review for accuracy and reasonableness
6. ✓ Add footnotes and disclosures
7. ✓ Get management review/approval
8. ✓ Distribute to stakeholders

**Output:** Three formal financial statements ready for distribution

---

### ✅ FLOW 7: Accountant Reviews and Produces Reports

**Action:** Generate Specialized Reports  
**Frequency:** Daily, Weekly, Monthly  
**Users:** Management, Auditors, Regulatory Bodies

**Types of Reports:**

#### A. ACCOUNTING REPORTS (Internal)

```
1. Journal Entry Register
   └─ Lists all journal entries posted in period
   └─ Shows account impacts and balances
   └─ Helps verify posting completeness

2. General Ledger Detail Report
   └─ Complete GL with transaction details
   └─ Shows every debit and credit to each account
   └─ Audit trail for compliance

3. Account Reconciliations
   └─ Bank account vs. GL
   └─ Customer deposits vs. GL
   └─ Loan receivables vs. contracts

4. Adjusting Entries Report
   └─ All non-routine entries made
   └─ Depreciation, accruals, corrections
   └─ Justification for each entry

5. Account Analysis Reports
   └─ Unusual transactions by account
   └─ High-value or suspicious activity
   └─ Month-over-month comparisons
```

#### B. BANKING REPORTS (Operational)

```
1. Daily Cash Position Report
   └─ Current bank balances by account
   └─ Available liquidity status
   └─ Short-term cash needs

2. Fund Transfer Report
   └─ All transfers in period
   └─ Pending vs. completed
   └─ Transfer fees charged

3. Loan Activity Report
   └─ New loans vs. repayments
   └─ Interest accrued
   └─ Overdue amounts

4. Card Transaction Report
   └─ Total card spending
   └─ By merchant category
   └─ Chargebacks or disputes

5. Customer Account Activity
   └─ Account openings/closings
   └─ Transaction frequency
   └─ Balance trends
```

#### C. COMPLIANCE REPORTS (Regulatory)

```
1. Trial Balance Report
   └─ All accounts with balances
   └─ Required for financial reporting
   └─ Audit support

2. Regulatory Reports
   └─ Capital adequacy ratios
   └─ Liquidity coverage ratios
   └─ Non-performing loans

3. Audit Logs Report
   └─ Who did what and when
   └─ Changes to transaction records
   └─ System access history
```

#### D. MANAGEMENT REPORTS (Decision Support)

```
1. Budget vs. Actual Report
   └─ Expense categories vs. budget
   └─ Variances and explanations
   └─ Cost control analysis

2. Financial Ratio Analysis
   └─ Profitability (ROA, ROE)
   └─ Liquidity (Current Ratio, Quick Ratio)
   └─ Efficiency (Asset Turnover)

3. Trend Analysis
   └─ Revenue trends
   └─ Expense trends
   └─ Profitability trends

4. Performance Dashboard
   └─ Key metrics at a glance
   └─ Variances from plan
   └─ Early warning indicators
```

**Verification Checklist Before Release:**
- [ ] Data completeness verified
- [ ] Calculations double-checked
- [ ] Balances reconciled to GL
- [ ] Comparisons to prior period accurate
- [ ] Formatting clear and professional
- [ ] Notes and explanations complete
- [ ] Management approval obtained
- [ ] Distribution list confirmed

**Output:** Reports distributed to stakeholders

---

### ✅ FLOW 8: Accountant Coordinates With Other Banking Units

**Action:** Communicate and Resolve Issues  
**Frequency:** Ongoing, as needed  
**Purpose:** Ensure accuracy and completeness

**Coordination Points:**

#### A. WITH OPERATIONS TEAM

**Issue:** Transaction Mismatches

```
Scenario: Bank shows $5,000 deposit, GL shows $3,000

Accountant Actions:
1. Reviews original deposit documentation
2. Checks journal entry for the transaction
3. Contacts Operations to verify amount
4. Determines if:
   - Entry amount was wrong (correction needed)
   - Deposit was split across two entries (combine in report)
   - Timing difference (not yet recorded)
5. Creates adjustment entry if needed
```

**Issue:** Missing Transactions

```
Scenario: Operations reports fund transfer but no GL entry

Accountant Actions:
1. Verifies transfer was authorized and completed
2. Determines why entry wasn't posted:
   - System glitch?
   - Manual entry overlooked?
   - Pending processing?
3. Creates journal entry
4. Updates Operations on posting status
```

#### B. WITH LOAN OFFICERS

**Issue:** Interest and Amortization Posting

```
Scenario: Customer loan due for monthly interest

Accountant Actions:
1. Reviews loan terms with Loan Officer
2. Calculates correct interest amount
3. Creates journal entry:
   Debit: Interest Receivable (Asset)
   Credit: Interest Income (Revenue)
4. Posts entry to GL
5. Confirms with Loan Officer that amount matches contract
```

**Issue:** Loan Defaults or Write-offs

```
Scenario: Loan 90+ days overdue

Accountant Actions:
1. Reviews loan status with Loan Officer
2. Determines if loan should be written off
3. Creates adjustment entry:
   Debit: Loan Loss Provision
   Credit: Loan Receivable
4. Updates financial statements
5. Reports to management
```

#### C. WITH CARD SERVICES

**Issue:** Chargebacks and Reversals

```
Scenario: Customer disputes card charge

Accountant Actions:
1. Receives chargeback notification from Card Services
2. Verifies original transaction in GL
3. Creates reversal entry:
   Debit: Card Chargeback Liability
   Credit: Card Transaction Revenue
4. Updates customer account
5. Documents for reconciliation
```

**Issue:** Card Fees and Interest

```
Scenario: Monthly card management fees and interest

Accountant Actions:
1. Receives fee schedule from Card Services
2. Calculates total fees and interest by customer
3. Creates batch journal entry:
   Debit: Accounts Receivable (Card Fees)
   Credit: Card Fee Income
4. Posts to GL
5. Confirms amounts with Card Services
```

#### D. WITH MANAGEMENT

**Issue:** Financial Performance Review

```
Scenario: Monthly management meeting

Accountant Provides:
1. Financial statements
2. Key metrics and ratios
3. Variance analysis vs. budget
4. Explanations for significant variances
5. Recommendations for cost control
6. Forward projections for cash flow
```

**Issue:** Executive Decision Support

```
Scenario: Should we grant a large line of credit?

Accountant Actions:
1. Reviews customer financial statements
2. Calculates credit metrics
3. Analyzes repayment capacity
4. Presents risk assessment to management
5. Provides accounting entry if approved
```

#### E. WITH AUDITORS

**Issue:** External Audit Preparation

```
Scenario: Year-end external audit

Accountant Provides:
1. General Ledger with all transactions
2. Trial Balance report
3. Journal entry summaries and support
4. Bank reconciliations
5. Account confirmations
6. Adjusting entries documentation
7. Schedule of significant transactions
```

**Issue:** Audit Findings and Corrections

```
Scenario: Auditor finds transaction error

Accountant Actions:
1. Reviews auditor's finding
2. Determines if adjustment is required
3. Creates correcting entry (if needed)
4. Documents in audit trail
5. Confirms correction with auditor
```

**Communication Log Template:**

| Date | Unit | Issue | Resolution | Entry Created | Status |
|------|------|-------|-----------|---------------|--------|
| 11/22 | Operations | Missing JE | Verified & posted | JE-045 | Resolved |
| 11/22 | Loan Mgmt | Interest calc | Monthly accrual created | JE-046 | Resolved |
| 11/23 | Card Services | Chargeback | Reversal entry posted | JE-047 | Resolved |

---

### ✅ FLOW 9: Period-End Closing

**Action:** Finalize Financial Records  
**Frequency:** Monthly/Quarterly/Annually  
**Duration:** 2-5 days typical

**Pre-Close Checklist:**

```
□ All daily transactions posted
□ Bank reconciliation completed
□ Customer accounts reconciled
□ Loan receivables confirmed
□ Trial Balance balanced
□ All adjusting entries reviewed
□ Supporting documentation complete
□ Management approval pending
□ Audit log reviewed
```

**Closing Procedures:**

#### Phase 1: Adjustments and Accruals (Day 1)

```
1. Interest Accruals
   JE-Adj-001: Interest earned but not yet received
   Debit: Interest Receivable
   Credit: Interest Income

2. Depreciation
   JE-Adj-002: Monthly equipment depreciation
   Debit: Depreciation Expense
   Credit: Accumulated Depreciation

3. Loan Loss Provisions
   JE-Adj-003: Provision for questionable loans
   Debit: Loan Loss Provision
   Credit: Allowance for Loan Losses

4. Accrued Expenses
   JE-Adj-004: Expenses incurred but not yet paid
   Debit: Utilities Expense
   Credit: Utilities Payable

5. Accrued Liabilities
   JE-Adj-005: Bonuses earned not yet paid
   Debit: Bonus Expense
   Credit: Bonus Payable

6. Inventory Adjustments
   JE-Adj-006: Supplies used during period
   Debit: Supplies Expense
   Credit: Supplies Inventory

7. Other Adjustments
   JE-Adj-007-NNN: Various year-end adjustments
```

#### Phase 2: Review and Approve (Day 2)

```
1. Recalculate Trial Balance
   └─ Must balance after all adjustments
   └─ Verify all adjusting entries posted

2. Review for Unusual Items
   └─ Large transactions
   └─ One-time adjustments
   └─ Reversals or corrections

3. Get Management Sign-off
   └─ Manager reviews adjustments
   └─ Approves period close
   └─ Authorizes financial statements
```

#### Phase 3: Financial Statements (Day 3)

```
1. Prepare Financial Statements
   └─ Balance Sheet
   └─ Income Statement
   └─ Cash Flow Statement
   └─ Statement of Changes in Equity

2. Add Notes and Disclosures
   └─ Accounting policies
   └─ Significant transactions
   └─ Contingencies or risks
   └─ Management discussion
```

#### Phase 4: Archival and Lock (Day 4-5)

```
1. Complete Audit Trail
   └─ All entries documented
   └─ Supporting docs filed
   └─ Approvals recorded

2. Lock Period
   └─ Mark month as "closed"
   └─ Restrict new entries to this period
   └─ Prevent unauthorized changes

3. Distribution
   └─ Financial statements to management
   └─ Reports to regulatory bodies
   └─ Archive backup copies

4. Next Period Prep
   └─ Reopen GL for new entries
   └─ Carry forward balances
   └─ Setup new budget/forecast
```

**Post-Close Verification:**

```
Accountant confirms:
✓ All accounts have current balances
✓ No pending transactions remain
✓ Period properly closed and locked
✓ Financial statements released to authorized users
✓ GL ready for next period
✓ Archive backup completed
```

---

## MODULE INTERACTIONS

### Visual: Complete Accounting Workflow Map

```
BANKING OPERATIONS
(Generate Transactions)
      ↓ Daily
┌─────────────────────────────────────────────────┐
│ • Bank Accounts Module                          │
│ • Funds Transfer Module                         │
│ • Loan Management Module                        │
│ • Card Management Module                        │
│ • Banking Reports Module                        │
└─────────────────────────────────────────────────┘
      ↓ Accountant Reviews
┌─────────────────────────────────────────────────┐
│ JOURNAL ENTRIES MODULE                          │
│ └─ Record all activities as JEs                │
│ └─ Verify debits = credits                     │
│ └─ Status: Draft → Posted                      │
└─────────────────────────────────────────────────┘
      ↓ Auto Post
┌─────────────────────────────────────────────────┐
│ GENERAL LEDGER MODULE                           │
│ └─ All accounts updated                        │
│ └─ Running balances calculated                 │
│ └─ GL transactions recorded                    │
└─────────────────────────────────────────────────┘
      ↓ Accountant Reviews
┌─────────────────────────────────────────────────┐
│ TRIAL BALANCE MODULE                            │
│ └─ Verify Debits = Credits                     │
│ └─ Check for errors                            │
│ └─ Investigate reconciliations                 │
└─────────────────────────────────────────────────┘
      ↓ If Balanced
┌─────────────────────────────────────────────────┐
│ FINANCIAL STATEMENTS MODULE                     │
│ └─ Balance Sheet                               │
│ └─ Income Statement                            │
│ └─ Cash Flow Statement                         │
└─────────────────────────────────────────────────┘
      ↓ Analysis & Review
┌─────────────────────────────────────────────────┐
│ ACCOUNTING REPORTS & BANKING REPORTS            │
│ └─ GL Detail Reports                           │
│ └─ Account Reconciliations                     │
│ └─ Compliance Reports                          │
│ └─ Management Reports                          │
└─────────────────────────────────────────────────┘
      ↓ Distribution
MANAGEMENT | AUDITORS | REGULATORY BODIES
```

### Module Reference Table

| Module | Function | Accountant Role | Output |
|--------|----------|-----------------|--------|
| **Bank Accounts** | Track account balances | Verify daily position | Reconciliation |
| **Funds Transfer** | Record transfers | Post to GL | Journal entries |
| **Loan Management** | Track loan status | Calculate interest | Accrual entries |
| **Card Management** | Track card activity | Post fees & interest | Transaction posting |
| **Banking Reports** | Summarize operations | Review completeness | Adjustment list |
| **Journal Entries** | Record transactions | Create & post entries | Posted entries |
| **General Ledger** | Maintain accounts | Verify balances | GL transactions |
| **Trial Balance** | Verify balancing | Review & approve | Trial Balance report |
| **Financial Statements** | Report position | Prepare statements | Financial reports |
| **Accounting Reports** | Provide detail | Generate & review | Detailed reports |
| **Chart of Accounts** | Account reference | Use for coding | GL structure |

---

## DATABASE MAPPING

### Key Tables for Accountant Operations

#### 1. **JournalEntries** Table

```sql
-- Purpose: Store all accounting entries
-- Accountant Activity: Create, review, post entries

Column               | Purpose
─────────────────────|───────────────────────────────
JournalId            | Unique entry identifier
JournalNumber        | Reference number (JE-20251122-001)
TransactionDate      | When event occurred
Description          | What was recorded
Reference            | Supporting document
TotalDebit           | Sum of debit amounts
TotalCredit          | Sum of credit amounts
Status               | Draft | Posted | Reversed
CreatedAt            | Entry creation time
CreatedBy            | Accountant who created
PostedAt             | When entry was posted
PostedBy             | Accountant who posted

Key Relationships:
→ Relates to JournalEntryLines (detail lines)
→ Links to CustomerTransactions (source)
→ References ChartOfAccounts (account codes)
```

#### 2. **JournalEntryLines** Table

```sql
-- Purpose: Detail lines within each journal entry
-- Accountant Activity: Add individual account impacts

Column               | Purpose
─────────────────────|───────────────────────────────
LineId               | Unique line identifier
JournalId            | Links to parent journal entry
AccountCode          | GL account code
AccountName          | Account description
Description          | Line-specific detail
DebitAmount          | Increase asset/expense
CreditAmount         | Increase liability/income

Example:
Journal Entry JE-001
  Line 1: Account 1010 (Bank) - Debit $5,000
  Line 2: Account 2100 (Deposit Liability) - Credit $5,000
  
Validation: All lines for an entry must balance
```

#### 3. **GeneralLedger** Table

```sql
-- Purpose: Master account list with current balances
-- Accountant Activity: Review account balances

Column               | Purpose
─────────────────────|───────────────────────────────
LedgerId             | Unique GL account
AccountCode          | Account identifier (1010, 2100, etc.)
AccountName          | Account description
AccountType          | Asset, Liability, Income, Expense, Equity
ParentAccountCode    | For hierarchical accounts
Balance              | Current account balance
IsActive             | Is account in use?

Hierarchy Example:
  1000 - ASSETS
    1010 - Bank Accounts
      1011 - Checking Account
      1012 - Savings Account
    1020 - Investments
```

#### 4. **GeneralLedgerTransactions** Table

```sql
-- Purpose: Detail ledger of all GL movements
-- Accountant Activity: Review transaction history

Column               | Purpose
─────────────────────|───────────────────────────────
GLTransactionId      | Unique transaction
JournalLineId        | Links to journal entry line
AccountCode          | GL account affected
TransactionDate      | Date of transaction
Description          | What happened
DebitAmount          | Amount debited
CreditAmount         | Amount credited
RunningBalance       | Balance after this transaction

Used For:
- Audit trail (see all changes to account)
- Account analysis (find large transactions)
- Reconciliation (match transactions to source)
```

#### 5. **TrialBalance** Table

```sql
-- Purpose: Summary of all GL accounts with balances
-- Accountant Activity: Verify balancing, investigate errors

Column               | Purpose
─────────────────────|───────────────────────────────
TrialBalanceId       | Unique report
PeriodStart          | Report period start
PeriodEnd            | Report period end
AccountCode          | GL account code
AccountName          | Account description
DebitBalance         | Debit side total
CreditBalance        | Credit side total
GeneratedAt          | When report created
GeneratedBy          | Who generated report

Validation:
Sum of DebitBalance = Sum of CreditBalance (should balance)

If doesn't balance: Investigate for posting errors
```

#### 6. **FinancialStatements** Table

```sql
-- Purpose: Store generated financial statements
-- Accountant Activity: Create and review statements

Column               | Purpose
─────────────────────|───────────────────────────────
StatementId          | Unique statement
StatementType        | BalanceSheet, IncomeStatement, CashFlow
PeriodStart          | Statement period start
PeriodEnd            | Statement period end
StatementData        | Full statement (JSON or text)
GeneratedAt          | Creation time
GeneratedBy          | Accountant who created

Example StatementData (JSON):
{
  "balanceSheet": {
    "assets": { "current": 1274900, "fixed": 700000 },
    "liabilities": { "current": 680000 },
    "equity": { "capital": 800000, "retained": 494900 }
  }
}
```

#### 7. **ChartOfAccounts** Table

```sql
-- Purpose: Reference of all valid GL accounts
-- Accountant Activity: Use to code journal entries

Column               | Purpose
─────────────────────|───────────────────────────────
AccountId            | Unique account identifier
AccountCode          | Code used in GL (1010, 2100, etc.)
AccountName          | Description (Bank Account, etc.)
AccountType          | Asset, Liability, Income, Expense, Equity
ParentAccountCode    | Parent for hierarchical structure
Level                | 1=Main, 2=Sub, 3=Detail
IsActive             | Is account available for posting?

Chart Example:
Code   Name                         Type       Level
1000   ASSETS                       Asset      1
1010   Bank Accounts                Asset      2
1011   Checking Account             Asset      3
1012   Savings Account              Asset      3
2000   LIABILITIES                  Liability  1
2100   Customer Deposits Liability  Liability  2
3000   EQUITY                       Equity     1
4000   INCOME                       Income     1
4100   Interest Income              Income     2
5000   EXPENSES                     Expense    1
5100   Operating Expenses           Expense    2
```

#### 8. **CustomerTransactions** Table

```sql
-- Purpose: Source transactions from banking operations
-- Accountant Activity: Review and post to GL

Column               | Purpose
─────────────────────|───────────────────────────────
TransactionId        | Unique transaction
TransactionNumber    | Reference (TXN-20251122-001)
AccountId            | Customer account involved
TransactionType      | Deposit, Withdrawal, Transfer, Payment
Amount               | Transaction amount
Fee                  | Fee charged
Status               | Pending, Completed, Reversed
Description          | What happened
Reference            | Supporting document
ToAccountId          | Destination account
BillerName           | If payment to biller
CreatedAt            | Transaction creation
ProcessedAt          | When posted to GL
ProcessedByEmployeeId| Employee who posted

Used For:
- Source data for journal entries
- Verification of posted amounts
- Transaction reconciliation
```

#### 9. **FundTransfers** Table

```sql
-- Purpose: Track inter-account transfers
-- Accountant Activity: Create GL entries for transfers

Column               | Purpose
─────────────────────|───────────────────────────────
TransferId           | Unique transfer
TransferNumber       | Reference
FromAccountId        | Source bank account
ToAccountId          | Destination bank account
Amount               | Transfer amount
Fee                  | Transfer fee
Status               | Pending, Completed, Reversed
CreatedAt            | When initiated
ProcessedAt          | When completed
ApprovedBy           | Approver name
ApprovedAt           | Approval timestamp

Journal Entry Created:
Debit: FromAccount (decreases)
Credit: ToAccount (increases)
Plus: Debit FeeExpense / Credit FromAccount (for fee)
```

#### 10. **BankAccounts** Table

```sql
-- Purpose: Track bank account balances
-- Accountant Activity: Reconcile to GL

Column               | Purpose
─────────────────────|───────────────────────────────
BankAccountId        | Unique account
AccountNumber        | Bank account number
AccountName          | Description
AccountType          | Checking, Savings, etc.
BankName             | External bank name
Balance              | Current balance
Currency             | Currency code

Reconciliation:
BankAccounts.Balance should = Related GL Account Balance
If different: Find missing or incorrect journal entry
```

#### 11. **CustomerLoans** Table

```sql
-- Purpose: Track loan portfolio
-- Accountant Activity: Calculate interest, track status

Column               | Purpose
─────────────────────|───────────────────────────────
LoanId               | Unique loan
LoanNumber           | Loan reference
LoanType             | Personal, Home, Auto, etc.
LoanAmount           | Original loan amount
OutstandingBalance   | Amount still owed
InterestRate         | Annual interest rate
TermMonths           | Loan term in months
MonthlyPayment       | Expected monthly payment
StartDate            | Loan start date
EndDate              | Scheduled end date
NextDueDate          | Next payment due
Status               | Active, Paid Off, Defaulted

Journal Entries Created:
1. At Disbursement:
   Debit: Loan Receivable
   Credit: Bank Account (money lent out)

2. Monthly for Interest:
   Debit: Interest Receivable
   Credit: Interest Income

3. On Payment:
   Debit: Bank Account
   Credit: Loan Receivable (principal)
   Credit: Interest Income (interest portion)
```

#### 12. **CustomerCards** Table

```sql
-- Purpose: Track customer credit cards
-- Accountant Activity: Post fees and interest

Column               | Purpose
─────────────────────|───────────────────────────────
CardId               | Unique card
AccountId            | Customer account
CardNumber           | Card number
CardType             | Visa, Mastercard, etc.
CreditLimit          | Maximum balance allowed
AvailableCredit      | Limit minus current balance
OutstandingBalance   | Amount owed
ExpiryDate           | Card expiration
DueDate              | Payment due date
InterestRate         | Annual APR
IsActive             | Is card active?

Journal Entries Created:
1. Monthly Card Fee:
   Debit: Card Fee Expense
   Credit: Fee Income

2. Monthly Interest on Balance:
   Debit: Interest Receivable
   Credit: Card Interest Income

3. On Payment:
   Debit: Bank Account
   Credit: Card Revenue
```

#### 13. **AuditLogs** Table

```sql
-- Purpose: Record all system changes
-- Accountant Activity: Review for compliance and verification

Column               | Purpose
─────────────────────|───────────────────────────────
AuditId              | Unique audit record
UserId               | User making change
Action               | Create, Update, Delete, Post
Module               | Which module affected
Description          | What changed
OldValues            | Previous values
NewValues            | New values
IpAddress            | User's IP address
UserAgent            | Browser/app information
CreatedAt            | When change made

Accountant Uses For:
- Verify who posted each entry
- Review corrections and reversals
- Compliance verification
- Fraud detection
```

#### 14. **RolePermissions** Table

```sql
-- Purpose: Define access rights by role
-- Accountant Activity: Determine available functions

Column               | Purpose
─────────────────────|───────────────────────────────
PermissionId         | Unique permission
RoleName             | Role (Accountant, Admin, etc.)
ModuleName           | Module name
Permission           | Read, Write, Delete, Full

Accountant Permissions Example:
RoleName         ModuleName              Permission
──────────────────────────────────────────────────
Accountant       JournalEntries          Full
Accountant       GeneralLedger           Read
Accountant       TrialBalance            Full
Accountant       FinancialStatements     Full
Accountant       AccountingReports       Full
Accountant       ChartOfAccounts         Read
Accountant       AuditLogs               Read
Accountant       BankAccounts            Read
Accountant       FundTransfers           Read
Accountant       CustomerTransactions    Read
Accountant       AdminPanel              None
```

#### 15. **SystemReports** Table

```sql
-- Purpose: Store generated reports
-- Accountant Activity: Create, store, distribute reports

Column               | Purpose
─────────────────────|───────────────────────────────
ReportId             | Unique report
ReportType           | Type of report
ReportName           | Report description
ReportData           | Full report content (JSON, XML, or text)
Parameters           | Report parameters used
GeneratedAt          | When created
GeneratedBy          | Accountant who created
AccessedCount        | How many times accessed
LastAccessedAt       | Last access timestamp

Used For:
- Archive of generated reports
- Audit trail of report distribution
- Historical report comparison
```

---

## ROLE-BASED ACCESS CONTROL

### Admin vs. Accountant Comparison

#### ACCESS MATRIX

| Function | Admin | Accountant | Notes |
|----------|:-----:|:----------:|-------|
| **JOURNAL ENTRIES** | | | |
| Create entries | ✅ | ✅ | Core accounting function |
| Edit own entries | ✅ | ✅ | Before posting only |
| Edit others' entries | ✅ | ❌ | Security/audit trail |
| Delete entries | ✅ | ❌ | Use reversals instead |
| Post entries | ✅ | ✅ | Create GL impact |
| Reverse entries | ✅ | ✅ | For corrections |
| **GENERAL LEDGER** | | | |
| View GL accounts | ✅ | ✅ | Read-only for Accountant |
| View GL transactions | ✅ | ✅ | Read-only for Accountant |
| Edit GL balances | ✅ | ❌ | Via journal entries only |
| Delete GL records | ✅ | ❌ | Cannot delete |
| **TRIAL BALANCE** | | | |
| Generate TB | ✅ | ✅ | Core process |
| Review TB | ✅ | ✅ | Find errors |
| Approve TB | ✅ | ✅ | Before statements |
| Lock TB | ✅ | ✅ | Prevent changes |
| **FINANCIAL STATEMENTS** | | | |
| Generate statements | ✅ | ✅ | Monthly/quarterly |
| Edit statements | ✅ | ✅ | Add notes/disclosure |
| Approve statements | ✅ | ✅ | With manager |
| Distribute statements | ✅ | ✅ | To authorized users |
| **ACCOUNTING REPORTS** | | | |
| Generate reports | ✅ | ✅ | All accounting reports |
| View reports | ✅ | ✅ | Historical & current |
| Modify reports | ✅ | ✅ | Parameters only |
| Delete reports | ✅ | ❌ | Archive instead |
| **BANKING OPERATIONS** | | | |
| View bank accounts | ✅ | ✅ | Read-only |
| View fund transfers | ✅ | ✅ | Read-only |
| View loan management | ✅ | ✅ | Read-only |
| View card management | ✅ | ✅ | Read-only |
| View banking reports | ✅ | ✅ | Read-only |
| **CHART OF ACCOUNTS** | | | |
| View accounts | ✅ | ✅ | Reference only |
| Create accounts | ✅ | ❌ | Admin authority |
| Edit accounts | ✅ | ❌ | Structure change |
| Delete accounts | ✅ | ❌ | Structure change |
| **AUDIT LOGS** | | | |
| View audit logs | ✅ | ✅ | Limited to own entries |
| Search audit logs | ✅ | ✅ | By module/date |
| Export audit logs | ✅ | ❌ | Security restriction |
| Modify audit logs | ✅ | ❌ | Never allow |
| **SYSTEM SETTINGS** | | | |
| View settings | ✅ | ❌ | Admin only |
| Modify settings | ✅ | ❌ | Admin only |
| Backup system | ✅ | ❌ | Admin only |
| User management | ✅ | ❌ | Admin only |
| **APPROVAL QUEUE** | | | |
| View pending approvals | ✅ | ❌ | Admin function |
| Approve transactions | ✅ | ❌ | Admin function |
| Reject transactions | ✅ | ❌ | Admin function |
| **ROLE PERMISSIONS** | | | |
| View role permissions | ✅ | ❌ | Admin only |
| Modify permissions | ✅ | ❌ | Admin only |
| Create new roles | ✅ | ❌ | Admin only |

### Database Implementation

#### RolePermissions Configuration for Accountant

```sql
-- Accountant Permissions Setup

INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES
  -- Journal Entries (Full Control)
  ('Accountant', 'JournalEntries', 'Full', 1),
  
  -- General Ledger (Read Only)
  ('Accountant', 'GeneralLedger', 'Read', 1),
  
  -- Trial Balance (Full Control)
  ('Accountant', 'TrialBalance', 'Full', 1),
  
  -- Financial Statements (Full Control)
  ('Accountant', 'FinancialStatements', 'Full', 1),
  
  -- Accounting Reports (Full Control)
  ('Accountant', 'AccountingReports', 'Full', 1),
  
  -- Banking Reports (Read Only)
  ('Accountant', 'BankingReports', 'Read', 1),
  
  -- Chart of Accounts (Read Only)
  ('Accountant', 'ChartOfAccounts', 'Read', 1),
  
  -- Bank Accounts (Read Only)
  ('Accountant', 'BankAccounts', 'Read', 1),
  
  -- Fund Transfers (Read Only)
  ('Accountant', 'FundTransfers', 'Read', 1),
  
  -- Loan Management (Read Only)
  ('Accountant', 'LoanManagement', 'Read', 1),
  
  -- Card Management (Read Only)
  ('Accountant', 'CardManagement', 'Read', 1),
  
  -- Customer Transactions (Read Only)
  ('Accountant', 'CustomerTransactions', 'Read', 1),
  
  -- Customer Accounts (Read Only)
  ('Accountant', 'CustomerAccounts', 'Read', 1),
  
  -- Audit Logs (Read Own)
  ('Accountant', 'AuditLogs', 'Read', 1),
  
  -- System Reports (Read)
  ('Accountant', 'SystemReports', 'Read', 1);

-- Admin Permissions (Full Control All)
INSERT INTO RolePermissions (RoleName, ModuleName, Permission, IsActive)
VALUES
  ('Admin', 'All', 'Full', 1);
```

#### Query to Check Accountant Permissions

```sql
-- Check what an Accountant can do in a specific module
SELECT 
    ModuleName,
    Permission,
    CASE Permission
        WHEN 'Read' THEN 'Can view only'
        WHEN 'Write' THEN 'Can view and modify'
        WHEN 'Delete' THEN 'Can view, modify, and delete'
        WHEN 'Full' THEN 'Has full control'
    END AS Capability
FROM RolePermissions
WHERE RoleName = 'Accountant'
ORDER BY ModuleName;
```

---

## DAILY OPERATIONS CHECKLIST

### Start of Day (8:00 AM)

- [ ] **System Access**
  - [ ] Log in to FINSYS
  - [ ] Verify user role is "Accountant"
  - [ ] Check messages for urgent items

- [ ] **Email & Notifications**
  - [ ] Check email for any alerts
  - [ ] Review exception reports from overnight
  - [ ] Note any transactions requiring investigation

- [ ] **Banking Operations Review**
  - [ ] Check daily bank settlement report
  - [ ] Review fund transfers from yesterday
  - [ ] Note any pending transactions
  - [ ] Check loan payments received
  - [ ] Review card transactions posted

### Mid-Day (11:00 AM - 12:00 PM)

- [ ] **Journal Entry Review**
  - [ ] Review draft journal entries from team
  - [ ] Verify amounts and account codes
  - [ ] Check for any imbalances
  - [ ] Request corrections as needed
  - [ ] Approve entries for posting

- [ ] **Posting**
  - [ ] Post approved journal entries to GL
  - [ ] Verify posting was successful
  - [ ] Create audit trail documentation

- [ ] **Reconciliation**
  - [ ] Review bank reconciliation
  - [ ] Investigate any outstanding items
  - [ ] Follow up on overdue items

### End of Day (4:00 PM)

- [ ] **Daily Trial Balance**
  - [ ] Generate end-of-day trial balance
  - [ ] Verify debits = credits
  - [ ] Investigate any imbalances
  - [ ] Document any findings

- [ ] **Transaction Completeness**
  - [ ] Verify all banking operations posted
  - [ ] Check for any transactions pending posting
  - [ ] Estimate GL balances for next day

- [ ] **Communication**
  - [ ] Send updates to operations team
  - [ ] Flag any items needing follow-up tomorrow
  - [ ] Document any issues for the manager

- [ ] **System Closure**
  - [ ] Save all reports
  - [ ] Close off any draft entries
  - [ ] Lock period if end of month
  - [ ] Backup important files

---

## PERIOD-END CLOSE PROCESS

### Month-End Close (3-5 Days)

**Day 1: Adjustments**
- [ ] Post interest accruals
- [ ] Post depreciation
- [ ] Record loan loss provisions
- [ ] Accrue expenses
- [ ] Record inventory adjustments

**Day 2: Reconciliations**
- [ ] Bank reconciliation complete
- [ ] Customer accounts verified
- [ ] Loan portfolio verified
- [ ] Trial balance generated and balanced

**Day 3: Statements**
- [ ] Balance Sheet prepared
- [ ] Income Statement prepared
- [ ] Cash Flow Statement prepared
- [ ] Notes and disclosures added

**Day 4: Review**
- [ ] Management review completed
- [ ] Adjustments approved
- [ ] Statements signed off
- [ ] Distribution prepared

**Day 5: Archival**
- [ ] Statements distributed
- [ ] Period locked
- [ ] Backups completed
- [ ] Next period opened

---

## COORDINATION & ESCALATION

### When to Contact Operations
- Unexplained transaction differences
- Missing deposits or withdrawals
- Timing differences in postings
- Transaction reversals or corrections

### When to Contact Loan Officers
- Interest calculation discrepancies
- Loan status changes
- Defaulted loan management
- Early payoff or modification

### When to Contact Card Services
- Charge-back disputes
- Fee discrepancies
- Interest calculation questions
- Card holder disputes

### When to Escalate to Management
- Unusual or suspicious transactions
- Financial statement variances exceeding thresholds
- Potential compliance issues
- Emergency or time-sensitive items

---

## SUMMARY

The **Accountant** is the financial record-keeper of the bank. Working systematically through the accounting cycle:

1. **Receives** banking operations data
2. **Records** transactions as Journal Entries
3. **Maintains** the General Ledger
4. **Validates** accuracy via Trial Balance
5. **Prepares** Financial Statements
6. **Generates** Reports for stakeholders
7. **Coordinates** with other departments
8. **Closes** periods properly
9. **Archives** records securely

The Accountant has **limited but focused access** to accounting modules and **read-only access** to operational modules. This ensures financial integrity while preventing unauthorized changes to system-critical data.

---

**Document Version:** 1.0  
**Last Updated:** November 22, 2025  
**Created By:** System Documentation Team  
**Approved By:** Finance Manager

