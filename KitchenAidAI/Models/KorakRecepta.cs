using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitchenAidAI.Models
{
    public class KorakRecepta
    {
        [Key]
        public int id { get; set; }

        public int receptId { get; set; }

        [ForeignKey(nameof(receptId))]
        public Recept? recept { get; set; }

        public int redniBroj { get; set; }
        public string? opis { get; set; }
        public double trajanje { get; set; }

        public DateTime kreirano { get; set; }

        public bool isDeleted { get; set; }

        public KorakRecepta()
        {
            kreirano = DateTime.Now;
        }
    }
}
