using Volo.Abp.Identity;

namespace Esh3arTech;

public static class Esh3arTechConsts
{
    public const string DbTablePrefix = "Et";
    public const string QueuePrefix = "Esh3arTech";

    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;

    public const string TblMobileUsers = DbTablePrefix + "MobileUsers";
    public const string TblRegistretionRequest = DbTablePrefix + "RegistretionRequest";
    
    public const string TblUserPlan = DbTablePrefix + "UserPlans";
    public const string TblSubscriptions = DbTablePrefix + "Subscriptions";
    public const string TblSubscriptionRenewalHistory = DbTablePrefix + "SubscriptionRenewalHistory";

    public const string TblMessage = DbTablePrefix + "Messages";
    public const string TblMessageAttachment = DbTablePrefix + "MessageAttachments";

    public const string DefaultOtpSubject = "OTP";

    public const int BufferLimit = 10000; // Limit to 10k to prevent OOM

    public static class HubMethods
    {
        public const string ReceivePendingMessages = "ReceivePendingMessages";
        public const string ReceiveMessage = "ReceiveMessage";
        public const string ReceiveChatMessage = "ReceiveChatMessage";
    }

    public static class BackgroundJobNames
    {
        public const string BatchMessageArg = "IngestionBatchMessageArg";
        public const string SendMessageFromUiArg = "SendMessageFromUiArg";
        public const string SendMessageFromUiWithAttachmentArg = "SendMessageFromUiWithAttachmentArg";
    }
}
