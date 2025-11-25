using AutoMapper;
using Esh3arTech.UserPlans;
using Esh3arTech.UserPlans.Plans;

namespace Esh3arTech.Web;

public class Esh3arTechWebAutoMapperProfile : Profile
{
    public Esh3arTechWebAutoMapperProfile()
    {
        CreateMap<PlanDto, UpdatePlanDto>().ReverseMap();
    }
}
