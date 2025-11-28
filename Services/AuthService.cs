using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FinanceBank.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<BFASDbContext>? _contextFactory;
        
        public bool IsAuthenticated { get; private set; }
        public int? CurrentUserId { get; private set; }
        public string? CurrentUser { get; private set; }
        public string? CurrentRole { get; private set; }
        public string? FullName { get; private set; }
        public string? Email { get; private set; }
        public int? EmployeeId { get; private set; }
        public string? Department { get; private set; }
        public List<string> Permissions { get; private set; } = new();
        
        // Event to notify when authentication state changes
        public event Action? OnAuthStateChanged;

        // Constructor for dependency injection with DbContextFactory
        public AuthService(IDbContextFactory<BFASDbContext>? contextFactory = null)
        {
            _contextFactory = contextFactory;
        }

        // 6 ROLES
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Admin = "Admin";
            public const string Accountant = "Accountant";
            public const string FinanceManager = "FinanceManager";
            public const string Teller = "Teller";
            public const string Customer = "Customer";
        }

        // role permission nilang 5 
        public static class Modules
        {
            public const string Dashboard = "Dashboard";
            public const string Banking = "Banking";
            public const string Accounting = "Accounting";
            public const string Finance = "Finance";
            public const string Transactions = "Transactions";
            public const string Accounts = "Accounts";
            public const string Users = "Users";
            public const string Reports = "Reports";
            public const string Settings = "Settings";
            public const string Approvals = "Approvals";
            public const string Loans = "Loans";
            public const string Cards = "Cards";
            public const string Bills = "Bills";
            public const string Security = "Security";
            public const string Audit = "Audit";
        }

        // Authenticate user with database
        public async Task<(bool success, string message)> AuthenticateAsync(string username, string password, string? ipAddress = null, string? userAgent = null)
        {
            if (_contextFactory == null)
            {
                // Fallback to simple authentication if no database
                return AuthenticateSimple(username, password);
            }

            try
            {
                // Create a new DbContext instance for this operation
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Find user by username (include Employee data)
                var user = await context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user == null)
                {
                    // Log failed attempt
                    await LogFailedLoginAsync(username, "Invalid credentials", ipAddress, userAgent);
                    return (false, "Invalid username or password");
                }

                // Verify password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    await LogFailedLoginAsync(username, "Invalid password", ipAddress, userAgent);
                    return (false, "Invalid username or password");
                }

                // Successful login
                await LoginAsyncInternal(user, ipAddress, userAgent);
                return (true, "Login successful");
            }
            catch (Exception ex)
            {
                // If database connection fails, fall back to simple authentication
                if (ex.Message.Contains("SQL Server") || ex.Message.Contains("database") || ex.Message.Contains("connection"))
                {
                    return AuthenticateSimple(username, password);
                }
                return (false, $"Authentication error: {ex.Message}");
            }
        }

        // Simple authentication (fallback when no database)
        private (bool success, string message) AuthenticateSimple(string username, string password)
        {
            var validCredentials = new Dictionary<string, (string password, string role, string? fullName, string? dept, int? empId)>
            {
                ["admin"] = ("admin123", Roles.SuperAdmin, "System Admin", "IT", 1),
                ["accountant"] = ("accountant123", Roles.Accountant, "John Accountant", "Accounting", 2),
                ["financemanager"] = ("financemanager123", Roles.FinanceManager, "Jane Finance", "Finance", 3),
                ["teller"] = ("teller123", Roles.Teller, "Anna Teller", "Teller", 4),
                ["customer"] = ("customer123", Roles.Customer, "Pedro Garcia", null, null)
            };

            if (validCredentials.TryGetValue(username, out var creds) && creds.password == password)
            {
                Login(username, creds.role, creds.fullName, creds.dept, creds.empId);
                return (true, "Login successful (offline mode)");
            }

            return (false, "Invalid username or password");
        }

        // Login with database user (public for service-to-service calls)
        public async Task<bool> LoginAsync(string username, string password, string? ipAddress = null, string? userAgent = null)
        {
            if (_contextFactory == null)
                return false;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var user = await context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return false;

                // Set authentication properties
                IsAuthenticated = true;
                CurrentUserId = user.UserId;
                CurrentUser = user.Username;
                CurrentRole = user.Role;
                FullName = user.FullName;
                Email = user.Email;
                
                // Get employee info if user is an employee
                if (user.Employee != null)
                {
                    EmployeeId = user.Employee.EmployeeId;
                    Department = user.Employee.Department;
                }
                else
                {
                    EmployeeId = null;
                    Department = null;
                }
                
                Permissions = GetPermissionsForRole(user.Role);

                // Update last login time
                user.LastLoginAt = DateTime.Now;
                await context.SaveChangesAsync();

                // Log successful login
                var loginHistory = new LoginHistory
                {
                    UserId = user.UserId,
                    LoginTime = DateTime.Now,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    LoginStatus = "Success"
                };
                context.LoginHistories.Add(loginHistory);
                await context.SaveChangesAsync();
                
                // Notify authentication state changed
                OnAuthStateChanged?.Invoke();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Internal login with database user (private helper)
        private async Task LoginAsyncInternal(AuthUser user, string? ipAddress, string? userAgent)
        {
            IsAuthenticated = true;
            CurrentUserId = user.UserId;
            CurrentUser = user.Username;
            CurrentRole = user.Role;
            FullName = user.FullName;
            Email = user.Email;
            
            // Get employee info if user is an employee
            if (user.Employee != null)
            {
                EmployeeId = user.Employee.EmployeeId;
                Department = user.Employee.Department;
            }
            else
            {
                EmployeeId = null;
                Department = null;
            }
            
            Permissions = GetPermissionsForRole(user.Role);

            if (_contextFactory != null)
            {
                try
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    
                    // Attach and update the user
                    context.Users.Attach(user);
                    user.LastLoginAt = DateTime.Now;
                    context.Entry(user).Property(u => u.LastLoginAt).IsModified = true;
                    await context.SaveChangesAsync();

                    // Log successful login
                    var loginHistory = new LoginHistory
                    {
                        UserId = user.UserId,
                        LoginTime = DateTime.Now,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        LoginStatus = "Success"
                    };
                    context.LoginHistories.Add(loginHistory);
                    await context.SaveChangesAsync();
                }
                catch { /* Ignore logging errors */ }
            }
            
            // Notify authentication state changed
            OnAuthStateChanged?.Invoke();
        }

        // Simple login (without database)
        public void Login(string username, string role, string? fullName = null, string? department = null, int? employeeId = null)
        {
            IsAuthenticated = true;
            CurrentUser = username;
            CurrentRole = role;
            FullName = fullName;
            Department = department;
            EmployeeId = employeeId;
            Permissions = GetPermissionsForRole(role);
            
            // Notify authentication state changed
            OnAuthStateChanged?.Invoke();
        }

        // Log failed login attempt
        private async Task LogFailedLoginAsync(string username, string reason, string? ipAddress, string? userAgent)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    var loginHistory = new LoginHistory
                    {
                        UserId = user.UserId,
                        LoginTime = DateTime.Now,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        LoginStatus = "Failed",
                        FailureReason = reason
                    };
                    context.LoginHistories.Add(loginHistory);
                    await context.SaveChangesAsync();
                }
            }
            catch { /* Ignore logging errors */ }
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUserId = null;
            CurrentUser = null;
            CurrentRole = null;
            FullName = null;
            Email = null;
            EmployeeId = null;
            Department = null;
            Permissions.Clear();
            
            // Notify authentication state changed
            OnAuthStateChanged?.Invoke();
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
                    Modules.Dashboard, Modules.Banking, Modules.Accounting, Modules.Finance,
                    Modules.Transactions, Modules.Accounts, Modules.Users, Modules.Reports,
                    Modules.Settings, Modules.Approvals, Modules.Loans, Modules.Cards,
                    Modules.Bills, Modules.Security, Modules.Audit
                },
                Roles.Accountant => new List<string>
                {
                    Modules.Dashboard, Modules.Banking, Modules.Accounting,
                    Modules.Transactions, Modules.Reports, Modules.Accounts
                },
                Roles.FinanceManager => new List<string>
                {
                    Modules.Dashboard, Modules.Finance, Modules.Accounting, Modules.Reports
                },
                Roles.Teller => new List<string>
                {
                    Modules.Dashboard, Modules.Banking, Modules.Transactions, Modules.Accounts
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
        public static string GetRoleDisplayName(string? role)
        {
            return role switch
            {
                Roles.SuperAdmin => "Super Administrator",
                Roles.Accountant => "Accountant",
                Roles.FinanceManager => "Finance Manager",
                Roles.Teller => "Teller",
                Roles.Customer => "Customer",
                _ => role ?? "Unknown Role"
            };
        }

        // Check if user can access a specific page/route
        public bool CanAccessRoute(string route)
        {
            if (!IsAuthenticated) return false;
            if (CurrentRole == Roles.SuperAdmin) return true; // SuperAdmin can access everything

            // Route-based access control
            var routeLower = route.ToLower();

            // Teller routes
            if (routeLower.Contains("/teller/"))
                return CurrentRole == Roles.Teller;

            // Customer routes
            if (routeLower.Contains("/customer/"))
                return CurrentRole == Roles.Customer;

            // Admin routes - check specific modules
            if (routeLower.Contains("/admin/"))
            {
                if (routeLower.Contains("/banking/"))
                    return HasPermission(Modules.Banking);
                
                if (routeLower.Contains("/accounting/"))
                    return HasPermission(Modules.Accounting);
                
                if (routeLower.Contains("/finance/"))
                    return HasPermission(Modules.Finance);
                
                if (routeLower.Contains("/system/"))
                    return HasPermission(Modules.Reports);
                
                if (routeLower.Contains("/approvals/"))
                    return HasPermission(Modules.Approvals);

                // Default admin dashboard access
                if (routeLower.Contains("/dashboard"))
                    return HasAnyRole(Roles.SuperAdmin, Roles.Accountant, Roles.FinanceManager);
            }

            return true; // Allow access to public routes
        }
    }
}

