using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class ReceptDto
    {
        public int id { get; set; }
        public string? naziv { get; set; }
        public string? opis { get; set; }
        public double vrijemeKuhanja { get; set; }
        public TezinaRecepta tezina { get; set; }
        public int brojPorcija { get; set; }
        public bool isDeleted { get; set; }
    }
}
