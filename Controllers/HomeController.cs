using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webhostEnvironment) : Controller
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IWebHostEnvironment hostEnvironment = webhostEnvironment;

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
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<string> uploadedFileNames =  ProcessUploadedFiles(model.Photos);
                Employee newEmployee = new()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department
                };

                if (model.Photos != null && model.Photos.Count > 0)
                {
                    foreach (var fileName in uploadedFileNames)
                    {
                        newEmployee.Photos.Add(new EmployeePhoto
                        {
                            FileName = fileName
                        });
                    }
                }

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("Details", new { id = newEmployee.Id });
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

        private List<string> ProcessUploadedFiles(List<IFormFile>? photos)
        {
            List<string> uniqueFileNames = [];

            if (photos == null || photos.Count == 0)
                return uniqueFileNames;

            string uploadsFolder = Path.Combine(hostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var photo in photos)
            {
                if (photo.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    photo.CopyTo(fileStream);

                    uniqueFileNames.Add(uniqueFileName);
                }
            }

            return uniqueFileNames;
        }
    }
}
