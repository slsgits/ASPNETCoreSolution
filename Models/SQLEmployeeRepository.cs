using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    public class SQLEmployeeRepository(AppDBContext context) : IEmployeeRepository
    {
        private readonly AppDBContext _context = context;

        public Employee Add(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
                return null!;

            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _context.Employees.AsNoTracking()
                                     .Include(e => e.Photos)
                                     .ToList();
        }

        public Employee GetEmployee(int id)
        {
            return _context.Employees.AsNoTracking().
                   Include(e => e.Photos).
                   FirstOrDefault(e => e.Id == id)!;
        }

        public Employee Update(Employee employeeChanges)
        {
            var employee = _context.Employees.Find(employeeChanges.Id) 
                         ?? throw new InvalidOperationException($"Employee with Id {employeeChanges.Id} not found.");
            employee.Name = employeeChanges.Name;
            employee.Email = employeeChanges.Email;
            employee.Department = employeeChanges.Department;

            _context.SaveChanges();
            return employee;
        }
}
}
