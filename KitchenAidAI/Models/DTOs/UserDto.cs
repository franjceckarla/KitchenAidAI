using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models.DTOs
{
    public class UserDto
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? ime { get; set; }
        public string? prezime { get; set; }
        public DateTime? datumRodenja { get; set; }
        public string? zemlja { get; set; }
        public string? email { get; set; }
        public PreferencijaPrehrane preferencijaPrehrane { get; set; }
        public DateTime kreirano { get; set; }
        public bool isAdmin { get; set; }
        public bool isDeleted { get; set; }
        public FriziderDto? frizider { get; set; }
        public KuharicaDto? kuharica { get; set; }
    }
}
