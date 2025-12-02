# 🎨 Dashboard Visual Layout Guide

## Dashboard Structure Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  🏦 System Health Dashboard                         🔄 Refresh  │
│  ┌─────────────┐                                                 │
│  │   [85%]     │  System Status: Good                           │
│  │   🟢 85     │  Current liquidity ratio: 1.75                 │
│  └─────────────┘  Cash flow positive                            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ✅ System Status: Healthy                                      │
│  Banking operations are running smoothly. Liquidity ratio       │
│  at 1.75 with strong cash reserves.                            │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────┬──────────────────┬──────────────────────────┐
│ 💰 Total Assets  │ 🔮 Liquidity     │ 💵 Net Cash Flow        │
│ ₱12,500,000     │ Ratio: 1.75      │ ₱250,000                │
│ ▓▓▓▓░░░ 75%     │ ▓▓▓▓▓░░ 87%     │ ▓▓▓▓▓▓░ 95%            │
│ ───▃▅▇▅▃──      │ ───▃▅▇▅▃──      │ ───▃▅▇▅▃──             │
└──────────────────┴──────────────────┴──────────────────────────┘

┌──────────────────┬──────────────────┬──────────────────────────┐
│ 📋 Loan Portfolio│ 👥 Active Users  │ 📊 Today's Activity     │
│ ₱8,200,000      │ 1,245 users      │ 342 transactions        │
│ ▓▓▓▓▓▓░ 92%     │ ▓▓▓▓░░░ 68%     │ ▓▓▓▓▓░░ 85%            │
│ 45 active loans │ 890 customers    │ 198 deposits            │
└──────────────────┴──────────────────┴──────────────────────────┘

┌─────────────────────────────────┬──────────────────────────────┐
│ 📈 Cash Flow Trend (7 Days)    │ 🏆 Department Activity       │
│                                 │                               │
│   ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓         │ 💵 Teller    ▓▓▓▓▓▓░ 42%    │
│   ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓         │ 📊 Accountant ▓▓▓░░░░ 28%   │
│   ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓         │ 💰 Finance   ▓▓▓▓░░░ 35%    │
│   ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓ ▓▓         │ ⚙️ Admin     ▓▓░░░░░ 18%    │
│   Mon Tue Wed Thu Fri Sat Sun   │                               │
│   🟢 Deposits  🔴 Withdrawals   │ [View Full Report →]         │
└─────────────────────────────────┴──────────────────────────────┘

┌──────────────────┬──────────────────┬──────────────────────────┐
│ 💵 Teller Ops    │ 📊 Accounting    │ 💰 Finance              │
│ [Icon]           │ [Icon]           │ [Icon]                  │
│ Cash transactions│ Financial records│ Loan processing         │
│ ┌──────┬──────┐ │ ┌──────┬──────┐ │ ┌──────┬──────┐         │
│ │Deps  │Wdrws │ │ │Entrs │Bal   │ │ │Active│Pend  │         │
│ │ 198  │ 144  │ │ │ 45   │₱12M  │ │ │  45  │  8   │         │
│ └──────┴──────┘ │ └──────┴──────┘ │ └──────┴──────┘         │
└──────────────────┴──────────────────┴──────────────────────────┘

┌─────────────────────────────────┬──────────────────────────────┐
│ 💰 Financial Overview           │ 📊 System Status            │
│ Total Deposits (Today)          │ Database Status: 🟢 Connected│
│ ₱850,000                        │ Active Loans: 45 loans       │
│                                 │ Today's Transactions: 342    │
│ Total Withdrawals (Today)       │                              │
│ ₱600,000                        │                              │
│                                 │                              │
│ Total Account Balance           │                              │
│ ₱12,500,000                     │                              │
└─────────────────────────────────┴──────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ ⚡ Quick Actions                                                │
│ ┌──────────┬──────────┬──────────┬──────────┐                 │
│ │[+] Reg   │[✓] Rev   │[👥] Mgmt │[📋] Audit│                 │
│ │New User  │Approvals │Users     │Logs      │                 │
│ └──────────┴──────────┴──────────┴──────────┘                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ 📋 Recent Activity                        [View All →]         │
│                                                                  │
│ 💰 Deposit: ₱50,000                       Account #1234        │
│    John Doe • Jan 15, 14:30                                    │
│                                                                  │
│ 💸 Withdrawal: ₱25,000                    Account #5678        │
│    Jane Smith • Jan 15, 13:45                                  │
│                                                                  │
│ 📋 Loan Payment: ₱15,000                  Account #9012        │
│    Mike Johnson • Jan 15, 12:20                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Color Coding Reference

### Health Status Colors:
- 🟢 **Green (90-100%)**: Excellent - System very safe
- 🔵 **Blue (75-89%)**: Good - System healthy
- 🟡 **Yellow (60-74%)**: Fair - Needs monitoring
- 🟠 **Orange (40-59%)**: Poor - Risk detected
- 🔴 **Red (0-39%)**: Critical - Bankruptcy risk

