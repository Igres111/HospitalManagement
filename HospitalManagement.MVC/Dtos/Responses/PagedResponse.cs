namespace HospitalManagement.MVC.Dtos.Responses
{
    public record PagedResponse<T>(
        IReadOnlyCollection<T> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage);
}