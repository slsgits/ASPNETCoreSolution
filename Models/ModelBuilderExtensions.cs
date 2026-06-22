using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee 
                { 
                    Id = 1, 
                    Name = "Ganesh", 
                    Email = "ganesh@example.com",
                    Department = Dept.HR
                },
                new Employee
                {
                    Id = 2,
                    Name = "Mangesh",
                    Email = "mangesh@example.com",
                    Department = Dept.IT
                }
            );
        }
    }
}
