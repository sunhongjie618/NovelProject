namespace NovelProject
{
    public class CommentLike
    {
        public long LikeId { get; set; }
        public long UserId { get; set; }
        public long CommentId { get; set; }
        public long NovelId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
