namespace KitchenAidAI.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public T? data { get; set; }
        public ApiAlertDto? alert { get; set; }

        public static ApiResponse<T> Ok(T data)
        {
            return new ApiResponse<T> { success = true, data = data };
        }

        public static ApiResponse<T> Fail(string title, string message, string? code = null, string type = "error")
        {
            return new ApiResponse<T>
            {
                success = false,
                alert = new ApiAlertDto
                {
                    title = title,
                    message = message,
                    code = code,
                    type = type
                }
            };
        }
    }
}
