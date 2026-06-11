namespace KitchenAidAI.Models.DTOs
{
    public class ReceptKuharicaDto
    {
        public int id { get; set; }
        public int receptId { get; set; }
        public int kuharicaId { get; set; }
        public DateTime kreirano { get; set; }
        public bool isDeleted { get; set; }
        public ReceptDto? recept { get; set; }
    }
}
