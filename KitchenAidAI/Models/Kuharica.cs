using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitchenAidAI.Models
{
    public class Kuharica
    {
        [Key]
        public int id { get; set; }

        [StringLength(200)]
        public string? naziv { get; set; }

        public int? userId { get; set; }

        [ForeignKey(nameof(userId))]
        public virtual User? user { get; set; }

        public virtual ICollection<ReceptKuharica> receptKuharice { get; set; }
        public DateTime kreirano { get; set; }

        public Kuharica()
        {
            kreirano = DateTime.Now;
            receptKuharice = new List<ReceptKuharica>();
        }
    }
}
