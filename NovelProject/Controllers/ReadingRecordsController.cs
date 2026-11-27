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
    public class ReadingRecordsController : ControllerBase
    {
        private readonly MyDbContext ctx;

        public ReadingRecordsController(MyDbContext ctx)
        {
            this.ctx = ctx;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetReadingRecords([FromQuery] int page = 1, [FromQuery] int pageSize=20)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail("请先登录"));
            }

            var query = ctx.UserReadingRecords.AsNoTracking().Include(r => r.Novel)
                .Where(r => r.UserId == userId && !r.IsDeleted).OrderByDescending(r => r.LastReadTime);

            var total = await query.CountAsync();

            var readingRecords = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReadingRecordsList
                {
                    RecordId=r.RecordId,
                    NovelId=r.NovelId,
                    LastReadChapterId=r.LastReadChapterId,
                    LastReadTime=r.LastReadTime,
                    Title=r.Novel!.Title,
                    AuthorName=r.Novel.AuthorName,
                    CoverUrl=r.Novel.CoverUrl
                }).ToListAsync();

            var result = new
            {
                Total = total,
                Data = readingRecords,
                Page = page,
                PageSize = pageSize
            };

            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpDelete("{novelId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> DeleteSingleReadingRecord(long RecordId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<long>.Fail("请先登录"));
            }
            var record = await ctx.UserReadingRecords.Where(r => r.RecordId == RecordId && r.UserId == userId)
                .ExecuteDeleteAsync();
            if (record == 0)
                return NotFound(ApiResponse<long>.Fail("记录不存在或无权操作", 404));

            await ctx.SaveChangesAsync();
            return Ok(ApiResponse<long>.Success(RecordId));
        }


        [HttpGet("{novelId:long}/last")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object?>>> GetLastRead(long novelId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || !long.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<object?>.Fail("请先登录"));
            }

            // (UserId, NovelId) 上有唯一索引，所以最多一条；IsDeleted 已由全局过滤器处理
            var record = await ctx.UserReadingRecords
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.NovelId == novelId)
                .Select(r => new { r.RecordId, r.LastReadChapterId, r.LastReadTime })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<object?>.Success(record, record == null ? "还没有阅读记录" : "查询成功"));
        }
    }
}
