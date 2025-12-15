using System.Text;
using FinanceBank.Models;

namespace FinanceBank.Services;

/// <summary>
/// Service for generating loan contract documents
/// </summary>
public class LoanContractService
{
    /// <summary>
    /// Generates HTML content for a loan contract that can be printed or saved as PDF
    /// </summary>
    public string GenerateLoanContractHtml(LoanContractData data)
    {
        var sb = new StringBuilder();
        
        sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Loan Agreement Contract - " + data.ContractReference + @"</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        @page { size: A4; margin: 15mm; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            font-size: 11pt; 
            line-height: 1.5; 
            color: #1e293b;
            background: white;
        }
        .container { max-width: 210mm; margin: 0 auto; padding: 20px; }
        .header { text-align: center; border-bottom: 3px solid #059669; padding-bottom: 20px; margin-bottom: 25px; }
        .logo { font-size: 28pt; font-weight: 800; color: #059669; letter-spacing: 2px; }
        .logo-sub { font-size: 12pt; color: #64748b; margin-top: 5px; }
        .doc-title { font-size: 20pt; font-weight: 700; color: #1e293b; margin-top: 20px; text-transform: uppercase; letter-spacing: 1px; }
        .contract-info { display: flex; justify-content: space-between; background: #f8fafc; padding: 15px 20px; border-radius: 8px; margin-bottom: 25px; }
        .contract-info div { text-align: center; }
        .contract-info label { font-size: 9pt; color: #64748b; text-transform: uppercase; display: block; }
        .contract-info strong { font-size: 13pt; color: #059669; }
        .section { margin-bottom: 25px; }
        .section-title { font-size: 13pt; font-weight: 700; color: #059669; border-bottom: 2px solid #bbf7d0; padding-bottom: 8px; margin-bottom: 15px; text-transform: uppercase; }
        .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px 30px; }
        .info-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e2e8f0; }
        .info-row label { color: #64748b; }
        .info-row strong { color: #1e293b; }
        .terms-box { background: #f0fdf4; border: 2px solid #bbf7d0; border-radius: 10px; padding: 20px; }
        .terms-table { width: 100%; border-collapse: collapse; }
        .terms-table td { padding: 10px 0; border-bottom: 1px solid #dcfce7; }
        .terms-table td:first-child { color: #475569; width: 55%; }
        .terms-table td:last-child { text-align: right; font-weight: 600; color: #1e293b; }
        .terms-table tr:last-child td { border-bottom: none; border-top: 2px solid #bbf7d0; padding-top: 15px; }
        .terms-table .total td:first-child { font-weight: 700; font-size: 12pt; color: #065f46; }
        .terms-table .total td:last-child { font-size: 14pt; font-weight: 800; color: #059669; }
        .penalty-box { background: #fef3c7; border: 2px solid #fde68a; border-radius: 10px; padding: 20px; margin-top: 25px; }
        .penalty-title { font-size: 12pt; font-weight: 700; color: #92400e; margin-bottom: 12px; }
        .penalty-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; font-size: 10pt; }
        .penalty-item { display: flex; gap: 10px; }
        .penalty-item label { color: #78350f; }
        .penalty-item strong { color: #dc2626; }
        .legal-warning { background: #1e293b; color: #e2e8f0; border-radius: 10px; padding: 20px; margin-top: 25px; }
        .legal-warning-title { font-size: 12pt; font-weight: 700; color: #f87171; margin-bottom: 12px; text-transform: uppercase; }
        .legal-warning p { font-size: 10pt; margin-bottom: 10px; line-height: 1.6; }
        .legal-warning strong { color: #fca5a5; }
        .timeline { background: #fee2e2; border: 2px solid #fecaca; border-radius: 10px; padding: 20px; margin-top: 25px; }
        .timeline-title { font-size: 12pt; font-weight: 700; color: #991b1b; margin-bottom: 15px; }
        .timeline-item { display: flex; gap: 15px; margin-bottom: 12px; font-size: 10pt; }
        .timeline-days { background: #dc2626; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700; min-width: 70px; text-align: center; }
        .timeline-desc { color: #7f1d1d; }
        .imprisonment-warning { 
            background: linear-gradient(135deg, #7f1d1d, #991b1b); 
            color: white; 
            border-radius: 10px; 
            padding: 25px; 
            margin-top: 25px; 
            text-align: center;
            border: 3px solid #dc2626;
        }
        .imprisonment-warning h3 { font-size: 14pt; margin-bottom: 12px; color: #fef2f2; }
        .imprisonment-warning p { font-size: 11pt; line-height: 1.7; }
        .imprisonment-warning .highlight { background: rgba(255,255,255,0.2); padding: 2px 8px; border-radius: 4px; font-weight: 700; }
        .acknowledgment { background: #f8fafc; border: 2px dashed #cbd5e1; border-radius: 10px; padding: 25px; margin-top: 30px; }
        .acknowledgment-title { font-size: 12pt; font-weight: 700; color: #475569; text-transform: uppercase; margin-bottom: 15px; }
        .acknowledgment ul { font-size: 10pt; color: #64748b; padding-left: 20px; margin-bottom: 25px; }
        .acknowledgment li { margin-bottom: 8px; }
        .signature-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 50px; margin-top: 30px; }
        .signature-box { text-align: center; }
        .signature-line { border-bottom: 2px solid #1e293b; padding-bottom: 10px; margin-bottom: 8px; min-height: 50px; }
        .signature-name { font-weight: 600; color: #1e293b; margin-bottom: 4px; }
        .signature-label { font-size: 10pt; color: #64748b; }
        .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #e2e8f0; text-align: center; font-size: 9pt; color: #94a3b8; }
        @media print {
            body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
            .no-print { display: none !important; }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>FINSYS</div>
            <div class='logo-sub'>Banking Corporation</div>
            <div class='doc-title'>Loan Agreement Contract</div>
        </div>

        <div class='contract-info'>
            <div>
                <label>Contract Reference</label>
                <strong>" + data.ContractReference + @"</strong>
            </div>
            <div>
                <label>Contract Date</label>
                <strong>" + data.ContractDate.ToString("MMMM dd, yyyy") + @"</strong>
            </div>
            <div>
                <label>Application ID</label>
                <strong>" + data.ApplicationNumber + @"</strong>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Borrower Information</div>
            <div class='info-grid'>
                <div class='info-row'><label>Full Name:</label><strong>" + data.BorrowerName + @"</strong></div>
                <div class='info-row'><label>Account Number:</label><strong>" + data.AccountNumber + @"</strong></div>
                <div class='info-row'><label>Contact Number:</label><strong>" + data.ContactNumber + @"</strong></div>
                <div class='info-row'><label>Email Address:</label><strong>" + data.EmailAddress + @"</strong></div>
                <div class='info-row'><label>Address:</label><strong>" + data.Address + @"</strong></div>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Loan Terms & Conditions</div>
            <div class='terms-box'>
                <table class='terms-table'>
                    <tr><td>Principal Amount</td><td>₱" + data.PrincipalAmount.ToString("N2") + @"</td></tr>
                    <tr><td>Annual Interest Rate</td><td>" + data.InterestRate.ToString("N2") + @"% per annum</td></tr>
                    <tr><td>Loan Term</td><td>" + data.TermMonths + @" months</td></tr>
                    <tr><td>Monthly Payment</td><td>₱" + data.MonthlyPayment.ToString("N2") + @"</td></tr>
                    <tr><td>First Payment Due Date</td><td>" + data.FirstDueDate.ToString("MMMM dd, yyyy") + @"</td></tr>
                    <tr><td>Final Payment Due Date</td><td>" + data.FinalDueDate.ToString("MMMM dd, yyyy") + @"</td></tr>
                    <tr><td>Total Interest</td><td>₱" + data.TotalInterest.ToString("N2") + @"</td></tr>
                    <tr class='total'><td>Total Amount Payable</td><td>₱" + data.TotalPayable.ToString("N2") + @"</td></tr>
                </table>
            </div>
        </div>

        <div class='penalty-box'>
            <div class='penalty-title'>⚠️ Penalty & Grace Period Terms</div>
            <div class='penalty-grid'>
                <div class='penalty-item'><label>Grace Period:</label><strong>5 calendar days</strong></div>
                <div class='penalty-item'><label>Daily Penalty Rate:</label><strong>0.05% per day</strong></div>
                <div class='penalty-item'><label>Maximum Penalty:</label><strong>25% per billing cycle</strong></div>
                <div class='penalty-item'><label>Late Payment Fee:</label><strong>₱500.00 per missed payment</strong></div>
            </div>
        </div>

        <div class='timeline'>
            <div class='timeline-title'>⚖️ Legal Escalation Timeline (Philippine Banking Standards)</div>
            <div class='timeline-item'>
                <div class='timeline-days'>30 DAYS</div>
                <div class='timeline-desc'><strong>Written Demand Letter</strong> - Formal notice of default. Account flagged as delinquent.</div>
            </div>
            <div class='timeline-item'>
                <div class='timeline-days'>60 DAYS</div>
                <div class='timeline-desc'><strong>Legal Action Notice</strong> - Case referred to legal counsel. Lawyer's demand letter sent.</div>
            </div>
            <div class='timeline-item'>
                <div class='timeline-days'>90 DAYS</div>
                <div class='timeline-desc'><strong>Court Filing</strong> - Civil case filed. May result in court summons and asset seizure.</div>
            </div>
            <div class='timeline-item'>
                <div class='timeline-days'>120+ DAYS</div>
                <div class='timeline-desc'><strong>Criminal Prosecution</strong> - For fraud, criminal charges filed under RPC Art. 315 (Estafa).</div>
            </div>
        </div>

        <div class='imprisonment-warning'>
            <h3>🚨 CRIMINAL LIABILITY WARNING 🚨</h3>
            <p>
                Under the <span class='highlight'>Revised Penal Code of the Philippines</span>, borrowers who obtain loans through 
                <strong>fraudulent means, misrepresentation, or deceit</strong> may be criminally liable for <span class='highlight'>ESTAFA (Article 315)</span> 
                which carries a penalty of <strong>imprisonment ranging from 6 months to 20 years</strong> depending on the amount involved.
            </p>
            <p style='margin-top: 15px;'>
                Additionally, issuance of <strong>bounced checks</strong> for loan payments is punishable under <span class='highlight'>Batas Pambansa Blg. 22</span> 
                with <strong>imprisonment of up to 1 year</strong> and/or fine of up to double the amount of the check.
            </p>
            <p style='margin-top: 15px; font-size: 10pt; opacity: 0.9;'>
                The bank reserves the right to file criminal complaints and pursue legal action to the fullest extent of the law.
            </p>
        </div>

        <div class='legal-warning'>
            <div class='legal-warning-title'>🔒 Important Legal Notices</div>
            <p><strong>1. Credit Bureau Reporting:</strong> All payment history will be reported to Credit Information Corporation (CIC) 
            and other credit bureaus. Late payments will negatively affect your credit score for up to 10 years.</p>
            <p><strong>2. Collateral Seizure:</strong> For secured loans, failure to pay may result in foreclosure or repossession 
            of pledged assets without further court order.</p>
            <p><strong>3. Co-Borrower Liability:</strong> Co-borrowers and guarantors are jointly and severally liable 
            for the full amount including penalties, interest, and collection costs.</p>
            <p><strong>4. Collection Costs:</strong> Borrower agrees to pay all costs of collection including attorney's fees 
            (25% of principal + interest) and court costs in case of default.</p>
        </div>

        <div class='acknowledgment'>
            <div class='acknowledgment-title'>📝 Borrower's Acknowledgment</div>
            <p style='font-size: 11pt; color: #475569; margin-bottom: 15px;'>
                By signing this contract, I, <strong>" + data.BorrowerName + @"</strong>, hereby acknowledge and agree that:
            </p>
            <ul>
                <li>I have read and fully understood all terms, conditions, penalties, and legal consequences stated in this agreement.</li>
                <li>I agree to pay the monthly installment of <strong>₱" + data.MonthlyPayment.ToString("N2") + @"</strong> on or before the due date each month.</li>
                <li>I understand that late payments will incur daily penalties of 0.05% after a 5-day grace period.</li>
                <li>I am aware that persistent non-payment may result in legal action, credit score damage, asset seizure, and criminal prosecution.</li>
                <li>All information provided in my loan application is true, accurate, and complete to the best of my knowledge.</li>
                <li>I understand that providing false information constitutes fraud and may result in criminal charges.</li>
                <li>I voluntarily enter into this loan agreement without any duress or undue influence.</li>
            </ul>
            <div class='signature-grid'>
                <div class='signature-box'>
                    <div class='signature-line'></div>
                    <div class='signature-name'>" + data.BorrowerName + @"</div>
                    <div class='signature-label'>Borrower's Signature Over Printed Name</div>
                </div>
                <div class='signature-box'>
                    <div class='signature-line'></div>
                    <div class='signature-name'>" + data.BankRepresentative + @"</div>
                    <div class='signature-label'>Bank Representative</div>
                </div>
            </div>
            <div style='margin-top: 25px; text-align: center;'>
                <div style='display: inline-block; margin-right: 50px;'>
                    <label style='font-size: 10pt; color: #64748b;'>Date Signed:</label>
                    <strong style='margin-left: 10px;'>_________________________</strong>
                </div>
                <div style='display: inline-block;'>
                    <label style='font-size: 10pt; color: #64748b;'>Valid ID Presented:</label>
                    <strong style='margin-left: 10px;'>_________________________</strong>
                </div>
            </div>
        </div>

        <div class='footer'>
            <p>This document is system-generated and is valid without signature when electronically verified.</p>
            <p>FINSYS Banking Corporation | Main Branch: 123 Financial District, Metro Manila, Philippines</p>
            <p>Customer Service: (02) 8888-BANK | Email: support@finsys.com.ph | www.finsys.com.ph</p>
            <p style='margin-top: 10px;'>Document Generated: " + DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt") + @" | Document ID: " + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper() + @"</p>
        </div>
    </div>
</body>
</html>");

        return sb.ToString();
    }
}

/// <summary>
/// Data model for loan contract generation
/// </summary>
public class LoanContractData
{
    public string ContractReference { get; set; } = "";
    public DateTime ContractDate { get; set; } = DateTime.Now;
    public string ApplicationNumber { get; set; } = "";
    
    // Borrower Info
    public string BorrowerName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string ContactNumber { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public string Address { get; set; } = "";
    
    // Loan Terms
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime FirstDueDate { get; set; }
    public DateTime FinalDueDate { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayable { get; set; }
    
    // Bank Info
    public string BankRepresentative { get; set; } = "";

    // Asset Purchase Info (for asset marketplace loans)
    public bool IsAssetPurchase { get; set; } = false;
    public string? AssetType { get; set; } // Property, Vehicle, Other
    public string? AssetName { get; set; }
    public string? AssetDescription { get; set; }
    public decimal AssetTotalPrice { get; set; }
    public decimal DownPaymentAmount { get; set; }
    
    // Property-specific
    public string? PropertyType { get; set; }
    public string? PropertyLocation { get; set; }
    public decimal? LandAreaSqm { get; set; }
    public decimal? FloorAreaSqm { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public string? TitleStatus { get; set; }
    public string? DeedOfSaleNumber { get; set; }
    
    // Vehicle-specific
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleCondition { get; set; }
    public string? ORNumber { get; set; } // Official Receipt
    public string? CRNumber { get; set; } // Certificate of Registration
    
    // Other asset
    public string? OtherAssetCategory { get; set; }
    public string? SalesAgreementNumber { get; set; }
}
