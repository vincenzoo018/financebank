# Account Unlock Request Feature Implementation

## Overview
This feature implements a complete workflow for handling locked customer accounts. When a customer's account gets locked (e.g., due to multiple failed PIN attempts), they can visit the bank where a Teller creates an unlock request that goes to the Admin for approval.

## Workflow Process

### 1. Customer Account Gets Locked
- Account can be locked due to multiple failed PIN attempts
- The system sets `IsLocked = true` and records the `LockReason`
- Customer sees a message: "Account Fully Locked - Please contact SuperAdmin"

### 2. Customer Visits the Bank
- Customer goes to the bank branch
- Explains their situation to the Teller

### 3. Teller Creates Unlock Request
- **Location**: Teller Portal → Account Management → Account Unlock Requests
- **URL**: `/teller/account-unlock-requests`
- Teller searches for the locked customer account
- Verifies customer's identity using a government-issued ID
- Creates an unlock request with:
  - Customer information (auto-filled)
  - Lock reason (auto-filled from system)
  - ID verification status and type
  - Priority level (Low, Normal, High, Urgent)
  - Customer's statement/explanation
  - Teller notes

### 4. Admin Reviews and Processes Request
- **Location**: Admin Portal → User Management → Account Unlock Requests
- **URL**: `/admin/account-unlock-requests`
- Admin sees all pending unlock requests
- Can filter by status and priority
- Urgent requests are highlighted
- Admin can:
  - **Approve**: Unlocks the account, resets failed attempts
  - **Reject**: Keeps account locked, provides rejection reason

### 5. Direct Unlock from All Users (Alternative)
- Admin can also unlock accounts directly from All Users page
- **URL**: `/admin/all-users`
- Filter by "Locked" status to see all locked accounts
- Click "🔓 Unlock" button to unlock directly

---

## Database Changes

### New Table: AccountUnlockRequests
```sql
CREATE TABLE AccountUnlockRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerUsername NVARCHAR(100) NOT NULL,
    CustomerEmail NVARCHAR(200) NULL,
    CustomerPhone NVARCHAR(50) NULL,
    LockReason NVARCHAR(500) NOT NULL,
    CustomerStatement NVARCHAR(1000) NULL,
    IdentificationVerified BIT NOT NULL DEFAULT 0,
    IdentificationType NVARCHAR(100) NULL,
    IdentificationNumber NVARCHAR(100) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    RequestedByTellerId INT NOT NULL,
    RequestedByTellerName NVARCHAR(200) NOT NULL,
    RequestedAt DATETIME NOT NULL DEFAULT GETDATE(),
    TellerNotes NVARCHAR(1000) NULL,
    ProcessedByAdminId INT NULL,
    ProcessedByAdminName NVARCHAR(200) NULL,
    ProcessedAt DATETIME NULL,
    AdminNotes NVARCHAR(1000) NULL,
    RejectionReason NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
```

### New Columns in Users Table
```sql
ALTER TABLE Users ADD IsLocked BIT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD LockReason NVARCHAR(500) NULL;
ALTER TABLE Users ADD LockedAt DATETIME NULL;
ALTER TABLE Users ADD FailedPinAttempts INT NOT NULL DEFAULT 0;
```

---

## Files Created/Modified

### New Files
1. `Components/Pages/Teller/AccountUnlockRequests.razor` - Teller page for creating unlock requests
2. `Components/Pages/Admin/AccountUnlockRequests.razor` - Admin page for processing unlock requests
3. `ACCOUNT_UNLOCK_REQUEST.sql` - Database schema script

### Modified Files
1. `Models/AuthModels.cs` - Added `AccountUnlockRequest` model and lock-related fields to `AuthUser`
2. `Data/BFASDbContext.cs` - Added `DbSet<AccountUnlockRequest>`
3. `Components/Layout/TellerLayout.razor` - Added sidebar link
4. `Components/Layout/AdminLayout.razor` - Added sidebar link
5. `Components/Pages/Admin/AllUsers.razor` - Added locked status display and unlock functionality

---

## UI Features

### Teller Page Features
- Summary cards showing request counts by status
- Search and filter functionality
- Create new unlock request wizard:
  - Step 1: Search for locked customer
  - Step 2: View customer account information
  - Step 3: Verification and request details
- View submitted requests with status tracking

### Admin Page Features
- Summary cards with quick filters (click to filter)
- Urgent requests highlighted
- Process requests with Approve/Reject decisions
- View detailed request information
- Add admin notes

### All Users Page Enhancements
- New "Locked" status filter
- Locked user count in summary cards
- Visual indicator for locked accounts
- Quick unlock button for locked users

---

## To Run the SQL Script
Execute `ACCOUNT_UNLOCK_REQUEST.sql` on your database to create the necessary table and add columns to the Users table.

---

## Status Values
- **Pending**: Request submitted, awaiting admin review
- **Approved**: Admin approved, account unlocked
- **Rejected**: Admin rejected, account remains locked

## Priority Levels
- **Low**: Can wait for normal processing
- **Normal**: Standard processing time
- **High**: Needs attention today
- **Urgent**: Immediate attention required

---

## Security Considerations
- Teller must verify customer's identity before creating request
- Admin must review before unlocking
- All actions are logged with timestamps and user information
- Lock reason is preserved for audit purposes
