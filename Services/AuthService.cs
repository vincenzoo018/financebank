namespace FinanceBank.Services
{
    public class AuthService
    {
        public bool IsAuthenticated { get; private set; }
        public string? CurrentUser { get; private set; }
        public string? CurrentRole { get; private set; }
        public string? EmployeeId { get; private set; }
        public string? Department { get; private set; }
        public List<string> Permissions { get; private set; } = new();

        // Define all roles in the ERP system (4 roles only)
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Accountant = "Accountant";
            public const string FinanceManager = "FinanceManager";
            public const string Customer = "Customer";
        }

        // Define permission modules
        public static class Modules
        {
            public const string Dashboard = "Dashboard";
            public const string Transactions = "Transactions";
            public const string Accounts = "Accounts";
            public const string Users = "Users";
            public const string Reports = "Reports";
            public const string Settings = "Settings";
            public const string Accounting = "Accounting";
            public const string HRPayroll = "HRPayroll";
            public const string CRM = "CRM";
            public const string Loans = "Loans";
            public const string Cards = "Cards";
            public const string Bills = "Bills";
            public const string Security = "Security";
            public const string Audit = "Audit";
        }

        public void Login(string username, string role, string? employeeId = null, string? department = null)
        {
            IsAuthenticated = true;
            CurrentUser = username;
            CurrentRole = role;
            EmployeeId = employeeId;
            Department = department;
            Permissions = GetPermissionsForRole(role);
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUser = null;
            CurrentRole = null;
            EmployeeId = null;
            Department = null;
            Permissions.Clear();
        }

        // Check if user has permission to access a module
        public bool HasPermission(string module)
        {
            return Permissions.Contains(module);
        }

        // Check if user has specific role
        public bool HasRole(string role)
        {
            return CurrentRole == role;
        }

        // Check if user has any of the specified roles
        public bool HasAnyRole(params string[] roles)
        {
            return roles.Contains(CurrentRole);
        }

        // Get permissions based on role
        public static List<string> GetPermissionsForRole(string role)
        {
            return role switch
            {
                Roles.SuperAdmin => new List<string>
                {
                    Modules.Dashboard, Modules.Transactions, Modules.Accounts, Modules.Users,
                    Modules.Reports, Modules.Settings, Modules.Accounting, Modules.HRPayroll,
                    Modules.CRM, Modules.Loans, Modules.Cards, Modules.Bills, Modules.Security, Modules.Audit
                },
                Roles.Accountant => new List<string>
                {
                    Modules.Dashboard, Modules.Accounting, Modules.Transactions, Modules.Reports, Modules.Accounts
                },
                Roles.FinanceManager => new List<string>
                {
                    Modules.Dashboard, Modules.Accounting, Modules.Reports
                },
                Roles.Customer => new List<string>
                {
                    Modules.Dashboard, Modules.Transactions, Modules.Accounts,
                    Modules.Loans, Modules.Cards, Modules.Bills, Modules.Settings
                },
                _ => new List<string>()
            };
        }

        // Role display names
        public static string GetRoleDisplayName(string role)
        {
            return role switch
            {
                Roles.SuperAdmin => "Super Administrator",
                Roles.Accountant => "Accountant",
                Roles.FinanceManager => "Finance Manager",
                Roles.Customer => "Customer",
                _ => role
            };
        }
    }
}
