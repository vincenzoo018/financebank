using FinanceBank.Services;

namespace FinanceBank.Services;

public class RoleBasedNavigationService
{
    private readonly AuthService _authService;

    public RoleBasedNavigationService(AuthService authService)
    {
        _authService = authService;
    }

    // Define what pages each role can access
    public Dictionary<string, List<string>> GetAllowedPages()
    {
        return new Dictionary<string, List<string>>
        {
            [AuthService.Roles.SuperAdmin] = new List<string>
            {
                // Dashboard
                "/admin/dashboard",
                
                // Banking Module - Full Access
                "/admin/banking/accounts",
                "/admin/banking/fund-transfer",
                "/admin/banking/loan-management",
                "/admin/banking/billers-management",
                "/admin/banking/bill-payment",
                "/admin/banking/card-management",
                
                // Accounting Module - Full Access
                "/admin/accounting/general-ledger",
                "/admin/accounting/trial-balance",
                "/admin/accounting/financial-statements",
                "/admin/accounting/journal-entries",
                "/admin/accounting/chart-of-accounts",
                
                // Finance Module - Full Access
                "/admin/finance/budget-management",
                "/admin/finance/cashflow-analysis",
                "/admin/finance/financial-forecasting",
                "/admin/finance/payables",
                "/admin/finance/receivables",
                
                // System Module - Full Access
                "/admin/system/user-management",
                "/admin/system/user-activity-logs",
                "/admin/system/role-management",
                "/admin/system/security-center",
                "/admin/system/audit-logs",
                "/admin/system/system-settings",
                "/admin/system/accountant-reports",
                "/admin/system/finance-manager-reports",
                "/admin/system/login-history",
                "/admin/system/session-management",
                
                // Approval Module - Full Access
                "/admin/approvals/transfer-approvals",
                "/admin/approvals/deposit-approvals",
                "/admin/approvals/withdrawal-approvals",
                "/admin/approvals/loan-application-approvals",
                "/admin/approvals/card-application-approvals"
            },
            
            [AuthService.Roles.Admin] = new List<string>
            {
                // Dashboard
                "/admin/dashboard",
                
                // Banking Module - Full Access
                "/admin/banking/accounts",
                "/admin/banking/fund-transfer",
                "/admin/banking/loan-management",
                "/admin/banking/billers-management",
                "/admin/banking/bill-payment",
                "/admin/banking/card-management",
                
                // Accounting Module - Full Access
                "/admin/accounting/general-ledger",
                "/admin/accounting/trial-balance",
                "/admin/accounting/financial-statements",
                "/admin/accounting/journal-entries",
                "/admin/accounting/chart-of-accounts",
                
                // Finance Module - Full Access
                "/admin/finance/budget-management",
                "/admin/finance/cashflow-analysis",
                "/admin/finance/financial-forecasting",
                "/admin/finance/payables",
                "/admin/finance/receivables",
                
                // System Module - Full Access
                "/admin/system/user-management",
                "/admin/system/user-activity-logs",
                "/admin/system/role-management",
                "/admin/system/security-center",
                "/admin/system/audit-logs",
                "/admin/system/system-settings",
                
                // Approval Module - Full Access
                "/admin/approvals/transfer-approvals",
                "/admin/approvals/deposit-approvals",
                "/admin/approvals/withdrawal-approvals",
                "/admin/approvals/loan-application-approvals",
                "/admin/approvals/card-application-approvals"
            },
            
            [AuthService.Roles.Accountant] = new List<string>
            {
                // Dashboard
                "/admin/dashboard",
                
                // Banking Module - Full Access
                "/admin/banking/accounts",
                "/admin/banking/fund-transfer",
                "/admin/banking/loan-management",
                "/admin/banking/billers-management",
                
                // Accounting Module - Full Access
                "/admin/accounting/general-ledger",
                "/admin/accounting/trial-balance",
                "/admin/accounting/financial-statements",
                "/admin/accounting/journal-entries",
                
                // Reports - Accountant specific
                "/admin/system/accountant-reports",
                
                // Approval Module - Banking related
                "/admin/approvals/transfer-approvals",
                "/admin/approvals/deposit-approvals",
                "/admin/approvals/withdrawal-approvals"
            },
            
            [AuthService.Roles.FinanceManager] = new List<string>
            {
                // Dashboard
                "/admin/dashboard",
                
                // Finance Module - Full Access
                "/admin/finance/payables",
                "/admin/finance/receivables",
                "/admin/finance/budget-management",
                "/admin/finance/cashflow-analysis",
                "/admin/finance/financial-forecasting",
                
                // Accounting Module - Read Only
                "/admin/accounting/financial-statements",
                
                // Reports - Finance Manager specific
                "/admin/system/finance-manager-reports"
            },
            
            [AuthService.Roles.Customer] = new List<string>
            {
                // Customer Portal - Full Access
                "/customer/dashboard",
                "/customer/transfer",
                "/customer/transfer-money",
                "/customer/deposit",
                "/customer/withdraw",
                "/customer/bills",
                "/customer/cards",
                "/customer/loans",
                "/customer/transactions",
                "/customer/savings",
                "/customer/rewards",
                "/customer/profile",
                "/customer/settings"
            }
        };
    }

    public List<string> GetUserPages(string role)
    {
        var allPages = GetAllowedPages();
        return allPages.ContainsKey(role) ? allPages[role] : new List<string>();
    }

    public bool CanAccessPage(string role, string pagePath)
    {
        var allowedPages = GetUserPages(role);
        return allowedPages.Any(page => pagePath.StartsWith(page, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsCurrentUserAuthorized(string pagePath)
    {
        if (!_authService.IsAuthenticated)
            return false;

        return CanAccessPage(_authService.CurrentRole ?? "", pagePath);
    }
}

