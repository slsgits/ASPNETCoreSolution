using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public required string Name { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }
        [Required]
        public required Dept? Department { get; set; }
        public List<IFormFile>? Photos { get; set; }
    }
}
