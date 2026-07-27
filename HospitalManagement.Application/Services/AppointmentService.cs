using FluentValidation;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Services
{
    public class AppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IValidator<CreateAppointmentRequest> _createAppointmentValidator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<GetAppointmentsRequest> _getAppointmentsValidator;
        private readonly IValidator<UpdateAppointmentRequest> _updateAppointmentValidator;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IValidator<CreateAppointmentRequest> createAppointmentValidator,
            ICurrentUserService currentUserService,
            IValidator<GetAppointmentsRequest> getAppointmentsValidator,
            IValidator<UpdateAppointmentRequest> updateAppointmentValidator)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _createAppointmentValidator = createAppointmentValidator;
            _currentUserService = currentUserService;
            _getAppointmentsValidator = getAppointmentsValidator;
            _updateAppointmentValidator = updateAppointmentValidator;
        }

        public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            await _createAppointmentValidator.ValidateAndThrowAsync(request, cancellationToken);

            var doctor = await _doctorRepository.GetActiveByIdAsync(request.DoctorId, cancellationToken);

            if (doctor is null)
            {
                throw new KeyNotFoundException($"Doctor with ID {request.DoctorId} was not found.");
            }

            var patient = await _patientRepository.GetActiveByIdAsync(request.PatientId, cancellationToken);

            if (patient is null)
            {
                throw new KeyNotFoundException($"Patient with ID {request.PatientId} was not found.");
            }

            var hasOverlap =
                await _appointmentRepository.HasOverlappingAppointmentAsync(
                    request.DoctorId,
                    request.AppointmentDate,
                    cancellationToken);

            if (hasOverlap)
            {
                throw new InvalidOperationException("Appointments are overlapping.");
            }

            var userId = _currentUserService.UserId;

            if (_currentUserService.Role == UserRole.Receptionist)
            {
                var futureAppointmentCount = await _appointmentRepository.CountFutureAppointmentsAsync(userId, cancellationToken);

                if (futureAppointmentCount >= 3)
                {
                    throw new InvalidOperationException("A receptionist cannot book more than three future appointments.");
                }
            }

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                AppointmentDate = request.AppointmentDate,
                Status = AppointmentStatus.Scheduled,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            await _appointmentRepository.SaveChangesAsync(cancellationToken);

            return new AppointmentResponse(
                appointment.Id,
                appointment.DoctorId,
                appointment.Doctor.FirstName,
                appointment.Doctor.LastName,
                appointment.PatientId,
                appointment.Patient.FirstName,
                appointment.Patient.LastName,
                appointment.AppointmentDate,
                appointment.Status,
                appointment.CreatedByUserId,
                appointment.CreatedAt);
        }

        public async Task<PagedResponse<AppointmentResponse>> GetAllAsync(GetAppointmentsRequest request, CancellationToken cancellationToken)
        {
            await _getAppointmentsValidator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await _appointmentRepository.GetAllAsync(
                request.DoctorId,
                request.PatientId,
                request.Status,
                request.DateFrom,
                request.DateTo,
                request.SortBy,
                request.SortDescending,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var appointments = result.Items
                .Select(appointment => new AppointmentResponse(
                    appointment.Id,
                    appointment.DoctorId,
                    appointment.Doctor.FirstName,
                    appointment.Doctor.LastName,
                    appointment.PatientId,
                    appointment.Patient.FirstName,
                    appointment.Patient.LastName,
                    appointment.AppointmentDate,
                    appointment.Status,
                    appointment.CreatedByUserId,
                    appointment.CreatedAt))
                .ToList();

            var totalPages = result.TotalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    result.TotalCount / (double)request.PageSize);

            return new PagedResponse<AppointmentResponse>(
                appointments,
                request.PageNumber,
                request.PageSize,
                result.TotalCount,
                totalPages,
                request.PageNumber > 1,
                request.PageNumber < totalPages);
        }

        public async Task<AppointmentResponse> GetByIdAsync(int appointmentId, CancellationToken cancellationToken)
        {
            if (appointmentId <= 0)
            {
                throw new ArgumentException("Appointment ID must be greater than zero.");
            }

            var appointment = await _appointmentRepository.GetActiveByIdAsync(appointmentId, cancellationToken);

            if (appointment is null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} was not found.");
            }

            return new AppointmentResponse(
                appointment.Id,
                appointment.DoctorId,
                appointment.Doctor.FirstName,
                appointment.Doctor.LastName,
                appointment.PatientId,
                appointment.Patient.FirstName,
                appointment.Patient.LastName,
                appointment.AppointmentDate,
                appointment.Status,
                appointment.CreatedByUserId,
                appointment.CreatedAt);
        }

        public async Task<AppointmentResponse> UpdateAsync(
            int appointmentId,
            UpdateAppointmentRequest request,
            CancellationToken cancellationToken)
        {
            if (appointmentId <= 0)
            {
                throw new ArgumentException("Appointment ID must be greater than zero.");
            }

            await _updateAppointmentValidator.ValidateAndThrowAsync(request,cancellationToken);

            var appointment = await _appointmentRepository.GetActiveByIdAsync(appointmentId,cancellationToken);

            if (appointment is null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} was not found.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Completed appointments cannot be modified.");
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled appointments cannot be modified.");
            }

            var doctor = appointment.Doctor;
            var patient = appointment.Patient;

            var updatedDoctorId = request.DoctorId ?? appointment.DoctorId;
            var updatedPatientId = request.PatientId ?? appointment.PatientId;
            var updatedAppointmentDate = request.AppointmentDate ?? appointment.AppointmentDate;

            var updatedStatus = request.Status ?? appointment.Status;

            if (request.DoctorId is not null && request.DoctorId.Value != appointment.DoctorId)
            {
                doctor = await _doctorRepository.GetActiveByIdAsync(request.DoctorId.Value, cancellationToken);

                if (doctor is null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {request.DoctorId.Value} was not found.");
                }
            }

            if (request.PatientId is not null &&
                request.PatientId.Value != appointment.PatientId)
            {
                patient = await _patientRepository.GetActiveByIdAsync(request.PatientId.Value, cancellationToken);

                if (patient is null)
                {
                    throw new KeyNotFoundException($"Patient with ID {request.PatientId.Value} was not found.");
                }
            }

            if (updatedStatus == AppointmentStatus.Completed && updatedAppointmentDate > DateTime.UtcNow)
            {
                throw new InvalidOperationException("A future appointment cannot be marked as completed.");
            }

            if (updatedStatus == AppointmentStatus.Scheduled)
            {
                var hasOverlap =
                    await _appointmentRepository.HasOverlappingAppointmentAsync(
                        updatedDoctorId,
                        updatedAppointmentDate,
                        appointmentId,
                        cancellationToken);

                if (hasOverlap)
                {
                    throw new InvalidOperationException("Appointments are overlapping.");
                }
            }

            appointment.DoctorId = updatedDoctorId;
            appointment.PatientId = updatedPatientId;
            appointment.AppointmentDate = updatedAppointmentDate;
            appointment.Status = updatedStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.SaveChangesAsync(cancellationToken);

            return new AppointmentResponse(
                appointment.Id,
                appointment.DoctorId,
                appointment.Doctor.FirstName,
                appointment.Doctor.LastName,
                appointment.PatientId,
                appointment.Patient.FirstName,
                appointment.Patient.LastName,
                appointment.AppointmentDate,
                appointment.Status,
                appointment.CreatedByUserId,
                appointment.CreatedAt);
        }

        public async Task DeleteAsync(int appointmentId, CancellationToken cancellationToken)
        {
            if (appointmentId <= 0)
            {
                throw new ArgumentException("Appointment ID must be greater than zero.");
            }

            var appointment = await _appointmentRepository.GetActiveByIdAsync(appointmentId,cancellationToken);

            if (appointment is null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} was not found.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Completed appointments cannot be deleted.");
            }

            appointment.DeletedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.SaveChangesAsync(cancellationToken);
        }
    }
}