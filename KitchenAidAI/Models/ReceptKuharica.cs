using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitchenAidAI.Models
{
    public class ReceptKuharica
    {
        [Key]
        public int id { get; set; }

        public int receptId { get; set; }

        [ForeignKey(nameof(receptId))]
        public Recept? recept { get; set; }

        public int kuharicaId { get; set; }

        [ForeignKey(nameof(kuharicaId))]
        public Kuharica? kuharica { get; set; }
    }
}
