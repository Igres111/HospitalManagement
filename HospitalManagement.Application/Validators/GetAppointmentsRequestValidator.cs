using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class GetAppointmentsRequestValidator : AbstractValidator<GetAppointmentsRequest>
    {
        private static readonly string[] AllowedSortFields =
        [
            "id",
            "appointmentdate",
            "status",
            "createdat",
            "doctorname",
            "patientname"
        ];

        public GetAppointmentsRequestValidator()
        {
            RuleFor(request => request.DoctorId)
                .GreaterThan(0)
                .WithMessage("Doctor ID must be greater than zero.")
                .When(request => request.DoctorId is not null);

            RuleFor(request => request.PatientId)
                .GreaterThan(0)
                .WithMessage("Patient ID must be greater than zero.")
                .When(request => request.PatientId is not null);

            RuleFor(request => request.DateTo)
                .GreaterThanOrEqualTo(request => request.DateFrom)
                .WithMessage("DateTo must be greater than or equal to DateFrom.")
                .When(request =>
                    request.DateFrom is not null &&
                    request.DateTo is not null);

            RuleFor(request => request.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than zero.");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(request => request.SortBy)
                .Must(BeValidSortField)
                .WithMessage("SortBy must be one of: id, appointmentDate, status, createdAt, doctorName, patientName.");
        }

        private static bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return true;
            }

            return AllowedSortFields.Contains(sortBy.Trim().ToLower());
        }
    }
}