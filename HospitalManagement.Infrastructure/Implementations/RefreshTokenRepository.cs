using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Infrastructure.Implementations.BaseRepository;
using HospitalManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Implementations;

public sealed class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return Context.RefreshTokens.Include(refreshToken => refreshToken.User).FirstOrDefaultAsync(refreshToken => refreshToken.Token == token, cancellationToken);
    }
}
