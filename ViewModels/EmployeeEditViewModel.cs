using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.ViewModels
{
    public class EmployeeEditViewModel : EmployeeCreateViewModel
    {
        public int Id { get; set; }

        [NotMapped]
        public string EncryptedId { get; set; } = string.Empty;
        public List<string> ExistingPhotoPaths { get; set; } = [];
    }
}
