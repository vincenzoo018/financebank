# Finance Manager Role – Limited Access Flowchart

## Overview
The Finance Manager role is a **mid-level position** with **review and analysis privileges** but **no administrative authority**. This flowchart demonstrates the controlled access model where Finance Managers can view, validate, and recommend actions, but cannot make final approval decisions.

---

## 1. FINANCE MANAGER LOGIN & DASHBOARD ACCESS

```
┌─────────────────────────┐
│ Finance Manager Login   │
│ (Email + Password)      │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Dashboard Access        │
│ (Limited - Finance View │
│  DENIED: User Mgmt      │
│  DENIED: System Settings│
│  DENIED: Admin Settings)│
└────────────┬────────────┘
             │
             ▼
    ┌────────────────┐
    │ Main Dashboard │
    │ (Finance Only) │
    └────────────────┘
             │
    ┌────────┴────────┬──────────┬─────────┬──────────┬──────────┐
    │                 │          │         │          │          │
    ▼                 ▼          ▼         ▼          ▼          ▼
   [AP]          [AR]        [Budget]  [Forecast]  [CashFlow] [Reports]
```

---

## 2. ACCOUNTS PAYABLE (LIMITED ACCESS)

### 2A. View Invoices
```
┌─────────────────────────────┐
│ Accounts Payable Dashboard  │
│ [VIEW ONLY]                 │
└────────────┬────────────────┘
             │
    ┌────────┴────────────────┐
    │                         │
    ▼                         ▼
┌─────────────────┐   ┌──────────────────┐
│ View All        │   │ View by Status   │
│ Outstanding     │   │ - Pending        │
│ Bills           │   │ - Scheduled      │
└────────────┬────┘   │ - Overdue        │
             │        └────────────┬─────┘
             │                     │
    ┌────────┴─────────────────────┘
    │
    ▼
┌────────────────────────────────────┐
│ Filter & Search                    │
│ - By Vendor                        │
│ - By Invoice Date                  │
│ - By Amount Range                  │
│ - By Due Date                      │
└────────────┬───────────────────────┘
             │
             ▼
    ┌─────────────────┐
    │ Display: Bills  │
    │ • Invoice #     │
    │ • Vendor Name   │
    │ • Amount        │
    │ • Due Date      │
    │ • Status        │
    └─────────────────┘
```

### 2B. Validate Invoice Details
```
┌──────────────────────────┐
│ Select Invoice to Review │
│ [REVIEW ONLY]            │
└────────────┬─────────────┘
             │
             ▼
┌───────────────────────────────┐
│ Validate Invoice Details:     │
│ ✓ Verify vendor info          │
│ ✓ Verify amounts              │
│ ✓ Check invoice number        │
│ ✓ Confirm due date            │
│ ✓ Review description          │
│ ✗ CANNOT EDIT any fields      │
└────────────┬──────────────────┘
             │
    ┌────────┴────────┐
    │                 │
    ▼                 ▼
┌──────────┐   ┌──────────────────┐
│ Valid    │   │ Issues Found     │
│ ✓        │   │ Flag for Admin   │
└────────┬─┘   └────────┬─────────┘
         │               │
         │               ▼
         │        ┌──────────────┐
         │        │ Add Comments │
         │        │ (Validation  │
         │        │  Notes)      │
         │        └──────┬───────┘
         │               │
         └───────┬───────┘
                 │
                 ▼
        ┌────────────────────┐
        │ Submit for Admin   │
        │ Review             │
        └────────────────────┘
```

### 2C. Recommend Payment
```
┌──────────────────────────────┐
│ Payment Recommendation       │
│ [SUBMIT FOR APPROVAL]        │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Review Bill for Payment      │
│ • Validate vendor info       │
│ • Confirm amount correct     │
│ • Check payment method       │
│ • Verify due date            │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Add Payment Recommendation   │
│ • Select payment method      │
│ • Choose schedule date       │
│ • Add comments               │
│ • Reason for payment         │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Submit for Approval          │
│ [CANNOT APPROVE DIRECTLY]    │
└────────────┬─────────────────┘
             │
    ┌────────┴────────┐
    │                 │
    ▼                 ▼
┌──────────┐  ┌──────────────────────┐
│ Pending  │  │ Submitted for        │
│ Review   │  │ Admin Approval       │
│ Status   │  └──────────┬───────────┘
└──────────┘             │
                         ▼
                  ┌────────────────┐
                  │ Awaiting Admin │
                  │ Decision       │
                  └────────────────┘
```

