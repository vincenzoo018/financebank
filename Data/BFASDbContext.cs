using Microsoft.EntityFrameworkCore;
using FinanceBank.Models;

namespace FinanceBank.Data
{
    public class BFASDbContext : DbContext
    {
        public BFASDbContext(DbContextOptions<BFASDbContext> options) : base(options)
        {
        }

        // Authentication Tables
        public DbSet<AuthUser> Users { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Banking Module Tables
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<FundTransfer> FundTransfers { get; set; }

        // Accounting Module Tables
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }

        // Finance Module Tables
        public DbSet<Budget> Budgets { get; set; }

        // Customer Portal Tables
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }

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
        }
    }
}

