using System.Threading.Channels;

namespace Esh3arTech.Messages.Buffer
{
    public interface IMessageBuffer
    {
        ChannelWriter<MessageBufferDto> Writer { get; }
        ChannelReader<MessageBufferDto> Reader { get; }
    }
}
