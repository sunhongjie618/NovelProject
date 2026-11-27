namespace NovelProject.DTO
{
    public class FileUploadResult
    {
        //前端可以直接用这个URL显示图片
        public string FileUrl { get; set; }

        //服务器上保存的文件名，以后删除图片时可能用到
        public string FileName { get; set; }
    }
}
