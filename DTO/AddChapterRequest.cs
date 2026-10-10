namespace NovelProject.DTO
{
    public class AddChapterRequest
    {
        public long NovelId { get; set; }//小说Id
        public string Title { get; set; } = string.Empty;//章节标题
        public string Content { get; set; } = string.Empty;//章节正文
        public bool IsLocked { get; set; }//是否锁定
        public int ChapterNumber { get; set; } //章节序号
    }
}
