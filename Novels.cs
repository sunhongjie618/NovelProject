using System.ComponentModel.DataAnnotations;

namespace NovelProject
{
    public class Novels
    {
        public long NovelId { get; set; }
        public long CategoryId { get; set; }//分类Id
        public long AuthorId { get; set; }//作者Id
        public string Title { get; set; } = string.Empty;//小说标题
        public string AuthorName { get; set; } = string.Empty;//作者昵称
        [MaxLength(500)]
        public string? CoverUrl { get; set; }//小说封面Url
        public string? Description { get; set; }//小说简介
        public int Status { get; set; }//状态(1 连载/2 完结/3 下架)
        public int WordCount { get; set; }//总字数
        public int ChapterCount { get; set; }//总章节数
        public DateTime PublishTime { get; set; }//首发时间
        public DateTime LastUpdateTime { get; set; }//最后更新时间
        public int ClickCount { get; set; }//总点击量
        public int FavoriteCount { get; set; }//总收藏数
        public bool IsDeleted { get; set; }//软删除

        public Categories? Category { get; set; }//关联分类
        public User? Author { get; set; }//关联作者

        public List<Chapters> Chapters { get; set; } = new List<Chapters>();

        //小说的评论
        public List<NovelComments> Comments { get; set; } = new List<NovelComments>();
        //小说的收藏记录
        public List<UserFavorites> Favorites { get; set; } = new List<UserFavorites>();
        //小说的阅读记录
        public List<UserReadingRecords> ReadingRecords { get; set; } = new List<UserReadingRecords>();
    }
}
