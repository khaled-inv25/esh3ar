using AutoMapper;
using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using static Esh3arTech.Web.Pages.Plans.Subscriptions.AssignToUserModel;
using static Esh3arTech.Web.Pages.Plans.Subscriptions.RenewModalModel;

namespace Esh3arTech.Web;

public class Esh3arTechWebAutoMapperProfile : Profile
{
    public Esh3arTechWebAutoMapperProfile()
    {
        CreateMap<PlanDto, UpdatePlanDto>().ReverseMap();
        CreateMap<PlanDto, PlanViewModel>().ReverseMap();

        CreateMap<SubscriptionToUserViewModel, AssignSubscriptionToUserDto>().ReverseMap();
        CreateMap<SubscriptionDto, SubscriptionViewModel>().ReverseMap();
        CreateMap<RenewSubscriptionViewModel, RenewSubscriptionDto>();
    }
}
