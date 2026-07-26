using FluentValidation;
using HospitalManagement.WebAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace HospitalManagement.WebAPI.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = GetStatusCode(exception);

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

        var response = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = GetMessage(exception, statusCode),
            Details = _environment.IsDevelopment()
                ? exception.ToString()
                : ""
        };

        context.Response.Clear();
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            DbUpdateException dbUpdateException => GetDbUpdateStatusCode(dbUpdateException),

            _ => HttpStatusCode.InternalServerError
        };
    }

    private static HttpStatusCode GetDbUpdateStatusCode(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return sqlException.Number switch
            {
                2601 or 2627 or 547 => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };
        }

        return HttpStatusCode.InternalServerError;
    }

    private static string GetMessage(Exception exception, HttpStatusCode statusCode)
    {
        return exception switch
        {
            ValidationException validationException =>
                string.Join(
                    "; ",
                    validationException.Errors.Select(error =>
                        error.ErrorMessage)),

            ArgumentException => exception.Message,

            KeyNotFoundException => exception.Message,

            UnauthorizedAccessException =>
                "You do not have permission to perform this operation.",

            DbUpdateConcurrencyException =>
                "The record was modified by another operation.",

            DbUpdateException when statusCode == HttpStatusCode.Conflict =>
                "This operation conflicts with existing data, such as a duplicate value or a record that is still referenced elsewhere.",

            DbUpdateException =>
                "A database error occurred.",

            _ => "An unexpected error occurred."
        };
    }
}