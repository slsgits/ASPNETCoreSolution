using System.Security.Claims;

namespace EmployeeManagement.Models
{
    public static class ClaimsStore
    {
        public static readonly List<Claim> AllClaims =
        [
            new Claim("Create Role", "true"),
            new Claim("Edit Role", "true"),
            new Claim("Delete Role", "true"),
            //"Create User",
            //"Edit User",
            //"Delete User"
        ];
    }
}
