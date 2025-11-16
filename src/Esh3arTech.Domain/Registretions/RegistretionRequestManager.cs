using Esh3arTech.MobileUsers;
using Esh3arTech.RegistrationRequests;
using Esh3arTech.Settings;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace Esh3arTech.Registretions
{
    public class RegistretionRequestManager : DomainService
    {
        private readonly IRepository<MobileUser, Guid> _mobilUserRepository;
        private readonly IRepository<RegistretionRequest, Guid> _registretionRequestRepository;
        private readonly ISettingProvider _settingProvider;

        public RegistretionRequestManager(
            IRepository<MobileUser, Guid> mobilUserRepository,
            ISettingProvider settingProvider,
            IRepository<RegistretionRequest, Guid> registretionRequestRepository)
        {
            _mobilUserRepository = mobilUserRepository;
            _settingProvider = settingProvider;
            _registretionRequestRepository = registretionRequestRepository;
        }

        public async Task<RegistretionRequest?> CreateRegistretionRequestAsync(OS oS, string secret, Guid mobileUserId)
        {
            return new RegistretionRequest(GuidGenerator.Create(), oS, secret, mobileUserId);
        }

        public async Task<RegistretionRequest?> GetLastNotVerifiedRequest(MobileUser mobileUser)
        {
            var minutes = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.CodeTimeout);
            var LastDateTime = Clock.Now.AddSeconds(-minutes);

            var querable = await _registretionRequestRepository.GetQueryableAsync();

            var query = from reg in querable
                        where reg.MobileUserId == mobileUser.Id
                        && !reg.Verified
                        && reg.CreationTime >= LastDateTime
                        orderby reg.CreationTime descending
                        select reg;

            return await AsyncExecuter.FirstOrDefaultAsync(query);
        }

        public async Task<RegistretionRequest> IsRegistrationRequestRelatedToMobileNumber(Guid registrationRequestId)
        {
            var queryable = await _registretionRequestRepository.WithDetailsAsync(r => r.MobileUser);
            var registrationRequest = await AsyncExecuter.SingleAsync(queryable.Where($"Id == @0", registrationRequestId));

            return registrationRequest is null ? throw new UserFriendlyException("Invalid registration request id") : registrationRequest;
        }
    }
}