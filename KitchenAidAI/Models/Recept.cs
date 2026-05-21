using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class Recept
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Polje Naziv je obavezno.")]
        [StringLength(200, ErrorMessage = "Naziv moze imati najvise 200 znakova.")]
        public string? naziv { get; set; }

        [StringLength(1000, ErrorMessage = "Opis moze imati najvise 1000 znakova.")]
        public string? opis { get; set; }
        public double vrijemeKuhanja { get; set; }
        public TezinaRecepta tezina { get; set; }
        public int brojPorcija { get; set; }
        [Column("datumKreiranja")]
        public DateTime kreirano { get; set; }

        public bool isDeleted { get; set; }

        public virtual ICollection<KorakRecepta> koraci { get; set; }
        public virtual ICollection<ReceptKuharica> receptKuharice { get; set; }

        public Recept()
        {
            kreirano = DateTime.Now;
            koraci = new List<KorakRecepta>();
            receptKuharice = new List<ReceptKuharica>();
        }
    }
}



