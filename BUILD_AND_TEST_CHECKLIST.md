# ✅ BUILD & TEST CHECKLIST

## 🔨 BUILD STEPS

- [ ] **Step 1:** Clean Solution
  - Visual Studio → Build → Clean Solution
  - Wait for completion

- [ ] **Step 2:** Rebuild Solution
  - Visual Studio → Build → Rebuild Solution
  - Verify no compilation errors in Error List

- [ ] **Step 3:** Verify No Errors
  - Check Error List window
  - Should show 0 errors, 0 warnings
  - All files should compile successfully

---

## 🧪 TESTING STEPS

### Test 1: Access Card Applications Page
- [ ] Run application (F5)
- [ ] Navigate to `http://localhost/admin/approvals/cards`
- [ ] Page should load with green header
- [ ] Should see "Card Applications" title
- [ ] Stats cards should display (Pending, Approved, Rejected)

### Test 2: Access Loan Applications Page
- [ ] Navigate to `http://localhost/admin/approvals/loans`
- [ ] Page should load with green header
- [ ] Should see "Loan Applications" title
- [ ] Stats cards should display (Pending, Approved, Rejected, Total Amount)

### Test 3: Tab Switching
- [ ] Click "Pending" tab
- [ ] Click "Approved" tab
- [ ] Click "Rejected" tab
- [ ] Tabs should switch smoothly
- [ ] Content should update correctly

### Test 4: Empty State
- [ ] With no applications, should see "No pending applications" message
- [ ] Message should display checkmark icon
- [ ] Layout should be centered and readable

### Test 5: Modal Functionality
- [ ] (When applications exist) Click "Review" button
- [ ] Modal should open with customer information
- [ ] Modal should have Cancel, Reject, and Approve buttons
- [ ] Click Cancel to close modal

### Test 6: Rejection Workflow
- [ ] Click "Review" on an application
- [ ] Click "Reject" button
- [ ] Rejection modal should open
- [ ] Enter rejection reason
- [ ] Click "Confirm Rejection"
- [ ] Modal should close
- [ ] Application should move to Rejected tab

### Test 7: Approval Workflow
- [ ] Click "Review" on an application
- [ ] (Optional) Add admin notes
- [ ] Click "Approve" button
- [ ] Application should move to Approved tab
- [ ] Admin notes should be saved

### Test 8: Loan Amount Modification
- [ ] Click "Review" on a loan application
- [ ] Enter a different approved amount
- [ ] Click "Approve"
- [ ] Approved tab should show the modified amount

### Test 9: Responsive Design
- [ ] Resize browser window
- [ ] Layout should adapt to smaller screens
- [ ] Tables should remain readable
- [ ] Buttons should remain clickable

### Test 10: Database Integration
- [ ] Check SQL Server
- [ ] Open ApprovalQueue table
- [ ] Verify applications are being saved
- [ ] Check RequestType = "CARD" or "LOAN"
- [ ] Check Status values (Pending, Approved, Rejected)

---

## 🔍 VERIFICATION CHECKLIST

### Code Quality
- [ ] No compilation errors
- [ ] No runtime errors in console
- [ ] No JavaScript errors in browser console
- [ ] All async operations complete successfully

### UI/UX
- [ ] Green gradient header displays correctly
- [ ] SVG icons render properly
- [ ] Text is readable and properly formatted
- [ ] Colors match design specification
- [ ] Spacing and padding are consistent

### Functionality
- [ ] Applications can be approved
- [ ] Applications can be rejected
- [ ] Rejection reasons are required
- [ ] Admin notes are optional
- [ ] Loan amounts can be modified
- [ ] Status updates persist in database

### Performance
- [ ] Pages load quickly
- [ ] No lag when switching tabs
- [ ] Modals open smoothly
- [ ] Database queries complete in reasonable time

---

## 🐛 TROUBLESHOOTING

### If Build Fails
1. Check Error List for specific errors
2. Verify all files were created correctly
3. Ensure using statements are correct
4. Clean and rebuild solution

### If Page Won't Load
1. Check browser console for errors
2. Verify route is correct (`/admin/approvals/cards` or `/admin/approvals/loans`)
3. Check if user is authenticated
4. Verify user has admin role

### If Modals Don't Open
1. Check browser console for JavaScript errors
2. Verify @onclick handlers are correct
3. Check if showApprovalModal variable is being set
4. Verify modal HTML is not hidden

### If Data Doesn't Save
1. Check database connection string
2. Verify ApprovalQueue table exists
3. Check SQL Server is running
4. Verify user has database permissions

---

## 📊 TEST DATA

### Sample Card Application
- Customer: John Doe
- Email: john@example.com
- Card Type: Credit
- Delivery: Physical
- Applied: Today

### Sample Loan Application
- Customer: Jane Smith
- Email: jane@example.com
- Loan Type: Personal
- Amount: ₱75,000
- Term: 12 months
- Purpose: Home improvement

---

## ✅ SIGN-OFF

- [ ] All build steps completed
- [ ] All tests passed
- [ ] No errors or warnings
- [ ] Ready for production

---

## 📝 NOTES

- Keep browser console open during testing to catch any errors
- Test with different screen sizes to verify responsiveness
- Test with different user roles to verify access control
- Test with empty database to verify empty state handling
- Test with multiple applications to verify list functionality

---

**Last Updated:** January 2025
**Status:** Ready for Testing ✅
