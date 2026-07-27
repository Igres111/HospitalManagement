using HospitalManagement.Application.Interfaces;
using System.Security.Cryptography;

namespace HospitalManagement.Infrastructure.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 64;

    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

        return Convert.ToBase64String(randomBytes);
    }
}
