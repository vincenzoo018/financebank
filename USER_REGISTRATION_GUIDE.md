# 👤 User Registration & Authentication Guide

## 🎯 Overview

The user registration system now includes:
- ✅ Automatic Employee ID generation
- ✅ Database persistence
- ✅ Immediate authentication
- ✅ Role-based redirection
- ✅ Comprehensive error handling

---

## 📝 Registration Process

### Step 1: Access Registration Page
- Navigate to `/admin/user-registration`
- Admin role required

### Step 2: Select User Type
- **Employee** - Staff member with role and department
- **Customer** - Banking customer

### Step 3: Fill Personal Information
- Full Name (required)
- Email (required)
- Contact Number (required)
- Address (required)
- Date of Birth (optional)
- Gender (optional)

### Step 4: Upload Valid ID
- Supported formats: JPG, PNG, PDF
- Max file size: 5MB
- Required for verification

### Step 5: Create Credentials
- Username (required, 4+ characters)
- Password (required, 8+ characters)
- Show/Hide toggle available

### Step 6: Employee-Specific Details (if Employee selected)
- **Role** (required)
  - SuperAdmin
  - Accountant
  - FinanceManager
  - Cashier/Teller
  - Loan Officer
  - Customer Service
  - Manager
  - Auditor

- **Department** (required)
  - Finance
  - Accounting
  - Loans
  - Operations
  - Customer Service
  - IT
  - HR
  - Audit
  - Management

- **Employee ID** (optional)
  - Leave blank for auto-generation
  - Format: `EMP-YYYYMMDD-XXXXX`
  - Example: `EMP-20250118-00001`

- **Branch/Location** (optional)

### Step 7: Set Account Status
- **Pending Approval** (default)
- **Activate Immediately**

### Step 8: Submit
- Click "Create Account"
- System validates all required fields
- Creates user in database
- Authenticates user immediately

---

## 🔄 What Happens Behind the Scenes

### 1. Validation
```
✓ Username not empty
✓ Password not empty
✓ Full name not empty
✓ Email not empty
✓ Contact not empty
✓ Address not empty
✓ Role selected (if Employee)
✓ Department selected (if Employee)
✓ Username not already in database
```

### 2. Generation
```
Account Number: ACC-2025-XXXXX (e.g., ACC-2025-12345)
Employee ID: EMP-YYYYMMDD-XXXXX (e.g., EMP-20250118-00001)
Role Mapping: "Cashier/Teller" → "Teller"
```

### 3. Database Save
```
INSERT INTO Users (
    Username,
    PasswordHash,
    Role,
    FullName,
    Email,
    PhoneNumber,
    Department,
    EmployeeId,
    IsActive,
    CreatedAt
) VALUES (...)
```

### 4. Authentication
```
LoginAsync(username, password)
├─ Create user session
├─ Set CurrentUserId
├─ Set CurrentUser
├─ Set CurrentRole
└─ Create LoginHistory record
```

### 5. Redirect
```
Teller → /teller/dashboard
Admin → /admin/dashboard
Customer → /customer/dashboard
Accountant → /admin/dashboard
Finance Manager → /admin/dashboard
```

---

## ✅ Success Response

When account is created successfully:

```
✓ Account created successfully!

Account Number: ACC-2025-12345
Username: newteller
Role: Cashier/Teller
Employee ID: Auto-generated (EMP-YYYYMMDD-XXXXX)
Department: Operations

User has been authenticated and can log in immediately.
```

User is automatically logged in and redirected to their dashboard.

---

## ❌ Error Handling

### Common Errors

**"Username is required"**
- Solution: Enter a username

**"Username already exists"**
- Solution: Choose a different username
- Check if user already registered

**"Password is required"**
- Solution: Enter a password (8+ characters)

**"Please fill in all required fields"**
- Solution: Complete all marked required fields

**"Please select a role and department for the employee"**
- Solution: Select both role and department for employee accounts

**"Database error: ..."**
- Solution: Check database connection
- Check SQL Server is running
- Check connection string in appsettings.json

**"Error creating user: ..."**
- Solution: Check console logs for details
- Verify all inputs are valid
- Contact system administrator

---

## 🔐 Authentication Details

### Automatic Login
- User is logged in immediately after creation
- No need to manually log in
- Session created in database
- User can access role-specific pages

### Role-Based Access
```
Teller Role:
├─ /teller/dashboard
├─ /teller/deposits
├─ /teller/withdrawals
└─ /teller/transactions

Admin Role:
├─ /admin/dashboard
├─ /admin/manage-deposits-withdrawals
├─ /admin/user-registration
└─ ... (all admin pages)

Customer Role:
├─ /customer/dashboard
├─ /customer/deposit
├─ /customer/withdraw
└─ ... (all customer pages)
```

---

## 📊 Employee ID Generation

### Format
`EMP-YYYYMMDD-XXXXX`

### Example
- Date: 2025-01-18
- Random: 00001
- Result: `EMP-20250118-00001`

### Manual Override
- Leave blank for auto-generation
- Or enter custom Employee ID
- System uses provided value if not blank

---

## 🧪 Testing Scenarios

### Scenario 1: Create Teller Account
1. Go to `/admin/user-registration`
2. Select "Employee"
3. Fill all required fields
4. Role: "Cashier/Teller"
5. Department: "Operations"
6. Leave Employee ID blank
7. Click "Create Account"
8. Verify: User logged in and at `/teller/dashboard`

### Scenario 2: Create Customer Account
1. Go to `/admin/user-registration`
2. Select "Customer"
3. Fill all required fields
4. Leave Role/Department blank
5. Click "Create Account"
6. Verify: User logged in and at `/customer/dashboard`

### Scenario 3: Duplicate Username
1. Try to create account with existing username
2. Verify: Error message "Username already exists"
3. Try different username

### Scenario 4: Missing Required Fields
1. Leave one required field empty
2. Click "Create Account"
3. Verify: Error message shows which field is required

---

## 📋 Database Schema

### Users Table
```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    Department NVARCHAR(50),
    EmployeeId NVARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLoginAt DATETIME
)
```

---

## 🚀 Quick Start

### For Admin Creating Teller
1. Navigate to `/admin/user-registration`
2. Select "Employee"
3. Enter details:
   - Name: John Doe
   - Email: john@bank.com
   - Contact: +63-917-123-4567
   - Address: 123 Main St
   - Username: johndoe
   - Password: SecurePass123
   - Role: Cashier/Teller
   - Department: Operations
4. Leave Employee ID blank
5. Click "Create Account"
6. Success! User is now logged in as Teller

---

## 📞 Support

### Console Logs
Check browser console for detailed logs:
```
✓ User 'johndoe' saved to database successfully
  - Employee ID: EMP-20250118-00001
  - Role: Teller
  - Department: Operations
✓ User 'johndoe' authenticated successfully
```

### Database Verification
```sql
SELECT * FROM Users WHERE Username = 'johndoe'
```

### Troubleshooting
- Check database connection
- Verify SQL Server is running
- Check appsettings.json connection string
- Review console logs for errors
- Check database permissions

---

## ✨ Features

✅ Auto-generated Employee IDs
✅ Immediate authentication
✅ Role-based redirection
✅ Comprehensive validation
✅ Database persistence
✅ Error handling
✅ Console logging
✅ File upload support
✅ Multi-role support
✅ Department assignment

---

**Status:** ✅ Production Ready

**Last Updated:** 2025-01-18
