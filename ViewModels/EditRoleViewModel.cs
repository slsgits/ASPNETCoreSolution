using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class EditRoleViewModel
    {
        public required string Id { get; set; }
        [Required]
        [Display(Name = "Role Name")]
        public required string RoleName { get; set; }
        public required List<string> Users { get; set; } = [];
    }
}
