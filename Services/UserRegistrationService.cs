using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FinanceBank.Services
{
    public class UserRegistrationService
    {
        private readonly BFASDbContext _dbContext;
        private readonly AuthService _authService;

        public UserRegistrationService(BFASDbContext dbContext, AuthService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new user account in the database and authenticates them
        /// </summary>
        public async Task<(bool success, string message, string accountNumber)> CreateUserAsync(
            string username,
            string password,
            string fullName,
            string email,
            string phoneNumber,
            string role,
            string? department = null,
            int? employeeId = null,
            bool isActive = true)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(username))
                    return (false, "Username is required", "");

                if (string.IsNullOrWhiteSpace(password))
                    return (false, "Password is required", "");

                if (string.IsNullOrWhiteSpace(fullName))
                    return (false, "Full name is required", "");

                // Check if username already exists in database
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (existingUser != null)
                    return (false, "Username already exists. Please choose a different username.", "");

                // Generate account number and employee ID
                string accountNumber = "";
                var generatedEmployeeId = employeeId ?? GenerateEmployeeId();

                // Validate and trim role
                var mappedRole = role?.Trim() ?? "Customer";

                // Validate role against database constraints
                var validRoles = new[] { "SuperAdmin", "Admin", "Accountant", "FinanceManager", "Teller", "Customer", "Registrar" };
                if (!validRoles.Contains(mappedRole))
                {
                    return (false, $"Invalid role: {role}. Allowed roles: SuperAdmin, Admin, Accountant, FinanceManager, Teller, Customer, Registrar", "");
                }

                // Create new user
                var newUser = new AuthUser
                {
                    Username = username,
                    PasswordHash = password,
                    Role = mappedRole,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now
                };

                // Add to database
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"[SUCCESS] User '{username}' saved to database successfully");
                Console.WriteLine($"  - Role: {mappedRole}");

                // If employee role, create employee record
                if (mappedRole != "Customer")
                {
                    var employee = new Employee
                    {
                        EmployeeId = generatedEmployeeId,
                        UserId = newUser.UserId,
                        FirstName = fullName.Split(' ')[0],
                        LastName = fullName.Contains(' ') ? string.Join(" ", fullName.Split(' ').Skip(1)) : "",
                        Department = department ?? "General",
                        IsActive = isActive,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Employees.Add(employee);
                    
                    newUser.EmployeeId_FK = generatedEmployeeId;
                    _dbContext.Users.Update(newUser);
                    await _dbContext.SaveChangesAsync();
                    
                    Console.WriteLine($"  - Employee ID: {generatedEmployeeId}");
                    Console.WriteLine($"  - Department: {department ?? "General"}");
                }
                
                // If customer role, create customer account
                if (mappedRole == "Customer")
                {
                    accountNumber = CustomerAccount.GenerateAccountNumber();
                    var customerAccount = new CustomerAccount
                    {
                        AccountNumber = accountNumber,
                        CustomerId = newUser.UserId,
                        Balance = 0,
                        AvailableBalance = 0,
                        Currency = "PHP",
                        IsActive = isActive,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.CustomerAccounts.Add(customerAccount);
                    await _dbContext.SaveChangesAsync();
                    
                    Console.WriteLine($"  - Account Number: {accountNumber}");
                    Console.WriteLine($"  - Initial Balance: 0.00");
                }

                // Authenticate the user immediately
                var authResult = await _authService.LoginAsync(username, password);
                if (authResult)
                {
                    Console.WriteLine($"[SUCCESS] User '{username}' authenticated successfully");
                }
                else
                {
                    Console.WriteLine($"[WARNING] User created but authentication failed. User can log in manually.");
                }

                // Success - return account number
                return (true, "Account created successfully and authenticated", accountNumber);
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"[ERROR] Database error creating user: {errorMessage}");
                
                // Check for CHECK constraint violation
                if (errorMessage.Contains("CK_Users_Role") || errorMessage.Contains("CHECK constraint"))
                {
                    return (false, $"Invalid role. Allowed roles: SuperAdmin, Admin, Accountant, FinanceManager, Teller, Customer, Registrar", "");
                }
                
                return (false, $"Database error: {errorMessage}", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error creating user: {ex.Message}");
                return (false, $"Error creating user: {ex.Message}", "");
            }
        }

        /// <summary>
        /// Generates a unique account number
        /// </summary>
        private string GenerateAccountNumber()
        {
            var year = DateTime.Now.Year;
            var randomPart = new Random().Next(1, 99999).ToString("D5");
            return $"ACC-{year}-{randomPart}";
        }

        /// <summary>
        /// Generates a unique employee ID automatically
        /// Returns the next available integer ID
        /// </summary>
        private int GenerateEmployeeId()
        {
            var maxId = _dbContext.Employees.Max(e => (int?)e.EmployeeId) ?? 0;
            return maxId + 1;
        }

    }
}
