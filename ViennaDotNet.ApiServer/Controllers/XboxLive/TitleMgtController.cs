using Microsoft.AspNetCore.Mvc;
using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ViennaDotNet.ApiServer.Controllers.XboxLive;

[Route("titles")]
public class TitleMgtController : ViennaControllerBase
{
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static DomainParser? _domainParser;

    private static Config config => Program.config;

    private sealed record EndpointsResponse(IEnumerable<Endpoint> EndPoints);

    private sealed record Endpoint(string Protocol, string Host, int? Port, string HostType, string RelyingParty, string TokenType);

    [HttpGet("{title}/endpoints")]
    public async Task<IActionResult> GetEndpoints(string title)
    {
        var cancellationToken = Request.HttpContext.RequestAborted;

        IEnumerable<Endpoint> endpoints;

        switch (title)
        {
            case "default":
                {
                    string protocol = Request.IsHttps ? "https" : "http";
                    var host = Request.Host;
                    Debug.Assert(host.HasValue);

                    bool isHostIp = IPAddress.TryParse(host.Host, out _);
                    if (isHostIp && !config.Environment.SingleDomainMode)
                    {
                        Log.Error("A connection using ip address was made, but Environment.SingleDomainMode is set to false. Only domain connections are allowed when SingleDomainMode is false");
                        return BadRequest();
                    }

                    string hostString = isHostIp
                        ? host.Host
                        : (await GetDomainParserAsync(cancellationToken)).Parse(host.Host)?.RegistrableDomain ?? host.Host;

                    endpoints =
                    [
                        config.Environment.SingleDomainMode ?
                        new Endpoint(
                            protocol,
                            hostString,
                            host.Port ?? 80,
                            isHostIp ? "ip" : "fqdn",
                            "http://xboxlive.com",
                            "JWT"
                        ) :
                        new Endpoint(
                            protocol,
                            $"*.{hostString}",
                            host.Port ?? 80,
                            "wildcard",
                            "http://xboxlive.com",
                            "JWT"
                        ),
                        new Endpoint(
                            "https",
                            "xboxlive.com",
                            null,
                            "fqdn",
                            "http://xboxlive.com",
                            "JWT"
                        ),
                    ];
                }

                break;
            case "2037747551":
                {
                    endpoints =
                    [
                        new Endpoint(
                            "https",
                            "*.playfabapi.com",
                            null,
                            "wildcard",
                            "https://b980a380.minecraft.playfabapi.com/",
                            "JWT"
                        ),
                    ];
                }

                break;
            default:
                return BadRequest();
        }

        return Content(JsonSerializer.Serialize(new EndpointsResponse(endpoints), jsonOptions), "application/json");
    }

    private static async Task<DomainParser> GetDomainParserAsync(CancellationToken cancellationToken)
    {
        if (_domainParser is not null)
        {
            return _domainParser;
        }

        var ruleProvider = new LocalFileRuleProvider("public_suffix_list.dat");
        await ruleProvider.BuildAsync(cancellationToken: cancellationToken);

        return _domainParser = new DomainParser(ruleProvider);
    }
}
