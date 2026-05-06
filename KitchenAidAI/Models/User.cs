using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string? username { get; set; }

        [Required]
        [StringLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Molimo unesite email u ispravnom formatu (npr. ime@domena.com).")]
        public string? email { get; set; }
        public DateTime kreirano { get; set; }

        public virtual Frizider? frizider { get; set; }

        public PreferencijaPrehrane preferencijaPrehrane { get; set; }

        public virtual Kuharica? kuharica { get; set; }

        public virtual ICollection<ChatMessage> chatPoruke { get; set; }

        public User()
        {
            kreirano = DateTime.Now;
            chatPoruke = new List<ChatMessage>();
        }
    }
}
