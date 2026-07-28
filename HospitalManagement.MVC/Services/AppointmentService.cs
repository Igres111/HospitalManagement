using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Appointments;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace HospitalManagement.MVC.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IApiClient _apiClient;

        public AppointmentService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<PagedResponse<AppointmentResponse>> GetAllAsync(AppointmentFilterViewModel filter, CancellationToken cancellationToken)
        {
            var url = BuildQueryUrl(filter);

            return _apiClient.GetAsync<PagedResponse<AppointmentResponse>>(url, cancellationToken);
        }

        public Task<AppointmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return _apiClient.GetAsync<AppointmentResponse>($"api/appointments/{id}", cancellationToken);
        }

        public Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            return _apiClient.PostAsync<CreateAppointmentRequest, AppointmentResponse>("api/appointments", request, cancellationToken);
        }

        public Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
        {
            return _apiClient.PutAsync<UpdateAppointmentRequest, AppointmentResponse>($"api/appointments/{id}", request, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            return _apiClient.DeleteAsync($"api/appointments/{id}", cancellationToken);
        }

        private static string BuildQueryUrl(AppointmentFilterViewModel filter)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["pageNumber"] = filter.PageNumber.ToString(),
                ["pageSize"] = filter.PageSize.ToString(),
                ["sortDescending"] = filter.SortDescending.ToString()
            };

            if (filter.DoctorId is not null)
            {
                queryParams["doctorId"] = filter.DoctorId.Value.ToString();
            }

            if (filter.PatientId is not null)
            {
                queryParams["patientId"] = filter.PatientId.Value.ToString();
            }

            if (filter.Status is not null)
            {
                queryParams["status"] = filter.Status.Value.ToString();
            }

            if (filter.DateFrom is not null)
            {
                queryParams["dateFrom"] = filter.DateFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (filter.DateTo is not null)
            {
                queryParams["dateTo"] = filter.DateTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                queryParams["sortBy"] = filter.SortBy;
            }

            return QueryHelpers.AddQueryString("api/appointments", queryParams);
        }
    }
}