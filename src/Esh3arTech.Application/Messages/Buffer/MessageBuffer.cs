using System.Threading.Channels;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Buffer
{
    public class MessageBuffer : IMessageBuffer, ISingletonDependency
    {
        private readonly Channel<MessageBufferDto> _channel;

        public MessageBuffer() => _channel = Channel.CreateBounded<MessageBufferDto>(Esh3arTechConsts.BufferLimit); // To prevent OOM

        public ChannelWriter<MessageBufferDto> Writer => _channel.Writer;

        public ChannelReader<MessageBufferDto> Reader => _channel.Reader;
    }
}
