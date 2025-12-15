using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models
{
    /// <summary>
    /// Represents an asset in the marketplace (Property, Vehicle, or Other)
    /// </summary>
    [Table("Assets")]
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssetType { get; set; } = ""; // Property, Vehicle, Other

        [Required]
        [MaxLength(200)]
        public string AssetName { get; set; } = "";

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DownPaymentPercent { get; set; } = 20; // Default 20%

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DownPaymentAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; } = 12; // Annual interest rate

        public int DefaultTermMonths { get; set; } = 60; // Default 5 years

        [MaxLength(50)]
        public string Status { get; set; } = "Available"; // Available, Reserved, Sold, Inactive

        // ========== PROPERTY-SPECIFIC FIELDS ==========
        [MaxLength(100)]
        public string? PropertyType { get; set; } // House & Lot, Condo, Townhouse, Lot Only

        [MaxLength(500)]
        public string? PropertyLocation { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LandAreaSqm { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FloorAreaSqm { get; set; }

        public int? Bedrooms { get; set; }

        public int? Bathrooms { get; set; }

        public int? ParkingSlots { get; set; }

        [MaxLength(200)]
        public string? DeveloperName { get; set; }

        [MaxLength(100)]
        public string? TitleStatus { get; set; } // Clean Title, TCT, CCT

        // ========== VEHICLE-SPECIFIC FIELDS ==========
        [MaxLength(100)]
        public string? VehicleBrand { get; set; }

        [MaxLength(100)]
        public string? VehicleModel { get; set; }

        public int? VehicleYear { get; set; }

        [MaxLength(50)]
        public string? VehicleCondition { get; set; } // New, Pre-owned

        public int? Mileage { get; set; } // For pre-owned

        [MaxLength(50)]
        public string? Transmission { get; set; } // Manual, Automatic, CVT

        [MaxLength(50)]
        public string? FuelType { get; set; } // Gasoline, Diesel, Electric, Hybrid

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(100)]
        public string? EngineNumber { get; set; }

        [MaxLength(100)]
        public string? ChassisNumber { get; set; }

        [MaxLength(50)]
        public string? PlateNumber { get; set; }

        // ========== OTHER ASSET FIELDS ==========
        [MaxLength(100)]
        public string? OtherAssetCategory { get; set; } // Appliances, Jewelry, Equipment, Electronics, Furniture

        [MaxLength(200)]
        public string? OtherAssetBrand { get; set; }

        [MaxLength(200)]
        public string? OtherAssetModel { get; set; }

        [MaxLength(100)]
        public string? OtherAssetCondition { get; set; } // New, Like New, Good, Fair

        [MaxLength(500)]
        public string? OtherAssetSpecifications { get; set; }

        // ========== COMMON FIELDS ==========
        [MaxLength(200)]
        public string? SellerName { get; set; }

        [MaxLength(100)]
        public string? SellerContact { get; set; }

        [MaxLength(500)]
        public string? SellerAddress { get; set; }

        [MaxLength(200)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for images
        public virtual ICollection<AssetImage>? Images { get; set; }

        // Navigation property for applications
        public virtual ICollection<AssetApplication>? Applications { get; set; }

        // ========== COMPUTED PROPERTIES ==========
        [NotMapped]
        public decimal ComputedDownPayment => TotalPrice * (DownPaymentPercent / 100);

        [NotMapped]
        public decimal LoanableAmount => TotalPrice - ComputedDownPayment;

        [NotMapped]
        public decimal MonthlyInstallmentPreview
        {
            get
            {
                if (DefaultTermMonths <= 0) return 0;
                var principal = LoanableAmount;
                var monthlyRate = (InterestRate / 100) / 12;
                if (monthlyRate == 0) return principal / DefaultTermMonths;
                
                var payment = principal * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), DefaultTermMonths)) 
                              / ((decimal)Math.Pow((double)(1 + monthlyRate), DefaultTermMonths) - 1);
                return Math.Round(payment, 2);
            }
        }

        [NotMapped]
        public string DisplayTitle
        {
            get
            {
                return AssetType switch
                {
                    "Property" => $"{PropertyType} - {PropertyLocation}",
                    "Vehicle" => $"{VehicleYear} {VehicleBrand} {VehicleModel}",
                    "Other" => $"{OtherAssetCategory} - {OtherAssetBrand} {OtherAssetModel}",
                    _ => AssetName
                };
            }
        }
    }

    /// <summary>
    /// Represents images associated with an asset
    /// </summary>
    [Table("AssetImages")]
    public class AssetImage
    {
        [Key]
        public int ImageId { get; set; }

        public int AssetId { get; set; }

        [MaxLength(200)]
        public string? FileName { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public byte[]? ImageData { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsPrimary { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }
    }

    /// <summary>
    /// Represents a customer's application to purchase/loan an asset
    /// </summary>
    [Table("AssetApplications")]
    public class AssetApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApplicationNumber { get; set; } = "";

        public int AssetId { get; set; }

        public int CustomerId { get; set; } // UserId of customer

        public int? CustomerAccountId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PurchaseType { get; set; } = "Loan"; // Cash, Loan

        [Column(TypeName = "decimal(18,2)")]
        public decimal AssetPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DownPaymentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanAmount { get; set; }

        public int TermMonths { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        // ========== REQUIREMENT CHECKBOXES (Customer must check these) ==========
        public bool AgreedTermsAndConditions { get; set; } = false;
        public bool AgreedPaymentTerms { get; set; } = false;
        public bool AgreedPenaltyTerms { get; set; } = false;
        public bool AgreedLegalAction { get; set; } = false;
        public bool CertifiedInfoAccuracy { get; set; } = false;

        // ========== DOCUMENT SUBMISSION TRACKING (Customer checks what they will submit) ==========
        public bool ValidIdRequired { get; set; } = false;
        public bool ValidIdSubmitted { get; set; } = false;

        public bool ProofOfIncomeRequired { get; set; } = false;
        public bool ProofOfIncomeSubmitted { get; set; } = false;

        public bool ProofOfBillingRequired { get; set; } = false;
        public bool ProofOfBillingSubmitted { get; set; } = false;

        public bool BankStatementRequired { get; set; } = false;
        public bool BankStatementSubmitted { get; set; } = false;

        public bool COERequired { get; set; } = false; // Certificate of Employment
        public bool COESubmitted { get; set; } = false;

        public bool BusinessPermitRequired { get; set; } = false;
        public bool BusinessPermitSubmitted { get; set; } = false;

        public bool CollateralDocsRequired { get; set; } = false;
        public bool CollateralDocsSubmitted { get; set; } = false;

        // For property
        public bool TitleDeedRequired { get; set; } = false;
        public bool TitleDeedSubmitted { get; set; } = false;
        
        public bool TaxDeclarationRequired { get; set; } = false;
        public bool TaxDeclarationSubmitted { get; set; } = false;

        // For vehicle
        public bool ORCRRequired { get; set; } = false; // Official Receipt / Certificate of Registration
        public bool ORCRSubmitted { get; set; } = false;

        // ========== UPLOADED DOCUMENTS (JSON array of document info) ==========
        [MaxLength(4000)]
        public string? UploadedDocuments { get; set; } // JSON: [{name, type, uploadedAt, uploadedBy}]

        // ========== TELLER VERIFICATION CHECKLIST ==========
        public bool TellerVerifiedId { get; set; } = false;
        public bool TellerVerifiedIncome { get; set; } = false;
        public bool TellerVerifiedEmployment { get; set; } = false;
        public bool TellerVerifiedDocuments { get; set; } = false;
        public bool TellerVerifiedAddress { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "SUBMITTED";
        // SUBMITTED, PENDING_TELLER_REVIEW, VERIFIED, PENDING_ACCOUNTANT_REVIEW, 
        // ASSESSED, PENDING_FINANCEMANAGER_APPROVAL, APPROVED, REJECTED, 
        // RELEASED, CANCELLED

        // ========== CUSTOMER INFORMATION (captured at application time) ==========
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        [MaxLength(500)]
        public string? CustomerAddress { get; set; }

        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        [MaxLength(50)]
        public string? CustomerPhone { get; set; }

        [MaxLength(100)]
        public string? EmploymentStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyIncome { get; set; }

        // ========== WORKFLOW TRACKING ==========
        [MaxLength(200)]
        public string? SubmittedBy { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? TellerReviewedBy { get; set; }

        public DateTime? TellerReviewedAt { get; set; }

        [MaxLength(500)]
        public string? TellerRemarks { get; set; }

        [MaxLength(200)]
        public string? AccountantAssessedBy { get; set; }

        public DateTime? AccountantAssessedAt { get; set; }

        [MaxLength(500)]
        public string? AccountantRemarks { get; set; }

        // ========== ACCOUNTANT ASSESSMENT FIELDS ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AssessedMonthlyPayment { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AssessedInterestRate { get; set; }

        public int? AssessedTermMonths { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DebtToIncomeRatio { get; set; }

        [MaxLength(50)]
        public string? CreditRiskLevel { get; set; } // Low, Medium, High

        [MaxLength(50)]
        public string? AccountantRecommendation { get; set; } // APPROVE, DECLINE

        [MaxLength(200)]
        public string? FinanceManagerApprovedBy { get; set; }

        public DateTime? FinanceManagerApprovedAt { get; set; }

        [MaxLength(500)]
        public string? FinanceManagerRemarks { get; set; }

        [MaxLength(200)]
        public string? ReleasedBy { get; set; }

        public DateTime? ReleasedAt { get; set; }

        [MaxLength(500)]
        public string? ReleaseRemarks { get; set; }

        // ========== CONTRACT INFORMATION ==========
        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractDate { get; set; }

        [MaxLength(500)]
        public string? DeedOfSaleNumber { get; set; }

        public DateTime? DeedOfSaleDate { get; set; }

        // For vehicles
        [MaxLength(100)]
        public string? ORNumber { get; set; } // Official Receipt

        [MaxLength(100)]
        public string? CRNumber { get; set; } // Certificate of Registration

        // ========== LINKED LOAN (if applicable) ==========
        public int? LinkedLoanId { get; set; }

        // Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }

        [ForeignKey("CustomerId")]
        public virtual AuthUser? Customer { get; set; }

        // ========== HELPER METHODS ==========
        public static string GenerateApplicationNumber()
        {
            return $"AST-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        public static string GenerateContractNumber()
        {
            return $"CON-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }

    /// <summary>
    /// Represents history/audit trail for asset applications
    /// Tracks all workflow events including forwarding and release with contracts
    /// </summary>
    [Table("AssetHistory")]
    public class AssetHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int ApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = ""; 
        // SUBMITTED, VERIFIED, FORWARDED_TO_ACCOUNTANT, ASSESSED, FORWARDED_TO_FINANCEMANAGER, 
        // APPROVED, REJECTED, RELEASED, CANCELLED

        [MaxLength(200)]
        public string? ActionBy { get; set; } // Username who performed the action

        [MaxLength(100)]
        public string? ActionByRole { get; set; } // Role of the user (Teller, Accountant, FinanceManager)

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(50)]
        public string? PreviousStatus { get; set; }

        [MaxLength(50)]
        public string? NewStatus { get; set; }

        // ========== CONTRACT INFORMATION (captured at release) ==========
        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractDate { get; set; }

        [MaxLength(500)]
        public string? DeedOfSaleNumber { get; set; }

        public DateTime? DeedOfSaleDate { get; set; }

        [MaxLength(100)]
        public string? ORNumber { get; set; }

        [MaxLength(100)]
        public string? CRNumber { get; set; }

        // ========== FINANCIAL SNAPSHOT (captured at time of action) ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AssetPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DownPaymentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LoanAmount { get; set; }

        public int? TermMonths { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? InterestRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyPayment { get; set; }

        // ========== ASSESSMENT SNAPSHOT (for accountant actions) ==========
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DebtToIncomeRatio { get; set; }

        [MaxLength(50)]
        public string? CreditRiskLevel { get; set; }

        [MaxLength(50)]
        public string? Recommendation { get; set; }

        // ========== LINKED LOAN INFO (for release) ==========
        public int? LinkedLoanId { get; set; }

        // Navigation property
        [ForeignKey("ApplicationId")]
        public virtual AssetApplication? Application { get; set; }

        // ========== HELPER METHODS ==========
        public static AssetHistory CreateFromApplication(AssetApplication app, string actionType, string actionBy, string actionByRole, string? remarks = null)
        {
            return new AssetHistory
            {
                ApplicationId = app.ApplicationId,
                ActionType = actionType,
                ActionBy = actionBy,
                ActionByRole = actionByRole,
                ActionDate = DateTime.Now,
                Remarks = remarks,
                AssetPrice = app.AssetPrice,
                DownPaymentAmount = app.DownPaymentAmount,
                LoanAmount = app.LoanAmount,
                TermMonths = app.TermMonths,
                InterestRate = app.InterestRate,
                MonthlyPayment = app.MonthlyPayment,
                DebtToIncomeRatio = app.DebtToIncomeRatio,
                CreditRiskLevel = app.CreditRiskLevel,
                Recommendation = app.AccountantRecommendation,
                ContractNumber = app.ContractNumber,
                ContractDate = app.ContractDate,
                DeedOfSaleNumber = app.DeedOfSaleNumber,
                DeedOfSaleDate = app.DeedOfSaleDate,
                ORNumber = app.ORNumber,
                CRNumber = app.CRNumber,
                LinkedLoanId = app.LinkedLoanId
            };
        }
    }
}
