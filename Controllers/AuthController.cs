using System.Text;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Services.Auth;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;
using StudentManagementSystem.Data;
using System.Security.Cryptography;

namespace StudentManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOAuthClientService _oauthService;
        private readonly IConfiguration _configuration;

        public AuthController(IOAuthClientService oauthService, IConfiguration configuration)
        {
            _oauthService = oauthService;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            var state = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("OAuth_State", state);
            
            var authorizationUrl = _oauthService.GenerateAuthorizationRequest(state);
            return Redirect(authorizationUrl);
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegisterViewModel model)
        {
            // if (!ModelState.IsValid)
            // {
            //     return View(model);
            // }

            // Check if email already exists
            if (await _context.UserCredentials.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Create User entity based on role
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Role = model.Role
            };

            // Add additional details for Student
            if (model.Role == "Student")
            {
                user.StudentDetails.Course = model.StudentDetails.Course;
                user.StudentDetails.Semester = model.StudentDetails.Semester;
            }

            // Create user credential
            var userCredential = new UserCredential
            {
                Email = model.Email,
                Password = HashPassword(model.Password),
                LastLoginDate = DateTime.UtcNow,
                User = user
            };

            // Add to context and save
            try
            {
                _context.User.Add(user);
                _context.UserCredentials.Add(userCredential);
                await _context.SaveChangesAsync();

                // Redirect to login or home page
                TempData["SuccessMessage"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration: " + ex.Message);
                return View(model);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<IActionResult> Callback(string code, string state)
        {
            var savedState = HttpContext.Session.GetString("OAuth_State");
            if (state != savedState)
                return BadRequest("Invalid state parameter");
            try
            {
                var token = await _oauthService.ExchangeCodeForTokenAsync(code, state);
                
                // Store the token in session or cookie
                HttpContext.Session.SetString("AccessToken", token.AccessToken);
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest("Token exchange failed");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}