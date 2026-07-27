using FluentValidation;
using HospitalManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<AuthenticationService>();
        services.AddScoped<DoctorService>();
        services.AddScoped<PatientService>();
        services.AddScoped<AppointmentService>();

        return services;
    }
}