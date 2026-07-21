using System.Data;
using System.Security.Claims;
using AspNetCoreGeneratedDocument;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]         //case 1 : User should have Admin role.
    //[Authorize(Roles = "Admin, User")] //case 2 : user should have Admin or User role
    //[Authorize(Roles = "Admin")]       //case 3 : user should have both Admin and User role
    //[Authorize(Roles = "User")]        //case 3 : user should have both Admin and User role

    public class AdministrationController(
                 RoleManager<IdentityRole> roleManager,
                 UserManager<ApplicationUser> userManager,
                 ILogger<AdministrationController> logger) : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<AdministrationController> _logger = logger;

        [HttpGet]
        public IActionResult TestRole()
        {
            return Content(User.IsInRole("Admin").ToString());
        }
        [HttpGet]
        public IActionResult ListRoles()
        {
            return View(_roleManager.Roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new()
                {
                    Name = model.RoleName
                };

                IdentityResult result = await _roleManager.CreateAsync(identityRole);
                
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id) { 
          var role = await _roleManager.FindByIdAsync(id);
            if (role == null) { 
             ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name ?? string.Empty,
                Users = []
            };

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty))
                {
                    model.Users.Add(user.UserName ?? string.Empty);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
   
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            // Get the role ID from the query string
            ViewBag.RoleId = roleId;

            // Find the role by ID
            var role = await _roleManager.FindByIdAsync(roleId);

            // If the role is not found, return a NotFound view with an error message
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            // Create a list of UserRoleViewModel to hold the users and their role selection status
            var model = new List<UserRoleViewModel>();

            // Iterate through all users and check if they are in the specified role
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                // Create a UserRoleViewModel for each user and set the properties accordingly
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty)
                };

                // Add the UserRoleViewModel to the model list
                model.Add(userRoleViewModel);
            }

            // Return the view with the model containing the list of users and their role selection status
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            // Find the role by ID
            var role = await _roleManager.FindByIdAsync(roleId);

            // If the role is not found or the role name is null or empty,
            // return a NotFound view with an error message
            if (role == null || string.IsNullOrEmpty(role.Name))
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            // Iterate through the list of UserRoleViewModel to update
            // the users' role selection status
            for (int i = 0; i < model.Count; i++)
            {
                // Find the user by ID
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                
                // If the user is not found, continue to the next iteration
                if (user == null)
                {
                    continue;
                }

                // Check if the user is selected for the role and update their role membership accordingly
                IdentityResult result;
                // If the user is selected and not already in the role,
                // add them to the role
                if (model[i].IsSelected && 
                    !await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                } // If the user is not selected and is currently in the role,
                else if (!model[i].IsSelected && 
                         await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else // If the user's selection status has not changed, continue to the next iteration
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            try
            {
                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListRoles");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error deleting role: {ex}");
                ViewBag.Title = $"{role.Name} role is in use.";
                ViewBag.ErrorMessage = $"The {role.Name} role cannot be deleted as there are users in this role. " +
                                       $"If you want to delete this role, please remove the users from the role and then try to delete.";
                return View("Error");
            }
        }
        [HttpGet]
        public IActionResult ListUsers()
        {
            return View(_userManager.Users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                City = user.City ?? string.Empty,
                Roles = userRoles.ToList(),
                Claims = [.. userClaims.Select(c => new UserClaim { ClaimType = c.Type })]
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        // Post: Administration/DeleteUser/5
        // This action method displays a confirmation view for deleting a user.
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListUsers");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error deleting user: {ex}");
                ViewBag.Title = $"{user.UserName} user is in use.";
                ViewBag.ErrorMessage = $"The {user.UserName} user cannot be deleted as it is linked with other data." +
                                       $"If you want to delete this user, please remove the dependencies from the user and then try to delete.";

                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty)
                };
                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> userRolesViewModels,string userId)
        {
            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);

            // If the user is not found, return a NotFound view with an error message
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // Get the current roles of the user
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove the user from all current roles
            var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // If the removal of roles failed, add an error message to the ModelState
            // and return the view with the userRolesViewModels
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(userRolesViewModels);
            }

            // Get the selected roles from the userRolesViewModels
            var selectedRoles = userRolesViewModels.Where(x => x.IsSelected)
                                                   .Select(y => y.RoleName)
                                                   .ToList();

            // Add the user to the selected roles
            result = await _userManager.AddToRolesAsync(user, selectedRoles);

            // If the addition of roles failed, add an error message to the ModelState
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(userRolesViewModels);
            }

            // If everything succeeded, redirect to the EditUser action with the userId
            return RedirectToAction("EditUser", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            //get user by ID
            var user = await _userManager.FindByIdAsync(userId);

            //Check if user is null
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            //user claim view model instance creation
            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            // Loop through each claim we have in our application
            foreach (string claimType in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new()
                {
                    ClaimType = claimType,
                    // If the user has the claim, set IsSelected property
                    // to true, so the checkbox
                    // next to the claim is checked on the UI
                    IsSelected = existingUserClaims.Any(c => c.Type == claimType)
                };
                model.Claims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            //Get user by Id
            var user = await _userManager.FindByIdAsync(model.UserId);

            //check if user is null
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            // Get all the user existing claims and delete them
            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            // If the removal of claims failed, add an error message
            // to the ModelState
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            // Add all the claims that are selected on the UI
            result = await _userManager
                    .AddClaimsAsync(user,
                    model.Claims.Where(c => c.IsSelected)
                    .Select(c => new Claim(c.ClaimType, c.ClaimType)));

            // If the addition of claims failed, add an error message
            // to the ModelState
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });

        }
    }
}
