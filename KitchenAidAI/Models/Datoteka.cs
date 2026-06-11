using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models
{
    public class Datoteka
    {
        [Key]
        public int id { get; set; }

        public int userId { get; set; }

        [StringLength(260)]
        public string? naziv { get; set; }

        [StringLength(1000)]
        public string? opis { get; set; }

        [StringLength(255)]
        public string? contentType { get; set; }

        public long velicina { get; set; }

        [StringLength(500)]
        public string? putanja { get; set; }

        public DateTime kreirano { get; set; }

        public bool isDeleted { get; set; }

        public DateTime? deletedAt { get; set; }

        public virtual User? user { get; set; }

        public Datoteka()
        {
            kreirano = DateTime.Now;
        }
    }
}
