using System.Threading.Tasks;
using Volo.Abp.Content;
using Volo.Abp.Features;

namespace Esh3arTech.Sandbox
{
    public class SandboxAppService : Esh3arTechAppService
    {
        private readonly IFeatureChecker _featureChecker;

        public SandboxAppService(IFeatureChecker featureChecke)
        {
            _featureChecker = featureChecke;
        }

        //public async Task<string> Test()
        //{
        //    var isEnabled = await _featureChecker.IsEnabledAsync("Esh3arTech.PdfReporting");
        //    var maxProductCountLimit = await _featureChecker.GetAsync<int>("Esh3arTech.MaxMessages");

        //    return "Hello from SandboxAppService";
        //}

        //public string Test(SandboxDto input, RemoteStreamContent stream)
        //{
        //    return "test";
        //}
    }
}