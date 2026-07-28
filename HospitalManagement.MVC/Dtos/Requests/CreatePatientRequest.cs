namespace HospitalManagement.MVC.Dtos.Requests
{
    public record CreatePatientRequest(
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string? PhoneNumber,
        string Email);
}