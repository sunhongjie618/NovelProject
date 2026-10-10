namespace NovelProject.DTO
{
    public class FavoriteList
    {
        public long FavoriteId { get; set; }
        public long UserId { get; set; }//用户Id
        public long NovelId { get; set; }//小说Id
        public string NovelName { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }

        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int FavoriteCount { get; set; }
        public DateTime CreateTime { get; set; }//收藏时间
    }
}
