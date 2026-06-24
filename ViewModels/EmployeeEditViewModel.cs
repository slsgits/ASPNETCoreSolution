namespace EmployeeManagement.ViewModels
{
    public class EmployeeEditViewModel : EmployeeCreateViewModel
    {
        public int Id { get; set; }
        public List<string> ExistingPhotoPaths { get; set; } = [];
    }
}
