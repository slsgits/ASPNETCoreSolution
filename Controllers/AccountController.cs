using System.Security.Claims;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           ILogger<HomeController> logger)
         : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<HomeController> _logger = logger;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                City = model.City ?? string.Empty
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                       "New user registered: {Email}",
                        model.Email);

                if(_signInManager.IsSignedIn(User) && 
                    User.IsInRole("Admin"))
                {
                  return RedirectToAction("ListUsers", "Administration");
                }

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
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            // Store the returnUrl in the ViewBag to be used in the view
            //ViewBag.ReturnUrl = returnUrl;

            // Populate the ExternalLogins property of the LoginViewModel
            var model = new LoginViewModel
            {
               Email = string.Empty,
               Password = string.Empty,
               ReturnUrl = returnUrl ?? Url.Content("~/"),
               ExternalLogins =
               (await signInManager
                     .GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
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

                if (!string.IsNullOrEmpty(returnUrl)
                    && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
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

        [AcceptVerbs("GET", "POST")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin
            (
             string provider, 
             string returnUrl
            )
        {
            var redirectUrl = Url
                               .Action(
                                       "ExternalLoginCallback", 
                                       "Account",
                                       new { ReturnUrl = returnUrl }
                                       );
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties
                (provider, redirectUrl);

            // Challenge the user to log in with the external provider
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult>
            ExternalLoginCallback
            (
            string? returnUrl = null, 
            string? remoteError = null
            )
        {
            // If returnUrl is null, set it to the root URL
            returnUrl ??= Url.Content("~/");

            // Create a new instance of the LoginViewModel to pass to the view
            LoginViewModel loginViewModel = new()
            {
                Email = string.Empty,
                Password = string.Empty,
                ReturnUrl = returnUrl,
                // Populate the ExternalLogins property with the
                // external authentication schemes
                ExternalLogins =
                        (await _signInManager
                              .GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                _logger.LogWarning(
                                "External login failed. Provider Error: {RemoteError}",
                                 remoteError
                                 );

                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            // Get the login information about the user
            // from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            // Get the email claim value from the external login info
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // If the user already has a login (i.e if there is a record
            // in AspNetUserLogins table) then sign-in the user with
            // this external login provider
            var signInResult = await _signInManager
                                    .ExternalLoginSignInAsync
                                     (
                                      info.LoginProvider,
                                      info.ProviderKey, 
                                      isPersistent: false, 
                                      bypassTwoFactor: true
                                      );

            if (signInResult.Succeeded)
            {
                _logger.LogInformation(
                          "User {Email} logged in using {Provider}.",
                          email,
                          info.LoginProvider);

                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table,
            // the user may not have
            // a local account
            else
            {
                // Get the email claim value
                //var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            City = string.Empty
                        };

                        // Create the user in the database
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            AddIdentityErrors(createResult);
                            return View("Login", loginViewModel);
                        }
                        _logger.LogInformation(
                             "New user created using {Provider}. Email: {Email}",
                             info.LoginProvider,
                             email);
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        AddIdentityErrors(addLoginResult);
                        return View("Login", loginViewModel);
                    }
                    _logger.LogInformation(
                            "{Provider} login linked successfully for user {Email}.",
                            info.LoginProvider,
                            email);

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation(
                                    "User {Email} signed in successfully using {Provider}.",
                                    email,
                                    info.LoginProvider);

                    return LocalRedirect(returnUrl);
                    //return Content("Google Login Successful");
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on xyz@xyz.com";
                return View("Error");
            }
        }

        // Helper method to add identity errors to the ModelState
        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #region chatgpt generated code for external login
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback
        //    (
        //     string? returnUrl = null,
        //     string? remoteError = null)
        //{
        //    if (remoteError != null)
        //    {
        //        ModelState.AddModelError(
        //            "",
        //            $"Error from external provider: {remoteError}");

        //        return View("Login");
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync();

        //    if (info == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var result =
        //                await _signInManager.ExternalLoginSignInAsync(
        //                info.LoginProvider,
        //                info.ProviderKey,
        //                isPersistent: false,
        //                bypassTwoFactor: true);

        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(returnUrl);
        //    }

        //    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //    var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        //    var user = new ApplicationUser
        //    {
        //        UserName = email,
        //        Email = email,
        //        City = ""
        //    };

        //    var createResult = await _userManager.CreateAsync(user);
        //    if (createResult.Succeeded) {
        //        await _userManager.AddLoginAsync(user, info);
        //    }
        //    await _signInManager.SignInAsync(user, false);
        //    return RedirectToLocal(returnUrl);
        //}

        // Helper method to redirect to a
        // local URL or fallback to the home page
        //private IActionResult RedirectToLocal(string? returnUrl)
        //{
        //    if (!string.IsNullOrEmpty(returnUrl)
        //        && Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }

        //    return RedirectToAction("Index", "Home");
        //}
        #endregion
    }
}
