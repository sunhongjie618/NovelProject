namespace NovelProject
{
    public class Message
    {
        public long MessageId { get; set; }
        public long ToUserId { get; set; }
        public long FromUserId { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public bool Isread { get; set; } = false;
    }
}
