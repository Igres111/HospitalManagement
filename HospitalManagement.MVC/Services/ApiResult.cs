using HospitalManagement.MVC.Services.Interfaces;

namespace HospitalManagement.MVC.Services
{
    public sealed class ApiResult : IApiResult
    {
        public bool IsError { get; }
        public int? StatusCode { get; }
        public string? ErrorMessage { get; }
        public string? ErrorDetails { get; }

        private ApiResult(bool isError, int? statusCode, string? errorMessage, string? errorDetails)
        {
            IsError = isError;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorDetails = errorDetails;
        }

        public static ApiResult Success() => new(false, null, null, null);

        public static ApiResult Failure(int statusCode, string errorMessage, string? errorDetails) =>
            new(true, statusCode, errorMessage, errorDetails);
    }

    public sealed class ApiResult<T> : IApiResult
    {
        public bool IsError { get; }
        public T? Value { get; }
        public int? StatusCode { get; }
        public string? ErrorMessage { get; }
        public string? ErrorDetails { get; }

        private ApiResult(bool isError, T? value, int? statusCode, string? errorMessage, string? errorDetails)
        {
            IsError = isError;
            Value = value;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorDetails = errorDetails;
        }

        public static ApiResult<T> Success(T value) => new(false, value, null, null, null);

        public static ApiResult<T> Failure(int statusCode, string errorMessage, string? errorDetails) =>
            new(true, default, statusCode, errorMessage, errorDetails);
    }
}
