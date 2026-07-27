namespace HospitalManagement.Application.Responses
{
    public record DoctorResponse(
        int Id,
        string FirstName,
        string LastName,
        string Specialization,
        string? PhoneNumber,
        string? Email,
        DateTime CreatedAt);
}