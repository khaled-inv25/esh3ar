using System;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.RetryPolicy
{
    public interface IMessageRetryPolicy : ITransientDependency
    {
        TimeSpan CalculateDelay(int retryCount);
        bool CanRetry(int retryCount);
        int MaxRetries { get; }
    }
}

