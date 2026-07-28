using FluentValidation;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Options;
using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using Microsoft.Extensions.Options;
using System.Security.Authentication;

namespace HospitalManagement.Application.Services;

public class AuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterUserRequest> _validator;
    private readonly IValidator<LoginUserRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly JwtOptions _jwtOptions;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserRequest> validator,
        IValidator<LoginUserRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterUserRequest body, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(body, cancellationToken);

        var username = body.Username.Trim();

        var userExists = await _userRepository.ExistsByUsernameAsync(username, cancellationToken);

        if (userExists)
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        var user = new User
        {
            Username = username.ToLower(),
            PasswordHash = _passwordHasher.Hash(body.Password),
            Role = UserRole.Receptionist,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.CreatedAt);
    }

    public async Task<LoginResponse> LoginAsync(LoginUserRequest body, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(body, cancellationToken);

        var username = body.Username.Trim();

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("Invalid username or password.");
        }

        var passwordIsValid = _passwordHasher.Verify(body.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            throw new AuthenticationException("Invalid username or password.");
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        var refreshToken = await IssueRefreshTokenAsync(user, cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, refreshToken.Token);
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest body, CancellationToken cancellationToken)
    {
        await _refreshTokenValidator.ValidateAndThrowAsync(body, cancellationToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(body.RefreshToken, cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        var user = storedToken.User;

        if (user.DeletedAt is not null)
        {
            throw new AuthenticationException("Invalid refresh token.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        var newRefreshToken = await IssueRefreshTokenAsync(user, cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, newRefreshToken.Token);
    }

    private async Task<RefreshToken> IssueRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Token = _refreshTokenGenerator.GenerateToken(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return refreshToken;
    }

    public async Task LogoutAsync(RefreshTokenRequest body, CancellationToken cancellationToken)
    {
        await _refreshTokenValidator.ValidateAndThrowAsync(body, cancellationToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(body.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}