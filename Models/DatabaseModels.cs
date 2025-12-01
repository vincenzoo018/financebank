using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Models;

// =============================================
// ADDITIONAL DATABASE MODELS FOR CRUD
// =============================================

[Table("BankAccounts")]
public class BankAccount
{
    [Key]
    public int BankAccountId { get; set; }

    [Required, MaxLength(50)]
    public string AccountNumber { get; set; } = "";

    [Required, MaxLength(100)]
    public string AccountName { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountType { get; set; } = "";

    [Required, MaxLength(100)]
    public string BankName { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    [MaxLength(3)]
    public string Currency { get; set; } = "PHP";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [MaxLength(50)]
    public string? LastModifiedBy { get; set; }
}

[Table("FundTransfers")]
public class FundTransfer
{
    [Key]
    public int TransferId { get; set; }

    [Required, MaxLength(50)]
    public string TransferNumber { get; set; } = "";

    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int ToAccountId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; } = 0;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }

    [MaxLength(50)]
    public string? ProcessedBy { get; set; }

    [MaxLength(50)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    // Navigation
    [ForeignKey("FromAccountId")]
    public virtual BankAccount? FromAccount { get; set; }

    [ForeignKey("ToAccountId")]
    public virtual BankAccount? ToAccount { get; set; }
}

// =============================================
// CHART OF ACCOUNTS MODEL
// =============================================

[Table("ChartOfAccounts")]
public class ChartOfAccounts
{
    [Key]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string AccountCode { get; set; } = "";

    [Required, MaxLength(100)]
    public string AccountName { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountType { get; set; } = ""; // Asset, Liability, Equity, Revenue, Expense

    [MaxLength(50)]
    public string? SubAccountType { get; set; } // More specific type

    [Required, MaxLength(10)]
    public string NormalBalance { get; set; } = "Debit"; // Debit or Credit

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsHeader { get; set; } = false; // If true, this is a header/parent account

    public int? ParentAccountId { get; set; } // For hierarchical account structure

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string CreatedBy { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey("ParentAccountId")]
    public virtual ChartOfAccounts? ParentAccount { get; set; }

    public virtual ICollection<ChartOfAccounts> SubAccounts { get; set; } = new List<ChartOfAccounts>();
}

[Table("JournalEntries")]
public class JournalEntry
{
    [Key]
    public int JournalId { get; set; }

    [Required, MaxLength(50)]
    public string JournalNumber { get; set; } = "";

