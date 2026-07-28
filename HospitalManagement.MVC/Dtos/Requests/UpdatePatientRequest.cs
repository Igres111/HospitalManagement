namespace HospitalManagement.MVC.Dtos.Requests
{
    public record UpdatePatientRequest(
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth,
        string? PhoneNumber,
        string? Email);
}