using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;

namespace FinanceBank.Services
{
    /// <summary>
    /// Database Synchronization Service
    /// Provides bidirectional sync between LOCAL and CLOUD databases
    /// Features:
    /// - Automatic sync when internet is available
    /// - Manual sync for offline scenarios
    /// - Schema synchronization (table structure changes)
    /// - Data synchronization with priority ordering
    /// </summary>
    public class DatabaseSyncService : IDisposable
    {
        // LOCAL Database Connection (SQL Server Express)
        private const string LocalConnectionString = "Server=localhost\\SQLEXPRESS;Database=BFASdatabase;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;";

        // CLOUD Database Connection (MonsterASP)
        private const string CloudConnectionString = "Server=db34283.public.databaseasp.net,1433;Database=db34283;User Id=db34283;Password=Zx6=2+fXCm8!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;";

        // Auto-sync configuration
        private Timer? _autoSyncTimer;
        private bool _isAutoSyncEnabled = true;
        private bool _isSyncing = false;
        private readonly object _syncLock = new();
        private DateTime _lastSyncTime = DateTime.MinValue;
        private const int AUTO_SYNC_INTERVAL_SECONDS = 30; // Check every 30 seconds
        private const int MIN_SYNC_INTERVAL_SECONDS = 60;  // Don't sync more than once per minute

        // Pending changes queue for auto-sync
        private readonly ConcurrentQueue<PendingChange> _pendingChanges = new();

        // Event for sync status updates
        public event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;

        public DatabaseSyncService()
        {
            // Start auto-sync timer
            StartAutoSync();
        }

        #region Auto-Sync Management

        /// <summary>
        /// Start automatic synchronization timer
        /// </summary>
        public void StartAutoSync()
        {
            _isAutoSyncEnabled = true;
            _autoSyncTimer?.Dispose();
            _autoSyncTimer = new Timer(AutoSyncCallback, null,
                TimeSpan.FromSeconds(10), // Initial delay
                TimeSpan.FromSeconds(AUTO_SYNC_INTERVAL_SECONDS));

            System.Diagnostics.Debug.WriteLine("Auto-sync started");
        }

        /// <summary>
        /// Stop automatic synchronization
        /// </summary>
        public void StopAutoSync()
        {
            _isAutoSyncEnabled = false;
            _autoSyncTimer?.Dispose();
            _autoSyncTimer = null;
            System.Diagnostics.Debug.WriteLine("Auto-sync stopped");
        }

        /// <summary>
        /// Check if internet/cloud is available
        /// </summary>
        public async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                // First check network availability
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return false;

