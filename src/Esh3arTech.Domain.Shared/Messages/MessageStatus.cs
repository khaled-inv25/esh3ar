namespace Esh3arTech.Messages
{
    public enum MessageStatus : byte
    {
        Pending = 0,    // Created, not yet sent
        Sent = 1,       // Enqueued for delivery
        Delivered = 2,  // Received by recipient device
        Read = 3,       // Opened by recipient
        Queued = 4,     // Waiting for action
        Failed = 5      // Delivery failed after retries
    }
}
