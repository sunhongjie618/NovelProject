namespace NovelProject
{
    public class UserReadingRecords
    {
        public long RecordId { get; set; }
        public long UserId { get; set; }//用户Id
        public long NovelId { get; set; }//小说Id
        public long? LastReadChapterId { get; set; }//最后阅读章节Id
        public DateTime LastReadTime { get; set; }//最后阅读时间
        public bool IsDeleted { get; set; }//软删除
        public User? User { get; set; }//关联用户
        public Novels? Novel { get; set; }//关联小说
        public Chapters? Chapter { get; set; }//关联章节
    }
}
