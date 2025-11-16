using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models
{
    // User table for authentication
    [Table("Users")]
    public class AuthUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        [MaxLength(50)]
        public string? Department { get; set; }

        [MaxLength(50)]
        public string? EmployeeId { get; set; }

        // Navigation properties
        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    }

    // Login history table
    [Table("LoginHistory")]
    public class LoginHistory
    {
        [Key]
        public int LoginId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime LoginTime { get; set; } = DateTime.Now;

        public DateTime? LogoutTime { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(255)]
        public string? UserAgent { get; set; }

        [MaxLength(20)]
        public string LoginStatus { get; set; } = "Success"; // Success, Failed, Locked

        [MaxLength(255)]
        public string? FailureReason { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual AuthUser User { get; set; } = null!;
    }

    // User sessions table
    [Table("UserSessions")]
    public class UserSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SessionToken { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(255)]
        public string? UserAgent { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual AuthUser User { get; set; } = null!;
    }

    // Role permissions table
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Permission { get; set; } = string.Empty; // Read, Write, Delete, Full

        public bool IsActive { get; set; } = true;
    }
}

