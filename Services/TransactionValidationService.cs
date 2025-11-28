using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services;

/// <summary>
/// Comprehensive transaction validation service based on BPI banking rules
/// </summary>
public class TransactionValidationService
{
    private readonly BFASDbContext _context;

    // BPI-based transaction limits
    private const decimal MinimumTransactionAmount = 100m;
    private const decimal MaximumWithdrawalAmount = 100000m; // ₱100,000 per transaction
    private const decimal MaximumTransferAmount = 1000000m; // ₱1,000,000 per transaction
    private const decimal MaximumDepositAmount = 500000m; // ₱500,000 per transaction
    
    // Daily limits
    private const decimal DailyWithdrawalLimit = 200000m; // ₱200,000 per day
    private const decimal DailyTransferLimit = 2000000m; // ₱2,000,000 per day
    private const decimal DailyTransactionCountLimit = 20; // Maximum 20 transactions per day

    public TransactionValidationService(BFASDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validate a deposit transaction
    /// </summary>
    public async Task<ValidationResult> ValidateDeposit(int accountId, decimal amount, string accountNumber)
    {
        var result = new ValidationResult();

        // Validate amount
        if (amount <= 0)
        {
            result.IsValid = false;
            result.ErrorMessage = "❌ Deposit amount must be greater than zero.";
            return result;
        }

        if (amount < MinimumTransactionAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Minimum deposit amount is ₱{MinimumTransactionAmount:N2}.";
            return result;
        }

        if (amount > MaximumDepositAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Maximum deposit amount per transaction is ₱{MaximumDepositAmount:N2}. For larger amounts, please contact the branch manager.";
            return result;
        }

        // Validate account exists and is active
        var account = await _context.CustomerAccounts.FindAsync(accountId);
        if (account == null)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Account number {accountNumber} not found in the system.";
            return result;
        }

        if (account.Status != "ACTIVE")
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Account {accountNumber} is {account.Status}. Only ACTIVE accounts can receive deposits.";
            return result;
        }

