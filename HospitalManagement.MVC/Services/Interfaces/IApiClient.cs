namespace HospitalManagement.MVC.Services.Interfaces
{
    public interface IApiClient
    {
        Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken);
        Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken);
        Task DeleteAsync(string requestUri, CancellationToken cancellationToken);
    }
}
