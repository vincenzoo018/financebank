namespace FinanceBank.Services;

/// <summary>
/// Tax calculation service based on BIR (Bureau of Internal Revenue) regulations
/// </summary>
public class TaxCalculationService
{
    // Transaction tax rates (DST - Documentary Stamp Tax)
    private const decimal DocumentaryStampTaxRate = 0.0015m; // 0.15% on transactions above threshold
    private const decimal TransactionTaxThreshold = 1000m; // Threshold for DST application

    // Interest withholding tax (20% final tax on interest income per BIR)
    private const decimal InterestWithholdingTaxRate = 0.20m;

    /// <summary>
    /// Calculate documentary stamp tax on a transaction
    /// </summary>
    public decimal CalculateDocumentaryStampTax(decimal amount)
    {
        if (amount <= TransactionTaxThreshold)
            return 0m;

        var tax = amount * DocumentaryStampTaxRate;
        return Math.Round(tax, 2);
    }

    /// <summary>
    /// Calculate withholding tax on interest income
    /// </summary>
    public decimal CalculateInterestWithholdingTax(decimal interestAmount)
    {
        if (interestAmount <= 0)
            return 0m;

        var tax = interestAmount * InterestWithholdingTaxRate;
        return Math.Round(tax, 2);
    }

    /// <summary>
    /// Calculate net interest after tax deduction
    /// </summary>
    public decimal CalculateNetInterest(decimal grossInterest)
    {
        var tax = CalculateInterestWithholdingTax(grossInterest);
        return grossInterest - tax;
    }

    /// <summary>
    /// Calculate total transaction cost including tax
    /// </summary>
    public TransactionTaxBreakdown CalculateTransactionCost(decimal amount, string transactionType)
    {
        var breakdown = new TransactionTaxBreakdown
        {
            BaseAmount = amount,
            TransactionType = transactionType
        };

        // Apply DST for applicable transactions
        if (transactionType.ToUpper() is "WITHDRAWAL" or "TRANSFER")
        {
            breakdown.DocumentaryStampTax = CalculateDocumentaryStampTax(amount);
        }

        breakdown.TotalTax = breakdown.DocumentaryStampTax;
        breakdown.NetAmount = amount - breakdown.TotalTax;
        breakdown.GrossAmount = amount;

        return breakdown;
    }

    /// <summary>
    /// Calculate transaction with fees and taxes
    /// </summary>
    public TransactionTaxBreakdown CalculateTransactionWithFees(
        decimal amount,
        string transactionType,
        decimal additionalFees = 0m)
    {
        var breakdown = CalculateTransactionCost(amount, transactionType);
        breakdown.TransactionFees = additionalFees;
        breakdown.TotalTax += additionalFees;
        breakdown.NetAmount = amount - breakdown.TotalTax;

        return breakdown;
    }

    /// <summary>
    /// Calculate interest with tax deduction breakdown
    /// </summary>
    public InterestTaxBreakdown CalculateInterestBreakdown(
        decimal principal,
        decimal interestRate,
        int termInMonths)
    {
        var grossInterest = principal * (interestRate / 100) * (termInMonths / 12m);
        var withholdingTax = CalculateInterestWithholdingTax(grossInterest);
        var netInterest = grossInterest - withholdingTax;

        return new InterestTaxBreakdown
        {
            Principal = principal,
            InterestRate = interestRate,
            TermInMonths = termInMonths,
            GrossInterest = Math.Round(grossInterest, 2),
            WithholdingTax = withholdingTax,
            NetInterest = Math.Round(netInterest, 2),
            TotalRepayment = principal + Math.Round(grossInterest, 2)
        };
    }

    /// <summary>
    /// Validate if tax calculation is correct
    /// </summary>
    public bool ValidateTaxCalculation(decimal amount, decimal calculatedTax, string transactionType)
    {
        var expected = CalculateTransactionCost(amount, transactionType);
        return Math.Abs(expected.TotalTax - calculatedTax) < 0.01m; // Allow 1 centavo variance
    }
}

public class TransactionTaxBreakdown
{
    public decimal BaseAmount { get; set; }
    public decimal DocumentaryStampTax { get; set; }
    public decimal TransactionFees { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public string TransactionType { get; set; } = "";
}

public class InterestTaxBreakdown
{
    public decimal Principal { get; set; }
    public decimal InterestRate { get; set; }
    public int TermInMonths { get; set; }
    public decimal GrossInterest { get; set; }
    public decimal WithholdingTax { get; set; }
    public decimal NetInterest { get; set; }
    public decimal TotalRepayment { get; set; }
}
