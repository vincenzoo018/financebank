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

        public int? EmployeeId_FK { get; set; }

        [MaxLength(255)]
        public string? TransferPinHash { get; set; }

        // Profile Picture stored as binary data (VARBINARY in SQL Server)
        [Column(TypeName = "varbinary(max)")]
        public byte[]? ProfilePicture { get; set; }

        [MaxLength(50)]
        public string? ProfilePictureFileName { get; set; }

        [MaxLength(100)]
        public string? ProfilePictureContentType { get; set; }

        // Valid ID stored as binary data (VARBINARY in SQL Server)
        [Column(TypeName = "varbinary(max)")]
        public byte[]? ValidIdDocument { get; set; }

        [MaxLength(50)]
        public string? ValidIdFileName { get; set; }

        [MaxLength(100)]
        public string? ValidIdContentType { get; set; }

        // Navigation properties
        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

        // Employee relationship (optional - only for employee roles)
        [ForeignKey("EmployeeId_FK")]
        public virtual Employee? Employee { get; set; }
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

    // Employee table
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Position { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual AuthUser? User { get; set; }
    }
}