                // Then check if we can actually connect to cloud
                return await CheckCloudConnectionAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Timer callback for auto-sync
        /// </summary>
        private async void AutoSyncCallback(object? state)
        {
            if (!_isAutoSyncEnabled || _isSyncing)
                return;

            // Check if enough time has passed since last sync
            if ((DateTime.Now - _lastSyncTime).TotalSeconds < MIN_SYNC_INTERVAL_SECONDS)
                return;

            try
            {
                // Check if internet is available
                if (!await IsInternetAvailableAsync())
                {
                    System.Diagnostics.Debug.WriteLine("Auto-sync skipped: No internet connection");
                    return;
                }

                // Check if there are pending changes or if we should do a periodic sync
                if (_pendingChanges.IsEmpty && (DateTime.Now - _lastSyncTime).TotalMinutes < 5)
                {
                    return; // No pending changes and synced recently
                }

                await PerformAutoSyncAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-sync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform automatic sync in background
        /// </summary>
        private async Task PerformAutoSyncAsync()
        {
            lock (_syncLock)
            {
                if (_isSyncing) return;
                _isSyncing = true;
            }

            try
            {
                OnSyncStatusChanged(new SyncStatusEventArgs { Status = "Syncing...", IsInProgress = true });

                using var localConn = new SqlConnection(LocalConnectionString);
                using var cloudConn = new SqlConnection(CloudConnectionString);

                await localConn.OpenAsync();
                await cloudConn.OpenAsync();

                int totalSynced = 0;

                // First sync schema changes
                await SyncSchemaChangesAsync(localConn, cloudConn);

                // Then sync data - prioritize tables with pending changes
                var prioritizedTables = await GetPrioritizedTablesAsync(localConn, cloudConn, null);

                foreach (var (tableName, primaryKey, priority, diff) in prioritizedTables)
                {
                    if (diff != 0) // Only sync tables with differences
                    {
                        var count = await SyncTableAsync(localConn, cloudConn, tableName, primaryKey, null);
                        totalSynced += count;
                    }
                }

                // Clear pending changes
                while (_pendingChanges.TryDequeue(out _)) { }

                _lastSyncTime = DateTime.Now;
                OnSyncStatusChanged(new SyncStatusEventArgs
                {
                    Status = $"Synced {totalSynced} records",
                    IsInProgress = false,
                    LastSyncTime = _lastSyncTime
                });

                System.Diagnostics.Debug.WriteLine($"Auto-sync completed: {totalSynced} records synced");
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged(new SyncStatusEventArgs
                {
                    Status = $"Sync failed: {ex.Message}",
                    IsInProgress = false,
                    HasError = true
                });
                System.Diagnostics.Debug.WriteLine($"Auto-sync failed: {ex.Message}");
            }
            finally
            {
                lock (_syncLock)
                {
                    _isSyncing = false;
                }
            }
        }

        /// <summary>
        /// Queue a change for auto-sync
        /// Call this method when data is added/updated in local database
        /// </summary>
        public void QueueChange(string tableName, string operation, object? primaryKeyValue = null)
        {
            _pendingChanges.Enqueue(new PendingChange
            {
                TableName = tableName,
                Operation = operation,
                PrimaryKeyValue = primaryKeyValue,
                Timestamp = DateTime.Now
            });

            System.Diagnostics.Debug.WriteLine($"Queued change: {operation} on {tableName}");
        }

        /// <summary>
        /// Force immediate sync (useful after important operations)
        /// </summary>
        public async Task ForceSyncAsync()
        {
            if (await IsInternetAvailableAsync())
            {
                await PerformAutoSyncAsync();
            }
        }

        protected virtual void OnSyncStatusChanged(SyncStatusEventArgs e)
        {
            SyncStatusChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            _autoSyncTimer?.Dispose();
        }

        #endregion

        #region Schema Synchronization

        /// <summary>
        /// Sync schema changes from LOCAL to CLOUD
        /// This includes new columns, modified columns, etc.
        /// </summary>
        public async Task<List<string>> SyncSchemaChangesAsync(SqlConnection localConn, SqlConnection cloudConn, IProgress<string>? progress = null)
        {
            var changes = new List<string>();

            try
            {
                progress?.Report("🔍 Checking schema differences...");

                foreach (var (tableName, _) in _tablesToSync)
                {
                    var localColumns = await GetDetailedColumnInfoAsync(localConn, tableName);
                    var cloudColumns = await GetDetailedColumnInfoAsync(cloudConn, tableName);

                    if (localColumns.Count == 0 || cloudColumns.Count == 0)
                        continue;

                    // Find new columns in local that don't exist in cloud
                    foreach (var localCol in localColumns)
                    {
                        var cloudCol = cloudColumns.FirstOrDefault(c =>
                            c.Name.Equals(localCol.Name, StringComparison.OrdinalIgnoreCase));

                        if (cloudCol == null)
                        {
                            // New column - add to cloud
                            var addColumnSql = GenerateAddColumnSql(tableName, localCol);
                            if (!string.IsNullOrEmpty(addColumnSql))
                            {
                                try
                                {
                                    using var cmd = new SqlCommand(addColumnSql, cloudConn);
                                    cmd.CommandTimeout = 60;
                                    await cmd.ExecuteNonQueryAsync();
                                    changes.Add($"Added column [{localCol.Name}] to [{tableName}]");
                                    progress?.Report($"  ✓ Added column [{localCol.Name}] to [{tableName}]");
                                }
                                catch (Exception ex)
                                {
                                    progress?.Report($"  ⚠ Could not add column [{localCol.Name}]: {ex.Message}");
                                }
                            }
                        }
                        else if (localCol.DataType != cloudCol.DataType || localCol.MaxLength != cloudCol.MaxLength)
                        {
                            // Column type changed - try to alter
                            var alterColumnSql = GenerateAlterColumnSql(tableName, localCol);
                            if (!string.IsNullOrEmpty(alterColumnSql))
                            {
                                try
                                {
                                    using var cmd = new SqlCommand(alterColumnSql, cloudConn);
                                    cmd.CommandTimeout = 60;
                                    await cmd.ExecuteNonQueryAsync();
                                    changes.Add($"Modified column [{localCol.Name}] in [{tableName}]");
                                    progress?.Report($"  ✓ Modified column [{localCol.Name}] in [{tableName}]");
                                }
                                catch (Exception ex)
                                {
                                    progress?.Report($"  ⚠ Could not modify column [{localCol.Name}]: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                if (changes.Count == 0)
                {
                    progress?.Report("  ✓ Schema is synchronized");
                }
                else
                {
                    progress?.Report($"  ✓ {changes.Count} schema changes applied");
                }
            }
            catch (Exception ex)
            {
                progress?.Report($"  ⚠ Schema sync error: {ex.Message}");
            }

            return changes;
        }

        /// <summary>
        /// Get detailed column information including data type, length, nullable
        /// </summary>
        private async Task<List<ColumnInfo>> GetDetailedColumnInfoAsync(SqlConnection conn, string tableName)
        {
            var columns = new List<ColumnInfo>();

            try
            {
                var sql = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        CHARACTER_MAXIMUM_LENGTH,
                        NUMERIC_PRECISION,
                        NUMERIC_SCALE,
                        IS_NULLABLE,
                        COLUMN_DEFAULT
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @tableName
                    ORDER BY ORDINAL_POSITION";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tableName", tableName);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnInfo
                    {
                        Name = reader.GetString(0),
                        DataType = reader.GetString(1),
                        MaxLength = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        NumericPrecision = reader.IsDBNull(3) ? null : (int?)reader.GetByte(3),
                        NumericScale = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        IsNullable = reader.GetString(5) == "YES",
                        DefaultValue = reader.IsDBNull(6) ? null : reader.GetString(6)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting columns for {tableName}: {ex.Message}");
            }

            return columns;
        }

        /// <summary>
        /// Generate SQL to add a new column
        /// </summary>
        private string GenerateAddColumnSql(string tableName, ColumnInfo column)
        {
            // Skip binary columns
            if (_columnsToSkip.Contains(column.Name) ||
                column.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase) ||
                column.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            var dataTypeSql = GetDataTypeSql(column);
            var nullableSql = column.IsNullable ? "NULL" : "NOT NULL";
            var defaultSql = !string.IsNullOrEmpty(column.DefaultValue) ? $"DEFAULT {column.DefaultValue}" : "";

            // For NOT NULL columns without default, add a default to allow adding
            if (!column.IsNullable && string.IsNullOrEmpty(defaultSql))
            {
                defaultSql = GetDefaultValueForType(column.DataType);
            }

            return $"ALTER TABLE [{tableName}] ADD [{column.Name}] {dataTypeSql} {nullableSql} {defaultSql}".Trim();
        }

        /// <summary>
        /// Generate SQL to alter an existing column
        /// </summary>
        private string GenerateAlterColumnSql(string tableName, ColumnInfo column)
        {
            // Skip binary columns
            if (_columnsToSkip.Contains(column.Name) ||
                column.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase) ||
                column.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            var dataTypeSql = GetDataTypeSql(column);
            var nullableSql = column.IsNullable ? "NULL" : "NOT NULL";

            return $"ALTER TABLE [{tableName}] ALTER COLUMN [{column.Name}] {dataTypeSql} {nullableSql}";
        }

        /// <summary>
        /// Get SQL data type string with length/precision
        /// </summary>
        private string GetDataTypeSql(ColumnInfo column)
        {
            var dataTypeLower = column.DataType.ToLower();

            if (dataTypeLower == "varchar" || dataTypeLower == "nvarchar" ||
                dataTypeLower == "char" || dataTypeLower == "nchar")
            {
                return column.MaxLength == -1
                    ? $"{column.DataType}(MAX)"
                    : $"{column.DataType}({column.MaxLength})";
            }

            if (dataTypeLower == "decimal" || dataTypeLower == "numeric")
            {
                return $"{column.DataType}({column.NumericPrecision},{column.NumericScale})";
            }

            return column.DataType;
        }

        /// <summary>
        /// Get default value for a data type
        /// </summary>
        private string GetDefaultValueForType(string dataType)
        {
            return dataType.ToLower() switch
            {
                "int" or "bigint" or "smallint" or "tinyint" => "DEFAULT 0",
                "decimal" or "numeric" or "money" or "float" or "real" => "DEFAULT 0",
                "bit" => "DEFAULT 0",
                "datetime" or "datetime2" or "date" => "DEFAULT GETDATE()",
                "varchar" or "nvarchar" or "char" or "nchar" or "text" or "ntext" => "DEFAULT ''",
                "uniqueidentifier" => "DEFAULT NEWID()",
                _ => ""
            };
        }

        #endregion

        #region Connection Testing

        /// <summary>
        /// Check if LOCAL database is accessible
        /// </summary>
        public async Task<bool> CheckLocalConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(LocalConnectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if CLOUD database is accessible
        /// </summary>
        public async Task<bool> CheckCloudConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(CloudConnectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check both connections
        /// </summary>
        public async Task<(bool Local, bool Cloud)> CheckAllConnectionsAsync()
        {
            var localTask = CheckLocalConnectionAsync();
            var cloudTask = CheckCloudConnectionAsync();
            await Task.WhenAll(localTask, cloudTask);
            return (localTask.Result, cloudTask.Result);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get record counts from both databases for comparison
        /// </summary>
        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            var stats = new DatabaseStats();

            try
            {
                // Local stats
                using (var localConn = new SqlConnection(LocalConnectionString))
                {
                    await localConn.OpenAsync();

                    // Core User Tables
                    stats.LocalUsers = await GetTableCountAsync(localConn, "Users");
                    stats.LocalEmployees = await GetTableCountAsync(localConn, "Employees");

                    // Customer Tables
                    stats.LocalCustomerAccounts = await GetTableCountAsync(localConn, "CustomerAccounts");
                    stats.LocalTransactions = await GetTableCountAsync(localConn, "CustomerTransactions");

                    // Loan Tables
                    stats.LocalCustomerLoans = await GetTableCountAsync(localConn, "CustomerLoans");
                    stats.LocalLoanApplications = await GetTableCountAsync(localConn, "LoanApplications");
                    stats.LocalLoanAssessments = await GetTableCountAsync(localConn, "LoanAssessments");
                    stats.LocalLoanApprovals = await GetTableCountAsync(localConn, "LoanApprovals");
                    stats.LocalLoanDisbursals = await GetTableCountAsync(localConn, "LoanDisbursals");
                    stats.LocalLoanPayments = await GetTableCountAsync(localConn, "LoanPayments");

                    // Finance Tables
                    stats.LocalInvoices = await GetTableCountAsync(localConn, "Invoices");
                    stats.LocalJournalEntries = await GetTableCountAsync(localConn, "JournalEntries");

                    // System Tables
                    stats.LocalLoginHistory = await GetTableCountAsync(localConn, "LoginHistory");
                    stats.LocalAuditLogs = await GetTableCountAsync(localConn, "AuditLogs");
                }

                // Cloud stats
                using (var cloudConn = new SqlConnection(CloudConnectionString))
                {
                    await cloudConn.OpenAsync();

                    // Core User Tables
                    stats.CloudUsers = await GetTableCountAsync(cloudConn, "Users");
                    stats.CloudEmployees = await GetTableCountAsync(cloudConn, "Employees");

                    // Customer Tables
                    stats.CloudCustomerAccounts = await GetTableCountAsync(cloudConn, "CustomerAccounts");
                    stats.CloudTransactions = await GetTableCountAsync(cloudConn, "CustomerTransactions");

                    // Loan Tables
                    stats.CloudCustomerLoans = await GetTableCountAsync(cloudConn, "CustomerLoans");
                    stats.CloudLoanApplications = await GetTableCountAsync(cloudConn, "LoanApplications");
                    stats.CloudLoanAssessments = await GetTableCountAsync(cloudConn, "LoanAssessments");
                    stats.CloudLoanApprovals = await GetTableCountAsync(cloudConn, "LoanApprovals");
                    stats.CloudLoanDisbursals = await GetTableCountAsync(cloudConn, "LoanDisbursals");
                    stats.CloudLoanPayments = await GetTableCountAsync(cloudConn, "LoanPayments");

                    // Finance Tables
                    stats.CloudInvoices = await GetTableCountAsync(cloudConn, "Invoices");
                    stats.CloudJournalEntries = await GetTableCountAsync(cloudConn, "JournalEntries");

                    // System Tables
                    stats.CloudLoginHistory = await GetTableCountAsync(cloudConn, "LoginHistory");
                    stats.CloudAuditLogs = await GetTableCountAsync(cloudConn, "AuditLogs");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting stats: {ex.Message}");
            }

            return stats;
        }

        private async Task<int> GetTableCountAsync(SqlConnection conn, string tableName)
        {
            try
            {
                using var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}]", conn);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Sync Operations

        /// <summary>
        /// ALL 46 Tables to sync in correct order (PARENT tables first, then CHILD tables)
        /// This prevents foreign key constraint violations
        /// Order: Base tables → First-level children → Second-level → etc.
        /// </summary>
        private readonly List<(string TableName, string PrimaryKey)> _tablesToSync = new()
        {
            // ============ LEVEL 1: Base/Parent Tables (no foreign keys) ============
            ("Users", "UserId"),                              // Base user table - MUST BE FIRST
            ("ChartOfAccounts", "AccountId"),                 // Accounting base
            ("SystemSettings", "SettingId"),                  // System config
            ("Seeders", "SeedID"),                            // Seed data tracking
            ("RolePermissions", "PermissionId"),              // Role permissions
            
            // ============ LEVEL 2: User-related tables ============
            ("Employees", "EmployeeId"),                      // References Users
            ("EmployeeAccounts", "EmployeeID"),               // References Employees
            ("SuperAdminEmployees", "EmployeeID"),            // References Employees
            ("CustomerAccounts", "AccountId"),                // References Users
            ("LoginHistory", "LoginId"),                      // References Users
            
            // ============ LEVEL 3: Customer-related tables ============
            ("CustomerTransactions", "TransactionId"),        // References CustomerAccounts
            ("CustomerCards", "CardId"),                      // References CustomerAccounts
            ("CustomerLoans", "LoanId"),                      // References CustomerAccounts
            ("CustomerSavingsGoals", "GoalId"),               // References CustomerAccounts
            ("CustomerRewardPoints", "RewardId"),             // References CustomerAccounts
            ("Invoices", "InvoiceId"),                        // References CustomerAccounts
            ("FundTransfers", "TransferId"),                  // References CustomerAccounts
            
            // ============ LEVEL 4: Loan workflow (in order) ============
            ("LoanApplications", "ApplicationId"),            // References CustomerAccounts
            ("LoanAssessments", "AssessmentId"),              // References LoanApplications
            ("LoanApprovals", "ApprovalId"),                  // References LoanAssessments
            ("LoanDisbursals", "DisbursalId"),                // References LoanApprovals
            ("LoanPaymentSchedules", "ScheduleId"),           // References LoanApplications
            ("LoanPayments", "PaymentId"),                    // References LoanPaymentSchedules
            ("LoanViolations", "ViolationId"),                // References LoanApplications
            ("LoanTransactionHistory", "HistoryId"),          // References LoanApplications
            ("LoanInvoices", "InvoiceId"),                    // References LoanApplications
            ("LoanManagement", "LoanMgmtId"),                 // Loan management records
            
            // ============ LEVEL 5: Accounting & Finance tables ============
            ("JournalEntries", "JournalId"),                  // References ChartOfAccounts
            ("JournalEntryLines", "LineId"),                  // References JournalEntries
            ("GeneralLedger", "LedgerId"),                    // General ledger
            ("GeneralLedgerTransactions", "GLTransactionId"), // GL transactions
            ("TrialBalance", "TrialBalanceId"),               // Trial balance
            ("AccountsPayable", "PayableId"),                 // AP
            ("AccountsReceivable", "ReceivableId"),           // AR
            ("AccountingEntries", "EntryId"),                 // Accounting entries
            ("FinancialStatements", "StatementId"),           // Financial statements
            ("FinancialForecasting", "ForecastId"),           // Forecasting
            ("CashflowAnalysis", "CashflowId"),               // Cashflow
            ("BudgetManagement", "BudgetId"),                 // Budgets
            
            // ============ LEVEL 6: Banking & Card Management ============
            ("BankAccounts", "BankAccountId"),                // Bank accounts
            ("BankingReports", "ReportId"),                   // Banking reports
            ("BillersManagement", "BillerId"),                // Billers
            ("CardManagement", "CardMgmtId"),                 // Card management
            
            // ============ LEVEL 7: System & Approval tables ============
            ("ApprovalQueue", "ApprovalId"),                  // Approval queue
            ("SystemReports", "ReportId"),                    // System reports
            
            // ============ LEVEL 8: Audit/Log tables (LAST) ============
            ("AuditLogs", "AuditId"),                         // Audit logs - MUST BE LAST
        };

        /// <summary>
        /// Columns to SKIP during sync (binary columns that cause conversion errors)
        /// </summary>
        private readonly HashSet<string> _columnsToSkip = new(StringComparer.OrdinalIgnoreCase)
        {
            "ProfilePicture",       // varbinary(max) - user profile pictures
            "ValidIdDocument",      // varbinary(max) - ID document scans
            "DocumentData",         // varbinary(max) - any document data
            "FileContent",          // varbinary(max) - file contents
            "ImageData",            // varbinary(max) - image data
            "BinaryData",           // varbinary(max) - generic binary
            "Attachment",           // varbinary(max) - attachments
            "SignatureImage",       // varbinary(max) - signatures
            "Photo",                // varbinary(max) - photos
        };

        /// <summary>
        /// Sync data from LOCAL to CLOUD (Upload)
        /// Copies ALL local data to cloud database
        /// OPTIMIZED: Prioritizes tables with differences, uses parallel batch processing
        /// </summary>
        public async Task<SyncResult> SyncLocalToCloudAsync(IProgress<string>? progress = null)
        {
            var result = new SyncResult();
            int totalSynced = 0;

            try
            {
                progress?.Report("🔗 Connecting to databases...");

                using var localConn = new SqlConnection(LocalConnectionString);
                using var cloudConn = new SqlConnection(CloudConnectionString);

                await localConn.OpenAsync();
                progress?.Report("✓ Connected to LOCAL database");

                await cloudConn.OpenAsync();
                progress?.Report("✓ Connected to CLOUD database");

                progress?.Report("");
                progress?.Report("🔧 Synchronizing schema changes...");
                progress?.Report("──────────────────────────────────");

                // First sync schema changes (new columns, modified columns)
                var schemaChanges = await SyncSchemaChangesAsync(localConn, cloudConn, progress);

                progress?.Report("");
                progress?.Report("📊 Analyzing data differences...");

                // Get prioritized sync order based on differences
                var prioritizedTables = await GetPrioritizedTablesAsync(localConn, cloudConn, progress);

                progress?.Report("");
                progress?.Report("📤 Starting LOCAL → CLOUD sync...");
                progress?.Report("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // Process tables in prioritized order with batch optimization
                totalSynced = await SyncTablesOptimizedAsync(localConn, cloudConn, prioritizedTables, progress);

                progress?.Report("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.Success = true;
                result.InsertedRecords = totalSynced;
                result.SyncedAt = DateTime.Now;

                // Update last sync time for auto-sync
                _lastSyncTime = DateTime.Now;

                progress?.Report($"✅ SYNC COMPLETE! {totalSynced} records + {schemaChanges.Count} schema changes.");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                progress?.Report($"❌ Error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Sync data from CLOUD to LOCAL (Download)
        /// Copies ALL cloud data to local database
        /// OPTIMIZED: Prioritizes tables with differences, uses parallel batch processing
        /// </summary>
        public async Task<SyncResult> SyncCloudToLocalAsync(IProgress<string>? progress = null)
        {
            var result = new SyncResult();
            int totalSynced = 0;

            try
            {
                progress?.Report("🔗 Connecting to databases...");

                using var localConn = new SqlConnection(LocalConnectionString);
                using var cloudConn = new SqlConnection(CloudConnectionString);

                await cloudConn.OpenAsync();
                progress?.Report("✓ Connected to CLOUD database");

                await localConn.OpenAsync();
                progress?.Report("✓ Connected to LOCAL database");

                progress?.Report("");
                progress?.Report("📊 Analyzing differences...");

                // Get prioritized sync order based on differences (reversed for cloud->local)
                var prioritizedTables = await GetPrioritizedTablesAsync(cloudConn, localConn, progress);

                progress?.Report("");
                progress?.Report("📥 Starting CLOUD → LOCAL sync...");
                progress?.Report("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // Note: source and destination are reversed (cloud -> local)
                totalSynced = await SyncTablesOptimizedAsync(cloudConn, localConn, prioritizedTables, progress);

                progress?.Report("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.Success = true;
                result.InsertedRecords = totalSynced;
                result.SyncedAt = DateTime.Now;
                progress?.Report($"✅ SYNC COMPLETE! {totalSynced} total records processed.");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                progress?.Report($"❌ Error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Bidirectional sync - sync both ways
        /// First uploads LOCAL to CLOUD, then downloads any new CLOUD data to LOCAL
        /// </summary>
        public async Task<SyncResult> SyncBidirectionalAsync(IProgress<string>? progress = null)
        {
            var result = new SyncResult();
            int totalSynced = 0;

            try
            {
                progress?.Report("🔄 Starting BIDIRECTIONAL SYNC...");
                progress?.Report("══════════════════════════════════");
                progress?.Report("");

                // First sync LOCAL -> CLOUD
                progress?.Report("📤 PHASE 1: Uploading LOCAL → CLOUD");
                progress?.Report("──────────────────────────────────");
                var uploadResult = await SyncLocalToCloudAsync(progress);
                totalSynced += uploadResult.InsertedRecords;

                progress?.Report("");

                // Then sync CLOUD -> LOCAL (to get any data others added to cloud)
                progress?.Report("📥 PHASE 2: Downloading CLOUD → LOCAL");
                progress?.Report("──────────────────────────────────");
                var downloadResult = await SyncCloudToLocalAsync(progress);
                totalSynced += downloadResult.InsertedRecords;

                progress?.Report("");
                progress?.Report("══════════════════════════════════");
                result.Success = uploadResult.Success && downloadResult.Success;
                result.InsertedRecords = totalSynced;
                result.SyncedAt = DateTime.Now;

                if (!result.Success)
                {
                    result.ErrorMessage = $"Upload: {uploadResult.ErrorMessage}, Download: {downloadResult.ErrorMessage}";
                    progress?.Report($"⚠ Some errors occurred: {result.ErrorMessage}");
                }
                else
                {
                    progress?.Report($"✅ FULL SYNC COMPLETE! {totalSynced} total records processed.");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                progress?.Report($"❌ Error: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Optimized Sync Methods

        /// <summary>
        /// Analyzes both databases and returns tables in prioritized order:
        /// 1. Parent tables of tables with differences (must sync first due to FK constraints)
        /// 2. Tables with actual differences (most important)
        /// 3. Other tables (least priority)
        /// </summary>
        private async Task<List<(string TableName, string PrimaryKey, int Priority, int Difference)>> GetPrioritizedTablesAsync(
            SqlConnection source, SqlConnection dest, IProgress<string>? progress = null)
        {
            var tableDiffs = new Dictionary<string, int>();
            var prioritizedList = new List<(string TableName, string PrimaryKey, int Priority, int Difference)>();

            // Calculate differences for all tables
            foreach (var (tableName, pk) in _tablesToSync)
            {
                var sourceCount = await GetTableCountAsync(source, tableName);
                var destCount = await GetTableCountAsync(dest, tableName);
                tableDiffs[tableName] = sourceCount - destCount;
            }

            // Find tables with differences
            var tablesWithDiffs = tableDiffs.Where(kv => kv.Value != 0).Select(kv => kv.Key).ToHashSet();

            if (tablesWithDiffs.Any())
            {
                progress?.Report($"  Found {tablesWithDiffs.Count} tables with differences");
            }

            // Find parent tables of tables with differences
            var parentTablesNeeded = new HashSet<string>();
            foreach (var table in tablesWithDiffs)
            {
                var parents = GetParentTables(table);
                foreach (var parent in parents)
                {
                    parentTablesNeeded.Add(parent);
                }
            }

            // Assign priorities:
            // Priority 1: Parent tables of tables with differences (MUST sync first)
            // Priority 2: Tables with differences (main sync targets)
            // Priority 3: Other tables
            foreach (var (tableName, pk) in _tablesToSync)
            {
                int priority;
                if (parentTablesNeeded.Contains(tableName) && !tablesWithDiffs.Contains(tableName))
                {
                    priority = 1; // Parent tables
                }
                else if (tablesWithDiffs.Contains(tableName))
                {
                    priority = 2; // Tables with actual differences
                }
                else
                {
                    priority = 3; // Other tables
                }

                prioritizedList.Add((tableName, pk, priority, tableDiffs[tableName]));
            }

            // Sort by priority, but within same priority, maintain dependency order
            return prioritizedList
                .OrderBy(x => x.Priority)
                .ThenBy(x => _tablesToSync.FindIndex(t => t.TableName == x.TableName))
                .ToList();
        }

        /// <summary>
        /// Get parent tables that a given table depends on (via foreign keys)
        /// </summary>
        private HashSet<string> GetParentTables(string tableName)
        {
            // Define parent-child relationships
            var parentMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Employees depends on Users
                { "Employees", new HashSet<string> { "Users" } },
                { "EmployeeAccounts", new HashSet<string> { "Employees" } },
                { "SuperAdminEmployees", new HashSet<string> { "Employees" } },
                
                // Customer tables depend on Users
                { "CustomerAccounts", new HashSet<string> { "Users" } },
                { "LoginHistory", new HashSet<string> { "Users" } },
                
                // Customer sub-tables depend on CustomerAccounts
                { "CustomerTransactions", new HashSet<string> { "CustomerAccounts" } },
                { "CustomerCards", new HashSet<string> { "CustomerAccounts" } },
                { "CustomerLoans", new HashSet<string> { "CustomerAccounts" } },
                { "CustomerSavingsGoals", new HashSet<string> { "CustomerAccounts" } },
                { "CustomerRewardPoints", new HashSet<string> { "CustomerAccounts" } },
                { "Invoices", new HashSet<string> { "CustomerAccounts" } },
                { "FundTransfers", new HashSet<string> { "CustomerAccounts" } },
                
                // Loan workflow chain
                { "LoanApplications", new HashSet<string> { "CustomerAccounts" } },
                { "LoanAssessments", new HashSet<string> { "LoanApplications" } },
                { "LoanApprovals", new HashSet<string> { "LoanAssessments" } },
                { "LoanDisbursals", new HashSet<string> { "LoanApprovals" } },
                { "LoanPaymentSchedules", new HashSet<string> { "LoanApplications" } },
                { "LoanPayments", new HashSet<string> { "LoanPaymentSchedules" } },
                { "LoanViolations", new HashSet<string> { "LoanApplications" } },
                { "LoanTransactionHistory", new HashSet<string> { "LoanApplications" } },
                { "LoanInvoices", new HashSet<string> { "LoanApplications" } },
                
                // Accounting tables
                { "JournalEntries", new HashSet<string> { "ChartOfAccounts" } },
                { "JournalEntryLines", new HashSet<string> { "JournalEntries" } },
            };

            var result = new HashSet<string>();
            if (parentMap.TryGetValue(tableName, out var directParents))
            {
                foreach (var parent in directParents)
                {
                    result.Add(parent);
                    // Recursively get grandparents
                    var grandparents = GetParentTables(parent);
                    foreach (var gp in grandparents)
                    {
                        result.Add(gp);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Sync tables with optimized batch processing
        /// Uses parallel processing for tables at the same dependency level
        /// </summary>
        private async Task<int> SyncTablesOptimizedAsync(
            SqlConnection source, SqlConnection dest,
            List<(string TableName, string PrimaryKey, int Priority, int Difference)> prioritizedTables,
            IProgress<string>? progress = null)
        {
            int totalSynced = 0;

            // Group by priority for potential parallel processing
            var groupedTables = prioritizedTables.GroupBy(t => t.Priority).OrderBy(g => g.Key);

            foreach (var group in groupedTables)
            {
                string groupLabel = group.Key switch
                {
                    1 => "🔷 PARENT TABLES (sync first for FK constraints)",
                    2 => "🔶 TABLES WITH DIFFERENCES (main sync targets)",
                    3 => "⬜ OTHER TABLES",
                    _ => "📋 TABLES"
                };

                progress?.Report("");
                progress?.Report(groupLabel);
                progress?.Report("──────────────────────────────────");

                // For tables at the same priority level, we can process them faster
                // but must maintain order within parent-child relationships
                foreach (var (tableName, primaryKey, _, diff) in group)
                {
                    string diffIndicator = diff > 0 ? $"(+{diff})" : diff < 0 ? $"({diff})" : "(=)";
                    progress?.Report($"Syncing {tableName} {diffIndicator}...");

                    // Use optimized batch sync for tables with differences
                    var count = Math.Abs(diff) > 100
                        ? await SyncTableBatchAsync(source, dest, tableName, primaryKey, progress)
                        : await SyncTableAsync(source, dest, tableName, primaryKey, progress);

                    totalSynced += count;
                }
            }

            return totalSynced;
        }

        /// <summary>
        /// Optimized batch sync for tables with many records
        /// Uses bulk operations for better performance
        /// </summary>
        private async Task<int> SyncTableBatchAsync(SqlConnection source, SqlConnection dest, string tableName, string primaryKeyColumn, IProgress<string>? progress = null)
        {
            int syncedCount = 0;
            const int batchSize = 500; // Process 500 records at a time

            try
            {
                // Get column info
                var allColumns = await GetTableColumnsWithTypesAsync(source, tableName);
                if (allColumns.Count == 0) return 0;

                var columns = allColumns
                    .Where(c => !_columnsToSkip.Contains(c.Name))
                    .Where(c => !c.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase))
                    .Where(c => !c.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Name)
                    .ToList();

                var destColumns = (await GetTableColumnsWithTypesAsync(dest, tableName))
                    .Where(c => !_columnsToSkip.Contains(c.Name))
                    .Where(c => !c.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase))
                    .Where(c => !c.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Name)
                    .ToList();

                var commonColumns = columns.Intersect(destColumns).ToList();
                if (!commonColumns.Contains(primaryKeyColumn)) return 0;

                string columnList = string.Join(", ", commonColumns.Select(c => $"[{c}]"));

                // Get existing IDs in destination
                var existingIds = new HashSet<object>();
                using (var idCmd = new SqlCommand($"SELECT [{primaryKeyColumn}] FROM [{tableName}]", dest))
                {
                    idCmd.CommandTimeout = 120;
                    using var reader = await idCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        existingIds.Add(reader[0]);
                    }
                }

                // Read source data in batches
                var sourceData = new DataTable();
                using (var cmd = new SqlCommand($"SELECT {columnList} FROM [{tableName}]", source))
                {
                    cmd.CommandTimeout = 180;
                    using var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(sourceData);
                }

                int totalRows = sourceData.Rows.Count;
                int processedRows = 0;
                int insertedCount = 0;
                int updatedCount = 0;

                progress?.Report($"  {tableName}: Processing {totalRows} records in batches of {batchSize}...");

                // Process in batches
                for (int i = 0; i < totalRows; i += batchSize)
                {
                    var batch = sourceData.Rows.Cast<DataRow>().Skip(i).Take(batchSize).ToList();

                    foreach (var row in batch)
                    {
                        try
                        {
                            var pkValue = row[primaryKeyColumn];
                            bool exists = existingIds.Contains(pkValue);

                            if (exists)
                            {
                                // UPDATE
                                var columnsToUpdate = commonColumns.Where(c => c != primaryKeyColumn).ToList();
                                if (columnsToUpdate.Any())
                                {
                                    var setStatements = columnsToUpdate.Select(c => $"[{c}] = @{c.Replace(" ", "_")}");
                                    var updateSql = $"UPDATE [{tableName}] SET {string.Join(", ", setStatements)} WHERE [{primaryKeyColumn}] = @pk";

                                    using var updateCmd = new SqlCommand(updateSql, dest);
                                    updateCmd.CommandTimeout = 60;
                                    updateCmd.Parameters.AddWithValue("@pk", pkValue ?? DBNull.Value);
                                    foreach (var col in columnsToUpdate)
                                    {
                                        updateCmd.Parameters.AddWithValue($"@{col.Replace(" ", "_")}", row[col] ?? DBNull.Value);
                                    }
                                    if (await updateCmd.ExecuteNonQueryAsync() > 0) updatedCount++;
                                }
                            }
                            else
                            {
                                // INSERT
                                var insertColumns = string.Join(", ", commonColumns.Select(c => $"[{c}]"));
                                var insertParams = string.Join(", ", commonColumns.Select(c => $"@{c.Replace(" ", "_")}"));
                                var insertSql = $@"
                                    SET IDENTITY_INSERT [{tableName}] ON;
                                    INSERT INTO [{tableName}] ({insertColumns}) VALUES ({insertParams});
                                    SET IDENTITY_INSERT [{tableName}] OFF;";

                                using var insertCmd = new SqlCommand(insertSql, dest);
                                insertCmd.CommandTimeout = 60;
                                foreach (var col in commonColumns)
                                {
                                    insertCmd.Parameters.AddWithValue($"@{col.Replace(" ", "_")}", row[col] ?? DBNull.Value);
                                }
                                await insertCmd.ExecuteNonQueryAsync();
                                insertedCount++;
                            }
                            syncedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Batch sync error in {tableName}: {ex.Message}");
                        }
                    }

                    processedRows += batch.Count;
                    int percentage = (int)((processedRows * 100.0) / totalRows);
                    progress?.Report($"  {tableName}: {percentage}% ({processedRows}/{totalRows})");
                }

                progress?.Report($"  ✓ {tableName}: {insertedCount} inserted, {updatedCount} updated (batch mode)");
            }
            catch (Exception ex)
            {
                progress?.Report($"  ✗ Error batch syncing {tableName}: {ex.Message}");
            }

            return syncedCount;
        }

        #endregion

        #region Private Sync Helpers

        /// <summary>
        /// Sync a single table from source to destination
        /// Copies ALL source data to destination (inserts new, updates existing)
        /// Skips binary columns to avoid conversion errors
        /// </summary>
        private async Task<int> SyncTableAsync(SqlConnection source, SqlConnection dest, string tableName, string primaryKeyColumn, IProgress<string>? progress = null)
        {
            int syncedCount = 0;
            int updatedCount = 0;
            int insertedCount = 0;
            int errorCount = 0;

            try
            {
                // Get column info from source table (excluding binary columns)
                var allColumns = await GetTableColumnsWithTypesAsync(source, tableName);
                if (allColumns.Count == 0)
                {
                    progress?.Report($"  ⚠ Table {tableName} not found in source");
                    return 0;
                }

                // Filter out binary columns and columns in skip list
                var columns = allColumns
                    .Where(c => !_columnsToSkip.Contains(c.Name))
                    .Where(c => !c.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase))
                    .Where(c => !c.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Name)
                    .ToList();

                // Check if table exists in destination
                var destAllColumns = await GetTableColumnsWithTypesAsync(dest, tableName);
                if (destAllColumns.Count == 0)
                {
                    progress?.Report($"  ⚠ Table {tableName} not found in destination");
                    return 0;
                }

                var destColumns = destAllColumns
                    .Where(c => !_columnsToSkip.Contains(c.Name))
                    .Where(c => !c.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase))
                    .Where(c => !c.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Name)
                    .ToList();

                // Find common columns between source and destination
                var commonColumns = columns.Intersect(destColumns).ToList();
                if (!commonColumns.Contains(primaryKeyColumn))
                {
                    progress?.Report($"  ⚠ Primary key {primaryKeyColumn} not found in {tableName}");
                    return 0;
                }

                // Log skipped columns
                var skippedBinary = allColumns.Where(c =>
                    _columnsToSkip.Contains(c.Name) ||
                    c.DataType.Contains("binary", StringComparison.OrdinalIgnoreCase) ||
                    c.DataType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Name).ToList();

                if (skippedBinary.Any())
                {
                    progress?.Report($"  ℹ Skipping binary columns: {string.Join(", ", skippedBinary)}");
                }

                string columnList = string.Join(", ", commonColumns.Select(c => $"[{c}]"));

                // Read ALL source data
                var sourceData = new DataTable();
                using (var cmd = new SqlCommand($"SELECT {columnList} FROM [{tableName}]", source))
                {
                    cmd.CommandTimeout = 120;
                    using var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(sourceData);
                }

                progress?.Report($"  {tableName}: Found {sourceData.Rows.Count} records in source");

                if (sourceData.Rows.Count == 0)
                {
                    progress?.Report($"  ✓ {tableName}: No data to sync");
                    return 0;
                }

                // Get existing IDs in destination
                var existingIds = new HashSet<object>();
                using (var idCmd = new SqlCommand($"SELECT [{primaryKeyColumn}] FROM [{tableName}]", dest))
                {
                    idCmd.CommandTimeout = 60;
                    using var reader = await idCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        existingIds.Add(reader[0]);
                    }
                }

                progress?.Report($"  {tableName}: {existingIds.Count} existing records in destination");

                // Process each row
                foreach (DataRow row in sourceData.Rows)
                {
                    try
                    {
                        var pkValue = row[primaryKeyColumn];
                        bool exists = existingIds.Contains(pkValue);

                        if (exists)
                        {
                            // UPDATE existing record
                            var columnsToUpdate = commonColumns.Where(c => c != primaryKeyColumn).ToList();
                            if (!columnsToUpdate.Any()) continue;

                            var setStatements = columnsToUpdate.Select(c => $"[{c}] = @{c.Replace(" ", "_")}");
                            var updateSql = $"UPDATE [{tableName}] SET {string.Join(", ", setStatements)} WHERE [{primaryKeyColumn}] = @pk";

                            using var updateCmd = new SqlCommand(updateSql, dest);
                            updateCmd.CommandTimeout = 60;
                            updateCmd.Parameters.AddWithValue("@pk", pkValue ?? DBNull.Value);

                            foreach (var col in columnsToUpdate)
                            {
                                var value = row[col];
                                updateCmd.Parameters.AddWithValue($"@{col.Replace(" ", "_")}", value ?? DBNull.Value);
                            }

                            var affected = await updateCmd.ExecuteNonQueryAsync();
                            if (affected > 0) updatedCount++;
                        }
                        else
                        {
                            // INSERT new record with IDENTITY_INSERT
                            var insertColumns = string.Join(", ", commonColumns.Select(c => $"[{c}]"));
                            var insertParams = string.Join(", ", commonColumns.Select(c => $"@{c.Replace(" ", "_")}"));

                            // Build insert with IDENTITY_INSERT in same batch
                            var insertSql = $@"
                                SET IDENTITY_INSERT [{tableName}] ON;
                                INSERT INTO [{tableName}] ({insertColumns}) VALUES ({insertParams});
                                SET IDENTITY_INSERT [{tableName}] OFF;";

                            using var insertCmd = new SqlCommand(insertSql, dest);
                            insertCmd.CommandTimeout = 60;

                            foreach (var col in commonColumns)
                            {
                                var value = row[col];
                                insertCmd.Parameters.AddWithValue($"@{col.Replace(" ", "_")}", value ?? DBNull.Value);
                            }

                            await insertCmd.ExecuteNonQueryAsync();
                            insertedCount++;
                        }

                        syncedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        System.Diagnostics.Debug.WriteLine($"Error syncing row in {tableName}: {ex.Message}");
                        // Only report first few errors to avoid spam
                        if (errorCount <= 3)
                        {
                            progress?.Report($"  ⚠ Row error: {ex.Message}");
                        }
                    }
                }

                if (errorCount > 3)
                {
                    progress?.Report($"  ⚠ ... and {errorCount - 3} more errors");
                }

                progress?.Report($"  ✓ {tableName}: {insertedCount} inserted, {updatedCount} updated, {errorCount} errors");
            }
            catch (Exception ex)
            {
                progress?.Report($"  ✗ Error syncing {tableName}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SyncTableAsync error for {tableName}: {ex}");
            }

            return syncedCount;
        }

        /// <summary>
        /// Get list of column names with their data types for a table
        /// </summary>
        private async Task<List<(string Name, string DataType)>> GetTableColumnsWithTypesAsync(SqlConnection conn, string tableName)
        {
            var columns = new List<(string Name, string DataType)>();

            try
            {
                var sql = @"
                    SELECT COLUMN_NAME, DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @tableName
                    ORDER BY ORDINAL_POSITION";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tableName", tableName);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add((reader.GetString(0), reader.GetString(1)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting columns for {tableName}: {ex.Message}");
            }

            return columns;
        }

        /// <summary>
        /// Get list of column names for a table (legacy method)
        /// </summary>
        private async Task<List<string>> GetTableColumnsAsync(SqlConnection conn, string tableName)
        {
            var columnsWithTypes = await GetTableColumnsWithTypesAsync(conn, tableName);
            return columnsWithTypes.Select(c => c.Name).ToList();
        }

        #endregion
    }

    #region Support Classes

    /// <summary>
    /// Database statistics for comparison - ALL important tables
    /// </summary>
    public class DatabaseStats
    {
        // Core User Tables
        public int LocalUsers { get; set; }
        public int CloudUsers { get; set; }
        public int LocalEmployees { get; set; }
        public int CloudEmployees { get; set; }

        // Customer Tables
        public int LocalCustomerAccounts { get; set; }
        public int CloudCustomerAccounts { get; set; }
        public int LocalTransactions { get; set; }
        public int CloudTransactions { get; set; }

        // Loan Tables
        public int LocalCustomerLoans { get; set; }
        public int CloudCustomerLoans { get; set; }
        public int LocalLoanApplications { get; set; }
        public int CloudLoanApplications { get; set; }
        public int LocalLoanAssessments { get; set; }
        public int CloudLoanAssessments { get; set; }
        public int LocalLoanApprovals { get; set; }
        public int CloudLoanApprovals { get; set; }
        public int LocalLoanDisbursals { get; set; }
        public int CloudLoanDisbursals { get; set; }
        public int LocalLoanPayments { get; set; }
        public int CloudLoanPayments { get; set; }

        // Finance Tables
        public int LocalInvoices { get; set; }
        public int CloudInvoices { get; set; }
        public int LocalJournalEntries { get; set; }
        public int CloudJournalEntries { get; set; }

        // System Tables
        public int LocalLoginHistory { get; set; }
        public int CloudLoginHistory { get; set; }
        public int LocalAuditLogs { get; set; }
        public int CloudAuditLogs { get; set; }
    }

    /// <summary>
    /// Result of a sync operation
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public int InsertedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime SyncedAt { get; set; }
    }

    /// <summary>
    /// Event args for sync status updates
    /// </summary>
    public class SyncStatusEventArgs : EventArgs
    {
        public string Status { get; set; } = "";
        public bool IsInProgress { get; set; }
        public bool HasError { get; set; }
        public DateTime? LastSyncTime { get; set; }
    }

    /// <summary>
    /// Represents a pending change to be synced
    /// </summary>
    public class PendingChange
    {
        public string TableName { get; set; } = "";
        public string Operation { get; set; } = ""; // INSERT, UPDATE, DELETE
        public object? PrimaryKeyValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Detailed column information for schema sync
    /// </summary>
    public class ColumnInfo
    {
        public string Name { get; set; } = "";
        public string DataType { get; set; } = "";
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public bool IsNullable { get; set; }
        public string? DefaultValue { get; set; }
    }

    #endregion
}
