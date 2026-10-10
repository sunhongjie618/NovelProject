namespace NovelProject
{
    public class Categories
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }//分类名
        public int SortOrder { get; set; }//排序序号
        public string? Description { get; set; }//分类描述
        public bool? IsEnable { get; set; }//是否启用
        public List<Novels> Novel { get; set; } = new List<Novels>();//1个分类对应多本小说
        public bool IsDeleted { get; set; }
    }
}
