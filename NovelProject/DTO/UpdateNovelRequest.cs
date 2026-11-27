namespace NovelProject.DTO
{
    public class UpdateNovelRequest
    {
        public long? CategoryId { get; set; }
        public string? Title { get; set; }
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }//状态(1 连载/2 完结/3 下架)
    }
}
