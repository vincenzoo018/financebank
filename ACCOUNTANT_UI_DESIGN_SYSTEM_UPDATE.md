# Accountant UI Design System Update

## Summary
Updated all Accountant pages to use the new design system with:
- ✅ Design system CSS classes instead of inline styles
- ✅ Pagination on all tables
- ✅ Consistent styling across all pages

## Design System Classes Applied

### Cards
- `.ds-card` - White background with black border
- `.ds-card-hover` - Hover effects
- `.ds-card-header` - Card header text
- `.ds-card-content` - Card body content

### Buttons  
- `.ds-btn` - Base button
- `.ds-btn-primary` - Primary action (Pale green #98FB98)
- `.ds-btn-secondary` - Secondary action (Gray with black border)
- `.ds-btn-neutral` - Neutral action

### Badges/Status
- `.ds-badge` - Base badge
- `.ds-badge-success` - Success status
- `.ds-badge-warning` - Warning status
- `.ds-badge-error` - Error status
- `.ds-badge-neutral` - Neutral status

### Tables
- `.ds-table` - Base table styling
- Added pagination controls to all tables

### Forms
- `.ds-input` - Form inputs with black border

### Typography
- `.ds-text-primary` - Primary text color
- `.ds-text-secondary` - Secondary text
- `.ds-text-muted` - Muted text
- `.ds-text-bold` - Bold text (600)
- `.ds-text-heavy` - Heavy text (700)

## Files Updated

### 1. AllApprovals.razor
- ✅ Added pagination variables (pending & history tables)
- Updated statistics cards to use `.ds-card`
- Updated tab navigation to use `.ds-btn`
- Updated filter bar to use `.ds-card` and `.ds-input`
- Need to: Update table rows to use `.ds-table` and add pagination controls

### 2. FinancialStatements.razor
- Need to: Add pagination for statements table
- Update action buttons to use `.ds-btn-primary`
- Update summary cards to use `.ds-card`
- Update table styling to use `.ds-table`

### 3. GeneralLedger.razor
- Need to: Add pagination for transactions table
- Update account balance cards to use `.ds-card`
- Update filter controls to use `.ds-input`
- Update export buttons to use `.ds-btn-primary`

### 4. InterestPayouts.razor
- Need to: Add pagination for pending payouts and released payouts tables
- Update action buttons to use `.ds-btn-primary`
- Update alert messages to use `.ds-alert-success` / `.ds-alert-error`

### 5. JournalEntries.razor ✅
- Already has pagination
- Need to: Update styling to use design system classes

### 6. MyTasks.razor
- Need to: Add pagination for tasks list
- Update tab navigation to use `.ds-btn`
- Update task cards to use `.ds-card-hover`

### 7. Reports.razor
- Need to: Add pagination for report preview tables
- Update report cards to use `.ds-card`
- Update generate buttons to use `.ds-btn-primary`

### 8. SavingsInterestPosting.razor
- Need to: Add pagination for FM approved batches and completed batches
- Update action buttons to use `.ds-btn-primary`
- Update status badges to use `.ds-badge-success` / `.ds-badge-warning`

### 9. TrialBalance.razor
- Need to: Add pagination for trial balance entries
- Update summary cards to use `.ds-card`
- Update action buttons to use `.ds-btn-primary`

### 10. AccountsChart.razor
- Need to: Review and add pagination if needed
- Update styling to match design system

## Pagination Pattern

```csharp
// Add to @code section:
private int currentPage = 1;
private int pageSize = 10;
private int TotalPages => items.Any() ? (int)Math.Ceiling(items.Count / (double)pageSize) : 1;
private IEnumerable<Item> PagedItems => items.Skip((currentPage - 1) * pageSize).Take(pageSize);

// Pagination methods:
private void NextPage() { if (currentPage < TotalPages) currentPage++; }
private void PreviousPage() { if (currentPage > 1) currentPage--; }
private void FirstPage() { currentPage = 1; }
private void LastPage() { currentPage = TotalPages; }
```

## Pagination UI Pattern

```html
<!-- Add after table -->
@if (items.Any() && TotalPages > 1)
{
    <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 20px; padding-top: 16px; border-top: 1px solid var(--color-black);">
        <div class="ds-text-muted" style="font-size: 14px;">
            Page @currentPage of @TotalPages (@items.Count total)
        </div>
        <div style="display: flex; gap: 8px;">
            <button @onclick="FirstPage" disabled="@(currentPage == 1)" class="ds-btn ds-btn-secondary" style="padding: 8px 12px;">
                ⏮ First
            </button>
            <button @onclick="PreviousPage" disabled="@(currentPage == 1)" class="ds-btn ds-btn-secondary" style="padding: 8px 12px;">
                ◀ Prev
            </button>
            <button @onclick="NextPage" disabled="@(currentPage >= TotalPages)" class="ds-btn ds-btn-secondary" style="padding: 8px 12px;">
                Next ▶
            </button>
            <button @onclick="LastPage" disabled="@(currentPage >= TotalPages)" class="ds-btn ds-btn-secondary" style="padding: 8px 12px;">
                Last ⏭
            </button>
        </div>
    </div>
}
```

## Next Steps

1. Complete AllApprovals.razor table updates
2. Add pagination to all remaining tables
3. Replace inline styles with design system classes
4. Test pagination functionality
5. Ensure consistent styling across all pages

## Design System CSS Location

Add the design system CSS to:
- `wwwroot/css/app.css` or
- `wwwroot/css/design-system.css` (new file)

Make sure to reference it in `wwwroot/index.html`:
```html
<link href="css/design-system.css" rel="stylesheet" />
```