        // Check daily transaction limit
        var dailyCount = await GetDailyTransactionCount(accountId);
        if (dailyCount >= DailyTransactionCountLimit)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Daily transaction limit reached ({DailyTransactionCountLimit} transactions per day). Please try again tomorrow.";
            return result;
        }

        result.IsValid = true;
        result.SuccessMessage = $"✓ Deposit validation passed. Amount: ₱{amount:N2}";
        return result;
    }

    /// <summary>
    /// Validate a withdrawal transaction
    /// </summary>
    public async Task<ValidationResult> ValidateWithdrawal(int accountId, decimal amount, string accountNumber, string password)
    {
        var result = new ValidationResult();

        // Validate amount
        if (amount <= 0)
        {
            result.IsValid = false;
            result.ErrorMessage = "❌ Withdrawal amount must be greater than zero.";
            return result;
        }

        if (amount < MinimumTransactionAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Minimum withdrawal amount is ₱{MinimumTransactionAmount:N2}.";
            return result;
        }

        if (amount > MaximumWithdrawalAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Maximum withdrawal amount per transaction is ₱{MaximumWithdrawalAmount:N2}. For larger amounts, please contact the branch manager.";
            return result;
        }

        // Validate account exists and is active
        var account = await _context.CustomerAccounts.FindAsync(accountId);
        if (account == null)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Account number {accountNumber} not found in the system.";
            return result;
        }

        if (account.Status != "ACTIVE")
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Account {accountNumber} is {account.Status}. Only ACTIVE accounts can withdraw funds.";
            return result;
        }

        // Check sufficient balance
        if (account.Balance < amount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Insufficient funds. Available balance: ₱{account.Balance:N2}, Requested: ₱{amount:N2}. You need ₱{(amount - account.Balance):N2} more.";
            return result;
        }

        // Check daily withdrawal limit
        var dailyWithdrawals = await GetDailyWithdrawalTotal(accountId);
        if (dailyWithdrawals + amount > DailyWithdrawalLimit)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Daily withdrawal limit exceeded. Daily limit: ₱{DailyWithdrawalLimit:N2}, Already withdrawn today: ₱{dailyWithdrawals:N2}, Remaining: ₱{(DailyWithdrawalLimit - dailyWithdrawals):N2}.";
            return result;
        }

        // Check daily transaction count
        var dailyCount = await GetDailyTransactionCount(accountId);
        if (dailyCount >= DailyTransactionCountLimit)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Daily transaction limit reached ({DailyTransactionCountLimit} transactions per day). Please try again tomorrow.";
            return result;
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            result.ErrorMessage = "❌ Password is required for withdrawal verification.";
            return result;
        }

        result.IsValid = true;
        result.SuccessMessage = $"✓ Withdrawal validation passed. Amount: ₱{amount:N2}, Available balance: ₱{account.Balance:N2}";
        return result;
    }

    /// <summary>
    /// Validate a transfer transaction
    /// </summary>
    public async Task<ValidationResult> ValidateTransfer(int senderAccountId, int receiverAccountId, decimal amount, string senderAccountNumber, string receiverAccountNumber)
    {
        var result = new ValidationResult();

        // Validate amount
        if (amount <= 0)
        {
            result.IsValid = false;
            result.ErrorMessage = "❌ Transfer amount must be greater than zero.";
            return result;
        }

        if (amount < MinimumTransactionAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Minimum transfer amount is ₱{MinimumTransactionAmount:N2}.";
            return result;
        }

        if (amount > MaximumTransferAmount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Maximum transfer amount per transaction is ₱{MaximumTransferAmount:N2}. For larger amounts, please contact the branch manager.";
            return result;
        }

        // Validate sender account
        var senderAccount = await _context.CustomerAccounts.FindAsync(senderAccountId);
        if (senderAccount == null)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Sender account {senderAccountNumber} not found.";
            return result;
        }

        if (senderAccount.Status != "ACTIVE")
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Sender account {senderAccountNumber} is {senderAccount.Status}. Only ACTIVE accounts can transfer funds.";
            return result;
        }

        // Check sufficient balance
        if (senderAccount.Balance < amount)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Insufficient funds. Available balance: ₱{senderAccount.Balance:N2}, Requested: ₱{amount:N2}. You need ₱{(amount - senderAccount.Balance):N2} more.";
            return result;
        }

        // Validate receiver account
        var receiverAccount = await _context.CustomerAccounts.FindAsync(receiverAccountId);
        if (receiverAccount == null)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Recipient account {receiverAccountNumber} not found in the system.";
            return result;
        }

        if (receiverAccount.Status != "ACTIVE")
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Recipient account {receiverAccountNumber} is {receiverAccount.Status}. Only ACTIVE accounts can receive transfers.";
            return result;
        }

        // Prevent self-transfer
        if (senderAccountId == receiverAccountId)
        {
            result.IsValid = false;
            result.ErrorMessage = "❌ Cannot transfer to the same account. Please select a different recipient.";
            return result;
        }

        // Check daily transfer limit
        var dailyTransfers = await GetDailyTransferTotal(senderAccountId);
        if (dailyTransfers + amount > DailyTransferLimit)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Daily transfer limit exceeded. Daily limit: ₱{DailyTransferLimit:N2}, Already transferred today: ₱{dailyTransfers:N2}, Remaining: ₱{(DailyTransferLimit - dailyTransfers):N2}.";
            return result;
        }

        // Check daily transaction count
        var dailyCount = await GetDailyTransactionCount(senderAccountId);
        if (dailyCount >= DailyTransactionCountLimit)
        {
            result.IsValid = false;
            result.ErrorMessage = $"❌ Daily transaction limit reached ({DailyTransactionCountLimit} transactions per day). Please try again tomorrow.";
            return result;
        }

        result.IsValid = true;
        result.SuccessMessage = $"✓ Transfer validation passed. Amount: ₱{amount:N2}, From: {senderAccountNumber}, To: {receiverAccountNumber}";
        return result;
    }

    private async Task<int> GetDailyTransactionCount(int accountId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.CustomerTransactions
            .Where(t => t.AccountId == accountId && t.CreatedAt >= today && t.CreatedAt < tomorrow)
            .CountAsync();
    }

    private async Task<decimal> GetDailyWithdrawalTotal(int accountId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.CustomerTransactions
            .Where(t => t.AccountId == accountId 
                && t.TransactionType == "Withdrawal" 
                && t.CreatedAt >= today 
                && t.CreatedAt < tomorrow)
            .SumAsync(t => t.Amount);
    }

    private async Task<decimal> GetDailyTransferTotal(int accountId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.CustomerTransactions
            .Where(t => t.AccountId == accountId 
                && t.TransactionType == "Transfer" 
                && t.CreatedAt >= today 
                && t.CreatedAt < tomorrow)
            .SumAsync(t => t.Amount);
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
    public string SuccessMessage { get; set; } = "";
    public Dictionary<string, string> ValidationDetails { get; set; } = new();
}
