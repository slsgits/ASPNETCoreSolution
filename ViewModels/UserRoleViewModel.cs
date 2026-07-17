namespace EmployeeManagement.ViewModels
{
    public class UserRoleViewModel
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
