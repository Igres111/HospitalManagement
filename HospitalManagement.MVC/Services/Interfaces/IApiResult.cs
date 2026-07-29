namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IApiResult
    {
        bool IsError { get; }
        int? StatusCode { get; }
        string? ErrorMessage { get; }
        string? ErrorDetails { get; }
    }
}
