using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Doctors;

namespace HospitalManagement.MVC.Services.Interfaces;

public interface IDoctorService
{
    Task<PagedResponse<DoctorResponse>> GetAllAsync(
        DoctorFilterViewModel filter,
        CancellationToken cancellationToken = default);

    Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);

    Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
