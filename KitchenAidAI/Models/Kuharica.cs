namespace KitchenAidAI.Models
{
    public class Kuharica
    {
        public int id { get; set; }
        public string? naziv { get; set; }
        public List<ReceptKuharica>? receptKuharice { get; set; }
        public DateTime kreirano { get; set; }

        public Kuharica() { kreirano = DateTime.Now; }
    }
}
