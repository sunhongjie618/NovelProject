namespace NovelProject
{
    public class NovelComments
    {
        public long CommentId { get; set; }
        public long? UserId { get; set; }//评论者Id
        public long NovelId { get; set; }//评论小说Id
        public long? ChapterId { get; set; }//评论文章Id
        public string Content { get; set; } = string.Empty;//评论内容
        public DateTime CreateTime { get; set; }//发布时间
        public int LikeCount { get; set; }//点赞数
        public bool IsDeleted { get; set; }//软删除

        public User? User { get; set; }//关联用户
        public Novels? Novel { get; set; }//关联小说
        public Chapters? Chapter { get; set; }//关联小说章节

    }
}
