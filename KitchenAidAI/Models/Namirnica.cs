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

        [Required]
        [StringLength(200)]
        public string? naziv { get; set; }
        public KategorijaNamirnice kategorija { get; set; }
        public NutritivnaVrijednost? nutritivnaVrijednost { get; set; }
        public Mjera mjera { get; set; }
        public double kolicinaUFrizideru { get; set; }

        public Namirnica() { }
    }
}
