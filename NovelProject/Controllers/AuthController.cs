using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NovelProject.DTO;
using System.Security.Claims;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtService jwtService;
        private readonly MyDbContext ctx;

        public AuthController(UserManager<User> userManager, ILogger<AuthController> logger, IJwtService jwtService, MyDbContext ctx)
        {
            this.userManager = userManager;
            this._logger = logger;
            this.jwtService = jwtService;
            this.ctx = ctx;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register([FromBody]RegisterRequest request)
        {
            string UserName = request.UserName;
            string NickName = request.NickName ?? UserName;//如果为空则默认设置为UserName
            string Password = request.Password;

            var user1 = await userManager.FindByNameAsync(UserName);
            if (user1 != null)
            {
                
                return BadRequest(ApiResponse<string>.Fail("账号已存在!"));
            }
            else
            {
                user1 = new User { UserName = UserName, NickName = NickName, RegisterTime = DateTime.UtcNow };
                var r = await userManager.CreateAsync(user1, Password);
                if (!r.Succeeded)
                {
                    //记录注册失败的原因
                    var errors = string.Join(", ", r.Errors.Select(e => e.Description));
                    _logger.LogWarning("用户 {UserName} 注册失败，原因：{Errors}", UserName, errors);
                    return BadRequest(ApiResponse<string>.Fail($"注册失败,请稍后重试!"));
                }
                else
                {
                    _logger.LogInformation("用户{UserName}注册成功!",UserName);
                    return Ok(ApiResponse<string>.Success("注册成功!欢迎您的访问"));
                }
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> TLogin([FromBody]LoginRequest request)
        {
            string UserName = request.UserName;
            string Password = request.Password;

            var user2 = await userManager.FindByNameAsync(UserName);
            if (user2 == null||user2.IsDeleted)
            {
                return BadRequest(ApiResponse<LoginResponse>.Fail("账号或密码错误!"));
            }
            if(await userManager.IsLockedOutAsync(user2))
            {
                return BadRequest(ApiResponse<LoginResponse>.Fail("账号已锁定!"));
            }

            var result = await userManager.CheckPasswordAsync(user2, Password);
            if (result)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString()));
                claims.Add(new Claim("NickName", user2.NickName ?? user2.UserName));
                claims.Add(new Claim(ClaimTypes.Name, user2.UserName));

                var roles = await userManager.GetRolesAsync(user2);
                await userManager.ResetAccessFailedCountAsync(user2);
                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                string jwt = jwtService.GenerateToken(claims);
                user2.LastLoginTime = DateTime.UtcNow;
                await ctx.SaveChangesAsync();
                var response = new LoginResponse
                {
                    Token = jwt,
                    UserId = user2.Id,
                    NickName = user2.NickName??user2.UserName!
                };
                // 返回结构化的JSON，前端可以直接解析拿到Token和提示信息
                return Ok(ApiResponse<LoginResponse>.Success(response,"登录成功欢迎!"));
            }
            else
            {
                await userManager.AccessFailedAsync(user2);
                int n = await userManager.GetAccessFailedCountAsync(user2);
                return BadRequest(ApiResponse<LoginResponse>.Fail($"密码错误!当前错误次数{n}"));
            }
        }
    }
}
