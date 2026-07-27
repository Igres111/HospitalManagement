using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<bool> HasOverlappingAppointmentAsync(
            int doctorId,
            DateTime appointmentDate,
            CancellationToken cancellationToken);

        Task<bool> HasOverlappingAppointmentAsync(
            int doctorId,
            DateTime appointmentDate,
            int excludedAppointmentId,
            CancellationToken cancellationToken);

        Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetAllAsync(
            int? doctorId,
            int? patientId,
            AppointmentStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? sortBy,
            bool sortDescending,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);

        Task<int> CountFutureAppointmentsAsync(int userId, CancellationToken cancellationToken);

        Task<Appointment?> GetActiveByIdAsync(int appointmentId, CancellationToken cancellationToken);
    }
}