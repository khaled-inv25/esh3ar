using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Buffer
{
    public class HighThroughputMessageBuffer : IHighThroughputMessageBuffer, ISingletonDependency
    {
        private readonly Channel<Message> _channel;
        private volatile int _currentDepth;

        public HighThroughputMessageBuffer()
        {
            var options = new BoundedChannelOptions(Esh3arTechConsts.BufferLimit)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<Message>(options);
        }

        public ChannelWriter<Message> Writer => _channel.Writer;

        public ChannelReader<Message> Reader => _channel.Reader;

        public Task<int> GetCurrentDepthAsync()
        {
            return Task.FromResult(_currentDepth);
        }

        public Task<BufferMetrics> GetMetricsAsync()
        {
            return Task.FromResult(new BufferMetrics
            {
                CurrentDepth = _currentDepth,
                MaxCapacity = Esh3arTechConsts.BufferLimit,
                UtilizationPercentage = (double)_currentDepth / Esh3arTechConsts.BufferLimit * 100,
                LastUpdated = DateTime.UtcNow
            });
        }

        public async Task<bool> IsNearCapacityAsync(double threshold = 0.8)
        {
            var metrics = await GetMetricsAsync();

            return metrics.UtilizationPercentage >= (threshold * 100);
        }

        public async Task<bool> TryWriteAsync(Message message, TimeSpan timeout)
        {
            using var crs = new CancellationTokenSource(timeout);
            try
            {
                await _channel.Writer.WriteAsync(message, crs.Token);
                Interlocked.Increment(ref _currentDepth);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
