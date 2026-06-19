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
        [HttpGet]
        public ViewResult Create()
        {
           return View(); 
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee = _employeeRepository.Add(employee);
                return RedirectToAction("Details", new { id = employee.Id });
            }
            return View();
        }

        [Route("{id?}")]
        public ViewResult Details(int? id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new()
            {
                Employee = _employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }
    }
}
