using System.Threading.Tasks;

namespace Esh3arTech.Registrations
{
    public interface IRegistretionAppService
    {
        Task<RegisterOutputDto> RegisterAsync(RegisterRequestDto input);

        Task<TokenDto> VerifyOtpAsync(VerifyOtpRequestDto input);

        Task ResendOtpAsync(ResendOtpRequestDto input);
    }
}
