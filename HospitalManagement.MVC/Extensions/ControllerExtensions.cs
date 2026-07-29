using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.MVC.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectToApiError(this Controller controller, IApiResult result)
        {
            controller.TempData["ErrorStatusCode"] = result.StatusCode;
            controller.TempData["ErrorMessage"] = result.ErrorMessage;

            return controller.RedirectToAction("Error", "Home");
        }
    }
}
