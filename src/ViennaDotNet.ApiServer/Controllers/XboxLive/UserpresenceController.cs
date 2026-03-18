using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ViennaDotNet.ApiServer.Controllers.XboxLive;

[Route("users")]
[Route("userpresence.xboxlive.com/users")]
public partial class UserpresenceController : ViennaControllerBase
{
    [HttpPost("{xuidParam}/devices/current/titles/current")]
    public IActionResult GetTitles(string xuidParam)
    {
        var authUnion = XboxLiveAuth();
        if (authUnion.IsB)
        {
            return authUnion.B;
        }

        var token = authUnion.A;

        Match xuidMatch = GetXuidRegex().Match(xuidParam);

        string? xuid = xuidMatch.Success ? xuidMatch.Groups[1].Value : null;

        if (xuid is null)
        {
            return BadRequest();
        }

        if (xuid != token.UserId)
        {
            return Unauthorized();
        }

        // TODO

        return Ok();
    }

    // TODO

    [GeneratedRegex(@"^xuid\((.*)\)$")]
    private static partial Regex GetXuidRegex();
}
