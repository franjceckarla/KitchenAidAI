namespace KitchenAidAI.Models.ViewModels
{
    public class ExternalLoginCallbackViewModel
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Provider { get; set; }
        public string? ProviderKey { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
