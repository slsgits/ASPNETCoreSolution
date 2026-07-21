using EmployeeManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class UserClaimsViewModel
    {
        public required string UserId { get; set; }
        public List<UserClaim> Claims { get; set; } = [];
    }
}