    public DateTime TransactionDate { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = "";

    [MaxLength(100)]
    public string? Reference { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; } = 0;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Posted, Reversed

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string CreatedBy { get; set; } = "";

    public DateTime? PostedAt { get; set; }

    [MaxLength(50)]
    public string? PostedBy { get; set; }

    // Navigation
    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

[Table("JournalEntryLines")]
public class JournalEntryLine
{
    [Key]
    public int LineId { get; set; }

    [Required]
    public int JournalId { get; set; }

    [Required, MaxLength(50)]
    public string AccountCode { get; set; } = "";

    [Required, MaxLength(100)]
    public string AccountName { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditAmount { get; set; } = 0;

    // Navigation
    [ForeignKey("JournalId")]
    public virtual JournalEntry? Journal { get; set; }
}

[Table("Budgets")]
public class Budget
{
    [Key]
    public int BudgetId { get; set; }

    [Required, MaxLength(100)]
    public string BudgetName { get; set; } = "";

    [Required, MaxLength(50)]
    public string Department { get; set; } = "";

    [Required, MaxLength(50)]
    public string Category { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal AllocatedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }
}

// =============================================
// CUSTOMER PORTAL MODELS
// =============================================

[Table("CustomerAccounts")]
public class CustomerAccount
{
    [Key]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string AccountNumber { get; set; } = "";

    [Required]
    public int CustomerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AvailableBalance { get; set; } = 0;

    [MaxLength(3)]
    public string Currency { get; set; } = "PHP";

    [MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastTransactionAt { get; set; }

    // Navigation
    [ForeignKey("CustomerId")]
    public virtual AuthUser? Customer { get; set; }

    public virtual ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public virtual ICollection<SavingsGoalEntity> SavingsGoals { get; set; } = new List<SavingsGoalEntity>();
    public virtual ICollection<RewardPointsEntity> RewardPoints { get; set; } = new List<RewardPointsEntity>();

    // Helper method to generate unique 6-digit account number
    public static string GenerateAccountNumber()
    {
        // Generate unique 6-digit account number: ACC-XXXXXX
        var random = new Random();
        var sixDigitNumber = random.Next(100000, 999999);
        return $"ACC-{sixDigitNumber}";
    }
}

[Table("CustomerTransactions")]
public class CustomerTransaction
{
    [Key]
    public int TransactionId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionNumber { get; set; } = "";

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = ""; // Transfer, Deposit, Withdrawal, Bills, etc.

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; } = 0;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Cancelled

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    // For transfers
    public int? ToAccountId { get; set; }
    [MaxLength(100)]
    public string? ToAccountName { get; set; }

    // For bills payment
    [MaxLength(100)]
    public string? BillerName { get; set; }
    [MaxLength(50)]
    public string? BillerAccountNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedByEmployeeId { get; set; } // FK to Employees table

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    [ForeignKey("ToAccountId")]
    public virtual CustomerAccount? ToAccount { get; set; }

    [ForeignKey("ProcessedByEmployeeId")]
    public virtual Employee? ProcessedByEmployee { get; set; }
}

[Table("Cards")]
public class Card
{
    [Key]
    public int CardId { get; set; }

    [Required, MaxLength(20)]
    public string CardNumber { get; set; } = "";

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string CardType { get; set; } = ""; // Debit, Credit

    [Required, MaxLength(100)]
    public string CardName { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AvailableCredit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingBalance { get; set; } = 0;

    public DateTime ExpiryDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

[Table("CustomerLoans")]
public class Loan
{
    [Key]
    public int LoanId { get; set; }

    [Required, MaxLength(50)]
    public string LoanNumber { get; set; } = "";

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string LoanType { get; set; } = ""; // Personal, Home, Auto, Education

    [Column(TypeName = "decimal(18,2)")]
    public decimal LoanAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingBalance { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }

    public int TermMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPayment { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? NextDueDate { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Paid, Defaulted, Completed

    [MaxLength(500)]
    public string? Purpose { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Loan Process Tracking Fields
    public int? ApplicationId { get; set; }
    public int? AssessmentId { get; set; }
    public int? ApprovalId { get; set; }
    public int? DisbursalId { get; set; }

    public int CumulativeLateDays { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPenalties { get; set; } = 0;

    public DateTime? LastPaymentDate { get; set; }

    public bool IsBlacklisted { get; set; } = false;
    public DateTime? BlacklistedAt { get; set; }

    [MaxLength(500)]
    public string? BlacklistedReason { get; set; }

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    [ForeignKey("ApplicationId")]
    public virtual LoanApplication? LoanApplication { get; set; }

    [ForeignKey("AssessmentId")]
    public virtual LoanAssessment? LoanAssessment { get; set; }

    [ForeignKey("ApprovalId")]
    public virtual LoanApproval? LoanApprovalRecord { get; set; }

    [ForeignKey("DisbursalId")]
    public virtual LoanDisbursal? LoanDisbursalRecord { get; set; }

    public virtual ICollection<LoanPaymentSchedule>? PaymentSchedules { get; set; }
    public virtual ICollection<LoanPayment>? Payments { get; set; }
    public virtual ICollection<LoanViolation>? Violations { get; set; }
}

[Table("SavingsGoals")]
public class SavingsGoalEntity
{
    [Key]
    public int GoalId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(100)]
    public string GoalName { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentAmount { get; set; } = 0;

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime TargetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

[Table("RewardPoints")]
public class RewardPointsEntity
{
    [Key]
    public int RewardId { get; set; }

    [Required]
    public int AccountId { get; set; }

    public int Points { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CashbackAmount { get; set; } = 0;

    [Required, MaxLength(50)]
    public string EarnedFrom { get; set; } = ""; // Transaction, Promotion, etc.

    [MaxLength(100)]
    public string? Description { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiresAt { get; set; }

    public bool IsRedeemed { get; set; } = false;
    public DateTime? RedeemedAt { get; set; }

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

// =============================================
// BANKING MODULE MODELS (Admin/Accountant)
// =============================================

[Table("BillersManagement")]
public class Biller
{
    [Key]
    public int BillerId { get; set; }

    [Required, MaxLength(100)]
    public string BillerName { get; set; } = "";

    [Required, MaxLength(50)]
    public string BillerCode { get; set; } = "";

    [Required, MaxLength(50)]
    public string Category { get; set; } = ""; // Utilities, Telecom, Government, etc.

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFee { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("BankingReports")]
public class BankingReport
{
    [Key]
    public int ReportId { get; set; }

    [Required, MaxLength(50)]
    public string ReportType { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ReportName { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Data { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string GeneratedBy { get; set; } = string.Empty;
}

[Table("LoanManagement")]
public class LoanManagementEntity
{
    [Key]
    public int LoanMgmtId { get; set; }

    [Required]
    public int CustomerLoanId { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ApprovalStatus { get; set; } = "Pending";

    [MaxLength(50)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [MaxLength(1000)]
    public string? ProcessingNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("CardManagement")]
public class CardManagementEntity
{
    [Key]
    public int CardMgmtId { get; set; }

    [Required]
    public int CustomerCardId { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required, MaxLength(50)]
    public string ActionBy { get; set; } = string.Empty;

    public DateTime ActionAt { get; set; } = DateTime.Now;
}

// =============================================
// ACCOUNTING MODULE MODELS
// =============================================

[Table("AccountsPayable")]
public class AccountsPayable
{
    [Key]
    public int PayableId { get; set; }

    [Required, MaxLength(100)]
    public string VendorName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Partially Paid, Paid, Overdue

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
}

[Table("AccountsReceivable")]
public class AccountsReceivable
{
    [Key]
    public int ReceivableId { get; set; }

    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; } = 0;

    [Required, MaxLength(100)]
    public string Status { get; set; } = "Pending"; // Pending, Pending Review, FM Approved - Pending Accountant, Partially Paid, Paid, Overdue

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
}

[Table("GeneralLedger")]
public class GeneralLedgerEntry
{
    [Key]
    public int EntryId { get; set; }

    [Required, MaxLength(50)]
    public string EntryNumber { get; set; } = "";

    [Required]
    public int JournalId { get; set; }

    [Required, MaxLength(50)]
    public string AccountCode { get; set; } = "";

    [Required, MaxLength(100)]
    public string AccountName { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountType { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0; // Running balance

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    [MaxLength(50)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? CostCenter { get; set; }

    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Posted"; // Posted, Reversed

    // Navigation
    [ForeignKey("JournalId")]
    public virtual JournalEntry? Journal { get; set; }
}

[Table("TrialBalance")]
public class TrialBalanceEntry
{
    [Key]
    public int TrialBalanceId { get; set; }

    [Required, MaxLength(50)]
    public string AccountCode { get; set; } = "";

    [Required, MaxLength(100)]
    public string AccountName { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountType { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditBalance { get; set; } = 0;

    public DateTime AsOfDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Balanced, Unbalanced

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebits { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredits { get; set; } = 0;

    // Is balanced if TotalDebits == TotalCredits
    public bool IsBalanced => TotalDebits == TotalCredits;
}

[Table("FinancialStatements")]
public class FinancialStatement
{
    [Key]
    public int StatementId { get; set; }

    [Required, MaxLength(50)]
    public string StatementType { get; set; } = ""; // IncomeStatement, BalanceSheet, CashFlowStatement

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAssets { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalLiabilities { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEquity { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRevenue { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExpenses { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetIncome { get; set; } = 0;

    [Column(TypeName = "nvarchar(max)")]
    public string? Data { get; set; } // JSON detailed data

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? GeneratedBy { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Final, Archived
}

// =============================================
// FINANCE MODULE MODELS
// =============================================

// Removed duplicate Budget class - using the one from the top of the file

[Table("CashflowEntries")]
public class CashflowEntry
{
    [Key]
    public int CashflowId { get; set; }

    [Required, MaxLength(50)]
    public string EntryType { get; set; } = ""; // Inflow, Outflow

    [Required, MaxLength(50)]
    public string Category { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }
}

[Table("FinancialForecasting")]
public class FinancialForecast
{
    [Key]
    public int ForecastId { get; set; }

    [Required, MaxLength(100)]
    public string ForecastName { get; set; } = "";

    [Required, MaxLength(50)]
    public string ForecastType { get; set; } = ""; // Revenue, Expense, Cashflow

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProjectedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualAmount { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Variance { get; set; } = 0;

    [MaxLength(int.MaxValue)]
    public string? Assumptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }
}

// =============================================
// SYSTEM MODULE MODELS
// =============================================

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public int AuditId { get; set; }

    [Required, MaxLength(50)]
    public string UserId { get; set; } = "";

    [Required, MaxLength(100)]
    public string Action { get; set; } = "";

    [Required, MaxLength(100)]
    public string Module { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(255)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// =============================================
// ACCOUNTING ENTRY MODEL (Double-Entry Bookkeeping)
// =============================================

[Table("AccountingEntries")]
public class AccountingEntry
{
    [Key]
    public int EntryId { get; set; }

    public int? AccountId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountType { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    [MaxLength(100)]
    public string Reference { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";

    [MaxLength(50)]
    public string ProcessedBy { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }
}

// =============================================
// APPROVAL MODULE MODELS
// =============================================

[Table("ApprovalQueue")]
public class ApprovalQueueEntity
{
    [Key]
    public int ApprovalId { get; set; }

    [Required, MaxLength(50)]
    public string RequestType { get; set; } = ""; // Transfer, Deposit, Withdrawal, Loan

    [Required]
    public int RequestId { get; set; }

    [Required, MaxLength(50)]
    public string RequestedBy { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    [Required, MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }

    [MaxLength(50)]
    public string? ProcessedBy { get; set; }

    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }
}

// =============================================
// CUSTOMER CARD MODELS
// =============================================

[Table("CustomerCards")]
public class CustomerCardEntity
{
    [Key]
    public int CardId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string CardNumber { get; set; } = ""; // Encrypted

    [Required, MaxLength(50)]
    public string CardType { get; set; } = ""; // Debit or Credit

    [Required, MaxLength(100)]
    public string CardName { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AvailableCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingBalance { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }

    public bool IsActive { get; set; }

    public bool IsLocked { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// =============================================
// USER REGISTRATION MODEL
// =============================================

[Table("UserRegistrations")]
public class UserRegistration
{
    [Key]
    public int? RegistrationId { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = "";

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(50)]
    public string Role { get; set; } = "Customer";

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? EmployeeId { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }
}
