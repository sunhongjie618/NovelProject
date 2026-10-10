namespace NovelProject.DTO
{
    public class CategoriesDetail
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;//分类名
        public int SortOrder { get; set; }//排序序号
        public string? Description { get; set; }//分类描述
        public bool? IsEnable { get; set; }//是否启用
    }
}
