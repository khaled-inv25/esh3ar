using AutoMapper;
using Esh3arTech.Plans;

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
