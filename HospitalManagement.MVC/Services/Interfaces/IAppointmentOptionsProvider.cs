using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Services.Interfaces;

public interface IAppointmentOptionsProvider
{
    Task<IEnumerable<SelectListItem>> GetDoctorOptionsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SelectListItem>> GetPatientOptionsAsync(CancellationToken cancellationToken);
    IEnumerable<SelectListItem> GetStatusOptions();
}
