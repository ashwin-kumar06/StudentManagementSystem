using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly int _pageSize = 3;

        public UserController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        [Authorize]
        public IActionResult Home()
        {
            return View();
        }

        public async Task<IActionResult> Index(string sortOrder, string firstName, string lastName, string email, string course, int? semester, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["FirstNameSort"] = string.IsNullOrEmpty(sortOrder) ? "firstName_desc" : "";
            ViewData["LastNameSort"] = sortOrder == "lastName" ? "lastName_desc" : "lastName";
            ViewData["SemesterSort"] = sortOrder == "semester" ? "semester_desc" : "semester";

            ViewData["CurrentFirstNameFilter"] = firstName;
            ViewData["CurrentLastNameFilter"] = lastName;
            ViewData["CurrentEmailFilter"] = email;
            ViewData["CurrentCourseFilter"] = course;
            ViewData["CurrentSemesterFilter"] = semester;

            ViewData["Courses"] = await _context.StudentDetails
                .Select(s => s.Course)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewData["Semesters"] = await _context.StudentDetails
                .Select(s => s.Semester)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            IQueryable<User> user = _context.User;

            if (!string.IsNullOrEmpty(firstName))
            {
                user = user.Where(s => s.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                user = user.Where(s => s.LastName.Contains(lastName));
            }

            if (!string.IsNullOrEmpty(email))
            {
                user = user.Where(s => s.Credential.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(course))
            {
                user = user.Where(s => s.StudentDetails.Course == course);
            }

            if (semester.HasValue)
            {
                user = user.Where(s => s.StudentDetails.Semester == semester.Value);
            }

            user = sortOrder switch
            {
                "firstName_desc" => user.OrderByDescending(s => s.FirstName),
                "lastName" => user.OrderBy(s => s.LastName),
                "lastName_desc" => user.OrderByDescending(s => s.LastName),
                "semester" => user.OrderBy(s => s.StudentDetails.Semester),
                "semester_desc" => user.OrderByDescending(s => s.StudentDetails.Semester),
                _ => user.OrderBy(s => s.FirstName), // Default sort
            };

            var pageNumber = page ?? 1;
            var paginatedStudents = await PaginatedList<User>.CreateAsync(user, pageNumber, _pageSize);
            return View(paginatedStudents);
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserCredential model)
        {
            var user = await _context.User
                .Include(u => u.Credential)
                .FirstOrDefaultAsync(u => u.Credential.Email == model.Email);

            if (user != null && VerifyPassword(model.Password, user.Credential.Password))
            {
                var token = _authService.GenerateJwtToken(user);

                if (token != null)
                {
                    Response.Cookies.Append("JWTToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.Now.AddDays(7)
                    });
                }
                else
                {
                    HttpContext.Session.SetString("JWTToken", token);
                }
                return RedirectToAction("Home");
            }
            else
            {
                TempData["LoginFailureMessage"] = "Invalid email or password";
                return RedirectToAction("Login");
            }
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
                user.StudentDetails = new StudentDetails
                {
                    Course = model.StudentDetails.Course,
                    Semester = model.StudentDetails.Semester
                };
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

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                var hashedInput = Convert.ToBase64String(hashedBytes);
                return hashedInput == storedHash;
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student has been added successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["EditSuccessMessage"] = "Student has been edited successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}