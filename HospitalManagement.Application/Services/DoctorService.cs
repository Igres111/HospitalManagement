using FluentValidation;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Services;

public class DoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IValidator<CreateDoctorRequest> _createDoctorValidator;
    private readonly IValidator<GetDoctorsRequest> _getDoctorsValidator;
    private readonly IValidator<UpdateDoctorRequest> _updateDoctorValidator;

    public DoctorService(
        IDoctorRepository doctorRepository, 
        IValidator<CreateDoctorRequest> createDoctorValidator, 
        IValidator<GetDoctorsRequest> getDoctorsValidator, 
        IValidator<UpdateDoctorRequest> updateDoctorValidator)
    {
        _doctorRepository = doctorRepository;
        _createDoctorValidator = createDoctorValidator;
        _getDoctorsValidator = getDoctorsValidator;
        _updateDoctorValidator = updateDoctorValidator;
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest body, CancellationToken cancellationToken)
    {
        await _createDoctorValidator.ValidateAndThrowAsync(body,cancellationToken);

        var normalizedEmail = body.Email.Trim().ToLower();

        var doctorExists = await _doctorRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);

        if (doctorExists)
        {
            throw new InvalidOperationException("An active doctor with this email already exists.");
        }

        var doctor = new Doctor
        {
            FirstName = body.FirstName.Trim(),
            LastName = body.LastName.Trim(),
            Specialization = body.Specialization.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(body.PhoneNumber) ? null : body.PhoneNumber.Trim(),
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        await _doctorRepository.AddAsync(doctor, cancellationToken);

        await _doctorRepository.SaveChangesAsync(cancellationToken);

        return new DoctorResponse(
            doctor.Id,
            doctor.FirstName,
            doctor.LastName,
            doctor.Specialization,
            doctor.PhoneNumber,
            doctor.Email,
            doctor.CreatedAt);
    }
    public async Task<PagedResponse<DoctorResponse>> GetAllAsync(GetDoctorsRequest request, CancellationToken cancellationToken)
    {
        await _getDoctorsValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var result = await _doctorRepository.GetAllAsync(
            request.Search,
            request.SortBy,
            request.SortDescending,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var doctors = result.Items
            .Select(doctor => new DoctorResponse(
                doctor.Id,
                doctor.FirstName,
                doctor.LastName,
                doctor.Specialization,
                doctor.PhoneNumber,
                doctor.Email,
                doctor.CreatedAt))
            .ToList();

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                result.TotalCount / (double)request.PageSize);

        return new PagedResponse<DoctorResponse>(
            doctors,
            request.PageNumber,
            request.PageSize,
            result.TotalCount,
            totalPages,
            request.PageNumber > 1,
            request.PageNumber < totalPages);
    }

    public async Task<DoctorResponse> GetByIdAsync(int doctorId, CancellationToken cancellationToken)
    {
        if (doctorId <= 0)
        {
            throw new ArgumentException("Doctor ID must be greater than zero.");
        }

        var doctor = await _doctorRepository.GetActiveByIdAsync(doctorId, cancellationToken);

        if (doctor is null)
        {
            throw new KeyNotFoundException($"Doctor with ID {doctorId} was not found.");
        }

        return new DoctorResponse(
            doctor.Id,
            doctor.FirstName,
            doctor.LastName,
            doctor.Specialization,
            doctor.PhoneNumber,
            doctor.Email,
            doctor.CreatedAt);
    }
    public async Task<DoctorResponse> UpdateAsync(int doctorId, UpdateDoctorRequest body, CancellationToken cancellationToken)
    {
        if (doctorId <= 0)
        {
            throw new ArgumentException("Doctor ID must be greater than zero.");
        }

        await _updateDoctorValidator.ValidateAndThrowAsync(body, cancellationToken);

        var doctor = await _doctorRepository.GetActiveByIdAsync(
            doctorId,
            cancellationToken);

        if (doctor is null)
        {
            throw new KeyNotFoundException($"Doctor with ID {doctorId} was not found.");
        }

        if (body.FirstName is not null)
        {
            doctor.FirstName = body.FirstName.Trim();
        }

        if (body.LastName is not null)
        {
            doctor.LastName = body.LastName.Trim();
        }

        if (body.Specialization is not null)
        {
            doctor.Specialization = body.Specialization.Trim();
        }

        if (body.PhoneNumber is not null)
        {
            doctor.PhoneNumber = string.IsNullOrWhiteSpace(body.PhoneNumber) ? null : body.PhoneNumber.Trim();
        }

        if (body.Email is not null)
        {
            var normalizedEmail = body.Email
                .Trim()
                .ToLower();

            var emailExists =
                await _doctorRepository.ExistsByEmailAsync(
                    normalizedEmail,
                    doctorId,
                    cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException("Another active doctor with this email already exists.");
            }

            doctor.Email = normalizedEmail;
        }

        doctor.UpdatedAt = DateTime.UtcNow;

        await _doctorRepository.SaveChangesAsync(cancellationToken);

        return new DoctorResponse(
            doctor.Id,
            doctor.FirstName,
            doctor.LastName,
            doctor.Specialization,
            doctor.PhoneNumber,
            doctor.Email,
            doctor.CreatedAt);
    }
    public async Task DeleteAsync(int doctorId, CancellationToken cancellationToken)
    {
        if (doctorId <= 0)
        {
            throw new ArgumentException("Doctor ID must be greater than zero.");
        }

        var doctor = await _doctorRepository.GetActiveByIdAsync(doctorId,cancellationToken);

        if (doctor is null)
        {
            throw new KeyNotFoundException($"Doctor with ID {doctorId} was not found.");
        }

        var hasFutureAppointments =await _doctorRepository.HasFutureAppointmentsAsync(doctorId,cancellationToken);

        if (hasFutureAppointments)
        {
            throw new InvalidOperationException("The doctor cannot be deleted because they have future appointments.");
        }

        doctor.DeletedAt = DateTime.UtcNow;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _doctorRepository.SaveChangesAsync(cancellationToken);
    }
}