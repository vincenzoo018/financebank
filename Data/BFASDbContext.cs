using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;

namespace FinanceBank.Data
{
    public class BFASDbContext : DbContext
    {
        public BFASDbContext(DbContextOptions<BFASDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress pending model changes warning for migrations
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        // Authentication Tables
        public DbSet<AuthUser> Users { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Employee> Employees { get; set; }

        // Banking Module Tables
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<FundTransfer> FundTransfers { get; set; }
        public DbSet<Biller> Billers { get; set; }
        public DbSet<BankingReport> BankingReports { get; set; }
        public DbSet<LoanManagementEntity> LoanManagementRecords { get; set; }
        public DbSet<CardManagementEntity> CardManagementRecords { get; set; }

        // Accounting Module Tables
        public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<AccountsPayable> AccountsPayables { get; set; }
        public DbSet<AccountsReceivable> AccountsReceivables { get; set; }
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<TrialBalanceEntry> TrialBalances { get; set; }
        public DbSet<FinancialStatement> FinancialStatements { get; set; }

        // Finance Module Tables
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<CashflowEntry> CashflowEntries { get; set; }
        public DbSet<FinancialForecast> FinancialForecasts { get; set; }

        // Customer Portal Tables
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<SavingsGoalEntity> SavingsGoals { get; set; }
        public DbSet<RewardPointsEntity> RewardPoints { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        // Loan Process Tables
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanAssessment> LoanAssessments { get; set; }
        public DbSet<LoanApproval> LoanApprovals { get; set; }
        public DbSet<LoanPaymentSchedule> LoanPaymentSchedules { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
        public DbSet<LoanViolation> LoanViolations { get; set; }
        public DbSet<LoanDisbursal> LoanDisbursals { get; set; }
        public DbSet<LoanTransactionHistory> LoanTransactionHistory { get; set; }
        public DbSet<LoanInvoice> LoanInvoices { get; set; }

        // Approval Module Tables
        public DbSet<ApprovalQueueEntity> ApprovalQueues { get; set; }

        // Customer Card Tables
        public DbSet<CustomerCardEntity> CustomerCards { get; set; }

        // System / Audit Tables
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AccountingEntry> AccountingEntries { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AuthUser entity
            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure LoginHistory entity
            modelBuilder.Entity<LoginHistory>(entity =>
            {
                entity.HasKey(e => e.LoginId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LoginTime);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.LoginHistories)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.LoginTime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LoginStatus).HasDefaultValue("Success");
            });

            // Configure UserSession entity
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserSessions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
                entity.HasIndex(e => e.RoleName);
                entity.HasIndex(e => e.ModuleName);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.IsActive);

                // Configure EmployeeId as identity column (auto-generated by database)
                entity.Property(e => e.EmployeeId)
                    .HasColumnType("int")
                    .UseIdentityColumn();

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Employee)
                    .HasForeignKey<AuthUser>(u => u.EmployeeId_FK)
                    .HasPrincipalKey<Employee>(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure CustomerAccount <-> CustomerTransaction relationships
            modelBuilder.Entity<CustomerTransaction>(entity =>
            {
                entity.HasOne(ct => ct.Account)
                    .WithMany(ca => ca.Transactions)
                    .HasForeignKey(ct => ct.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ct => ct.ToAccount)
                    .WithMany()
                    .HasForeignKey(ct => ct.ToAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.AccountId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Account)
                    .WithMany()
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Transaction)
                    .WithMany()
                    .HasForeignKey(e => e.TransactionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure Finance Module - Map entities to actual database table names
            modelBuilder.Entity<Budget>()
                .ToTable("BudgetManagement");

            modelBuilder.Entity<CashflowEntry>()
                .ToTable("CashflowAnalysis");

            modelBuilder.Entity<FinancialForecast>()
                .ToTable("FinancialForecasting");

            // Configure ChartOfAccounts entity
            modelBuilder.Entity<ChartOfAccounts>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.HasIndex(e => e.AccountCode).IsUnique();
                entity.HasIndex(e => e.AccountType);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.NormalBalance).HasDefaultValue("Debit");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsHeader).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Self-referencing relationship for hierarchical accounts
                entity.HasOne(e => e.ParentAccount)
                    .WithMany(e => e.SubAccounts)
                    .HasForeignKey(e => e.ParentAccountId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure JournalEntry entity
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasKey(e => e.JournalId);
                entity.HasIndex(e => e.JournalNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.Status).HasDefaultValue("Draft");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure JournalEntryLine entity
            modelBuilder.Entity<JournalEntryLine>(entity =>
            {
                entity.HasKey(e => e.LineId);
                entity.HasIndex(e => e.JournalId);
                entity.HasIndex(e => e.AccountCode);

                entity.HasOne(e => e.Journal)
                    .WithMany(j => j.Lines)
                    .HasForeignKey(e => e.JournalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure GeneralLedgerEntry entity
            modelBuilder.Entity<GeneralLedgerEntry>(entity =>
            {
                entity.HasKey(e => e.EntryId);
                entity.HasIndex(e => e.AccountCode);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.Status).HasDefaultValue("Posted");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Journal)
                    .WithMany()
                    .HasForeignKey(e => e.JournalId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure TrialBalanceEntry entity
            modelBuilder.Entity<TrialBalanceEntry>(entity =>
            {
                entity.HasKey(e => e.TrialBalanceId);
                entity.HasIndex(e => e.AccountCode);
                entity.HasIndex(e => e.AsOfDate);
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure FinancialStatement entity
            modelBuilder.Entity<FinancialStatement>(entity =>
            {
                entity.HasKey(e => e.StatementId);
                entity.HasIndex(e => e.StatementType);
                entity.HasIndex(e => e.PeriodStart);
                entity.HasIndex(e => e.PeriodEnd);

                entity.Property(e => e.Status).HasDefaultValue("Draft");
                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}

