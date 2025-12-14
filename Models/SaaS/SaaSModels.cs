using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models.SaaS
{
    /// <summary>
    /// System Owner/Vendor who manages the SaaS platform
    /// </summary>
    public class SystemOwner
    {
        [Key]
        public int OwnerId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string CompanyName { get; set; } = "ERP Solutions Provider";

        [MaxLength(500)]
        public string? CompanyAddress { get; set; }

        [MaxLength(500)]
        public string? CompanyLogo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// Available modules in the ERP system
    /// </summary>
    public class SystemModule
    {
        [Key]
        public int ModuleId { get; set; }

        [Required, MaxLength(50)]
        public string ModuleCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ModuleName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal YearlyPrice { get; set; }

        public bool IsCore { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? IconClass { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; } // Icon path or CSS class

        public int SortOrder { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<PlanModule> PlanModules { get; set; } = new List<PlanModule>();
        public virtual ICollection<ClientModule> ClientModules { get; set; } = new List<ClientModule>();
    }

    /// <summary>
    /// Subscription plans (Basic, Pro, Enterprise)
    /// </summary>
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required, MaxLength(50)]
        public string PlanCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PlanName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal YearlyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SetupFee { get; set; }

        public int MaxUsers { get; set; } = 5;

        public int MaxBranches { get; set; } = 1;

        public int? MaxTransactionsPerMonth { get; set; }

        public int? MaxStorageGB { get; set; } = 5;

        [MaxLength(2000)]
        public string? Features { get; set; } // Comma-separated features list

        public bool IncludesSupport { get; set; } = true;

        [MaxLength(50)]
        public string SupportLevel { get; set; } = "Basic";

        public bool IsActive { get; set; } = true;

        public bool IsPopular { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<PlanModule> PlanModules { get; set; } = new List<PlanModule>();
        public virtual ICollection<ClientSubscription> Subscriptions { get; set; } = new List<ClientSubscription>();
    }

    /// <summary>
    /// Junction table: Which modules are in each plan
    /// </summary>
    public class PlanModule
    {
        [Key]
        public int PlanModuleId { get; set; }

        public int PlanId { get; set; }

        public int ModuleId { get; set; }

        public bool IsIncluded { get; set; } = true;

        // Navigation
        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan? Plan { get; set; }

        [ForeignKey("ModuleId")]
        public virtual SystemModule? Module { get; set; }
    }

    /// <summary>
    /// Client companies using the ERP system
    /// </summary>
    public class SaaSClient
    {
        [Key]
        public int ClientId { get; set; }

        [Required, MaxLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? TradeName { get; set; }

        [MaxLength(100)]
        public string? BusinessType { get; set; }

        [MaxLength(100)]
        public string? TaxId { get; set; }

        // Address
        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; } = "Philippines";

        // Contact Information
        [Required, MaxLength(256)]
        public string PrimaryEmail { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? SecondaryEmail { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Mobile { get; set; }

        [MaxLength(256)]
        public string? Website { get; set; }

        // Contact Person
        [MaxLength(200)]
        public string? ContactPersonName { get; set; }

        [MaxLength(100)]
        public string? ContactPersonTitle { get; set; }

        [MaxLength(256)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(50)]
        public string? ContactPersonPhone { get; set; }

        // System Access
        [MaxLength(100)]
        public string? DatabaseName { get; set; }

        [MaxLength(256)]
        public string? SystemUrl { get; set; }

        [MaxLength(256)]
        public string? LicenseKey { get; set; }

        // Subscription Status
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public DateTime? TrialEndsAt { get; set; }

        public DateTime? SubscriptionStartDate { get; set; }

        public DateTime? SubscriptionEndDate { get; set; }

        // Billing
        [MaxLength(50)]
        public string BillingCycle { get; set; } = "Monthly";

        public int BillingDay { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaid { get; set; }

        // Branding
        [MaxLength(500)]
        public string? Logo { get; set; }

        [MaxLength(20)]
        public string? PrimaryColor { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        // Navigation
        public virtual ICollection<ClientUser> Users { get; set; } = new List<ClientUser>();
        public virtual ICollection<ClientSubscription> Subscriptions { get; set; } = new List<ClientSubscription>();
        public virtual ICollection<ClientModule> Modules { get; set; } = new List<ClientModule>();
        public virtual ICollection<SaaSInvoice> Invoices { get; set; } = new List<SaaSInvoice>();
        public virtual ICollection<SaaSTransaction> Transactions { get; set; } = new List<SaaSTransaction>();
        public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }

    /// <summary>
    /// Users within each client company
    /// </summary>
    public class ClientUser
    {
        [Key]
        public int ClientUserId { get; set; }

        public int ClientId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Admin";

        [MaxLength(50)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPrimaryContact { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }
    }

    /// <summary>
    /// Client's active subscription
    /// </summary>
    public class ClientSubscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        public int ClientId { get; set; }

        public int? PlanId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string BillingCycle { get; set; } = "Monthly";

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalModulesPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [MaxLength(50)]
        public string? DiscountType { get; set; }

        [MaxLength(200)]
        public string? DiscountReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public bool AutoRenew { get; set; } = true;

        public int MaxUsers { get; set; } = 5;

        public int CurrentUsers { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan? Plan { get; set; }
    }

    /// <summary>
    /// Modules assigned to each client
    /// </summary>
    public class ClientModule
    {
        [Key]
        public int ClientModuleId { get; set; }

        public int ClientId { get; set; }

        public int ModuleId { get; set; }

        public int? SubscriptionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomPrice { get; set; }

        public bool IsCustomPrice { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime EnabledAt { get; set; } = DateTime.Now;

        public DateTime? DisabledAt { get; set; }

        [MaxLength(50)]
        public string Source { get; set; } = "Plan";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }

        [ForeignKey("ModuleId")]
        public virtual SystemModule? Module { get; set; }

        [ForeignKey("SubscriptionId")]
        public virtual ClientSubscription? Subscription { get; set; }
    }

    /// <summary>
    /// Available payment methods
    /// </summary>
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        [Required, MaxLength(50)]
        public string MethodCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string MethodName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MethodType { get; set; } // Bank, E-Wallet, Cash

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? AccountName { get; set; }

        [MaxLength(100)]
        public string? AccountNumber { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        [MaxLength(100)]
        public string? IconClass { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; } // Icon path or CSS class

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Generated invoices
    /// </summary>
    public class SaaSInvoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int ClientId { get; set; }

        public int? SubscriptionId { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 12.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceDue { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? InternalNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? PaidAt { get; set; }

        // Additional properties for UI compatibility
        [NotMapped]
        public DateTime IssueDate => InvoiceDate;

        [NotMapped]
        public string InvoiceType { get; set; } = "Subscription";

        [NotMapped]
        public string? BillingPeriod { get; set; }

        [NotMapped]
        public decimal SubTotal => Subtotal;

        [NotMapped]
        public decimal TaxAmount => Tax;

        [NotMapped]
        public decimal AmountDue => BalanceDue;

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }

        [ForeignKey("SubscriptionId")]
        public virtual ClientSubscription? Subscription { get; set; }

        public virtual ICollection<SaaSInvoiceItem> Items { get; set; } = new List<SaaSInvoiceItem>();
        public virtual ICollection<SaaSTransaction> Transactions { get; set; } = new List<SaaSTransaction>();
    }

    /// <summary>
    /// Invoice line items
    /// </summary>
    public class SaaSInvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        public int InvoiceId { get; set; }

        public int? ModuleId { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string ItemType { get; set; } = "Subscription";

        // Navigation
        [ForeignKey("InvoiceId")]
        public virtual SaaSInvoice? Invoice { get; set; }

        [ForeignKey("ModuleId")]
        public virtual SystemModule? Module { get; set; }
    }

    /// <summary>
    /// Payment transactions
    /// </summary>
    public class SaaSTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required, MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? TransactionRef { get; set; }

        public int ClientId { get; set; }

        public int? InvoiceId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        public int? PaymentMethodId { get; set; }

        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        [MaxLength(500)]
        public string? PaymentProof { get; set; }

        /// <summary>
        /// Base64 encoded image of payment proof (screenshot/receipt)
        /// </summary>
        public string? ProofImageBase64 { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "PHP";

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }

        public int? ProcessedBy { get; set; }

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual SaaSInvoice? Invoice { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
    }

    /// <summary>
    /// Support tickets
    /// </summary>
    public class SupportTicket
    {
        [Key]
        public int TicketId { get; set; }

        [Required, MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        public int ClientId { get; set; }

        public int? ClientUserId { get; set; }

        [Required, MaxLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "Normal";

        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public int? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        [MaxLength(4000)]
        public string? Resolution { get; set; }

        public int? SatisfactionRating { get; set; }

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }

        [ForeignKey("ClientUserId")]
        public virtual ClientUser? User { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }

    /// <summary>
    /// Ticket comments/replies
    /// </summary>
    public class TicketComment
    {
        [Key]
        public int CommentId { get; set; }

        public int TicketId { get; set; }

        public int? ClientUserId { get; set; }

        public int? OwnerId { get; set; }

        [Required, MaxLength(4000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; }

        [MaxLength(500)]
        public string? Attachment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("TicketId")]
        public virtual SupportTicket? Ticket { get; set; }

        [ForeignKey("ClientUserId")]
        public virtual ClientUser? User { get; set; }

        [ForeignKey("OwnerId")]
        public virtual SystemOwner? Owner { get; set; }
    }

    /// <summary>
    /// Activity log for audit trail
    /// </summary>
    public class SaaSActivityLog
    {
        [Key]
        public int LogId { get; set; }

        public int? OwnerId { get; set; }

        public int? ClientUserId { get; set; }

        public int? ClientId { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(4000)]
        public string? OldValues { get; set; }

        [MaxLength(4000)]
        public string? NewValues { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// License keys for validation
    /// </summary>
    public class LicenseKey
    {
        [Key]
        public int LicenseId { get; set; }

        public int ClientId { get; set; }

        [Required, MaxLength(256)]
        public string Key { get; set; } = string.Empty;

        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastValidatedAt { get; set; }

        public int ValidationCount { get; set; }

        [MaxLength(256)]
        public string? MachineId { get; set; }

        // Navigation
        [ForeignKey("ClientId")]
        public virtual SaaSClient? Client { get; set; }
    }
}
