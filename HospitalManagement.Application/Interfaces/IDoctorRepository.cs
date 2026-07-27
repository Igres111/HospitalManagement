using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
        Task<bool> ExistsByEmailAsync(string email, int excludedDoctorId, CancellationToken cancellationToken);
        Task<Doctor?> GetActiveByIdAsync(int id, CancellationToken cancellationToken);
        Task<bool> HasFutureAppointmentsAsync(int doctorId, CancellationToken cancellationToken);
        Task<(IReadOnlyList<Doctor> Items, int TotalCount)> GetAllAsync(
       string? search,
       string? sortBy,
       bool sortDescending,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken);
    }
}