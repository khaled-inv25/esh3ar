using System.Threading.Tasks;

namespace Esh3arTech.Registrations
{
    public interface IRegistretionAppService
    {
        Task<RegisterOutputDto> RegisterAsync(RegisterRequestDto input);
    }
}
