using AutoMapper;
using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.MobileUsers;
using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using Esh3arTech.Users;
using Volo.Abp.Identity;

namespace Esh3arTech;

public class Esh3arTechApplicationAutoMapperProfile : Profile
{
    public Esh3arTechApplicationAutoMapperProfile()
    {
        CreateMap<UserPlan, PlanInListDto>();
        CreateMap<UserPlan, PlanLookupDto>();
        CreateMap<UserPlan, PlanDto>();

        CreateMap<IdentityUser, UserLookupDto>();

        CreateMap<SubscriptionWithDetails, SubscriptionInListDto>();
        CreateMap<Subscription, SubscriptionDto>();
        CreateMap<SubscriptionRenewalHistory, SubscriptionHistoryInListDto>();

        CreateMap<BroadcastMessageDto, BroadcastMessageEvent>();

        CreateMap<Message, PendingMessageDto>();
        CreateMap<Message, SendOneWayMessageEto>();
        CreateMap<Message, MessageInListDto>();
        CreateMap<BatchMessageItem, BatchMessage>();
        CreateMap<BatchMessageItem, EtTempMobileUserData>();
    }
}
