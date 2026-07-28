using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Patients;

namespace HospitalManagement.MVC.Services.Interfaces;

public interface IPatientService
{
    Task<PagedResponse<PatientResponse>> GetAllAsync(
        PatientFilterViewModel filter,
        CancellationToken cancellationToken);

    Task<PatientResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken);

    Task<PatientResponse> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
