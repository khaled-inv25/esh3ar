using System.Collections.Generic;
using Volo.Abp.Caching;

namespace Esh3arTech.Web.MobileUsers.CacheItems
{
    [CacheName("MobileUsers")]
    public class UserConnectionsCacheItem
    {
        public List<string> ConnectionIds { get; set; } = new();
    }
}
