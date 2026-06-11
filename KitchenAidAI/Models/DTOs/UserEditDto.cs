using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class UserEditDto
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Polje Korisnicko ime je obavezno.")]
        [StringLength(100, ErrorMessage = "Korisnicko ime moze imati najvise 100 znakova.")]
        public string? username { get; set; }

        [Required(ErrorMessage = "Polje Email je obavezno.")]
        [StringLength(200, ErrorMessage = "Email moze imati najvise 200 znakova.")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Molimo unesite email u ispravnom formatu (npr. ime@domena.com).")]
        public string? email { get; set; }

        public PreferencijaPrehrane preferencijaPrehrane { get; set; }
        public bool isAdmin { get; set; }
    }
}
