namespace KitchenAidAI.Models.DTOs
{
    public class DatotekaDto
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string? naziv { get; set; }
        public string? opis { get; set; }
        public string? contentType { get; set; }
        public long velicina { get; set; }
        public string? putanja { get; set; }
        public DateTime kreirano { get; set; }
        public bool isDeleted { get; set; }
        public DateTime? deletedAt { get; set; }
    }
}
