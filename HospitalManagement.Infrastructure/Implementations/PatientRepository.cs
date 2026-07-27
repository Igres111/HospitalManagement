using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Infrastructure.Implementations.BaseRepository;
using HospitalManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Implementations
{
    public sealed class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Context.Patients.AnyAsync(
                patient =>
                    patient.Email == email &&
                    patient.DeletedAt == null,
                cancellationToken);
        }

        public Task<Patient?> GetActiveByIdAsync(int patientId, CancellationToken cancellationToken)
        {
            return Context.Patients
                .FirstOrDefaultAsync(
                    patient =>
                        patient.Id == patientId &&
                        patient.DeletedAt == null,
                    cancellationToken);
        }

        public Task<bool> ExistsByEmailAsync(
       string email,
       int excludedPatientId,
       CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLower();

            return Context.Patients.AnyAsync(
                patient =>
                    patient.Id != excludedPatientId &&
                    patient.Email.ToLower() == normalizedEmail &&
                    patient.DeletedAt == null,
                cancellationToken);
        }

        public Task<bool> HasFutureAppointmentsAsync(
       int patientId,
       CancellationToken cancellationToken)
        {
            return Context.Appointments.AnyAsync(
                appointment =>
                    appointment.PatientId == patientId &&
                    appointment.AppointmentDate > DateTime.UtcNow &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.DeletedAt == null,
                cancellationToken);
        }

        public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetAllAsync(
       string? search,
       string? sortBy,
       bool sortDescending,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken)
        {
            IQueryable<Patient> query = Context.Patients.AsNoTracking().Where(patient => patient.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchValue = search
                    .Trim()
                    .ToLower();

                query = query.Where(patient =>
                    patient.FirstName.ToLower().Contains(searchValue) ||
                    patient.LastName.ToLower().Contains(searchValue));
            }

            query = ApplySorting(
                query,
                sortBy,
                sortDescending);

            var totalCount = await query.CountAsync(cancellationToken);

            var patients = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (patients, totalCount);
        }

        private static IQueryable<Patient> ApplySorting(
       IQueryable<Patient> query,
       string? sortBy,
       bool sortDescending)
        {
            var normalizedSortBy = sortBy?.Trim().ToLower();

            return normalizedSortBy switch
            {
                "firstname" => sortDescending
                    ? query
                        .OrderByDescending(patient => patient.FirstName)
                        .ThenByDescending(patient => patient.Id)
                    : query
                        .OrderBy(patient => patient.FirstName)
                        .ThenBy(patient => patient.Id),

                "lastname" => sortDescending
                    ? query
                        .OrderByDescending(patient => patient.LastName)
                        .ThenByDescending(patient => patient.Id)
                    : query
                        .OrderBy(patient => patient.LastName)
                        .ThenBy(patient => patient.Id),

                "dateofbirth" => sortDescending
                    ? query
                        .OrderByDescending(patient => patient.DateOfBirth)
                        .ThenByDescending(patient => patient.Id)
                    : query
                        .OrderBy(patient => patient.DateOfBirth)
                        .ThenBy(patient => patient.Id),

                "email" => sortDescending
                    ? query
                        .OrderByDescending(patient => patient.Email)
                        .ThenByDescending(patient => patient.Id)
                    : query
                        .OrderBy(patient => patient.Email)
                        .ThenBy(patient => patient.Id),

                "createdat" => sortDescending
                    ? query
                        .OrderByDescending(patient => patient.CreatedAt)
                        .ThenByDescending(patient => patient.Id)
                    : query
                        .OrderBy(patient => patient.CreatedAt)
                        .ThenBy(patient => patient.Id),

                _ => sortDescending
                    ? query.OrderByDescending(patient => patient.Id)
                    : query.OrderBy(patient => patient.Id)
            };
        }
    }
}