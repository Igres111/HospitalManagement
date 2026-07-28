namespace HospitalManagement.MVC.Dtos.Responses
{
    public record ApiErrorResponse(int StatusCode, string Message, string? Details);
}