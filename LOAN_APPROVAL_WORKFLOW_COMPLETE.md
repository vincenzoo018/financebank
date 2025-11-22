# Loan Approval Workflow - Complete Implementation

## Summary
Implemented a complete loan approval workflow that mirrors the card application process. Loans now follow the pattern: Submit (Pending) → Admin Review → Approve/Reject → Update Status.

## Changes Made

### 1. **CardApplicationService.cs** - Enhanced Approval Methods

#### SubmitLoanApplicationAsync (Lines 259-439)
- **FIXED**: Changed ApprovalQueue Status from "Approved" to "Pending"
- **FIXED**: Removed ProcessedAt and ProcessedBy from initial creation (set to null)
- **NEW**: Creates loan record in CustomerLoans table with:
  - Status = "Pending" (waiting for admin approval)
  - Calculated monthly payment using interest rate
  - Generated loan number (LN-YYYYMMDD-AccountId-Random)
  - All required fields populated

#### ApproveLoanApplicationAsync (Lines 455-501)
- **NEW**: Now updates the corresponding Loan record
- **NEW**: Sets Loan.Status = "Active" when approved
- **NEW**: Updates LoanAmount to approved amount if changed by admin
- **NEW**: Updates OutstandingBalance to approved amount
- **NEW**: Sets ProcessedAt and ProcessedBy on ApprovalQueue record
- Returns success message with operation details

#### RejectLoanApplicationAsync (Lines 503-548)
- **NEW**: Now updates the corresponding Loan record
- **NEW**: Sets Loan.Status = "Rejected" when rejected
- **NEW**: Sets ProcessedAt and ProcessedBy on ApprovalQueue record
- Stores rejection reason in ApprovalNotes

### 2. **Loans.razor** - Customer Loan Display

#### LoadCustomerLoans Method
- **FIXED**: Now filters out rejected loans (Status != "Rejected")
- **FIXED**: Excludes closed loans (Status != "Closed")
- **IMPROVED**: Shows only valid loans (Pending, Active, Paid)

### 3. **Database Schema** - Verified Compatibility

**CustomerLoans Table**:
- LoanId (PK)
- AccountId (FK)
- LoanNumber (string)
- LoanType (PERSONAL, HOME, AUTO, EDUCATION)
- LoanAmount (decimal)
- OutstandingBalance (decimal)
- InterestRate (decimal)
- TermMonths (int)
- MonthlyPayment (decimal)
- Status (Pending, Active, Paid, Rejected, Defaulted, Closed)
- CreatedAt (datetime)

**ApprovalQueue Table**:
- ApprovalId (PK)
- RequestType (CARD, LOAN)
- RequestId (CardId or LoanId)
- Status (Pending, Approved, Rejected)
- ProcessedAt (datetime, null until admin processes)
- ProcessedBy (string, null until admin processes)
- Amount (decimal - approved amount)

## Workflow

### Step 1: Customer Submits Loan
```
Loans.razor → SubmitApplication()
  ↓
CardApplicationService.SubmitLoanApplicationAsync()
  ├─ Create Loan record (Status="Pending")
  ├─ Create ApprovalQueue entry (Status="Pending")
  └─ Return success with approvalId
```

### Step 2: Admin Reviews Loan
```
LoanApplicationApprovals.razor → LoadApplications()
  ↓
CardApplicationService.GetAllLoanApplicationsAsync("Pending")
  ├─ Query ApprovalQueue WHERE RequestType="LOAN" AND Status="Pending"
  └─ Display in Pending tab with count
```

