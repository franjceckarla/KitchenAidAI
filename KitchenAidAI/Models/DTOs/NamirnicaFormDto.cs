using System.ComponentModel.DataAnnotations;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class NamirnicaFormDto
    {
        public int id { get; set; }
        public int friziderId { get; set; }

        [Required(ErrorMessage = "Polje Naziv je obavezno.")]
        [StringLength(200, ErrorMessage = "Naziv moze imati najvise 200 znakova.")]
        public string? naziv { get; set; }

        public KategorijaNamirnice kategorija { get; set; }
        public Mjera mjera { get; set; }
        public double kolicinaUFrizideru { get; set; }
    }
}
