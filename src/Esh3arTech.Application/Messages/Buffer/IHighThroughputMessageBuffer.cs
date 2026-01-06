using System.Threading.Tasks;
using System;
using System.Threading.Channels;

namespace Esh3arTech.Messages.Buffer
{
    public interface IHighThroughputMessageBuffer
    {
        ChannelWriter<Message> Writer { get; }

        ChannelReader<Message> Reader { get; }

        Task<bool> TryWriteAsync(Message message, TimeSpan timeout);

        Task<BufferMetrics> GetMetricsAsync();

        Task<bool> IsNearCapacityAsync(double threshold = 0.8);

        Task<int> GetCurrentDepthAsync();
    }
}
