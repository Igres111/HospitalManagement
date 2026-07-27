namespace HospitalManagement.Application.Requests
{
    public record CreateDoctorRequest(
        string FirstName,
        string LastName,
        string Specialization,
        string? PhoneNumber,
        string? Email);
}