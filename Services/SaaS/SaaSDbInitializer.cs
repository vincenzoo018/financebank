using Microsoft.Extensions.DependencyInjection;
using FinanceBank.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceBank.Services.SaaS
{
    /// <summary>
    /// Service to initialize the SaaS database
    /// Called manually on app startup since IHostedService doesn't work well in MAUI
    /// </summary>
    public static class SaaSDbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SaaSDbContext>();
                
                // Ensure database is created with all tables
                await context.Database.EnsureCreatedAsync();
                
                System.Diagnostics.Debug.WriteLine("SaaS Database initialized successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing SaaS database: {ex.Message}");
            }
        }
    }
}
