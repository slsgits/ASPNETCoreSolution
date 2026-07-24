using System.Security.Claims;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler :
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement,string>
    {
        protected override Task HandleRequirementAsync
        (
            AuthorizationHandlerContext context, 
            ManageAdminRolesAndClaimsRequirement requirement, 
            string userIdBeingEdited)
        {
            //check if user has the required claim
            var loggedInUserId =
                context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            //check if logged in user is trying to edit their
            //own role and claims
            if (loggedInUserId != null &&
                loggedInUserId != userIdBeingEdited)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
