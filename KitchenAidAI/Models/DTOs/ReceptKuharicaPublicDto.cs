namespace KitchenAidAI.Models.DTOs
{
    public class ReceptKuharicaPublicDto
    {
        public int id { get; set; }
        public int receptId { get; set; }
        public int kuharicaId { get; set; }
        public DateTime kreirano { get; set; }
        public ReceptPublicDto? recept { get; set; }
    }
}
