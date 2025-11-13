# FINSYS BANKING SYSTEM - DESIGN STANDARDS

## 📐 TYPOGRAPHY STANDARDS

### Font Family
```css
-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif
```

### Font Sizes (Standardized Across All Portals)
| Element | Size | Usage |
|---------|------|-------|
| `--font-xs` | **11px** | Badge text, timestamps, fine print |
| `--font-sm` | **12px** | Form labels, captions, secondary text |
| `--font-base` | **14px** | Body text, buttons, inputs (DEFAULT) |
| `--font-md` | **16px** | Emphasis text, card titles |
| `--font-lg` | **18px** | Large amounts |
| `--font-xl` | **20px** | Section headers, modal titles |
| `--font-2xl` | **24px** | Page headers |
| `--font-3xl` | **28px** | Dashboard titles (OLD - deprecated) |
| `--font-4xl` | **32px** | Hero numbers (rare use) |

### Font Weights
- **Normal:** 400 (body text)
- **Medium:** 500 (buttons, emphasis)
- **Semibold:** 600 (headings, labels)
- **Bold:** 700 (numbers, strong emphasis)

---

## 🎨 ICON STANDARDS

### Icon Sizes (SVG only - NO EMOJIS)
| Size | Pixels | Usage |
|------|--------|-------|
| `--icon-xs` | **14px** | Inline icons in small text |
| `--icon-sm` | **16px** | Buttons, quick actions, sidebar |
| `--icon-base` | **20px** | Standard icons (DEFAULT) |
| `--icon-md` | **22px** | Card headers |
| `--icon-lg` | **24px** | Page headers |
| `--icon-xl` | **28px** | Hero icons (rare) |
| `--icon-2xl` | **32px** | Dashboard large icons (deprecated) |

### Icon Properties
- **Format:** SVG only (transparent, stroke-based)
- **Stroke Width:** 2px (standard)
- **Fill:** none
- **Colors:** Inherit from parent or use theme colors

### Icon Examples
```html
<!-- Standard icon (20px) -->
<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
    <path d="M5 12h14M12 5l7 7-7 7"/>
</svg>

<!-- Small icon (16px) -->
<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
    <circle cx="12" cy="12" r="10"/>
</svg>

<!-- Header icon (24px) -->
<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
    <path d="M12 5v14M5 12l7 7 7-7"/>
</svg>
```

---

## 🎨 COLOR SYSTEM (2-Color Gradients)

### Function-Based Gradients
| Function | Gradient | Hex Codes |
|----------|----------|-----------|
| **Transfer/Transactions** | Cyan | `#0891b2` → `#0e7490` |
| **Deposit/Savings** | Green | `#059669` → `#047857` |
| **Withdraw** | Red | `#dc2626` → `#991b1b` |
| **Bills** | Amber/Orange | `#f59e0b` → `#d97706` |
| **Cards/Rewards** | Purple | `#7c3aed` → `#6d28d9` |
| **Settings** | Indigo | `#6366f1` → `#4f46e5` |
| **Profile** | Gray | `#1f2937` → `#111827` |
| **Employee Approvals** | Blue | `#2563eb` → `#1e40af` |
| **Employee Tasks** | Green | `#059669` → `#047857` |

### Text Colors
- **Primary:** `#1f2937` (headings, main text)
- **Secondary:** `#6b7280` (labels, secondary info)
- **Tertiary:** `#9ca3af` (disabled, hints)

### Background Colors
- **White:** `#ffffff`
- **Gray 50:** `#f9fafb` (light backgrounds)
- **Gray 100:** `#f3f4f6` (secondary buttons)
- **Gray 200:** `#e5e7eb` (borders)

---

## 📏 SPACING SYSTEM

| Variable | Size | Usage |
|----------|------|-------|
| `--space-xs` | **4px** | Tight spacing |
| `--space-sm` | **8px** | Icon gaps, small margins |
| `--space-base` | **12px** | Standard gaps, form spacing |
| `--space-md` | **16px** | Card padding, between elements |
| `--space-lg` | **20px** | Section spacing |
| `--space-xl` | **24px** | Page padding, large gaps |
| `--space-2xl` | **32px** | Major sections |
| `--space-3xl` | **40px** | Page margins (rare) |

---

## 🎯 COMPONENT STANDARDS

### Page Header
```html
<div style="background: linear-gradient(135deg, #0891b2, #0e7490); 
            border-radius: 16px; 
            padding: 24px; 
            margin-bottom: 24px; 
            color: white;">
    <div style="display: flex; align-items: center; gap: 16px;">
        <div style="width: 48px; height: 48px; 
                    background: rgba(255,255,255,0.2); 
                    border-radius: 50%; 
                    display: flex; align-items: center; justify-content: center;">
            <svg width="24" height="24" viewBox="0 0 24 24" 
                 fill="none" stroke="currentColor" stroke-width="2">
                <!-- icon path -->
            </svg>
        </div>
        <div>
            <h1 style="font-size: 24px; font-weight: 700; margin: 0;">Page Title</h1>
            <p style="margin: 4px 0 0; opacity: 0.9; font-size: 14px;">Subtitle</p>
        </div>
    </div>
</div>
```

