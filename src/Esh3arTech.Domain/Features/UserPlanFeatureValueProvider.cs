using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Esh3arTech.Features
{
    public class UserPlanFeatureValueProvider : FeatureValueProvider, ITransientDependency
    {
        public override string Name => "UserPlan";

        private readonly ICurrentUser _currentUser;
        private readonly IIdentityUserRepository _userRepository;

        public UserPlanFeatureValueProvider(
            IFeatureStore featureStore, 
            ICurrentUser currentUser, 
            IIdentityUserRepository userRepository) : base(featureStore)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
        }

        public override async Task<string?> GetOrNullAsync(FeatureDefinition feature)
        {
            // To check if user is not logged in.
            if (!_currentUser.Id.HasValue)
            {
                return null;
            }

            var user = await _userRepository.GetAsync(_currentUser.Id.Value);

            if (user is null)
            {
                return null;
            }
            
            var planId = user.GetProperty<Guid?>("PlanId");

            if (!planId.HasValue)
            {
                return null;
            }

            return await FeatureStore.GetOrNullAsync(feature.Name, "P", planId.ToString());
        }
    }
}
