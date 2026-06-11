namespace KitchenAidAI.Models.DTOs
{
    public class ApiAlertDto
    {
        public string type { get; set; } = "error";
        public string title { get; set; } = "Greska";
        public string message { get; set; } = string.Empty;
        public string? code { get; set; }
    }
}
