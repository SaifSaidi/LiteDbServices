using LiteDbServices.Exceptions;
using LiteDbServices.Models;
using LiteDbServices.Repositories;
using LiteDBP = LiteDB;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using LiteDB;

namespace LiteDbServices.Services
{
    public class LiteDBService<T> : ILiteDBService<T> where T : ILiteEntity
    {
        private readonly IRepository<T> _repository;
        private readonly ICacheService _cacheService; 
        private readonly ILogger<LiteDBService<T>> _logger;

        public LiteDBService(IRepository<T> repository,
            ICacheService cacheService, 
            ILogger<LiteDBService<T>> logger
            )
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger; 
        }

        public async Task<T> GetByIdAsync(ObjectId id)
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
        
        public async Task<T> GetOneByQueryAsync(Expression<Func<T, bool>> query)
        { 
            string cacheKey = $"{typeof(T).Name}_query_{Guid.NewGuid()}";
            try
            {
                return await _cacheService.GetOrSetAsync(cacheKey, () => _repository.GetOneByQueryAsync(query));
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning($"{typeof(T).Name} with {query.Name} was not found.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            string cacheKey = $"all_{typeof(T).Name}s";
            return await _cacheService.GetOrSetAsync(cacheKey, () => _repository.GetAllAsync());
        }

        public async Task<ObjectId> CreateAsync(T entity)
        {
            try
            {
 
                ObjectId id = await _repository.CreateAsync(entity);
                await _cacheService.RemoveAsync($"all_{typeof(T).Name}s");
                _logger.LogInformation($"Created {typeof(T).Name} with id {id}");
                return id;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Validation failed when creating {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                await _repository.UpdateAsync(entity);
                await _cacheService.RemoveAsync($"{typeof(T).Name}_{entity.Id}");
                await _cacheService.RemoveAsync($"all_{typeof(T).Name}s");
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

        public async Task DeleteAsync(ObjectId id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                await _cacheService.RemoveAsync($"{typeof(T).Name}_{id}");
                await _cacheService.RemoveAsync($"all_{typeof(T).Name}s");
                _logger.LogInformation($"Deleted {typeof(T).Name} with id {id}");
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning($"{typeof(T).Name} with id {id} was not found for deletion.");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(ObjectId id)
        {
            return await _repository.ExistsAsync(id);
        }

     }
}
