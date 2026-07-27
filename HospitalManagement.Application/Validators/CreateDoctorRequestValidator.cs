using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators;
public class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Specialization)
            .NotEmpty()
            .WithMessage("Specialization is required.")
            .MaximumLength(100)
            .WithMessage("Specialization cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .WithMessage("Phone number cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email address is invalid.")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters.");
    }
}