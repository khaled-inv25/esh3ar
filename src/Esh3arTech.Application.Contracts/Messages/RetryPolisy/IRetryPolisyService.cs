using System;

namespace Esh3arTech.Messages.RetryPolisy
{
    public interface IRetryPolisyService
    {
        bool CanRetry(int currentRetryCount);
        TimeSpan CalculateDelay(int retryCount);
    }
}
