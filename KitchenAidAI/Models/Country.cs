using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models
{
    public class Country
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Polje Naziv je obavezno.")]
        [StringLength(120, ErrorMessage = "Naziv moze imati najvise 120 znakova.")]
        public string? naziv { get; set; }
    }
}
