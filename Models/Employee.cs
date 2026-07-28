using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [NotMapped]
        public string EncryptedId { get; set; } = string.Empty;
        public required string Name { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }
        [Required]
        public required Dept? Department { get; set; }
        // One employee can have many photos
        public ICollection<EmployeePhoto> Photos { get; set; } = [];
    }
}
