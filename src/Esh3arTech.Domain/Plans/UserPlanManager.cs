using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Esh3arTech.Plans
{
    public class UserPlanManager : DomainService
    {
        private readonly IUserPlanRepository _userPlanRepository;
        private readonly UserManager<IdentityUser> _identityUserManager;

        public UserPlanManager(
            IUserPlanRepository userPlanRepository,
            UserManager<IdentityUser> identityUserManager)
        {
            _userPlanRepository = userPlanRepository;
            _identityUserManager = identityUserManager;
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
            if (await _userPlanRepository.AnyAsync(up => up.Name.ToLower() == name))
            {
                throw new UserFriendlyException("Plan is already exists!");
            }

            var createdUserPlan = new UserPlan(
                GuidGenerator.Create(),
                name,
                displayName);

            createdUserPlan.SetExpiringPlanId(expiringPlanId);
            createdUserPlan.SetPriceInfo(dailyPrice, weeklyPrice, monthlayPrice, annualPrice);
            createdUserPlan.SetTrialDayCount(trialDayCount);
            createdUserPlan.SetWaitingDayAfterExpire(waitingDayAfterExpire);

            return createdUserPlan;
        }
        
        public async Task<UserPlan> UpdateUserPlanAsync(Guid Id, string name, Guid? expiringPlanId)
        {
            name = name.ToLower();
            var currentUserPlan = await _userPlanRepository.GetAsync(Id);

            if (!currentUserPlan.Name.Equals(name) && await _userPlanRepository.AnyAsync(up => up.Name.Equals(name)))
            {
                throw new BusinessException("Plan name is already taken!");
            }

            return currentUserPlan;
        }

        public async Task<bool> CanDeletePlanAsync(Guid planId)
        {
            return !await _userPlanRepository.IsThereLinkedUsersAsync(planId) && 
                   !await _userPlanRepository.AnyAsync(up => up.ExpiringPlanId.Equals(planId)); // to check if there is expiring plans linked to it.
        }
    }
}
