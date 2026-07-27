using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
        Task<bool> ExistsByEmailAsync(string email, int excludedPatientId, CancellationToken cancellationToken);
        Task<Patient?> GetActiveByIdAsync(int patientId, CancellationToken cancellationToken);
        Task<bool> HasFutureAppointmentsAsync(int patientId, CancellationToken cancellationToken);
        Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetAllAsync(
      string? search,
      string? sortBy,
      bool sortDescending,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken);
    }
}