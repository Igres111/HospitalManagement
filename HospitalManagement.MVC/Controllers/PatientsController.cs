using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
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

        public async Task<IActionResult> Index(PatientFilterViewModel filter, CancellationToken cancellationToken)
        {
            var page = await _patientService.GetAllAsync(filter, cancellationToken);

            return View(new PatientIndexViewModel
            {
                Page = page,
                Search = filter.Search,
                SortBy = filter.SortBy,
                SortDescending = filter.SortDescending
            });
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);

            return View(patient);
        }

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

            var request = new CreatePatientRequest(
                model.FirstName,
                model.LastName,
                model.DateOfBirth,
                model.PhoneNumber,
                model.Email);

            var created = await _patientService.CreateAsync(request, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);

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

            var request = new UpdatePatientRequest(
                model.FirstName,
                model.LastName,
                model.DateOfBirth,
                model.PhoneNumber,
                model.Email);

            await _patientService.UpdateAsync(id, request, cancellationToken);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _patientService.DeleteAsync(id, cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
