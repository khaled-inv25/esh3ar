using AutoMapper;
using Esh3arTech.UserPlans;
using Esh3arTech.UserPlans.Plans;

namespace Esh3arTech;

public class Esh3arTechApplicationAutoMapperProfile : Profile
{
    public Esh3arTechApplicationAutoMapperProfile()
    {
        CreateMap<UserPlan, PlanInListDto>();
        CreateMap<UserPlan, PlanLookupDto>();
        CreateMap<UserPlan, PlanDto>();
    }
}
