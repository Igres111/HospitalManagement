namespace HospitalManagement.MVC.Dtos.Responses
{
    public record DoctorResponse(
        int Id,
        string FirstName,
        string LastName,
        string Specialization,
        string? PhoneNumber,
        string Email,
        DateTime CreatedAt);
}