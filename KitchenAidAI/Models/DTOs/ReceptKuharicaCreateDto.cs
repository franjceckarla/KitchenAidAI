using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models.DTOs
{
    public class ReceptKuharicaCreateDto
    {
        [Required]
        public int receptId { get; set; }

        [Required]
        public int kuharicaId { get; set; }
    }
}
