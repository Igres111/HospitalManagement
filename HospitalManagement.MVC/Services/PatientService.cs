using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Patients;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace HospitalManagement.MVC.Services;

public class PatientService : IPatientService
{
    private readonly IApiClient _apiClient;

    public PatientService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<PagedResponse<PatientResponse>> GetAllAsync(
        PatientFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        var url = BuildQueryUrl(filter.Search, filter.SortBy, filter.SortDescending, filter.PageNumber, filter.PageSize);

        return _apiClient.GetAsync<PagedResponse<PatientResponse>>(url, cancellationToken);
    }

    public Task<PatientResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _apiClient.GetAsync<PatientResponse>($"api/patients/{id}", cancellationToken);
    }

    public Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        return _apiClient.PostAsync<CreatePatientRequest, PatientResponse>("api/patients", request, cancellationToken);
    }

    public Task<PatientResponse> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        return _apiClient.PutAsync<UpdatePatientRequest, PatientResponse>($"api/patients/{id}", request, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return _apiClient.DeleteAsync($"api/patients/{id}", cancellationToken);
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

        return QueryHelpers.AddQueryString("api/patients", queryParams);
    }
}
