namespace NovelProject.DTO
{
    public class ReadingRecordsList
    {
        public long RecordId { get; set; }
        public long NovelId { get; set; }//小说Id
        public long? LastReadChapterId { get; set; }//最后阅读章节Id
        public DateTime LastReadTime { get; set; }//最后阅读时间
        public string Title { get; set; } = string.Empty;//小说标题
        public string AuthorName { get; set; } = string.Empty;//作者昵称
        public string? CoverUrl { get; set; }//小说封面Url
    }
}
