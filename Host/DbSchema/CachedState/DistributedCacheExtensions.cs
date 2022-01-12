using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DbSchema.CachedState
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            string recordId,
            T data,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
            options.SlidingExpiration = TimeSpan.FromDays(30);

            var jsonData = JsonConvert.SerializeObject(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);

            if (jsonData is null)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}
