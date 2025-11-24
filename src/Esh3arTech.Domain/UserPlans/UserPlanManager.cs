using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Esh3arTech.UserPlans
{
    public class UserPlanManager : DomainService
    {
        private readonly IUserPlanRepository _userPlanRepository;

        public UserPlanManager(IUserPlanRepository userPlanRepository)
        {
            _userPlanRepository = userPlanRepository;
        }

        public async Task<UserPlan> CreateUserPlan(
            string name, 
            string displayName,
            Guid? expiringPlanId,
            decimal? dailyPrice,
            decimal? weeklyPrice,
            decimal? monthlayPrice,
            decimal? annualPrice,
            int? trialDayCount,
            int? waitingDayAfterExpire)
        {
            var createdUserPlan = new UserPlan(
                GuidGenerator.Create(),
                name,
                displayName);
           
            if (await _userPlanRepository.AnyAsync(up => up.Name == name))
            {
                throw new UserFriendlyException("Plan is already exists!");
            }

            createdUserPlan.SetExpiringPlanId(expiringPlanId);
            createdUserPlan.SetPriceInfo(dailyPrice, weeklyPrice, monthlayPrice, annualPrice);
            createdUserPlan.SetTrialDayCount(trialDayCount);
            createdUserPlan.SetWaitingDayAfterExpire(waitingDayAfterExpire);

            return createdUserPlan;
        }
    }
}
