using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models.DTOs
{
    public class FriziderCreateDto
    {
        [Required]
        public int userId { get; set; }
    }
}
