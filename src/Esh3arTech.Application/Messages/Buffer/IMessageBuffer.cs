using System.Threading.Channels;

namespace Esh3arTech.Messages.Buffer
{
    public interface IMessageBuffer
    {
        ChannelWriter<Message> Writer { get; }
        ChannelReader<Message> Reader { get; }
    }
}
