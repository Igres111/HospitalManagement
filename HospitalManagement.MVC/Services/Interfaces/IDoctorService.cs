using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Services;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<ApiResult<PagedResponse<DoctorResponse>>> GetAllAsync(
            DoctorFilterViewModel filter,
            CancellationToken cancellationToken);

        Task<ApiResult<DoctorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<ApiResult<DoctorResponse>> CreateAsync(DoctorCreateViewModel model, CancellationToken cancellationToken);

        Task<ApiResult<DoctorResponse>> UpdateAsync(int id, DoctorEditViewModel model, CancellationToken cancellationToken);

        Task<ApiResult> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
