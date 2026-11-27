namespace NovelProject.DTO
{
    public class AddCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;//分类名
        public string? Description { get; set; }//分类描述

        public bool? IsEnable { get; set; } = true;//是否启用
    }
}
