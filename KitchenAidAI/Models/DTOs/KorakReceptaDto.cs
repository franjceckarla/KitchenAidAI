namespace KitchenAidAI.Models.DTOs
{
    public class KorakReceptaDto
    {
        public int id { get; set; }
        public int receptId { get; set; }
        public int redniBroj { get; set; }
        public string? opis { get; set; }
        public double trajanje { get; set; }
        public bool isDeleted { get; set; }
    }
}
