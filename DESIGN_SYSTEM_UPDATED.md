# FINSYS Design System - Complete Guide

## 🎨 Overview
This document outlines the complete design system for the FINSYS Finance System, ensuring consistency and modern aesthetics across all pages.

## 📊 Color Palette

### Primary Colors
- **Primary Green**: `#10b981` - `#059669` - `#047857`
- **Primary Gradient**: `linear-gradient(135deg, #10b981 0%, #059669 50%, #047857 100%)`

### Role-Specific Colors
- **Teller**: `#10b981` (Green)
- **Accountant**: `#8b5cf6` (Purple)
- **Finance Manager**: `#f59e0b` (Amber)
- **Admin**: `#2563eb` (Blue)

### Status Colors
- **Success**: `#10b981` / Background: `#d1fae5`
- **Warning**: `#f59e0b` / Background: `#fef3c7`
- **Danger**: `#ef4444` / Background: `#fee2e2`
- **Info**: `#3b82f6` / Background: `#dbeafe`

### Neutral Colors
- **Text Dark**: `#111827`
- **Text Gray**: `#6b7280`
- **Text Light**: `#9ca3af`
- **Background Light**: `#f9fafb`
- **Border**: `#e5e7eb`

## 🏗️ Layout Components

### Sidebar
- **Width**: 280px
- **Background**: Primary gradient with subtle overlay
- **Animations**: Smooth hover states with translateX and scale effects
- **Active States**: White background with primary color text

### Topbar
- **Height**: 80px
- **User Avatar**: 44px circle with gradient background
- **Typography**: 22px bold for page title

### Content Area
- **Padding**: 32px
- **Max Width**: Flexible with proper spacing

## 🎴 Card System

### Standard Card
```html
<div class="card">
    <!-- Content -->
</div>
```
- **Padding**: 28px
- **Border Radius**: 14px (--radius-lg)
- **Shadow**: Subtle with hover lift effect
- **Border**: 1px solid #e5e7eb

### Page Header Card
```html
<div class="card-header">
    <div class="page-header-content">
        <div class="page-header-icon">
            <!-- SVG Icon -->
        </div>
        <div class="page-header-text">
            <h1 class="page-header-title">Title</h1>
            <p class="page-header-subtitle">Subtitle</p>
        </div>
    </div>
</div>
```
- **Gradient Background**: Primary gradient
- **Padding**: 32px
- **Icon Container**: 56px with backdrop blur

### Role-Specific Headers
Add class for role-specific colors:
- `.page-header-teller` - Green gradient
- `.page-header-accountant` - Purple gradient
- `.page-header-finance` - Amber gradient
- `.page-header-admin` - Blue gradient

## 🔘 Button System

### Primary Button
```html
<button class="btn btn-primary">
    <span>Action</span>
</button>
```
- **Gradient**: Primary green gradient
- **Padding**: 13px 28px
- **Font Weight**: 700 (bold)
- **Hover**: Lift effect (-3px translateY)
- **Ripple Effect**: Built-in with ::before pseudo-element

### Button Variants
- `.btn-primary` - Primary action (gradient green)
- `.btn-secondary` - Secondary action (white with border)
- `.btn-success` - Success action (green)
- `.btn-danger` - Danger action (red)
- `.btn-warning` - Warning action (amber)
- `.btn-info` - Info action (blue)
- `.btn-outline` - Outline style

### Button Sizes
- `.btn-sm` - Small (9px 20px)
- Default - Medium (13px 28px)
- `.btn-lg` - Large (16px 36px)

## 📝 Form System

### Form Group
```html
<div class="form-group">
    <label class="form-label required">Label</label>
    <input type="text" class="form-control" placeholder="..." />
    <span class="form-error">Error message</span>
    <span class="form-help">Help text</span>
</div>
```

### Form Controls
- **Padding**: 14px 18px
- **Border**: 2px solid with transition
- **Focus**: Primary color border with shadow ring
- **Font Weight**: 500 (medium)

### Form States
- `.error` - Red border with light red background
- `.success` - Green border with light green background
- `:disabled` - Gray background, reduced opacity

### Input Group
```html
<div class="input-group">
    <div class="input-group-prepend">₱</div>
    <input type="number" class="form-control" />
    <div class="input-group-append">.00</div>
</div>
```

## 📋 Table System

### Modern Table
```html
<div class="table-container">
    <table class="table">
        <thead>
            <tr>
                <th>Column 1</th>
                <th>Column 2</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Data 1</td>
                <td>Data 2</td>
            </tr>
        </tbody>
    </table>
</div>
```

