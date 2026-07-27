namespace HospitalManagement.Application.Requests
{
    public record UpdatePatientRequest(
            string? FirstName,
            string? LastName,
            DateOnly? DateOfBirth,
            string? PhoneNumber,
            string? Email);
}