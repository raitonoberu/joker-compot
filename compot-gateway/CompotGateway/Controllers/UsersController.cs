using CompotGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersService.Protos;

namespace CompotGateway.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly InfoService.InfoServiceClient _infoClient;
    private readonly AuthService.AuthServiceClient _authClient;

    public UsersController(
        InfoService.InfoServiceClient infoClient,
        AuthService.AuthServiceClient authClient)
    {
        _infoClient = infoClient;
        _authClient = authClient;
    }

    [HttpGet("{id}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetUser(string id, CancellationToken cancellationToken)
    {
        var response = await _infoClient.GetUserAsync(new GetUserRequest { UserId = id },
            cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authClient.LoginAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authClient.RegisterAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }
}