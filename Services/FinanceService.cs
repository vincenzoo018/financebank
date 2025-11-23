using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for Finance module CRUD operations
    /// </summary>
    public class FinanceService
    {
        private readonly BFASDbContext _context;

        public FinanceService(BFASDbContext context)
        {
            _context = context;
        }

        // Budgets
        public async Task<List<Budget>> GetAllBudgetsAsync()
        {
            try
            {
                return await _context.Budgets.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving budgets: {ex.Message}", ex);
            }
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id)
        {
            try
            {
                return await _context.Budgets.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving budget: {ex.Message}", ex);
            }
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            try
            {
                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                return budget;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating budget: {ex.Message}", ex);
            }
        }

        public async Task<Budget> UpdateBudgetAsync(Budget budget)
        {
            try
            {
                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
                return budget;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating budget: {ex.Message}", ex);
            }
        }

        public async Task DeleteBudgetAsync(int id)
        {
            try
            {
                var budget = await GetBudgetByIdAsync(id);
                if (budget != null)
                {
                    _context.Budgets.Remove(budget);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting budget: {ex.Message}", ex);
            }
        }

        // Cashflow Entries
        public async Task<List<CashflowEntry>> GetAllCashflowEntriesAsync()
        {
            try
            {
                return await _context.CashflowEntries.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving cashflow entries: {ex.Message}", ex);
            }
        }

        public async Task<CashflowEntry?> GetCashflowEntryByIdAsync(int id)
        {
            try
            {
                return await _context.CashflowEntries.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving cashflow entry: {ex.Message}", ex);
            }
        }

        public async Task<CashflowEntry> CreateCashflowEntryAsync(CashflowEntry entry)
        {
            try
            {
                _context.CashflowEntries.Add(entry);
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating cashflow entry: {ex.Message}", ex);
            }
        }

        public async Task<CashflowEntry> UpdateCashflowEntryAsync(CashflowEntry entry)
        {
            try
            {
                _context.CashflowEntries.Update(entry);
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating cashflow entry: {ex.Message}", ex);
            }
        }

        // Financial Forecasts
        public async Task<List<FinancialForecast>> GetAllFinancialForecastsAsync()
        {
            try
            {
                return await _context.FinancialForecasts.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving financial forecasts: {ex.Message}", ex);
            }
        }

        public async Task<FinancialForecast?> GetFinancialForecastByIdAsync(int id)
        {
            try
            {
                return await _context.FinancialForecasts.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving financial forecast: {ex.Message}", ex);
            }
        }

        public async Task<FinancialForecast> CreateFinancialForecastAsync(FinancialForecast forecast)
        {
            try
            {
                _context.FinancialForecasts.Add(forecast);
                await _context.SaveChangesAsync();
                return forecast;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating financial forecast: {ex.Message}", ex);
            }
        }

        public async Task<FinancialForecast> UpdateFinancialForecastAsync(FinancialForecast forecast)
        {
            try
            {
                _context.FinancialForecasts.Update(forecast);
                await _context.SaveChangesAsync();
                return forecast;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating financial forecast: {ex.Message}", ex);
            }
        }

        // Summary statistics
        public async Task<Dictionary<string, decimal>> GetFinancialSummaryAsync()
        {
            try
            {
                var budgets = await _context.Budgets.ToListAsync();
                var cashflows = await _context.CashflowEntries.ToListAsync();

                var totalBudget = budgets.Sum(b => b.AllocatedAmount);
                var totalCashflow = cashflows.Sum(c => c.Amount);
                var averageCashflow = cashflows.Count > 0 ? totalCashflow / cashflows.Count : 0m;

                var summary = new Dictionary<string, decimal>
                {
                    { "TotalBudget", totalBudget },
                    { "TotalCashflow", totalCashflow },
                    { "AverageCashflow", averageCashflow }
                };
                return summary;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving financial summary: {ex.Message}", ex);
            }
        }

        // Get forecasts by date range
        public async Task<List<FinancialForecast>> GetForecastsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.FinancialForecasts
                    .Where(f => f.PeriodStart >= startDate && f.PeriodEnd <= endDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving forecasts by date range: {ex.Message}", ex);
            }
        }
    }
}
