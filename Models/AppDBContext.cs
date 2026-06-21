using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) 
               : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
    }
}
