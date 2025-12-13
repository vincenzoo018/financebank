# Document Upload & Loan Fixes - Complete

## Summary of Changes

This update fixes the document checkboxes not saving, adds document upload functionality, fixes the Accountant modal scrolling, and fixes the PDF contract download for MAUI WebView.

---

## 1. Database Schema Updates

### Run this SQL script first:
**File:** `ADD_LOAN_DOCUMENT_COLUMNS.sql`

This script adds:
- Document submission flags: `BankStatementSubmitted`, `PayslipsSubmitted`, `ITRSubmitted`, `COESubmitted`, `ValidIdSubmitted`, `ProofOfBillingSubmitted`
- Legal acknowledgment flags: `AcknowledgedTerms`, `AcknowledgedPenaltyTerms`, `AcknowledgedLegalAction`, `AcknowledgedInfoAccuracy`
- Additional loan fields: `LoanCategory`, `HasCollateral`, `HasCoBorrower`, `TellerReviewedBy`, etc.
- **New table:** `LoanDocuments` for storing uploaded document files

---

## 2. Root Cause Fix - Document Flags Not Saving

### Problem:
All document submission flags and legal acknowledgments were configured with `entity.Ignore()` in `BFASDbContext.cs`, preventing them from being saved to/loaded from the database.

### Solution:
Removed the `Ignore()` calls in [Data/BFASDbContext.cs](Data/BFASDbContext.cs#L600-L605) so the fields are now properly persisted.

---

## 3. New Features

### 3.1 Document Upload in Teller LoanReview

**File:** [Components/Pages/Teller/LoanReview.razor](Components/Pages/Teller/LoanReview.razor)

- Added document upload section after Document Submission Status
- Teller can upload scanned copies of documents the customer checked during application
- Supports: Bank Statements, Payslips, ITR, COE, Valid ID, Proof of Billing
- Files stored in `LoanDocuments` table with verification status

### 3.2 Document Viewer in Accountant AllApprovals

**File:** [Components/Pages/Accountant/AllApprovals.razor](Components/Pages/Accountant/AllApprovals.razor)

- Added uploaded documents section in the review modal (Step 1)
- Accountant can view uploaded documents (images displayed inline, PDFs as download)
- Accountant can verify or reject each document
- Document verification status tracked in database

### 3.3 Fixed Modal Scrolling

**File:** [Components/Pages/Accountant/AllApprovals.razor](Components/Pages/Accountant/AllApprovals.razor#L307-L312)

- Added `max-height: 90vh` and `overflow-y: auto` to modal container
- Modal now scrolls properly even with lots of content

---

## 4. PDF Contract Download Fix (MAUI Compatible)

### Problem:
The PDF contract used `window.open()` popup which is blocked in MAUI WebView.

### Solution:

**File:** [wwwroot/js/app.js](wwwroot/js/app.js)

Added new MAUI-compatible functions:
- `downloadHtmlAsFile()` - Downloads contract as HTML file
- `printContractInline()` - Uses hidden iframe for printing (works in MAUI)

**File:** [Components/Pages/Teller/LoanRelease.razor](Components/Pages/Teller/LoanRelease.razor)

- Replaced popup-based contract with in-page modal
- Contract displayed in iframe within the app
- Buttons for "Print / Save as PDF" and "Download HTML"

---

## 5. New Model Added

### LoanDocument Model

**File:** [Models/LoanProcessModels.cs](Models/LoanProcessModels.cs)

```csharp
public class LoanDocument
{
    public int DocumentId { get; set; }
    public int ApplicationId { get; set; }
    public string DocumentType { get; set; }  // BankStatement, Payslip, ITR, COE, ValidID, ProofOfBilling
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileData { get; set; }
    public long? FileSize { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string VerificationStatus { get; set; }  // PENDING, VERIFIED, REJECTED
    public string VerificationNotes { get; set; }
}
```

---

## 6. Files Modified

| File | Changes |
|------|---------|
| `Data/BFASDbContext.cs` | Removed Ignore() for document/acknowledgment fields, added LoanDocuments DbSet |
| `Models/LoanProcessModels.cs` | Added LoanDocument model, added Documents_Files navigation property |
| `Components/Pages/Teller/LoanReview.razor` | Added document upload section with InputFile |
| `Components/Pages/Accountant/AllApprovals.razor` | Added document viewer, fixed modal scrolling |
| `Components/Pages/Teller/LoanRelease.razor` | Added contract preview modal instead of popup |
| `wwwroot/js/app.js` | Added MAUI-compatible download/print functions |
| `ADD_LOAN_DOCUMENT_COLUMNS.sql` | New SQL script for database columns |

---

## How to Test

1. **Run the SQL script** `ADD_LOAN_DOCUMENT_COLUMNS.sql` against your database
2. **Restart the application**
3. **Test Document Checkboxes:**
   - Create a loan application with document checkboxes checked
   - Go to Teller LoanReview - verify checkboxes show ✅ for checked items
   - Upload documents for each checked document type
   - Forward to Accountant
4. **Test Accountant Document Review:**
   - Login as Accountant
   - Open an application in All Approvals
   - View uploaded documents
   - Verify or reject documents
5. **Test PDF Contract:**
   - Go to Loan Release
   - Click "Download PDF" for an approved loan
   - Contract should appear in modal
   - Use Print button to save as PDF

---

## Build Status

✅ Build succeeded with 0 errors
