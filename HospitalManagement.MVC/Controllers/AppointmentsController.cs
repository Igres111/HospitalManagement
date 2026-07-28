using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Models.Appointments;
using HospitalManagement.MVC.Services;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.MVC.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentOptionsProvider _appointmentOptionsProvider;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentOptionsProvider appointmentOptionsProvider)
        {
            _appointmentService = appointmentService;
            _appointmentOptionsProvider = appointmentOptionsProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AppointmentFilterViewModel filter, CancellationToken cancellationToken)
        {
            var page = await _appointmentService.GetAllAsync(filter, cancellationToken);

            return View(new AppointmentIndexViewModel
            {
                Filter = filter,
                Page = page,
                DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken),
                StatusOptions = _appointmentOptionsProvider.GetStatusOptions()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);

            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            return View(new AppointmentCreateViewModel
            {
                AppointmentDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                model.DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken);
                return View(model);
            }

            var request = new CreateAppointmentRequest(model.DoctorId, model.PatientId, model.AppointmentDate);

            try
            {
                var created = await _appointmentService.CreateAsync(request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (ApiException ex)
            {
                model.DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken);
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Create Appointment";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);

            return View(new AppointmentEditViewModel
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken),
                StatusOptions = _appointmentOptionsProvider.GetStatusOptions()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken);
                model.StatusOptions = _appointmentOptionsProvider.GetStatusOptions();
                return View(model);
            }

            var request = new UpdateAppointmentRequest(model.DoctorId, model.PatientId, model.AppointmentDate, model.Status);

            try
            {
                await _appointmentService.UpdateAsync(id, request, cancellationToken);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                model.DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken);
                model.StatusOptions = _appointmentOptionsProvider.GetStatusOptions();
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Update Appointment";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _appointmentService.DeleteAsync(id, cancellationToken);

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                var filter = new AppointmentFilterViewModel();
                var page = await _appointmentService.GetAllAsync(filter, cancellationToken);
                ViewBag.ApiErrorMessage = ex.Message;
                ViewBag.ApiErrorModalTitle = "Unable to Delete Appointment";
                return View(nameof(Index), new AppointmentIndexViewModel
                {
                    Filter = filter,
                    Page = page,
                    DoctorOptions = await _appointmentOptionsProvider.GetDoctorOptionsAsync(cancellationToken),
                    PatientOptions = await _appointmentOptionsProvider.GetPatientOptionsAsync(cancellationToken),
                    StatusOptions = _appointmentOptionsProvider.GetStatusOptions()
                });
            }
        }
    }
}
