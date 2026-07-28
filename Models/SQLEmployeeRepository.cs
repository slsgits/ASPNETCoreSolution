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

        public Employee Delete(Employee employee)
        {
            _context.EmployeePhotos.RemoveRange(employee.Photos);
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
            return _context.Employees
                   .AsNoTracking()
                   .Include(e => e.Photos).
                   FirstOrDefault(e => e.Id == id)!;
        }

        public Employee Update(Employee employeeChanges)
        {
            _context.Employees.Update(employeeChanges);
            _context.SaveChanges();
            return employeeChanges;
        }
        public void DeleteEmployeePhotos(List<EmployeePhoto> photos)
        {
            _context.EmployeePhotos.RemoveRange(photos);
        }
    }
}
