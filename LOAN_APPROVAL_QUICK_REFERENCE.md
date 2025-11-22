# Loan Approval Workflow - Quick Reference

## What Changed? ⚡

### The Problem
Loans were being auto-approved when submitted (Status="Approved") instead of waiting for admin review.

### The Solution
1. Changed initial ApprovalQueue Status from "Approved" → "Pending" ✅
2. Added Loan status update in approval method (Pending → Active) ✅
3. Added Loan status update in rejection method (Pending → Rejected) ✅
4. Filter rejected loans from customer view ✅

### The Result
- Loans now appear in admin approval queue
- Admin can approve/reject each loan
- Approved loans become active
- Rejected loans hidden from customer
- Workflow matches card application pattern

---

## Workflow at a Glance

```
CUSTOMER SUBMITS LOAN
        ↓
    [Pending]
        ↓
ADMIN REVIEWS
        ↓
    ┌─────────────┐
    │   Approve   │
    │    Reject   │
    └──┬────────┬─┘
       ↓        ↓
    [Active] [Rejected]
       ↓        ↓
    Customer  Hidden
    Sees It   from View
```

---

## Files Modified

| File | Changes | Impact |
|------|---------|--------|
| `CardApplicationService.cs` | SubmitLoanApplicationAsync: Change Status="Pending" | Loans now queue for approval |
| `CardApplicationService.cs` | ApproveLoanApplicationAsync: Update Loan status | Approval makes loan active |
| `CardApplicationService.cs` | RejectLoanApplicationAsync: Update Loan status | Rejection hides loan |
| `Loans.razor` | Filter: `Status != "Rejected"` | Rejected loans hidden |

---

## Quick Test

### 1. Customer Submits
```
Go to Customer → Loans
Click "Apply for Loan"
Select: Personal, ₱50,000, 24 months
Click Submit
→ Success message shown
```

### 2. Check Database
```
SELECT * FROM CustomerLoans WHERE LoanId = [NewId]
→ Status should be "Pending"

SELECT * FROM ApprovalQueue WHERE RequestType = 'LOAN' 
→ Status should be "Pending"
```

### 3. Admin Reviews
```
Go to Admin → Approvals → Loan Applications
→ Pending (1) tab should show 1 loan
→ Loan should be clickable
```

### 4. Admin Approves
```
Click "Review" on loan
Click "Approve" (keep ₱50,000)
→ Success notification
→ Check database: Status = "Approved"
```

### 5. Customer Sees Loan
```
Customer logs in
Go to Loans page
→ Loan now appears in "My Active Loans"
```

---

## Key Methods

### SubmitLoanApplicationAsync
```csharp
// Creates:
// 1. Loan record (Status="Pending")
// 2. ApprovalQueue entry (Status="Pending")

Result: Loan awaits admin review
```

### ApproveLoanApplicationAsync
```csharp
// Updates:
// 1. ApprovalQueue (Status="Approved", ProcessedAt, ProcessedBy)
// 2. Loan (Status="Active")

Result: Loan becomes available to customer
```

### RejectLoanApplicationAsync
```csharp
// Updates:
// 1. ApprovalQueue (Status="Rejected", reason)
// 2. Loan (Status="Rejected")

Result: Loan hidden from customer
```

---

## Status Values

| Status | Meaning | Visible? | Admin Can |
|--------|---------|----------|-----------|
| Pending | Awaiting admin review | No | Approve/Reject |
| Active | Approved, customer has it | Yes | View, Track |
| Rejected | Admin denied | No | View reason |
| Paid | Customer finished paying | Yes | View history |
| Closed | Account closed | No | Archive |
| Defaulted | Payment default | No | Manage |

---

## Build & Run

```powershell
# Stop running instance
taskkill /F /IM FinanceBank.exe

# Rebuild
dotnet build

# Run
dotnet run
```

---

## Verify It Works

1. **Build**: ✅ No compilation errors
2. **Submit**: ✅ Loan appears in database as Pending
3. **Queue**: ✅ Loan shows in admin Pending tab (count > 0)
4. **Approve**: ✅ Loan status changes to Active
5. **Display**: ✅ Loan appears in customer view
6. **Reject**: ✅ Loan marked as Rejected (hidden from customer)

---

## Debug Tips

### Check database tables:
```sql
-- See pending loans
SELECT * FROM CustomerLoans WHERE Status = 'Pending'

-- See approval queue
SELECT * FROM ApprovalQueue WHERE RequestType = 'LOAN'

-- See specific loan
SELECT * FROM CustomerLoans WHERE LoanId = 1
```

### Enable Debug Output:
```
Visual Studio → Debug → Windows → Output
Watch for: [ApproveLoanApplication], [RejectLoanApplication]
```

### Common Issues:
- **Loan not in queue?** → Check if Status='Pending' in database
- **Approval fails?** → Check debug output for exception
- **Loan doesn't activate?** → Verify Loan.Status updated to 'Active'
- **Still shows rejected?** → Clear browser cache (Ctrl+F5)

---

## Success Criteria ✓

- [x] Compilation successful (all platforms)
- [x] Loan creation with Pending status
- [x] Loan appears in admin queue
- [x] Admin approval updates status to Active
- [x] Admin rejection updates status to Rejected
- [x] Approved loans visible to customer
- [x] Rejected loans hidden from customer
- [x] Database consistency maintained
- [x] Matches card workflow pattern
- [x] Timestamps recorded correctly

---

## Architecture Summary

```
Customer Portal
    ↓
  Loans.razor
    ↓
CardApplicationService
    ├─ SubmitLoanApplicationAsync
    ├─ ApproveLoanApplicationAsync
    └─ RejectLoanApplicationAsync
    ↓
BFASDbContext (Entity Framework)
    ├─ CustomerLoans table
    └─ ApprovalQueue table
    ↓
SQL Server Database
```

---

## Time to Test: ~15 minutes

1. Submit loan: 1 min
2. Check database: 2 min
3. Admin review: 2 min
4. Admin approve: 2 min
5. Verify customer sees: 2 min
6. Test rejection: 3 min
7. Final verification: 3 min

Total: ~15 min for complete workflow validation

---

## Support Resources

- **Code Changes**: See LOAN_WORKFLOW_CODE_CHANGES.md
- **Full Details**: See LOAN_APPROVAL_WORKFLOW_COMPLETE.md
- **Status**: See LOAN_APPROVAL_WORKFLOW_STATUS.md
- **Database**: Check bfasdatabase.sql for schema

---

## Success Indicator 🎯

When you see this flow working end-to-end:
1. Customer submits → Admin approves → Loan active ✅
2. Customer submits → Admin rejects → Loan hidden ✅
3. Multiple loans processed independently ✅
4. Status timestamps recorded ✅

**YOU'VE SUCCESSFULLY IMPLEMENTED THE LOAN APPROVAL WORKFLOW!**

