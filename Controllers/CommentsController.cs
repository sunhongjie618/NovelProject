using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelProject.DTO;
using System.Security.Claims;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(MyDbContext ctx, ILogger<CommentsController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetComments([FromQuery]long novelId,[FromQuery]long? chapterId, [FromQuery]int page = 1, [FromQuery] int pageSize = 20, [FromQuery]bool sortBy=true)
        {
            long? CurrentUserId = null;
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userClaim!=null&&long.TryParse(userClaim.Value,out long userId))
            {
                CurrentUserId = userId;
            }


            var novelExist = await ctx.Novels.AnyAsync(c => c.NovelId == novelId);
            if (!novelExist)
            {
                return BadRequest(ApiResponse<object>.Fail("小说不存在!"));
            }

            var query = ctx.NovelComments
                .Include(c => c.User)
                .Where(c => c.NovelId == novelId)
                .AsNoTracking();

            if (chapterId.HasValue)
            {
                query = query.Where(c => c.ChapterId == chapterId);
            }
            //默认按时间倒序排，SorderBy如果是false，则按照点赞数倒叙排,如果还有更多的选择，则改sortBy为string类型,用switch来完成
            query = sortBy ? query.OrderByDescending(c => c.CreateTime) : query.OrderByDescending(c => c.LikeCount).ThenByDescending(c => c.CreateTime);

            var total = await query.CountAsync();//作分页统计

            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 7. 批量查询当前用户在这本小说下的所有点赞记录（只查一次，单表查询）
            HashSet<long> likedCommentIds = new HashSet<long>();
            if (CurrentUserId.HasValue)
            {
                //存疑? 

                //likedCommentIds = await ctx.CommentLikes
                //    .Where(l => l.UserId == CurrentUserId.Value && l.NovelId == novelId)
                //    .Select(l => l.CommentId)
                //    .ToHashSetAsync();

                //解决N+1性能问题
                var likedCommentIdList = await ctx.CommentLikes
                    .Where(l => l.UserId == CurrentUserId.Value && l.NovelId == novelId)
                    .Select(l => l.CommentId)
                    .ToListAsync();
                //.ToHashSetAsync();//这个在EF core 5++时才生效
                likedCommentIds = new HashSet<long>(likedCommentIdList);
            }

            var commentsDTO = comments.Select(c => new CommentsList
            {
                CommentId=c.CommentId,
                UserId=c.UserId,
                UserNickName = c.User?.NickName ?? "匿名用户",
                UserAvatarUrl = c.User?.AvatarUrl?? "https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png",
                ChapterId=c.ChapterId,
                Content=c.Content,
                CreateTime=c.CreateTime,
                LikeCount=c.LikeCount,
                IsLiked = CurrentUserId.HasValue && likedCommentIds.Contains(c.CommentId)

            }).ToList();

            var result = new
            {
                Total=total,
                Data=commentsDTO,
                Page=page,
                PageSize=pageSize,
                NovelId=novelId,
                ChapterId=chapterId,
                SortBy=sortBy
            };

            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<long>>> AddComment([FromBody] AddCommentRequest request)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            long? userId = null;
            if (userClaim != null)
            {
                userId = long.Parse(userClaim!.Value);
            }

            var novelsExist = await ctx.Novels.AnyAsync(c => c.NovelId == request.NovelId);
            if (!novelsExist)
            {
                return BadRequest(ApiResponse<long>.Fail("小说不存在!"));
            }
            //验证章节是否存在且属于该小说
            if (request.ChapterId.HasValue)
            {
                var chapterExist = await ctx.Chapters.AnyAsync(c => c.ChapterId == request.ChapterId.Value && c.NovelId == request.NovelId);
                if (!chapterExist)
                {
                    return BadRequest(ApiResponse<long>.Fail("章节不存在或不属于该小说")); ;
                }
            }

            var comment = new NovelComments
            {
                NovelId = request.NovelId,
                ChapterId = request.ChapterId,
                UserId = userId,
                Content = request.Content,
                CreateTime = DateTime.UtcNow,
                LikeCount = 0
            };

            ctx.NovelComments.Add(comment);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("评论发表成功!评论者ID{UserId},小说ID{NovelId},评论ID{CommentId}",userId,request.NovelId,comment.CommentId); ;
            return Ok(ApiResponse<long>.Success(comment.CommentId, "评论发表成功!"));
        }


        [HttpPost("{commentId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ToggleLikeResult>>> ToggleLike(long commentId)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if(userClaim==null||!long.TryParse(userClaim.Value,out var userId))
            {
                return Unauthorized(ApiResponse<ToggleLikeResult>.Fail("请先登录"));
            }

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var comment = await ctx.NovelComments.FirstOrDefaultAsync(c=>c.CommentId==commentId);

                if (comment == null)
                {
                    return BadRequest(ApiResponse<ToggleLikeResult>.Fail("评论不存在!"));
                }

                var existingLike = await ctx.CommentLikes.FirstOrDefaultAsync(l=>l.UserId==userId&&l.CommentId==commentId);

                bool isLike;
                int newLikeCount;

                if (existingLike == null)
                {
                    var like = new CommentLike
                    {
                        UserId=userId,
                        CommentId=commentId,
                        NovelId = comment.NovelId
                    };

                    ctx.CommentLikes.Add(like);
                    // 原子更新点赞数（解决并发问题，不会丢失点赞）
                    await ctx.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE T_NovelComments SET LikeCount = LikeCount + 1 WHERE CommentId = {commentId}");

                    isLike = true;
                    newLikeCount = comment.LikeCount + 1;
                }
                else
                {
                    ctx.CommentLikes.Remove(existingLike);
                    await ctx.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE T_NovelComments SET LikeCount = CASE WHEN LikeCount > 0 THEN LikeCount - 1 ELSE 0 END WHERE CommentId = {commentId}");
                    isLike = false;
                    newLikeCount = Math.Max(comment.LikeCount - 1, 0);
                }

                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                var result = new ToggleLikeResult
                {
                    IsLike=isLike,
                    LikeCount=newLikeCount
                };

                return Ok(ApiResponse<ToggleLikeResult>.Success(result,isLike ? "点赞成功":"取消点赞成功"));
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "点赞失败!点赞用户ID{UserId},评论ID{commentId}",userId,commentId);
                return BadRequest(ApiResponse<ToggleLikeResult>.Fail("操作失败,请稍后重试!"));
            }
        }
    }
}
