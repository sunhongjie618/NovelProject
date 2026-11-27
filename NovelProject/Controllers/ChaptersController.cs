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
    public class ChaptersController : ControllerBase
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(MyDbContext ctx, ILogger<ChaptersController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetChapters([FromQuery] long novelId, [FromQuery] int page = 1, [FromQuery] int pagesize = 50, [FromQuery] bool isDes=false)
        {
            var novelExist = await ctx.Novels.AnyAsync(n => n.NovelId == novelId);
            if (!novelExist)
            {
                return BadRequest(ApiResponse<object>.Fail("小说不存在!"));
            }
            var query = ctx.Chapters.Where(c => c.NovelId == novelId && c.IsDeleted == false)
                .AsNoTracking();

            query = isDes ? query.OrderByDescending(c => c.ChapterNumber) : query.OrderBy(c => c.ChapterNumber);

            var total = await query.CountAsync();//做分页统计

            var Chapters = await query.Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Select(n => new ChapterList
                {
                    ChapterId=n.ChapterId,
                    ChapterNumber=n.ChapterNumber,
                    Title=n.Title,
                    WordCount=n.WordCount,
                    PublishTime=n.PublishTime,
                    LastUpdateTime=n.UpdateTime,
                    IsLocked=n.IsLocked
                })
                .ToListAsync();

            var result = new
            {
                Total=total,
                Data=Chapters,
                Page=page,
                PageSize=pagesize,
                NovelId=novelId
            };

            return Ok(ApiResponse<object>.Success(result));
        }

        private async Task UpdateReadingRecordAsync(long userId, long novelId, long chapterId)
        {
            var readingRecord = await ctx.UserReadingRecords
                .FirstOrDefaultAsync(r => r.UserId == userId && r.NovelId == novelId && !r.IsDeleted);

            if (readingRecord != null)
            {
                readingRecord.LastReadChapterId = chapterId;
                readingRecord.LastReadTime = DateTime.UtcNow;
            }
            else
            {
                var newRecord = new UserReadingRecords
                {
                    UserId = userId,
                    NovelId = novelId,
                    LastReadChapterId = chapterId,
                    LastReadTime = DateTime.UtcNow
                };
                ctx.UserReadingRecords.Add(newRecord);
            }

            await ctx.SaveChangesAsync();
        }


        [HttpGet("{chapterId:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ChapterDetail>>> GetChaptersDetail(long chapterId, [FromQuery] long novelId)
        {
            // 1. 获取当前登录用户ID
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            //获取章节细节的时候存在两个写操作，需要开启事务
            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var chapter = await ctx.Chapters.AsNoTracking().FirstOrDefaultAsync(c => c.ChapterId == chapterId&&!c.IsDeleted);
                if (chapter == null)
                {
                    return BadRequest(ApiResponse<ChapterDetail>.Fail("章节不存在或已删除！"));
                }
                //数据一致性校验
                if (chapter.NovelId != novelId)
                {
                    return BadRequest(ApiResponse<ChapterDetail>.Fail("章节不属于该小说！"));
                }

                //为什么用原生SQL语句更新，不用EF Core实体更新:数据库会把这条 SQL 语句作为一个不可分割的整体执行，无论多少个用户同时点击，都会正确累加。


                // 原子更新小说点击量
                await ctx.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE T_Novels SET ClickCount = ClickCount + 1 WHERE NovelId = {chapter.NovelId}");


                //  自动更新阅读记录,安全解析userId,防止被篡改时抛出异常
                if (userClaim != null&& long.TryParse(userClaim.Value, out var userId))
                {
                    await UpdateReadingRecordAsync(userId, chapter.NovelId, chapterId);
                }

                await transaction.CommitAsync();

                //跳转到上一章
                var preChapterId = await ctx.Chapters
                    .Where(c => c.NovelId == chapter.NovelId && c.ChapterNumber < chapter.ChapterNumber&&!c.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(c => c.ChapterNumber)
                    .Select(c => c.ChapterId)
                    .FirstOrDefaultAsync();

                //跳转到下一章
                var nextChapterId = await ctx.Chapters
                    .Where(c => c.NovelId == chapter.NovelId && c.ChapterNumber > chapter.ChapterNumber&&!c.IsDeleted)
                    .AsNoTracking()
                    .OrderBy(c => c.ChapterNumber)
                    .Select(c => c.ChapterId)
                    .FirstOrDefaultAsync();


                var dto = new ChapterDetail
                {
                    ChapterId = chapter.ChapterId,
                    NovelId = chapter.NovelId,
                    ChapterNumber = chapter.ChapterNumber,
                    Title = chapter.Title,
                    Content = chapter.Content,
                    WordCount = chapter.WordCount,
                    PublishTime = chapter.PublishTime,
                    UpdateTime = chapter.UpdateTime,
                    IsLocked = chapter.IsLocked,
                    PreChapterId = preChapterId == 0 ? null : preChapterId,//如果查出来为0，则赋值为Null，返回给前端语义清晰，否则赋原值
                    NextChapterId = nextChapterId == 0 ? null : nextChapterId

                };
                return Ok(ApiResponse<ChapterDetail>.Success(dto));
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "获取章节详细失败!章节ID{chapterId},小说ID{NovelId}", chapterId, novelId);
                return BadRequest(ApiResponse<ChapterDetail>.Fail("获取小说详细失败，请稍后再试!"));
            }           
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> AddChapter([FromBody] AddChapterRequest request)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var novel = await ctx.Novels.FirstOrDefaultAsync(n => n.NovelId == request.NovelId);
                if (novel == null)
                {
                    return BadRequest(ApiResponse<long>.Fail("小说不存在!"));
                }
                if (novel.AuthorId != userId)
                {
                    return BadRequest(ApiResponse<long>.Fail("无权限!"));
                }

                // 直接使用小说的ChapterCount+1作为新章节号
                int newChapterNumber = novel.ChapterCount + 1;

                //计算章节字数
                var wordCount = request.Content.Replace("\r", "").Replace("\n", "").Replace(" ", "").Length;
                var chapter = new Chapters
                {
                    NovelId = request.NovelId,
                    ChapterNumber = newChapterNumber,
                    WordCount = wordCount,
                    Title = request.Title,
                    Content = request.Content,
                    IsLocked = request.IsLocked,
                    PublishTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                };

                ctx.Chapters.Add(chapter);
                await ctx.SaveChangesAsync();

                novel.LastUpdateTime = DateTime.UtcNow;
                novel.ChapterCount = novel.ChapterCount + 1;
                novel.WordCount = novel.WordCount + wordCount;
                await ctx.SaveChangesAsync();

                
                await transaction.CommitAsync();
                _logger.LogInformation("章节添加成功!小说ID{NovelId},章节ID{chapterId}", request.NovelId, chapter.ChapterId);
                return Ok(ApiResponse<long>.Success(chapter.ChapterId, "章节添加成功!"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                _logger.LogError(ex, "章节添加失败!小说ID{NovelId}", request.NovelId);
                return BadRequest(ApiResponse<long>.Fail(
                    $"添加失败：{ex.Message}\n" +
                    $"内部异常：{ex.InnerException?.Message}\n" +
                    $"堆栈跟踪：{ex.StackTrace}"
                ));
            }
        }


        [HttpDelete("{chapterId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> DeleteChapter(long chapterId, [FromQuery] long novelId)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                var chapter = await ctx.Chapters
                    .FirstOrDefaultAsync(c => c.ChapterId == chapterId && c.NovelId == novelId && !c.IsDeleted);

                if (chapter == null)
                {
                    return BadRequest(ApiResponse<long>.Fail("章节不存在或已被删除!"));
                }

                var novel = await ctx.Novels.FirstOrDefaultAsync(n => n.NovelId == novelId);
                if (novel == null || novel.AuthorId != userId)
                {
                    return BadRequest(ApiResponse<long>.Fail("无权限!"));
                }

                //被删除的章节的Number改为负的
                chapter.ChapterNumber = -(int)chapter.ChapterId; 
                chapter.IsDeleted = true;
                chapter.UpdateTime = DateTime.UtcNow;
                await ctx.SaveChangesAsync();

                
                novel.ChapterCount = Math.Max(0, novel.ChapterCount - 1);
                novel.WordCount = Math.Max(0, novel.WordCount - chapter.WordCount);
                novel.LastUpdateTime = DateTime.UtcNow;
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("章节删除成功!小说ID{NovelId},章节ID{chapterId}", novelId, chapterId);
                return Ok(ApiResponse<long>.Success(chapterId, "章节删除成功!"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "删除章节失败!小说ID{NovelId}章节ID{chapterId}", novelId, chapterId);
                return BadRequest(ApiResponse<long>.Fail($"删除失败：{ex.Message}"));
            }
        }
    }
}
