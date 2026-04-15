using KitchenAidAI.Models;

namespace KitchenAidAI.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public IReadOnlyList<User> Users { get; set; } = new List<User>();
    }
}
