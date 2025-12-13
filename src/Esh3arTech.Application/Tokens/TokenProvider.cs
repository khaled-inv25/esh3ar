using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Tokens
{
    public class TokenProvider : ITokenProvider, ITransientDependency
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateTokenAsync(CreateTokenDto token)
        {
            var signinCredentials = GetSigningCredentials();
            var claims = await GetClaims(token.MobileNumber);
            var jwtTokenOption = GenerateTokenOptions(signinCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(jwtTokenOption);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SECRET"] ?? throw new ArgumentNullException("SecretKeyNull"));
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(string mobileNumber)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.MobilePhone, mobileNumber)
            };

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials credentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var tokenOption = new JwtSecurityToken
            (
             issuer: jwtSettings["validIssuer"],
             audience: jwtSettings["validAudience"],
             claims: claims,
             expires: DateTime.Now.AddMonths(Convert.ToInt16(jwtSettings["expires"])),
             signingCredentials: credentials
            );

            return tokenOption;
        }

    }
}
