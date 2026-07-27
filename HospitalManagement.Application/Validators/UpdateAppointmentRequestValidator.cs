using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
    {
        public UpdateAppointmentRequestValidator()
        {
            RuleFor(request => request)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field must be provided.");

            RuleFor(request => request.DoctorId)
                .GreaterThan(0)
                .WithMessage("Doctor ID must be greater than zero.")
                .When(request => request.DoctorId is not null);

            RuleFor(request => request.PatientId)
                .GreaterThan(0)
                .WithMessage("Patient ID must be greater than zero.")
                .When(request => request.PatientId is not null);

            RuleFor(request => request.AppointmentDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Appointment date must be in the future.")
                .When(request => request.AppointmentDate is not null);

            RuleFor(request => request.Status)
                .IsInEnum()
                .WithMessage(
                    "Status must be Scheduled, Completed, or Cancelled.")
                .When(request => request.Status is not null);
        }

        private static bool HaveAtLeastOneField(UpdateAppointmentRequest request)
        {
            return request.DoctorId is not null ||
                   request.PatientId is not null ||
                   request.AppointmentDate is not null ||
                   request.Status is not null;
        }
    }
}