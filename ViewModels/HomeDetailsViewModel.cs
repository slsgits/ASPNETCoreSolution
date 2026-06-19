using EmployeeManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class HomeDetailsViewModel
    {
        public required Employee Employee { get; set; }
        public required string PageTitle { get; set; }
    }
}
