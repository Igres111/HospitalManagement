using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }

    UserRole Role { get; }
}
