using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            // Validate configuration
            var jwtConfig = _configuration.GetSection("JwtConfig");
            if (string.IsNullOrEmpty(jwtConfig["Secret"]))
                throw new InvalidOperationException("JWT Secret key is not configured");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                // Uncomment if email claim is needed:
                // new Claim(ClaimTypes.Email, user.Credential.Email),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Secret"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Use the configured expiration time from appsettings.json
            var expirationMinutes = Convert.ToInt32(jwtConfig["ExpirationInMinutes"]);
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}