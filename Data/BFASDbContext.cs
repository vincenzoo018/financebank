using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using FinanceBank.Models;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace FinanceBank.Data
{
    public class BFASDbContext : DbContext
    {
        // CLOUD Database Connection String - for dual-write replication
        private const string CloudConnectionString = "Server=db34283.public.databaseasp.net,1433;Database=db34283;User Id=db34283;Password=Zx6=2+fXCm8!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;";

        // Cloud availability caching
        private static bool _cloudAvailable = true;
        private static DateTime _lastCloudCheck = DateTime.MinValue;
        private const int CLOUD_CHECK_INTERVAL_SECONDS = 5; // Check every 5 seconds (FASTER)

        // Entity metadata cache for performance
        private static readonly ConcurrentDictionary<Type, EntityMetadataInfo> _metadataCache = new();

        // Enable/disable dual write (can be toggled)
        public static bool DualWriteEnabled { get; set; } = true;

        // Retry configuration for dual-write
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MS = 1000; // 1 second between retries

        public BFASDbContext(DbContextOptions<BFASDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress pending model changes warning for migrations
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        #region Dual-Write SaveChanges Override

        /// <summary>
        /// Override SaveChangesAsync to replicate changes to CLOUD database
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Capture pending changes BEFORE saving
            var pendingChanges = DualWriteEnabled ? CapturePendingChanges() : new List<DualWriteChange>();

            // Save to LOCAL database first (primary)
            var result = await base.SaveChangesAsync(cancellationToken);

            // Replicate to CLOUD if enabled and we have changes
            if (DualWriteEnabled && pendingChanges.Any())
            {
                // Update insert records with generated IDs
                UpdateInsertedIds(pendingChanges);

                // Fire-and-forget cloud replication (don't block the user)
                _ = Task.Run(() => ReplicateToCloudAsync(pendingChanges));
            }

            return result;
        }

        /// <summary>
        /// Override SaveChanges to replicate changes to CLOUD database
        /// </summary>
        public override int SaveChanges()
        {
            // Capture pending changes BEFORE saving
            var pendingChanges = DualWriteEnabled ? CapturePendingChanges() : new List<DualWriteChange>();

            // Save to LOCAL database first (primary)
            var result = base.SaveChanges();

            // Replicate to CLOUD if enabled and we have changes
            if (DualWriteEnabled && pendingChanges.Any())
            {
                // Update insert records with generated IDs
                UpdateInsertedIds(pendingChanges);

                // Fire-and-forget cloud replication (don't block the user)
                _ = Task.Run(() => ReplicateToCloudAsync(pendingChanges));
            }

            return result;
        }

        /// <summary>
        /// Capture all pending changes from change tracker
        /// </summary>
        private List<DualWriteChange> CapturePendingChanges()
        {
            var changes = new List<DualWriteChange>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.State == EntityState.Deleted)
                {
                    var entityType = entry.Entity.GetType();
                    var metadata = GetEntityMetadata(entityType);
                    if (metadata == null) continue;

                    var change = new DualWriteChange
                    {
                        Entity = entry.Entity,
                        TableName = metadata.TableName,
                        PrimaryKeyColumn = metadata.PrimaryKeyColumn,
                        HasIdentity = metadata.HasIdentity,
                        Operation = entry.State switch
                        {
                            EntityState.Added => "INSERT",
                            EntityState.Modified => "UPDATE",
                            EntityState.Deleted => "DELETE",
                            _ => "NONE"
                        }
                    };

                    // Capture values
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        change.Values = new Dictionary<string, object?>();
                        foreach (var prop in entry.Properties)
                        {
                            // Skip navigation properties
                            if (prop.Metadata.IsPrimaryKey() || !prop.Metadata.IsKey())
                            {
                                var colName = prop.Metadata.GetColumnName();
                                change.Values[colName] = prop.CurrentValue;
                            }
                        }
                    }

                    // For updates/deletes, get the PK value
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        var pkProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                        if (pkProp != null)
                        {
                            change.PrimaryKeyValue = pkProp.CurrentValue;
                        }
                    }

                    changes.Add(change);
                }
            }

            return changes;
        }

        /// <summary>
        /// After local save, update insert changes with generated IDs
        /// </summary>
        private void UpdateInsertedIds(List<DualWriteChange> changes)
        {
            foreach (var change in changes.Where(c => c.Operation == "INSERT"))
            {
                if (string.IsNullOrEmpty(change.PrimaryKeyColumn)) continue;

                var metadata = GetEntityMetadata(change.Entity.GetType());
                if (metadata == null) continue;

                // Get the generated ID from the entity
                var prop = change.Entity.GetType().GetProperty(metadata.PrimaryKeyProperty);
                if (prop != null)
                {
                    var pkValue = prop.GetValue(change.Entity);
                    change.PrimaryKeyValue = pkValue;

                    // Add PK to values if not present
                    if (change.Values != null && !change.Values.ContainsKey(change.PrimaryKeyColumn))
                    {
                        change.Values[change.PrimaryKeyColumn] = pkValue;
                    }
                }
            }
        }

        /// <summary>
        /// Replicate changes to CLOUD database with retry logic
        /// </summary>
        private async Task ReplicateToCloudAsync(List<DualWriteChange> changes)
        {
            if (!await IsCloudAvailableAsync())
            {
                System.Diagnostics.Debug.WriteLine("☁️ DUAL-WRITE: Cloud unavailable - data saved to LOCAL only");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    using var cloudConn = new SqlConnection(CloudConnectionString);
                    await cloudConn.OpenAsync();

                    foreach (var change in changes)
                    {
                        try
                        {
                            switch (change.Operation)
                            {
                                case "INSERT":
                                    await ExecuteCloudInsertAsync(cloudConn, change);
                                    break;
                                case "UPDATE":
                                    await ExecuteCloudUpdateAsync(cloudConn, change);
                                    break;
                                case "DELETE":
                                    await ExecuteCloudDeleteAsync(cloudConn, change);
                                    break;
                            }
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            System.Diagnostics.Debug.WriteLine($"⚠️ DUAL-WRITE: Failed {change.Operation} on {change.TableName}: {ex.Message}");
                        }
                    }

                    if (successCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ DUAL-WRITE: Replicated {successCount} changes to CLOUD (attempt {attempt})");
                    }
                    if (failCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ DUAL-WRITE: {failCount} changes failed to replicate");
                    }

                    // Success - exit retry loop
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ DUAL-WRITE: Cloud connection failed (attempt {attempt}/{MAX_RETRY_ATTEMPTS}): {ex.Message}");

                    if (attempt < MAX_RETRY_ATTEMPTS)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔄 DUAL-WRITE: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS);
                    }
                    else
                    {
                        _cloudAvailable = false;
                        System.Diagnostics.Debug.WriteLine("❌ DUAL-WRITE: All retry attempts failed - marking cloud as unavailable");
                    }
                }
            }
        }

        private async Task ExecuteCloudInsertAsync(SqlConnection conn, DualWriteChange change)
        {
            if (change.Values == null || !change.Values.Any()) return;

            var columns = change.Values.Keys.ToList();
            var columnList = string.Join(", ", columns.Select(c => $"[{c}]"));
            var paramList = string.Join(", ", columns.Select((c, i) => $"@p{i}"));

            string sql;
            if (change.HasIdentity && !string.IsNullOrEmpty(change.PrimaryKeyColumn))
            {
                sql = $@"
                    SET IDENTITY_INSERT [{change.TableName}] ON;
                    IF NOT EXISTS (SELECT 1 FROM [{change.TableName}] WHERE [{change.PrimaryKeyColumn}] = @pkCheck)
                    BEGIN
                        INSERT INTO [{change.TableName}] ({columnList}) VALUES ({paramList});
                    END
                    SET IDENTITY_INSERT [{change.TableName}] OFF;";
            }
            else
            {
                sql = $"INSERT INTO [{change.TableName}] ({columnList}) VALUES ({paramList})";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 60;

            // Add PK check parameter
            if (change.HasIdentity && change.PrimaryKeyValue != null)
            {
                cmd.Parameters.AddWithValue("@pkCheck", change.PrimaryKeyValue);
            }

            var values = change.Values.Values.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);
            }

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ExecuteCloudUpdateAsync(SqlConnection conn, DualWriteChange change)
        {
            if (change.Values == null || change.PrimaryKeyValue == null || string.IsNullOrEmpty(change.PrimaryKeyColumn))
                return;

            var updateCols = change.Values.Where(v => v.Key != change.PrimaryKeyColumn).ToList();
            if (!updateCols.Any()) return;

            var setClause = string.Join(", ", updateCols.Select((v, i) => $"[{v.Key}] = @p{i}"));
            var sql = $"UPDATE [{change.TableName}] SET {setClause} WHERE [{change.PrimaryKeyColumn}] = @pkValue";

            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 60;

            for (int i = 0; i < updateCols.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", updateCols[i].Value ?? DBNull.Value);
            }
            cmd.Parameters.AddWithValue("@pkValue", change.PrimaryKeyValue);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ExecuteCloudDeleteAsync(SqlConnection conn, DualWriteChange change)
        {
            if (change.PrimaryKeyValue == null || string.IsNullOrEmpty(change.PrimaryKeyColumn))
                return;

            var sql = $"DELETE FROM [{change.TableName}] WHERE [{change.PrimaryKeyColumn}] = @pkValue";

            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 60;
            cmd.Parameters.AddWithValue("@pkValue", change.PrimaryKeyValue);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Check if cloud database is available (with caching)
        /// </summary>
        private static async Task<bool> IsCloudAvailableAsync()
        {
            if ((DateTime.Now - _lastCloudCheck).TotalSeconds < CLOUD_CHECK_INTERVAL_SECONDS)
                return _cloudAvailable;

            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    _cloudAvailable = false;
                    _lastCloudCheck = DateTime.Now;
                    return false;
                }

                using var conn = new SqlConnection(CloudConnectionString.Replace("Connection Timeout=30", "Connection Timeout=5"));
                await conn.OpenAsync();
                _cloudAvailable = true;
            }
            catch
            {
                _cloudAvailable = false;
            }

            _lastCloudCheck = DateTime.Now;
            return _cloudAvailable;
        }

        /// <summary>
        /// Force cloud availability check (reset cache)
        /// </summary>
        public static void ResetCloudAvailability()
        {
            _lastCloudCheck = DateTime.MinValue;
        }

        /// <summary>
        /// Get current cloud availability status
        /// </summary>
        public static bool IsCloudAvailable => _cloudAvailable;

        /// <summary>
        /// Get entity metadata (table name, primary key, etc.)
        /// </summary>
        private EntityMetadataInfo? GetEntityMetadata(Type entityType)
        {
            if (_metadataCache.TryGetValue(entityType, out var cached))
                return cached;

            try
            {
                // Get table name
                var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
                var tableName = tableAttr?.Name ?? entityType.Name;

                // Handle pluralization for common entities
                if (tableName == "AuthUser") tableName = "Users";
                else if (!tableName.EndsWith("s") && !tableName.Contains("Entry") && !tableName.Contains("History"))
                {
                    // Don't pluralize if already has Table attribute
                    if (tableAttr == null) tableName = tableName + "s";
                }

                // Find primary key
                string? pkColumn = null;
                string? pkProperty = null;
                bool hasIdentity = true;

                var properties = entityType.GetProperties();

                // Look for [Key] attribute
                var keyProp = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
                if (keyProp != null)
                {
                    pkProperty = keyProp.Name;
                    var colAttr = keyProp.GetCustomAttribute<ColumnAttribute>();
                    pkColumn = colAttr?.Name ?? keyProp.Name;
                    hasIdentity = keyProp.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption != DatabaseGeneratedOption.None;
                }
                else
                {
                    // Convention: look for Id or {Type}Id
                    keyProp = properties.FirstOrDefault(p =>
                        p.Name == "Id" ||
                        p.Name == entityType.Name + "Id" ||
                        p.Name == entityType.Name.Replace("Entity", "") + "Id");

                    if (keyProp != null)
                    {
                        pkProperty = keyProp.Name;
                        var colAttr = keyProp.GetCustomAttribute<ColumnAttribute>();
                        pkColumn = colAttr?.Name ?? keyProp.Name;
                    }
                }

                var metadata = new EntityMetadataInfo
                {
                    TableName = tableName,
                    PrimaryKeyColumn = pkColumn,
                    PrimaryKeyProperty = pkProperty,
                    HasIdentity = hasIdentity
                };

                _metadataCache[entityType] = metadata;
                return metadata;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Supporting Classes for Dual-Write

        private class DualWriteChange
        {
            public object Entity { get; set; } = null!;
            public string TableName { get; set; } = "";
            public string Operation { get; set; } = "";
            public string? PrimaryKeyColumn { get; set; }
            public object? PrimaryKeyValue { get; set; }
            public bool HasIdentity { get; set; } = true;
            public Dictionary<string, object?>? Values { get; set; }
        }

        private class EntityMetadataInfo
        {
            public string TableName { get; set; } = "";
            public string? PrimaryKeyColumn { get; set; }
            public string? PrimaryKeyProperty { get; set; }
            public bool HasIdentity { get; set; } = true;
        }

        #endregion

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
        public DbSet<GeneralLedger> GeneralLedger { get; set; }
        public DbSet<GeneralLedgerTransaction> GeneralLedgerTransactions { get; set; }
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
        public DbSet<LoanDocument> LoanDocuments { get; set; }

        // Approval Module Tables
        public DbSet<ApprovalQueueEntity> ApprovalQueues { get; set; }

        // Customer Card Tables
        public DbSet<CustomerCardEntity> CustomerCards { get; set; }

        // System / Audit Tables
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AccountingEntry> AccountingEntries { get; set; }
        public DbSet<UnifiedTransactionHistory> UnifiedTransactionHistory { get; set; }

        // Savings Account Module Tables
        public DbSet<SavingsAccountType> SavingsAccountTypes { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<SavingsTransaction> SavingsTransactions { get; set; }
        public DbSet<SavingsInterest> SavingsInterestRecords { get; set; }
        public DbSet<SavingsInterestPosting> SavingsInterestPostings { get; set; }
        public DbSet<SavingsWithdrawalRequest> SavingsWithdrawalRequests { get; set; }

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

            // =====================================================================
            // IGNORE NEW CustomerAccount COLUMNS THAT DON'T EXIST IN DATABASE YET
            // Run LOAN_PROCESS_ENHANCEMENT.sql to add these columns, then remove this section
            // =====================================================================
            modelBuilder.Entity<CustomerAccount>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                // Ignore new columns that don't exist in the database yet
                entity.Ignore(e => e.AccountType);
                entity.Ignore(e => e.AccountPurpose);
                entity.Ignore(e => e.InitialDeposit);
                entity.Ignore(e => e.EligibilityStatus);
                entity.Ignore(e => e.EligibilityRemarks);
                entity.Ignore(e => e.EligibilityCheckedBy);
                entity.Ignore(e => e.EligibilityCheckedAt);
                entity.Ignore(e => e.CanApplyForLoan);
                entity.Ignore(e => e.LoanEligibilityReason);
                entity.Ignore(e => e.MaxLoanAmount);
            });

            // =====================================================================
            // LOAN APPLICATION CONFIGURATION
            // Database columns added via ADD_LOAN_DOCUMENT_COLUMNS.sql
            // =====================================================================
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasKey(e => e.ApplicationId);

                // These columns still need to be added via LOAN_PROCESS_ENHANCEMENT.sql
                entity.Ignore(e => e.RequestedInterestRate);
                entity.Ignore(e => e.ExistingDebtsDetails);
            });

            // =====================================================================
            // IGNORE NEW LoanAssessment COLUMNS THAT DON'T EXIST IN DATABASE YET
            // =====================================================================
            modelBuilder.Entity<LoanAssessment>(entity =>
            {
                entity.HasKey(e => e.AssessmentId);

                // Ignore new columns that don't exist in the database yet
                entity.Ignore(e => e.CreditScore);
                entity.Ignore(e => e.CreditScoreSource);
                entity.Ignore(e => e.DebtToIncomeRatio);
                entity.Ignore(e => e.RiskScore);
                entity.Ignore(e => e.RiskCategory);
                entity.Ignore(e => e.RiskAssessmentDetails);
                entity.Ignore(e => e.IdentityVerified);
                entity.Ignore(e => e.IncomeVerified);
                entity.Ignore(e => e.AddressVerified);
                entity.Ignore(e => e.EmploymentVerified);
                entity.Ignore(e => e.CollateralVerified);
                entity.Ignore(e => e.CoBorrowerVerified);
                entity.Ignore(e => e.BankStatementsReviewed);
                entity.Ignore(e => e.AverageMonthlyBalance);
                entity.Ignore(e => e.VerificationNotes);
                entity.Ignore(e => e.AccountantRecommendation);
                entity.Ignore(e => e.RecommendationReason);
                entity.Ignore(e => e.SuggestedAmount);
                entity.Ignore(e => e.SuggestedTerm);
                entity.Ignore(e => e.SuggestedRate);
                // NOTE: RejectionReason, RejectedAt, RejectedBy exist in DB but not in model - no need to ignore
            });

            // =====================================================================
            // IGNORE NEW LoanApproval COLUMNS THAT DON'T EXIST IN DATABASE YET
            // =====================================================================
            modelBuilder.Entity<LoanApproval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);

                // Ignore new columns that don't exist in the database yet
                entity.Ignore(e => e.ApprovalReason);
                entity.Ignore(e => e.ReviewedCreditScore);
                entity.Ignore(e => e.ReviewedDTI);
                entity.Ignore(e => e.ReviewedRiskAssessment);
                entity.Ignore(e => e.ReviewedCollateral);
                entity.Ignore(e => e.ReviewedCoBorrower);
                entity.Ignore(e => e.FinalDecisionNotes);
                entity.Ignore(e => e.RequiresAdditionalDocuments);
                entity.Ignore(e => e.AdditionalDocumentsRequired);
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

                entity.Property(e => e.Level).HasDefaultValue(1);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
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

            // Configure GeneralLedger entity (Chart of Accounts)
            modelBuilder.Entity<GeneralLedger>(entity =>
            {
                entity.HasKey(e => e.LedgerId);
                entity.HasIndex(e => e.AccountCode).IsUnique();
                entity.HasIndex(e => e.AccountType);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure GeneralLedgerTransaction entity
            modelBuilder.Entity<GeneralLedgerTransaction>(entity =>
            {
                entity.HasKey(e => e.GLTransactionId);
                entity.HasIndex(e => e.AccountCode);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.JournalLineId);

                entity.HasOne(e => e.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(e => e.AccountCode)
                    .HasPrincipalKey(a => a.AccountCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.JournalLine)
                    .WithMany()
                    .HasForeignKey(e => e.JournalLineId)
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

                // Ignore NotMapped properties
                entity.Ignore(e => e.Status);
                entity.Ignore(e => e.Notes);
                entity.Ignore(e => e.TotalAssets);
                entity.Ignore(e => e.TotalLiabilities);
                entity.Ignore(e => e.TotalEquity);
                entity.Ignore(e => e.TotalRevenue);
                entity.Ignore(e => e.TotalExpenses);
                entity.Ignore(e => e.NetIncome);

                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}

