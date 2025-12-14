# SaaS Management Portal - Complete Flow Guide

## 📋 System Overview

The SaaS Management Portal is a **separate system** for the ERP vendor/owner to manage client subscriptions, modules, payments, and support. It consists of two portals:

1. **Owner Portal** (`/owner/`) - For ERP system owner/admin
2. **Client Portal** (`/client-portal/`) - For client companies to manage their subscription

---

## 🔐 Login Flow

### Option 1: Owner Login (ERP Vendor/Admin)
```
┌─────────────────────────────────────────────────────────────────┐
│                      OWNER LOGIN                                 │
├─────────────────────────────────────────────────────────────────┤
│  URL: /owner/login (needs to be created)                        │
│  Default Credentials:                                            │
│    Username: admin                                               │
│    Password: admin123 (should be changed)                        │
│                                                                  │
│  After Login → Redirects to /owner/dashboard                    │
└─────────────────────────────────────────────────────────────────┘
```

### Option 2: Client Login (Subscriber Company)
```
┌─────────────────────────────────────────────────────────────────┐
│                      CLIENT LOGIN                                │
├─────────────────────────────────────────────────────────────────┤
│  URL: /client-portal/login (needs to be created)                │
│  Credentials provided by Owner after registration               │
│                                                                  │
│  After Login → Redirects to /client-portal/dashboard            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Complete System Flow Chart

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           SAAS MANAGEMENT PORTAL                                 │
│                              COMPLETE FLOW                                       │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    │                                   │
                    ▼                                   ▼
        ┌───────────────────┐               ┌───────────────────┐
        │   OWNER PORTAL    │               │   CLIENT PORTAL   │
        │   /owner/*        │               │   /client-portal/*│
        └───────────────────┘               └───────────────────┘
                    │                                   │
    ┌───────────────┼───────────────┐                   │
    │               │               │                   │
    ▼               ▼               ▼                   ▼
┌────────┐    ┌──────────┐    ┌──────────┐    ┌──────────────────┐
│Dashboard│    │ Clients  │    │Invoices  │    │Client Dashboard  │
│/owner/ │    │Management│    │Management│    │                  │
│dashboard│    │          │    │          │    │  - View Status   │
└────────┘    └──────────┘    └──────────┘    │  - View Modules  │
    │               │               │          │  - View Invoices │
    │               │               │          └──────────────────┘
    │               ▼               │                   │
    │         ┌──────────┐          │                   ▼
    │         │Add Client│          │          ┌──────────────────┐
    │         │          │          │          │   Make Payment   │
    │         │ - Company│          │          │                  │
    │         │   Info   │          │          │ - Select Invoice │
    │         │ - Plan   │          │          │ - Choose Method  │
    │         │ - Modules│          │          │ - Upload Proof   │
    │         └──────────┘          │          │ - Submit         │
    │               │               │          └──────────────────┘
    │               ▼               │                   │
    │         ┌──────────┐          │                   │
    │         │ Assign   │          │                   │
    │         │ License  │          │                   │
    │         │ Key      │          │                   │
    │         └──────────┘          │                   │
    │               │               │                   │
    └───────────────┼───────────────┘                   │
                    │                                   │
                    ▼                                   │
        ┌───────────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────────────────────┐
│                    TRANSACTION PROCESSING                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Client submits payment (via Client Portal)                  │
│     └── Status: "Pending"                                       │
│                                                                  │
│  2. Owner reviews payment (via Owner Portal)                    │
│     └── Checks proof of payment                                 │
│                                                                  │
│  3. Owner approves/rejects payment                              │
│     ├── Approve: Status → "Completed"                           │
│     │   └── Invoice marked as "Paid"                            │
│     │   └── Client balance updated                              │
│     │                                                            │
│     └── Reject: Status → "Failed"                               │
│         └── Client notified                                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Step-by-Step Workflow

### PHASE 1: Initial Setup (Owner)

```
┌─────────────────────────────────────────────────────────────────┐
│ STEP 1: Owner sets up the system                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1.1 Configure Modules (/owner/modules)                         │
│      ┌────────────────────────────────────────┐                 │
│      │ Add/Edit modules:                      │                 │
│      │ - Module Name (e.g., "Loan Management")│                 │
│      │ - Monthly Price (e.g., ₱1,000)         │                 │
│      │ - Yearly Price (e.g., ₱10,000)         │                 │
│      │ - Category (Finance, HR, etc.)         │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  1.2 Create Subscription Plans (/owner/plans)                   │
│      ┌────────────────────────────────────────┐                 │
│      │ Create plans like:                     │                 │
│      │ - Basic (₱2,999/month)                 │                 │
│      │ - Pro (₱5,999/month)                   │                 │
│      │ - Enterprise (₱9,999/month)            │                 │
│      │                                        │                 │
│      │ Assign modules to each plan            │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  1.3 Configure Payment Methods (/owner/settings)                │
│      ┌────────────────────────────────────────┐                 │
│      │ Add payment options:                   │                 │
│      │ - GCash (09XX-XXX-XXXX)               │                 │
│      │ - PayMaya (09XX-XXX-XXXX)             │                 │
│      │ - BDO Bank Transfer                    │                 │
│      │ - Cash Payment                         │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### PHASE 2: Client Onboarding (Owner)

