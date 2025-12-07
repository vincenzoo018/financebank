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

            // Add Entity Framework DbContext with Factory pattern for proper lifetime management
            // Using lazy initialization to prevent startup errors
            try
            {
                // Cloud Database Connection (MonsterASP)
                // var connectionString = "Server=db34283.public.databaseasp.net,1433;Database=db34283;User Id=db34283;Password=Zx6=2+fXCm8!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;";

                // Local Database Connection (Development)
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=BFASdatabase;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;";

                builder.Services.AddDbContextFactory<BFASDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.EnableSensitiveDataLogging(false);
                });

                // Also add regular DbContext for scoped usage
                builder.Services.AddDbContext<BFASDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.EnableSensitiveDataLogging(false);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database context registration error: {ex.Message}");
                // Continue without database context - will use fallback auth
            }

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
            builder.Services.AddScoped<BillerService>();
            builder.Services.AddScoped<BankingReportService>();
            builder.Services.AddScoped<LoanManagementService>();
            builder.Services.AddScoped<CardManagementService>();

            // Register Accounting Services
            builder.Services.AddScoped<JournalEntryService>();
            builder.Services.AddScoped<GeneralLedgerService>();
            builder.Services.AddScoped<TrialBalanceService>();
            builder.Services.AddScoped<FinancialStatementService>();
            builder.Services.AddScoped<FinancialReportingService>();  // New enhanced service

            // Register other services
            builder.Services.AddScoped<BudgetManagementService>();
            builder.Services.AddScoped<AccountsPayableService>();
            builder.Services.AddScoped<AccountsReceivableService>();
            builder.Services.AddScoped<CashflowAnalysisService>();
            builder.Services.AddScoped<FinancialForecastService>();
            builder.Services.AddScoped<CustomerAccountService>();
            builder.Services.AddScoped<CustomerTransactionService>();
            builder.Services.AddScoped<CardService>();
            builder.Services.AddScoped<LoanService>();
            builder.Services.AddScoped<SavingsGoalService>();
            builder.Services.AddScoped<RewardPointsService>();
            builder.Services.AddScoped<AuditLogService>();

            // Register Security and ERP Services
            builder.Services.AddScoped<PasswordHashingService>();
            builder.Services.AddScoped<TaxCalculationService>();
            builder.Services.AddScoped<AccountingEntryService>();
            builder.Services.AddScoped<TransactionValidationService>();

            // Register Card Application Service (card and loan approvals)
            builder.Services.AddScoped<CardApplicationService>();

            // Register CRUD Services Layer
            builder.Services.AddScoped<BankingService>();
            builder.Services.AddScoped<AccountingService>();
            builder.Services.AddScoped<FinanceService>();
            builder.Services.AddScoped<ApprovalsService>();

            // Register Automatic GL Posting Service (real-time General Ledger posting)
            builder.Services.AddScoped<AutomaticGLPostingService>();

            // Register Loan Process Services
            builder.Services.AddScoped<LoanProcessService>();
            builder.Services.AddScoped<LoanPaymentService>();
            builder.Services.AddScoped<LoanEligibilityService>();

            // Register Teller Report Service (analytics and report generation)
            builder.Services.AddScoped<TellerReportService>();

            // Register Database Sync Service (LOCAL <-> CLOUD synchronization)
            builder.Services.AddSingleton<DatabaseSyncService>();

            // Register Sync-Aware DbContext Service (wraps DB operations with auto-sync)
            builder.Services.AddScoped<SyncAwareDbContextService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

