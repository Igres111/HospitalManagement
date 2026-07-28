namespace HospitalManagement.MVC.Dtos.Requests
{
    public record UpdateDoctorRequest(
        string? FirstName,
        string? LastName,
        string? Specialization,
        string? PhoneNumber,
        string? Email);
}