using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovelProject.DTO;

namespace NovelProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        //用来获取服务器上的路径信息，比如项目根目录在哪里
        private readonly IWebHostEnvironment _env;

        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("upload-cover")]
        public async Task<ActionResult<ApiResponse<FileUploadResult>>> UploadCover(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FileUploadResult>.Fail("请选择要上传的图片"));
            }
            //允许上传的文件的格式
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            //获取上传文件的扩展名，全转换成小写
            var extension = Path.GetExtension(file.FileName).ToLower();
            //判断用户上传的文件是否为指定格式的图片
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<FileUploadResult>.Fail("只支持jpg、jpeg、png、gif格式的图片"));
            }
            //限制用户上传的图片的大小最多2MB
            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<FileUploadResult>.Fail("图片大小不能超过2MB"));
            }

            try
            {
                //获取Picture的完整路径
                var uploadPath = Path.Combine(_env.ContentRootPath, "Picture");
                //判断Picture是否存在，不存在的话就自动创建
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                //用guid生成唯一的文件名，避免两个用户上传同名文件导致覆盖的问题
                var fileName = $"{Guid.NewGuid()}{extension}";
                //拼接出文件的完整保存路径
                var filePath = Path.Combine(uploadPath, fileName);

                //最重要的一步:将用户上传的文件复制保存到服务器上
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //生成完整的访问URL
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fileUrl = $"{baseUrl}/pictures/{fileName}";

                return Ok(ApiResponse<FileUploadResult>.Success(new FileUploadResult
                {
                    FileUrl = fileUrl,
                    FileName = fileName
                }, "封面上传成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<FileUploadResult>.Fail($"上传失败：{ex.Message}"));
            }
        }
    }
}
