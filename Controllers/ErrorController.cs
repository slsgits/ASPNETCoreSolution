using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class ErrorController(ILogger<ErrorController> logger) : Controller
    {
        private readonly ILogger<ErrorController> logger = logger;

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found";
                    logger.LogWarning("404 error occurred. Path = {Path} and QueryString = {QueryString}", statusCodeResult?.OriginalPath, statusCodeResult?.OriginalQueryString);
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Sorry, something went wrong on the server";
                    break;
            }
            return View("NotFound");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            logger.LogError("The path {Path} threw an exception {Exception}", exceptionDetails?.Path, exceptionDetails?.Error);
            return View("Error");
        }
    }
}
