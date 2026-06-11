using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models.DTOs
{
    public class KuharicaCreateDto
    {
        [Required]
        public int userId { get; set; }

        [StringLength(200)]
        public string? naziv { get; set; }
    }
}
