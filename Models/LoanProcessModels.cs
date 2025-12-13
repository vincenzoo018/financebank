using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models;

// =====================================================================
// LOAN PROCESS MODELS - COMPLETE WORKFLOW (ENHANCED)
// =====================================================================

/// <summary>
/// Loan Application Customer Submission Stage - Enhanced with full details
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

    public int RequestedTermMonths { get; set; } = 12;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? RequestedInterestRate { get; set; }

    [MaxLength(50)]
    public string LoanCategory { get; set; } = "UNSECURED"; // SECURED, UNSECURED

    [MaxLength(500)]
    public string? Purpose { get; set; }

    [MaxLength(100)]
    public string? PurposeCategory { get; set; } // Medical, Education, Business Capital, Home Improvement, Debt Consolidation, etc.

    [MaxLength(1000)]
    public string? PurposeDetails { get; set; }

    // Legacy fields (still used for backward compatibility)
    [MaxLength(50)]
    public string? EmploymentStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyIncome { get; set; }

    public int? ExistingLoans { get; set; } = 0;

    // =====================================================================
    // ENHANCED DEBT AND LIABILITY INFORMATION
    // =====================================================================

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExistingDebtsTotal { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExistingMonthlyPayments { get; set; } = 0;

    [MaxLength(1000)]
    public string? ExistingDebtsDetails { get; set; } // JSON or description of existing debts

    // =====================================================================
    // COLLATERAL AND CO-BORROWER FLAGS
    // =====================================================================

    public bool HasCollateral { get; set; } = false;

    public bool HasCoBorrower { get; set; } = false;

    public bool HasGuarantor { get; set; } = false;

    // =====================================================================
    // DOCUMENT SUBMISSION TRACKING
    // =====================================================================

    [MaxLength(2000)]
    public string? Documents { get; set; } // JSON array of document names

    public bool BankStatementSubmitted { get; set; } = false;

    public int? BankStatementMonths { get; set; } // Number of months of statements

    public bool PayslipsSubmitted { get; set; } = false;

    public bool ITRSubmitted { get; set; } = false; // Income Tax Return

    public bool COESubmitted { get; set; } = false; // Certificate of Employment

    public bool BusinessPermitSubmitted { get; set; } = false;

    public bool ValidIdSubmitted { get; set; } = false;

    public bool ProofOfBillingSubmitted { get; set; } = false;

    // =====================================================================
    // ADDITIONAL INCOME INFORMATION
    // =====================================================================

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherIncome { get; set; } // Other sources of income

    // =====================================================================
    // TERMS ACKNOWLEDGMENT
    // =====================================================================

    public bool AcknowledgedTerms { get; set; } = false;

    public bool AcknowledgedPenaltyTerms { get; set; } = false;

    public bool AcknowledgedLegalAction { get; set; } = false;

    public bool AcknowledgedInfoAccuracy { get; set; } = false; // Certified information accuracy

    [MaxLength(200)]
    public string? ApplicantSignature { get; set; }

    public DateTime? ApplicantSignatureDate { get; set; }

    // =====================================================================
    // STATUS AND WORKFLOW
    // =====================================================================

    [Required, MaxLength(50)]
    public string Status { get; set; } = "SUBMITTED"; // SUBMITTED, PENDING_TELLER_REVIEW, VERIFIED, PENDING_ASSESSMENT, ASSESSED, PENDING_APPROVAL, APPROVED, REJECTED, CANCELLED

    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string SubmittedBy { get; set; } = "";

    // Teller Review
    [MaxLength(100)]
    public string? TellerReviewedBy { get; set; }

    public DateTime? TellerReviewedAt { get; set; }

    [MaxLength(1000)]
    public string? TellerRemarks { get; set; }

    [MaxLength(50)]
    public string? TellerRecommendation { get; set; } // RECOMMEND_APPROVE, RECOMMEND_DECLINE

    // Rejection
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
    public virtual ICollection<LoanCollateral>? Collaterals { get; set; }
    public virtual ICollection<LoanCoBorrower>? CoBorrowers { get; set; }
    public virtual ICollection<LoanVerificationChecklist>? VerificationChecklists { get; set; }
    public virtual ICollection<LoanDocument>? Documents_Files { get; set; }
}