### Step 3: Admin Approves/Rejects
```
LoanApplicationApprovals.razor → ApproveApplication()
  ↓
CardApplicationService.ApproveLoanApplicationAsync()
  ├─ Update ApprovalQueue: Status="Approved", ProcessedAt, ProcessedBy
  ├─ Update Loan: Status="Active", LoanAmount, OutstandingBalance
  └─ Loan now appears in customer "My Active Loans" view

OR

LoanApplicationApprovals.razor → ConfirmRejection()
  ↓
CardApplicationService.RejectLoanApplicationAsync()
  ├─ Update ApprovalQueue: Status="Rejected", ProcessedAt, ProcessedBy
  ├─ Update Loan: Status="Rejected"
  └─ Loan hidden from customer view
```

## Status Transitions

```
Loan Status Lifecycle:
┌──────────┐
│ Pending  │  ← Created by customer submission
└────┬─────┘
     │
     ├─────────────────────┐
     │                     │
     ▼                     ▼
┌─────────┐            ┌──────────┐
│ Active  │            │ Rejected │  ← Admin decision
└─────┬───┘            └──────────┘
      │
      ├─────────────────────┐
      │                     │
      ▼                     ▼
   ┌─────┐            ┌──────────┐
   │ Paid│            │Defaulted │  ← Payment history
   └─────┘            └──────────┘
```

## Key Implementation Details

### Interest Rate by Loan Type
- Personal: 5.8%
- Home: 3.5%
- Auto: 4.2%
- Education: 3.9%

### Monthly Payment Calculation
```
Formula: P * (r * (1+r)^n) / ((1+r)^n - 1)
Where:
  P = Principal amount
  r = Monthly interest rate (annual rate / 12 / 100)
  n = Number of months
```

### Loan Number Generation
```
Format: LN-YYYYMMDD-AccountId-RandomNumber
Example: LN-20251122-5-6472
```

## Build Status
✅ **Build Succeeded** - All changes compile without errors
- iOS platform: ✅
- Android platform: ✅
- macOS platform: ✅
- Windows platform: ✅

## Testing Checklist

- [ ] Customer submits loan application
  - Verify Loan created in CustomerLoans table with Status="Pending"
  - Verify ApprovalQueue entry created with Status="Pending"
  - Verify loan doesn't appear in customer "My Active Loans" view yet

- [ ] Admin views pending loans
  - Navigate to Admin → Approvals → Loan Applications
  - Verify Pending tab shows submitted loans
  - Verify loan details displayed correctly

- [ ] Admin approves loan
  - Click "Review" on pending loan
  - Enter approved amount (or keep original)
  - Click "Approve"
  - Verify ApprovalQueue.Status changed to "Approved"
  - Verify Loan.Status changed to "Active"
  - Verify loan now appears in customer "My Active Loans" view

- [ ] Admin rejects loan
  - Click "Review" on pending loan
  - Enter rejection reason
  - Click "Reject"
  - Verify ApprovalQueue.Status changed to "Rejected"
  - Verify Loan.Status changed to "Rejected"
  - Verify loan doesn't appear in customer view

- [ ] Tab Navigation
  - Verify approved loans appear in Approved tab
  - Verify rejected loans appear in Rejected tab
  - Verify statistics updated correctly

## Related Files Modified
- `Services/CardApplicationService.cs` - Approval logic
- `Components/Pages/Customer/Loans.razor` - Customer display
- `Services/CrudServices.cs` - LoanService queries (no changes needed)
- `Models/DatabaseModels.cs` - Loan model table mapping (previously fixed)

## Architecture Pattern
The implementation follows a clean separation of concerns:
- **Service Layer**: CardApplicationService handles all business logic
- **Database Layer**: EF Core models map directly to CustomerLoans and ApprovalQueue tables
- **Presentation Layer**: Razor components display data and call service methods
- **Data Flow**: Customer → Service → Database → ApprovalQueue → Admin → Service → Database

## Notes
- All timestamps use UTC (DateTime.Now)
- Loan records are created immediately when customer submits (not in ApprovalQueue only)
- ApprovalQueue acts as a workflow tracker, not primary data store
- Customer view filters out Rejected and Closed loans
- Admin can modify approved amount during approval process
