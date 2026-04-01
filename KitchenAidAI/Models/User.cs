using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class User
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public DateTime kreirano { get; set; }
        public Frizider? frizider { get; set; }
        public PreferencijaPrehrane preferencijaPrehrane { get; set; }
        public int kuharicaId { get; set; }
        public Kuharica? kuharica { get; set; }
        public List<ChatMessage>? chatPoruke { get; set; }

        public User() { kreirano = DateTime.Now; }
    }
}
