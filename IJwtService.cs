using System.Security.Claims;

namespace NovelProject
{
    public interface IJwtService
    {
        public string GenerateToken(IEnumerable<Claim> claims);
    }
}
