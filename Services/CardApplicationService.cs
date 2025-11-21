using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services;

public class CardApplicationService
{
    private readonly BFASDbContext _context;
    private readonly AuthService _authService;

    public CardApplicationService(BFASDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // ==================== CARD APPLICATIONS ====================

    /// <summary>
    /// Submit a new card application
    /// </summary>
    public async Task<(bool success, string message, int approvalId)> SubmitCardApplicationAsync(
        string cardType, 
        string deliveryMethod, 
        string? notes = null)
    {
        try
        {
            if (!_authService.CurrentUserId.HasValue)
                return (false, "User not authenticated", 0);
            
            var userId = _authService.CurrentUserId.Value;

            // Get user's account
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return (false, "User not found", 0);

            // Create approval queue entry
            var approval = new ApprovalQueueEntity
            {
                RequestType = "CARD",
                RequestId = 0, // Will be updated after card creation
                RequestedBy = userId.ToString(),
                Amount = 0, // Card applications don't have amounts
                Status = "Pending",
                Priority = "Normal",
                Description = $"{cardType} Card Application - {deliveryMethod} Delivery",
                RequestedAt = DateTime.Now,
                ApprovalNotes = notes ?? ""
            };

            _context.ApprovalQueues.Add(approval);
            await _context.SaveChangesAsync();

            return (true, $"{cardType} card application submitted successfully", approval.ApprovalId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error submitting card application: {ex.Message}");
            return (false, $"Error: {ex.Message}", 0);
        }
    }

    /// <summary>
    /// Get all pending card applications
    /// </summary>
    public async Task<List<CardApplicationDto>> GetPendingCardApplicationsAsync()
    {
        try
        {
            var applications = await _context.ApprovalQueues
                .Where(a => a.RequestType == "CARD" && a.Status == "Pending")
                .Join(_context.Users,
                    a => a.RequestedBy,
                    u => u.UserId.ToString(),
                    (a, u) => new CardApplicationDto
                    {
                        ApprovalId = a.ApprovalId,
                        RequestedBy = a.RequestedBy,
                        CustomerName = u.FullName,
                        CustomerEmail = u.Email,
                        CardType = ExtractCardType(a.Description),
                        DeliveryMethod = ExtractDeliveryMethod(a.Description),
                        RequestedAt = a.RequestedAt,
                        Status = a.Status,
                        Priority = a.Priority,
                        Notes = a.ApprovalNotes
                    })
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            return applications;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching card applications: {ex.Message}");
            return new List<CardApplicationDto>();
        }
    }

    /// <summary>
    /// Get all card applications (pending, approved, rejected)
    /// </summary>
    public async Task<List<CardApplicationDto>> GetAllCardApplicationsAsync(string? status = null)
    {
        try
        {
            // Materialize ALL approvals first, then do robust in-memory filtering
            // so that trailing spaces / casing in RequestType or Status never hide rows.
            var allApprovals = await _context.ApprovalQueues
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            var targetStatus = string.IsNullOrWhiteSpace(status)
                ? null
                : status.Trim().ToUpper();

            var approvals = allApprovals
                .Where(a =>
                {
                    var rt = (a.RequestType ?? string.Empty).Trim().ToUpper();
                    if (rt != "CARD")
                        return false;

                    if (targetStatus == null)
                        return true;

                    var st = (a.Status ?? string.Empty).Trim().ToUpper();
                    return st == targetStatus;
                })
                .ToList();

            // Preload related users into a dictionary (may be empty if some users are missing)
            var userIds = approvals
                .Select(a =>
                {
                    return int.TryParse(a.RequestedBy, out var id)
                        ? (int?)id
                        : null;
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var userDict = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            // Map to DTOs, falling back gracefully if user info is missing
            var applications = approvals
                .Select(a =>
                {
                    string customerName = a.RequestedBy;
                    string customerEmail = string.Empty;

                    if (int.TryParse(a.RequestedBy, out var uid) && userDict.TryGetValue(uid, out var user))
                    {
                        customerName = user.FullName;
                        customerEmail = user.Email;
                    }

                    return new CardApplicationDto
                    {
                        ApprovalId = a.ApprovalId,
                        RequestedBy = a.RequestedBy,
                        CustomerName = customerName,
                        CustomerEmail = customerEmail,
                        CardType = ExtractCardType(a.Description ?? string.Empty),
                        DeliveryMethod = ExtractDeliveryMethod(a.Description ?? string.Empty),
                        RequestedAt = a.RequestedAt,
                        Status = a.Status,
                        Priority = a.Priority,
                        Notes = a.ApprovalNotes ?? string.Empty,
                        ProcessedAt = a.ProcessedAt,
                        ProcessedBy = a.ProcessedBy
                    };
                })
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[CardApplications] Loaded {applications.Count} applications for status '{status ?? "ALL"}'.");

            return applications;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching card applications: {ex.Message}");
            return new List<CardApplicationDto>();
        }
    }

    /// <summary>
    /// Approve a card application
    /// </summary>
    public async Task<(bool success, string message)> ApproveCardApplicationAsync(
        int approvalId, 
        string? adminNotes = null)
    {
        try
        {
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
            if (approval == null)
                return (false, "Application not found");

            approval.Status = "Approved";
            approval.ProcessedAt = DateTime.Now;
            approval.ProcessedBy = _authService.CurrentUserId?.ToString();
            approval.ApprovalNotes = adminNotes ?? approval.ApprovalNotes;

            _context.ApprovalQueues.Update(approval);
            await _context.SaveChangesAsync();

            return (true, "Card application approved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error approving card application: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reject a card application
    /// </summary>
    public async Task<(bool success, string message)> RejectCardApplicationAsync(
        int approvalId, 
        string rejectionReason)
    {
        try
        {
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
            if (approval == null)
                return (false, "Application not found");

            approval.Status = "Rejected";
            approval.ProcessedAt = DateTime.Now;
            approval.ProcessedBy = _authService.CurrentUserId?.ToString();
            approval.ApprovalNotes = $"Rejection Reason: {rejectionReason}";

            _context.ApprovalQueues.Update(approval);
            await _context.SaveChangesAsync();

            return (true, "Card application rejected");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rejecting card application: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== LOAN APPLICATIONS ====================

    /// <summary>
    /// Submit a new loan application
    /// </summary>
    public async Task<(bool success, string message, int approvalId)> SubmitLoanApplicationAsync(
        string loanType,
        decimal requestedAmount,
        int loanTerm,
        string? purpose = null,
        string? notes = null)
    {
        try
        {
            if (!_authService.CurrentUserId.HasValue)
                return (false, "User not authenticated", 0);
            
            var userId = _authService.CurrentUserId.Value;

            var approval = new ApprovalQueueEntity
            {
                RequestType = "LOAN",
                RequestId = 0,
                RequestedBy = userId.ToString(),
                Amount = requestedAmount,
                Status = "Pending",
                Priority = "Normal",
                Description = $"{loanType} Loan - ₱{requestedAmount:N2} for {loanTerm} months. Purpose: {purpose}",
                RequestedAt = DateTime.Now,
                ApprovalNotes = notes ?? ""
            };

            _context.ApprovalQueues.Add(approval);
            await _context.SaveChangesAsync();

            return (true, $"{loanType} loan application submitted successfully", approval.ApprovalId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error submitting loan application: {ex.Message}");
            return (false, $"Error: {ex.Message}", 0);
        }
    }

    /// <summary>
    /// Get all pending loan applications
    /// </summary>
    public async Task<List<LoanApplicationDto>> GetPendingLoanApplicationsAsync()
    {
        try
        {
            var applications = await _context.ApprovalQueues
                .Where(a => a.RequestType == "LOAN" && a.Status == "Pending")
                .Join(_context.Users,
                    a => a.RequestedBy,
                    u => u.UserId.ToString(),
                    (a, u) => new LoanApplicationDto
                    {
                        ApprovalId = a.ApprovalId,
                        RequestedBy = a.RequestedBy,
                        CustomerName = u.FullName,
                        CustomerEmail = u.Email,
                        LoanType = ExtractLoanType(a.Description),
                        RequestedAmount = a.Amount,
                        LoanTerm = ExtractLoanTerm(a.Description),
                        Purpose = ExtractPurpose(a.Description),
                        RequestedAt = a.RequestedAt,
                        Status = a.Status,
                        Priority = a.Priority,
                        Notes = a.ApprovalNotes
                    })
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            return applications;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching loan applications: {ex.Message}");
            return new List<LoanApplicationDto>();
        }
    }

    /// <summary>
    /// Get all loan applications (pending, approved, rejected)
    /// </summary>
    public async Task<List<LoanApplicationDto>> GetAllLoanApplicationsAsync(string? status = null)
    {
        try
        {
            var query = _context.ApprovalQueues
                .Where(a => a.RequestType == "LOAN");

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var applications = await query
                .Join(_context.Users,
                    a => a.RequestedBy,
                    u => u.UserId.ToString(),
                    (a, u) => new LoanApplicationDto
                    {
                        ApprovalId = a.ApprovalId,
                        RequestedBy = a.RequestedBy,
                        CustomerName = u.FullName,
                        CustomerEmail = u.Email,
                        LoanType = ExtractLoanType(a.Description),
                        RequestedAmount = a.Amount,
                        LoanTerm = ExtractLoanTerm(a.Description),
                        Purpose = ExtractPurpose(a.Description),
                        RequestedAt = a.RequestedAt,
                        Status = a.Status,
                        Priority = a.Priority,
                        Notes = a.ApprovalNotes,
                        ProcessedAt = a.ProcessedAt,
                        ProcessedBy = a.ProcessedBy
                    })
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            return applications;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching loan applications: {ex.Message}");
            return new List<LoanApplicationDto>();
        }
    }

    /// <summary>
    /// Approve a loan application
    /// </summary>
    public async Task<(bool success, string message)> ApproveLoanApplicationAsync(
        int approvalId,
        decimal approvedAmount,
        string? adminNotes = null)
    {
        try
        {
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
            if (approval == null)
                return (false, "Application not found");

            approval.Status = "Approved";
            approval.Amount = approvedAmount;
            approval.ProcessedAt = DateTime.Now;
            approval.ProcessedBy = _authService.CurrentUserId?.ToString();
            approval.ApprovalNotes = adminNotes ?? approval.ApprovalNotes;

            _context.ApprovalQueues.Update(approval);
            await _context.SaveChangesAsync();

            return (true, "Loan application approved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error approving loan application: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reject a loan application
    /// </summary>
    public async Task<(bool success, string message)> RejectLoanApplicationAsync(
        int approvalId,
        string rejectionReason)
    {
        try
        {
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
            if (approval == null)
                return (false, "Application not found");

            approval.Status = "Rejected";
            approval.ProcessedAt = DateTime.Now;
            approval.ProcessedBy = _authService.CurrentUserId?.ToString();
            approval.ApprovalNotes = $"Rejection Reason: {rejectionReason}";

            _context.ApprovalQueues.Update(approval);
            await _context.SaveChangesAsync();

            return (true, "Loan application rejected");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rejecting loan application: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== CUSTOMER CARD OPERATIONS ====================

    /// <summary>
    /// Submit a new card application from customer
    /// </summary>
    public async Task<(bool success, string message, int cardId)> SubmitCardApplicationAsync(
        int customerId,
        string cardType,
        string deliveryMethod)
    {
        try
        {
            // Get customer user info
            var customer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == customerId);
            if (customer == null)
                return (false, "Customer not found", 0);

            // Get customer's primary account
            var account = await _context.CustomerAccounts
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsActive);
            if (account == null)
                return (false, "Customer account not found", 0);

            // Generate card number
            var cardNumber = GenerateCardNumber();
            var encryptedCardNumber = EncryptCardNumber(cardNumber);

            // Create card record with IsActive = false (pending approval)
            // NOTE: ExpiryDate and DueDate cannot be NULL in the database,
            // so we set sensible defaults here.
            var card = new CustomerCardEntity
            {
                AccountId = account.AccountId,
                CardType = cardType,
                CardNumber = encryptedCardNumber,
                CardName = $"{cardType} Card - {customer.FullName}",
                CreditLimit = cardType == "Credit" ? 50000 : 0,
                AvailableCredit = cardType == "Credit" ? 50000 : 0,
                OutstandingBalance = 0,
                // Set default expiry to 5 years from now (both Debit and Credit)
                ExpiryDate = DateTime.Now.AddYears(5),
                // Set first due date to next month; admin can adjust later if needed
                DueDate = DateTime.Now.AddMonths(1),
                InterestRate = cardType == "Credit" ? 18.5m : 0,
                IsActive = false, // Pending approval
                IsLocked = false,
                CreatedAt = DateTime.Now
            };

            // Save card first to get CardId
            _context.CustomerCards.Add(card);
            await _context.SaveChangesAsync();

            // Create approval queue entry with the CardId
            var approval = new ApprovalQueueEntity
            {
                RequestType = "CARD",
                RequestId = card.CardId, // Links to CardId
                RequestedBy = customerId.ToString(),
                Amount = card.CreditLimit,
                Status = "Pending",
                Priority = "Normal",
                Description = $"{cardType} Card Application - {deliveryMethod} Delivery for {customer.FullName}",
                RequestedAt = DateTime.Now,
                ApprovalNotes = ""
            };

            _context.ApprovalQueues.Add(approval);
            await _context.SaveChangesAsync();

            return (true, "Card application submitted successfully", card.CardId);
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException?.Message ?? ex.Message;
            System.Diagnostics.Debug.WriteLine($"Error submitting card application: {errorMsg}");
            System.Diagnostics.Debug.WriteLine($"Full exception: {ex}");
            return (false, $"Error: {errorMsg}", 0);
        }
    }

    /// <summary>
    /// Get pending cards for a customer
    /// </summary>
    public async Task<List<CustomerCardDto>> GetPendingCardsForCustomerAsync(int customerId)
    {
        try
        {
            // Get customer's accounts
            var accountIds = await _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId)
                .Select(a => a.AccountId)
                .ToListAsync();

            // Get pending cards (IsActive = false means not yet approved)
            var cards = await _context.CustomerCards
                .Where(c => accountIds.Contains(c.AccountId) && !c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CustomerCardDto
                {
                    CardId = c.CardId,
                    AccountId = c.AccountId,
                    CardType = c.CardType,
                    CardNumber = c.CardNumber,
                    CardName = c.CardName,
                    CreditLimit = c.CreditLimit,
                    AvailableCredit = c.AvailableCredit,
                    OutstandingBalance = c.OutstandingBalance,
                    ExpiryDate = c.ExpiryDate,
                    DueDate = c.DueDate,
                    InterestRate = c.InterestRate,
                    IsActive = c.IsActive,
                    IsLocked = c.IsLocked,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return cards;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching pending cards: {ex.Message}");
            return new List<CustomerCardDto>();
        }
    }

    /// <summary>
    /// Get approved/active cards for a customer
    /// </summary>
    public async Task<List<CustomerCardDto>> GetApprovedCardsForCustomerAsync(int customerId)
    {
        try
        {
            // Get customer's accounts
            var accountIds = await _context.CustomerAccounts
                .Where(a => a.CustomerId == customerId)
                .Select(a => a.AccountId)
                .ToListAsync();

            // Get approved/active cards (IsActive = true means approved)
            var cards = await _context.CustomerCards
                .Where(c => accountIds.Contains(c.AccountId) && c.IsActive && !c.IsLocked)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CustomerCardDto
                {
                    CardId = c.CardId,
                    AccountId = c.AccountId,
                    CardType = c.CardType,
                    CardNumber = c.CardNumber,
                    CardName = c.CardName,
                    CreditLimit = c.CreditLimit,
                    AvailableCredit = c.AvailableCredit,
                    OutstandingBalance = c.OutstandingBalance,
                    ExpiryDate = c.ExpiryDate,
                    DueDate = c.DueDate,
                    InterestRate = c.InterestRate,
                    IsActive = c.IsActive,
                    IsLocked = c.IsLocked,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return cards;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching approved cards: {ex.Message}");
            return new List<CustomerCardDto>();
        }
    }

    /// <summary>
    /// Approve a card application (Admin operation)
    /// </summary>
    public async Task<(bool success, string message)> ApproveCardApplicationForCustomerAsync(
        int cardId,
        string? adminNotes = null)
    {
        try
        {
            var card = await _context.CustomerCards.FirstOrDefaultAsync(c => c.CardId == cardId);
            if (card == null)
                return (false, "Card not found");

            card.IsActive = true;
            card.ExpiryDate = DateTime.Now.AddYears(5); // 5-year expiry
            card.DueDate = DateTime.Now.AddMonths(1); // Due date for first statement

            _context.CustomerCards.Update(card);

            // Update approval queue entry (find by RequestId = CardId and RequestType = CARD)
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(
                a => a.RequestType == "CARD" && a.RequestId == cardId && a.Status == "Pending");
            if (approval != null)
            {
                approval.Status = "Approved";
                approval.ProcessedAt = DateTime.Now;
                approval.ProcessedBy = _authService.CurrentUserId?.ToString();
                approval.ApprovalNotes = adminNotes ?? approval.ApprovalNotes;
                _context.ApprovalQueues.Update(approval);
            }

            await _context.SaveChangesAsync();
            return (true, "Card application approved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error approving card: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reject a card application (Admin operation)
    /// </summary>
    public async Task<(bool success, string message)> RejectCardApplicationForCustomerAsync(
        int cardId,
        string rejectionReason)
    {
        try
        {
            var card = await _context.CustomerCards.FirstOrDefaultAsync(c => c.CardId == cardId);
            if (card == null)
                return (false, "Card not found");

            // Update approval queue entry (find by RequestId = CardId and RequestType = CARD)
            var approval = await _context.ApprovalQueues.FirstOrDefaultAsync(
                a => a.RequestType == "CARD" && a.RequestId == cardId && a.Status == "Pending");
            if (approval != null)
            {
                approval.Status = "Rejected";
                approval.ProcessedAt = DateTime.Now;
                approval.ProcessedBy = _authService.CurrentUserId?.ToString();
                approval.ApprovalNotes = $"Rejection Reason: {rejectionReason}";
                _context.ApprovalQueues.Update(approval);
            }

            await _context.SaveChangesAsync();
            return (true, "Card application rejected");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rejecting card: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== HELPER METHODS ====================

    private string ExtractCardType(string description)
    {
        if (description.Contains("Credit"))
            return "Credit";
        if (description.Contains("Debit"))
            return "Debit";
        return "Unknown";
    }

    private string ExtractDeliveryMethod(string description)
    {
        if (description.Contains("Physical"))
            return "Physical";
        if (description.Contains("Digital"))
            return "Digital";
        return "Unknown";
    }

    private string ExtractLoanType(string description)
    {
        var parts = description.Split(" Loan");
        return parts.Length > 0 ? parts[0] : "Unknown";
    }

    private int ExtractLoanTerm(string description)
    {
        var match = System.Text.RegularExpressions.Regex.Match(description, @"(\d+)\s*months");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private string ExtractPurpose(string description)
    {
        var match = System.Text.RegularExpressions.Regex.Match(description, @"Purpose:\s*(.+?)$");
        return match.Success ? match.Groups[1].Value : "Not specified";
    }

    // ==================== ENCRYPTION & CARD GENERATION ====================

    private string EncryptCardNumber(string cardNumber)
    {
        // Store only last 4 digits encrypted for security
        // Full card number is only shown once at creation
        try
        {
            if (cardNumber.Length < 4)
                return cardNumber;
            
            var last4 = cardNumber.Substring(cardNumber.Length - 4);
            var bytes = System.Text.Encoding.UTF8.GetBytes(last4);
            return Convert.ToBase64String(bytes);
        }
        catch
        {
            return cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
        }
    }

    private string DecryptCardNumber(string encryptedCardNumber)
    {
        try
        {
            // Decrypt the last 4 digits
            var bytes = Convert.FromBase64String(encryptedCardNumber);
            var last4 = System.Text.Encoding.UTF8.GetString(bytes);
            return $"**** **** **** {last4}";
        }
        catch
        {
            // If decryption fails, just show masked format
            return "**** **** **** ****";
        }
    }

    private string GenerateCardNumber()
    {
        // Generate a random 16-digit card number
        var random = new Random();
        return string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10)));
    }
}

// ==================== DTOs ====================

public class CardApplicationDto
{
    public int ApprovalId { get; set; }
    public string RequestedBy { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CardType { get; set; } = ""; // Debit or Credit
    public string DeliveryMethod { get; set; } = ""; // Physical or Digital
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = ""; // Pending, Approved, Rejected
    public string Priority { get; set; } = "Normal";
    public string Notes { get; set; } = "";
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
}

public class LoanApplicationDto
{
    public int ApprovalId { get; set; }
    public string RequestedBy { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string LoanType { get; set; } = ""; // Personal, Home, Auto, Education
    public decimal RequestedAmount { get; set; }
    public int LoanTerm { get; set; } // In months
    public string Purpose { get; set; } = "";
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = ""; // Pending, Approved, Rejected
    public string Priority { get; set; } = "Normal";
    public string Notes { get; set; } = "";
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
}

// ==================== CUSTOMER CARD DTOs ====================

public class CustomerCardDto
{
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string CardType { get; set; } = "";
    public string CardNumber { get; set; } = ""; // Encrypted
    public string CardName { get; set; } = "";
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal InterestRate { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
