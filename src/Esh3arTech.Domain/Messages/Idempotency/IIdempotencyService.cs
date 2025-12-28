using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Idempotency
{
    public interface IIdempotencyService : ITransientDependency
    {
        Task<bool> IsProcessedAsync(string idempotencyKey);
        Task MarkAsProcessedAsync(string idempotencyKey, System.TimeSpan ttl);
        Task<string> GenerateKeyAsync(System.Guid messageId);
    }
}

