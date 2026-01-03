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

    [HttpGet("{userId}")]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfile>> GetProfile(string userId, CancellationToken cancellationToken)
    {
        var profile = await _aggregator.GetProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }
}

