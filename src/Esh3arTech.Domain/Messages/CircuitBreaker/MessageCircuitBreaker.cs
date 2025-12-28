using Esh3arTech.Messages.RetryPolicy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.CircuitBreaker
{
    public class MessageCircuitBreaker : ICircuitBreaker, ITransientDependency
    {
        private readonly IDistributedCache<CircuitBreakerState> _cache;
        private readonly MessageReliabilityOptions _options;
        private const string CacheKey = "circuit_breaker_state";

        public MessageCircuitBreaker(
            IDistributedCache<CircuitBreakerState> cache,
            IOptions<MessageReliabilityOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public async Task<bool> IsOpenAsync()
        {
            var state = await GetStateAsync();
            return state.State == CircuitState.Open;
        }

        public async Task RecordSuccessAsync()
        {
            var state = await GetStateAsync();
            
            if (state.State == CircuitState.HalfOpen)
            {
                // Success in half-open means we can close the circuit
                state.State = CircuitState.Closed;
                state.FailureCount = 0;
                state.SuccessCount = 0;
                state.LastFailureTime = null;
            }
            else
            {
                state.SuccessCount++;
                // Reset failure count on success streak
                if (state.SuccessCount >= _options.CircuitBreakerSampleSize)
                {
                    state.FailureCount = 0;
                    state.SuccessCount = 0;
                }
            }

            await SaveStateAsync(state);
        }

        public async Task RecordFailureAsync()
        {
            var state = await GetStateAsync();
            state.FailureCount++;
            state.LastFailureTime = DateTime.UtcNow;

            var totalRequests = state.FailureCount + state.SuccessCount;
            if (totalRequests >= _options.CircuitBreakerSampleSize)
            {
                var failureRate = (double)state.FailureCount / totalRequests;
                
                if (failureRate >= _options.CircuitBreakerFailureThreshold)
                {
                    state.State = CircuitState.Open;
                    state.OpenedAt = DateTime.UtcNow;
                }
            }

            await SaveStateAsync(state);
        }

        public CircuitState GetState()
        {
            // Synchronous access for immediate checks
            var state = _cache.Get(CacheKey);
            return state?.State ?? CircuitState.Closed;
        }

        private async Task<CircuitBreakerState> GetStateAsync()
        {
            var state = await _cache.GetAsync(CacheKey);
            
            if (state == null)
            {
                state = new CircuitBreakerState
                {
                    State = CircuitState.Closed,
                    FailureCount = 0,
                    SuccessCount = 0
                };
            }
            else if (state.State == CircuitState.Open)
            {
                // Check if timeout has passed to move to half-open
                if (state.OpenedAt.HasValue && 
                    DateTime.UtcNow - state.OpenedAt.Value > TimeSpan.FromSeconds(_options.CircuitBreakerTimeoutSeconds))
                {
                    state.State = CircuitState.HalfOpen;
                    state.FailureCount = 0;
                    state.SuccessCount = 0;
                }
            }

            return state;
        }

        private async Task SaveStateAsync(CircuitBreakerState state)
        {
            await _cache.SetAsync(CacheKey, state, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }

    public class CircuitBreakerState
    {
        public CircuitState State { get; set; }
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime? LastFailureTime { get; set; }
        public DateTime? OpenedAt { get; set; }
    }
}

