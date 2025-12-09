using FinanceBank.Data;
using FinanceBank.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services;

/// <summary>
/// Core service for Savings Account operations
/// Handles account opening, deposits, withdrawals, and account management
/// Integrates with TransactionHistoryService for AR/AP tracking.
/// Integrates with AutomaticGLPostingService for General Ledger posting.
/// </summary>
public class SavingsAccountService
{
    private readonly IDbContextFactory<BFASDbContext> _contextFactory;
    private readonly TransactionHistoryService? _transactionHistoryService;
    private readonly AutomaticGLPostingService? _glPostingService;

    public SavingsAccountService(
        IDbContextFactory<BFASDbContext> contextFactory,
        TransactionHistoryService? transactionHistoryService = null,
        AutomaticGLPostingService? glPostingService = null)
    {
        _contextFactory = contextFactory;
        _transactionHistoryService = transactionHistoryService;
        _glPostingService = glPostingService;
    }

    // =============================================
    // ACCOUNT MANAGEMENT
    // =============================================

    /// <summary>
    /// Open a new savings account (Teller action)
    /// </summary>
    public async Task<(bool success, string message, SavingsAccount? account)> OpenSavingsAccountAsync(
        int customerId,
        int customerAccountId,
        int accountTypeId,
        decimal initialDeposit,
        int? customLockInDays,
        string openedBy)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            // Validate customer exists
            var customer = await context.Users.FindAsync(customerId);
            if (customer == null)
                return (false, "Customer not found", null);

            // Validate linked checking account
            var checkingAccount = await context.CustomerAccounts.FindAsync(customerAccountId);
            if (checkingAccount == null || checkingAccount.CustomerId != customerId)
                return (false, "Invalid checking account", null);

            // Get account type
            var accountType = await context.SavingsAccountTypes.FindAsync(accountTypeId);
            if (accountType == null || !accountType.IsActive)
                return (false, "Invalid account type", null);

            // Validate minimum deposit
            if (initialDeposit < accountType.MinimumInitialDeposit)
                return (false, $"Minimum initial deposit is ₱{accountType.MinimumInitialDeposit:N2}", null);

            // Generate unique account number
            var accountNumber = await GenerateUniqueSavingsAccountNumberAsync(context);

            // Determine lock-in period
            var lockInDays = customLockInDays ?? accountType.DefaultLockInDays;
            var maturityDate = lockInDays > 0 ? DateTime.Now.AddDays(lockInDays) : (DateTime?)null;

            // Create savings account
            var savingsAccount = new SavingsAccount
            {
                AccountNumber = accountNumber,
                CustomerId = customerId,
                CustomerAccountId = customerAccountId,
                AccountTypeId = accountTypeId,
                Balance = initialDeposit,
                TotalDeposits = initialDeposit,
                CurrentInterestRate = accountType.InterestRateMin,
                LockInPeriodDays = lockInDays,
                MaturityDate = maturityDate,
                Status = "ACTIVE",
                OpenedDate = DateTime.Now,
                OpenedBy = openedBy,
                CreatedAt = DateTime.Now
            };

            context.SavingsAccounts.Add(savingsAccount);
            await context.SaveChangesAsync();


