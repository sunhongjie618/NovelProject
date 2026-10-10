using Microsoft.AspNetCore.Identity;

namespace NovelProject
{
    public class User:IdentityUser<long>
    {
        public string? NickName { get; set; }//昵称:对外展示
        public string? AvatarUrl { get; set; }//头像URL
        public DateTime RegisterTime { get; set; }//注册时间
        public DateTime? LastLoginTime { get; set; }//最后登录的时间
        public bool IsDeleted { get; set; }

        //用户作为作者写的小说
        public List<Novels> AuthoredNovels { get; set; } = new List<Novels>();
        //用户发表的评论
        public List<NovelComments> Comments { get; set; } = new List<NovelComments>();
        //用户的收藏
        public List<UserFavorites> Favorites { get; set; } = new List<UserFavorites>();
        //用户的阅读记录
        public List<UserReadingRecords> ReadingRecords { get; set; } = new List<UserReadingRecords>();
    }
}
