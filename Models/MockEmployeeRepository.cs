
namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList =
            [
                new() { Id = 1, Name = "John Doe", Email = "JohnDoe@gmail.com", Department = "IT" },
                new() { Id = 2, Name = "Cohn Doe", Email = "CohnDoe@gmail.com", Department = "HR" },
                new() { Id = 3, Name = "Dohn Doe", Email = "DohnDoe@gmail.com", Department = "IT" }
            ];
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee? GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }
    }
}