            var transaction = new SavingsTransaction
            {
                TransactionNumber = SavingsTransaction.GenerateTransactionNumber(),
                SavingsAccountId = savingsAccount.SavingsAccountId,
                TransactionType = "Deposit",
                Amount = initialDeposit,
                BalanceBefore = 0,
                BalanceAfter = initialDeposit,
                Source = "Cash",
                Status = "Completed",
                Description = "Initial deposit - Account opening",
                ProcessedBy = openedBy,
                TransactionDate = DateTime.Now,
                ProcessedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            context.SavingsTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return (true, $"Savings account {accountNumber} opened successfully", savingsAccount);
        }
        catch (Exception ex)
        {
            return (false, $"Error opening account: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Get all savings accounts for a customer
    /// </summary>
    public async Task<List<SavingsAccount>> GetSavingsAccountsByCustomerAsync(int customerId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsAccounts
            .Include(sa => sa.AccountType)
            .Include(sa => sa.LinkedAccount)
            .Include(sa => sa.Customer)
            .Where(sa => sa.CustomerId == customerId)
            .OrderByDescending(sa => sa.OpenedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get savings account by account number
    /// </summary>
    public async Task<SavingsAccount?> GetSavingsAccountByNumberAsync(string accountNumber)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsAccounts
            .Include(sa => sa.AccountType)
            .Include(sa => sa.LinkedAccount)
            .Include(sa => sa.Customer)
            .FirstOrDefaultAsync(sa => sa.AccountNumber == accountNumber);
    }

    /// <summary>
    /// Get all savings accounts
    /// </summary>
    public async Task<List<SavingsAccount>> GetAllSavingsAccountsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsAccounts
            .Include(sa => sa.AccountType)
            .Include(sa => sa.LinkedAccount)
            .Include(sa => sa.Customer)
            .OrderByDescending(sa => sa.OpenedDate)
            .ToListAsync();
    }

    // =============================================
    // DEPOSIT OPERATIONS
    // =============================================

    /// <summary>
    /// Deposit to savings account (Cash or from checking account)
    /// </summary>
    public async Task<(bool success, string message, SavingsTransaction? transaction)> DepositToSavingsAsync(
        int savingsAccountId,
        decimal amount,
        string source, // "Cash" or "FromAccount"
        int? relatedCustomerAccountId,
        string processedBy,
        string? description = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var savingsAccount = await context.SavingsAccounts.FindAsync(savingsAccountId);
            if (savingsAccount == null)
                return (false, "Savings account not found", null);

            if (savingsAccount.Status != "ACTIVE")
                return (false, "Savings account is not active", null);

            if (amount <= 0)
                return (false, "Deposit amount must be greater than zero", null);

            // If depositing from checking account, validate and deduct
            if (source == "FromAccount" && relatedCustomerAccountId.HasValue)
            {
                var checkingAccount = await context.CustomerAccounts.FindAsync(relatedCustomerAccountId.Value);
                if (checkingAccount == null)
                    return (false, "Checking account not found", null);

                if (checkingAccount.AvailableBalance < amount)
                    return (false, "Insufficient balance in checking account", null);

                // Deduct from checking account
                checkingAccount.Balance -= amount;
                checkingAccount.AvailableBalance -= amount;
                checkingAccount.LastTransactionAt = DateTime.Now;
            }

            // Add to savings account
            var balanceBefore = savingsAccount.Balance;
            savingsAccount.Balance += amount;
            savingsAccount.TotalDeposits += amount;
            savingsAccount.UpdatedAt = DateTime.Now;

            // Create transaction record
            var transaction = new SavingsTransaction
            {
                TransactionNumber = SavingsTransaction.GenerateTransactionNumber(),
                SavingsAccountId = savingsAccountId,
                TransactionType = "Deposit",
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = savingsAccount.Balance,
                Source = source,
                RelatedCustomerAccountId = relatedCustomerAccountId,
                Status = "Completed",
                Description = description ?? $"Deposit {source}",
                ProcessedBy = processedBy,
                TransactionDate = DateTime.Now,
                ProcessedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            context.SavingsTransactions.Add(transaction);
            await context.SaveChangesAsync();

            // Post to General Ledger
            try
            {
                if (_glPostingService != null)
                {
                    var customer = await context.Users.FindAsync(savingsAccount.CustomerId);
                    var customerName = customer?.FullName ?? "Customer";

                    await _glPostingService.PostSavingsDepositAsync(
                        transaction.TransactionId,
                        transaction.TransactionNumber,
                        amount,
                        customerName,
                        savingsAccount.AccountNumber ?? "",
                        DateTime.Now);
                }
            }
            catch
            {
                // GL posting failure should not prevent deposit from being recorded
            }

            // Create AR entry (savings deposit = money IN = AR)
            try
            {
                if (_transactionHistoryService != null)
                {
                    var customer = await context.Users.FindAsync(savingsAccount.CustomerId);
                    var customerName = customer?.FullName ?? "Customer";

                    await _transactionHistoryService.RecordSavingsDepositAsync(
                        savingsAccountId: savingsAccountId,
                        customerAccountId: savingsAccount.CustomerAccountId,
                        accountNumber: savingsAccount.AccountNumber ?? "",
                        customerName: customerName,
                        amount: amount,
                        transactionId: transaction.TransactionId,
                        processedBy: processedBy,
                        notes: description
                    );
                }
            }
            catch
            {
                // AR tracking failure should not prevent deposit from being recorded
            }

            return (true, $"Successfully deposited ₱{amount:N2}", transaction);
        }
        catch (Exception ex)
        {
            return (false, $"Error processing deposit: {ex.Message}", null);
        }
    }

    // =============================================
    // WITHDRAWAL OPERATIONS
    // =============================================

    /// <summary>
    /// Create withdrawal request (Customer or Teller initiates)
    /// </summary>
    public async Task<(bool success, string message, SavingsWithdrawalRequest? request)> CreateWithdrawalRequestAsync(
        int savingsAccountId,
        int customerId,
        decimal requestedAmount,
        string? customerReason = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var savingsAccount = await context.SavingsAccounts
                .Include(sa => sa.AccountType)
                .FirstOrDefaultAsync(sa => sa.SavingsAccountId == savingsAccountId);

            if (savingsAccount == null)
                return (false, "Savings account not found", null);

            if (savingsAccount.CustomerId != customerId)
                return (false, "Unauthorized: Account does not belong to customer", null);

            if (savingsAccount.Status != "ACTIVE")
                return (false, "Savings account is not active", null);

            if (requestedAmount <= 0)
                return (false, "Withdrawal amount must be greater than zero", null);

            if (requestedAmount > savingsAccount.Balance)
                return (false, "Insufficient balance", null);

            // Check if early withdrawal (before maturity)
            var isEarlyWithdrawal = savingsAccount.MaturityDate.HasValue && DateTime.Now < savingsAccount.MaturityDate.Value;
            var penaltyAmount = 0m;
            var interestForfeited = 0m;

            if (isEarlyWithdrawal && savingsAccount.AccountType != null)
            {
                // Calculate penalty
                penaltyAmount = requestedAmount * savingsAccount.AccountType.EarlyWithdrawalPenaltyRate;

                // Forfeit all interest earned
                interestForfeited = savingsAccount.TotalInterestEarned;
            }

            var netAmount = requestedAmount - penaltyAmount - interestForfeited;

            // Create withdrawal request
            var request = new SavingsWithdrawalRequest
            {
                SavingsAccountId = savingsAccountId,
                CustomerId = customerId,
                RequestedAmount = requestedAmount,
                WithdrawalType = isEarlyWithdrawal ? "Early" : "Normal",
                IsEarlyWithdrawal = isEarlyWithdrawal,
                PenaltyAmount = penaltyAmount,
                InterestForfeited = interestForfeited,
                NetWithdrawalAmount = netAmount,
                Status = requestedAmount > 200000 ? "Pending" : "TellerApproved", // Auto-approve if under ₱200k
                CustomerReason = customerReason,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            context.SavingsWithdrawalRequests.Add(request);
            await context.SaveChangesAsync();

            var message = requestedAmount > 200000
                ? "Withdrawal request submitted. Awaiting Finance Manager approval."
                : "Withdrawal request approved. Please proceed to teller.";

            if (isEarlyWithdrawal)
                message += $" Warning: Early withdrawal penalty ₱{penaltyAmount:N2} + interest forfeited ₱{interestForfeited:N2}";

            return (true, message, request);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating withdrawal request: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Finance Manager approves/rejects withdrawal request
    /// </summary>
    public async Task<(bool success, string message)> ApproveWithdrawalRequestAsync(
        int requestId,
        bool approve,
        string approvedBy,
        string? rejectionReason = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var request = await context.SavingsWithdrawalRequests.FindAsync(requestId);
            if (request == null)
                return (false, "Withdrawal request not found");

            if (request.Status != "Pending" && request.Status != "TellerApproved")
                return (false, "Request already processed");

            if (approve)
            {
                request.Status = "FMApproved";
                request.ApprovedByFM = approvedBy;
                request.ApprovedByFMAt = DateTime.Now;
            }
            else
            {
                request.Status = "Rejected";
                request.RejectionReason = rejectionReason ?? "Rejected by Finance Manager";
            }

            await context.SaveChangesAsync();

            return (true, approve ? "Withdrawal request approved" : "Withdrawal request rejected");
        }
        catch (Exception ex)
        {
            return (false, $"Error processing approval: {ex.Message}");
        }
    }

    /// <summary>
    /// Process approved withdrawal (Teller completes)
    /// </summary>
    public async Task<(bool success, string message, SavingsTransaction? transaction)> ProcessWithdrawalAsync(
        int requestId,
        string processedBy)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var request = await context.SavingsWithdrawalRequests
                .Include(r => r.SavingsAccount)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return (false, "Withdrawal request not found", null);

            if (request.Status != "FMApproved" && request.Status != "TellerApproved")
                return (false, "Request not approved for processing", null);

            var savingsAccount = request.SavingsAccount;
            if (savingsAccount == null)
                return (false, "Savings account not found", null);

            if (savingsAccount.Balance < request.RequestedAmount)
                return (false, "Insufficient balance", null);

            // Deduct from savings account
            var balanceBefore = savingsAccount.Balance;
            savingsAccount.Balance -= request.RequestedAmount;
            savingsAccount.TotalWithdrawals += request.RequestedAmount;

            // Forfeit interest if early withdrawal
            if (request.IsEarlyWithdrawal && request.InterestForfeited > 0)
            {
                savingsAccount.TotalInterestEarned -= request.InterestForfeited;
            }

            savingsAccount.UpdatedAt = DateTime.Now;

            // Create withdrawal transaction
            var transaction = new SavingsTransaction
            {
                TransactionNumber = SavingsTransaction.GenerateTransactionNumber(),
                SavingsAccountId = savingsAccount.SavingsAccountId,
                TransactionType = "Withdrawal",
                Amount = request.RequestedAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = savingsAccount.Balance,
                Source = "Cash",
                Status = "Completed",
                Description = $"Withdrawal - Request #{request.RequestId}. Penalty: ₱{request.PenaltyAmount:N2}",
                ProcessedBy = processedBy,
                TransactionDate = DateTime.Now,
                ProcessedAt = DateTime.Now,
                ApprovedBy = request.ApprovedByFM,
                ApprovedAt = request.ApprovedByFMAt,
                CreatedAt = DateTime.Now
            };

            context.SavingsTransactions.Add(transaction);

            // Update request status
            request.Status = "Completed";
            request.CompletedDate = DateTime.Now;
            request.ProcessedByTeller = processedBy;
            request.ProcessedByTellerAt = DateTime.Now;

            await context.SaveChangesAsync();

            // Post to General Ledger
            try
            {
                if (_glPostingService != null)
                {
                    var customer = await context.Users.FindAsync(savingsAccount.CustomerId);
                    var customerName = customer?.FullName ?? "Customer";

                    await _glPostingService.PostSavingsWithdrawalAsync(
                        transaction.TransactionId,
                        transaction.TransactionNumber,
                        request.NetWithdrawalAmount, // Net amount after penalty
                        request.PenaltyAmount,
                        customerName,
                        savingsAccount.AccountNumber ?? "",
                        DateTime.Now);
                }
            }
            catch
            {
                // GL posting failure should not prevent withdrawal from being recorded
            }

            // Create AP entry (savings withdrawal = money OUT = AP)
            try
            {
                if (_transactionHistoryService != null)
                {
                    var customer = await context.Users.FindAsync(savingsAccount.CustomerId);
                    var customerName = customer?.FullName ?? "Customer";

                    await _transactionHistoryService.RecordSavingsWithdrawalAsync(
                        savingsAccountId: savingsAccount.SavingsAccountId,
                        customerAccountId: savingsAccount.CustomerAccountId,
                        accountNumber: savingsAccount.AccountNumber ?? "",
                        customerName: customerName,
                        amount: request.RequestedAmount,
                        transactionId: transaction.TransactionId,
                        processedBy: processedBy,
                        notes: $"Withdrawal Request #{request.RequestId}. Penalty: ₱{request.PenaltyAmount:N2}"
                    );
                }
            }
            catch
            {
                // AP tracking failure should not prevent withdrawal from being recorded
            }

            return (true, $"Withdrawal processed successfully. Net amount: ₱{request.NetWithdrawalAmount:N2}", transaction);
        }
        catch (Exception ex)
        {
            return (false, $"Error processing withdrawal: {ex.Message}", null);
        }
    }

    // =============================================
    // TRANSACTION HISTORY
    // =============================================

    /// <summary>
    /// Get transaction history for a savings account
    /// </summary>
    public async Task<List<SavingsTransaction>> GetTransactionHistoryAsync(int savingsAccountId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsTransactions
            .Where(st => st.SavingsAccountId == savingsAccountId)
            .OrderByDescending(st => st.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending withdrawal requests
    /// </summary>
    public async Task<List<SavingsWithdrawalRequest>> GetPendingWithdrawalRequestsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsWithdrawalRequests
            .Include(r => r.SavingsAccount)
            .ThenInclude(sa => sa!.Customer)
            .Include(r => r.SavingsAccount)
            .ThenInclude(sa => sa!.AccountType)
            .Where(r => r.Status == "Pending" || r.Status == "TellerApproved")
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get withdrawal requests for a customer
    /// </summary>
    public async Task<List<SavingsWithdrawalRequest>> GetWithdrawalRequestsByCustomerAsync(int customerId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsWithdrawalRequests
            .Include(r => r.SavingsAccount)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();
    }

    // =============================================
    // ACCOUNT TYPES
    // =============================================

    /// <summary>
    /// Get all savings account types
    /// </summary>
    public async Task<List<SavingsAccountType>> GetAccountTypesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SavingsAccountTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.TypeId)
            .ToListAsync();
    }

    // =============================================
    // HELPER METHODS
    // =============================================

    /// <summary>
    /// Generate unique savings account number
    /// </summary>
    private async Task<string> GenerateUniqueSavingsAccountNumberAsync(BFASDbContext context)
    {
        string accountNumber;
        bool exists;

        do
        {
            accountNumber = SavingsAccount.GenerateAccountNumber();
            exists = await context.SavingsAccounts.AnyAsync(sa => sa.AccountNumber == accountNumber);
        } while (exists);

        return accountNumber;
    }

    /// <summary>
    /// Get savings summary statistics
    /// </summary>
    public async Task<(decimal TotalSavings, int ActiveAccounts, decimal TotalInterestEarned)> GetSavingsSummaryAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var accounts = await context.SavingsAccounts
            .Where(sa => sa.Status == "ACTIVE")
            .ToListAsync();

        return (
            accounts.Sum(sa => sa.Balance),
            accounts.Count,
            accounts.Sum(sa => sa.TotalInterestEarned)
        );
    }
}