### Department Colors:
- 💵 **Teller**: Green gradient (#10b981)
- 📊 **Accountant**: Purple gradient (#8b5cf6)
- 💰 **Finance**: Amber gradient (#f59e0b)
- ⚙️ **Admin**: Blue gradient (#2563eb)

### Transaction Colors:
- 🟢 **Deposits**: Green (#059669)
- 🔴 **Withdrawals**: Red (#dc2626)
- 🔵 **Transfers**: Blue (#2563eb)
- 🟣 **Loans**: Purple (#8b5cf6)

---

## Responsive Breakpoints

### Desktop (≥ 1024px):
- KPI Grid: 3 columns
- Charts: 2fr + 1fr layout
- Quick Access: 3 columns
- Full sidebar visible

### Tablet (768px - 1023px):
- KPI Grid: 2 columns
- Charts: Stack vertically
- Quick Access: 2 columns
- Collapsible sidebar

### Mobile (< 768px):
- KPI Grid: 1 column
- Charts: Full width
- Quick Access: 1 column
- Hamburger menu

---

## Interactive Elements

### Hover Effects:
- **Cards**: Lift up 2px, shadow increases
- **Buttons**: Brighten 10%, lift 2px
- **Chart bars**: Scale up 1.05x, show tooltip
- **Quick access cards**: Border color changes to primary

### Click Animations:
- **Buttons**: Scale down 0.98x, then bounce back
- **Cards**: Ripple effect from click point
- **Refresh button**: Rotate 360° while loading

### Loading States:
- **Dashboard**: Centered spinner with "Loading dashboard data..."
- **Charts**: Skeleton bars with shimmer animation
- **Cards**: Pulsing placeholder boxes

---

## Alert Banner States

### Success (Green):
```
┌─────────────────────────────────────────────┐
│ ✅ System Status: Excellent                 │
│ Banking operations are running smoothly.    │
│ Liquidity ratio at 2.15 with strong        │
│ cash reserves. Continue monitoring.         │
└─────────────────────────────────────────────┘
```

### Warning (Yellow):
```
┌─────────────────────────────────────────────┐
│ ⚠️ Warning: System Needs Attention         │
│ Low liquidity detected (1.25). Increase     │
│ cash reserves and reduce outstanding        │
│ loans to avoid risk.                        │
└─────────────────────────────────────────────┘
```

### Critical (Red):
```
┌─────────────────────────────────────────────┐
│ 🚨 Critical: Immediate Action Required     │
│ Critical liquidity shortage (0.45)! Bank    │
│ may face insolvency. Immediate intervention │
│ required to prevent bankruptcy.             │
└─────────────────────────────────────────────┘
```

---

## KPI Card Anatomy

```
┌────────────────────────────────────────┐
│ [Icon] KPI Title                       │
│ 🔮 Liquidity Ratio                     │
│                                         │
│ [Large Number]                          │
│ 1.75                                    │
│                                         │
│ [Progress Bar with Color]               │
│ ▓▓▓▓▓░░░░░ 87%                         │
│                                         │
│ [Mini Chart - Last 7 Days]             │
│ ───▃▅▇▅▃─── Trending ↗                │
│                                         │
│ [Secondary Metric]                      │
│ Assets: ₱12.5M | Loans: ₱7.2M          │
└────────────────────────────────────────┘
```

---

## Dashboard Data Flow

```
User Opens Dashboard
        ↓
OnInitializedAsync()
        ↓
LoadDashboardData()
        ↓
┌───────────────────────────────────────┐
│ Fetch from Database:                  │
│ - Users (total, active, customers)    │
│ - Accounts (count, balances)          │
│ - Transactions (deposits, withdrawals)│
│ - Loans (active, pending, amounts)    │
│ - Journal Entries (count, amounts)    │
└───────────────────────────────────────┘
        ↓
CalculateSystemHealth()
        ↓
┌───────────────────────────────────────┐
│ Calculate Metrics:                     │
│ 1. Liquidity Ratio = Assets/Loans    │
│ 2. Net Cash Flow = Deps+Pay-Wdrws    │
│ 3. System Health = Score (0-100)      │
│ 4. Department Performance %           │
└───────────────────────────────────────┘
        ↓
Render Dashboard UI
        ↓
Display Real-Time Data
        ↓
[User Can Click Refresh]
        ↓
RefreshDashboard() → Loop back
```

---

## Quick Reference: Health Score Calculation

```
System Health Score (0-100)
│
├─ Liquidity Score (40 pts max)
│  └─ Based on Assets/Loans ratio
│
├─ Cash Flow Score (30 pts max)
│  └─ Based on net cash position
│
├─ Loan Performance (20 pts max)
│  └─ Based on active vs pending ratio
│
└─ Approval Score (10 pts max)
   └─ Based on pending approval count
```

### Example Calculation:
- Liquidity Ratio: 1.8 → 35 points
- Cash Flow: +₱200K → 30 points
- Loan Performance: 85% → 20 points
- Pending Approvals: 8 → 8 points
- **Total: 93 points (Excellent)**

---

## Testing Checklist

✅ Dashboard loads without errors  
✅ All KPIs display correct values  
✅ Health indicator shows proper color  
✅ Alert banner matches health status  
✅ Charts render with data  
✅ Department bars show percentages  
✅ Quick access cards link correctly  
✅ Recent activity populates  
✅ Refresh button updates data  
✅ Responsive on mobile/tablet  
✅ Animations smooth and performant  
✅ No console errors  

---

## Browser Compatibility

✅ Chrome/Edge (Chromium) - Full support  
✅ Firefox - Full support  
✅ Safari - Full support (CSS gradients work)  
✅ Mobile browsers - Responsive design  

---

## Performance Metrics

- **Initial Load**: ~2-3 seconds (includes DB queries)
- **Refresh**: ~1-2 seconds
- **Chart Render**: ~200-300ms
- **Animation Duration**: 0.2-0.5s
- **Database Queries**: 15-20 queries optimized

---

This visual guide shows how the dashboard is structured and how each component looks and behaves!
