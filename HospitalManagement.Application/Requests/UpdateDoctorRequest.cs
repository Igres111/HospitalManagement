namespace HospitalManagement.Application.Requests
{
    public record UpdateDoctorRequest(
         string? FirstName,
         string? LastName,
         string? Specialization,
         string? PhoneNumber,
         string? Email);
}