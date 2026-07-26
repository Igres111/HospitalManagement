namespace HospitalManagement.Application.Requests
{
    public record RegisterUserRequest(
        string Username,
        string Password,
        string ConfirmPassword);
}