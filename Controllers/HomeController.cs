using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class HomeController(IEmployeeRepository employeeRepository) : Controller
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;

        public ViewResult Index()
        {
            var employee = _employeeRepository.GetAllEmployees();
            return View(employee);
        }
        public ViewResult? Details()
        {
            int id = 1;
            HomeDetailsViewModel homeDetailsViewModel = new()
            {
                Employee = _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }
    }
}
