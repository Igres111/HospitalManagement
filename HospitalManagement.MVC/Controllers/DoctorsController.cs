using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Extensions;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.MVC.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DoctorFilterViewModel filter, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAllAsync(filter, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            return View(new DoctorIndexViewModel
            {
                Page = result.Value!,
                Search = filter.Search,
                SortBy = filter.SortBy,
                SortDescending = filter.SortDescending
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetByIdAsync(id, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new DoctorCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _doctorService.CreateAsync(model, cancellationToken);

            if (result.IsError)
            {
                ViewBag.ApiErrorMessage = result.ErrorMessage;
                ViewBag.ApiErrorModalTitle = "Unable to Create Doctor";
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetByIdAsync(id, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            var doctor = result.Value!;

            return View(new DoctorEditViewModel
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _doctorService.UpdateAsync(id, model, cancellationToken);

            if (result.IsError)
            {
                ViewBag.ApiErrorMessage = result.ErrorMessage;
                ViewBag.ApiErrorModalTitle = "Unable to Update Doctor";
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleteResult = await _doctorService.DeleteAsync(id, cancellationToken);

            if (!deleteResult.IsError)
            {
                return RedirectToAction(nameof(Index));
            }

            var filter = new DoctorFilterViewModel();
            var pageResult = await _doctorService.GetAllAsync(filter, cancellationToken);

            if (pageResult.IsError)
            {
                return this.RedirectToApiError(pageResult);
            }

            ViewBag.ApiErrorMessage = deleteResult.ErrorMessage;
            ViewBag.ApiErrorModalTitle = "Unable to Delete Doctor";

            return View(nameof(Index), new DoctorIndexViewModel
            {
                Page = pageResult.Value!,
                Search = filter.Search,
                SortBy = filter.SortBy,
                SortDescending = filter.SortDescending
            });
        }
    }
}
