using KitchenAidAI.Models.DTOs;

namespace KitchenAidAI.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public IReadOnlyList<UserDto> Users { get; set; } = new List<UserDto>();
    }
}
