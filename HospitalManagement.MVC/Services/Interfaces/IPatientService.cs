using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IPatientService
    {
        Task<ApiResult<PagedResponse<PatientResponse>>> GetAllAsync(
            PatientFilterViewModel filter,
            CancellationToken cancellationToken);

        Task<ApiResult<PatientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<ApiResult<PatientResponse>> CreateAsync(PatientCreateViewModel model, CancellationToken cancellationToken);

        Task<ApiResult<PatientResponse>> UpdateAsync(int id, PatientEditViewModel model, CancellationToken cancellationToken);

        Task<ApiResult> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
