using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Polje Korisnicko ime je obavezno.")]
        [StringLength(100, ErrorMessage = "Korisnicko ime moze imati najvise 100 znakova.")]
        public string? username { get; set; }

        [StringLength(120, ErrorMessage = "Ime moze imati najvise 120 znakova.")]
        public string? ime { get; set; }

        [StringLength(120, ErrorMessage = "Prezime moze imati najvise 120 znakova.")]
        public string? prezime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? datumRodenja { get; set; }

        [StringLength(120, ErrorMessage = "Zemlja moze imati najvise 120 znakova.")]
        public string? zemlja { get; set; }

        [Required(ErrorMessage = "Polje Email je obavezno.")]
        [StringLength(200, ErrorMessage = "Email moze imati najvise 200 znakova.")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Molimo unesite email u ispravnom formatu (npr. ime@domena.com).")]
        public string? email { get; set; }

        [Required(ErrorMessage = "Polje Lozinka je obavezno.")]
        [DataType(DataType.Password)]
        public string? password { get; set; }

        public PreferencijaPrehrane preferencijaPrehrane { get; set; }
    }
}