### 2D. Access Restrictions (What Finance Manager CANNOT Do)
```
┌────────────────────────────────────┐
│ RESTRICTED ACTIONS - ACCOUNTS      │
│ PAYABLE (Cannot Perform)           │
├────────────────────────────────────┤
│ ✗ Approve or finalize payments     │
│ ✗ Release final payment            │
│ ✗ Modify invoice amounts           │
│ ✗ Change vendor details            │
│ ✗ Delete invoice records           │
│ ✗ Record payment directly          │
│ ✗ Override approval workflow       │
│ ✗ Access financial statements      │
│ ✗ Modify chart of accounts         │
│ ✗ Post to general ledger           │
└────────────────────────────────────┘
```

---

## 3. ACCOUNTS RECEIVABLE (LIMITED ACCESS)

### 3A. View Customer Payments
```
┌──────────────────────────────┐
│ Accounts Receivable          │
│ Dashboard                    │
│ [VIEW ONLY]                  │
└────────────┬─────────────────┘
             │
    ┌────────┴────────────┐
    │                     │
    ▼                     ▼
┌───────────────┐  ┌──────────────────┐
│ View All      │  │ Payment Status   │
│ Outstanding   │  │ - Paid           │
│ Receivables   │  │ - Pending        │
└────────────┬──┘  │ - Overdue        │
             │     │ - Partial        │
             │     └────────────┬─────┘
             │                  │
    ┌────────┴──────────────────┘
    │
    ▼
┌────────────────────────────────┐
│ Filter & Search                │
│ - By Customer Name             │
│ - By Invoice Date              │
│ - By Amount Range              │
│ - By Payment Status            │
└────────────┬───────────────────┘
             │
             ▼
    ┌─────────────────┐
    │ Display:        │
    │ • Invoice #     │
    │ • Customer Name │
    │ • Amount Due    │
    │ • Amount Paid   │
    │ • Due Date      │
    │ • Status        │
    └─────────────────┘
```

### 3B. Validate Entries
```
┌─────────────────────────────┐
│ Validate Payment Entry      │
│ [REVIEW ONLY]               │
└────────────┬────────────────┘
             │
             ▼
┌─────────────────────────────┐
│ Verify Payment Details:     │
│ ✓ Customer information      │
│ ✓ Invoice amount            │
│ ✓ Payment amount received   │
│ ✓ Payment date              │
│ ✓ Reference number          │
│ ✗ CANNOT MODIFY             │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │                 │
    ▼                 ▼
┌──────────┐   ┌──────────────────┐
│ Valid    │   │ Discrepancies    │
│ Entry    │   │ Found            │
└────────┬─┘   └────────┬─────────┘
         │               │
         │               ▼
         │        ┌──────────────┐
         │        │ Flag Issue   │
         │        │ (Documented) │
         │        └──────┬───────┘
         │               │
         └───────┬───────┘
                 │
                 ▼
        ┌────────────────────┐
        │ Status: Validated  │
        │ Pending Admin      │
        │ Review             │
        └────────────────────┘
```

### 3C. Flag Discrepancies
```
┌────────────────────────────────┐
│ Identify Discrepancies         │
│ [SUBMIT FOR APPROVAL]          │
└────────────┬───────────────────┘
             │
    ┌────────┴──────┬──────────┬─────────┐
    │               │          │         │
    ▼               ▼          ▼         ▼
┌────────┐   ┌─────────┐  ┌────────┐  ┌────────┐
│Amount  │   │Payment  │  │Date    │  │Missing │
│Mismatch│   │Late     │  │Mismatch│  │Docs    │
└────┬───┘   └────┬────┘  └───┬────┘  └───┬────┘
     │            │           │           │
     └────────┬───┴───────┬───┴───────┬───┘
              │           │           │
              ▼           ▼           ▼
        ┌─────────────────────────────────┐
        │ Flag Details:                   │
        │ • Description of discrepancy    │
        │ • Supporting documentation      │
        │ • Recommended action            │
        │ • Priority level                │
        └────────────┬────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │ Submit to Admin for    │
        │ Investigation & Action │
        └────────────────────────┘
```

