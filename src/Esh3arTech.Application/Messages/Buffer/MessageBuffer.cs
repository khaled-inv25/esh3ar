using System.Threading.Channels;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Buffer
{
    public class MessageBuffer : IMessageBuffer, ISingletonDependency
    {
        private readonly Channel<Message> _channel;

        public MessageBuffer() => _channel = Channel.CreateBounded<Message>(Esh3arTechConsts.BufferLimit); // To prevent OOM

        public ChannelWriter<Message> Writer => _channel.Writer;

        public ChannelReader<Message> Reader => _channel.Reader;
    }
}
