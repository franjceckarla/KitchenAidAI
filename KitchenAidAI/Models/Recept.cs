using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class Recept
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string? naziv { get; set; }

        [StringLength(1000)]
        public string? opis { get; set; }
        public double vrijemeKuhanja { get; set; }
        public TezinaRecepta tezina { get; set; }
        public int brojPorcija { get; set; }
        public DateTime datumKreiranja { get; set; }

        public virtual ICollection<KorakRecepta> koraci { get; set; }
        public virtual ICollection<ReceptKuharica> receptKuharice { get; set; }

        public Recept()
        {
            datumKreiranja = DateTime.Now;
            koraci = new List<KorakRecepta>();
            receptKuharice = new List<ReceptKuharica>();
        }
    }
}



