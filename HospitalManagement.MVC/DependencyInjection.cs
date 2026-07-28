using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Options;
using HospitalManagement.MVC.Services;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace HospitalManagement.MVC;

public static class DependencyInjection
{
    public static IServiceCollection AddMvcServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();

        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

        services.AddHttpContextAccessor();
        services.AddTransient<AuthorizationHeaderHandler>();

        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        services.AddHttpClient(ApiClientNames.HospitalApi, (serviceProvider, client) =>
        {
            var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<AuthorizationHeaderHandler>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = false;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
