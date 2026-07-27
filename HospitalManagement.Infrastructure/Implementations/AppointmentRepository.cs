using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Infrastructure.Implementations.BaseRepository;
using HospitalManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Implementations
{
    public sealed class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private const int AppointmentDurationMinutes = 30;

        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<bool> HasOverlappingAppointmentAsync(
            int doctorId,
            DateTime appointmentDate,
            CancellationToken cancellationToken)
        {
            var newAppointmentEnd = appointmentDate.AddMinutes(AppointmentDurationMinutes);

            return Context.Appointments.AnyAsync(
                appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.DeletedAt == null &&
                    appointment.AppointmentDate < newAppointmentEnd &&
                    appointment.AppointmentDate
                        .AddMinutes(AppointmentDurationMinutes) > appointmentDate,
                cancellationToken);
        }

        public Task<bool> HasOverlappingAppointmentAsync(
            int doctorId,
            DateTime appointmentDate,
            int excludedAppointmentId,
            CancellationToken cancellationToken)
        {
            var newAppointmentEnd = appointmentDate.AddMinutes(AppointmentDurationMinutes);

            return Context.Appointments.AnyAsync(
                appointment =>
                    appointment.Id != excludedAppointmentId &&
                    appointment.DoctorId == doctorId &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.DeletedAt == null &&
                    appointment.AppointmentDate < newAppointmentEnd &&
                    appointment.AppointmentDate
                        .AddMinutes(AppointmentDurationMinutes) > appointmentDate,
                cancellationToken);
        }

        public Task<int> CountFutureAppointmentsAsync(int userId, CancellationToken cancellationToken)
        {
            return Context.Appointments.CountAsync(
                appointment =>
                    appointment.CreatedByUserId == userId &&
                    appointment.AppointmentDate > DateTime.UtcNow &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.DeletedAt == null,
                cancellationToken);
        }

        public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetAllAsync(
           int? doctorId,
           int? patientId,
           AppointmentStatus? status,
           DateTime? dateFrom,
           DateTime? dateTo,
           string? sortBy,
           bool sortDescending,
           int pageNumber,
           int pageSize,
           CancellationToken cancellationToken)
        {
            IQueryable<Appointment> query = Context.Appointments
                .AsNoTracking()
                .Include(appointment => appointment.Doctor)
                .Include(appointment => appointment.Patient)
                .Where(appointment => appointment.DeletedAt == null);

            if (doctorId is not null)
            {
                query = query.Where(appointment => appointment.DoctorId == doctorId.Value);
            }

            if (patientId is not null)
            {
                query = query.Where(appointment => appointment.PatientId == patientId.Value);
            }

            if (status is not null)
            {
                query = query.Where(appointment => appointment.Status == status.Value);
            }

            if (dateFrom is not null)
            {
                query = query.Where(appointment =>appointment.AppointmentDate >= dateFrom.Value);
            }

            if (dateTo is not null)
            {
                query = query.Where(appointment =>appointment.AppointmentDate <= dateTo.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(
                query,
                sortBy,
                sortDescending);

            var appointments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (appointments, totalCount);
        }
        private static IQueryable<Appointment> ApplySorting(
           IQueryable<Appointment> query,
           string? sortBy,
           bool sortDescending)
        {
            var normalizedSortBy = sortBy?.Trim().ToLower();

            return normalizedSortBy switch
            {
                "appointmentdate" => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.AppointmentDate)
                        .ThenByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.AppointmentDate)
                        .ThenBy(
                            appointment => appointment.Id),

                "status" => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.Status)
                        .ThenByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.Status)
                        .ThenBy(
                            appointment => appointment.Id),

                "createdat" => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.CreatedAt)
                        .ThenByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.CreatedAt)
                        .ThenBy(
                            appointment => appointment.Id),

                "doctorname" => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.Doctor.LastName)
                        .ThenByDescending(
                            appointment => appointment.Doctor.FirstName)
                        .ThenByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.Doctor.LastName)
                        .ThenBy(
                            appointment => appointment.Doctor.FirstName)
                        .ThenBy(
                            appointment => appointment.Id),

                "patientname" => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.Patient.LastName)
                        .ThenByDescending(
                            appointment => appointment.Patient.FirstName)
                        .ThenByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.Patient.LastName)
                        .ThenBy(
                            appointment => appointment.Patient.FirstName)
                        .ThenBy(
                            appointment => appointment.Id),

                _ => sortDescending
                    ? query
                        .OrderByDescending(
                            appointment => appointment.Id)
                    : query
                        .OrderBy(
                            appointment => appointment.Id)
            };
        }

        public Task<Appointment?> GetActiveByIdAsync(int appointmentId, CancellationToken cancellationToken)
        {
            return Context.Appointments
                .Include(appointment => appointment.Doctor)
                .Include(appointment => appointment.Patient)
                .FirstOrDefaultAsync(
                    appointment =>
                        appointment.Id == appointmentId &&
                        appointment.DeletedAt == null,
                    cancellationToken);
        }
    }
}