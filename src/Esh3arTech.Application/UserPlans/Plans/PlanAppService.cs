using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Esh3arTech.UserPlans.Plans
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

        public PlanAppService(
            IUserPlanRepository planRepository,
            ICurrentUser currentUser,
            UserManager<IdentityUser> userManager,
            IdentityUserManager identityUserManager,
            IFeatureManager featureManager,
            IFeatureDefinitionManager featureDefinitionManager,
            UserPlanManager userPlanManager)
        {
            _planRepository = planRepository;
            _currentUser = currentUser;
            _userManager = userManager;
            _identityUserManager = identityUserManager;
            _featureManager = featureManager;
            _featureDefinitionManager = featureDefinitionManager;
            _userPlanManager = userPlanManager;
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

        public async Task AssginPlanToUserAsync(AssignPlanToUserDto input)
        {
            // Use GetAsync to make sure plan is exists.
            var plan = await _planRepository.GetAsync(p => p.Id == input.PlanId);

            var user = await _identityUserManager.GetByIdAsync(input.UserId) 
                ?? throw new UserFriendlyException("No user");

            user.SetProperty("PlanId", input.PlanId.ToString());

            await _userManager.UpdateAsync(user);
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

        public Task<PlanDto> GetPlanByIdAsync(string planId)
        {
            throw new NotImplementedException();
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

        public async Task GetEditionsForComboboxAsync()
        {

        }

        public async Task<List<PlanLookupDto>> GetPlanLookupAsync()
        {
            var plans = await _planRepository.GetListAsync();

            return ObjectMapper.Map<List<UserPlan>, List<PlanLookupDto>>(plans);
        }
    }
}
