namespace Esh3arTech.Messages
{
    public enum MessageStatus : byte
    {
        Pending = 0,
        Delivered = 1,
        Read = 2,
        Failed = 3,
        Sent = 4,
    }
}
