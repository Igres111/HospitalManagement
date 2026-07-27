using FluentValidation;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Services
{
    public class PatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IValidator<CreatePatientRequest> _createPatientValidator;
        private readonly IValidator<GetPatientsRequest> _getPatientsValidator;
        private readonly IValidator<UpdatePatientRequest> _updatePatientValidator;

        public PatientService(
            IPatientRepository patientRepository,
            IValidator<CreatePatientRequest> createPatientValidator,
            IValidator<GetPatientsRequest> getPatientsValidator,
            IValidator<UpdatePatientRequest> updatePatientValidator)
        {
            _patientRepository = patientRepository;
            _createPatientValidator = createPatientValidator;
            _getPatientsValidator = getPatientsValidator;
            _updatePatientValidator = updatePatientValidator;
        }

        public async Task<PatientResponse> CreateAsync(CreatePatientRequest body, CancellationToken cancellationToken)
        {
            await _createPatientValidator.ValidateAndThrowAsync(body, cancellationToken);

            var normalizedEmail = body.Email.Trim().ToLower();

            var patientExists = await _patientRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);

            if (patientExists)
            {
                throw new InvalidOperationException(
                    "An active patient with this email already exists.");
            }

            var patient = new Patient
            {
                FirstName = body.FirstName.Trim(),
                LastName = body.LastName.Trim(),
                DateOfBirth = body.DateOfBirth,
                PhoneNumber = string.IsNullOrWhiteSpace(body.PhoneNumber) ? null : body.PhoneNumber.Trim(),
                Email = normalizedEmail,
                CreatedAt = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient, cancellationToken);

            await _patientRepository.SaveChangesAsync(cancellationToken);

            return new PatientResponse(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.PhoneNumber,
                patient.Email,
                patient.CreatedAt);
        }
        public async Task<PagedResponse<PatientResponse>> GetAllAsync(GetPatientsRequest request, CancellationToken cancellationToken)
        {
            await _getPatientsValidator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await _patientRepository.GetAllAsync(
                request.Search,
                request.SortBy,
                request.SortDescending,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var patients = result.Items
                .Select(patient => new PatientResponse(
                    patient.Id,
                    patient.FirstName,
                    patient.LastName,
                    patient.DateOfBirth,
                    patient.PhoneNumber,
                    patient.Email,
                    patient.CreatedAt))
                .ToList();

            var totalPages = result.TotalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    result.TotalCount / (double)request.PageSize);

            return new PagedResponse<PatientResponse>(
                patients,
                request.PageNumber,
                request.PageSize,
                result.TotalCount,
                totalPages,
                request.PageNumber > 1,
                request.PageNumber < totalPages);
        }

        public async Task<PatientResponse> GetByIdAsync(int patientId, CancellationToken cancellationToken)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("Patient ID must be greater than zero.");
            }

            var patient = await _patientRepository.GetActiveByIdAsync(patientId, cancellationToken);

            if (patient is null)
            {
                throw new KeyNotFoundException($"Patient with ID {patientId} was not found.");
            }

            return new PatientResponse(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.PhoneNumber,
                patient.Email,
                patient.CreatedAt);
        }

        public async Task<PatientResponse> UpdateAsync(
       int patientId,
       UpdatePatientRequest request,
       CancellationToken cancellationToken)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("Patient ID must be greater than zero.");
            }
            
            await _updatePatientValidator.ValidateAndThrowAsync(request, cancellationToken);

            var patient = await _patientRepository.GetActiveByIdAsync(patientId, cancellationToken);

            if (patient is null)
            {
                throw new KeyNotFoundException(
                    $"Patient with ID {patientId} was not found.");
            }

            if (request.Email is not null)
            {
                var normalizedEmail = request.Email.Trim().ToLower();

                var emailExists = await _patientRepository.ExistsByEmailAsync(
                    normalizedEmail,
                    patientId,
                    cancellationToken);

                if (emailExists)
                {
                    throw new InvalidOperationException("An active patient with this email already exists.");
                }

                patient.Email = normalizedEmail;
            }

            if (request.FirstName is not null)
            {
                patient.FirstName = request.FirstName.Trim();
            }

            if (request.LastName is not null)
            {
                patient.LastName = request.LastName.Trim();
            }

            if (request.DateOfBirth is not null)
            {
                patient.DateOfBirth = request.DateOfBirth.Value;
            }

            if (request.PhoneNumber is not null)
            {
                patient.PhoneNumber = request.PhoneNumber.Trim();
            }

            patient.UpdatedAt = DateTime.UtcNow;

            await _patientRepository.SaveChangesAsync(cancellationToken);

            return new PatientResponse(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.PhoneNumber,
                patient.Email,
                patient.CreatedAt);
        }

        public async Task DeleteAsync(int patientId, CancellationToken cancellationToken)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("Patient ID must be greater than zero.");
            }

            var patient = await _patientRepository.GetActiveByIdAsync(patientId, cancellationToken);

            if (patient is null)
            {
                throw new KeyNotFoundException($"Patient with ID {patientId} was not found.");
            }

            var hasFutureAppointments = await _patientRepository.HasFutureAppointmentsAsync(patientId, cancellationToken);

            if (hasFutureAppointments)
            {
                throw new InvalidOperationException("The patient cannot be deleted because they have future appointments.");
            }

            patient.DeletedAt = DateTime.UtcNow;
            patient.UpdatedAt = DateTime.UtcNow;

            await _patientRepository.SaveChangesAsync(cancellationToken);
        }
    }
}