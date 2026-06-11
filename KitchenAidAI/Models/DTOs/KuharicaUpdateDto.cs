using System.ComponentModel.DataAnnotations;

namespace KitchenAidAI.Models.DTOs
{
    public class KuharicaUpdateDto
    {
        [StringLength(200)]
        public string? naziv { get; set; }
    }
}
