using System;
using System.Threading.Tasks;
using Esh3arTech.Messages.RetryPolicy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Idempotency
{
    public class IdempotencyService : IIdempotencyService, ITransientDependency
    {
        private readonly IDistributedCache<IdempotencyRecord> _cache;
        private readonly MessageReliabilityOptions _options;

        public IdempotencyService(
            IDistributedCache<IdempotencyRecord> cache,
            IOptions<MessageReliabilityOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public async Task<bool> IsProcessedAsync(string idempotencyKey)
        {
            var record = await _cache.GetAsync(idempotencyKey);
            return record != null;
        }

        public async Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl)
        {
            var record = new IdempotencyRecord
            {
                Key = idempotencyKey,
                ProcessedAt = DateTime.UtcNow
            };
            
            await _cache.SetAsync(idempotencyKey, record, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }

        public async Task<string> GenerateKeyAsync(Guid messageId)
        {
            return $"msg_{messageId}";
        }
    }

    public class IdempotencyRecord
    {
        public string Key { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

