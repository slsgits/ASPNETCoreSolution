using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController(IEmployeeRepository employeeRepository, 
                                IWebHostEnvironment webhostEnvironment,
                                ILogger<HomeController> logger) 
               : Controller
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IWebHostEnvironment _hostEnvironment = webhostEnvironment;
        private readonly ILogger<HomeController> _logger = logger;

        [Route("")]
        [Route("~/")]
        [Route("~/Home")]
        public ViewResult Index()
        {
            _logger.LogInformation("Employee list requested.");
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
                _logger.LogWarning("Edit failed. Employee with Id {EmployeeId} was not found.",id);
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
            try
            {
                // reload existing photos if validation fails
                Employee? employee = _employeeRepository.GetEmployee(model.Id);

                if (employee == null)
                {
                    _logger.LogWarning("Edit failed. Employee with Id {EmployeeId} was not found.", model.Id);
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validation failed while editing employee {EmployeeId}.", model.Id);
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
                _logger.LogInformation("Employee {EmployeeId} updated successfully.", employee.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error while updating employee {EmployeeId}.",model.Id);
                throw;
            }
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<string> uploadedFileNames = ProcessUploadedFiles(model.Photos);
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
                    _logger.LogInformation("Employee created successfully with Id {EmployeeId}", newEmployee.Id);
                    return RedirectToAction("Details", new { id = newEmployee.Id });
                }
                _logger.LogWarning("Employee creation failed because model validation failed.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while addming employee.");
                throw;
            }
        }

        [Route("{id?}")]
        public ViewResult Details(int? id)
        {
            try
            {
                Employee employee = _employeeRepository.GetEmployee(id ?? 1);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with Id {EmployeeId} was not found.", id);
                    Response.StatusCode = 404;
                    return View("EmployeeNotFound", id);
                }

                HomeDetailsViewModel homeDetailsViewModel = new()
                {
                    Employee = employee,
                    PageTitle = "Employee Details"
                };

                return View(homeDetailsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employee details for Id {EmployeeId}.", id);
                throw; // Re-throw the exception to be handled by the global exception handler
            }
        }

        private List<string> ProcessUploadedFiles(List<IFormFile>? photos)
        {
            List<string> uniqueFileNames = [];

            if (photos == null || photos.Count == 0)
                return uniqueFileNames;

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");

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
                    try
                    {
                        photo.CopyTo(fileStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,"Failed to save uploaded photo '{FileName}'.",photo.FileName);
                        throw;
                    }
                    uniqueFileNames.Add(uniqueFileName);
                }
            }

            return uniqueFileNames;
        }

        private void DeletePhotoFiles(List<string> existingPhotoPaths)
        {
            if (existingPhotoPaths == null || existingPhotoPaths.Count == 0)
                return;

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");

            foreach (var photoPath in existingPhotoPaths)
            {
                if (!string.IsNullOrEmpty(photoPath))
                {
                    string filePath = Path.Combine(uploadsFolder, photoPath);

                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete photo file '{FilePath}'.", filePath);
                            // Optionally, you can choose to continue or throw the exception based on your requirements
                        }
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee = _employeeRepository.GetEmployee(id);
            if (employee == null)
            {
                _logger.LogWarning("Delete failed. Employee with Id {EmployeeId} was not found.", id);
                return NotFound();
            }
            _logger.LogInformation("Delete confirmation requested for employee {EmployeeId}.",id);
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var employee = _employeeRepository.GetEmployee(id);
                if (employee == null)
                {
                    _logger.LogWarning("Delete failed. Employee with Id {EmployeeId} was not found.", id);
                    return NotFound();
                }

                // 1. Delete physical photo files
                var photoPaths = employee.Photos.Select(p => p.FileName).ToList();
                DeletePhotoFiles(photoPaths);

                // 2. Delete employee record from DB
                _employeeRepository.Delete(employee);
                _logger.LogInformation("Employee {EmployeeId} deleted successfully.", id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while deleting employee {EmployeeId}.",id);
                throw;
            }
        }
    }
}
