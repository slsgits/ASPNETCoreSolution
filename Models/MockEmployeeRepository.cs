namespace EmployeeManagement.Models
{
    //In-Memory Employee Repository
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

        public Employee Delete(int id)
        {
            Employee? employee = _employeeList.FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee!;
        }

        public void DeleteEmployeePhotos(List<EmployeePhoto> photos)
        {
            throw new NotImplementedException();
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

        public Employee Update(Employee employeeChanges)
        {
            Employee? employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if (employee != null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }
            return employee!;
        }
    }
}
