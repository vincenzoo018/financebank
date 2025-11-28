# FINSYS Unified Design System

## Overview
A modern, minimal design system with a gradient green color palette and consistent UI patterns across all roles (Admin, Accountant, Customer, Finance Manager).

---

## Color Palette

### Primary Colors
```css
--primary-green-start: #4a7c59  /* Main gradient start */
--primary-green-end: #6b9b7e    /* Main gradient end */
--primary-gradient: linear-gradient(135deg, #4a7c59 0%, #6b9b7e 100%)
```

### Supporting Colors
```css
--white: #ffffff
--text-dark: #1f2937
--text-gray: #6b7280
--text-light: #9ca3af
--bg-light: #f9fafb
--bg-lighter: #f3f4f6
--border: #e5e7eb
```

### Status Colors
```css
--success: #10b981
--warning: #f59e0b
--danger: #ef4444
--info: #3b82f6
```

---

## Layout System

### Sidebar Navigation
All role-based layouts (Admin, Accountant, Customer, Finance Manager) use a consistent sidebar with:

- **Gradient green background** with white text
- **280px width**
- **Hover effects** with subtle transparency
- **Active state** with white background and green text
- **Smooth transitions** on all interactions

#### HTML Structure
```html
<div class="sidebar-container">
    <div class="sidebar">
        <div class="sidebar-header">
            <!-- Logo and title -->
        </div>
        <div class="sidebar-nav">
            <div class="sidebar-section">
                <div class="sidebar-section-title">SECTION NAME</div>
                <a href="#" class="sidebar-item active">
                    <svg class="sidebar-icon">...</svg>
                    <span>Menu Item</span>
                </a>
            </div>
        </div>
    </div>
    <div class="sidebar-main">
        <div class="sidebar-topbar">...</div>
        <div class="sidebar-content">...</div>
    </div>
</div>
```

---

## Card System

### Standard Card
```html
<div class="card">
    <h3 class="card-title">Card Title</h3>
    <p>Card content here...</p>
</div>
```

### Card with Gradient Header
```html
<div class="card">
    <div class="card-header">
        <h3 style="margin: 0; color: white;">Header Title</h3>
    </div>
    <div class="card-body">
        Content here...
    </div>
</div>
```

### Stat Card
```html
<div class="stat-card">
    <div class="stat-label">Label</div>
    <div class="stat-value">$1,234.56</div>
    <div class="stat-change positive">+12.5%</div>
</div>
```

---

## Step-Based Modal System

### Overview
All modals should follow a 2-step pattern:
- **Step 1: Details** - User enters information
- **Step 2: Confirmation** - User reviews and confirms

### Using StepModal Component

```razor
<StepModal IsVisible="@showModal"
           OnClose="@CloseModal"
           OnConfirm="@ConfirmAction"
           Title="Modal Title"
           Subtitle="Modal description"
           IconPath='<path d="...">SVG Path</path>'
           ShowSteps="true"
           CurrentStep="@currentStep"
           Step1Label="Details"
           Step2Label="Confirmation"
           Step1Valid="@IsStep1Valid"
           Step2Valid="true"
           ConfirmButtonText="Confirm"
           OnStepChanged="@HandleStepChanged">
    
    <Step1Content>
        <!-- Step 1 form fields -->
        <div class="step-section">
            <h3 class="step-section-title">Section Title</h3>
            <div class="form-group">
                <label class="form-label required">Field Label</label>
                <input type="text" class="form-control" @bind="value" />
            </div>
        </div>
    </Step1Content>
    
    <Step2Content>
        <!-- Step 2 confirmation summary -->
        <div class="confirmation-summary">
            <div class="summary-row">
                <span class="summary-label">Label:</span>
                <span class="summary-value">@value</span>
            </div>
            <div class="summary-row">
                <span class="summary-label">Total:</span>
                <span class="summary-value highlight">@total</span>
            </div>
        </div>
    </Step2Content>
</StepModal>
```

### Code-Behind Example

```csharp
@code {
    private bool showModal = false;
    private int currentStep = 1;
    private string value = "";
    
    private bool IsStep1Valid => !string.IsNullOrEmpty(value);
    
    private void HandleStepChanged(int step)
    {
        currentStep = step;
    }
    
    private void CloseModal()
    {
        currentStep = 1;
        showModal = false;
    }
    
    private async Task ConfirmAction()
    {
        // Process the action
        await ProcessAsync();
        CloseModal();
    }
}
```

---

## Button System

### Primary Button (Green Gradient)
```html
<button class="btn btn-primary">
    Primary Action
</button>
```

### Secondary Button
```html
<button class="btn btn-secondary">
    Secondary Action
</button>
```

### Success Button
```html
<button class="btn btn-success">
    Success Action
</button>
```

### Danger Button
```html
<button class="btn btn-danger">
    Delete
</button>
```

### Button Sizes
```html
<button class="btn btn-primary btn-lg">Large</button>
<button class="btn btn-primary">Default</button>
<button class="btn btn-primary btn-sm">Small</button>
<button class="btn btn-primary btn-block">Full Width</button>
```

---

## Form System

### Form Group
```html
<div class="form-group">
    <label class="form-label required">Field Name</label>
    <input type="text" class="form-control" placeholder="Enter value" />
    <small class="form-help">Help text here</small>
</div>
```

### Form with Validation
```html
<div class="form-group">
    <label class="form-label required">Email</label>
    <input type="email" class="form-control @(isValid ? "success" : "error")" />
    @if (!isValid)
    {
        <small class="form-error">Please enter a valid email</small>
    }
</div>
```

