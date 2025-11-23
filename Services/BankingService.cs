using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for Banking module CRUD operations
    /// </summary>
    public class BankingService
    {
        private readonly BFASDbContext _context;

        public BankingService(BFASDbContext context)
        {
            _context = context;
        }

        // Bank Accounts
        public async Task<List<BankAccount>> GetAllBankAccountsAsync()
        {
            try
            {
                return await _context.BankAccounts.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving bank accounts: {ex.Message}", ex);
            }
        }

        public async Task<BankAccount?> GetBankAccountByIdAsync(int id)
        {
            try
            {
                return await _context.BankAccounts.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving bank account: {ex.Message}", ex);
            }
        }

        public async Task<BankAccount> CreateBankAccountAsync(BankAccount account)
        {
            try
            {
                _context.BankAccounts.Add(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating bank account: {ex.Message}", ex);
            }
        }

        public async Task<BankAccount> UpdateBankAccountAsync(BankAccount account)
        {
            try
            {
                _context.BankAccounts.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating bank account: {ex.Message}", ex);
            }
        }

        public async Task DeleteBankAccountAsync(int id)
        {
            try
            {
                var account = await GetBankAccountByIdAsync(id);
                if (account != null)
                {
                    _context.BankAccounts.Remove(account);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting bank account: {ex.Message}", ex);
            }
        }

        // Fund Transfers
        public async Task<List<FundTransfer>> GetAllTransfersAsync()
        {
            try
            {
                return await _context.FundTransfers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving transfers: {ex.Message}", ex);
            }
        }

        public async Task<FundTransfer?> GetTransferByIdAsync(int id)
        {
            try
            {
                return await _context.FundTransfers.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving transfer: {ex.Message}", ex);
            }
        }

        public async Task<FundTransfer> CreateTransferAsync(FundTransfer transfer)
        {
            try
            {
                _context.FundTransfers.Add(transfer);
                await _context.SaveChangesAsync();
                return transfer;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating transfer: {ex.Message}", ex);
            }
        }

        public async Task<FundTransfer> UpdateTransferAsync(FundTransfer transfer)
        {
            try
            {
                _context.FundTransfers.Update(transfer);
                await _context.SaveChangesAsync();
                return transfer;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating transfer: {ex.Message}", ex);
            }
        }

        // Billers
        public async Task<List<Biller>> GetAllBillersAsync()
        {
            try
            {
                return await _context.Billers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving billers: {ex.Message}", ex);
            }
        }

        public async Task<Biller?> GetBillerByIdAsync(int id)
        {
            try
            {
                return await _context.Billers.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving biller: {ex.Message}", ex);
            }
        }

        public async Task<Biller> CreateBillerAsync(Biller biller)
        {
            try
            {
                _context.Billers.Add(biller);
                await _context.SaveChangesAsync();
                return biller;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating biller: {ex.Message}", ex);
            }
        }

        public async Task<Biller> UpdateBillerAsync(Biller biller)
        {
            try
            {
                _context.Billers.Update(biller);
                await _context.SaveChangesAsync();
                return biller;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating biller: {ex.Message}", ex);
            }
        }

        // Loans
        public async Task<List<LoanManagementEntity>> GetAllLoansAsync()
        {
            try
            {
                return await _context.LoanManagementRecords.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving loans: {ex.Message}", ex);
            }
        }

        public async Task<LoanManagementEntity?> GetLoanByIdAsync(int id)
        {
            try
            {
                return await _context.LoanManagementRecords.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving loan: {ex.Message}", ex);
            }
        }

        public async Task<LoanManagementEntity> CreateLoanAsync(LoanManagementEntity loan)
        {
            try
            {
                _context.LoanManagementRecords.Add(loan);
                await _context.SaveChangesAsync();
                return loan;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating loan: {ex.Message}", ex);
            }
        }

        // Cards
        public async Task<List<CardManagementEntity>> GetAllCardsAsync()
        {
            try
            {
                return await _context.CardManagementRecords.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving cards: {ex.Message}", ex);
            }
        }

        public async Task<CardManagementEntity?> GetCardByIdAsync(int id)
        {
            try
            {
                return await _context.CardManagementRecords.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving card: {ex.Message}", ex);
            }
        }

        public async Task<CardManagementEntity> CreateCardAsync(CardManagementEntity card)
        {
            try
            {
                _context.CardManagementRecords.Add(card);
                await _context.SaveChangesAsync();
                return card;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating card: {ex.Message}", ex);
            }
        }
    }
}
