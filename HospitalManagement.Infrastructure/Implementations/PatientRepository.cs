using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Infrastructure.Helpers;
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
                SortFieldParser.Parse(sortBy, PatientSortField.Id),
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
       PatientSortField sortField,
       bool sortDescending)
        {
            IOrderedQueryable<Patient> orderedQuery;

            if (sortField == PatientSortField.FirstName)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.FirstName)
                    : query.OrderBy(patient => patient.FirstName);
            }
            else if (sortField == PatientSortField.LastName)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.LastName)
                    : query.OrderBy(patient => patient.LastName);
            }
            else if (sortField == PatientSortField.DateOfBirth)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.DateOfBirth)
                    : query.OrderBy(patient => patient.DateOfBirth);
            }
            else if (sortField == PatientSortField.Email)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.Email)
                    : query.OrderBy(patient => patient.Email);
            }
            else if (sortField == PatientSortField.CreatedAt)
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.CreatedAt)
                    : query.OrderBy(patient => patient.CreatedAt);
            }
            else
            {
                orderedQuery = sortDescending
                    ? query.OrderByDescending(patient => patient.Id)
                    : query.OrderBy(patient => patient.Id);
            }

            return sortDescending
                ? orderedQuery.ThenByDescending(patient => patient.Id)
                : orderedQuery.ThenBy(patient => patient.Id);
        }
    }
}