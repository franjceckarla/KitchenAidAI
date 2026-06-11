using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class ChatMessagePublicDto
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string? message { get; set; }
        public string? response { get; set; }
        public DateTime kreirano { get; set; }
        public TipOdgovora tip { get; set; }
    }
}
