using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
    {
        public CreateAppointmentRequestValidator()
        {
            RuleFor(request => request.DoctorId)
                .GreaterThan(0)
                .WithMessage("Doctor ID must be greater than zero.");

            RuleFor(request => request.PatientId)
                .GreaterThan(0)
                .WithMessage("Patient ID must be greater than zero.");

            RuleFor(request => request.AppointmentDate)
                .NotEmpty()
                .WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Appointment date must be in the future.");
        }
    }
}