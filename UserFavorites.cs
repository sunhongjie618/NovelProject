namespace NovelProject
{
    public class UserFavorites
    {
        public long FavoriteId { get; set; }
        public long UserId { get; set; }//用户Id
        public long NovelId { get; set; }//小说Id
        public DateTime CreateTime { get; set; }//收藏时间
        public bool IsDeleted { get; set; }//是否取消收藏(软删除)
        public User? User { get; set; }//关联用户
        public Novels? Novel { get; set; }//关联小说
    }
}
