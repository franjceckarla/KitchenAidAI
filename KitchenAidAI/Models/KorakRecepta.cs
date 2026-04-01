namespace KitchenAidAI.Models
{
    public class KorakRecepta
    {
        public int id { get; set; }
        public int receptId { get; set; }
        public Recept? recept { get; set; }
        public int redniBroj { get; set; }
        public string? opis { get; set; }
        public double trajanje { get; set; }

        public KorakRecepta() { }
    }
}
