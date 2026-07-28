using HospitalManagement.MVC.Dtos;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Services;

public class AppointmentOptionsProvider : IAppointmentOptionsProvider
{
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public AppointmentOptionsProvider(IDoctorService doctorService, IPatientService patientService)
    {
        _doctorService = doctorService;
        _patientService = patientService;
    }

    public async Task<IEnumerable<SelectListItem>> GetDoctorOptionsAsync(CancellationToken cancellationToken)
    {
        var doctors = await _doctorService.GetAllAsync(
            new DoctorFilterViewModel { PageSize = 50 },
            cancellationToken);

        return doctors.Items.Select(doctor => new SelectListItem(
            $"{doctor.FirstName} {doctor.LastName}",
            doctor.Id.ToString()));
    }

    public async Task<IEnumerable<SelectListItem>> GetPatientOptionsAsync(CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAllAsync(
            new PatientFilterViewModel { PageSize = 50 },
            cancellationToken);

        return patients.Items.Select(patient => new SelectListItem(
            $"{patient.FirstName} {patient.LastName}",
            patient.Id.ToString()));
    }

    public IEnumerable<SelectListItem> GetStatusOptions()
    {
        return Enum.GetValues<AppointmentStatus>().Select(status => new SelectListItem(status.ToString(), status.ToString()));
    }
}
