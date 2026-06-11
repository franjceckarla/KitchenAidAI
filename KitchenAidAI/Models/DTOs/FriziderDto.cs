namespace KitchenAidAI.Models.DTOs
{
    public class FriziderDto
    {
        public int id { get; set; }
        public int? userId { get; set; }
        public bool isDeleted { get; set; }
        public List<NamirnicaDto> namirnice { get; set; } = new();
    }
}
