using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
    }
}