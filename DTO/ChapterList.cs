namespace NovelProject.DTO
{
    public class ChapterList
    {
        public long ChapterId { get; set; }
        public int ChapterNumber { get; set; } // 章节序号(第1/2/3章)
        public string Title { get; set; } = string.Empty; // 章节标题
        public int WordCount { get; set; } // 章节字数
        public DateTime PublishTime { get; set; } // 发布时间
        public DateTime? LastUpdateTime { get; set; } // 最后修改时间
        public bool IsLocked { get; set; } // 是否锁定(付费/VIP章节)
    }
}
