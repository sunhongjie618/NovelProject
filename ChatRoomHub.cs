using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace NovelProject
{
    public class ChatRoomHub:Hub
    {
        private static readonly ConcurrentDictionary<long, List<string>> _Online = new();
        private readonly MyDbContext _dbc;
        private readonly ILogger<ChatRoomHub> _logger;

        public ChatRoomHub(MyDbContext dbc, ILogger<ChatRoomHub> logger)
        {
            _dbc = dbc;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = long.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var connectionId = Context.ConnectionId;


            _Online.AddOrUpdate(userId,
                opt => new List<string> { connectionId },
                (opt, list) =>
                {
                    list.Add(connectionId);
                    return list;
                });
            //推广未读消息
            var unreadMessage = await _dbc.Messages
                .Where(m => m.ToUserId == userId && !m.Isread)
                .OrderBy(m => m.CreateTime)
                .Select(m => new { m.FromUserId,m.FromUserName,m.CreateTime,m.Content })
                .ToListAsync();

            if (unreadMessage.Any())
            {
                await Clients.Caller.SendAsync("ReceiveUnreadMessage", unreadMessage);
            }



            var FavoriteNovelId = await _dbc.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.NovelId)
                .ToListAsync();

            foreach(var favNovelId in FavoriteNovelId)
            {
                await Groups.AddToGroupAsync(connectionId,$"novel_{favNovelId}");
            }

            _logger.LogInformation("用户:{UserId}已连接,ConnectionId:{ConnectionId}",userId,connectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = long.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var connectionId = Context.ConnectionId;

            if(_Online.TryGetValue(userId,out var connections))
            {
                connections.Remove(connectionId);


                if (connections.Count == 0)
                {
                    _Online.TryRemove(userId, out _);
                }
            }

            _logger.LogInformation("用户:{UserId}已断开连接,ConnectionId:{ConnectionId}", userId, connectionId);
            await base.OnDisconnectedAsync(exception);
        }

        //必须是静态方法,发送系统消息，消息长时间保留
        public static Task SendSystemNotification(IHubContext<ChatRoomHub> hubContext,string title,string content)
        {
            return hubContext.Clients.All.SendAsync("ReceiveSystemMessage", new { Title = title, Content = content,Time=DateTime.UtcNow });
        }
        //发送公屏或社区消息
        public async Task SendPublicMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 500)
            {
                await Clients.Caller.SendAsync("SendMessageError","消息不能为空,且消息长度不能超过500");
                return;
            }

            string name = Context.User.FindFirstValue(ClaimTypes.Name);
            string msg = $"{name}{DateTime.UtcNow}:{message}";
            await Clients.Others.SendAsync("ReceivePublicMessage", msg);
        }


        public async Task SendPrivateMessage(long toUserId,string message)
        {
            try
            {
                var fromUserId = long.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var fromUserName = Context.User.FindFirstValue(ClaimTypes.Name);


                if (string.IsNullOrEmpty(message) || message.Length > 1000)
                {
                    await Clients.Caller.SendAsync("SendPrivateEmptyMeaaage", "发送的信息不能为空,且长度一次不能超过1000");
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
                    FromUserName=fromUserName,
                    FromUserId = fromUserId
                };

                await _dbc.Messages.AddAsync(msg);
                await _dbc.SaveChangesAsync();

                if(_Online.TryGetValue(toUserId,out _)){
                    await Clients.User(toUserId.ToString()).SendAsync("ReceivePrivateMessage", new { fromUserId = msg.FromUserId, fromUserName = fromUserName, Message = msg.Content,MessageId=msg.MessageId });
                }

                await Clients.Caller.SendAsync("SendPrivateMessageSuccess", new { toUserId = toUserId, message = message, Time = DateTime.UtcNow });

                //await Task.CompletedTask;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "发送消息失败!");
                await Clients.Caller.SendAsync("SendPrivateMessageFailed", "消息发送失败!");
            }
        }

        //将消息标记为已读
        public async Task MarkMessageAsRead(long messageId)
        {
            var userId = long.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var message = await _dbc.Messages.FindAsync(messageId);

            if (message != null&&message.ToUserId==userId && message.Isread == false)
            {
                message.Isread = true;
                await _dbc.SaveChangesAsync();
            }
        }
        //给收藏指定小说的用户推送更新消息
        public static Task SendChapterUpdate(IHubContext<ChatRoomHub> hubContext,long chapterId,string title,long novelId,string novelName)
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
