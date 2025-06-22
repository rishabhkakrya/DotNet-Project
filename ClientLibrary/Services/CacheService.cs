using ClientLibrary.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ClientLibrary.Services
{
    public class CacheService(IMemoryCache cache) : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (cache.TryGetValue(key, out T? value))
                return Task.FromResult(value);

            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
                cacheEntryOptions.SetAbsoluteExpiration(expiration.Value);

            cache.Set(key, value, cacheEntryOptions);
            return Task.CompletedTask;
        }
    }
}
