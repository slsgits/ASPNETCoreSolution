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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employeeRepository.GetEmployee(id);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPaths = employee.Photos.Select(p => p.FileName).ToList()
            };

            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            // reload existing photos if validation fails
            Employee? employee = _employeeRepository.GetEmployee(model.Id);

            if (employee == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ExistingPhotoPaths = employee.Photos.Select(p => p.FileName).ToList();
                return View(model);
            }

            // update employee basic fields
            employee.Name = model.Name;
            employee.Email = model.Email;
            employee.Department = model.Department;

            if (model.Photos != null && model.Photos.Count > 0)
            {
                // 1. Delete old physical files
                DeletePhotoFiles(model.ExistingPhotoPaths.ToList());

                // 2. Delete old photo records from DB
                _employeeRepository.DeleteEmployeePhotos(employee.Photos.ToList());

                // 3. Save new files
                List<string> uploadedFileNames = ProcessUploadedFiles(model.Photos);

                // 4. Replace employee photos with new ones
                employee.Photos = uploadedFileNames.Select(fileName => new EmployeePhoto
                {
                    FileName = fileName
                }).ToList();
            }

            _employeeRepository.Update(employee);

            return RedirectToAction("Index");
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

        private void DeletePhotoFiles(List<string> existingPhotoPaths)
        {
            if (existingPhotoPaths == null || !existingPhotoPaths.Any())
                return;

            string uploadsFolder = Path.Combine(hostEnvironment.WebRootPath, "images");

            foreach (var photoPath in existingPhotoPaths)
            {
                if (!string.IsNullOrEmpty(photoPath))
                {
                    string filePath = Path.Combine(uploadsFolder, photoPath);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }
    }
}
