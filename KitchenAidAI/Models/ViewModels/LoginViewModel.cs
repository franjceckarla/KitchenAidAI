using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Polje Korisnicko ime je obavezno.")]
        [StringLength(100, ErrorMessage = "Korisnicko ime moze imati najvise 100 znakova.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Polje Lozinka je obavezno.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
