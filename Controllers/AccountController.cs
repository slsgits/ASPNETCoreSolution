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
                var token = await _userManager
                           .GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                       new { userId = user.Id, token = token },
                                       Request.Scheme);

                _logger.Log(LogLevel.Warning,
                           "Email confirmation link generated: {ConfirmationLink}",
                           confirmationLink);

                _logger.LogInformation(
                       "New user registered: {Email}",
                        model.Email);

                if (_signInManager.IsSignedIn(User) &&
                    User.IsInRole("Admin"))
                {
                    return RedirectToAction("ListUsers", "Administration");
                }

                ViewBag.Title = "Registration successful";
                ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                        "email, by clicking on the confirmation link we have emailed you";
                return View("Error");
                //await _signInManager.SignInAsync(user, isPersistent: false);
                //return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("User registration failed for {Email}.", model.Email);

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

            // Check if the user exists and if their email is confirmed
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && !user.EmailConfirmed &&
                   (await userManager.CheckPasswordAsync(user, model.Password)))
            {
                ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                return View(model);
            }

            var result = await _signInManager
                        .PasswordSignInAsync(model.Email, model.Password,
                                             model.RememberMe,
                                             lockoutOnFailure: true);

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

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out : {Email}.", model.Email);
                ModelState.AddModelError("",
                "Your account has been locked due to multiple failed login " +
                "attempts. Please try again after 15 minutes or contact an " +
                "administrator.");
                return View(model);
            }

            _logger.LogWarning("User login failed : {Email}.", model.Email);

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
                _logger.LogWarning("External login failed. Provider Error: {RemoteError}", remoteError);

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
                    ApplicationUser? user = await _userManager.FindByEmailAsync(email);

                    // If email is not confirmed, display login view with validation error
                    if (user != null && !user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                        return View("Login", loginViewModel);
                    }

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

                        //token generate code here..
                        var token = await _userManager
                           .GenerateEmailConfirmationTokenAsync(user);

                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                               new { userId = user.Id, token = token },
                                               Request.Scheme);

                        _logger.Log(LogLevel.Warning,
                                   "Email confirmation link generated: {ConfirmationLink}",
                                   confirmationLink);

                        if (!createResult.Succeeded)
                        {
                            AddIdentityErrors(createResult);
                            return View("Login", loginViewModel);
                        }
                        _logger.LogInformation(
                             "New user created using {Provider}. Email: {Email}",
                             info.LoginProvider,
                             email);

                        //display registration successful message to user after email link send
                        ViewBag.Title = "Registration successful";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                                "email, by clicking on the confirmation link we have emailed you";
                        return View("Error");
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

        
        //method to confirm user registration 
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail
            (string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }

            //display error during email confirmation if any
            ViewBag.Title = "Email cannot be confirmed";
            ViewBag.ErrorMessage = string.Join
                                   (
                                     Environment.NewLine,
                                     result.Errors
                                     .Select(e => e.Description)
                                    );

            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword
            (ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                // If the user is found AND Email is confirmed
                if (user != null
                    && await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Check if the user has a local password
                    if (!await _userManager.HasPasswordAsync(user))
                    {
                        ViewBag.Title = "Password Reset Not Available";
                        ViewBag.ErrorMessage =
                            "This account uses an external login provider (Google, Facebook, Microsoft, etc.) and does not have a local password. " +
                            "Please sign in using your external account. If you want to use email and password login, first create a local password after signing in.";

                        return View("Error");
                    }

                    // Generate the reset password token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Build the password reset link
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token = token }, Request.Scheme);

                    // Log the password reset link
                    _logger.Log(LogLevel.Warning, passwordResetLink);

                    // Send the user to Forgot Password Confirmation view
                    //return View("ForgotPasswordConfirmation");
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If the token or email is null,
            // add a model error and return the view
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword
            (ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await _userManager
                        .ResetPasswordAsync(
                          user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword
            (ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (model.CurrentPassword == model.NewPassword)
                {
                    ModelState.AddModelError(
                        nameof(model.NewPassword),
                        "The new password must be different from your current password.");

                    return View(model);
                }
                // ChangePasswordAsync changes the user password
                var result = await _userManager
                    .ChangePasswordAsync
                    (
                     user,
                     model.CurrentPassword, 
                     model.NewPassword
                    );

                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    AddIdentityErrors(result);
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            return View(model);
        }
    }
}
