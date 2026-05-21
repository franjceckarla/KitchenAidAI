using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Polje Korisnicko ime je obavezno.")]
        [StringLength(100, ErrorMessage = "Korisnicko ime moze imati najvise 100 znakova.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Polje Ime je obavezno.")]
        [StringLength(120, ErrorMessage = "Ime moze imati najvise 120 znakova.")]
        public string? Ime { get; set; }

        [Required(ErrorMessage = "Polje Prezime je obavezno.")]
        [StringLength(120, ErrorMessage = "Prezime moze imati najvise 120 znakova.")]
        public string? Prezime { get; set; }

        [Required(ErrorMessage = "Polje Datum rodenja je obavezno.")]
        [DataType(DataType.DateTime)]
        public DateTime? DatumRodenja { get; set; }

        [Required(ErrorMessage = "Polje Email je obavezno.")]
        [StringLength(200, ErrorMessage = "Email moze imati najvise 200 znakova.")]
        [EmailAddress(ErrorMessage = "Molimo unesite email u ispravnom formatu (npr. ime@domena.com).")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Polje Zemlja je obavezno.")]
        [StringLength(120, ErrorMessage = "Zemlja moze imati najvise 120 znakova.")]
        public string? Zemlja { get; set; }

        [Required(ErrorMessage = "Polje Lozinka je obavezno.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public List<PreferencijaPrehrane> SelectedPreferences { get; set; } = new();
    }
}
