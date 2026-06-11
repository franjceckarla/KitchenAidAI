namespace KitchenAidAI.Models.DTOs
{
    public class KuharicaDto
    {
        public int id { get; set; }
        public string? naziv { get; set; }
        public int? userId { get; set; }
        public bool isDeleted { get; set; }
        public List<ReceptKuharicaDto> receptKuharice { get; set; } = new();
    }
}
