using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services
{
    public class UserCrudService
    {
        private readonly BFASDbContext _dbContext;

        public UserCrudService(BFASDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// CREATE - Add a new user to the database
        /// </summary>
        public async Task<(bool success, string message, int userId)> CreateUserAsync(
            string username,
            string password,
            string fullName,
            string email,
            string phoneNumber,
            string role,
            string? department = null,
            string? employeeId = null,
            bool isActive = true)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(username))
                    return (false, "Username is required", 0);

                if (string.IsNullOrWhiteSpace(password))
                    return (false, "Password is required", 0);

                if (string.IsNullOrWhiteSpace(fullName))
                    return (false, "Full name is required", 0);

                // Check if username already exists
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (existingUser != null)
                    return (false, "Username already exists", 0);

                // Create new user
                var newUser = new AuthUser
                {
                    Username = username,
                    PasswordHash = password,
                    Role = role,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Department = department,
                    EmployeeId = employeeId,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                return (true, "User created successfully", newUser.UserId);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating user: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// READ - Get all users
        /// </summary>
        public async Task<List<AuthUser>> GetAllUsersAsync()
        {
            try
            {
                return await _dbContext.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return new List<AuthUser>();
            }
        }

        /// <summary>
        /// READ - Get user by ID
        /// </summary>
        public async Task<AuthUser?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// READ - Get user by username
        /// </summary>
        public async Task<AuthUser?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// READ - Get users by role
        /// </summary>
        public async Task<List<AuthUser>> GetUsersByRoleAsync(string role)
        {
            try
            {
                return await _dbContext.Users
                    .Where(u => u.Role == role)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users by role: {ex.Message}");
                return new List<AuthUser>();
            }
        }

        /// <summary>
        /// UPDATE - Update user information
        /// </summary>
        public async Task<(bool success, string message)> UpdateUserAsync(
            int userId,
            string? fullName = null,
            string? email = null,
            string? phoneNumber = null,
            string? department = null,
            bool? isActive = null)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                if (!string.IsNullOrWhiteSpace(fullName))
                    user.FullName = fullName;

                if (!string.IsNullOrWhiteSpace(email))
                    user.Email = email;

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    user.PhoneNumber = phoneNumber;

                if (!string.IsNullOrWhiteSpace(department))
                    user.Department = department;

                if (isActive.HasValue)
                    user.IsActive = isActive.Value;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                return (true, "User updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user: {ex.Message}");
            }
        }

        /// <summary>
        /// UPDATE - Change user password
        /// </summary>
        public async Task<(bool success, string message)> ChangePasswordAsync(
            int userId,
            string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    return (false, "New password is required");

                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                user.PasswordHash = newPassword;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error changing password: {ex.Message}");
            }
        }

        /// <summary>
        /// UPDATE - Activate or deactivate user
        /// </summary>
        public async Task<(bool success, string message)> SetUserActiveStatusAsync(
            int userId,
            bool isActive)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                user.IsActive = isActive;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                var status = isActive ? "activated" : "deactivated";
                return (true, $"User {status} successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user status: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE - Delete a user
        /// </summary>
        public async Task<(bool success, string message)> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found");

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();

                return (true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting user: {ex.Message}");
            }
        }

        /// <summary>
        /// SEARCH - Search users by criteria
        /// </summary>
        public async Task<List<AuthUser>> SearchUsersAsync(
            string? searchTerm = null,
            string? role = null,
            bool? isActive = null)
        {
            try
            {
                var query = _dbContext.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u =>
                        u.Username.Contains(searchTerm) ||
                        (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                        (u.Email != null && u.Email.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                return await query
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
                return new List<AuthUser>();
            }
        }

        /// <summary>
        /// Get total user count
        /// </summary>
        public async Task<int> GetUserCountAsync()
        {
            try
            {
                return await _dbContext.Users.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting users: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get user count by role
        /// </summary>
        public async Task<Dictionary<string, int>> GetUserCountByRoleAsync()
        {
            try
            {
                var counts = await _dbContext.Users
                    .GroupBy(u => u.Role)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .ToListAsync();

                return counts.ToDictionary(x => x.Role, x => x.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting users by role: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }
    }
}
