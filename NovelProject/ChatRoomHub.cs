using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NovelProject
{
    [Authorize]
    public class ChatRoomHub : Hub
    {
        private readonly MyDbContext _dbc;
        private readonly ILogger<ChatRoomHub> _logger;

        public ChatRoomHub(MyDbContext dbc, ILogger<ChatRoomHub> logger)
        {
            _dbc = dbc;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // [Authorize] 已保证连接已认证；NameIdentifier 即 JWT 中的 UserId
            if (!long.TryParse(Context.UserIdentifier, out var userId))
            {
                _logger.LogWarning("连接缺少有效用户标识,ConnectionId:{ConnectionId}", Context.ConnectionId);
                Context.Abort();
                return;
            }

            var connectionId = Context.ConnectionId;

            // 推送离线期间收到的未读私聊消息
            var unreadMessage = await _dbc.Messages
                .Where(m => m.ToUserId == userId && !m.IsRead)
                .OrderBy(m => m.CreateTime)
                .Select(m => new { m.FromUserId, m.FromUserName, m.CreateTime, m.Content })
                .ToListAsync();

            if (unreadMessage.Any())
            {
                await Clients.Caller.SendAsync("ReceiveUnreadMessage", unreadMessage);
            }

            // 连接自动加入其收藏小说的分组,用于章节更新定向推送
            var favoriteNovelIds = await _dbc.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.NovelId)
                .ToListAsync();

            foreach (var favNovelId in favoriteNovelIds)
            {
                await Groups.AddToGroupAsync(connectionId, $"novel_{favNovelId}");
            }

            _logger.LogInformation("用户:{UserId}已连接,ConnectionId:{ConnectionId}", userId, connectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // SignalR 会在连接断开时自动将连接从分组中移除,无需手动清理
            if (long.TryParse(Context.UserIdentifier, out var userId))
            {
                _logger.LogInformation("用户:{UserId}已断开连接,ConnectionId:{ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // 发送系统消息(由后台服务或控制器通过 IHubContext 调用)
        public static Task SendSystemNotification(IHubContext<ChatRoomHub> hubContext, string title, string content)
        {
            return hubContext.Clients.All.SendAsync("ReceiveSystemMessage", new { Title = title, Content = content, Time = DateTime.UtcNow });
        }

        // 发送公屏/社区消息
        public async Task SendPublicMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 500)
            {
                await Clients.Caller.SendAsync("SendMessageError", "消息不能为空,且消息长度不能超过500");
                return;
            }

            string name = Context.User.FindFirstValue(ClaimTypes.Name);
            string msg = $"{name}{DateTime.UtcNow}:{message}";
            await Clients.Others.SendAsync("ReceivePublicMessage", msg);
        }

        // 发送私聊消息
        public async Task SendPrivateMessage(long toUserId, string message)
        {
            if (!long.TryParse(Context.UserIdentifier, out var fromUserId))
            {
                await Clients.Caller.SendAsync("SendPrivateMessageFailed", "当前连接身份无效!");
                return;
            }

            var fromUserName = Context.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(message) || message.Length > 1000)
            {
                await Clients.Caller.SendAsync("SendPrivateEmptyMessage", "发送的信息不能为空,且长度一次不能超过1000");
                return;
            }

            if (fromUserId == toUserId)
            {
                await Clients.Caller.SendAsync("SendMessageFailed", "不能给自己发信息!");
                return;
            }

            var msg = new Message
            {
                ToUserId = toUserId,
                CreateTime = DateTime.UtcNow,
                Content = message,
                FromUserName = fromUserName,
                FromUserId = fromUserId,
                ConversationId = $"{Math.Min(fromUserId, toUserId)}_{Math.Max(fromUserId, toUserId)}"
            };

            // 先落库,保证离线消息不丢失
            await _dbc.Messages.AddAsync(msg);
            await _dbc.SaveChangesAsync();

            // 再推送:对方在线则实时收到;离线则下次连接时由 OnConnectedAsync 补发
            // 推送失败只记日志,不影响已落库的消息,也不诱导调用方重发(避免重复消息)
            try
            {
                await Clients.User(toUserId.ToString()).SendAsync("ReceivePrivateMessage", new
                {
                    fromUserId = msg.FromUserId,
                    fromUserName = fromUserName,
                    Message = msg.Content,
                    MessageId = msg.MessageId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "私聊消息已落库,但实时推送失败,MessageId:{MessageId}", msg.MessageId);
            }

            await Clients.Caller.SendAsync("SendPrivateMessageSuccess", new { toUserId = toUserId, message = message, Time = DateTime.UtcNow });
        }

        // 将消息标记为已读
        public async Task MarkMessageAsRead(long messageId)
        {
            if (!long.TryParse(Context.UserIdentifier, out var userId))
            {
                return;
            }

            var message = await _dbc.Messages.FindAsync(messageId);

            if (message != null && message.ToUserId == userId && message.IsRead == false)
            {
                message.IsRead = true;
                await _dbc.SaveChangesAsync();
            }
        }

        // 给收藏指定小说的用户推送章节更新消息
        public static Task SendChapterUpdate(IHubContext<ChatRoomHub> hubContext, long chapterId, string title, long novelId, string novelName)
        {
            return hubContext.Clients.Group($"novel_{novelId}").SendAsync("ReceiveChapterUpdateMessage", new
            {
                NovelId = novelId,
                NovelName = novelName,
                ChapterId = chapterId,
                Title = title
            });
        }
    }
}
