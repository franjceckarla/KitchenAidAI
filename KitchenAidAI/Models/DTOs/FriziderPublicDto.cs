namespace KitchenAidAI.Models.DTOs
{
    public class FriziderPublicDto
    {
        public int id { get; set; }
        public int? userId { get; set; }
        public List<NamirnicaPublicDto> namirnice { get; set; } = new();
    }
}
