using Microsoft.Extensions.Logging;
using FinanceBank.Services;
using FinanceBank.Data;
using Microsoft.EntityFrameworkCore;

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
                var connectionString = builder.Configuration["ConnectionStrings:BFASConnection"] 
                    ?? "Server=localhost;Database=BFASdatabase;Trusted_Connection=True;TrustServerCertificate=True;";
                options.UseSqlServer(connectionString);
            });

            // Register AuthService as Singleton (persists across page navigations)
            builder.Services.AddSingleton<AuthService>();
            
            // Register Role-Based Navigation Service
            builder.Services.AddScoped<RoleBasedNavigationService>();

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

