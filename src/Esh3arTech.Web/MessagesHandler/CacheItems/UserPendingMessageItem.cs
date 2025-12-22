using System.Collections.Generic;
using Volo.Abp.Caching;

namespace Esh3arTech.Web.MessagesHandler.CacheItems
{
    [CacheName("PendingMessage")]
    public class UserPendingMessageItem
    {
        public List<string> penddingMessages { get; set; } = new();
    }
}
