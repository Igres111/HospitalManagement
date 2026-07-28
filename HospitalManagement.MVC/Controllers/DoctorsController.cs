using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Services;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace HospitalManagement.MVC.Controllers
{

    public class DoctorsController : Controller
    {
        private readonly IApiClient _apiClient;

        public DoctorsController(IApiClient apiClient)
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
            var url = BuildDoctorsQueryUrl(search, sortBy, sortDescending, pageNumber, pageSize);

            var page = await _apiClient.GetAsync<PagedResponse<DoctorResponse>>(url, cancellationToken);

            return View(new DoctorIndexViewModel
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
                var doctor = await _apiClient.GetAsync<DoctorResponse>($"api/doctors/{id}", cancellationToken);
                return View(doctor);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

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

            try
            {
                var request = new CreateDoctorRequest(
                    model.FirstName,
                    model.LastName,
                    model.Specialization,
                    model.PhoneNumber,
                    model.Email);

                var created = await _apiClient.PostAsync<CreateDoctorRequest, DoctorResponse>("api/doctors", request, cancellationToken);

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
                var doctor = await _apiClient.GetAsync<DoctorResponse>($"api/doctors/{id}", cancellationToken);

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
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
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

            try
            {
                var request = new UpdateDoctorRequest(
                    model.FirstName,
                    model.LastName,
                    model.Specialization,
                    model.PhoneNumber,
                    model.Email);

                await _apiClient.PutAsync<UpdateDoctorRequest, DoctorResponse>(
                    $"api/doctors/{id}", request, cancellationToken);

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
                await _apiClient.DeleteAsync($"api/doctors/{id}", cancellationToken);
            }
            catch (ApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static string BuildDoctorsQueryUrl(
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

            return QueryHelpers.AddQueryString("api/doctors", queryParams);
        }
    }
}