using HospitalManagement.MVC.Dtos;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Models.Appointments;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IDoctorService doctorService,
            IPatientService patientService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _patientService = patientService;
        }

        public async Task<IActionResult> Index(AppointmentFilterViewModel filter, CancellationToken cancellationToken)
        {
            var page = await _appointmentService.GetAllAsync(filter, cancellationToken);

            return View(new AppointmentIndexViewModel
            {
                Filter = filter,
                Page = page,
                DoctorOptions = await GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await GetPatientOptionsAsync(cancellationToken),
                StatusOptions = GetStatusOptions()
            });
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);

            return View(appointment);
        }

        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            return View(new AppointmentCreateViewModel
            {
                AppointmentDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                DoctorOptions = await GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await GetPatientOptionsAsync(cancellationToken)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                model.DoctorOptions = await GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await GetPatientOptionsAsync(cancellationToken);
                return View(model);
            }

            var request = new CreateAppointmentRequest(model.DoctorId, model.PatientId, model.AppointmentDate);

            var created = await _appointmentService.CreateAsync(request, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

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
                DoctorOptions = await GetDoctorOptionsAsync(cancellationToken),
                PatientOptions = await GetPatientOptionsAsync(cancellationToken),
                StatusOptions = GetStatusOptions()
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
                model.DoctorOptions = await GetDoctorOptionsAsync(cancellationToken);
                model.PatientOptions = await GetPatientOptionsAsync(cancellationToken);
                model.StatusOptions = GetStatusOptions();
                return View(model);
            }

            var request = new UpdateAppointmentRequest(model.DoctorId, model.PatientId, model.AppointmentDate, model.Status);

            await _appointmentService.UpdateAsync(id, request, cancellationToken);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _appointmentService.DeleteAsync(id, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetDoctorOptionsAsync(CancellationToken cancellationToken)
        {
            var doctors = await _doctorService.GetAllAsync(
                new DoctorFilterViewModel { PageSize = 50 },
                cancellationToken);

            return doctors.Items.Select(doctor => new SelectListItem(
                $"{doctor.FirstName} {doctor.LastName}",
                doctor.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetPatientOptionsAsync(CancellationToken cancellationToken)
        {
            var patients = await _patientService.GetAllAsync(
                new PatientFilterViewModel { PageSize = 50 },
                cancellationToken);

            return patients.Items.Select(patient => new SelectListItem(
                $"{patient.FirstName} {patient.LastName}",
                patient.Id.ToString()));
        }

        private static IEnumerable<SelectListItem> GetStatusOptions()
        {
            return Enum.GetValues<AppointmentStatus>().Select(status => new SelectListItem(status.ToString(), status.ToString()));
        }
    }
}