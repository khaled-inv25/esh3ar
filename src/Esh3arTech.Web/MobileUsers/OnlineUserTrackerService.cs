using Esh3arTech.Web.MobileUsers.CacheItems;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Web.MobileUsers
{
    public class OnlineUserTrackerService : ITransientDependency
    {
        private readonly IDistributedCache<UserConnectionsCacheItem> _cache;

        public OnlineUserTrackerService(IDistributedCache<UserConnectionsCacheItem> cache)
        {
            _cache = cache;
        }

        public async Task AddConnection(string mobileNumber, string connectionId)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey) ?? new UserConnectionsCacheItem();

            if (!cacheItem.ConnectionIds.Contains(cacheKey))
            {
                cacheItem.ConnectionIds.Add(connectionId);
                await _cache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }
        }

        public async Task RemoveConnection(string mobileNumber, string connectionId)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey);

            if (cacheItem != null)
            {
                cacheItem.ConnectionIds.Remove(connectionId);
                if (cacheItem.ConnectionIds.Count != 0)
                {
                    await _cache.SetAsync(cacheKey, cacheItem);
                }
                else
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }
        }

        public async Task<string?> GetFirstConnectionIdByPhoneNumberAsync(string mobileNumber)
        {
            var cacheKey = mobileNumber;
            var cacheItem = await _cache.GetAsync(cacheKey);

            return cacheItem?.ConnectionIds?.FirstOrDefault();
        }
    }
}
