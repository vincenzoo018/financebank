using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;

namespace FinanceBank.Services
{
    /// <summary>
    /// Generic base CRUD service for all entities
    /// </summary>
    public class BaseCrudService<TEntity> where TEntity : class
    {
        protected readonly BFASDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseCrudService(BFASDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving entities: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public virtual async Task<TEntity?> GetByIdAsync(object id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving entity by ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create/Add new entity
        /// </summary>
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update existing entity
        /// </summary>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete entity by ID
        /// </summary>
        public virtual async Task DeleteAsync(object id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete specific entity
        /// </summary>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if entity exists by ID
        /// </summary>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                return entity != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get count of all entities
        /// </summary>
        public virtual async Task<int> GetCountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error counting entities: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get paginated results
        /// </summary>
        public virtual async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var totalCount = await _dbSet.CountAsync();
                var items = await _dbSet
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving paged results: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Bulk create/insert
        /// </summary>
        public virtual async Task<List<TEntity>> CreateManyAsync(List<TEntity> entities)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return entities;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error bulk creating entities: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Bulk delete
        /// </summary>
        public virtual async Task DeleteManyAsync(List<TEntity> entities)
        {
            try
            {
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error bulk deleting entities: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Custom exception for service errors
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception? innerException = null) 
            : base(message, innerException)
        {
        }
    }
}
