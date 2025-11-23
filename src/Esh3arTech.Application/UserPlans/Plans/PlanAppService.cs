using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using System.Linq;
using Volo.Abp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.FeatureManagement;
using System.Collections.Generic;
using Volo.Abp.Features;

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

        public PlanAppService(
            IUserPlanRepository planRepository,
            ICurrentUser currentUser,
            UserManager<IdentityUser> userManager,
            IdentityUserManager identityUserManager,
            IFeatureManager featureManager,
            IFeatureDefinitionManager featureDefinitionManager)
        {
            _planRepository = planRepository;
            _currentUser = currentUser;
            _userManager = userManager;
            _identityUserManager = identityUserManager;
            _featureManager = featureManager;
            _featureDefinitionManager = featureDefinitionManager;
        }

        public async Task<PagedResultDto<PlanInListDto>> GetAllPlansAsync(PlanListFilter input)
        {
            // To make sure it's admin user and logged in.
            var currentUser = await _userManager.FindByIdAsync(_currentUser.GetId().ToString());
            if (currentUser is not null)
            {
                if (!await _userManager.IsInRoleAsync(currentUser, "Admin"))
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
            var plan = new UserPlan(GuidGenerator.Create(), input.Name, input.DisplayName);
            await _planRepository.InsertAsync(plan);

            /*
            foreach (var feature in input.Features)
            {
                var featureName = feature.Key;
                var featureValue = feature.Value;

                await _featureManager.SetAsync(
                    name: featureName,
                    value: featureValue,
                    providerName: "P",
                    providerKey: plan.Id.ToString());
            }
            */

            // we need to get all the features of a plan and inseted.
            await _featureManager.SetAsync(
                    name: "Esh3arTech.PdfReporting", 
                    value: "true",
                    providerName: "P",
                    providerKey: plan.Id.ToString());
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
    }
}
