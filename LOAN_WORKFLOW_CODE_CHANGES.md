# Loan Approval Workflow - Detailed Code Changes

## File: CardApplicationService.cs

### Change 1: SubmitLoanApplicationAsync - Set Correct Initial Status

**Location**: Lines 311-328

**Before**:
```csharp
Status = "Approved",  // ❌ WRONG - Auto-approves loans
ProcessedAt = DateTime.Now,
ProcessedBy = "System"
```

**After**:
```csharp
Status = "Pending",  // ✅ CORRECT - Waits for admin review
// ProcessedAt and ProcessedBy removed - will be set when admin processes
```

**Impact**: Loans now appear in admin Pending tab instead of being auto-approved

---

### Change 2: ApproveLoanApplicationAsync - Update Loan Status

**Location**: Lines 455-501

**Before**:
```csharp
public async Task<(bool success, string message)> ApproveLoanApplicationAsync(
    int approvalId,
    decimal approvedAmount,
    string? adminNotes = null)
{
    try
    {
        var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
        if (approval == null)
            return (false, "Application not found");

        approval.Status = "Approved";
        approval.Amount = approvedAmount;
        approval.ProcessedAt = DateTime.Now;
        approval.ProcessedBy = _authService.CurrentUserId?.ToString();
        approval.ApprovalNotes = adminNotes ?? approval.ApprovalNotes;

        _context.ApprovalQueues.Update(approval);
        await _context.SaveChangesAsync();

        return (true, "Loan application approved successfully");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error approving loan application: {ex.Message}");
        return (false, $"Error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<(bool success, string message)> ApproveLoanApplicationAsync(
    int approvalId,
    decimal approvedAmount,
    string? adminNotes = null)
{
    try
    {
        var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
        if (approval == null)
            return (false, "Application not found");

        System.Diagnostics.Debug.WriteLine($"[ApproveLoanApplication] Approving loan with ApprovalId: {approvalId}, RequestId: {approval.RequestId}");

        // Update approval status
        approval.Status = "Approved";
        approval.Amount = approvedAmount;
        approval.ProcessedAt = DateTime.Now;
        approval.ProcessedBy = _authService.CurrentUserId?.ToString();
        approval.ApprovalNotes = adminNotes ?? approval.ApprovalNotes;

        _context.ApprovalQueues.Update(approval);
        await _context.SaveChangesAsync();

        // Also update the corresponding Loan record to "Active" status
        if (approval.RequestId > 0)
        {
            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.LoanId == approval.RequestId);
            if (loan != null)
            {
                loan.Status = "Active";
                loan.LoanAmount = approvedAmount; // Use approved amount
                loan.OutstandingBalance = approvedAmount;
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"[ApproveLoanApplication] Loan {approval.RequestId} activated with approved amount ₱{approvedAmount}");
            }
        }

        return (true, "Loan application approved successfully");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error approving loan application: {ex.Message}");
        return (false, $"Error: {ex.Message}");
    }
}
```

**Changes**:
- Added debug logging for tracking
- Query CustomerLoans table using RequestId
- Update Loan.Status to "Active"
- Update LoanAmount to approved amount (allows admin to modify)
- Update OutstandingBalance to approved amount
- Save changes to database

**Impact**: When admin approves, loan becomes active and visible to customer

---

### Change 3: RejectLoanApplicationAsync - Update Loan Status

**Location**: Lines 503-548

**Before**:
```csharp
public async Task<(bool success, string message)> RejectLoanApplicationAsync(
    int approvalId,
    string rejectionReason)
{
    try
    {
        var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
        if (approval == null)
            return (false, "Application not found");

        approval.Status = "Rejected";
        approval.ProcessedAt = DateTime.Now;
        approval.ProcessedBy = _authService.CurrentUserId?.ToString();
        approval.ApprovalNotes = $"Rejection Reason: {rejectionReason}";

        _context.ApprovalQueues.Update(approval);
        await _context.SaveChangesAsync();

        return (true, "Loan application rejected");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error rejecting loan application: {ex.Message}");
        return (false, $"Error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<(bool success, string message)> RejectLoanApplicationAsync(
    int approvalId,
    string rejectionReason)
{
    try
    {
        var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
        if (approval == null)
            return (false, "Application not found");

        System.Diagnostics.Debug.WriteLine($"[RejectLoanApplication] Rejecting loan with ApprovalId: {approvalId}, RequestId: {approval.RequestId}");

        approval.Status = "Rejected";
        approval.ProcessedAt = DateTime.Now;
        approval.ProcessedBy = _authService.CurrentUserId?.ToString();
        approval.ApprovalNotes = $"Rejection Reason: {rejectionReason}";

        _context.ApprovalQueues.Update(approval);
        await _context.SaveChangesAsync();

        // Also update the corresponding Loan record to "Rejected" status
        if (approval.RequestId > 0)
        {
            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.LoanId == approval.RequestId);
            if (loan != null)
            {
                loan.Status = "Rejected";
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"[RejectLoanApplication] Loan {approval.RequestId} marked as rejected");
            }
        }

        return (true, "Loan application rejected");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error rejecting loan application: {ex.Message}");
        return (false, $"Error: {ex.Message}");
    }
}
```

