using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Esh3arTech.Messages.Buffer
{
    public interface IHighThroughputBatchMessageBuffer
    {
        ChannelWriter<List<Message>> Writer { get; }

        ChannelReader<List<Message>> Reader { get; }

        Task<bool> TryWriteAsync(List<Message> messages, TimeSpan timeout);
    }
}