```
┌─────────────────────────────────────────────────────────────────┐
│ STEP 2: Register a new client                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Navigate to: /owner/clients → Click "Add Client"               │
│                                                                  │
│  2.1 Enter Company Information                                  │
│      ┌────────────────────────────────────────┐                 │
│      │ Company Name: ABC Cooperative          │                 │
│      │ Email: abc@cooperative.com             │                 │
│      │ Phone: 02-1234-5678                    │                 │
│      │ Address: Manila, Philippines           │                 │
│      │ Contact Person: Juan Dela Cruz         │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  2.2 Assign Subscription Plan                                   │
│      ┌────────────────────────────────────────┐                 │
│      │ Select Plan: Professional (₱5,999/mo)  │                 │
│      │ Billing Cycle: Monthly / Yearly        │                 │
│      │ Start Date: December 15, 2024          │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  2.3 Select Modules (or use plan defaults)                      │
│      ┌────────────────────────────────────────┐                 │
│      │ ☑ Core System (Included)               │                 │
│      │ ☑ Teller Operations (₱500/mo)          │                 │
│      │ ☑ Loan Management (₱1,000/mo)          │                 │
│      │ ☑ General Ledger (₱800/mo)             │                 │
│      │ ☐ Audit Trail (₱400/mo) - Optional     │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  2.4 Generate License Key                                       │
│      ┌────────────────────────────────────────┐                 │
│      │ License Key: ABC-COOP-2024-XXXX-XXXX   │                 │
│      │ Valid Until: January 15, 2025          │                 │
│      │                                        │                 │
│      │ This key is used by the main ERP       │                 │
│      │ system to validate module access       │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  2.5 Send Welcome Email with Credentials                        │
│      ┌────────────────────────────────────────┐                 │
│      │ To: abc@cooperative.com                │                 │
│      │ Subject: Welcome to ERP System         │                 │
│      │                                        │                 │
│      │ Portal URL: https://portal.erp.com    │                 │
│      │ Username: abc_admin                    │                 │
│      │ Password: [temporary]                  │                 │
│      │ License Key: ABC-COOP-2024-XXXX-XXXX  │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### PHASE 3: Billing Cycle (Monthly)

```
┌─────────────────────────────────────────────────────────────────┐
│ STEP 3: Generate Invoice                                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Navigate to: /owner/invoices → Click "Create Invoice"          │
│                                                                  │
│  3.1 Select Client                                              │
│      ┌────────────────────────────────────────┐                 │
│      │ Client: ABC Cooperative                │                 │
│      │ Subscription: Professional Plan        │                 │
│      │ Billing Period: Dec 1 - Dec 31, 2024   │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  3.2 Invoice Line Items                                         │
│      ┌────────────────────────────────────────────────────────┐ │
│      │ Item                          Qty    Price     Amount  │ │
│      │ ─────────────────────────────────────────────────────  │ │
│      │ Professional Plan - Monthly    1    ₱5,999    ₱5,999  │ │
│      │ Additional Module: Audit       1    ₱400      ₱400    │ │
│      │ ─────────────────────────────────────────────────────  │ │
│      │ Subtotal:                                    ₱6,399    │ │
│      │ Tax (12% VAT):                               ₱767.88  │ │
│      │ ─────────────────────────────────────────────────────  │ │
│      │ TOTAL DUE:                                   ₱7,166.88 │ │
│      └────────────────────────────────────────────────────────┘ │
│                                                                  │
│  3.3 Send Invoice to Client                                     │
│      Click "Send Invoice" → Status changes to "Sent"            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### PHASE 4: Client Payment (Client Portal)

