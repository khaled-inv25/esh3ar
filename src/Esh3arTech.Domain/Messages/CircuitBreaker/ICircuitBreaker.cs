using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.CircuitBreaker
{
    public interface ICircuitBreaker : ITransientDependency
    {
        Task<bool> IsOpenAsync();
        Task RecordSuccessAsync();
        Task RecordFailureAsync();
        CircuitState GetState();
    }

    public enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }
}

