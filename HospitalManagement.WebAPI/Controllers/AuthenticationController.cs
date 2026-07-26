using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Application.Services;
using HospitalManagement.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;

    public AuthenticationController(
        AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Registers a new receptionist user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/register
    ///     {
    ///         "username": "receptionist1",
    ///         "password": "Password123",
    ///         "confirmPassword": "Password123"
    ///     }
    ///
    /// The new user is created with the Receptionist role.
    /// </remarks>
    /// <response code="201">User successfully registered.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">An active user with the same username already exists.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterUserRequest body,CancellationToken cancellationToken)
    {
        var response = await _authenticationService.RegisterAsync(body,cancellationToken);

        return StatusCode(StatusCodes.Status201Created,response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "username": "receptionist1",
    ///         "password": "Password123"
    ///     }
    /// </remarks>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Username or password is invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Login([FromBody] LoginUserRequest body, CancellationToken cancellationToken)
    {
        var response = await _authenticationService.LoginAsync(body,cancellationToken);

        return Ok(response);
    }
}