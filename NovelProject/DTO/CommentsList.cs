namespace NovelProject.DTO
{
    public class CommentsList
    {
        public long CommentId { get; set; }
        public long? UserId { get; set; }//评论者Id
        public string UserNickName { get; set; } = string.Empty; // 评论者昵称
        public string? UserAvatarUrl { get; set; } // 评论者头像URL
        public long? ChapterId { get; set; }//评论文章Id
        public string Content { get; set; } = string.Empty;//评论内容
        public DateTime CreateTime { get; set; }//发布时间
        public int LikeCount { get; set; }//点赞数
        public bool IsLiked { get; set; }
    }
}
