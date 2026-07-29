using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Extensions;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.MVC.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PatientFilterViewModel filter, CancellationToken cancellationToken)
        {
            var result = await _patientService.GetAllAsync(filter, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            return View(new PatientIndexViewModel
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
            var result = await _patientService.GetByIdAsync(id, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PatientCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _patientService.CreateAsync(model, cancellationToken);

            if (result.IsError)
            {
                ViewBag.ApiErrorMessage = result.ErrorMessage;
                ViewBag.ApiErrorModalTitle = "Unable to Create Patient";
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var result = await _patientService.GetByIdAsync(id, cancellationToken);

            if (result.IsError)
            {
                return this.RedirectToApiError(result);
            }

            var patient = result.Value!;

            return View(new PatientEditViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _patientService.UpdateAsync(id, model, cancellationToken);

            if (result.IsError)
            {
                ViewBag.ApiErrorMessage = result.ErrorMessage;
                ViewBag.ApiErrorModalTitle = "Unable to Update Patient";
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleteResult = await _patientService.DeleteAsync(id, cancellationToken);

            if (!deleteResult.IsError)
            {
                return RedirectToAction(nameof(Index));
            }

            var filter = new PatientFilterViewModel();
            var pageResult = await _patientService.GetAllAsync(filter, cancellationToken);

            if (pageResult.IsError)
            {
                return this.RedirectToApiError(pageResult);
            }

            ViewBag.ApiErrorMessage = deleteResult.ErrorMessage;
            ViewBag.ApiErrorModalTitle = "Unable to Delete Patient";

            return View(nameof(Index), new PatientIndexViewModel
            {
                Page = pageResult.Value!,
                Search = filter.Search,
                SortBy = filter.SortBy,
                SortDescending = filter.SortDescending
            });
        }
    }
}
