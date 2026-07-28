using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Doctors;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace HospitalManagement.MVC.Services;

public class DoctorService : IDoctorService
{
    private readonly IApiClient _apiClient;

    public DoctorService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<PagedResponse<DoctorResponse>> GetAllAsync(
        DoctorFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        var url = BuildQueryUrl(filter.Search, filter.SortBy, filter.SortDescending, filter.PageNumber, filter.PageSize);

        return _apiClient.GetAsync<PagedResponse<DoctorResponse>>(url, cancellationToken);
    }

    public Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _apiClient.GetAsync<DoctorResponse>($"api/doctors/{id}", cancellationToken);
    }

    public Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        return _apiClient.PostAsync<CreateDoctorRequest, DoctorResponse>("api/doctors", request, cancellationToken);
    }

    public Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        return _apiClient.PutAsync<UpdateDoctorRequest, DoctorResponse>($"api/doctors/{id}", request, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return _apiClient.DeleteAsync($"api/doctors/{id}", cancellationToken);
    }

    private static string BuildQueryUrl(
        string? search, string? sortBy, bool sortDescending, int pageNumber, int pageSize)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["sortDescending"] = sortDescending.ToString()
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams["search"] = search;
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            queryParams["sortBy"] = sortBy;
        }

        return QueryHelpers.AddQueryString("api/doctors", queryParams);
    }
}
