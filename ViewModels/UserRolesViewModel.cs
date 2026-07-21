using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class UserRolesViewModel
    {
        [Required]
        public required string RoleId { get; set; }
        public required string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
