using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name="Role Name")]
        public required string RoleName { get; set; }
    }
}
