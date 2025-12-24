namespace Esh3arTech.Messages.SendBehavior
{
    public interface IMessageFactory
    {
        IOneWayMessageManager Create(MessageType type);
    }
}
