using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class ReceptFormDto
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Polje Naziv je obavezno.")]
        [StringLength(200, ErrorMessage = "Naziv moze imati najvise 200 znakova.")]
        public string? naziv { get; set; }

        [StringLength(1000, ErrorMessage = "Opis moze imati najvise 1000 znakova.")]
        public string? opis { get; set; }

        public double vrijemeKuhanja { get; set; }
        public TezinaRecepta tezina { get; set; }
        public int brojPorcija { get; set; }
    }
}