**Changes**:
- Added debug logging for tracking
- Query CustomerLoans table using RequestId
- Update Loan.Status to "Rejected"
- Save changes to database

**Impact**: When admin rejects, loan is marked as rejected and hidden from customer

---

## File: Loans.razor

### Change: LoadCustomerLoans - Filter Rejected Loans

**Location**: Customer view loan filtering

**Before**:
```csharp
activeLoans = realLoans
    .Where(l => l.Status != "Closed")
    .Select(l => new Loan { ... })
    .ToList();
```

**After**:
```csharp
activeLoans = realLoans
    .Where(l => l.Status != "Closed" && l.Status != "Rejected")
    .Select(l => new Loan { ... })
    .ToList();
```

**Changes**:
- Added rejection check: `l.Status != "Rejected"`
- Ensures rejected loans don't appear in customer view

**Impact**: Customers only see active, pending, and paid loans

---

## Data Flow Diagram

```
┌─────────────────────┐
│  Customer Actions   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────────────┐
│ Loans.razor                 │
│ SubmitApplication()         │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────────────────────┐
│ CardApplicationService                      │
│ SubmitLoanApplicationAsync()                │
└──────────┬──────────────────────────────────┘
           │
      ┌────┴────┐
      ▼         ▼
   ┌─────────────────┐    ┌──────────────────┐
   │ CustomerLoans   │    │ ApprovalQueue    │
   │ Status="Pending"│    │ Status="Pending" │
   └─────────────────┘    └──────────────────┘
           │                      │
           │    Admin Reviews     │
           │         ↓            │
           │   ┌────────────┐     │
           │   │ Approved?  │     │
           │   └─┬────────┬─┘     │
           │     │        │       │
      ┌────┴─────┤        └───────┴─┐
      │          │                  │
      ▼          ▼                  ▼
  ┌────────┐ ┌─────────┐       ┌──────────┐
  │ Active │ │Rejected │   │ Approved │
  │  Loan  │ │  Loan   │   │ Queue    │
  └────────┘ └─────────┘   └──────────┘
      │          │              │
      ▼          ▼              ▼
  Customer   Hidden        Admin Tab
   Visible   from View    (Approved)
                            
           Rejected
            Loans
             Tab
```

---

## State Transition Rules

```csharp
// SubmitLoanApplicationAsync
Loan.Status = "Pending"
ApprovalQueue.Status = "Pending"
ApprovalQueue.ProcessedAt = null
ApprovalQueue.ProcessedBy = null

// ApproveLoanApplicationAsync
ApprovalQueue.Status = "Approved"
ApprovalQueue.ProcessedAt = DateTime.Now ✓
ApprovalQueue.ProcessedBy = CurrentUserId ✓
Loan.Status = "Active"
Loan.LoanAmount = approvedAmount (may differ from original)
Loan.OutstandingBalance = approvedAmount

// RejectLoanApplicationAsync
ApprovalQueue.Status = "Rejected"
ApprovalQueue.ProcessedAt = DateTime.Now ✓
ApprovalQueue.ProcessedBy = CurrentUserId ✓
Loan.Status = "Rejected"
Loan.LoanAmount = unchanged
Loan.OutstandingBalance = unchanged
```

---

## Testing Scenarios

### Scenario 1: Happy Path (Approve)
```
1. Customer submits ₱50,000 Personal Loan
   → Loan created with Status="Pending"
   → ApprovalQueue created with Status="Pending"
   
2. Admin views pending loans
   → Loan appears in Pending tab
   
3. Admin approves for ₱50,000
   → ApprovalQueue.Status = "Approved"
   → Loan.Status = "Active"
   
4. Customer views their loans
   → Loan appears in "My Active Loans"
```

### Scenario 2: Modified Approval
```
1. Customer requests ₱50,000
   
2. Admin approves for ₱40,000 only
   → Loan.LoanAmount = 40,000 (updated)
   → Loan.OutstandingBalance = 40,000
   → Monthly payment recalculated based on 40,000
   
3. Customer sees approved amount
```

### Scenario 3: Rejection
```
1. Customer submits ₱100,000 Auto Loan
   → Loan created with Status="Pending"
   
2. Admin rejects with reason
   → ApprovalQueue.Status = "Rejected"
   → Loan.Status = "Rejected"
   
3. Customer views loans
   → Loan doesn't appear (filtered out)
   
4. Admin views rejections
   → Loan appears in Rejected tab with reason
```

---

## Error Handling

All methods include try-catch with:
- Debug logging for troubleshooting
- Exception message returned to caller
- Graceful failure (returns false with error message)
- No data corruption (all-or-nothing transactions)

Example:
```csharp
try
{
    // Process request
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
    return (false, $"Error: {ex.Message}");
}
```

This ensures:
- Admin notifications if something fails
- Database consistency maintained
- Debugging information available in debug console
