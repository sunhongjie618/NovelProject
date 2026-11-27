namespace NovelProject.DTO
{
    public class NovelList
    {
        public long NovelId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public int WordCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime LastUpdateTime { get; set; }
        public int ClickCount { get; set; }//总点击量
        public int FavoriteCount { get; set; }//总收藏数

    }
}
