using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace EmployeeManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];
    }
}
