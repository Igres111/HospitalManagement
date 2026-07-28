using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Doctors;

namespace HospitalManagement.MVC.Services.Interfaces;

public interface IDoctorService
{
    Task<PagedResponse<DoctorResponse>> GetAllAsync(
        DoctorFilterViewModel filter,
        CancellationToken cancellationToken);

    Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken);

    Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
