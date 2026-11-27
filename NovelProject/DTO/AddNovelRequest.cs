namespace NovelProject.DTO
{
    public class AddNovelRequest
    {
        public long CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
    }
}
