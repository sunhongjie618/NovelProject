namespace NovelProject.DTO
{
    public class NovelDetail
    {
        public long NovelId { get; set; }
        public long CategoryId { get; set; }//分类Id
        public string CategoryName { get; set; }
        public string Title { get; set; }//小说标题
        public string AuthorName { get; set; }//作者昵称
        public string? CoverUrl { get; set; }//小说封面Url
        public string? Description { get; set; }//小说简介
        public int Status { get; set; }//状态(1 连载/2 完结/3 下架)
        public int WordCount { get; set; }//总字数
        public int ChapterCount { get; set; }//总章节数
        public DateTime PublishTime { get; set; }//首发时间
        public DateTime LastUpdateTime { get; set; }//最后更新时间
        public int ClickCount { get; set; }//总点击量
        public int FavoriteCount { get; set; }//总收藏数
    }
}
