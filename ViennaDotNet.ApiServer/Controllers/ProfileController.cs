using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Security.Claims;
using ViennaDotNet.ApiServer.Utils;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.StaticData;
using DatabaseException = ViennaDotNet.DB.EarthDB.DatabaseException;

namespace ViennaDotNet.ApiServer.Controllers;

[Authorize]
[ApiVersion("1.1")]
[Route("1/api/v{version:apiVersion}/player")]
public class ProfileController : ControllerBase
{
    private static EarthDB earthDB => Program.DB;
    private static StaticData.StaticData staticData => Program.staticData;

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetProfile(string userId, CancellationToken cancellationToken)
    {
        Profile profile = (Profile)(await new EarthDB.Query(false)
            .Get("profile", userId.ToLowerInvariant(), typeof(Profile))
            .ExecuteAsync(earthDB, cancellationToken))
            .Get("profile").Value;

        Levels.Level[] levels = staticData.levels.levels;
        int currentLevelExperience = profile.experience - (profile.level > 1 ? (profile.level - 2 < levels.Length ? levels[profile.level - 2].experienceRequired : levels[^1].experienceRequired) : 0);
        int experienceRemaining = profile.level - 1 < levels.Length ? levels[profile.level - 1].experienceRequired - profile.experience : 0;

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(new Types.Profile.Profile(
            Java.IntStream.Range(0, levels.Length).Collect(() => new Dictionary<int, Types.Profile.Profile.Level>(), (hashMap, levelIndex) =>
            {
                Levels.Level level = levels[levelIndex];
                hashMap[levelIndex + 1] = new Types.Profile.Profile.Level(level.experienceRequired, LevelUtils.makeLevelRewards(level).toApiResponse());
            }, DictionaryExtensions.AddRange),
            profile.experience,
            profile.level,
            currentLevelExperience,
            experienceRemaining,
            profile.health,
            profile.health / 20.0f * 100.0f)));

        return Content(resp, "application/json");
    }

    [ResponseCache(Duration = 11200)]
    [HttpGet("rubies")]
    public async Task<IActionResult> GetRubies(CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
            return BadRequest();

        try
        {
            Profile profile = (Profile)(await new EarthDB.Query(false)
                .Get("profile", playerId, typeof(Profile))
                .ExecuteAsync(earthDB, cancellationToken))
                .Get("profile").Value;

            string resp = JsonConvert.SerializeObject(new EarthApiResponse(profile.rubies.purchased + profile.rubies.earned));
            return Content(resp, "application/json");
        }
        catch (DatabaseException ex)
        {
            Log.Error("Exception in GetRubies", ex);
            return StatusCode(500);
        }
    }

    [ResponseCache(Duration = 11200)]
    [HttpGet("splitRubies")]
    public async Task<IActionResult> GetSplitRubies(CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
            return BadRequest();

        try
        {
            Profile profile = (Profile)(await new EarthDB.Query(false)
                .Get("profile", playerId, typeof(Profile))
                .ExecuteAsync(earthDB, cancellationToken))
                .Get("profile").Value;

            string resp = JsonConvert.SerializeObject(new EarthApiResponse(new Types.Profile.SplitRubies(profile.rubies.purchased, profile.rubies.earned)));
            return Content(resp, "application/json");
        }
        catch (DatabaseException ex)
        {
            Log.Error("Exception in GetRubies", ex);
            return StatusCode(500);
        }
    }

    // required for the language selection option in the client to work
    [HttpPost("profile/language")]
    public IActionResult ChangeLanguage()
        => Ok();
}
