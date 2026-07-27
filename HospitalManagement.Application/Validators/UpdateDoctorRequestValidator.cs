using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class UpdateDoctorRequestValidator
    : AbstractValidator<UpdateDoctorRequest>
    {
        public UpdateDoctorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field must be provided.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50)
                .When(x => x.FirstName is not null);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(50)
                .When(x => x.LastName is not null);

            RuleFor(x => x.Specialization)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => x.Specialization is not null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(30)
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Email)
                .EmailAddress()
                .MaximumLength(100)
                .When(x => x.Email is not null);
        }
        private static bool HaveAtLeastOneField(UpdateDoctorRequest request)
        {
            return request.FirstName is not null ||
                   request.LastName is not null ||
                   request.Specialization is not null ||
                   request.PhoneNumber is not null ||
                   request.Email is not null;
        }
    }
}