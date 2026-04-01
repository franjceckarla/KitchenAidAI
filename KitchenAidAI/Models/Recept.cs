using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class Recept
    {
        public int id { get; set; }
        public string? naziv { get; set; }
        public string? opis { get; set; }
        public double vrijemeKuhanja { get; set; }
        public TezinaRecepta tezina { get; set; }
        public int brojPorcija { get; set; }
        public DateTime datumKreiranja { get; set; }
        public List<KorakRecepta>? koraci { get; set; }
        public List<ReceptKuharica>? receptKuharice { get; set; }

        public Recept()
        {
            datumKreiranja = DateTime.Now;
            koraci = new List<KorakRecepta>();
            receptKuharice = new List<ReceptKuharica>();
        }
    }
}