### 3D. Generate AR Summary
```
┌──────────────────────────────┐
│ Generate AR Summary Report   │
│ [GENERATE ONLY]              │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Select Report Period:        │
│ • Start Date                 │
│ • End Date                   │
│ • Filter by Customer (opt.)  │
│ • Filter by Status (opt.)    │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Report Data Calculated:      │
│ • Total receivables          │
│ • Total collected            │
│ • Outstanding balance        │
│ • Aging analysis             │
│ • Collection rate %          │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Generated Report:            │
│ Format Options:              │
│ • Export as PDF              │
│ • Export as Excel            │
│ • View on Screen             │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Summary Report Ready         │
│ [Cannot Modify Data]         │
│ [Ready to Submit to Admin]   │
└──────────────────────────────┘
```

### 3E. Access Restrictions (AR)
```
┌────────────────────────────────────┐
│ RESTRICTED ACTIONS - ACCOUNTS      │
│ RECEIVABLE (Cannot Perform)        │
├────────────────────────────────────┤
│ ✗ Modify customer balance          │
│ ✗ Create new transactions          │
│ ✗ Record payments directly         │
│ ✗ Write off receivables            │
│ ✗ Change payment terms             │
│ ✗ Delete customer records          │
│ ✗ Post to general ledger           │
│ ✗ Override data validation         │
└────────────────────────────────────┘
```

---

## 4. BUDGET MANAGEMENT (RESTRICTED)

### 4A. View Existing Budgets
```
┌──────────────────────────┐
│ Budget Dashboard         │
│ [VIEW ONLY]              │
└────────────┬─────────────┘
             │
    ┌────────┴──────────┬─────────────┐
    │                   │             │
    ▼                   ▼             ▼
┌──────────┐  ┌──────────────┐  ┌─────────────┐
│ Current  │  │ By Category  │  │ By Year     │
│ Budgets  │  │ - Operating  │  │ - 2024      │
│          │  │ - Capital    │  │ - 2025      │
│          │  │ - Projects   │  │ - 2026      │
└────┬─────┘  └────┬─────────┘  └──────┬──────┘
     │             │                    │
     └─────────┬───┴────────────────────┘
               │
               ▼
    ┌──────────────────────┐
    │ View Budget Details: │
    │ • Allocated Amount   │
    │ • Spent to Date      │
    │ • Remaining Balance  │
    │ • Status             │
    │ • Performance vs Exp │
    └──────────────────────┘
```

### 4B. Propose New Budgets
```
┌────────────────────────────┐
│ Budget Proposal            │
│ [SUBMIT FOR APPROVAL]      │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ Create New Budget:         │
│ • Budget Name              │
│ • Department/Category      │
│ • Fiscal Year              │
│ • Start & End Date         │
│ • Proposed Amount          │
│ • Justification            │
│ • Supporting Documents     │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ Budget Proposal Details:   │
│ • Compare with Historical  │
│ • Analyze trends           │
│ • Calculate variance       │
│ • Add comments             │
│ • Recommend approval       │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│ Submit for Admin Approval  │
│ [CANNOT FINALIZE]          │
└────────────┬───────────────┘
             │
    ┌────────┴─────────┐
    │                  │
    ▼                  ▼
┌─────────┐   ┌──────────────────┐
│ Pending │   │ Awaiting Admin   │
│ Review  │   │ Decision         │
└─────────┘   └──────────────────┘
```

