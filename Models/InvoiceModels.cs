using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceBank.Models;

/// <summary>
/// Invoice model for deposits and withdrawals
/// </summary>
[Table("Invoices")]
public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = "";

    [Required, MaxLength(50)]
    public string InvoiceType { get; set; } = ""; // Deposit, Withdrawal

    [Required]
    public int AccountId { get; set; }

    [Required]
    public int TransactionId { get; set; }

    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = "";

    [Required, MaxLength(50)]
    public string AccountNumber { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceBefore { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TransactionAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }

    [Required, MaxLength(50)]
    public string TransactionMethod { get; set; } = "";

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required, MaxLength(100)]
    public string ProcessedBy { get; set; } = "";

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Completed";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? PrintedAt { get; set; }

    public DateTime? DownloadedAt { get; set; }

    // Navigation
    [ForeignKey("AccountId")]
    public virtual CustomerAccount? Account { get; set; }

    [ForeignKey("TransactionId")]
    public virtual CustomerTransaction? Transaction { get; set; }
}
