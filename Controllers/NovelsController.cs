using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelProject.DTO;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelsController : ControllerBase
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<NovelsController> _logger;

        public NovelsController(MyDbContext ctx, ILogger<NovelsController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetNovels([FromQuery] long? categoryId, [FromQuery] string? keyword, [FromQuery] int page=1, [FromQuery] int pagesize = 10, [FromQuery] bool sortBy = false, [FromQuery] bool? isCompleted=null, [FromQuery] string? sortType=null)
        {
            var query =  ctx.Novels.Include(n => n.Category).AsNoTracking().AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(n => n.CategoryId == categoryId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(n => n.Title.Contains(keyword) || n.AuthorName.Contains(keyword));
            }

            if (isCompleted == true)
            {
                query = query.Where(n => n.Status == 2);
            }



            if (sortType=="click")
            {
                query = query.OrderByDescending(n => n.ClickCount);
            }
            else if (sortType == "favorite")
            {
                // 排行：按收藏数降序
                query = query.OrderByDescending(n => n.FavoriteCount);
            }
            else if (sortType == "update")
            {
                // 按更新时间降序
                query = query.OrderByDescending(n => n.LastUpdateTime);
            }
            else
            {
                // 优化用户的体验
                query = sortBy ? query.OrderByDescending(n => n.FavoriteCount) : query.OrderByDescending(n => n.LastUpdateTime);
            }

            var total = await query.CountAsync();//分页统计


            var novels = await query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Select(n => new NovelList
                {
                    NovelId = n.NovelId,
                    Title=n.Title,
                    AuthorName=n.AuthorName,
                    CoverUrl=n.CoverUrl,
                    Description=n.Description,
                    Status=n.Status,
                    WordCount=n.WordCount,
                    CategoryName=n.Category!.CategoryName,
                    LastUpdateTime=n.LastUpdateTime,
                    FavoriteCount=n.FavoriteCount,
                    ClickCount=n.ClickCount

                })
                .ToListAsync();


            var result = new { Total = total, Data = novels, Page = page, Pagesize = pagesize };
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> AddNovel([FromBody] AddNovelRequest request)
        {
            // 1. 获取当前登录用户ID
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var nickName = User.FindFirst("NickName")?.Value ?? User.FindFirst(ClaimTypes.Name)!.Value;

            var CategoriesExist =  await ctx.Categories.AnyAsync(n => n.CategoryId == request.CategoryId);
            //判断添加的小说的种类是否存在
            if (!CategoriesExist)
            {
                return BadRequest(ApiResponse<long>.Fail("小说分类不存在!"));
            }
            var NovelExist = await ctx.Novels.AnyAsync(n => n.Title == request.Title);
            if (NovelExist)
            {
                return BadRequest(ApiResponse<long>.Fail("小说名已被使用!"));
            }
            var novel = new Novels
            {
                CategoryId=request.CategoryId,
                Title=request.Title,
                Status=1,
                CoverUrl=request.CoverUrl,
                Description=request.Description,
                AuthorId=userId,
                AuthorName=nickName
            };
            novel.PublishTime = DateTime.UtcNow;

            ctx.Novels.Add(novel);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("添加小说成功!作者ID{AuthorId},小说ID{NovelId}", userId, novel.NovelId);
            return Ok(ApiResponse<long>.Success(novel.NovelId, "添加成功!"));
        }

        [HttpDelete("{novelId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> DeleteNovel(long novelId)
        {
            // 1. 获取当前登录用户ID
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var novel = await ctx.Novels.FirstOrDefaultAsync(n => n.NovelId == novelId && n.AuthorId == userId);
            if (novel == null)
            {
                return BadRequest(ApiResponse<long>.Fail("小说不存在或无权限，删除失败!"));
            }
            novel.IsDeleted =true;
            await ctx.SaveChangesAsync();
            return Ok(ApiResponse<long>.Success(novelId));
        }
        [HttpPut("{novelId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> UpdateNovel(long novelId, [FromBody]UpdateNovelRequest request)
        {
            // 1. 获取当前登录用户ID
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var novel = await ctx.Novels.FirstOrDefaultAsync(n => n.NovelId == novelId);
            if (novel == null)
            {
                return BadRequest(ApiResponse<long>.Fail("小说不存在!"));
            }

            if (novel.AuthorId != userId)
            {
                return BadRequest(ApiResponse<long>.Fail("无权限修改小说"));
            }

            if (request.CategoryId.HasValue)
            {
                var CategoryExist = await ctx.Categories.AnyAsync(n => n.CategoryId == request.CategoryId.Value);
                if (!CategoryExist)
                {
                    return BadRequest(ApiResponse<long>.Fail("不存在这种小说分类!"));
                }
                novel.CategoryId = request.CategoryId.Value;
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                novel.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.CoverUrl))
            {
                novel.CoverUrl = request.CoverUrl;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                novel.Description = request.Description;
            }

            if (request.Status.HasValue)
            {
                if (request.Status.Value < 1 || request.Status.Value > 3)
                {
                    return BadRequest(ApiResponse<long>.Fail("不存在这种状态!"));
                }
                novel.Status = request.Status.Value;
            }

            novel.LastUpdateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            return Ok(ApiResponse<long>.Success(novelId, "修改成功!"));
        }


        [HttpGet("{novelId:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<NovelDetail>>> GetNovelDetail(long novelId)
        {
            
            try
            {
                var novelDetail = await ctx.Novels.Include(c => c.Category).AsNoTracking().FirstOrDefaultAsync(c => c.NovelId == novelId&&!c.IsDeleted);
                if (novelDetail == null)
                {
                    return BadRequest(ApiResponse<NovelDetail>.Fail("小说不存在!"));
                }

                // 原子更新点击量（解决并发点击丢失问题）
                // 直接用SQL语句更新，是原子操作，不会出现多个用户同时点击只加1的问题
                await ctx.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE T_Novels SET ClickCount = ClickCount + 1 WHERE NovelId = {novelId}");

                

                var dto = new NovelDetail
                {
                    NovelId=novelDetail.NovelId,
                    CategoryId=novelDetail.CategoryId,
                    Title=novelDetail.Title,
                    AuthorName=novelDetail.AuthorName,
                    CategoryName = novelDetail.Category?.CategoryName ?? "未知分类",
                    CoverUrl=novelDetail.CoverUrl,
                    Description=novelDetail.Description,
                    Status=novelDetail.Status,
                    WordCount=novelDetail.WordCount,
                    ChapterCount=novelDetail.ChapterCount,
                    PublishTime=novelDetail.PublishTime,
                    LastUpdateTime=novelDetail.LastUpdateTime,
                    ClickCount=novelDetail.ClickCount+1,
                    FavoriteCount=novelDetail.FavoriteCount
                };

                return Ok(ApiResponse<NovelDetail>.Success(dto));
            }
            catch(Exception ex)
            {
                
                _logger.LogError(ex, "获取小说详细页面失败!小说ID{NovelId}", novelId);
                return BadRequest(ApiResponse<NovelDetail>.Fail("获取小说详细页面失败!"));
            }
        }

        private List<Chapters> ParseTxtToChapters(string content, long novelId)
        {
            var chapters = new List<Chapters>();

            // 正则分成两组，自动剔除「第X章」前缀
            var regex = new Regex(
               @"^\s*第\s*[0-9一二三四五六七八九十百千]+\s*[章节卷集]\s*[:：]?\s*([^\r\n]*?)\s*[。！？]?\s*$",
               RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //找到所有符合标题正则表达式的章节标题,matches是一个集合，返回所有章节标题
            var matches = regex.Matches(content);

            if (matches.Count == 0)
            {
                return chapters;
            }

            int firstChapterIndex = matches[0].Index;
            content = content.Substring(firstChapterIndex);
            matches = regex.Matches(content);

            for (int i = 0; i < matches.Count; i++)
            {
                //  唯一修改：直接取纯标题，不带「第X章」
                var title = matches[i].Groups[1].Value.Trim();
                title = Regex.Replace(title, @"\s+", " ");

                int startIndex = matches[i].Index + matches[i].Length;
                int endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : content.Length;

                var chapterContent = content.Substring(startIndex, endIndex - startIndex).Trim();

                if (string.IsNullOrWhiteSpace(chapterContent))
                {
                    continue;
                }

                chapters.Add(new Chapters
                {
                    NovelId = novelId,
                    Title = title, // 纯标题存入数据库
                    Content = chapterContent,
                    IsLocked = false,
                    PublishTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                });
            }

            return chapters;
        }

        [HttpPost("{novelId:long}/import-txt")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<int>>> ImportTxt(long novelId, IFormFile file)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. 验证文件
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<int>.Fail("请选择要上传的TXT文件"));
            }

            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<int>.Fail("只能上传TXT格式的文件"));
            }

            // 2. 验证小说权限
            var novel = await ctx.Novels.FirstOrDefaultAsync(n => n.NovelId == novelId);
            if (novel == null)
            {
                return BadRequest(ApiResponse<int>.Fail("小说不存在"));
            }
            if (novel.AuthorId != userId)
            {
                return BadRequest(ApiResponse<int>.Fail("无权限"));
            }

            using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                // 3. 读取TXT文件内容
                //using var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding("GBK"));
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                var content = await reader.ReadToEndAsync();

                // 4. 解析章节
                var chapters = ParseTxtToChapters(content, novelId);

                if (chapters.Count == 0)
                {
                    return BadRequest(ApiResponse<int>.Fail("未识别到任何章节，请检查TXT格式"));
                }

                // 5. 计算起始章节号
                var maxChapterNumber = await ctx.Chapters
                    .Where(c => c.NovelId == novelId && !c.IsDeleted)
                    .MaxAsync(c => (int?)c.ChapterNumber) ?? 0;

                // 6. 批量添加章节
                int startNumber = maxChapterNumber + 1;
                int totalWordCount = 0;

                foreach (var chapter in chapters)
                {
                    chapter.ChapterNumber = startNumber++;
                    chapter.WordCount = chapter.Content.Replace("\r", "").Replace("\n", "").Replace(" ", "").Length;
                    totalWordCount += chapter.WordCount;
                }

                await ctx.Chapters.AddRangeAsync(chapters);
                await ctx.SaveChangesAsync();

                // 7. 更新小说统计信息
                novel.ChapterCount += chapters.Count;
                novel.WordCount += totalWordCount;
                novel.LastUpdateTime = DateTime.UtcNow;
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("TXT导入成功!小说ID{NovelId},导入章节数{Count}", novelId, chapters.Count);
                return Ok(ApiResponse<int>.Success(chapters.Count, $"成功导入{chapters.Count}章"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "TXT导入失败!小说ID{NovelId}", novelId);
                return BadRequest(ApiResponse<int>.Fail($"导入失败：{ex.Message}"));
            }
        }
    }
}