/// <summary>
/// Loan Assessment Accountant Financial Review and Computation - Enhanced
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

    // =====================================================================
    // CREDIT AND RISK ASSESSMENT
    // =====================================================================

    public int? CreditScore { get; set; } // 300-850

    [MaxLength(50)]
    public string? CreditScoreSource { get; set; } // INTERNAL, CIBI, TransUnion

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DebtToIncomeRatio { get; set; } // Percentage (e.g., 35.50%)

    public int? RiskScore { get; set; } // 1-100

    [MaxLength(20)]
    public string? RiskCategory { get; set; } // LOW, MEDIUM, HIGH, VERY_HIGH

    [MaxLength(2000)]
    public string? RiskAssessmentDetails { get; set; } // JSON with detailed breakdown

    // =====================================================================
    // VERIFICATION CHECKLIST RESULTS
    // =====================================================================

    public bool IdentityVerified { get; set; } = false;

    public bool IncomeVerified { get; set; } = false;

    public bool AddressVerified { get; set; } = false;

    public bool EmploymentVerified { get; set; } = false;

    public bool CollateralVerified { get; set; } = false;

    public bool CoBorrowerVerified { get; set; } = false;

    public bool BankStatementsReviewed { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AverageMonthlyBalance { get; set; }

    [MaxLength(2000)]
    public string? VerificationNotes { get; set; }

    // =====================================================================
    // ACCOUNTANT RECOMMENDATION
    // =====================================================================

    [MaxLength(50)]
    public string? AccountantRecommendation { get; set; } // RECOMMEND_APPROVE, RECOMMEND_DECLINE, NEEDS_REVIEW

    [MaxLength(1000)]
    public string? RecommendationReason { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SuggestedAmount { get; set; }

    public int? SuggestedTerm { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? SuggestedRate { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    public virtual ICollection<LoanApproval>? Approvals { get; set; }
}

/// <summary>
/// Loan Approval - Finance Manager Decision & Terms Setting - Enhanced
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

    // =====================================================================
    // ENHANCED APPROVAL CRITERIA AND REVIEW
    // =====================================================================

    [MaxLength(1000)]
    public string? ApprovalReason { get; set; }

    public bool ReviewedCreditScore { get; set; } = false;

    public bool ReviewedDTI { get; set; } = false;

    public bool ReviewedRiskAssessment { get; set; } = false;

    public bool ReviewedCollateral { get; set; } = false;

    public bool ReviewedCoBorrower { get; set; } = false;

    [MaxLength(2000)]
    public string? FinalDecisionNotes { get; set; }

    public bool RequiresAdditionalDocuments { get; set; } = false;

    [MaxLength(500)]
    public string? AdditionalDocumentsRequired { get; set; }

    // Navigation
    [ForeignKey("AssessmentId")]
    public virtual LoanAssessment? Assessment { get; set; }

    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    public virtual ICollection<LoanDisbursal>? Disbursals { get; set; }
    public virtual ICollection<LoanContract>? Contracts { get; set; }
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

/// <summary>
/// Loan Transaction History - Tracks all loan-related activities
/// </summary>
[Table("LoanTransactionHistory")]
public class LoanTransactionHistory
{
    [Key]
    public int HistoryId { get; set; }

    public int? LoanId { get; set; }

    public int? ApplicationId { get; set; }

    public int? ApprovalId { get; set; }

    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = ""; // APPLICATION, TELLER_REVIEW, ACCOUNTANT_ASSESSMENT, FM_APPROVAL, FM_DECLINE, RELEASE, PAYMENT, FULL_PAYMENT

    [MaxLength(500)]
    public string Description { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PenaltyAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BalanceBefore { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BalanceAfter { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [Required, MaxLength(100)]
    public string ProcessedBy { get; set; } = "";

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? Status { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

/// <summary>
/// Loan Invoice - Generated for release and payment transactions
/// </summary>
[Table("LoanInvoices")]
public class LoanInvoice
{
    [Key]
    public int InvoiceId { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = "";

    [Required, MaxLength(50)]
    public string InvoiceType { get; set; } = ""; // RELEASE, PAYMENT

    public int? LoanId { get; set; }

    public int? PaymentId { get; set; }

    public int? DisbursalId { get; set; }

    public int AccountId { get; set; }

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = "";

    [MaxLength(100)]
    public string? AccountNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FeesAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BalanceBefore { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BalanceAfter { get; set; }

    [Required, MaxLength(100)]
    public string ProcessedBy { get; set; } = "";

    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

// =====================================================================
// NEW ENHANCED LOAN PROCESS MODELS
// =====================================================================

/// <summary>
/// Loan Collateral - For Secured Loans
/// </summary>
[Table("LoanCollaterals")]
public class LoanCollateral
{
    [Key]
    public int CollateralId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required, MaxLength(50)]
    public string CollateralType { get; set; } = ""; // Real Estate, Vehicle, Equipment, Jewelry, Securities, Deposit

    [Required, MaxLength(500)]
    public string CollateralDescription { get; set; } = "";

    [MaxLength(500)]
    public string? CollateralLocation { get; set; }

    // Valuation
    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal EstimatedValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AppraisedValue { get; set; }

    public DateTime? AppraisalDate { get; set; }

    [MaxLength(200)]
    public string? AppraisedBy { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? LoanToValueRatio { get; set; } // LTV%

    // Ownership
    [Required, MaxLength(200)]
    public string OwnerName { get; set; } = "";

    [MaxLength(50)]
    public string? OwnerRelationship { get; set; } // Self, Spouse, Parent, etc.

    // Documents
    [MaxLength(100)]
    public string? TitleNumber { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(500)]
    public string? DocumentsSubmitted { get; set; } // JSON list

    // Status
    [MaxLength(50)]
    public string VerificationStatus { get; set; } = "PENDING"; // PENDING, VERIFIED, REJECTED

    [MaxLength(100)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(500)]
    public string? VerificationRemarks { get; set; }

    // Insurance
    public bool IsInsured { get; set; } = false;

    [MaxLength(200)]
    public string? InsuranceCompany { get; set; }

    [MaxLength(100)]
    public string? InsurancePolicyNumber { get; set; }

    public DateTime? InsuranceExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }
}

/// <summary>
/// Loan Document - Uploaded Document Files
/// </summary>
[Table("LoanDocuments")]
public class LoanDocument
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required, MaxLength(50)]
    public string DocumentType { get; set; } = ""; // BankStatement, Payslip, ITR, COE, ValidID, ProofOfBilling

    [Required, MaxLength(255)]
    public string FileName { get; set; } = "";

    [MaxLength(100)]
    public string? ContentType { get; set; }

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    public long? FileSize { get; set; }

    [Required, MaxLength(100)]
    public string UploadedBy { get; set; } = "";

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    // Verification
    [MaxLength(100)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(50)]
    public string VerificationStatus { get; set; } = "PENDING"; // PENDING, VERIFIED, REJECTED

    [MaxLength(500)]
    public string? VerificationNotes { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }
}

/// <summary>
/// Loan Co-Borrower or Guarantor
/// </summary>
[Table("LoanCoBorrowers")]
public class LoanCoBorrower
{
    [Key]
    public int CoBorrowerId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required, MaxLength(50)]
    public string RelationshipType { get; set; } = ""; // CO_BORROWER, GUARANTOR

    // Personal Information
    [Required, MaxLength(200)]
    public string FullName { get; set; } = "";

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(20)]
    public string? CivilStatus { get; set; }

    [MaxLength(50)]
    public string Nationality { get; set; } = "Filipino";

    // Contact
    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    // Employment
    [MaxLength(50)]
    public string? EmploymentStatus { get; set; }

    [MaxLength(200)]
    public string? EmployerName { get; set; }

    [MaxLength(500)]
    public string? EmployerAddress { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    public int? YearsEmployed { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyIncome { get; set; }

    // Government IDs
    [MaxLength(20)]
    public string? TIN { get; set; }

    [MaxLength(20)]
    public string? SSSNumber { get; set; }

    [MaxLength(50)]
    public string? ValidIdType { get; set; }

    [MaxLength(50)]
    public string? ValidIdNumber { get; set; }

    // Relationship to Borrower
    [MaxLength(50)]
    public string? RelationshipToBorrower { get; set; } // Spouse, Parent, Sibling, Friend, etc.

    // Verification
    [MaxLength(50)]
    public string VerificationStatus { get; set; } = "PENDING";

    [MaxLength(100)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(500)]
    public string? VerificationRemarks { get; set; }

    // Acknowledgment
    public bool HasAcknowledged { get; set; } = false;

    public DateTime? AcknowledgmentDate { get; set; }

    [MaxLength(200)]
    public string? Signature { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }
}

/// <summary>
/// Loan Contract - Comprehensive contract with legal terms
/// </summary>
[Table("LoanContracts")]
public class LoanContract
{
    [Key]
    public int ContractId { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public int ApprovalId { get; set; }

    // Contract Identification
    [Required, MaxLength(50)]
    public string ContractNumber { get; set; } = "";

    [Required]
    public DateTime ContractDate { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    [Required]
    public DateTime MaturityDate { get; set; }

    // Loan Terms
    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    [Required]
    public decimal InterestRate { get; set; }

    [Required]
    public int TermMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal MonthlyPayment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal TotalInterest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal TotalPayable { get; set; }

    // Payment Terms
    public int PaymentDueDay { get; set; } = 5; // Day of month payment is due

    [Required]
    public DateTime FirstPaymentDate { get; set; }

    [Required]
    public DateTime LastPaymentDate { get; set; }

    // Grace Period and Penalties (Industry Standard)
    public int GracePeriodDays { get; set; } = 5; // 5 days grace period

    [Column(TypeName = "decimal(5,4)")]
    public decimal LatePenaltyRate { get; set; } = 0.0005m; // 0.05% per day (industry standard)

    [MaxLength(20)]
    public string LatePenaltyType { get; set; } = "DAILY"; // DAILY, MONTHLY, FIXED

    [Column(TypeName = "decimal(5,2)")]
    public decimal? MaxPenaltyRate { get; set; } = 25.00m; // Max 25% of outstanding

    // Legal Action Thresholds
    public int FirstWarningDays { get; set; } = 7; // Days overdue for first reminder
    public int SecondWarningDays { get; set; } = 15; // Days overdue for second warning
    public int DemandLetterDays { get; set; } = 30; // Days overdue for demand letter
    public int LegalActionDays { get; set; } = 60; // Days overdue for legal action
    public int CollectionAgencyDays { get; set; } = 90; // Days overdue for collection agency

    // Legal Terms Text
    [Column(TypeName = "nvarchar(max)")]
    public string? LegalWarningText { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? PenaltyClauseText { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? DefaultClauseText { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? CollectionClauseText { get; set; }

    // Borrower Acknowledgments
    [Required, MaxLength(200)]
    public string BorrowerFullName { get; set; } = "";

    [Required, MaxLength(500)]
    public string BorrowerAddress { get; set; } = "";

    [MaxLength(200)]
    public string? BorrowerSignature { get; set; }

    public DateTime? BorrowerSignedAt { get; set; }

    // Co-Borrower
    [MaxLength(200)]
    public string? CoBorrowerFullName { get; set; }

    [MaxLength(200)]
    public string? CoBorrowerSignature { get; set; }

    public DateTime? CoBorrowerSignedAt { get; set; }

    // Guarantor
    [MaxLength(200)]
    public string? GuarantorFullName { get; set; }

    [MaxLength(200)]
    public string? GuarantorSignature { get; set; }

    public DateTime? GuarantorSignedAt { get; set; }

    // Witness
    [MaxLength(200)]
    public string? WitnessName { get; set; }

    [MaxLength(200)]
    public string? WitnessSignature { get; set; }

    // Bank Representative
    [Required, MaxLength(200)]
    public string BankRepresentativeName { get; set; } = "";

    [Required, MaxLength(100)]
    public string BankRepresentativeTitle { get; set; } = "";

    [MaxLength(200)]
    public string? BankRepresentativeSignature { get; set; }

    // Contract Status
    [MaxLength(50)]
    public string ContractStatus { get; set; } = "DRAFT"; // DRAFT, PENDING_SIGNATURE, ACTIVE, COMPLETED, DEFAULTED, LEGAL_ACTION

    // Notarization (Optional)
    public bool IsNotarized { get; set; } = false;

    [MaxLength(200)]
    public string? NotaryName { get; set; }

    [MaxLength(50)]
    public string? NotaryNumber { get; set; }

    public DateTime? NotarizationDate { get; set; }

    // Document
    [Column(TypeName = "varbinary(max)")]
    public byte[]? ContractDocument { get; set; } // PDF of signed contract

    [MaxLength(200)]
    public string? ContractDocumentName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    [Required, MaxLength(100)]
    public string CreatedBy { get; set; } = "";

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }

    [ForeignKey("ApprovalId")]
    public virtual LoanApproval? Approval { get; set; }
}

/// <summary>
/// Loan Verification Checklist - Detailed verification tracking
/// </summary>
[Table("LoanVerificationChecklists")]
public class LoanVerificationChecklist
{
    [Key]
    public int ChecklistId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    // Stage
    [Required, MaxLength(50)]
    public string VerificationStage { get; set; } = ""; // TELLER, ACCOUNTANT, FINANCE_MANAGER

    // Identity Verification
    public bool ValidIdChecked { get; set; } = false;

    [MaxLength(50)]
    public string? ValidIdType { get; set; }

    [MaxLength(50)]
    public string? ValidIdNumber { get; set; }

    public DateTime? ValidIdExpiry { get; set; }

    [MaxLength(200)]
    public string? ValidIdRemarks { get; set; }

    public bool SecondaryIdChecked { get; set; } = false;

    [MaxLength(50)]
    public string? SecondaryIdType { get; set; }

    public bool PhotoMatchesId { get; set; } = false;

    public bool SignatureVerified { get; set; } = false;

    // Income Verification
    public bool PayslipVerified { get; set; } = false;

    public int? PayslipMonths { get; set; }

    [MaxLength(200)]
    public string? PayslipRemarks { get; set; }

    public bool COEVerified { get; set; } = false;

    public DateTime? COEDate { get; set; }

    [MaxLength(200)]
    public string? COERemarks { get; set; }

    public bool ITRVerified { get; set; } = false;

    public int? ITRYear { get; set; }

    [MaxLength(200)]
    public string? ITRRemarks { get; set; }

    public bool BankStatementVerified { get; set; } = false;

    public int? BankStatementMonths { get; set; }

    [MaxLength(200)]
    public string? BankStatementRemarks { get; set; }

    // Business Verification (for self-employed)
    public bool BusinessPermitVerified { get; set; } = false;

    public bool DTIRegistrationVerified { get; set; } = false;

    public bool SECRegistrationVerified { get; set; } = false;

    public bool AuditedFSVerified { get; set; } = false;

    // Address Verification
    public bool ProofOfAddressChecked { get; set; } = false;

    [MaxLength(50)]
    public string? ProofOfAddressType { get; set; } // Utility Bill, Barangay Clearance, etc.

    public bool AddressMatchesRecord { get; set; } = false;

    // Employment Verification
    public bool EmployerContactVerified { get; set; } = false;

    public bool EmploymentCallMade { get; set; } = false;

    public DateTime? EmploymentCallDate { get; set; }

    [MaxLength(500)]
    public string? EmploymentCallNotes { get; set; }

    // Credit Check
    public bool CreditCheckPerformed { get; set; } = false;

    public DateTime? CreditCheckDate { get; set; }

    [MaxLength(50)]
    public string? CreditCheckSource { get; set; }

    [MaxLength(50)]
    public string? CreditCheckResult { get; set; } // PASS, FAIL, NEEDS_REVIEW

    // Collateral Verification
    public bool CollateralDocumentsChecked { get; set; } = false;

    public bool CollateralAppraisalDone { get; set; } = false;

    public bool CollateralInsuranceVerified { get; set; } = false;

    // Co-Borrower/Guarantor Verification
    public bool CoBorrowerIdVerified { get; set; } = false;

    public bool CoBorrowerIncomeVerified { get; set; } = false;

    public bool GuarantorIdVerified { get; set; } = false;

    public bool GuarantorIncomeVerified { get; set; } = false;

    // Overall
    [MaxLength(50)]
    public string OverallStatus { get; set; } = "PENDING"; // PENDING, COMPLETE, INCOMPLETE, FAILED

    public int CompletionPercentage { get; set; } = 0;

    [MaxLength(1000)]
    public string? MissingItems { get; set; }

    [Required, MaxLength(100)]
    public string VerifiedBy { get; set; } = "";

    public DateTime VerifiedAt { get; set; } = DateTime.Now;

    [MaxLength(2000)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? Application { get; set; }
}

/// <summary>
/// Loan Delinquency - Track overdue payments and legal actions
/// </summary>
[Table("LoanDelinquencies")]
public class LoanDelinquency
{
    [Key]
    public int DelinquencyId { get; set; }

    [Required]
    public int LoanId { get; set; }

    public int? ContractId { get; set; }

    // Delinquency Details
    [Required, MaxLength(50)]
    public string DelinquencyType { get; set; } = ""; // LATE_PAYMENT, MISSED_PAYMENT, CHRONIC_DELINQUENT, DEFAULT

    [Required]
    public int DaysOverdue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal OverdueAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal PenaltyAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Required]
    public decimal TotalAmountDue { get; set; }

    // Action Stage
    [Required, MaxLength(50)]
    public string ActionStage { get; set; } = ""; // REMINDER, WARNING, DEMAND_LETTER, LEGAL_NOTICE, LEGAL_ACTION, COLLECTION

    [Required]
    public DateTime ActionDate { get; set; }

    [Required, MaxLength(100)]
    public string ActionTakenBy { get; set; } = "";

    // Communication
    [MaxLength(50)]
    public string? CommunicationType { get; set; } // SMS, EMAIL, PHONE, LETTER, LEGAL_NOTICE

    public DateTime? CommunicationSentAt { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? CommunicationContent { get; set; }

    [MaxLength(1000)]
    public string? CustomerResponse { get; set; }

    public DateTime? CustomerResponseDate { get; set; }

    // Legal Action Details
    [MaxLength(200)]
    public string? LawyerAssigned { get; set; }

    [MaxLength(100)]
    public string? LegalCaseNumber { get; set; }

    public DateTime? LegalFilingDate { get; set; }

    [MaxLength(50)]
    public string? LegalStatus { get; set; }

    [MaxLength(2000)]
    public string? LegalNotes { get; set; }

    // Collection Agency Details
    [MaxLength(200)]
    public string? CollectionAgencyName { get; set; }

    [MaxLength(100)]
    public string? CollectionAgencyContact { get; set; }

    public DateTime? CollectionReferralDate { get; set; }

    [MaxLength(50)]
    public string? CollectionStatus { get; set; }

    // Resolution
    public bool IsResolved { get; set; } = false;

    [MaxLength(50)]
    public string? ResolutionType { get; set; } // PAID, RESTRUCTURED, SETTLED, WRITTEN_OFF

    public DateTime? ResolutionDate { get; set; }

    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }

    [MaxLength(100)]
    public string? ResolvedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    public virtual Loan? Loan { get; set; }

    [ForeignKey("ContractId")]
    public virtual LoanContract? Contract { get; set; }
}

/// <summary>
/// Loan Legal Templates - Standard legal text for contracts
/// </summary>
[Table("LoanLegalTemplates")]
public class LoanLegalTemplate
{
    [Key]
    public int TemplateId { get; set; }

    [Required, MaxLength(100)]
    public string TemplateName { get; set; } = "";

    [Required, MaxLength(50)]
    public string TemplateType { get; set; } = ""; // PENALTY_CLAUSE, DEFAULT_CLAUSE, LEGAL_WARNING, COLLECTION_WARNING

    [Required, Column(TypeName = "nvarchar(max)")]
    public string TemplateContent { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}

// =====================================================================
// EXTENDED ENTITIES FOR EXISTING TABLES
// =====================================================================
// NOTE: The Loan class in DatabaseModels.cs has been extended with 
// loan process tracking fields (ApplicationId, AssessmentId, ApprovalId, etc.)
// =====================================================================
