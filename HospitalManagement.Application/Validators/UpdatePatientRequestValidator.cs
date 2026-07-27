using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequest>
    {
        public UpdatePatientRequestValidator()
        {
            RuleFor(request => request)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field must be provided.");

            RuleFor(request => request.FirstName)
                .NotEmpty()
                .WithMessage("First name cannot be empty.")
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters.")
                .When(request => request.FirstName is not null);

            RuleFor(request => request.LastName)
                .NotEmpty()
                .WithMessage("Last name cannot be empty.")
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters.")
                .When(request => request.LastName is not null);

            RuleFor(request => request.DateOfBirth)
                .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth must be in the past.")
                .When(request => request.DateOfBirth is not null);

            RuleFor(request => request.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number cannot be empty.")
                .MaximumLength(30)
                .WithMessage("Phone number cannot exceed 30 characters.")
                .When(request => request.PhoneNumber is not null);

            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty.")
                .EmailAddress()
                .WithMessage("Email address is invalid.")
                .MaximumLength(100)
                .WithMessage("Email cannot exceed 100 characters.")
                .When(request => request.Email is not null);
        }

        private static bool HaveAtLeastOneField(UpdatePatientRequest request)
        {
            return request.FirstName is not null ||
                    request.LastName is not null ||
                    request.DateOfBirth is not null ||
                    request.PhoneNumber is not null ||
                    request.Email is not null;
        }
    }
}