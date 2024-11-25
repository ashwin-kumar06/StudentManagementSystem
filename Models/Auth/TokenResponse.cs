
    // Place this file in: StudentManagementSystem/Models/Auth/TokenResponse.cs

using System.Text.Json.Serialization;

namespace StudentManagementSystem.Models.Auth
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        // Additional property to track token expiration
        public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);

        // Helper method to check if token is expired
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAt;
        }
    }
}
