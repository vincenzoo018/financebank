using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models;

// =====================================================================
// LOAN PROCESS MODELS - COMPLETE WORKFLOW
// =====================================================================

/// <summary>
/// Loan Application Customer Submission Stage
/// </summary>
[Table("LoanApplications")]
public class LoanApplication
{
    [Key]
    public int ApplicationId { get; set; }

    [Required, MaxLength(50)]
    public string ApplicationNumber { get; set; } = "";

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string LoanType { get; set; } = ""; // Personal, Home, Auto, Education

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal RequestedAmount { get; set; }

    [MaxLength(500)]
    public string? Purpose { get; set; }

    [MaxLength(50)]
    public string? EmploymentStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyIncome { get; set; }

    public int ExistingLoans { get; set; } = 0;

    [MaxLength(2000)]
    public string? Documents { get; set; } // JSON array of document names

    [Required, MaxLength(50)]
    public string Status { get; set; } = "SUBMITTED"; // SUBMITTED, VERIFIED, REJECTED

    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string SubmittedBy { get; set; } = "";

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime? RejectedAt { get; set; }

    [MaxLength(50)]
    public string? RejectedBy { get; set; }

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    public virtual ICollection<LoanAssessment>? Assessments { get; set; }
    public virtual ICollection<LoanApproval>? Approvals { get; set; }
}

/// <summary>
/// Loan Assessment Accountant Financial Review and Computation
/// </summary>
[Table("LoanAssessments")]
public class LoanAssessment
{
    [Key]
    public int AssessmentId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal LoanAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    [Required]
    public decimal InterestRate { get; set; }

    [Required]
    public int TermMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal ComputedMonthlyPayment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal TotalPayable { get; set; }

    public DateTime AssessmentDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string AssessmentStatus { get; set; } = "ASSESSED"; // ASSESSED, FORWARDED

    [Required, MaxLength(50)]
    public string AssessedBy { get; set; } = "";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    public virtual ICollection<LoanApproval>? Approvals { get; set; }
}

/// <summary>
/// Loan Approval - Finance Manager Decision & Terms Setting
/// </summary>
[Table("LoanApprovals")]
public class LoanApproval
{
    [Key]
    public int ApprovalId { get; set; }

    [Required]
    public int AssessmentId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string ApprovalStatus { get; set; } = ""; // APPROVED, DECLINED

    public DateTime ApprovalDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string ApprovedBy { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ApprovedInterestRate { get; set; }

    public int? ApprovedTermMonths { get; set; }

    [MaxLength(500)]
    public string? SpecialConditions { get; set; }

    [MaxLength(500)]
    public string? DeclinationReason { get; set; }

    // Navigation
    [ForeignKey("AssessmentId")]
    public virtual LoanAssessment? Assessment { get; set; }

    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    public virtual ICollection<LoanDisbursal>? Disbursals { get; set; }
}

/// <summary>
/// Loan Payment Schedule - Auto-generated Installment Plan
/// </summary>
[Table("LoanPaymentSchedules")]
public class LoanPaymentSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Required]
    public int PaymentNumber { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal MinimumPayment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal InterestAmount { get; set; }

    [Required, MaxLength(50)]
    public string PaymentStatus { get; set; } = "PENDING"; // PENDING, PAID, OVERDUE, PARTIAL

    public int DaysOverdue { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Penalty { get; set; } = 0;

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    public virtual ICollection<LoanPayment>? Payments { get; set; }
    public virtual ICollection<LoanViolation>? Violations { get; set; }
}

/// <summary>
/// Loan Payment - Actual Payment Transaction Record
/// </summary>
[Table("LoanPayments")]
public class LoanPayment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    public int ScheduleId { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PaymentAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyPaid { get; set; } = 0;

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; } = ""; // Cash, Check, Transfer, etc.

    [MaxLength(100)]
    public string? Reference { get; set; }

    [Required, MaxLength(50)]
    public string ProcessedBy { get; set; } = "";

    public bool IsLatePayment { get; set; } = false;

    // Navigation
    [ForeignKey("ScheduleId")]
    public virtual LoanPaymentSchedule? PaymentSchedule { get; set; }

    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }
}

/// <summary>
/// Loan Violation - Late/Missed Payment Tracking with Penalties
/// </summary>
[Table("LoanViolations")]
public class LoanViolation
{
    [Key]
    public int ViolationId { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Required]
    public int ScheduleId { get; set; }

    [Required, MaxLength(50)]
    public string ViolationType { get; set; } = ""; // LATE_PAYMENT, MISSED_PAYMENT, REPEATED_VIOLATION

    [Required]
    public int DaysOverdue { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    [Required]
    public decimal PenaltyRate { get; set; } // BPI Standard Rate (0.05% daily)

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PenaltyAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal OutstandingBalance { get; set; }

    public DateTime ViolationDate { get; set; } = DateTime.Now;

    public DateTime? ResolvedAt { get; set; }

    public bool IsResolved { get; set; } = false;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("ScheduleId")]
    public virtual LoanPaymentSchedule? PaymentSchedule { get; set; }
}

/// <summary>
/// Loan Disbursal - Funds Release Record
/// </summary>
[Table("LoanDisbursals")]
public class LoanDisbursal
{
    [Key]
    public int DisbursalId { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Required]
    public int ApprovalId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal DisbursalAmount { get; set; }

    public DateTime DisbursalDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string DisbursalStatus { get; set; } = "RELEASED"; // RELEASED, PENDING

    [Required, MaxLength(50)]
    public string DisbursedTo { get; set; } = ""; // Account or Cash

    [Required, MaxLength(50)]
    public string ProcessedBy { get; set; } = "";

    [MaxLength(100)]
    public string? Reference { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("ApprovalId")]
    public virtual LoanApproval? Approval { get; set; }
}

// =====================================================================
// EXTENDED ENTITIES FOR EXISTING TABLES
// =====================================================================
// NOTE: The Loan class in DatabaseModels.cs has been extended with 
// loan process tracking fields (ApplicationId, AssessmentId, ApprovalId, etc.)
// =====================================================================

