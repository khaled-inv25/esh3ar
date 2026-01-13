using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Buffer
{
    public class HighThroughputBatchMessageBuffer : IHighThroughputBatchMessageBuffer, ISingletonDependency
    {
        private readonly Channel<List<Message>> _channel;

        public HighThroughputBatchMessageBuffer()
        {
            var options = new BoundedChannelOptions(Esh3arTechConsts.BufferLimit) // To prevent OOM
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<List<Message>>(options);
        }

        public ChannelWriter<List<Message>> Writer => _channel.Writer;

        public ChannelReader<List<Message>> Reader => _channel.Reader;

        public async Task<bool> TryWriteAsync(List<Message> msgs, TimeSpan timeout)
        {
            using var crs = new CancellationTokenSource(timeout);
            try
            {
                await _channel.Writer.WriteAsync(msgs, crs.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
