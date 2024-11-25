using Microsoft.Extensions.Options;
using StudentManagementSystem.Models.Auth;

namespace StudentManagementSystem.Services.Auth
{
    public class OAuthClientService : IOAuthClientService
    {
        private readonly HttpClient _httpClient;
        private readonly OAuthConfig _config;

        public OAuthClientService(HttpClient httpClient, IOptions<OAuthConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }

        public string GenerateAuthorizationRequest(string state)
        {
            var queryParams = new Dictionary<string, string>
            {
                {"client_id", _config.ClientId},
                {"redirect_uri", _config.RedirectUri},
                {"response_type", "code"},
                {"scope", "student.read student.write"},
                {"state", state}
            };

            var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{_config.AuthorizationServerBaseUrl}/authorize?{queryString}";
        }

        public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string state)
        {
            var tokenEndpoint = $"{_config.AuthorizationServerBaseUrl}/token";
            
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", _config.RedirectUri},
                {"client_id", _config.ClientId},
                {"client_secret", _config.ClientSecret}
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Token exchange failed");

            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
    }
}