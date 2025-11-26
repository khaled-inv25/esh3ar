using AutoMapper;
using Esh3arTech.Plans;
using Esh3arTech.UserPlans;

namespace Esh3arTech.Web;

public class Esh3arTechWebAutoMapperProfile : Profile
{
    public Esh3arTechWebAutoMapperProfile()
    {
        CreateMap<PlanDto, UpdatePlanDto>().ReverseMap();
    }
}