### Table Features
- **Row Hover**: Light green background (#d1fae5)
- **Header**: Gradient background, uppercase text
- **Cell Padding**: 16px 20px
- **Font Size**: 14px

### Table Variants
- `.table-striped` - Alternating row colors
- `.table-sm` - Compact table with smaller padding

## 🏷️ Badge System

```html
<span class="badge badge-success">Active</span>
<span class="badge badge-warning">Pending</span>
<span class="badge badge-danger">Rejected</span>
<span class="badge badge-info">Processing</span>
```

### Badge Styles
- **Border Radius**: 999px (pill shape)
- **Padding**: 6px 14px
- **Font**: 12px, 700 weight, uppercase
- **Letter Spacing**: 0.3px

## 🚨 Alert System

```html
<div class="alert alert-success">
    <div class="alert-icon">✓</div>
    <div class="alert-content">
        <div class="alert-title">Success!</div>
        <div>Operation completed successfully.</div>
    </div>
</div>
```

### Alert Variants
- `.alert-success` - Green background
- `.alert-warning` - Amber background
- `.alert-danger` - Red background
- `.alert-info` - Blue background

## ⏳ Loading States

### Spinner
```html
<div class="spinner"></div>
<div class="spinner spinner-sm"></div>
<div class="spinner spinner-lg"></div>
```

### Loading Overlay
```html
<div class="loading-overlay">
    <div class="spinner"></div>
</div>
```

## 🎯 Utility Classes

### Spacing
- Margin: `.mt-1` to `.mt-4`, `.mb-1` to `.mb-4`, `.ml-1` to `.ml-3`, `.mr-1` to `.mr-3`
- Padding: `.p-1` to `.p-4`

### Typography
- `.text-center`, `.text-right`, `.text-left`
- `.font-bold` (700), `.font-semibold` (600)
- `.text-sm` (13px), `.text-lg` (16px), `.text-xl` (18px), `.text-2xl` (22px)
- `.text-success`, `.text-danger`, `.text-warning`, `.text-info`, `.text-gray`, `.text-muted`

### Flexbox
- `.d-flex` - Display flex
- `.align-items-center` - Vertical center alignment
- `.justify-content-between` - Space between
- `.justify-content-center` - Center alignment
- `.gap-1` to `.gap-3` - Gap spacing
- `.flex-wrap` - Wrap flex items

### Layout
- `.w-full` - Full width
- `.rounded`, `.rounded-lg` - Border radius
- `.shadow-sm`, `.shadow-md`, `.shadow-lg` - Shadow levels

## 📱 Responsive Design

### Breakpoints
- **Desktop**: 1024px+
- **Tablet**: 768px - 1023px
- **Mobile**: < 768px

### Mobile Adjustments
- Sidebar becomes fixed off-canvas
- Reduced padding (16px vs 32px)
- Smaller typography
- Stacked layouts instead of grid

## ✨ Animations

### Transitions
- Standard: `0.25s cubic-bezier(0.4, 0, 0.2, 1)`
- Quick: `0.2s ease`
- Smooth: `0.3s ease`

### Hover Effects
- **Cards**: Lift 2px up, enhanced shadow
- **Buttons**: Lift 3px up, larger shadow
- **Sidebar Items**: Translate 6px right
- **Tables Rows**: Light green background

### Keyframe Animations
- `@keyframes spin` - 360deg rotation (1s linear infinite)
- `@keyframes fadeIn` - Opacity 0 to 1
- `@keyframes slideUp` - Translate and fade in
- `@keyframes slideDown` - Dropdown animation
- `@keyframes dropdownFadeIn` - Dropdown appearance

## 🎨 Best Practices

### Consistency Rules
1. **Always use design system classes** instead of inline styles
2. **Use utility classes** for spacing and layout
3. **Maintain role-specific colors** for their respective sections
4. **Apply hover states** to interactive elements
5. **Use proper hierarchy** with headers and cards

### Page Structure Template
```html
<!-- Page Header -->
<div class="card-header page-header-[role]">
    <div class="page-header-content">
        <div class="page-header-icon">
            <svg>...</svg>
        </div>
        <div class="page-header-text">
            <h1 class="page-header-title">Page Title</h1>
            <p class="page-header-subtitle">Subtitle</p>
        </div>
    </div>
    <div class="page-header-actions">
        <button class="btn btn-primary">Action</button>
    </div>
</div>

<!-- Alert (if needed) -->
<div class="alert alert-info">
    <div class="alert-icon">ℹ️</div>
    <div class="alert-content">
        <div class="alert-title">Information</div>
        <div>Important message here.</div>
    </div>
</div>

<!-- Main Content Card -->
<div class="card">
    <h3 class="card-title">Section Title</h3>
    
    <!-- Form or Table Content -->
    
</div>
```

### Typography Hierarchy
1. **H1 (Page Title)**: 28px, weight 800, letter-spacing -0.5px
2. **H2 (Section)**: 22px, weight 800, letter-spacing -0.3px
3. **H3 (Card Title)**: 20px, weight 800, letter-spacing -0.3px
4. **Body Text**: 14-15px, weight 500-600
5. **Small Text**: 12-13px, weight 500-600

### Spacing Scale
- **4px** - Micro spacing
- **8px** - Tiny spacing (.gap-1, .mt-1)
- **12px** - Small spacing
- **16px** - Medium spacing (.gap-2, .mt-2)
- **20px** - Default spacing
- **24px** - Large spacing (.gap-3, .mt-3)
- **32px** - XL spacing (.mt-4)

## 🔧 Implementation Notes

### CSS File
All styles are in `/wwwroot/css/unified-design-system.css`

### Shared Components
- `PageHeader.razor` - Reusable page header component
- Located in `/Components/Shared/`

### Color Variables
All colors are defined as CSS custom properties (`:root` variables) for easy theming.

### Performance
- Transitions use `cubic-bezier` for smooth, natural motion
- Hover effects use `transform` (GPU-accelerated)
- Shadows are pre-defined as variables
- Border radius uses consistent scale

---

**Last Updated**: December 2, 2025
**Version**: 2.0
**Maintained By**: FINSYS Development Team
