namespace NovelProject.DTO
{
    public class ChapterDetail
    {
        public long ChapterId { get; set; }
        public long NovelId { get; set; }//小说Id
        public int ChapterNumber { get; set; }//章节序号(第1/2/3章)
        public string Title { get; set; } = string.Empty;//章节标题
        public string Content { get; set; } = string.Empty;//章节正文
        public int WordCount { get; set; }//章节字数
        public DateTime PublishTime { get; set; }//发布时间
        public DateTime? UpdateTime { get; set; }//最后修改时间
        public bool IsLocked { get; set; }//是否锁定
        public long? PreChapterId { get; set; }//返回到上一章
        public long? NextChapterId { get; set; }//返回到下一章
    }
}
