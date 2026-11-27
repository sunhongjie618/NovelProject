namespace NovelProject.DTO
{
    public class UserFavoritesList
    {
        public long FavoriteId { get; set; }
        public long UserId { get; set; }//用户Id
        public long NovelId { get; set; }//小说Id
        public DateTime CreateTime { get; set; }//收藏时间
    }
}