### 4C. Submit Changes for Admin Approval
```
┌──────────────────────────────┐
│ Budget Modification Request  │
│ [SUBMIT FOR APPROVAL]        │
└────────────┬─────────────────┘
             │
    ┌────────┴──────────────┐
    │                       │
    ▼                       ▼
┌────────────┐    ┌──────────────────┐
│ Increase   │    │ Reallocation     │
│ Budget     │    │ Between Category │
└────┬───────┘    └────────┬─────────┘
     │                      │
     ▼                      ▼
┌────────────────────────────────┐
│ Modification Details:          │
│ • Current Amount               │
│ • Proposed Amount              │
│ • Reason for Change            │
│ • Impact Analysis              │
│ • Supporting Justification     │
└────────────┬───────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Add Comments & Approval      │
│ Request:                     │
│ • Finance Manager signature  │
│ • Timestamp                  │
│ • Supporting docs            │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Submit to Admin for Review   │
│ & Approval                   │
└────────────┬─────────────────┘
             │
    ┌────────┴──────────┐
    │                   │
    ▼                   ▼
┌─────────┐    ┌─────────────────────┐
│ Pending │    │ Awaiting Admin      │
│ Review  │    │ Approval/Decision   │
└─────────┘    └─────────────────────┘
```

### 4D. Access Restrictions (Budget)
```
┌────────────────────────────────────┐
│ RESTRICTED ACTIONS - BUDGET        │
│ MANAGEMENT (Cannot Perform)        │
├────────────────────────────────────┤
│ ✗ Finalize budgets                 │
│ ✗ Publish budgets                  │
│ ✗ Override budget limits           │
│ ✗ Delete approved budgets          │
│ ✗ Change budget period             │
│ ✗ Approve budget requests          │
│ ✗ Allocate funds directly          │
│ ✗ Post to ledger                   │
└────────────────────────────────────┘
```

---

## 5. FINANCIAL FORECASTING (FINANCE ONLY)

### 5A. Access Financial History
```
┌──────────────────────────────┐
│ Financial History Access     │
│ [VIEW ONLY]                  │
└────────────┬─────────────────┘
             │
    ┌────────┴────────────────────┐
    │                             │
    ▼                             ▼
┌──────────────┐   ┌───────────────────────┐
│ Revenue Data │   │ Expense Data          │
│ - Monthly    │   │ - Monthly             │
│ - Quarterly  │   │ - Quarterly           │
│ - Annual     │   │ - Annual              │
└────┬─────────┘   └─────────┬─────────────┘
     │                       │
     └───────────┬───────────┘
                 │
                 ▼
    ┌────────────────────────────┐
    │ Historical Analysis:       │
    │ • Trends & patterns        │
    │ • Seasonal variations      │
    │ • Growth rates             │
    │ • Anomalies & outliers     │
    └────────────────────────────┘
```

### 5B. Generate Revenue/Expense Projections
```
┌────────────────────────────────┐
│ Create Forecast Projection     │
│ [GENERATE ONLY]                │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Forecast Parameters:           │
│ • Period (Quarter/Year)        │
│ • Revenue assumptions          │
│ • Expense assumptions          │
│ • Growth factors               │
│ • Market conditions            │
│ • Historical variance          │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Projection Calculation:        │
│ • Run forecasting model        │
│ • Revenue projections          │
│ • Expense projections          │
│ • Net income forecast          │
│ • Variance analysis            │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Review Forecast Results:       │
│ • Reasonableness check         │
│ • Compare vs budget            │
│ • Identify risks               │
│ • Add assumptions note         │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Forecast Data Generated        │
└────────────────────────────────┘
```

### 5C. Create Forecasting Reports
```
┌────────────────────────────┐
│ Generate Forecast Report   │
│ [GENERATE ONLY]            │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Report Parameters:             │
│ • Report Type (Quarterly/Yr)   │
│ • Forecast Period              │
│ • Department/Division          │
│ • Level of detail              │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Report Components:             │
│ • Executive Summary            │
│ • Revenue Forecast             │
│ • Expense Forecast             │
│ • Key Assumptions              │
│ • Sensitivity Analysis         │
│ • Risk Factors                 │
│ • Recommendations              │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Export Options:                │
│ • PDF                          │
│ • Excel                        │
│ • Email distribution           │
│ • Print                        │
└────────────┬───────────────────┘
             │
             ▼
┌────────────────────────────────┐
│ Report Ready for Submission    │
│ to Admin/Executive Team        │
└────────────────────────────────┘
```

