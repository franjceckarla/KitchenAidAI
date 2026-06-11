using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class NamirnicaPublicDto
    {
        public int id { get; set; }
        public int friziderId { get; set; }
        public string? naziv { get; set; }
        public KategorijaNamirnice kategorija { get; set; }
        public Mjera mjera { get; set; }
        public double kolicinaUFrizideru { get; set; }
    }
}
