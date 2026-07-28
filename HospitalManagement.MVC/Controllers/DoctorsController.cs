using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Services;
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
            var page = await _doctorService.GetAllAsync(filter, cancellationToken);

            return View(new DoctorIndexViewModel
            {
                Page = page,
                Search = filter.Search,
                SortBy = filter.SortBy,
                SortDescending = filter.SortDescending
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var doctor = await _doctorService.GetByIdAsync(id, cancellationToken);

            return View(doctor);
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

            var request = new CreateDoctorRequest(
                model.FirstName,
                model.LastName,
                model.Specialization,
                model.PhoneNumber,
                model.Email);

            try
            {
                var created = await _doctorService.CreateAsync(request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (ApiException ex)
            {
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Create Doctor";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var doctor = await _doctorService.GetByIdAsync(id, cancellationToken);

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

            var request = new UpdateDoctorRequest(
                model.FirstName,
                model.LastName,
                model.Specialization,
                model.PhoneNumber,
                model.Email);

            try
            {
                await _doctorService.UpdateAsync(id, request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Update Doctor";
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
                await _doctorService.DeleteAsync(id, cancellationToken);

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                var filter = new DoctorFilterViewModel();
                var page = await _doctorService.GetAllAsync(filter, cancellationToken);
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Delete Doctor";
                return View(nameof(Index), new DoctorIndexViewModel
                {
                    Page = page,
                    Search = filter.Search,
                    SortBy = filter.SortBy,
                    SortDescending = filter.SortDescending
                });
            }
        }
    }
}
