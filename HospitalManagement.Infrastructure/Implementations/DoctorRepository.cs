using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Infrastructure.Implementations.BaseRepository;
using HospitalManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Implementations
{
    public sealed class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context)
        {

        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Context.Doctors.AnyAsync(
                doctor =>
                    doctor.Email == email &&
                    doctor.DeletedAt == null,
                cancellationToken);
        }
        public Task<bool> ExistsByEmailAsync(string email, int excludedDoctorId, CancellationToken cancellationToken)
        {
            return Context.Doctors.AnyAsync(
                doctor =>
                    doctor.Id != excludedDoctorId &&
                    doctor.Email == email &&
                    doctor.DeletedAt == null,
                cancellationToken);
        }

        public Task<Doctor?> GetActiveByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Context.Doctors
                .FirstOrDefaultAsync(
                    doctor => doctor.Id == id && doctor.DeletedAt == null,
                    cancellationToken);
        }

        public async Task<(IReadOnlyList<Doctor> Items, int TotalCount)> GetAllAsync(
       string? search,
       string? sortBy,
       bool sortDescending,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken)
        {
            IQueryable<Doctor> query = Context.Doctors.AsNoTracking().Where(doctor => doctor.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchValue = search.Trim();

                query = query.Where(doctor =>
                    doctor.FirstName.Contains(searchValue) ||
                    doctor.LastName.Contains(searchValue));
            }

            query = ApplySorting(
                query,
                sortBy,
                sortDescending);

            var totalCount = await query.CountAsync(cancellationToken);

            var doctors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (doctors, totalCount);
        }

        private static IQueryable<Doctor> ApplySorting(
            IQueryable<Doctor> query,
            string? sortBy,
            bool sortDescending)
        {
            var normalizedSortBy = sortBy?
                .Trim()
                .ToLower();

            return normalizedSortBy switch
            {
                "firstname" => sortDescending
                    ? query
                        .OrderByDescending(doctor => doctor.FirstName)
                        .ThenByDescending(doctor => doctor.Id)
                    : query
                        .OrderBy(doctor => doctor.FirstName)
                        .ThenBy(doctor => doctor.Id),

                "lastname" => sortDescending
                    ? query
                        .OrderByDescending(doctor => doctor.LastName)
                        .ThenByDescending(doctor => doctor.Id)
                    : query
                        .OrderBy(doctor => doctor.LastName)
                        .ThenBy(doctor => doctor.Id),

                "specialization" => sortDescending
                    ? query
                        .OrderByDescending(doctor => doctor.Specialization)
                        .ThenByDescending(doctor => doctor.Id)
                    : query
                        .OrderBy(doctor => doctor.Specialization)
                        .ThenBy(doctor => doctor.Id),

                "email" => sortDescending
                    ? query
                        .OrderByDescending(doctor => doctor.Email)
                        .ThenByDescending(doctor => doctor.Id)
                    : query
                        .OrderBy(doctor => doctor.Email)
                        .ThenBy(doctor => doctor.Id),

                "createdat" => sortDescending
                    ? query
                        .OrderByDescending(doctor => doctor.CreatedAt)
                        .ThenByDescending(doctor => doctor.Id)
                    : query
                        .OrderBy(doctor => doctor.CreatedAt)
                        .ThenBy(doctor => doctor.Id),

                _ => sortDescending
                    ? query.OrderByDescending(doctor => doctor.Id)
                    : query.OrderBy(doctor => doctor.Id)
            };
        }
        public Task<bool> HasFutureAppointmentsAsync(int doctorId,CancellationToken cancellationToken)
        {
            return Context.Appointments.AnyAsync(
                appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate > DateTime.UtcNow &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.DeletedAt == null,
                cancellationToken);
        }
    }
}