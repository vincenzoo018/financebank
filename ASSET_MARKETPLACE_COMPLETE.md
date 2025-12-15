# Asset Marketplace Feature - Complete Implementation Guide

## Overview
The Asset Marketplace feature allows customers to browse and apply for properties, vehicles, and other assets offered by the bank. The feature supports both **Cash Purchase** and **Loan Purchase** options, with the loan process following the same workflow as regular bank loans.

---

## UI Pages Summary

### Admin Pages (Routes)

| Page | Route | Purpose |
|------|-------|---------|
| [AssetManagement.razor](Components/Pages/Admin/AssetManagement.razor) | `/admin/asset-management` | CRUD operations for assets (add, edit, delete), upload multiple images, set pricing/terms |
| [AssetApplications.razor](Components/Pages/Admin/AssetApplications.razor) | `/admin/asset-applications` | View all customer applications, process workflow (Verify → Assess → Approve → Release) |

### Customer Pages (Routes)

| Page | Route | Purpose |
|------|-------|---------|
| [AssetMarketplace.razor](Components/Pages/Customer/AssetMarketplace.razor) | `/customer/asset-marketplace` | Browse available assets, filter by type (Property/Vehicle/Other) |
| [AssetDetails.razor](Components/Pages/Customer/AssetDetails.razor) | `/customer/asset-details/{AssetId}` | View asset details, loan calculator, submit application |
| [MyAssetApplications.razor](Components/Pages/Customer/MyAssetApplications.razor) | `/customer/my-asset-applications` | Track application status, view progress timeline |

---

## Navigation Access

### Admin Sidebar
Located in `Components/Layout/AdminLayout.razor` under **ASSET MARKETPLACE** section:
- Asset Management
- Asset Applications

### Customer Sidebar  
Located in `Components/Layout/CustomerLayout.razor` under **SERVICES** section:
- Asset Marketplace
- My Asset Applications

---

## Asset Categories

### 1. Property
- **Types**: House & Lot, Condo Unit, Townhouse, Lot Only
- **Fields**: Location, Land Area (sqm), Floor Area (sqm), Bedrooms, Bathrooms, Parking Slots, Developer, Title Status
- **Contract Documents**: Deed of Sale, Transfer Certificate of Title (TCT)

### 2. Vehicle
- **Types**: Any brand/model (Car, Motorcycle, SUV, etc.)
- **Fields**: Brand, Model, Year, Condition (Brand New/Used), Mileage, Transmission, Fuel Type, Color, Engine Number, Chassis Number, Plate Number
- **Contract Documents**: Official Receipt (OR), Certificate of Registration (CR)

### 3. Other Assets
- **Categories**: Appliances, Jewelry, Equipment, Electronics, Furniture
- **Fields**: Brand, Model, Condition, Specifications
- **Contract Documents**: Sales Agreement

---

## Application Workflow

The asset purchase process follows the same workflow as regular loans:

```
┌──────────────────────────────────────────────────────────────────────────┐
│                          ASSET APPLICATION WORKFLOW                       │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. SUBMITTED        Customer submits application                        │
│        ↓                                                                 │
│  2. VERIFIED         Teller reviews and verifies documents               │
│        ↓                                                                 │
│  3. ASSESSED         Accountant assesses financial eligibility           │
│        ↓                                                                 │
│  4. APPROVED         Finance Manager approves the application            │
│        ↓                                                                 │
│  5. RELEASED         Teller releases asset and generates contract        │
│                                                                          │
│  At any stage: Application can be REJECTED or CANCELLED                  │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## Purchase Types

### Cash Purchase
- Customer pays full amount upfront
- No loan record created
- Immediate asset transfer

### Loan Purchase
- Customer specifies down payment and loan term
- Monthly payment calculated with interest
- Loan record created upon release
- Payment schedule auto-generated
- Follows standard loan repayment process

---

## Database Tables

Run the SQL script [ASSET_MARKETPLACE_TABLES.sql](ASSET_MARKETPLACE_TABLES.sql) to create:

| Table | Description |
|-------|-------------|
| `Assets` | Main asset details (property/vehicle/other specific fields) |
| `AssetImages` | Multiple images per asset (VARBINARY storage) |
| `AssetApplications` | Customer applications with workflow tracking |

---

## Key Features

### Admin Features
✅ Add new assets with full details  
✅ Upload multiple images (max 10 per asset)  
✅ Set primary image for display  
✅ Configure pricing (total price, down payment %, interest rate, term)  
✅ Filter assets by type and status  
✅ Edit and delete assets  
✅ View all applications with status filtering  
✅ Process applications through workflow stages  
✅ Add remarks at each workflow stage  
✅ Approve or reject applications  

### Customer Features
✅ Browse available assets  
✅ Filter by asset type  
✅ View detailed asset information  
✅ Image gallery with multiple images  
✅ Loan calculator (monthly payment estimation)  
✅ Choose purchase type (Cash/Loan)  
✅ Submit applications  
✅ Track application progress  
✅ View timeline of workflow stages  
✅ Cancel pending applications  

---

## Service Integration

### AssetMarketplaceService
Located in `Services/AssetMarketplaceService.cs`:
- `GetAllAssetsAsync()` - List all assets
- `GetAvailableAssetsAsync()` - List available assets only
- `GetAssetByIdAsync()` - Get single asset with images
- `CreateAssetAsync()` - Create new asset
- `UpdateAssetAsync()` - Update existing asset
- `DeleteAssetAsync()` - Delete asset
- `AddAssetImageAsync()` - Add image to asset
- `SubmitApplicationAsync()` - Submit new application
- `TellerReviewAsync()` - Teller verification
- `AccountantAssessAsync()` - Accountant assessment
- `FinanceManagerApproveAsync()` - FM approval
- `ReleaseAssetAsync()` - Release asset and create loan

### Contract Generation
Extended `LoanContractService.cs` with asset-specific fields:
- `IsAssetPurchase` - Flag for asset purchase contracts
- `AssetType`, `AssetName`, `AssetDescription` - Asset details
- Property fields (PropertyType, Location, Area, etc.)
- Vehicle fields (Brand, Model, OR/CR numbers)
- Other asset fields (Category, Sales Agreement)

---

## Error Handling

All pages include:
- Success message display (green banner)
- Error message display (red banner)
- Input validation
- Try-catch blocks for async operations
- User-friendly error messages

---

## Getting Started

1. **Database Setup**: Run `ASSET_MARKETPLACE_TABLES.sql`
2. **Admin Login**: Navigate to `/admin/asset-management`
3. **Add Assets**: Click "Add New Asset" and fill in details
4. **Customer Access**: Navigate to `/customer/asset-marketplace`
5. **Process Applications**: Use `/admin/asset-applications` to manage workflow

---

## Files Modified/Created

### Created Files
- `Components/Pages/Admin/AssetManagement.razor`
- `Components/Pages/Admin/AssetApplications.razor`
- `Components/Pages/Customer/AssetMarketplace.razor`
- `Components/Pages/Customer/AssetDetails.razor`
- `Components/Pages/Customer/MyAssetApplications.razor`
- `ASSET_MARKETPLACE_TABLES.sql`

### Modified Files
- `Components/Layout/AdminLayout.razor` - Added navigation links
- `Components/Layout/CustomerLayout.razor` - Added navigation links
- `Data/BFASDbContext.cs` - Added DbSets for Asset entities
- `MauiProgram.cs` - Registered AssetMarketplaceService
- `Services/LoanContractService.cs` - Extended contract data model
- `Services/AssetMarketplaceService.cs` - Fixed Loan model properties

---

## Build Status
✅ **Build Successful** - 0 Errors
