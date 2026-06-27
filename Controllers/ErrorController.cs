using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found";
                    ViewBag.Path = statusCodeResult?.OriginalPath;
                    ViewBag.QS = statusCodeResult?.OriginalQueryString;
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Sorry, something went wrong on the server";
                    break;
            }
            return View("NotFound");
        }
    }
}
