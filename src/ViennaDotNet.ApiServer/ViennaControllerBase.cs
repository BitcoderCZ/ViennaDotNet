using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ViennaDotNet.ApiServer.Models;
using ViennaDotNet.ApiServer.Utils;

namespace ViennaDotNet.ApiServer;

[ApiController]
public abstract class ViennaControllerBase : ControllerBase
{
    private static Config config => Program.config;

    // TODO: make these generic, might change output
    protected ContentResult EarthJson(object results)
        => JsonCamelCase(new EarthApiResponse(results));

    protected ContentResult EarthJson(object? results, EarthApiResponse.UpdatesResponse? updates)
        => JsonCamelCase(new EarthApiResponse(results, updates));

    protected ContentResult JsonCamelCase(object value)
        => Content(Common.Json.Serialize(value), "application/json");

    protected ContentResult JsonPascalCase(object value)
        => Content(JsonSerializer.Serialize(value), "application/json");

    protected Union<Tokens.Xbox.XapiToken, IActionResult> XboxLiveAuth()
    {
        var authorization = XboxAuthorizationUtils.Parse(Request.Headers["Authorization"].FirstOrDefault());

        if (authorization is not { } authValue)
        {
            return BadRequest();
        }

        var token = JwtUtils.Verify<Tokens.Xbox.XapiToken>(authValue.TokenString, config.XboxLive.XapiTokenSecretBytes)?.Data;

        if (token is null || token.UserId != authValue.UserId)
        {
            return Unauthorized();
        }

        return token;
    }

    protected Union<Tokens.Playfab.EntityToken, IActionResult> PlayfabAuth()
    {
        if (!Request.Headers.TryGetValue("X-EntityToken", out var tokenString) || tokenString.Count < 1)
        {
            return BadRequest();
        }

        var token = JwtUtils.Verify<Tokens.Playfab.EntityToken>(tokenString[0] ?? "", config.PlayfabApi.EntityTokenSecretBytes)?.Data;
        if (token is null)
        {
            return Forbid();
        }

        return token;
    }
}
