namespace NovelProject.DTO
{
    public class ToggeFavoriteResult
    {
        public  bool isFavorited { get; set; }//是否收藏
        public int FavoriteCount { get; set; } // 最新的收藏数
    }
}
