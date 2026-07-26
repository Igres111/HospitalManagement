using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Application.Services;
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

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterUserRequest body,CancellationToken cancellationToken)
    {
        var response = await _authenticationService.RegisterAsync(body,cancellationToken);

        return StatusCode(StatusCodes.Status201Created,response);
    }
}