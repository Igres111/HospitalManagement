using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Appointments;
using HospitalManagement.MVC.Services;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<ApiResult<PagedResponse<AppointmentResponse>>> GetAllAsync(AppointmentFilterViewModel filter, CancellationToken cancellationToken);

        Task<ApiResult<AppointmentResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<ApiResult<AppointmentResponse>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);

        Task<ApiResult<AppointmentResponse>> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken);

        Task<ApiResult> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
