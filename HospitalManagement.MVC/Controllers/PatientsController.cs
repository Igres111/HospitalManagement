using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace HospitalManagement.MVC.Controllers
{

    public class PatientsController : Controller
    {
        private readonly IApiClient _apiClient;

        public PatientsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? sortBy,
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var url = BuildPatientsQueryUrl(search, sortBy, sortDescending, pageNumber, pageSize);

            var page = await _apiClient.GetAsync<PagedResponse<PatientResponse>>(url, cancellationToken);

            return View(new PatientIndexViewModel
            {
                Page = page,
                Search = search,
                SortBy = sortBy,
                SortDescending = sortDescending
            });
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            try
            {
                var patient = await _apiClient.GetAsync<PatientResponse>($"api/patients/{id}", cancellationToken);
                return View(patient);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
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

            try
            {
                var request = new CreatePatientRequest(
                    model.FirstName,
                    model.LastName,
                    model.DateOfBirth,
                    model.PhoneNumber,
                    model.Email);

                var created = await _apiClient.PostAsync<CreatePatientRequest, PatientResponse>(
                    "api/patients", request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            try
            {
                var patient = await _apiClient.GetAsync<PatientResponse>($"api/patients/{id}", cancellationToken);

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
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
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

            try
            {
                var request = new UpdatePatientRequest(
                    model.FirstName,
                    model.LastName,
                    model.DateOfBirth,
                    model.PhoneNumber,
                    model.Email);

                await _apiClient.PutAsync<UpdatePatientRequest, PatientResponse>(
                    $"api/patients/{id}", request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.DeleteAsync($"api/patients/{id}", cancellationToken);
            }
            catch (ApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static string BuildPatientsQueryUrl(
            string? search, string? sortBy, bool sortDescending, int pageNumber, int pageSize)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["sortDescending"] = sortDescending.ToString()
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams["search"] = search;
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                queryParams["sortBy"] = sortBy;
            }

            return QueryHelpers.AddQueryString("api/patients", queryParams);
        }
    }
}