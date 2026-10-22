using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SharedContracts.Responses;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Login successful."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RevokeTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Logged out successfully."));
    }
}
