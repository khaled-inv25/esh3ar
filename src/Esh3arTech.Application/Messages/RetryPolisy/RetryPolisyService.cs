using Esh3arTech.Messages.RetryPolicy;
using System;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.RetryPolisy
{
    public class RetryPolisyService : IRetryPolisyService, ITransientDependency
    {
        private readonly ExponentialBackoffRetryPolicyManager _retryPolisyManager;

        public RetryPolisyService(ExponentialBackoffRetryPolicyManager retryPolisyManager)
        {
            _retryPolisyManager = retryPolisyManager;
        }

        public TimeSpan CalculateDelay(int retryCount) => _retryPolisyManager.CalculateDelay(retryCount);

        public bool CanRetry(int currentRetryCount) => _retryPolisyManager.CanRetry(currentRetryCount);
    }
}
