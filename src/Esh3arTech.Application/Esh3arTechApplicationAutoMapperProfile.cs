using AutoMapper;
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
    }
}