### Card
```html
<div style="background: white; 
            border-radius: 16px; 
            padding: 20px; 
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);">
    <h3 style="font-size: 16px; font-weight: 600; 
               color: #1f2937; margin-bottom: 16px;">
        Card Title
    </h3>
    <!-- Content -->
</div>
```

### Button Primary
```html
<button style="padding: 12px 24px; 
               background: linear-gradient(135deg, #0891b2, #0e7490); 
               border: none; 
               border-radius: 8px; 
               color: white; 
               font-size: 14px; 
               font-weight: 600; 
               cursor: pointer;">
    Button Text
</button>
```

### Button Secondary
```html
<button style="padding: 12px 24px; 
               background: #f3f4f6; 
               border: none; 
               border-radius: 8px; 
               color: #374151; 
               font-size: 14px; 
               font-weight: 600; 
               cursor: pointer;">
    Button Text
</button>
```

### Form Input
```html
<div class="form-group" style="margin-bottom: 16px;">
    <label class="form-label" style="font-size: 12px; 
                                     font-weight: 500; 
                                     color: #6b7280; 
                                     margin-bottom: 6px; 
                                     display: block;">
        Label Text
    </label>
    <input type="text" 
           class="form-control" 
           style="font-size: 14px; padding: 12px; 
                  width: 100%; 
                  border: 1px solid #e5e7eb; 
                  border-radius: 8px;" 
           placeholder="Enter text" />
</div>
```

### Badge/Status
```html
<span style="padding: 4px 12px; 
             background: #d1fae5; 
             color: #065f46; 
             border-radius: 6px; 
             font-size: 11px; 
             font-weight: 600;">
    Status
</span>
```

### Modal
```html
<div style="position: fixed; top: 0; left: 0; right: 0; bottom: 0; 
            background: rgba(0,0,0,0.5); 
            display: flex; align-items: center; justify-content: center; 
            z-index: 1000;">
    <div style="background: white; 
                border-radius: 16px; 
                padding: 28px; 
                max-width: 600px; 
                width: 90%; 
                box-shadow: 0 20px 60px rgba(0,0,0,0.3);">
        <h2 style="font-size: 20px; font-weight: 700; 
                   color: #1f2937; margin-bottom: 20px;">
            Modal Title
        </h2>
        <!-- Content -->
    </div>
</div>
```

---

## ✅ IMPLEMENTATION CHECKLIST

### For Each Page:
- [ ] Replace ALL emojis with SVG icons
- [ ] Header icon: 24px (in 48px container)
- [ ] Body icons: 16-20px
- [ ] Page title: 24px (not 28px)
- [ ] Section headers: 16px (not 18px)
- [ ] Body text: 14px
- [ ] Labels: 12px
- [ ] Small text/timestamps: 11px
- [ ] Button text: 14px
- [ ] Use system font stack
- [ ] Apply correct gradient for function
- [ ] Card padding: 20px (not 24px+)
- [ ] Page padding: 24px max-width 1200px

---

## 📊 STATUS BY PORTAL

### Customer Portal (12 pages)
- ✅ TransferMoney.razor - STANDARDIZED
- ⏳ DepositMoney.razor - NEEDS UPDATE
- ⏳ WithdrawMoney.razor - NEEDS UPDATE
- ⏳ Bills.razor - NEEDS UPDATE
- ⏳ Cards.razor - NEEDS UPDATE
- ⏳ Loans.razor - NEEDS UPDATE
- ⏳ Transactions.razor - PARTIAL (has SVG, needs font fix)
- ⏳ Savings.razor - PARTIAL (has SVG, needs font fix)
- ⏳ Rewards.razor - PARTIAL (has SVG, needs font fix)
- ⏳ Settings.razor - NEEDS UPDATE
- ⏳ Profile.razor - PARTIAL (has SVG, needs font fix)
- ⏳ CustomerDashboard.razor - NEEDS UPDATE

### Employee Portal (2 pages)
- ⏳ ApprovalQueue.razor - NEEDS UPDATE
- ⏳ MyTasks.razor - NEEDS UPDATE

### Admin Portal (23 pages)
- ⏳ All pages need standardization

---

## 🚀 QUICK REFERENCE

**Standard Icon Sizes:**
- Quick actions sidebar: **16px**
- Page header: **24px**
- Transaction list: **16px**
- Card decorative: **20px**

**Standard Font Sizes:**
- Page titles: **24px**
- Section headers: **16px**
- Body/buttons: **14px**
- Labels: **12px**
- Fine print: **11px**

**Standard Colors:**
- Use function-based 2-color gradients
- Text: #1f2937 (primary), #6b7280 (secondary)
- Borders: #e5e7eb
- All icons: transparent SVG, stroke-width: 2

---

Last Updated: Nov 13, 2025
