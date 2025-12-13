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
        public string? TransferPinHash { get; set; }  // Also used as Security PIN for account recovery

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

        // =====================================================================
        // EMPLOYMENT AND INCOME VERIFICATION FIELDS (For Account Opening)
        // =====================================================================

        [MaxLength(50)]
        public string? EmploymentStatus { get; set; } // Employed, Self-Employed, Business Owner, Student, Retired, Unemployed

        [MaxLength(200)]
        public string? EmployerName { get; set; }

        [MaxLength(500)]
        public string? EmployerAddress { get; set; }

        [MaxLength(20)]
        public string? EmployerContactNumber { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public int? YearsAtCurrentJob { get; set; }

        public int? MonthsAtCurrentJob { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyGrossIncome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualIncome { get; set; }

        [MaxLength(200)]
        public string? OtherIncomeSource { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherIncomeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalMonthlyIncome { get; set; } // Combined income from all sources

        // Government IDs
        [MaxLength(20)]
        public string? TaxIdentificationNumber { get; set; } // TIN

        [MaxLength(20)]
        public string? SSSNumber { get; set; }

        [MaxLength(20)]
        public string? PagIbigNumber { get; set; }

        [MaxLength(20)]
        public string? PhilHealthNumber { get; set; }

        // Personal Details
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? CivilStatus { get; set; } // Single, Married, Widowed, Separated

        [MaxLength(50)]
        public string? Nationality { get; set; } = "Filipino";

        [MaxLength(500)]
        public string? PermanentAddress { get; set; }

        [MaxLength(500)]
        public string? PresentAddress { get; set; }

        public int? YearsAtCurrentAddress { get; set; }

        [MaxLength(50)]
        public string? HomeOwnership { get; set; } // Owned, Rented, Living with Parents, Company Provided

        public int? NumberOfDependents { get; set; } = 0;

        [MaxLength(100)]
        public string? EducationalAttainment { get; set; }

        // Spouse Information (if married)
        [MaxLength(200)]
        public string? SpouseName { get; set; }

        [MaxLength(200)]
        public string? SpouseEmployer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpouseIncome { get; set; }

        [MaxLength(200)]
        public string? MotherMaidenName { get; set; }

        // Employment Verification Status
        public bool EmploymentVerified { get; set; } = false;

        [MaxLength(100)]
        public string? EmploymentVerifiedBy { get; set; }

        public DateTime? EmploymentVerifiedAt { get; set; }

        public bool IncomeVerified { get; set; } = false;

        [MaxLength(100)]
        public string? IncomeVerifiedBy { get; set; }

        public DateTime? IncomeVerifiedAt { get; set; }

        // Risk Profile
        [MaxLength(20)]
        public string? CustomerRiskRating { get; set; } // LOW, MEDIUM, HIGH

        public int? InternalCreditScore { get; set; } // 300-850 simulated score

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

