using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NovelProject
{
    public class JwtService:IJwtService
    {
        private readonly IOptionsSnapshot<JwtSettings> jwtsettings;

        public JwtService(IOptionsSnapshot<JwtSettings> jwtsettings)
        {
            this.jwtsettings = jwtsettings;
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var settings = jwtsettings.Value;

            string key = jwtsettings.Value.SecKey;
            DateTime expire = DateTime.UtcNow.AddSeconds(jwtsettings.Value.ExpireSeconds);
            byte[] secBytes = Encoding.UTF8.GetBytes(key);
            var seckey = new SymmetricSecurityKey(secBytes);
            var credentials = new SigningCredentials(seckey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: expire,
                signingCredentials: credentials);
            string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return jwt;
        }
    }
}
