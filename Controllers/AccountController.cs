using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController(UserManager<IdentityUser> userManager,
           SignInManager<IdentityUser> signInManager,
           ILogger<HomeController> logger) : Controller
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly ILogger<HomeController> _logger = logger;

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new IdentityUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                       "New user registered: {Email}",
                        model.Email);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning(
                   "User registration failed for {Email}.",
                   model.Email);

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
           
            var result = await _signInManager
                        .PasswordSignInAsync(model.Email, model.Password, 
                                             model.RememberMe, 
                                             lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                       "User logged in : {Email}",
                        model.Email);
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning(
                   "User login failed : {Email}.",
                   model.Email);
           
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt!");
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out : {Email}", User.Identity?.Name ?? "Unknown");
            return RedirectToAction("Index", "Home");
        }
    }
}
