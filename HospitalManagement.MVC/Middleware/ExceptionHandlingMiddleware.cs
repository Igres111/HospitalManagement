using HospitalManagement.MVC.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;

namespace HospitalManagement.MVC.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client.",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }

            HandleException(context, exception);
        }
    }

    private void HandleException(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = GetMessage(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request {Method} {Path} failed with {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                (int)statusCode);
        }

        context.Response.Clear();

        var tempData = context.RequestServices
            .GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(context);

        tempData["ErrorStatusCode"] = (int)statusCode;
        tempData["ErrorMessage"] = message;
        tempData.Save();

        context.Response.Redirect("/Home/Error");
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ApiException apiException => (HttpStatusCode)apiException.StatusCode,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static string GetMessage(Exception exception)
    {
        return exception is ApiException apiException
            ? apiException.Message
            : "An unexpected error occurred.";
    }
}
