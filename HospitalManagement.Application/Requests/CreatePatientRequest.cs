namespace HospitalManagement.Application.Requests
{
    public record CreatePatientRequest(
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string? PhoneNumber,
        string Email);
}