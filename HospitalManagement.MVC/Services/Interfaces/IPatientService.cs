using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Patients;

namespace HospitalManagement.MVC.Services.Interfaces;

public interface IPatientService
{
    Task<PagedResponse<PatientResponse>> GetAllAsync(
        PatientFilterViewModel filter,
        CancellationToken cancellationToken = default);

    Task<PatientResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);

    Task<PatientResponse> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
