using Marvin.ClanQueueServer.Attributes;
using Marvin.ClanQueueServer.Models.Responses;
using Marvin.ClanQueueServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Marvin.ClanQueueServer.Controllers;

[ApiController]
[Route("api")]
public class BungieHealthController : ControllerBase
{
    private readonly IBungieNetHealthCheck _bungieNetHealthCheck;

    public BungieHealthController(IBungieNetHealthCheck bungieNetHealthCheck)
    {
        _bungieNetHealthCheck = bungieNetHealthCheck;
    }

    [HttpGet("systems")]
    [HeaderApiKeyAuth]
    public IActionResult GetSystems()
    {
        var status =
            _bungieNetHealthCheck
                .LatestSettingsResponse?
                .Systems
                .ToDictionary(x => x.Key, x => x.Value.IsEnabled);

        return new OkObjectResult(new ApiResponse<Dictionary<string, bool>>()
        {
            Data = status,
            Message = null,
            Status = "success",
            StatusCode = ApiResponseCodes.Success
        });
    }
}