### 5D. Access Restrictions (Forecasting)
```
┌────────────────────────────────────┐
│ RESTRICTED ACTIONS - FINANCIAL     │
│ FORECASTING (Cannot Perform)       │
├────────────────────────────────────┤
│ ✗ Edit raw accounting data         │
│ ✗ Modify historical transactions   │
│ ✗ Alter chart of accounts          │
│ ✗ Post journal entries             │
│ ✗ Override forecast assumptions    │
│ ✗ Publish forecasts as actual      │
│ ✗ Access confidential data         │
│ ✗ Modify forecasting methodology   │
└────────────────────────────────────┘
```

---

## 6. CASH FLOW ANALYSIS

### 6A. View Cash Inflows/Outflows
```
┌────────────────────────────┐
│ Cash Flow Dashboard        │
│ [VIEW ONLY]                │
└────────────┬───────────────┘
             │
    ┌────────┴──────────┬────────────────┐
    │                   │                │
    ▼                   ▼                ▼
┌────────────┐  ┌──────────────┐  ┌────────────┐
│ Operating  │  │ Investing    │  │ Financing  │
│ Activities │  │ Activities   │  │ Activities │
└────┬───────┘  └────┬─────────┘  └───┬────────┘
     │               │                │
     └───────────┬───┴────────────────┘
                 │
                 ▼
    ┌────────────────────────┐
    │ Cash Flow Summary:     │
    │ • Opening Balance      │
    │ • Inflows (Daily)      │
    │ • Outflows (Daily)     │
    │ • Net Change           │
    │ • Closing Balance      │
    └────────────────────────┘
```

### 6B. Analyze Liquidity
```
┌──────────────────────────────┐
│ Liquidity Analysis           │
│ [ANALYZE ONLY]               │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Liquidity Metrics:           │
│ • Current Ratio              │
│ • Quick Ratio                │
│ • Cash Ratio                 │
│ • Days of Cash on Hand       │
│ • Cash Conversion Cycle      │
│ • Operating Cash Flow        │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Trend Analysis:              │
│ • 12-Month trends            │
│ • Seasonal patterns          │
│ • Risk factors               │
│ • Adequacy assessment        │
│ • Recommendations            │
└────────────┬─────────────────┘
             │
             ▼
    ┌────────────────────┐
    │ Analysis Complete  │
    └────────────────────┘
```

### 6C. Produce Cash Flow Insights
```
┌───────────────────────────────┐
│ Generate Cash Flow Insights   │
│ [SUBMIT FOR APPROVAL]         │
└────────────┬──────────────────┘
             │
             ▼
┌───────────────────────────────┐
│ Insight Analysis:             │
│ • Key drivers of cash flow    │
│ • Seasonal patterns           │
│ • Volatility factors          │
│ • Forecast cash requirements  │
│ • Opportunity identification  │
└────────────┬──────────────────┘
             │
             ▼
┌───────────────────────────────┐
│ Generate Report:              │
│ • Executive summary           │
│ • Detailed analysis           │
│ • Visualizations              │
│ • Recommendations             │
│ • Action items                │
└────────────┬──────────────────┘
             │
             ▼
┌───────────────────────────────┐
│ Submit to Admin:              │
│ "Cash Flow Insights Ready"    │
└───────────────────────────────┘
```

### 6D. Access Restrictions (Cash Flow)
```
┌────────────────────────────────────┐
│ RESTRICTED ACTIONS - CASH FLOW     │
│ ANALYSIS (Cannot Perform)          │
├────────────────────────────────────┤
│ ✗ Alter ledger entries             │
│ ✗ Modify transaction records       │
│ ✗ Execute payments/transfers       │
│ ✗ Override approval process        │
│ ✗ Delete transaction history       │
│ ✗ Change reconciliation            │
│ ✗ Post to general ledger           │
└────────────────────────────────────┘
```

---

## 7. FINANCIAL REPORTS (READ + GENERATE ONLY)

