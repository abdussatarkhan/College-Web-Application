using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace College_Web_Application.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToUserArea();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by UserName or Email
            var user = await _userManager.FindByNameAsync(model.Username) 
                       ?? await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please check your Roll Number/Email and password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect based on role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin") || roles.Contains("Principal") || roles.Contains("Clerk"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                if (roles.Contains("Faculty") || roles.Contains("HOD"))
                {
                    return RedirectToAction("MarkClassAttendance", "Attendance", new { area = "Faculty" });
                }
                if (roles.Contains("Student"))
                {
                    return RedirectToAction("Index", "Portal", new { area = "Student" });
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials. For students, default password is your CNIC digits (without dashes).");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToUserArea()
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Principal") || User.IsInRole("Clerk"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            if (User.IsInRole("Faculty") || User.IsInRole("HOD"))
            {
                return RedirectToAction("MarkClassAttendance", "Attendance", new { area = "Faculty" });
            }
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Portal", new { area = "Student" });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
