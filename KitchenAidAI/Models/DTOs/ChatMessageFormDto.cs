namespace KitchenAidAI.Models.DTOs
{
    public class ChatMessageFormDto
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string? message { get; set; }
        public string? response { get; set; }
    }
}
