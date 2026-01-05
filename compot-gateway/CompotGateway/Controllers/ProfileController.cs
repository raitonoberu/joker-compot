using System.Security.Claims;
using CompotGateway.Services;
using CompotGateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompotGateway.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ProfileAggregator _aggregator;

    public ProfileController(ProfileAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    [HttpGet("me")]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    public Task<ActionResult<UserProfile>> GetAuthenticatedProfile(CancellationToken cancellationToken)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return GetProfile(userId, cancellationToken);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfile>> GetProfile(string userId, CancellationToken cancellationToken)
    {
        var profile = await _aggregator.GetProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }
}

