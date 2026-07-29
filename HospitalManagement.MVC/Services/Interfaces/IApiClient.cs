using HospitalManagement.MVC.Services;

namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IApiClient
    {
        Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken);
        Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task<ApiResult> PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task<ApiResult> DeleteAsync(string requestUri, CancellationToken cancellationToken);
    }
}
