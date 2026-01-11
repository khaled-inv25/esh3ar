using Esh3arTech.Web.MobileUsers.CacheItems;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Web.MobileUsers
{
    public class OnlineUserTrackerService : ISingletonDependency
    {
        private readonly IDistributedCache<MobileUserConnectionCacheItem> _cache;

        public OnlineUserTrackerService(IDistributedCache<MobileUserConnectionCacheItem> cache)
        {
            _cache = cache;
        }

        public async Task AddConnection(string mobileNumber, string connectionId)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey) ?? new MobileUserConnectionCacheItem();

            if (string.IsNullOrEmpty(cacheItem.ConnectionId))
            {
                cacheItem.ConnectionId = connectionId;
                await _cache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }
        }

        public async Task RemoveConnection(string mobileNumber)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey);

            if (cacheItem != null)
            {
                await _cache.RemoveAsync(cacheKey);
            }
        }

        public async Task<string?> GetFirstConnectionIdByPhoneNumberAsync(string mobileNumber)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey);

            return cacheItem?.ConnectionId;
        }
    }
}
