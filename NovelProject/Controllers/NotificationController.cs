using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NovelProject.DTO;
using System.Security.Claims;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        public IHubContext<ChatRoomHub> _hub;
        private readonly ILogger<NotificationController> _logger;
        private readonly MyDbContext ctx;

        public NotificationController(IHubContext<ChatRoomHub> hub, ILogger<NotificationController> logger, MyDbContext ctx)
        {
            _hub = hub;
            _logger = logger;
            this.ctx = ctx;
        }

        [HttpPost("sitewidebroadcast")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> SiteWideBroadcast([FromBody] BroadcastRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Content))
                return BadRequest(ApiResponse<object>.Fail("标题和内容不能为空!"));

            await ChatRoomHub.SendSystemNotification(_hub,req.Title,req.Content);
            return Ok(ApiResponse<object>.Success(new { req.Title, req.Content },"系统广播已推送"));

        }

        //仅供管理员后台发送公屏消息使用,后期会加限制
        [HttpPost("publicmessage")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> SendAnnouncement([FromBody] AnnouncementRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.message))
                return BadRequest(ApiResponse<string>.Fail("内容不能为空!"));
            if (req.message.Length > 500)
                return BadRequest(ApiResponse<string>.Fail("消息长度不能超过 500！"));


            var from = User.FindFirst("NickName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "系统";
            var text = $"{from} {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {req.message.Trim()}";

            await _hub.Clients.All.SendAsync("ReceivePublicMessage", text);
            return Ok(ApiResponse<string>.Success("公屏消息已发送"));
        }


    }
}
