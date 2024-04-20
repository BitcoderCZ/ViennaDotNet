using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.DB;
using DatabaseException = ViennaDotNet.DB.EarthDB.DatabaseException;
using Newtonsoft.Json;
using ViennaDotNet.ApiServer.Utils;

namespace ViennaDotNet.ApiServer.Controllers
{
    [Authorize]
    [ApiVersion("1.1")]
    [Route("1/api/v{version:apiVersion}/player")]
    public class ProfileController : ControllerBase
    {
        private static EarthDB earthDB => Program.db;

        [ResponseCache(Duration = 11200)]
        [Route("rubies")]
        public IActionResult GetRubies()
        {
            string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(playerId))
                return BadRequest();

            try
            {
                Profile profile = (Profile)new EarthDB.Query(false)
                    .Get("profile", playerId, typeof(Profile))
                    .Execute(earthDB)
                    .Get("profile").Value;

                string resp = JsonConvert.SerializeObject(new EarthApiResponsePlus(profile.rubies.purchased + profile.rubies.earned));
                return Content(resp, "application/json");
            }
            catch (DatabaseException ex)
            {
                Log.Error("Exception in GetRubies", ex);
                return StatusCode(500);
            }
        }
    }
}
