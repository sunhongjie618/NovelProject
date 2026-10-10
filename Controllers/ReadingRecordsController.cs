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
    }
}