```
┌─────────────────────────────────────────────────────────────────┐
│ STEP 4: Client makes payment                                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Client logs in to: /client-portal/dashboard                    │
│                                                                  │
│  4.1 View Invoice                                               │
│      ┌────────────────────────────────────────┐                 │
│      │ Invoice #: INV-2024-0001               │                 │
│      │ Amount Due: ₱7,166.88                  │                 │
│      │ Due Date: December 15, 2024            │                 │
│      │ Status: Sent                           │                 │
│      │                                        │                 │
│      │ [Pay Now] button                       │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  4.2 Navigate to: /client-portal/make-payment                   │
│      ┌────────────────────────────────────────┐                 │
│      │ Select Invoice: INV-2024-0001          │                 │
│      │ Amount: ₱7,166.88                      │                 │
│      │                                        │                 │
│      │ Payment Method:                        │                 │
│      │ ○ GCash                                │                 │
│      │ ● BDO Bank Transfer                    │                 │
│      │ ○ PayMaya                              │                 │
│      │                                        │                 │
│      │ Reference Number: [1234567890]         │                 │
│      │ Upload Proof: [deposit_slip.jpg]       │                 │
│      │                                        │                 │
│      │ [Submit Payment]                       │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│  4.3 Payment Submitted                                          │
│      Transaction Status: "Pending Approval"                     │
│      Transaction #: TXN-2024-00001                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### PHASE 5: Payment Approval (Owner Portal)

```
┌─────────────────────────────────────────────────────────────────┐
│ STEP 5: Owner approves payment                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Owner logs in to: /owner/transactions                          │
│                                                                  │
│  5.1 View Pending Payments                                      │
│      ┌────────────────────────────────────────────────────────┐ │
│      │ PENDING APPROVALS                                      │ │
│      │ ───────────────────────────────────────────────────── │ │
│      │ TXN-2024-00001 | ABC Cooperative | ₱7,166.88 | Pending │ │
│      │     Payment Method: BDO Bank Transfer                  │ │
│      │     Reference: 1234567890                              │ │
│      │     [View Proof] [Approve] [Reject]                    │ │
│      └────────────────────────────────────────────────────────┘ │
│                                                                  │
│  5.2 Review Payment Proof                                       │
│      - Download/view deposit slip image                         │
│      - Verify amount matches                                    │
│      - Check reference number in bank records                   │
│                                                                  │
│  5.3 Approve Payment                                            │
│      ┌────────────────────────────────────────┐                 │
│      │ Click [Approve]                        │                 │
│      │                                        │                 │
│      │ System automatically:                  │                 │
│      │ ✓ Updates transaction to "Completed"   │                 │
│      │ ✓ Updates invoice to "Paid"            │                 │
│      │ ✓ Updates client balance               │                 │
│      │ ✓ Extends subscription if needed       │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🖥️ Portal URLs Summary

| Portal | URL | Purpose |
|--------|-----|---------|
| Owner Dashboard | `/owner/dashboard` | Main admin overview |
| Client Management | `/owner/clients` | Add/edit clients |
| Module Management | `/owner/modules` | Configure ERP modules |
| Subscription Plans | `/owner/plans` | Create pricing plans |
| Invoice Management | `/owner/invoices` | Generate & send invoices |
| Transaction Management | `/owner/transactions` | Approve payments |
| Client Dashboard | `/client-portal/dashboard` | Client's main view |
| Make Payment | `/client-portal/make-payment` | Submit payment |

---

## 🔗 Integration with Main ERP System

```
┌─────────────────────────────────────────────────────────────────┐
│                    LICENSE VALIDATION FLOW                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Main ERP System                    SaaS Management Portal       │
│  ┌─────────────────┐                ┌─────────────────┐         │
│  │                 │   1. Validate  │                 │         │
│  │  User tries to  │───────────────▶│  Check license  │         │
│  │  access a       │   License Key  │  key validity   │         │
│  │  module         │                │                 │         │
│  │                 │                └────────┬────────┘         │
│  │                 │                         │                   │
│  │                 │◀────────────────────────┘                   │
│  │                 │   2. Response:                              │
│  │  Grant/Deny     │   - Valid/Invalid                          │
│  │  access based   │   - Enabled modules list                   │
│  │  on response    │   - Subscription status                    │
│  └─────────────────┘   - Days until expiry                      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📝 Transaction Types

| Type | Description | Example |
|------|-------------|---------|
| **Subscription** | Monthly/yearly subscription payment | ₱5,999/month |
| **One-Time** | One-time purchases | Setup fee ₱10,000 |
| **Add-On** | Additional module purchase | Audit module ₱400/mo |
| **Refund** | Money returned to client | -₱1,000 |
| **Credit** | Credit applied to account | ₱500 promo credit |
| **Penalty** | Late payment fee | ₱100 late fee |

---

## ⚠️ Important Notes

1. **Database**: The system uses a **separate SQLite database** (`saas_management.db`) stored in LocalApplicationData. For SQL Server, run the `SaaS_Management_Schema_SQLServer.sql` script.

2. **Authentication**: Login pages need to be created. Currently, the portals are accessible directly for testing.

3. **Email Notifications**: Not yet implemented. Invoice emails and payment confirmations would need SMTP configuration.

4. **License Validation API**: The `SaaSLicenseService` provides methods to validate licenses from the main ERP system.

---

## 🚀 Quick Start Testing

1. **Run the application**
2. **Navigate to** `/owner/dashboard` to see the Owner Portal
3. **Navigate to** `/client-portal/dashboard` to see the Client Portal
4. **Test the flow:**
   - Add a client in Owner Portal
   - Create an invoice
   - Switch to Client Portal to view and pay
   - Switch back to Owner Portal to approve payment

