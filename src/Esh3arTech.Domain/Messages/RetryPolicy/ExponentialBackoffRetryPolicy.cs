using System;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.RetryPolicy
{
    public class ExponentialBackoffRetryPolicy : IMessageRetryPolicy, ITransientDependency
    {
        private readonly MessageReliabilityOptions _options;

        public ExponentialBackoffRetryPolicy(IOptions<MessageReliabilityOptions> options)
        {
            _options = options.Value;
        }

        public int MaxRetries => _options.MaxRetries;

        public TimeSpan CalculateDelay(int retryCount)
        {
            var baseDelay = TimeSpan.FromSeconds(_options.BaseRetryDelaySeconds);
            var maxDelay = TimeSpan.FromSeconds(_options.MaxRetryDelaySeconds);
            
            var delay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(2, retryCount));
            return delay > maxDelay ? maxDelay : delay;
        }

        public bool CanRetry(int retryCount)
        {
            return retryCount < MaxRetries;
        }
    }
}

