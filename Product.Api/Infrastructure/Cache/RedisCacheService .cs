using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Product.Api.Application.Abstractions;

namespace Product.Api.Infrastructure.Cache
{
    public sealed class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache) => _cache = cache;

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var bytes = await _cache.GetAsync(key, ct);
            if (bytes is null) return default;
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var json = JsonConvert.SerializeObject(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var options = new DistributedCacheEntryOptions();
            if (ttl.HasValue) options.SetAbsoluteExpiration(ttl.Value);
            await _cache.SetAsync(key, bytes, options, ct);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _cache.RemoveAsync(key, ct);
    }
}
