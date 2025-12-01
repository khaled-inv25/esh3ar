namespace Esh3arTech.Plans.Subscriptions
{
    public enum SubscriptionStatus : byte
    {
        Inactive,
        Active,
        PastDue,
        Canceled,
        Unpaid,
        Expired,
        Trialing,
        Pending
    }
}
