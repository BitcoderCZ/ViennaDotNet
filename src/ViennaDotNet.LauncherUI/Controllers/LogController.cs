using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ViennaDotNet.LauncherUI.Controllers;

[ApiController]
[Route("api/logs")]
public class LogController : ControllerBase
{
    [HttpPost("create")]
    public async Task<Ok> CreateLogs([FromBody] LogEvent[] body)
    {
        throw new NotImplementedException();
    }
}