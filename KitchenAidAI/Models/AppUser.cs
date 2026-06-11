using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string? ime { get; set; }
        public string? prezime { get; set; }

        [Column(TypeName = "date")]
        public DateTime? datumRodenja { get; set; }

        public string? zemlja { get; set; }

        public bool isAdmin { get; set; }

        public bool isDeleted { get; set; }

        public DateTime kreirano { get; set; } = DateTime.Now;

        public PreferencijaPrehrane preferencijaPrehrane { get; set; }

        public virtual Frizider? frizider { get; set; }
        public virtual Kuharica? kuharica { get; set; }

        public virtual ICollection<ChatMessage> chatPoruke { get; set; } = new List<ChatMessage>();
        public virtual ICollection<Datoteka> datoteke { get; set; } = new List<Datoteka>();

        [StringLength(100)]
        public string? authProvider { get; set; }

        [StringLength(200)]
        public string? authProviderKey { get; set; }
    }
}
