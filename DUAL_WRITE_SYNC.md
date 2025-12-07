# 🔄 DUAL-WRITE DATABASE SYNCHRONIZATION

## Overview
The FinanceBank application now features **DUAL-WRITE** functionality that automatically saves all database changes to **BOTH** LOCAL and CLOUD databases simultaneously.

## How It Works

### Automatic Dual-Write (Default: ENABLED)
When dual-write is enabled (default), every database operation (INSERT, UPDATE, DELETE) is automatically:
1. **First** - Saved to the LOCAL database (SQL Server Express)
2. **Then** - Replicated to the CLOUD database (MonsterASP)

This happens transparently in the background and does not slow down the user interface.

### Connection Details
- **LOCAL Database**: `localhost\SQLEXPRESS` → Database: `BFASdatabase`
- **CLOUD Database**: `db34283.public.databaseasp.net,1433` → Database: `db34283`

## Features

### 1. Automatic Replication
- All Entity Framework SaveChanges() calls automatically replicate to cloud
- Works for ALL tables and ALL operations (INSERT, UPDATE, DELETE)
- Fire-and-forget: doesn't block the user interface

### 2. Smart Cloud Detection
- Caches cloud availability for 30 seconds to avoid repeated connection attempts
- When cloud is unavailable, saves to LOCAL only (no data loss)
- Automatically resumes cloud replication when connection is restored

### 3. Identity Column Handling
- Properly handles IDENTITY columns
- Uses `SET IDENTITY_INSERT ON/OFF` for cloud inserts
- Ensures same IDs in both databases

### 4. Toggle Control
- Can be enabled/disabled from the Database Sync page
- Status: Admin → Database Sync → Dual-Write toggle

## Database Sync Page Location
Navigate to: **Admin Dashboard → Database Sync**

The page shows:
- **Dual-Write Status Banner** - Shows if dual-write is enabled/disabled
- **Cloud Connection Status** - Shows if cloud is reachable
- **Toggle Button** - Enable/disable dual-write

## Code Location

### Main Implementation
`Data/BFASDbContext.cs`:
- `SaveChangesAsync()` - Overridden to capture changes and replicate
- `SaveChanges()` - Overridden to capture changes and replicate
- `CapturePendingChanges()` - Captures all pending Entity Framework changes
- `ReplicateToCloudAsync()` - Sends changes to cloud database
- `IsCloudAvailableAsync()` - Checks cloud connectivity (cached)

### Configuration
```csharp
// Enable/disable dual-write globally
BFASDbContext.DualWriteEnabled = true;  // Default: enabled
```

## What Gets Synced

ALL changes through Entity Framework are synced, including:

### User Management
- Users (authentication records)
- Employees
- Login History
- User Sessions

### Customer Data
- Customer Accounts
- Customer Transactions (deposits, withdrawals, transfers, bills)
- Invoices

### Loan Management
- Loan Applications
- Loan Assessments
- Loan Approvals
- Loan Disbursals
- Loan Payments
- Loans

### Finance & Accounting
- Journal Entries
- Journal Entry Lines
- Budget Management
- General Ledger Entries

### System
- Audit Logs
- Cards
- Savings Goals
- And more...

## Offline Behavior

When the cloud database is unreachable:
1. Data is saved to LOCAL database immediately
2. No error is shown to user (graceful degradation)
3. Debug log shows: `"☁️ DUAL-WRITE: Cloud unavailable - data saved to LOCAL only"`
4. Use manual "Sync to Cloud" button later to catch up

## Manual Sync (Backup Option)

If dual-write misses some records (cloud was down), you can still use:
- **Admin → Database Sync → Sync to Cloud** button
- This does a full comparison and syncs all missing records

## Debug Logging

The system logs dual-write activity:
```
✅ DUAL-WRITE: Replicated 3 changes to CLOUD
⚠️ DUAL-WRITE: Failed INSERT on TableName: [error message]
☁️ DUAL-WRITE: Cloud unavailable - data saved to LOCAL only
```

## Performance

- Cloud replication runs asynchronously (fire-and-forget)
- User operations are NOT blocked waiting for cloud
- Cloud connection check is cached for 30 seconds
- Metadata (table names, columns) is cached for performance

## Best Practices

1. **Keep dual-write enabled** for real-time data safety
2. **Check sync status** periodically from Database Sync page
3. **Use manual sync** if you notice discrepancies
4. **Monitor debug logs** during development

## Created
Date: $(Get-Date)
Feature: Dual-Write Database Synchronization
