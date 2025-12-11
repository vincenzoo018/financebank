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

        // 7 ROLES
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Admin = "Admin";
            public const string Accountant = "Accountant";
            public const string FinanceManager = "FinanceManager";
            public const string Teller = "Teller";
            public const string Customer = "Customer";
            public const string Registrar = "Registrar";
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

        /// <summary>
        /// Verify a user's password WITHOUT logging them in.
        /// Used for customer verification during teller transactions.
        /// </summary>
        public async Task<(bool success, string message)> VerifyPasswordOnlyAsync(string username, string password)
        {
            if (_contextFactory == null)
            {
                return (false, "Database not available");
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Find user by username
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user == null)
                {
                    return (false, "User not found");
                }

                // Verify password using BCrypt - DO NOT LOG IN
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    return (false, "Invalid password");
                }

                // Password is valid - but do NOT change CurrentUser or session
                return (true, "Password verified");
            }
            catch (Exception ex)
            {
                return (false, $"Verification error: {ex.Message}");
            }
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
                    // Log failed attempt to LoginHistory
                    await LogFailedLoginAsync(username, "Invalid credentials", ipAddress, userAgent);
                    // Log failed attempt to AuditLogs
                    await LogAuditFailedLoginAsync(username, "User not found or inactive", ipAddress ?? "Unknown", userAgent ?? "Unknown");
                    return (false, "Invalid username or password");
                }

                // Check if account is fully locked (2 failed PIN attempts - needs SuperAdmin)
                var isFullyLocked = await context.AuditLogs
                    .Where(a => a.CustomerAccountId == user.UserId &&
                               a.Action == "PIN_VERIFICATION_FAILED" &&
                               a.PinAttemptCount >= 2 &&
                               a.CreatedAt >= DateTime.Now.AddHours(-24))
                    .AnyAsync();

                if (isFullyLocked)
                {
                    // Check if SuperAdmin unlocked
                    var adminUnlock = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "ADMIN_ACCOUNT_UNLOCK" &&
                                   a.CreatedAt >= DateTime.Now.AddHours(-24))
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    var lastPinFailure = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "PIN_VERIFICATION_FAILED" &&
                                   a.CreatedAt >= DateTime.Now.AddHours(-24))
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (adminUnlock == null || (lastPinFailure != null && adminUnlock.CreatedAt < lastPinFailure.CreatedAt))
                    {
                        return (false, "ACCOUNT_FULLY_LOCKED");
                    }
                }

                // Check if account is locked (5 failed password attempts - needs PIN)
                var recentMaliciousAttempt = await context.AuditLogs
                    .Where(a => a.CustomerAccountId == user.UserId &&
                               a.IsAccountLocked == true &&
                               a.Action == "MALICIOUS_ATTEMPT" &&
                               a.CreatedAt >= DateTime.Now.AddHours(-24))
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (recentMaliciousAttempt != null)
                {
                    // Check if there's a subsequent unlock
                    var wasUnlocked = await context.AuditLogs
                        .AnyAsync(a => a.CustomerAccountId == user.UserId &&
                                      (a.Action == "ACCOUNT_UNLOCKED" || a.Action == "ADMIN_ACCOUNT_UNLOCK") &&
                                      a.CreatedAt > recentMaliciousAttempt.CreatedAt);

                    if (!wasUnlocked)
                    {
                        await LogAuditFailedLoginAsync(username, "Account locked - requires PIN verification", ipAddress ?? "Unknown", userAgent ?? "Unknown");
                        return (false, "ACCOUNT_LOCKED");
                    }
                }

                // Verify password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    // Find the last time the account was unlocked or password reset (counter reset point)
                    var lastCounterReset = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   (a.Action == "ACCOUNT_UNLOCKED" || 
                                    a.Action == "ADMIN_ACCOUNT_UNLOCK" || 
                                    a.Action == "PASSWORD_RESET" ||
                                    a.Action == "LOGIN_SUCCESS"))
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    // Count failed attempts only since the last counter reset (or last hour if no reset)
                    var countAfterTime = lastCounterReset > DateTime.MinValue 
                        ? lastCounterReset 
                        : DateTime.Now.AddHours(-1);

                    var failedAttempts = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "LOGIN_FAILED" &&
                                   a.CreatedAt > countAfterTime)
                        .CountAsync();

                    failedAttempts++; // Include current attempt

                    // Check if customer has exceeded 5 failed attempts
                    if (user.Role == Roles.Customer && failedAttempts >= 5)
                    {
                        // Log as malicious attempt
                        await LogMaliciousAttemptAsync(user.UserId.ToString(), user.Username, "5 failed login attempts", ipAddress, userAgent, user.UserId);

                        await LogFailedLoginAsync(username, "Account locked after 5 failed attempts", ipAddress, userAgent);
                        return (false, "MALICIOUS_ATTEMPT");
                    }

                    await LogFailedLoginAsync(username, "Invalid password", ipAddress, userAgent);
                    // Log failed password to AuditLogs with new columns
                    await LogAuditFailedLoginWithColumnsAsync(user.UserId, username, $"Invalid password (Attempt {failedAttempts}/5)", ipAddress ?? "Unknown", userAgent ?? "Unknown", failedAttempts);

                    int remainingAttempts = 5 - failedAttempts;
                    if (user.Role == Roles.Customer && remainingAttempts > 0 && remainingAttempts <= 2)
                    {
                        return (false, $"Invalid password. {remainingAttempts} attempt(s) remaining before account is locked.");
                    }
                    return (false, "Invalid username or password");
                }

                // Successful login
                await LoginAsyncInternal(user, ipAddress, userAgent);
                // Log successful login to AuditLogs
                await LogAuditSuccessfulLoginAsync(user.UserId.ToString(), user.FullName ?? user.Username, ipAddress ?? "Unknown", userAgent ?? "Unknown");
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
                ["registrar"] = ("registrar123", Roles.Registrar, "Maria Registrar", "Registration", 5),
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

        public async Task LogoutAsync()
        {
            // Log logout to AuditLogs before clearing session
            if (_contextFactory != null && CurrentUserId.HasValue)
            {
                await LogAuditLogoutAsync(CurrentUserId.Value.ToString(), FullName ?? CurrentUser ?? "Unknown");
            }

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

        // ==================== AUDIT LOGGING HELPERS ====================

        /// <summary>
        /// Log successful login to AuditLogs table
        /// </summary>
        private async Task LogAuditSuccessfulLoginAsync(string userId, string userName, string ipAddress, string userAgent)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var auditService = new AuditLogService(context);
                await auditService.LogLoginSuccessAsync(userId, userName, ipAddress, userAgent);
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Log failed login attempt to AuditLogs table
        /// </summary>
        private async Task LogAuditFailedLoginAsync(string attemptedUserId, string reason, string ipAddress, string userAgent)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var auditService = new AuditLogService(context);
                await auditService.LogLoginFailureAsync(attemptedUserId, reason, ipAddress, userAgent);
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Log failed login attempt with new security columns
        /// </summary>
        private async Task LogAuditFailedLoginWithColumnsAsync(int userId, string username, string reason, string ipAddress, string userAgent, int failedAttemptCount)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var auditLog = new AuditLog
                {
                    UserId = userId.ToString(),
                    Action = "LOGIN_FAILED",
                    Module = "Authentication",
                    Description = $"Failed login attempt for '{username}': {reason}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.Now,
                    // New security columns
                    IsMalicious = failedAttemptCount >= 3,
                    CustomerAccountId = userId,
                    AccountStatus = failedAttemptCount >= 5 ? "Locked" : "Active",
                    FailedAttemptCount = failedAttemptCount,
                    PinAttemptCount = 0,
                    IsAccountLocked = failedAttemptCount >= 5
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Log malicious attempt to AuditLogs (5+ failed login attempts)
        /// Uses new security columns: IsMalicious, CustomerAccountId, AccountStatus, IsAccountLocked, LockReason
        /// </summary>
        private async Task LogMaliciousAttemptAsync(string userId, string username, string reason, string? ipAddress, string? userAgent, int? customerAccountId = null)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "MALICIOUS_ATTEMPT",
                    Module = "Security",
                    Description = $"[LOCKED] Malicious login attempt detected for user '{username}'. Reason: {reason}",
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent ?? "Unknown",
                    CreatedAt = DateTime.Now,
                    // New security columns
                    IsMalicious = true,
                    CustomerAccountId = customerAccountId,
                    AccountStatus = "Locked",
                    FailedAttemptCount = 5,
                    PinAttemptCount = 0,
                    IsAccountLocked = true,
                    LockReason = $"5 failed login attempts - {reason}"
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Verify customer's 4-digit security PIN (uses TransferPinHash)
        /// Tracks PIN attempts (max 2 attempts before full account lock)
        /// </summary>
        public async Task<(bool success, string message, int attemptsRemaining)> VerifySecurityPinAsync(string username, string pin)
        {
            if (_contextFactory == null) return (false, "Database not available", 0);

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return (false, "User not found", 0);

                if (string.IsNullOrEmpty(user.TransferPinHash))
                    return (false, "NO_PIN_SET", 0);

                // Count recent PIN attempts
                var recentPinAttempts = await context.AuditLogs
                    .Where(a => a.CustomerAccountId == user.UserId &&
                               a.Action == "PIN_VERIFICATION_FAILED" &&
                               a.CreatedAt >= DateTime.Now.AddHours(-24))
                    .CountAsync();

                // Check if already fully locked (2 failed PIN attempts)
                if (recentPinAttempts >= 2)
                {
                    return (false, "ACCOUNT_FULLY_LOCKED", 0);
                }

                // Verify PIN using BCrypt
                bool isPinValid = BCrypt.Net.BCrypt.Verify(pin, user.TransferPinHash);
                if (!isPinValid)
                {
                    recentPinAttempts++;
                    int attemptsRemaining = 2 - recentPinAttempts;

                    // Log failed PIN attempt
                    var failedPinLog = new AuditLog
                    {
                        UserId = user.UserId.ToString(),
                        Action = "PIN_VERIFICATION_FAILED",
                        Module = "Security",
                        Description = $"Failed PIN verification for user '{username}' (Attempt {recentPinAttempts}/2)",
                        CreatedAt = DateTime.Now,
                        IsMalicious = true,
                        CustomerAccountId = user.UserId,
                        AccountStatus = attemptsRemaining <= 0 ? "FullyLocked" : "PinPending",
                        PinAttemptCount = recentPinAttempts,
                        IsAccountLocked = attemptsRemaining <= 0,
                        LockReason = attemptsRemaining <= 0 ? "2 failed PIN attempts - requires SuperAdmin intervention" : null
                    };
                    context.AuditLogs.Add(failedPinLog);
                    await context.SaveChangesAsync();

                    if (attemptsRemaining <= 0)
                    {
                        return (false, "ACCOUNT_FULLY_LOCKED", 0);
                    }

                    return (false, $"Invalid security PIN. {attemptsRemaining} attempt(s) remaining.", attemptsRemaining);
                }

                // Log successful PIN verification
                var successLog = new AuditLog
                {
                    UserId = user.UserId.ToString(),
                    Action = "PIN_VERIFICATION_SUCCESS",
                    Module = "Security",
                    Description = $"Successful PIN verification for user '{username}'",
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = user.UserId,
                    AccountStatus = "Active",
                    PinAttemptCount = 0,
                    IsAccountLocked = false
                };
                context.AuditLogs.Add(successLog);
                await context.SaveChangesAsync();

                return (true, "PIN verified successfully", 2);
            }
            catch (Exception ex)
            {
                return (false, $"Verification error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Reset customer password after PIN verification
        /// </summary>
        public async Task<(bool success, string message)> ResetPasswordWithPinAsync(string username, string newPassword)
        {
            if (_contextFactory == null) return (false, "Database not available");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return (false, "User not found");

                // Hash and update the new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await context.SaveChangesAsync();

                // Log password reset with new security columns
                var auditLog = new AuditLog
                {
                    UserId = user.UserId.ToString(),
                    Action = "PASSWORD_RESET",
                    Module = "Security",
                    Description = $"Password reset via security PIN for user '{username}'",
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = user.UserId,
                    AccountStatus = "Active",
                    FailedAttemptCount = 0,
                    PinAttemptCount = 0,
                    IsAccountLocked = false
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();

                return (true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Reset error: {ex.Message}");
            }
        }

        /// <summary>
        /// Unlock account after PIN verification (clears all security flags)
        /// </summary>
        public async Task<(bool success, string message)> UnlockAccountWithPinAsync(string username)
        {
            if (_contextFactory == null) return (false, "Database not available");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return (false, "User not found");

                // Log account unlock with new security columns
                var auditLog = new AuditLog
                {
                    UserId = user.UserId.ToString(),
                    Action = "ACCOUNT_UNLOCKED",
                    Module = "Security",
                    Description = $"Account unlocked via security PIN for user '{username}'",
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = user.UserId,
                    AccountStatus = "Active",
                    FailedAttemptCount = 0,
                    PinAttemptCount = 0,
                    IsAccountLocked = false
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();

                return (true, "Account unlocked successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Unlock error: {ex.Message}");
            }
        }

        /// <summary>
        /// SuperAdmin: Unlock a fully locked account (after 2 failed PIN attempts)
        /// </summary>
        public async Task<(bool success, string message)> AdminUnlockAccountAsync(int userId, string adminUsername, string? newPassword = null)
        {
            if (_contextFactory == null) return (false, "Database not available");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                // Update password if provided
                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }

                await context.SaveChangesAsync();

                // Log admin unlock with new security columns
                var auditLog = new AuditLog
                {
                    UserId = userId.ToString(),
                    Action = "ADMIN_ACCOUNT_UNLOCK",
                    Module = "Security",
                    Description = $"Account unlocked by SuperAdmin '{adminUsername}' for user '{user.Username}'" +
                                 (!string.IsNullOrEmpty(newPassword) ? " - Password reset" : ""),
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = userId,
                    AccountStatus = "Active",
                    FailedAttemptCount = 0,
                    PinAttemptCount = 0,
                    IsAccountLocked = false,
                    LockReason = null
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();

                return (true, "Account unlocked by administrator");
            }
            catch (Exception ex)
            {
                return (false, $"Admin unlock error: {ex.Message}");
            }
        }

        /// <summary>
        /// SuperAdmin: Deactivate a user account
        /// </summary>
        public async Task<(bool success, string message)> AdminDeactivateAccountAsync(int userId, string adminUsername, string reason)
        {
            if (_contextFactory == null) return (false, "Database not available");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                user.IsActive = false;
                await context.SaveChangesAsync();

                // Log deactivation with new security columns
                var auditLog = new AuditLog
                {
                    UserId = userId.ToString(),
                    Action = "ADMIN_ACCOUNT_DEACTIVATE",
                    Module = "Security",
                    Description = $"Account deactivated by SuperAdmin '{adminUsername}' for user '{user.Username}'. Reason: {reason}",
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = userId,
                    AccountStatus = "Deactivated",
                    IsAccountLocked = true,
                    LockReason = reason
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();

                return (true, "Account deactivated");
            }
            catch (Exception ex)
            {
                return (false, $"Deactivation error: {ex.Message}");
            }
        }

        /// <summary>
        /// SuperAdmin: Activate a user account
        /// </summary>
        public async Task<(bool success, string message)> AdminActivateAccountAsync(int userId, string adminUsername)
        {
            if (_contextFactory == null) return (false, "Database not available");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                user.IsActive = true;
                await context.SaveChangesAsync();

                // Log activation with new security columns
                var auditLog = new AuditLog
                {
                    UserId = userId.ToString(),
                    Action = "ADMIN_ACCOUNT_ACTIVATE",
                    Module = "Security",
                    Description = $"Account activated by SuperAdmin '{adminUsername}' for user '{user.Username}'",
                    CreatedAt = DateTime.Now,
                    IsMalicious = false,
                    CustomerAccountId = userId,
                    AccountStatus = "Active",
                    IsAccountLocked = false,
                    LockReason = null
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();

                return (true, "Account activated");
            }
            catch (Exception ex)
            {
                return (false, $"Activation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set or update security PIN for a user (uses TransferPinHash)
        /// </summary>
        public async Task<(bool success, string message)> SetSecurityPinAsync(string username, string pin)
        {
            if (_contextFactory == null) return (false, "Database not available");

            if (pin.Length != 4 || !pin.All(char.IsDigit))
                return (false, "PIN must be exactly 4 digits");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return (false, "User not found");

                // Hash and store the PIN in TransferPinHash
                user.TransferPinHash = BCrypt.Net.BCrypt.HashPassword(pin);
                await context.SaveChangesAsync();

                return (true, "Security PIN set successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error setting PIN: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user's failed login attempts count from AuditLogs
        /// </summary>
        public async Task<int> GetFailedLoginAttemptsAsync(string username)
        {
            if (_contextFactory == null) return 0;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return 0;

                // Count recent failed attempts from AuditLogs
                var failedAttempts = await context.AuditLogs
                    .Where(a => a.UserId == user.UserId.ToString() &&
                               a.Action == "LOGIN_FAILED" &&
                               a.CreatedAt >= DateTime.Now.AddHours(-1))
                    .CountAsync();

                return failedAttempts;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if user account is locked (uses new IsAccountLocked column)
        /// </summary>
        public async Task<(bool isLocked, string lockType, string? reason)> IsAccountLockedAsync(string username)
        {
            if (_contextFactory == null) return (false, "", null);

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return (false, "", null);

                // Check for recent security events (within 24 hours)
                var recentLockEvent = await context.AuditLogs
                    .Where(a => a.CustomerAccountId == user.UserId &&
                               a.IsAccountLocked == true &&
                               a.CreatedAt >= DateTime.Now.AddHours(-24))
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (recentLockEvent != null)
                {
                    // Check if account was unlocked after the lock event
                    var unlockEvent = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "ACCOUNT_UNLOCKED" &&
                                   a.CreatedAt > recentLockEvent.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (unlockEvent == null)
                    {
                        // Determine lock type
                        string lockType = recentLockEvent.Action == "PIN_VERIFICATION_FAILED" && recentLockEvent.PinAttemptCount >= 2
                            ? "FULLY_LOCKED" // 2 failed PIN attempts - needs SuperAdmin
                            : "PIN_REQUIRED"; // 5 failed password attempts - needs PIN

                        return (true, lockType, recentLockEvent.LockReason);
                    }
                }

                return (false, "", null);
            }
            catch
            {
                return (false, "", null);
            }
        }

        /// <summary>
        /// Check if account is fully locked (requires SuperAdmin intervention)
        /// </summary>
        public async Task<bool> IsAccountFullyLockedAsync(string username)
        {
            if (_contextFactory == null) return false;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                // Check for 2 failed PIN attempts (fully locked)
                var pinFailures = await context.AuditLogs
                    .Where(a => a.CustomerAccountId == user.UserId &&
                               a.Action == "PIN_VERIFICATION_FAILED" &&
                               a.CreatedAt >= DateTime.Now.AddHours(-24))
                    .CountAsync();

                if (pinFailures >= 2)
                {
                    // Check if SuperAdmin unlocked after
                    var adminUnlock = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "ADMIN_ACCOUNT_UNLOCK" &&
                                   a.CreatedAt >= DateTime.Now.AddHours(-24))
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    var lastPinFailure = await context.AuditLogs
                        .Where(a => a.CustomerAccountId == user.UserId &&
                                   a.Action == "PIN_VERIFICATION_FAILED" &&
                                   a.CreatedAt >= DateTime.Now.AddHours(-24))
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    return adminUnlock == null || (lastPinFailure != null && adminUnlock.CreatedAt < lastPinFailure.CreatedAt);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Log failed password verification for teller transactions
        /// </summary>
        public async Task LogTellerTransactionFailedPasswordAsync(int customerId, string customerUsername, string tellerUsername, string transactionType)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Count recent failed attempts from AuditLogs
                var failedAttempts = await context.AuditLogs
                    .Where(a => a.UserId == customerId.ToString() &&
                               (a.Action == "FAILED_PASSWORD_VERIFICATION" || a.Action == "LOGIN_FAILED") &&
                               a.CreatedAt >= DateTime.Now.AddHours(-1))
                    .CountAsync();

                failedAttempts++; // Include current attempt

                // Check if 5 failed attempts reached
                if (failedAttempts >= 5)
                {
                    // Log as malicious attempt
                    var auditLog = new AuditLog
                    {
                        UserId = customerId.ToString(),
                        Action = "MALICIOUS_ATTEMPT",
                        Module = "Teller Banking",
                        Description = $"[LOCKED] UserId:{customerId} Malicious attempt: 5 failed password verifications during {transactionType} by teller '{tellerUsername}' for customer '{customerUsername}'",
                        CreatedAt = DateTime.Now
                    };
                    context.AuditLogs.Add(auditLog);
                }
                else
                {
                    // Log regular failed attempt
                    var auditLog = new AuditLog
                    {
                        UserId = customerId.ToString(),
                        Action = "FAILED_PASSWORD_VERIFICATION",
                        Module = "Teller Banking",
                        Description = $"UserId:{customerId} Failed password verification during {transactionType} (Attempt {failedAttempts}/5) by teller '{tellerUsername}'",
                        CreatedAt = DateTime.Now
                    };
                    context.AuditLogs.Add(auditLog);
                }

                await context.SaveChangesAsync();
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Reset customer's failed attempts after successful transaction (logs success to clear attempts)
        /// </summary>
        public async Task ResetFailedAttemptsAsync(int customerId)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Log successful verification to mark end of failed attempt series
                var auditLog = new AuditLog
                {
                    UserId = customerId.ToString(),
                    Action = "SUCCESSFUL_VERIFICATION",
                    Module = "Security",
                    Description = $"UserId:{customerId} Successful password verification - failed attempts reset",
                    CreatedAt = DateTime.Now
                };
                context.AuditLogs.Add(auditLog);
                await context.SaveChangesAsync();
            }
            catch { /* Ignore errors */ }
        }

        /// <summary>
        /// Log logout to AuditLogs table
        /// </summary>
        private async Task LogAuditLogoutAsync(string userId, string userName)
        {
            if (_contextFactory == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var auditService = new AuditLogService(context);
                await auditService.LogLogoutAsync(userId, userName);
            }
            catch { /* Ignore logging errors */ }
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
                Roles.Registrar => new List<string>
                {
                    Modules.Dashboard, Modules.Users, Modules.Accounts
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
                Roles.Registrar => "Registrar",
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

            // Registrar routes
            if (routeLower.Contains("/registrar/"))
                return CurrentRole == Roles.Registrar;

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