### Select Dropdown
```html
<div class="form-group">
    <label class="form-label">Category</label>
    <select class="form-control">
        <option>Option 1</option>
        <option>Option 2</option>
    </select>
</div>
```

### Textarea
```html
<div class="form-group">
    <label class="form-label">Description</label>
    <textarea class="form-control" rows="4"></textarea>
</div>
```

---

## Typography

### Headings
```html
<h1 style="font-size: 32px; font-weight: 700;">Heading 1</h1>
<h2 style="font-size: 24px; font-weight: 700;">Heading 2</h2>
<h3 style="font-size: 20px; font-weight: 700;">Heading 3</h3>
<h4 style="font-size: 18px; font-weight: 600;">Heading 4</h4>
```

### Body Text
```html
<p style="font-size: 15px; color: var(--text-dark);">Regular text</p>
<p style="font-size: 14px; color: var(--text-gray);">Secondary text</p>
<p style="font-size: 13px; color: var(--text-light);">Helper text</p>
```

---

## Icons

Use SVG icons with consistent sizing:
```html
<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="..."></path>
</svg>
```

### Sidebar Icons
- Width/Height: 20px
- Class: `sidebar-icon`
- Stroke: currentColor
- Stroke-width: 2

### Small Icons (Dropdowns)
- Width/Height: 16px
- Class: `sidebar-icon-small`
- Stroke: currentColor
- Stroke-width: 2

---

## Utility Classes

### Spacing
```css
.mt-8  { margin-top: 8px; }
.mt-16 { margin-top: 16px; }
.mt-24 { margin-top: 24px; }
.mb-8  { margin-bottom: 8px; }
.mb-16 { margin-bottom: 16px; }
.mb-24 { margin-bottom: 24px; }
```

### Layout
```css
.flex          { display: flex; }
.flex-column   { flex-direction: column; }
.gap-8         { gap: 8px; }
.gap-12        { gap: 12px; }
.gap-16        { gap: 16px; }
.items-center  { align-items: center; }
.justify-between { justify-content: space-between; }
.w-full        { width: 100%; }
```

### Text Alignment
```css
.text-center { text-align: center; }
.text-left   { text-align: left; }
.text-right  { text-align: right; }
```

---

## Implementation Checklist

### For New Pages
- [ ] Import unified-design-system.css
- [ ] Use gradient green header with icon
- [ ] Apply consistent card styling
- [ ] Use StepModal for all multi-step forms
- [ ] Follow button system conventions
- [ ] Use form-control classes for inputs
- [ ] Maintain consistent spacing

### For Modals
- [ ] Use StepModal component
- [ ] Define Step 1 for details input
- [ ] Define Step 2 for confirmation
- [ ] Add appropriate validation
- [ ] Include confirmation summary in Step 2
- [ ] Use gradient green header
- [ ] Add helpful icons

### For Forms
- [ ] Use form-group wrapper
- [ ] Add form-label with required indicator if needed
- [ ] Apply form-control to all inputs
- [ ] Include form-help or form-error messages
- [ ] Validate before allowing submission

---

## File Structure

```
wwwroot/css/
├── unified-design-system.css    # Main design system
├── bfas-style.css              # Legacy styles (being phased out)
└── app.css                     # App-specific overrides

Components/
├── Layout/
│   ├── AdminLayout.razor
│   ├── AccountantLayout.razor
│   ├── CustomerLayout.razor
│   └── FinanceManagerLayout.razor
└── Shared/
    └── StepModal.razor         # Reusable step modal

Components/Pages/
└── Customer/
    └── Bills.razor             # Example implementation
```

---

## Best Practices

1. **Always use CSS variables** - Don't hardcode colors
2. **Consistent spacing** - Use multiples of 4px (4, 8, 12, 16, 20, 24, 32)
3. **Mobile-first** - Design works on all screen sizes
4. **Accessibility** - Include proper labels and ARIA attributes
5. **Performance** - Minimize inline styles, use classes
6. **Reusability** - Use StepModal and card components
7. **Gradients** - Apply gradient green to headers and primary actions
8. **Whitespace** - Give content room to breathe
9. **Consistency** - Same patterns across all roles
10. **Validation** - Always validate user input before submission

---

## Migration Guide

### From Old to New Design

1. **Replace inline sidebar styles:**
   ```html
   <!-- OLD -->
   <div style="background: white; border-right: 1px solid #e5e7eb;">
   
   <!-- NEW -->
   <div class="sidebar">
   ```

2. **Update card styling:**
   ```html
   <!-- OLD -->
   <div style="background: white; border-radius: 16px; padding: 24px;">
   
   <!-- NEW -->
   <div class="card">
   ```

3. **Replace custom modals with StepModal:**
   ```razor
   @* OLD *@
   @if (showModal)
   {
       <div style="position: fixed; ...">
           <!-- Custom modal HTML -->
       </div>
   }
   
   @* NEW *@
   <StepModal IsVisible="@showModal" ...>
       <Step1Content>...</Step1Content>
       <Step2Content>...</Step2Content>
   </StepModal>
   ```

4. **Update buttons:**
   ```html
   <!-- OLD -->
   <button style="background: #059669; color: white; padding: 12px 24px;">
   
   <!-- NEW -->
   <button class="btn btn-primary">
   ```

---

## Support & Questions

For questions or issues with the design system:
1. Check this documentation first
2. Review the Bills.razor example implementation
3. Inspect the unified-design-system.css for available classes
4. Test with the StepModal component

---

**Version:** 1.0  
**Last Updated:** November 27, 2025  
**Maintained By:** FINSYS Development Team
