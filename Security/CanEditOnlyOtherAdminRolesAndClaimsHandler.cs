using System.Security.Claims;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler(UserManager<ApplicationUser> userManager) :
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement,string>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        protected override async Task HandleRequirementAsync
        (
            AuthorizationHandlerContext context, 
            ManageAdminRolesAndClaimsRequirement requirement, 
            string userIdBeingEdited)
        {
            // Get the logged in user id
            var loggedInUserId =
                context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if logged in user id is null, return
            if (loggedInUserId == null)
            {
                //await Task.CompletedTask;
                return;
            }

            // Rule 1: Super Admin can manage anyone (including themselves if you want)
            if (context.User.IsInRole("Super Admin"))
            {
                context.Succeed(requirement);
            }

            // Find the user being edited
            var targetUser =
                await _userManager.FindByIdAsync(userIdBeingEdited);

            if (targetUser == null)
                return;

            // Rule 2 : Admin cannot manage Super Admin
            if (await _userManager.IsInRoleAsync(targetUser, "Super Admin"))
            {
                return;
            }

            // Rule 3 : Admin cannot manage themselves
            if (loggedInUserId == userIdBeingEdited)
            {
                return;
            }

            // Rule 4 : Admin must have Edit Role claim
            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim("Edit Role", "true"))
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
