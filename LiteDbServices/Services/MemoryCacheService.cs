using Microsoft.Extensions.Caching.Memory;

namespace LiteDbServices.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItemCallback)
        {
            if (!_memoryCache.TryGetValue(key, out T cachedItem))
            {
                cachedItem = await getItemCallback();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _memoryCache.Set(key, cachedItem, cacheEntryOptions);
            }
            return cachedItem;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
