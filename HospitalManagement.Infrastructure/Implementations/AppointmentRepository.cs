using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Infrastructure.Helpers;
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
                query = query.Where(appointment => appointment.AppointmentDate >= dateFrom.Value);
            }

            if (dateTo is not null)
            {
                query = query.Where(appointment => appointment.AppointmentDate <= dateTo.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(
                query,
                SortFieldParser.Parse(sortBy, AppointmentSortField.Id),
                sortDescending);

            var appointments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (appointments, totalCount);
        }
        private static IQueryable<Appointment> ApplySorting(
            IQueryable<Appointment> query,
            AppointmentSortField sortField,
            bool sortDescending)
        {
            IOrderedQueryable<Appointment> orderedQuery;

            if (sortField == AppointmentSortField.AppointmentDate)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(appointment => appointment.AppointmentDate)
                    : query.OrderBy(appointment => appointment.AppointmentDate);
            }
            else if (sortField == AppointmentSortField.Status)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(appointment => appointment.Status)
                    : query.OrderBy(appointment => appointment.Status);
            }
            else if (sortField == AppointmentSortField.CreatedAt)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(appointment => appointment.CreatedAt)
                    : query.OrderBy(appointment => appointment.CreatedAt);
            }
            else if (sortField == AppointmentSortField.DoctorName)
            {
                orderedQuery = sortDescending
                    ? query
                        .OrderByDescending(appointment => appointment.Doctor.LastName)
                        .ThenByDescending(appointment => appointment.Doctor.FirstName)
                    : query
                        .OrderBy(appointment => appointment.Doctor.LastName)
                        .ThenBy(appointment => appointment.Doctor.FirstName);
            }
            else if (sortField == AppointmentSortField.PatientName)
            {
                orderedQuery = sortDescending
                    ? query
                        .OrderByDescending(appointment => appointment.Patient.LastName)
                        .ThenByDescending(appointment => appointment.Patient.FirstName)
                    : query
                        .OrderBy(appointment => appointment.Patient.LastName)
                        .ThenBy(appointment => appointment.Patient.FirstName);
            }
            else
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(appointment => appointment.Id)
                    : query.OrderBy(appointment => appointment.Id);
            }

            return sortDescending
                ? orderedQuery.ThenByDescending(appointment => appointment.Id)
                : orderedQuery.ThenBy(appointment => appointment.Id);
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