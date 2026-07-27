namespace HospitalManagement.Application.Requests
{
    public record GetPatientsRequest(
        string? Search,
        string? SortBy,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10);
}