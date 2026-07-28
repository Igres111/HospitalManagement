using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Options;
using HospitalManagement.MVC.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HospitalManagement.MVC.Services
{
    public class ApiClient : IApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _httpClient;

        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(ApiClientNames.HospitalApi);
        }

        public async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(requestUri, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildApiExceptionAsync(response, cancellationToken);
            }
        }

        private static async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await BuildApiExceptionAsync(response, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

            return result ?? throw new ApiException((int)response.StatusCode, "The API returned an empty response body.");
        }

        private static async Task<ApiException> BuildApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);

            return new ApiException(
                (int)response.StatusCode,
                error?.Message ?? "An unexpected error occurred while calling the API.",
                error?.Details);
        }

        public async Task PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BuildApiExceptionAsync(response, cancellationToken);
            }
        }
    }
}