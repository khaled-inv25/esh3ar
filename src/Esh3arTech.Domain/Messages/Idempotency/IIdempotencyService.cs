using System.Threading.Tasks;

namespace Esh3arTech.Messages.Idempotency
{
    public interface IIdempotencyService
    {
        Task<bool> IsProcessedAsync(string idempotencyKey);
        Task MarkAsProcessedAsync(string idempotencyKey, System.TimeSpan ttl);
        Task<string> GenerateKeyAsync(System.Guid messageId);
    }
}