### 7A. Generate Standard Reports
```
┌────────────────────────────┐
│ Report Generation Menu     │
│ [GENERATE ONLY]            │
└────────────┬───────────────┘
             │
    ┌────────┴────────┬─────────────┬───────────┐
    │                 │             │           │
    ▼                 ▼             ▼           ▼
┌──────────┐  ┌──────────────┐  ┌─────────┐  ┌──────────┐
│ P&L      │  │ Balance      │  │ Cash    │  │ Custom   │
│ Statement│  │ Sheet        │  │ Flow    │  │ Report   │
└────┬─────┘  └────┬─────────┘  └────┬────┘  └────┬─────┘
     │             │                 │           │
     └─────────┬───┴─────────────┬───┴───────────┘
               │                 │
               ▼                 ▼
    ┌────────────────────────────────────┐
    │ Select Report Parameters:          │
    │ • Period (Month/Quarter/Year)      │
    │ • Department (if applicable)       │
    │ • Detail Level (Summary/Detail)    │
    │ • Comparative (YTD, Prior Year)    │
    │ • Format (Excel, PDF, etc.)        │
    └────────────┬───────────────────────┘
                 │
                 ▼
    ┌────────────────────────────────────┐
    │ Generate Report:                   │
    │ • Aggregate data                   │
    │ • Calculate metrics                │
    │ • Format output                    │
    │ • Validate completeness            │
    └────────────┬───────────────────────┘
                 │
                 ▼
    ┌────────────────────────────────────┐
    │ Report Generated Successfully      │
    │ [CANNOT MODIFY DATA]               │
    └────────────────────────────────────┘
```

### 7B. Export Reports
```
┌────────────────────────────┐
│ Export Generated Report    │
│ [EXPORT ALLOWED]           │
└────────────┬───────────────┘
             │
    ┌────────┴──────────────────────┐
    │                               │
    ▼                               ▼
┌─────────┐                  ┌──────────────┐
│ Format  │                  │ Distribution │
├─────────┤                  ├──────────────┤
│ PDF     │                  │ Email        │
│ Excel   │                  │ Print        │
│ CSV     │                  │ Share Link   │
│ Print   │                  │ Archive      │
└────┬────┘                  └────┬─────────┘
     │                            │
     └──────────┬─────────────────┘
                │
                ▼
    ┌──────────────────────────┐
    │ Export Complete          │
    │ Report Ready for Use     │
    └──────────────────────────┘
```

### 7C. Submit Reports to Admin
```
┌──────────────────────────┐
│ Submit Report for Review │
│ [SUBMIT FOR APPROVAL]    │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Add Submission Details:      │
│ • Cover memo                 │
│ • Key findings               │
│ • Recommendations            │
│ • Action items               │
│ • Deadline/urgency           │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Attach Supporting Docs:      │
│ • Calculations               │
│ • Source data                │
│ • Assumptions                │
│ • Methodology notes          │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────┐
│ Submit to Admin for Review   │
└────────────┬─────────────────┘
             │
    ┌────────┴─────────┐
    │                  │
    ▼                  ▼
┌─────────┐   ┌──────────────────┐
│ Submitted│   │ Awaiting Admin   │
│ Status   │   │ Review           │
└──────────┘   └──────────────────┘
```

### 7D. Access Restrictions (Reports)
```
┌──────────────────────────────────┐
│ RESTRICTED ACTIONS - FINANCIAL   │
│ REPORTS (Cannot Perform)         │
├──────────────────────────────────┤
│ ✗ Alter underlying data           │
│ ✗ Modify journal entries          │
│ ✗ Change accounting policies      │
│ ✗ Post transactions               │
│ ✗ Delete report data              │
│ ✗ Publish reports as official    │
│ ✗ Change report template          │
│ ✗ Access confidential data        │
└──────────────────────────────────┘
```

---

## 8. END STATES & OUTCOMES

### 8A. Pending Approvals
```
┌────────────────────────────────────┐
│ PENDING APPROVALS                  │
├────────────────────────────────────┤
│ • AP Payments: Submitted for       │
│   Admin Approval (Final Payment)   │
│ • AR Validations: Submitted for    │
│   Admin Review                     │
│ • Budget Changes: Submitted for    │
│   Admin Approval                   │
│ • Cash Flow Insights: Available    │
│   for Admin Review                 │
│ • Reports: Submitted for Admin     │
│   Review & Distribution            │
└────────────────────────────────────┘
```

