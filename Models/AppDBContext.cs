using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) 
               : IdentityDbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeePhoto> EmployeePhotos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmployeePhoto>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Photos)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); ;

            modelBuilder.Seed();
        }
    }
}
