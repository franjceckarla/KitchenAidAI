namespace KitchenAidAI.Models.DTOs
{
    public class KuharicaPublicDto
    {
        public int id { get; set; }
        public string? naziv { get; set; }
        public int? userId { get; set; }
        public List<ReceptKuharicaPublicDto> receptKuharice { get; set; } = new();
    }
}
