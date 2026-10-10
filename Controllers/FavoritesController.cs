using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelProject.DTO;
using System.ComponentModel.Design;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<FavoritesController> _logger;


        public FavoritesController(MyDbContext ctx, ILogger<FavoritesController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }


        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ToggeFavoriteResult>>> TiggleFavorites([FromBody] AddFavorityRequest request)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<ToggeFavoriteResult>.Fail("请先登录"));
            }

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var novel = await ctx.Novels.AsNoTracking().FirstOrDefaultAsync(n => n.NovelId == request.NovelId);
                if (novel == null)
                {
                    return BadRequest(ApiResponse<ToggeFavoriteResult>.Fail("小说不存在!"));
                }

                var existingFavority = await ctx.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId&&f.NovelId==request.NovelId);

                bool isFavorite;
                int newFavoriteCount;

                if (existingFavority == null)
                {
                    var favorite = new UserFavorites
                    {
                        UserId=userId,
                        NovelId=request.NovelId,
                        CreateTime=DateTime.UtcNow
                    };

                    ctx.UserFavorites.Add(favorite);
                    //解决并发问题，统计小说的总收藏数
                    await ctx.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE T_Novels SET FavoriteCount = FavoriteCount + 1 WHERE NovelId = {request.NovelId}");

                    isFavorite = true;
                    newFavoriteCount = novel.FavoriteCount + 1;
                }
                else
                {
                    ctx.UserFavorites.Remove(existingFavority);
                    await ctx.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE T_Novels SET FavoriteCount = CASE WHEN FavoriteCount > 0 THEN FavoriteCount - 1 ELSE 0 END WHERE NovelId = {request.NovelId}");
                    isFavorite = false;
                    newFavoriteCount = Math.Max(novel.FavoriteCount - 1, 0);
                }

                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                var result = new ToggeFavoriteResult
                {
                    isFavorited=isFavorite,
                    FavoriteCount=newFavoriteCount
                };

                return Ok(ApiResponse<ToggeFavoriteResult>.Success(result, isFavorite ? "收藏成功!" : "取消收藏成功!"));
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "收藏/取消收藏失败!小说ID{NovelId}", request.NovelId);
                return BadRequest(ApiResponse<ToggeFavoriteResult>.Fail("操作失败,请稍后重试!"));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 15, [FromQuery] bool sortBy=false)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail("请先登录"));
            }

            var query = ctx.UserFavorites.AsNoTracking().Include(f=>f.Novel).Where(f => f.UserId == userId);

            var total = await query.CountAsync();

            query = sortBy ? query.OrderByDescending(f => f.Novel!.FavoriteCount) : query.OrderByDescending(f => f.CreateTime);

            var favorites = await query.Skip((page-1)*pageSize)
                .Take(pageSize)
                .Select(c => new FavoriteList
            {
                FavoriteId=c.FavoriteId,
                NovelId=c.NovelId,
                UserId=c.UserId,
                NovelName=c.Novel!.Title,
                CoverUrl=c.Novel.CoverUrl,
                Author=c.Novel.AuthorName,
                Description=c.Novel.Description,
                FavoriteCount=c.Novel.FavoriteCount,
                CreateTime=c.CreateTime
            }).ToListAsync();


            var result = new
            {
                Total = total,
                Data = favorites,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy
            };


            return Ok(ApiResponse<object>.Success(result));
        }


        //单独设置的一个取消收藏的方法，适合在用户的收藏列表中，直接取消收藏
        [HttpPost("remove")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<int>>> RemoveFavorite([FromBody] RemoveFavoriteRequest request)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<int>.Fail("请先登录"));
            }

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var favorite = await ctx.UserFavorites.FirstOrDefaultAsync(f => f.FavoriteId == request.FavoriteId && f.UserId == userId);
                if (favorite == null)
                {
                    return BadRequest(ApiResponse<int>.Fail("收藏记录不存在!"));
                }


                ctx.UserFavorites.Remove(favorite);

                await ctx.Database.ExecuteSqlInterpolatedAsync(
                       $"UPDATE T_Novels SET FavoriteCount = CASE WHEN FavoriteCount > 0 THEN FavoriteCount - 1 ELSE 0 END WHERE NovelId = {favorite.NovelId}");

                await ctx.SaveChangesAsync();
                await transaction.CommitAsync();

                var newFavoriteCount = await ctx.Novels.AsNoTracking().Where(n => n.NovelId == favorite.NovelId)
                    .Select(n => n.FavoriteCount)
                    .FirstAsync();

                return Ok(ApiResponse<int>.Success(newFavoriteCount,"取消收藏成功!"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "取消收藏失败!收藏ID{FavoriteId}", request.FavoriteId);
                return BadRequest(ApiResponse<int>.Fail("取消收藏失败!"));
            }
        }
    }
}
