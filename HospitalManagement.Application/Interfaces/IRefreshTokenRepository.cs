using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
}
