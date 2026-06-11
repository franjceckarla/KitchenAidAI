using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class UserPublicDto
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? ime { get; set; }
        public string? prezime { get; set; }
        public DateTime? datumRodenja { get; set; }
        public string? zemlja { get; set; }
        public string? email { get; set; }
        public PreferencijaPrehrane preferencijaPrehrane { get; set; }
        public FriziderPublicDto? frizider { get; set; }
        public KuharicaPublicDto? kuharica { get; set; }
    }
}
