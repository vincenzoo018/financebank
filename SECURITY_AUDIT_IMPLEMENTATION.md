# Security & Audit Logs Feature Implementation

## Overview
This document describes the comprehensive security feature for tracking malicious login attempts and customer account management.

## Features Implemented

### 1. Enhanced AuditLog Model
**File:** `Models/DatabaseModels.cs`

The AuditLog model has been updated with the following changes:
- **Removed:** `Amount`, `BalanceBefore`, `BalanceAfter`, `OldValues`, `NewValues` columns (simplified)
- **Added:**
  - `IsMalicious` (bool) - Flag for malicious attempts
  - `CustomerAccountId` (int?) - Link to customer account
  - `AccountStatus` (string) - Current account status (Active, Locked, Deactivated)

### 2. AuthUser Security Fields
**File:** `Models/AuthModels.cs`

Added security fields to track failed login attempts:
- `SecurityPinHash` - BCrypt-hashed 4-digit security PIN
- `FailedLoginAttempts` - Counter for failed attempts (resets on success)
- `LastFailedAttempt` - Timestamp of last failed attempt
- `IsLocked` - Account lock status
- `LockoutEnd` - When the lockout expires (24 hours by default)

### 3. Customer Login Security Flow
**File:** `Components/Pages/Login.razor`

When a customer enters incorrect password:
1. Failed attempts are counted (up to 5)
2. After 5 failed attempts:
   - Account is automatically locked
   - Logged as `MALICIOUS_ATTEMPT` in AuditLogs
   - PIN verification modal appears
3. Customer enters their 4-digit Security PIN
4. Customer sets a new password
5. Account is unlocked and customer can log in

### 4. Teller Withdrawal Security Flow
**File:** `Components/Pages/Teller/ProcessWithdrawals.razor`

When customer enters incorrect password during teller withdrawal:
1. Failed attempts are tracked (up to 5)
2. After 5 failed attempts:
   - Account is locked
   - Logged as `MALICIOUS_ATTEMPT` in AuditLogs
   - PIN verification modal appears
3. Customer enters their 4-digit Security PIN
4. Account is unlocked (no password reset)
5. Customer can re-enter their password to complete transaction

### 5. SuperAdmin Audit Logs Page
**File:** `Components/Pages/Admin/AuditTrail.razor`

Enhanced with 4 tabs:
1. **🚨 Malicious Attempts** - View all flagged malicious attempts
2. **🔐 Locked Accounts** - View/manage locked customer accounts
3. **🛡️ Security Logs** - All security-related events
4. **📋 All Audit Logs** - Complete audit trail with filters

SuperAdmin can:
- View malicious attempt details (user, IP, timestamp)
- Unlock locked accounts
- Deactivate suspicious accounts
- Export security reports

### 6. AuthService Security Methods
**File:** `Services/AuthService.cs`

New methods added:
- `VerifySecurityPinAsync()` - Verify 4-digit PIN
- `ResetPasswordWithPinAsync()` - Reset password after PIN verification
- `UnlockAccountWithPinAsync()` - Unlock account without password reset
- `SetSecurityPinAsync()` - Set/update security PIN
- `GetFailedLoginAttemptsAsync()` - Get failed attempt count
- `IsAccountLockedAsync()` - Check if account is locked
- `LogTellerTransactionFailedPasswordAsync()` - Log failed teller transaction password
- `ResetFailedAttemptsAsync()` - Reset counter after successful transaction

### 7. AuditLogService Methods
**File:** `Services/CrudServices.cs`

New methods:
- `GetMaliciousAttemptsAsync()` - Get all malicious attempts
- `GetSecurityLogsAsync()` - Get security-related logs
- `GetCustomerLogsAsync()` - Get customer-specific logs
- `GetRecentMaliciousAttemptsCountAsync()` - Count malicious attempts (24h)

## Database Migration
**File:** `Database/SECURITY_AUDIT_MIGRATION.sql`

Run this script to update the database schema:
```sql
-- Execute in SQL Server Management Studio
USE BFASdatabase;
GO

-- Run the migration script
EXEC sp_executesql N'path\to\SECURITY_AUDIT_MIGRATION.sql'
```

The migration adds:
- New columns to `AuditLogs` table
- Security columns to `Users` table
- Performance indexes for security queries

## Security PIN Setup

Customers need to set their 4-digit Security PIN in their profile settings. Without a PIN, they cannot recover from a locked account through self-service.

**Alternative:** SuperAdmin can manually unlock accounts from the Audit Logs page.

## Testing the Feature

### Test Customer Login Lockout:
1. Log in as a customer
2. Enter wrong password 5 times
3. PIN modal should appear
4. Enter 4-digit PIN and new password
5. Account should unlock

### Test Teller Withdrawal Lockout:
1. Log in as teller
2. Start a withdrawal transaction
3. Enter wrong customer password 5 times
4. PIN modal should appear
5. Customer enters PIN
6. Account unlocks (no password change)
7. Customer re-enters correct password

### Test SuperAdmin Audit Logs:
1. Log in as SuperAdmin
2. Navigate to /admin/audit-trail
3. View malicious attempts tab
4. Test unlock/deactivate buttons

## Security Considerations

1. **PIN Storage:** PINs are BCrypt-hashed (same as passwords)
2. **Lockout Duration:** 24 hours by default
3. **Audit Trail:** All security events are logged
4. **Admin Override:** SuperAdmin can unlock any account
5. **No Password Exposure:** Teller workflow doesn't allow password reset

## Routes

- `/admin/audit-trail` - Security & Audit Logs page
- `/admin/system/audit-logs` - Alternate route (same page)
- `/login` - Customer login with PIN recovery
- `/teller/withdrawals` - Teller withdrawal with PIN verification

## Files Modified

1. `Models/DatabaseModels.cs` - AuditLog model
2. `Models/AuthModels.cs` - AuthUser model
3. `Services/AuthService.cs` - Security methods
4. `Services/CrudServices.cs` - AuditLogService methods
5. `Components/Pages/Login.razor` - PIN modal
6. `Components/Pages/Teller/ProcessWithdrawals.razor` - PIN modal
7. `Components/Pages/Admin/AuditTrail.razor` - Admin UI
8. `Database/SECURITY_AUDIT_MIGRATION.sql` - Migration script (new)
