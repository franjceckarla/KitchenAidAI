using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class Namirnica
    {
        [Key]
        public int id { get; set; }

        public int friziderId { get; set; }

        [ForeignKey(nameof(friziderId))]
        public virtual Frizider? frizider { get; set; }

        [Required(ErrorMessage = "Polje Naziv je obavezno.")]
        [StringLength(200, ErrorMessage = "Naziv moze imati najvise 200 znakova.")]
        public string? naziv { get; set; }
        public KategorijaNamirnice kategorija { get; set; }
        public NutritivnaVrijednost? nutritivnaVrijednost { get; set; }
        public Mjera mjera { get; set; }
        public double kolicinaUFrizideru { get; set; }

        public DateTime kreirano { get; set; }

        public bool isDeleted { get; set; }

        public Namirnica()
        {
            kreirano = DateTime.Now;
        }
    }
}
