﻿
namespace LiteDbServices.Services.Cache
{
    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItemCallback);
        Task RemoveAsync(string key);
    }

}
