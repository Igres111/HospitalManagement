namespace HospitalManagement.MVC.Dtos.Requests
{
    public record RegisterRequest(string Username, string Password, string ConfirmPassword);
}