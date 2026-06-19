
namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList =
            [
                new() { Id = 1, Name = "John Doe", Email = "JohnDoe@gmail.com", Department = Dept.IT },
                new() { Id = 2, Name = "Cohn Doe", Email = "CohnDoe@gmail.com", Department = Dept.HR },
                new() { Id = 3, Name = "Dohn Doe", Email = "DohnDoe@gmail.com", Department = Dept.IT }
            ];
        }

        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id) ?? 
                throw new Exception("Employee not found"); ;
        }
    }
}
