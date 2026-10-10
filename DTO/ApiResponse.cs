namespace NovelProject.DTO
{
    public class ApiResponse<T>
    {
        public int Code { get; set; } // 200成功，400参数错误，401未授权，500服务器错误
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "操作成功")
            => new ApiResponse<T> { Code = 200, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, int code = 400)
            => new ApiResponse<T> { Code = code, Message = message };
    }
}
