using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Appointments;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<PagedResponse<AppointmentResponse>> GetAllAsync(AppointmentFilterViewModel filter, CancellationToken cancellationToken);

        Task<AppointmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);

        Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}