using Microsoft.Extensions.Options;
using System;
using Volo.Abp.Domain.Services;

namespace Esh3arTech.Messages.RetryPolicy
{
    public class ExponentialBackoffRetryPolicyManager : DomainService
    {
        private readonly MessageReliabilityOptions _options;

        public ExponentialBackoffRetryPolicyManager(IOptions<MessageReliabilityOptions> options)
        {
            _options = options.Value;
        }

        public bool CanRetry(int currentRetryCount)
        {
            return currentRetryCount < _options.MaxRetryAttempts;
        }

        public TimeSpan CalculateDelay(int retryCount)
        {
            var baseDelay = TimeSpan.FromSeconds(_options.BaseRetryDelaySeconds);
            var maxDelay = TimeSpan.FromSeconds(_options.MaxRetryDelaySeconds);
            
            var delay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(2, retryCount));

            return delay > maxDelay ? maxDelay : delay;
        }
    }
}
