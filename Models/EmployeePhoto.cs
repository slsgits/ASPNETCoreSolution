namespace EmployeeManagement.Models
{
    public class EmployeePhoto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
