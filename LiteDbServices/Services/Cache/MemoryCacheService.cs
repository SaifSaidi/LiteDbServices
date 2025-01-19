using Microsoft.Extensions.Caching.Memory;

namespace LiteDbServices.Services.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItemCallback)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedItem))
            {
                return cachedItem!;
            }

            await _cacheLock.WaitAsync();
            try
            {
                // Double-check after acquiring the lock
                if (_memoryCache.TryGetValue(key, out cachedItem))
                {
                    return cachedItem!;
                }

                T item = await getItemCallback();
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.Normal,
                    PostEvictionCallbacks =
                    {
                        new PostEvictionCallbackRegistration
                        {
                            EvictionCallback = (key, value, reason, state) =>
                            {
                                Console.WriteLine($"Cache entry '{key}' was evicted. Reason: {reason}");
                            }
                        }
                    }
                };

                _memoryCache.Set(key, item, options);

                return item;
            }
            finally
            {
                _cacheLock.Release();
            }
        }


        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }

}
