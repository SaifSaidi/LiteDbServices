using LiteDbServices.Exceptions;
using LiteDbServices.Models;
using LiteDbServices.Repositories;
using LiteDBP = LiteDB;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using LiteDB;
using LiteDbServices.Services.Cache;

namespace LiteDbServices.Services
{
    public class LiteDBService<T, TKey> : ILiteDBService<T, TKey> where T : LiteEntity<TKey>
    {
        private readonly IRepository<T, TKey> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<LiteDBService<T, TKey>> _logger;

        public LiteDBService(ILogger<LiteDBService<T, TKey>> logger, ICacheService cacheService, IRepository<T, TKey> repository)
        {
            _logger = logger;
            _cacheService = cacheService;
            _repository = repository;
        }

        public async Task<int> ClearAsync()
        {
            try
            {
                int result = await _repository.ClearAsync();
                await InvalidateCacheAsync();
                _logger.LogInformation($"Cleared all {typeof(T).Name} records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while clearing {typeof(T).Name} records: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            return await _repository.CountAsync();
        }

        public async Task<T> CreateAsync(T entity)
        {
            try
            {
                await _repository.CreateAsync(entity);
                await InvalidateCacheAsync();
                _logger.LogInformation($"Created {typeof(T).Name} with id {entity.Id}");
                return entity;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Validation failed when creating {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<T> entities)
        {
            var createdEntities = await _repository.CreateBulkAsync(entities);
            await InvalidateCacheAsync();
            _logger.LogInformation($"Created bulk {typeof(T).Name}");
            return createdEntities;
        }

        public async Task DeleteAsync(TKey id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                await InvalidateCacheAsync(id);
                _logger.LogInformation($"Deleted {typeof(T).Name} with id {id}");
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning($"{typeof(T).Name} with id {id} was not found for deletion.");
                throw;
            }
        }

        public async Task DeleteBulkAsync(IEnumerable<TKey> ids)
        {
            await _repository.DeleteBulkAsync(ids);
            await InvalidateCacheAsync();
            _logger.LogInformation($"Deleted bulk {typeof(T).Name}");
        }

        public async Task<int> DeleteBulkAsync(BsonExpression BsonExpression)
        {
            try
            {
                int result = await _repository.DeleteBulkAsync(BsonExpression);
                await InvalidateCacheAsync();
                _logger.LogInformation($"Deleted bulk {typeof(T).Name} records matching the given expression");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting bulk {typeof(T).Name} records: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EnsureIndexAsync(Expression<Func<T, object>> indexExpression, bool unique = false)
        {
        return    await _repository.EnsureIndexAsync(indexExpression, unique);
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            return await _repository.ExistsAsync(id);
        }

        public async Task<bool> ExistsAsync(BsonExpression predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            return await _repository.FindAsync(predicate, skip, limit);
        }

        public async Task<IEnumerable<T>> FindAsync(BsonExpression BsonExpression, int skip = 0, int limit = int.MaxValue)
        {
            return await _repository.FindAsync(BsonExpression, skip, limit);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> FirstOrDefaultAsync(BsonExpression BsonExpression)
        {
            return await _repository.FirstOrDefaultAsync(BsonExpression);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            string cacheKey = $"all_{typeof(T).Name}s";
            return await _cacheService.GetOrSetAsync(cacheKey,  () => _repository.GetAllAsync());
        }

       

        public async Task<T> GetByIdAsync(TKey id)
        {
            string cacheKey = $"{typeof(T).Name}_{id}";
            try
            {
                return await _cacheService.GetOrSetAsync(cacheKey, () => _repository.GetByIdAsync(id));
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning($"{typeof(T).Name} with id {id} was not found.");
                throw;
            }
        }

      
        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetPagedAsync(pageNumber, pageSize);
        }

        public async Task IncludeAsync<TInclude>(Expression<Func<T, TInclude>> includeExpression)
        {
            await _repository.IncludeAsync(includeExpression);
        }

        public async Task<ILiteQueryable<T>> QueryAsync()
        {
            return await _repository.QueryAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.SingleOrDefaultAsync(predicate);
        }

        public async Task<T> SingleOrDefaultAsync(BsonExpression BsonExpression)
        {
            return await _repository.SingleOrDefaultAsync(BsonExpression);
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                await _repository.UpdateAsync(entity);
                await InvalidateCacheAsync(entity.Id);
                _logger.LogInformation($"Updated {typeof(T).Name} with id {entity.Id}");
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning($"{typeof(T).Name} with id {entity.Id} was not found for update.");
                throw;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Validation failed when updating {typeof(T).Name} with id {entity.Id}: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateBulkAsync(IEnumerable<T> entities)
        {
            await _repository.UpdateBulkAsync(entities);
            await InvalidateCacheAsync();
            _logger.LogInformation($"Updated bulk {typeof(T).Name}");
        }

        public async Task UpsertAsync(T entity)
        {
            await _repository.UpsertAsync(entity);
            await InvalidateCacheAsync(entity.Id);
            _logger.LogInformation($"Upserted {typeof(T).Name} with id {entity.Id}");
        }

        private async Task InvalidateCacheAsync(TKey id = default!)
        {
            await _cacheService.RemoveAsync($"all_{typeof(T).Name}s");
            if (!EqualityComparer<TKey>.Default.Equals(id, default))
            {
                await _cacheService.RemoveAsync($"{typeof(T).Name}_{id}");
            }
        }
    }
}
