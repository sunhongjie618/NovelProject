namespace NovelProject.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public long UserId { get; set; }
    }
}
