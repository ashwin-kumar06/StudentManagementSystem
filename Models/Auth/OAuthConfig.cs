namespace StudentManagementSystem.Models.Auth
{
    public class OAuthConfig
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AuthorizationServerBaseUrl { get; set; }
        public string? RedirectUri { get; set; }
    }
}