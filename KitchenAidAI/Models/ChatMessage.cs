using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class ChatMessage
    {
        [Key]
        public int id { get; set; }

        public int userId { get; set; }

        [ForeignKey(nameof(userId))]
        public User? user { get; set; }

        public string? message { get; set; }
        public string? response { get; set; }
        [Column("datumKreiranja")]
        public DateTime kreirano { get; set; }
        public TipOdgovora tip { get; set; }

        public bool isDeleted { get; set; }

        public ChatMessage() { kreirano = DateTime.Now; }
    }
}
