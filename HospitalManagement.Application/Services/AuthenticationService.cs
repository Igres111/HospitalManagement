using FluentValidation;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Services;

public class AuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterUserRequest> _validator;
    private readonly IValidator<LoginUserRequest> _loginValidator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserRequest> validator,
        IValidator<LoginUserRequest> loginValidator,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _loginValidator = loginValidator;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterUserRequest body,CancellationToken cancellationToken)
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

        await _userRepository.AddAsync(user,cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.CreatedAt);
    }

    public async Task<string> LoginAsync(LoginUserRequest body, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(body,cancellationToken);

        var username = body.Username.Trim();

        var user = await _userRepository.GetByUsernameAsync(username,cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var passwordIsValid = _passwordHasher.Verify(body.Password,user.PasswordHash);

        if (!passwordIsValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        return accessToken;
    }
}