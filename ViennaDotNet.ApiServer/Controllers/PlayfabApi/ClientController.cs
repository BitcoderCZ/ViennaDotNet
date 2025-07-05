using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ViennaDotNet.ApiServer.Models;
using ViennaDotNet.ApiServer.Models.Playfab;
using ViennaDotNet.ApiServer.Utils;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ApiServer.Controllers.PlayfabApi;

[Route("Client")]
public partial class ClientController : ViennaControllerBase
{
    private static Config config => Program.config;

    private sealed record GetUserPublisherDataRequest(
        GetUserPublisherDataRequest.EntityR Entity,
        string[] Keys
    )
    {
        public sealed record EntityR(
            string Id,
            string Type
        );
    }

    [HttpPost("GetUserPublisherData")]
    public async Task<IActionResult> GetUserPublisherData()
    {
        var cancellationToken = Request.HttpContext.RequestAborted;

        var request = await Request.Body.AsJsonAsync<GetUserPublisherDataRequest>(cancellationToken);

        if (request is null)
        {
            return BadRequest();
        }

        if (!Request.Headers.TryGetValue("X-Authorization", out var tokenHeader) || tokenHeader.Count < 1)
        {
            return BadRequest();
        }

        Match tokenMatch = GetAuthRegex().Match(tokenHeader[0] ?? "");

        string? tokenString = tokenMatch.Success ? tokenMatch.Groups[1].Value : null;

        if (tokenString is null)
        {
            return BadRequest();
        }

        var token = JwtUtils.Verify<Tokens.Shared.PlayfabSessionTicket>(tokenString, config.PlayfabApi.SessionTicketSecretBytes);
        if (token is null)
        {
            return Forbid();
        }

        switch (request.Entity.Type)
        {
            case "master_player_account":
                {
                    var publisherData = new Dictionary<string, object>()
                    {
                        ["PlayFabCommerceEnabled"] = new Dictionary<string, string>()
                        {
                            ["Value"] = "true",
                            ["Permission"] = "Private",
                            ["LastUpdated"] = "2020-05-17T13:25:32.85Z",
                        },
                    };

                    return JsonPascalCase(new OkResponse(
                        200,
                        "OK",
                        new Dictionary<string, object>()
                        {
                            ["Data"] = request.Keys
                                .Where(publisherData.ContainsKey)
                                .ToDictionary(field => field, field => publisherData[field]),
                            ["DataVersion"] = 84,
                        }
                    ));
                }

            default:
                return BadRequest();
        }
    }

    private sealed record GetPlayerStatisticsRequest(
        string[] StatisticNames
    );

    [HttpPost("GetPlayerStatistics")]
    public async Task<IActionResult> GetPlayerStatistics()
    {
        var cancellationToken = Request.HttpContext.RequestAborted;

        var request = await Request.Body.AsJsonAsync<GetPlayerStatisticsRequest>(cancellationToken);

        if (request is null)
        {
            return BadRequest();
        }

        if (!Request.Headers.TryGetValue("X-Authorization", out var tokenHeader) || tokenHeader.Count < 1)
        {
            return BadRequest();
        }

        Match tokenMatch = GetAuthRegex().Match(tokenHeader[0] ?? "");

        string? tokenString = tokenMatch.Success ? tokenMatch.Groups[1].Value : null;

        if (tokenString is null)
        {
            return BadRequest();
        }

        var token = JwtUtils.Verify<Tokens.Shared.PlayfabSessionTicket>(tokenString, config.PlayfabApi.SessionTicketSecretBytes);
        if (token is null)
        {
            return Forbid();
        }

        // TODO
        var statistics = new Dictionary<string, int>()
        {
            ["BlocksPlaced"] = 0,
            ["BlocksCollected"] = 0,
            ["Deaths"] = 0,
            ["ItemsCrafted"] = 0,
            ["ItemsSmelted"] = 0,
            ["ToolsBroken"] = 0,
            ["MobsKilled"] = 0,
            ["BuildplateSeconds"] = 0,
            ["SharedBuildplateViews"] = 0,
            ["AdventuresPlayed"] = 0,
            ["TappablesCollected"] = 0,
            ["MobsCollected"] = 0,
            ["ChallengesCompleted"] = 0,
        };

        return JsonPascalCase(new OkResponse(
            200,
            "OK",
            new Dictionary<string, object>()
            {
                ["Statistics"] = request.StatisticNames
                    .Where(statistics.ContainsKey)
                    .Select(field => new
                    {
                        StatisticName = field,
                        Value = statistics[field],
                    }),
            }
        ));
    }

    [GeneratedRegex("^[0-9A-F]{16}-(.*)$")]
    private static partial Regex GetAuthRegex();
}
