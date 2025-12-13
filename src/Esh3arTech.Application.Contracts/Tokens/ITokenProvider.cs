using System.Threading.Tasks;

namespace Esh3arTech.Tokens
{
    public interface ITokenProvider
    {
        Task<string> CreateTokenAsync(CreateTokenDto token);
    }
}
