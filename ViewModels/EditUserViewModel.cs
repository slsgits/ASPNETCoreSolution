using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Display(Name ="User Name")]
        public string UserName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public IList<string> Claims { get; set; } = [];
    }
}
