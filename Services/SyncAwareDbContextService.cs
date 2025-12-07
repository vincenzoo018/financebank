using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service that wraps database operations and automatically queues changes for sync
    /// Use this service when you want data changes to automatically sync to cloud
    /// </summary>
    public class SyncAwareDbContextService
    {
        private readonly IDbContextFactory<BFASDbContext> _contextFactory;
        private readonly DatabaseSyncService _syncService;

        public SyncAwareDbContextService(
            IDbContextFactory<BFASDbContext> contextFactory,
            DatabaseSyncService syncService)
        {
            _contextFactory = contextFactory;
            _syncService = syncService;
        }

        /// <summary>
        /// Save changes and queue for auto-sync
        /// </summary>
        public async Task<int> SaveChangesAndSyncAsync(BFASDbContext context, string tableName)
        {
            var changes = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var result = await context.SaveChangesAsync();

            // Queue changes for auto-sync
            foreach (var change in changes)
            {
                var operation = change.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                // Try to get primary key value
                var primaryKey = change.Properties
                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                    ?.CurrentValue;

                _syncService.QueueChange(tableName, operation, primaryKey);
            }

            return result;
        }

        /// <summary>
        /// Add entity and queue for sync
        /// </summary>
        public async Task<T> AddAndSyncAsync<T>(T entity, string tableName) where T : class
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Add(entity);
            await SaveChangesAndSyncAsync(context, tableName);
            return entity;
        }

        /// <summary>
        /// Update entity and queue for sync
        /// </summary>
        public async Task<T> UpdateAndSyncAsync<T>(T entity, string tableName) where T : class
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await SaveChangesAndSyncAsync(context, tableName);
            return entity;
        }

        /// <summary>
        /// Delete entity and queue for sync
        /// </summary>
        public async Task DeleteAndSyncAsync<T>(T entity, string tableName) where T : class
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Remove(entity);
            await SaveChangesAndSyncAsync(context, tableName);
        }

        /// <summary>
        /// Force immediate sync to cloud
        /// </summary>
        public async Task ForceSyncNowAsync()
        {
            await _syncService.ForceSyncAsync();
        }

        /// <summary>
        /// Check if auto-sync is available (internet connected)
        /// </summary>
        public async Task<bool> IsAutoSyncAvailableAsync()
        {
            return await _syncService.IsInternetAvailableAsync();
        }
    }
}
