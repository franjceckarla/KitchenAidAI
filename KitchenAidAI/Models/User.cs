using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Polje Korisnicko ime je obavezno.")]
        [StringLength(100, ErrorMessage = "Korisnicko ime moze imati najvise 100 znakova.")]
        public string? username { get; set; }

        [StringLength(120, ErrorMessage = "Ime moze imati najvise 120 znakova.")]
        public string? ime { get; set; }

        [StringLength(120, ErrorMessage = "Prezime moze imati najvise 120 znakova.")]
        public string? prezime { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime? datumRodenja { get; set; }

        [StringLength(120, ErrorMessage = "Zemlja moze imati najvise 120 znakova.")]
        public string? zemlja { get; set; }

        [StringLength(2000, ErrorMessage = "Lozinka moze imati najvise 2000 znakova.")]
        public string? passwordHash { get; set; }

        public bool isAdmin { get; set; }

        public bool isDeleted { get; set; }

        [Required(ErrorMessage = "Polje Email je obavezno.")]
        [StringLength(200, ErrorMessage = "Email moze imati najvise 200 znakova.")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Molimo unesite email u ispravnom formatu (npr. ime@domena.com).")]
        public string? email { get; set; }
        [StringLength(100, ErrorMessage = "Provider moze imati najvise 100 znakova.")]
        public string? authProvider { get; set; }

        [StringLength(200, ErrorMessage = "Provider key moze imati najvise 200 znakova.")]
        public string? authProviderKey { get; set; }

        public DateTime kreirano { get; set; }

        public virtual Frizider? frizider { get; set; }

        public PreferencijaPrehrane preferencijaPrehrane { get; set; }

        public virtual Kuharica? kuharica { get; set; }

        public virtual ICollection<ChatMessage> chatPoruke { get; set; }
        public virtual ICollection<Datoteka> datoteke { get; set; }

        public User()
        {
            kreirano = DateTime.Now;
            chatPoruke = new List<ChatMessage>();
            datoteke = new List<Datoteka>();
        }
    }
}
