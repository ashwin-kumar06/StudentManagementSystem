using StudentManagementSystem.Models.Auth;

namespace StudentManagementSystem.Services.Auth
{
    public interface IOAuthClientService
    {
        string GenerateAuthorizationRequest(string state);
        Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string state);
    }
}