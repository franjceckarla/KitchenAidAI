namespace KitchenAidAI.Models
{
    public class Frizider
    {
        public int id { get; set; }
        public List<Namirnica>? namirnice { get; set; }
        public DateTime azurirano { get; set; }

        public Frizider() {
            azurirano = DateTime.Now;
            namirnice = new List<Namirnica>();
        }
    }
}
