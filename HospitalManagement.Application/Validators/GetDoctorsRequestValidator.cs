using FluentValidation;
using HospitalManagement.Application.Requests;

namespace HospitalManagement.Application.Validators
{
    public class GetDoctorsRequestValidator : AbstractValidator<GetDoctorsRequest>
    {
        private static readonly string[] AllowedSortFields =
        [
            "id",
        "firstName",
        "lastName",
        "specialization",
        "email",
        "createdAt"
        ];

        public GetDoctorsRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search value cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x.SortBy)
                .Must(sortBy =>
                    string.IsNullOrWhiteSpace(sortBy) ||
                    AllowedSortFields.Contains(
                        sortBy,
                        StringComparer.OrdinalIgnoreCase))
                .WithMessage(
                    "SortBy must be one of: id, firstName, lastName, specialization, email, createdAt.");
        }
    }
}
