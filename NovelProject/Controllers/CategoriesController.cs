using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelProject.DTO;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<CategoriesController> _logger;
        public CategoriesController(MyDbContext ctx, ILogger<CategoriesController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoriesList>>>> GetCategories([FromQuery] bool? isEnable, [FromQuery] bool orderBySortAsc = true)
        {
            var query = ctx.Categories.AsNoTracking().AsQueryable();

            if (isEnable.HasValue)
            {
                query = query.Where(c => c.IsEnable == isEnable.Value);
            }

            query = orderBySortAsc ? query.OrderBy(c => c.SortOrder) : query.OrderByDescending(c => c.SortOrder);

            var categories = await query.Select(c => new CategoriesList
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                SortOrder = c.SortOrder,
                Description = c.Description,
                IsEnable = c.IsEnable
            }).ToListAsync();

            return Ok(ApiResponse<List<CategoriesList>>.Success(categories));
        }
        [HttpGet("{categoryId:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoriesDetail>>> GetCategoriesById(long categoryId)
        {
            var category = await ctx.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return BadRequest(ApiResponse<CategoriesDetail>.Fail("分类不存在!"));
            }

            var dto = new CategoriesDetail
            {
                CategoryId=category.CategoryId,
                CategoryName=category.CategoryName,
                SortOrder=category.SortOrder,
                Description=category.Description,
                IsEnable=category.IsEnable
            };

            return Ok(ApiResponse<CategoriesDetail>.Success(dto));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> AddCategory([FromBody] AddCategoryRequest request)
        {
            var categoryExist = await ctx.Categories.AnyAsync(c => c.CategoryName == request.CategoryName);
            if (categoryExist)
            {
                return BadRequest(ApiResponse<long>.Fail("分类已经存在!请勿重复添加")); 
            }

            var maxCategoryNumber = await ctx.Categories.MaxAsync(c => (int?)c.SortOrder) ?? 0;
            var newCategoryNumber = maxCategoryNumber + 1;

            var category = new Categories
            {
                CategoryName=request.CategoryName,
                Description=request.Description,
                IsEnable=request.IsEnable,
                SortOrder=newCategoryNumber
            };

            ctx.Categories.Add(category);
            await ctx.SaveChangesAsync();


            return Ok(ApiResponse<long>.Success(category.CategoryId, "分类添加成功!"));
        }

        [HttpDelete("{categoryId:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<long>>> DeleteCategory(long categoryId)
        {
            try
            {
                var category = await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (category == null)
                {
                    return BadRequest(ApiResponse<long>.Fail("该分类不存在!"));
                }
                var hasNovel = await ctx.Novels.AnyAsync(c => c.CategoryId == categoryId&&!c.IsDeleted);
                if (hasNovel)
                {
                    return BadRequest(ApiResponse<long>.Fail("该分类下还有小说，无法删除!"));
                }

                category.IsDeleted = true;
                await ctx.SaveChangesAsync();
                return Ok(ApiResponse<long>.Success(categoryId, "删除分类成功!"));
            }
            catch(Exception ex)
            {

                _logger.LogError(ex, "删除分类失败，分类Id{categoryId}", categoryId);
                return BadRequest(ApiResponse<long>.Fail("删除分类失败!请稍后重试"));
            }
        }

    }
}
