using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitchenAidAI.Models
{
    public class Frizider
    {
        [Key]
        public int id { get; set; }

        public int? userId { get; set; }

        [ForeignKey(nameof(userId))]
        public virtual User? user { get; set; }

        public virtual ICollection<Namirnica> namirnice { get; set; }
        public DateTime kreirano { get; set; }
        public DateTime azurirano { get; set; }

        public bool isDeleted { get; set; }

        public Frizider() {
            kreirano = DateTime.Now;
            azurirano = DateTime.Now;
            namirnice = new List<Namirnica>();
        }
    }
}
