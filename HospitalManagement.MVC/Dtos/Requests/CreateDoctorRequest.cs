namespace HospitalManagement.MVC.Dtos.Requests
{
    public record CreateDoctorRequest(
        string FirstName,
        string LastName,
        string Specialization,
        string? PhoneNumber,
        string Email);
}