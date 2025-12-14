using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Features;

namespace Esh3arTech.Sandbox
{
    public class SandboxAppService : Esh3arTechAppService
    {
        private readonly IFeatureChecker _featureChecker;

        private readonly IAccountAppService _accountAppService;

        public SandboxAppService(
            IFeatureChecker featureChecke, 
            IAccountAppService accountAppService)
        {
            _featureChecker = featureChecke;
            _accountAppService = accountAppService;
        }

        public async Task<string> Test()
        {
            new RegisterDto()
            {
                
            };
            var isEnabled = await _featureChecker.IsEnabledAsync("Esh3arTech.PdfReporting");
            var maxProductCountLimit = await _featureChecker.GetAsync<int>("Esh3arTech.MaxMessages");

            return "Hello from SandboxAppService";
        }
    }
}
