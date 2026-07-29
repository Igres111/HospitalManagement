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

        public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PutAsJsonAsync(requestUri, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<ApiResult> DeleteAsync(string requestUri, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await BuildFailureAsync(response, cancellationToken);
            }

            return ApiResult.Success();
        }

        public async Task<ApiResult> PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await BuildFailureAsync(response, cancellationToken);
            }

            return ApiResult.Success();
        }

        private static async Task<ApiResult<TResponse>> ReadResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                return await BuildFailureAsync<TResponse>(response, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException(
                    $"The API returned a successful ({(int)response.StatusCode}) response with an empty body for {response.RequestMessage?.RequestUri}.");
            }

            return ApiResult<TResponse>.Success(result);
        }

        private static async Task<ApiResult<TResponse>> BuildFailureAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);

            return ApiResult<TResponse>.Failure(
                (int)response.StatusCode,
                error?.Message ?? "An unexpected error occurred while calling the API.",
                error?.Details);
        }

        private static async Task<ApiResult> BuildFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);

            return ApiResult.Failure(
                (int)response.StatusCode,
                error?.Message ?? "An unexpected error occurred while calling the API.",
                error?.Details);
        }
    }
}
