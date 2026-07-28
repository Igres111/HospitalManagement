using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Appointments;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<PagedResponse<AppointmentResponse>> GetAllAsync(AppointmentFilterViewModel filter, CancellationToken cancellationToken = default);

        Task<AppointmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);

        Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}