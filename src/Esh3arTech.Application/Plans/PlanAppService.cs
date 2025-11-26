using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Esh3arTech.Plans
{
    public class PlanAppService : Esh3arTechAppService, IPlanAppService
    {
        private readonly IUserPlanRepository _planRepository;
        private readonly ICurrentUser _currentUser;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IdentityUserManager _identityUserManager;
        private readonly IFeatureManager _featureManager;
        private readonly IFeatureDefinitionManager _featureDefinitionManager;
        private readonly UserPlanManager _userPlanManager;
        private readonly IUserPlanRepository _userPlanRepository;

        public PlanAppService(
            IUserPlanRepository planRepository,
            ICurrentUser currentUser,
            UserManager<IdentityUser> userManager,
            IdentityUserManager identityUserManager,
            IFeatureManager featureManager,
            IFeatureDefinitionManager featureDefinitionManager,
            UserPlanManager userPlanManager,
            IUserPlanRepository userPlanRepository)
        {
            _planRepository = planRepository;
            _currentUser = currentUser;
            _userManager = userManager;
            _identityUserManager = identityUserManager;
            _featureManager = featureManager;
            _featureDefinitionManager = featureDefinitionManager;
            _userPlanManager = userPlanManager;
            _userPlanRepository = userPlanRepository;
        }

        public async Task<PagedResultDto<PlanInListDto>> GetAllPlansAsync(PlanListFilter input)
        {
            // To make sure it's authenticated admin user.
            if (_currentUser.IsAuthenticated)
            {
                if (!_currentUser.IsInRole("admin"))
                {
                    throw new AbpAuthorizationException("Only authenticated admin user can access plans.");
                }
            }
            else
            {
                throw new AbpAuthorizationException("Only authenticated user can access plans.");
            }

            var planQueryable = await _planRepository.GetQueryableAsync();

            var query = from up in planQueryable
                        join sup in planQueryable on up.ExpiringPlanId equals sup.Id into expiringPlanGroup
                        from sup in expiringPlanGroup.DefaultIfEmpty()
                        select new PlanInListDto
                        {
                            Id = up.Id,
                            DisplayName = up.DisplayName,
                            ExpiringPlan = sup.DisplayName,
                            DailyPrice = up.DailyPrice,
                            WeeklyPrice = up.WeeklyPrice,
                            MonthlayPrice = up.MonthlayPrice,
                            AnnualPrice = up.AnnualPrice,
                            TrialDayCount = up.TrialDayCount,
                            WaitingDayAfterExpire = up.WaitingDayAfterExpire
                        };

            query = query.PageBy(input);
            var count = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<PlanInListDto>(count, items);
        }

        public async Task CreatePlanAsync(CreatePlanDto input)
        {
            var plan = await _userPlanManager.CreateUserPlan(
                input.Name,
                input.DisplayName, 
                input.ExpiringPlanId, 
                input.DailyPrice,
                input.WeeklyPrice,
                input.MonthlayPrice,
                input.AnnualPrice,
                input.TrialDayCount,
                input.WaitingDayAfterExpire);

            await _planRepository.InsertAsync(plan);

            foreach (var planFeatureDto in input.Features)
            {
                var featureName = planFeatureDto.Name;
                var featureValue = planFeatureDto.Value;
                await _featureManager.SetAsync(
                    name: featureName,
                    value: featureValue,
                    providerName: "P",
                    providerKey: plan.Id.ToString());
            }
        }

        public async Task<PlanDto> GetPlanByIdAsync(Guid planId)
        {
            var plan = await _planRepository.GetAsync(planId);

            var planToReturn = ObjectMapper.Map<UserPlan, PlanDto>(plan);

            planToReturn.Features = await GetFeaturesForPlanAsync(planId);

            return planToReturn;
        }

        public async Task<List<PlanFeatureDto>> GetFeaturesForPlanAsync(Guid? planId = null)
        {
            var result = new List<PlanFeatureDto>();

            var featureGroups = await _featureDefinitionManager.GetGroupsAsync();

            foreach (var group in featureGroups)
            {
                foreach (var featureDef in group.Features)
                {
                    string currentValue = featureDef.DefaultValue;

                    if (planId.HasValue)
                    {
                        currentValue = await _featureManager.GetOrNullAsync(featureDef.Name, "P", planId.Value.ToString());
                        currentValue ??= featureDef.DefaultValue;
                    }

                    result.Add(new PlanFeatureDto
                    {
                        Name = featureDef.Name,
                        DisplayName = featureDef.DisplayName.ToString(),
                        Value = currentValue,
                        ValueType = featureDef.ValueType
                    });
                }
            }

            return result;
        }

        public async Task<List<PlanFeatureDto>> GetDefaultFeaturesAsync()
        {
            return await GetFeaturesForPlanAsync(null);
        }

        public async Task<List<PlanLookupDto>> GetPlanLookupAsync()
        {
            var plans = await _planRepository.GetListAsync();

            return ObjectMapper.Map<List<UserPlan>, List<PlanLookupDto>>(plans);
        }

        public async Task UpdateAsync(Guid Id, UpdatePlanDto input)
        
        {
            var planTobeUpdated = await _userPlanManager.UpdateUserPlanAsync(Id, input.Name, input.ExpiringPlanId);

            if (input.ExpiringPlanId.HasValue)
            {
                if (!await _userPlanRepository.AnyAsync(up => up.Id.Equals(input.ExpiringPlanId)) && !planTobeUpdated.Id.Equals(input.ExpiringPlanId.Value))
                {
                    throw new BusinessException("Expire plan not exists!");
                }
            }

            planTobeUpdated.SetExpiringPlanId(input.ExpiringPlanId);

            planTobeUpdated.DisplayName = input.DisplayName;

            planTobeUpdated.SetPriceInfo(
                    input.DailyPrice,
                    input.WeeklyPrice,
                    input.MonthlayPrice,
                    input.AnnualPrice);

            if (input.TrialDayCount.HasValue)
            {
                planTobeUpdated.SetTrialDayCount(input.TrialDayCount);
            }

            if (input.WaitingDayAfterExpire.HasValue)
            {
                planTobeUpdated.SetWaitingDayAfterExpire(input.WaitingDayAfterExpire);
            }

            foreach (var planFeatureDto in input.Features)
            {
                await _featureManager.SetAsync(planFeatureDto.Name, planFeatureDto.Value, "P", Id.ToString());
            }

            await _planRepository.UpdateAsync(planTobeUpdated);
        }

        public Task AssignAfallback(ExpireToPlanDto input)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetAssignedUsersCountForPlanAsync(Guid planId)
        {
            if (!await _userPlanRepository.AnyAsync(p => p.Id.Equals(planId)))
            {
                throw new UserFriendlyException("Plan not exists!");
            }

            return await _planRepository.GetLinkedUsersCountToPlan(planId);
        }

        public Task<PlanDto> GetUserPlanInfoAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Guid planId)
        {
            var planToDelete = await _planRepository.GetAsync(planId);

            if (!await _userPlanManager.CanDeletePlanAsync(planId))
            {
                throw new BusinessException("There are customers subscriped to this plan. please assign different plan to then then delete this plan.");
            }

            await _planRepository.DeleteAsync(planToDelete);
        }

        public Task<int> GetLinkedUsersCountToPlan(Guid planId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetLinkedUsersCountWithPlan()
        {
            return await _planRepository.GetLinkedUsersCountWithPlan();
        }

        public async Task MoveUsersToPlan(Guid planId)
        {
            if (await GetLinkedUsersCountWithPlan() == 0)
            {
                throw new UserFriendlyException("No assigned users to move!");
            }

            await _planRepository.MoveUsersToPlan(planId);
        }
    }
}
