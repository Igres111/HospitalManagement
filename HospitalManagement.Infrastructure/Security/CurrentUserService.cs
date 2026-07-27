using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HospitalManagement.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdValue = GetUser().FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Authenticated user ID is invalid.");
            }

            return userId;
        }
    }

    public UserRole Role
    {
        get
        {
            var roleValue = GetUser().FindFirstValue(ClaimTypes.Role);

            if (!Enum.TryParse<UserRole>(roleValue, out var role))
            {
                throw new UnauthorizedAccessException("Authenticated user role is invalid.");
            }

            return role;
        }
    }

    private ClaimsPrincipal GetUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Authenticated user was not found.");
        }

        return user;
    }
}
