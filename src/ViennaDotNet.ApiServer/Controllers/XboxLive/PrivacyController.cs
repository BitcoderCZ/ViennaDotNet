using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ViennaDotNet.ApiServer.Controllers.XboxLive;

[Route("users")]
[Route("privacy.xboxlive.com/users")]
public partial class PrivacyController : ViennaControllerBase
{
    private sealed record PeopleResponse(
        object[] Users
    );

    [HttpGet("{xuidParam}/people/avoid")]
    public IActionResult GetPeopleAvoid(string xuidParam)
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

        return JsonCamelCase(new PeopleResponse(
            []
        ));
    }

    [HttpGet("{xuidParam}/people/mute")]
    public IActionResult GetPeopleMute(string xuidParam)
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

        return JsonPascalCase(new PeopleResponse(
            []
        ));
    }

    [GeneratedRegex(@"^xuid\((.*)\)$")]
    private static partial Regex GetXuidRegex();
}
