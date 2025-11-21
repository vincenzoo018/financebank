using Microsoft.Extensions.Logging;
using FinanceBank.Services;
using FinanceBank.Data;
using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;

namespace FinanceBank
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Add Entity Framework DbContext
            builder.Services.AddDbContext<BFASDbContext>(options =>
            {
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=BFASdatabase;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;";
                options.UseSqlServer(connectionString);
            });

            // Register AuthService as Singleton (persists across page navigations)
            builder.Services.AddSingleton<AuthService>();
            
            // Register Role-Based Navigation Service
            builder.Services.AddScoped<RoleBasedNavigationService>();

            // Register User Registration Service
            builder.Services.AddScoped<UserRegistrationService>();

            // Register User CRUD Service
            builder.Services.AddScoped<UserCrudService>();

            // Register Customer Banking Service (customer-facing deposits, withdrawals, bills)
            builder.Services.AddScoped<CustomerBankingService>();

            // Register Teller Banking Service (teller/admin processing deposits and withdrawals)
            builder.Services.AddScoped<TellerBankingService>();

            // Register Invoice Service (creates and manages invoices for all transactions)
            builder.Services.AddScoped<InvoiceService>();

            // Register CRUD Services
            builder.Services.AddScoped<BankAccountService>();
            builder.Services.AddScoped<FundTransferService>();
            builder.Services.AddScoped<JournalEntryService>();
            builder.Services.AddScoped<BudgetManagementService>();
            builder.Services.AddScoped<CustomerAccountService>();
            builder.Services.AddScoped<CustomerTransactionService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

