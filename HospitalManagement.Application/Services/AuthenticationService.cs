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

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserRequest> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterUserRequest body,CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(body, cancellationToken);

        var username = body.Username.Trim();

        var userExists = await _userRepository.ExistsByUsernameAsync(username, cancellationToken);

        if (userExists)
        {
            throw new Exception("A user with this username already exists.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(body.Password),
            Role = UserRole.Receptionist,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user,cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.CreatedAt);
    }
}