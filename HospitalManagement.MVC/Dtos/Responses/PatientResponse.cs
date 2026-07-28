namespace HospitalManagement.MVC.Dtos.Responses
{
    public record PatientResponse(
        int Id,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string? PhoneNumber,
        string Email,
        DateTime CreatedAt);
}