namespace NovelProject.DTO
{
    public class AddCommentRequest
    {
        public long NovelId { get; set; }//评论小说Id
        public long? ChapterId { get; set; }//评论文章Id
        public string Content { get; set; } = string.Empty;//评论内容
    }
}
