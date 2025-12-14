using Microsoft.EntityFrameworkCore;
using FinanceBank.Models.SaaS;

namespace FinanceBank.Data
{
    /// <summary>
    /// Database context for SaaS Management System
    /// Separate from main BFASDbContext to maintain independence
    /// </summary>
    public class SaaSDbContext : DbContext
    {
        private readonly string _dbPath;

        public SaaSDbContext()
        {
            // Use separate database for SaaS management
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = Path.Combine(folder, "FinanceBank", "saas_management.db");
            
            // Ensure directory exists
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public SaaSDbContext(DbContextOptions<SaaSDbContext> options) : base(options)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = Path.Combine(folder, "FinanceBank", "saas_management.db");
        }

        // DbSets
        public DbSet<SystemOwner> SystemOwners { get; set; }
        public DbSet<SystemModule> SystemModules { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<PlanModule> PlanModules { get; set; }
        public DbSet<SaaSClient> Clients { get; set; }
        public DbSet<ClientUser> ClientUsers { get; set; }
        public DbSet<ClientSubscription> ClientSubscriptions { get; set; }
        public DbSet<ClientModule> ClientModules { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<SaaSInvoice> Invoices { get; set; }
        public DbSet<SaaSInvoiceItem> InvoiceItems { get; set; }
        public DbSet<SaaSTransaction> Transactions { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<SaaSActivityLog> ActivityLogs { get; set; }
        public DbSet<LicenseKey> LicenseKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SystemOwner
            modelBuilder.Entity<SystemOwner>(entity =>
            {
                entity.ToTable("SystemOwners");
                entity.HasKey(e => e.OwnerId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // SystemModule
            modelBuilder.Entity<SystemModule>(entity =>
            {
                entity.ToTable("SystemModules");
                entity.HasKey(e => e.ModuleId);
                entity.HasIndex(e => e.ModuleCode).IsUnique();
            });

            // SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("SubscriptionPlans");
                entity.HasKey(e => e.PlanId);
                entity.HasIndex(e => e.PlanCode).IsUnique();
            });

            // PlanModule
            modelBuilder.Entity<PlanModule>(entity =>
            {
                entity.ToTable("PlanModules");
                entity.HasKey(e => e.PlanModuleId);
                entity.HasIndex(e => new { e.PlanId, e.ModuleId }).IsUnique();

                entity.HasOne(e => e.Plan)
                    .WithMany(p => p.PlanModules)
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Module)
                    .WithMany(m => m.PlanModules)
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SaaSClient
            modelBuilder.Entity<SaaSClient>(entity =>
            {
                entity.ToTable("SaaSClients");
                entity.HasKey(e => e.ClientId);
                entity.HasIndex(e => e.ClientCode).IsUnique();
                entity.HasIndex(e => e.PrimaryEmail);
                entity.HasIndex(e => e.Status);
            });

            // ClientUser
            modelBuilder.Entity<ClientUser>(entity =>
            {
                entity.ToTable("ClientUsers");
                entity.HasKey(e => e.ClientUserId);
                entity.HasIndex(e => new { e.ClientId, e.Username }).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Users)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ClientSubscription
            modelBuilder.Entity<ClientSubscription>(entity =>
            {
                entity.ToTable("ClientSubscriptions");
                entity.HasKey(e => e.SubscriptionId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Subscriptions)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Plan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ClientModule
            modelBuilder.Entity<ClientModule>(entity =>
            {
                entity.ToTable("ClientModules");
                entity.HasKey(e => e.ClientModuleId);
                entity.HasIndex(e => new { e.ClientId, e.ModuleId }).IsUnique();

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Modules)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Module)
                    .WithMany(m => m.ClientModules)
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subscription)
                    .WithMany()
                    .HasForeignKey(e => e.SubscriptionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PaymentMethod
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethods");
                entity.HasKey(e => e.PaymentMethodId);
                entity.HasIndex(e => e.MethodCode).IsUnique();
            });

            // SaaSInvoice
            modelBuilder.Entity<SaaSInvoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.HasKey(e => e.InvoiceId);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DueDate);

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Invoices)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subscription)
                    .WithMany()
                    .HasForeignKey(e => e.SubscriptionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SaaSInvoiceItem
            modelBuilder.Entity<SaaSInvoiceItem>(entity =>
            {
                entity.ToTable("InvoiceItems");
                entity.HasKey(e => e.InvoiceItemId);

                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Items)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Module)
                    .WithMany()
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SaaSTransaction
            modelBuilder.Entity<SaaSTransaction>(entity =>
            {
                entity.ToTable("SaaSTransactions");
                entity.HasKey(e => e.TransactionId);
                entity.HasIndex(e => e.TransactionNumber).IsUnique();
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.TransactionDate);

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Transactions)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentMethodId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SupportTicket
            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.ToTable("SupportTickets");
                entity.HasKey(e => e.TicketId);
                entity.HasIndex(e => e.TicketNumber).IsUnique();
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.SupportTickets)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.ClientUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TicketComment
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.ToTable("TicketComments");
                entity.HasKey(e => e.CommentId);

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.ClientUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SaaSActivityLog
            modelBuilder.Entity<SaaSActivityLog>(entity =>
            {
                entity.ToTable("SaaSActivityLog");
                entity.HasKey(e => e.LogId);
            });

            // LicenseKey
            modelBuilder.Entity<LicenseKey>(entity =>
            {
                entity.ToTable("LicenseKeys");
                entity.HasKey(e => e.LicenseId);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.ClientId);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Default System Owner
            modelBuilder.Entity<SystemOwner>().HasData(new SystemOwner
            {
                OwnerId = 1,
                Username = "admin",
                PasswordHash = "admin123",
                Email = "admin@erpsolutions.com",
                FullName = "System Administrator",
                CompanyName = "ERP Solutions Inc.",
                CompanyAddress = "123 Tech Park, Manila, Philippines",
                Phone = "+63 912 345 6789",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            // Default Payment Methods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { PaymentMethodId = 1, MethodCode = "GCASH", MethodName = "GCash", AccountName = "ERP Solutions Inc.", AccountNumber = "09171234567", Instructions = "Send payment to the GCash number above and upload proof of payment.", IsActive = true, SortOrder = 1 },
                new PaymentMethod { PaymentMethodId = 2, MethodCode = "PAYMAYA", MethodName = "PayMaya", AccountName = "ERP Solutions Inc.", AccountNumber = "09181234567", Instructions = "Send payment to the PayMaya number above and upload proof of payment.", IsActive = true, SortOrder = 2 },
                new PaymentMethod { PaymentMethodId = 3, MethodCode = "BDO", MethodName = "BDO Bank Transfer", AccountName = "ERP Solutions Inc.", AccountNumber = "001234567890", BankName = "BDO Unibank", Instructions = "Transfer to the account above and upload deposit slip.", IsActive = true, SortOrder = 3 },
                new PaymentMethod { PaymentMethodId = 4, MethodCode = "BPI", MethodName = "BPI Bank Transfer", AccountName = "ERP Solutions Inc.", AccountNumber = "1234567890", BankName = "Bank of the Philippine Islands", Instructions = "Transfer to the account above and upload deposit slip.", IsActive = true, SortOrder = 4 },
                new PaymentMethod { PaymentMethodId = 5, MethodCode = "CASH", MethodName = "Cash Payment", AccountName = "ERP Solutions Inc.", Instructions = "Visit our office for cash payment. Receipt will be issued upon payment.", IsActive = true, SortOrder = 5 }
            );

            // Default System Modules
            modelBuilder.Entity<SystemModule>().HasData(
                // Core Modules
                new SystemModule { ModuleId = 1, ModuleCode = "DASHBOARD", ModuleName = "Dashboard", Category = "Core", IsCore = true, MonthlyPrice = 0, SortOrder = 1 },
                new SystemModule { ModuleId = 2, ModuleCode = "USER_MGMT", ModuleName = "User Management", Category = "Core", IsCore = true, MonthlyPrice = 0, SortOrder = 2 },
                new SystemModule { ModuleId = 3, ModuleCode = "SETTINGS", ModuleName = "System Settings", Category = "Core", IsCore = true, MonthlyPrice = 0, SortOrder = 3 },
                // Customer
                new SystemModule { ModuleId = 4, ModuleCode = "CUSTOMER_REG", ModuleName = "Customer Registration", Category = "Customer", MonthlyPrice = 500, SortOrder = 10 },
                new SystemModule { ModuleId = 5, ModuleCode = "CUSTOMER_ACCT", ModuleName = "Customer Accounts", Category = "Customer", MonthlyPrice = 750, SortOrder = 11 },
                // Teller
                new SystemModule { ModuleId = 6, ModuleCode = "TELLER_DASHBOARD", ModuleName = "Teller Dashboard", Category = "Teller", MonthlyPrice = 1000, SortOrder = 20 },
                new SystemModule { ModuleId = 7, ModuleCode = "DEPOSITS", ModuleName = "Deposit Processing", Category = "Teller", MonthlyPrice = 750, SortOrder = 21 },
                new SystemModule { ModuleId = 8, ModuleCode = "WITHDRAWALS", ModuleName = "Withdrawal Processing", Category = "Teller", MonthlyPrice = 750, SortOrder = 22 },
                new SystemModule { ModuleId = 9, ModuleCode = "TRANSFERS", ModuleName = "Fund Transfers", Category = "Teller", MonthlyPrice = 750, SortOrder = 23 },
                // Savings
                new SystemModule { ModuleId = 10, ModuleCode = "SAVINGS", ModuleName = "Savings Accounts", Category = "Savings", MonthlyPrice = 1250, SortOrder = 30 },
                new SystemModule { ModuleId = 11, ModuleCode = "SAVINGS_INTEREST", ModuleName = "Interest Posting", Category = "Savings", MonthlyPrice = 750, SortOrder = 31 },
                // Loans
                new SystemModule { ModuleId = 12, ModuleCode = "LOAN_MGMT", ModuleName = "Loan Management", Category = "Loans", MonthlyPrice = 1500, SortOrder = 40 },
                new SystemModule { ModuleId = 13, ModuleCode = "LOAN_RELEASE", ModuleName = "Loan Release", Category = "Loans", MonthlyPrice = 1000, SortOrder = 41 },
                new SystemModule { ModuleId = 14, ModuleCode = "LOAN_PAYMENTS", ModuleName = "Loan Payments", Category = "Loans", MonthlyPrice = 1000, SortOrder = 42 },
                // Accounting
                new SystemModule { ModuleId = 15, ModuleCode = "CHART_ACCTS", ModuleName = "Chart of Accounts", Category = "Accounting", MonthlyPrice = 1000, SortOrder = 50 },
                new SystemModule { ModuleId = 16, ModuleCode = "JOURNAL", ModuleName = "Journal Entries", Category = "Accounting", MonthlyPrice = 1000, SortOrder = 51 },
                new SystemModule { ModuleId = 17, ModuleCode = "GENERAL_LEDGER", ModuleName = "General Ledger", Category = "Accounting", MonthlyPrice = 1250, SortOrder = 52 },
                new SystemModule { ModuleId = 18, ModuleCode = "FIN_STATEMENTS", ModuleName = "Financial Statements", Category = "Accounting", MonthlyPrice = 1250, SortOrder = 53 },
                // Finance Manager
                new SystemModule { ModuleId = 19, ModuleCode = "FM_DASHBOARD", ModuleName = "Finance Manager Dashboard", Category = "Finance", MonthlyPrice = 1500, SortOrder = 60 },
                new SystemModule { ModuleId = 20, ModuleCode = "BUDGET_MGMT", ModuleName = "Budget Management", Category = "Finance", MonthlyPrice = 1250, SortOrder = 61 },
                // Reports
                new SystemModule { ModuleId = 21, ModuleCode = "REPORTS_BASIC", ModuleName = "Basic Reports", Category = "Reports", MonthlyPrice = 750, SortOrder = 70 },
                new SystemModule { ModuleId = 22, ModuleCode = "AUDIT_TRAIL", ModuleName = "Audit Trail", Category = "Reports", MonthlyPrice = 1000, SortOrder = 71 },
                // Admin
                new SystemModule { ModuleId = 23, ModuleCode = "ADMIN_FULL", ModuleName = "Full Administration", Category = "Admin", MonthlyPrice = 2500, SortOrder = 80 }
            );

            // Default Subscription Plans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { PlanId = 1, PlanCode = "BASIC", PlanName = "Basic Plan", Description = "Essential features for small organizations", MonthlyPrice = 4999, YearlyPrice = 49990, SetupFee = 5000, MaxUsers = 5, SupportLevel = "Basic", SortOrder = 1 },
                new SubscriptionPlan { PlanId = 2, PlanCode = "PRO", PlanName = "Professional Plan", Description = "Advanced features for growing businesses", MonthlyPrice = 9999, YearlyPrice = 99990, SetupFee = 10000, MaxUsers = 15, SupportLevel = "Priority", IsPopular = true, SortOrder = 2 },
                new SubscriptionPlan { PlanId = 3, PlanCode = "ENTERPRISE", PlanName = "Enterprise Plan", Description = "Full-featured solution for large organizations", MonthlyPrice = 24999, YearlyPrice = 249990, SetupFee = 25000, MaxUsers = 50, SupportLevel = "Premium", SortOrder = 3 }
            );
        }

        /// <summary>
        /// Initialize the database and ensure it's created
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                await Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SaaS database: {ex.Message}");
            }
        }
    }
}
