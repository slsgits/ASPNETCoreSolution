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
                 UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

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
    }
}