### 8B. Completed Tasks
```
┌────────────────────────────────────┐
│ COMPLETED TASKS                    │
├────────────────────────────────────┤
│ ✓ Reviewed outstanding bills       │
│ ✓ Validated invoice details        │
│ ✓ Recommended payments             │
│ ✓ Validated payment entries        │
│ ✓ Flagged discrepancies            │
│ ✓ Generated AR summary             │
│ ✓ Reviewed budgets                 │
│ ✓ Proposed new budgets             │
│ ✓ Analyzed liquidity               │
│ ✓ Generated cash flow insights     │
│ ✓ Generated financial reports      │
│ ✓ Exported reports                 │
│ ✓ Submitted for admin action       │
└────────────────────────────────────┘
```

### 8C. Status Indicators
```
┌────────────────────────────────┐
│ ACTION STATUS INDICATORS       │
├────────────────────────────────┤
│ 🟡 PENDING          Awaiting   │
│    (Yellow)         admin      │
│                    action      │
│                                │
│ 🟢 REVIEW READY     Ready for  │
│    (Green)          admin      │
│                    review      │
│                                │
│ 🔴 ACTION REQUIRED  Admin      │
│    (Red)            returned   │
│                    for         │
│                    revision    │
└────────────────────────────────┘
```

---

## 9. KEY PRINCIPLES

### 9A. Finance Manager Authority Model
```
┌─────────────────────────────────────────┐
│ AUTHORITY HIERARCHY                     │
├─────────────────────────────────────────┤
│ 1. VIEW/ANALYZE (Finance Manager)       │
│    └─ Can see all finance data          │
│    └─ Can analyze & report              │
│                                         │
│ 2. RECOMMEND/SUBMIT (Finance Manager)   │
│    └─ Can propose actions               │
│    └─ Can submit for approval           │
│                                         │
│ 3. APPROVE/EXECUTE (Admin Only)         │
│    └─ Finance Manager CANNOT do this    │
│    └─ Admin has final authority         │
└─────────────────────────────────────────┘
```

### 9B. Workflow Pattern
```
Finance Manager        →  Submission Queue  →  Admin Review  →  Execute/Reject
┌─────────────────┐      ┌───────────────┐      ┌──────────┐      ┌──────────┐
│ • View Data     │      │ • Stored in   │      │ • Verify │      │ • Final  │
│ • Analyze       │  →   │   approval    │  →   │ • Decide │  →   │   Action │
│ • Recommend     │      │   queue       │      │ • Comment│      │ • Report │
│ • Document      │      │ • Tracked     │      │ • Log    │      │ • Audit  │
└─────────────────┘      └───────────────┘      └──────────┘      └──────────┘
    (Mid-level)         (Transparent)          (Review)           (Authority)
```

### 9C. Access Control Summary
```
┌──────────────────────────────────────────────┐
│ FINANCE MANAGER - ACCESS MATRIX              │
├──────────────┬──────────┬──────────┬─────────┤
│ Module       │ View     │ Analyze  │ Execute │
├──────────────┼──────────┼──────────┼─────────┤
│ A/P          │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ A/R          │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ Budget       │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ Forecast     │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ Cash Flow    │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ Reports      │ ✓ Yes    │ ✓ Yes    │ ✗ No    │
│ GL/Ledger    │ ✓ View   │ ✗ No     │ ✗ No    │
│ Settings     │ ✗ No     │ ✗ No     │ ✗ No    │
│ User Mgmt    │ ✗ No     │ ✗ No     │ ✗ No    │
└──────────────┴──────────┴──────────┴─────────┘
```

---

## SUMMARY

**Finance Manager Role: Mid-Level Financial Authority**

The Finance Manager operates within a controlled environment with specific boundaries:
- **CAN**: View, analyze, validate, recommend, and submit financial actions
- **CANNOT**: Approve, execute, or modify underlying financial data
- **SUBMIT TO**: Admin/Finance Director for final authorization

All Finance Manager actions flow through an **approval queue** for audit and control purposes, ensuring proper governance and accountability in the banking-finance system.

