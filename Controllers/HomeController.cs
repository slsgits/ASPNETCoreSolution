using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController(IEmployeeRepository employeeRepository) : Controller
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;

        [Route("")]
        [Route("~/")]
        [Route("~/Home")]
        public ViewResult Index()
        {
            var employee = _employeeRepository.GetAllEmployees();
            return View(employee);
        }
        public ViewResult Create()
        {
           return View(); 
        }

        [Route("{id?}")]
        public ViewResult? Details(int? id)
        {
            //int id = 3;
            HomeDetailsViewModel homeDetailsViewModel = new()
            {
                Employee = _employeeRepository.GetEmployee(id??1),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }
    }
}
