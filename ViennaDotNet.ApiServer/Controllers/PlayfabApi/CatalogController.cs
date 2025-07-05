using Microsoft.AspNetCore.Mvc;
using ViennaDotNet.ApiServer.Models.Playfab;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ApiServer.Controllers.PlayfabApi;

[Route("Catalog")]
public class CatalogController : ViennaControllerBase
{
    private sealed record CatalogSearchRequest(
        bool Count,
        string Filter,
        string? Select,
        string? OrderBy,
        int? Top,
        int? Skip,
        string Scid
    );

    [HttpPost("Search")]
    public async Task<IActionResult> SearchAsync()
    {
        var cancellationToken = Request.HttpContext.RequestAborted;

        var request = await Request.Body.AsJsonAsync<CatalogSearchRequest>(cancellationToken);

        if (request is null)
        {
            return BadRequest();
        }

        Item[] items;

        // TODO: load objects from static data, parse the filter
        switch (request.Filter)
        {
            case "(contentType eq 'BundleOffer_V1.0' or contentType eq 'MarketplaceDurableCatalog_V1.2' or contentType eq 'PersonaDurable') and platforms/any(tp: tp eq 'android.googleplay' and tp eq 'title.earth') and (tags/any(t: t eq 'd7725840-4376-44fc-9220-585f45775371' or t eq '230f5996-04b2-4f0e-83e5-4056c7f1d946')) and not tags/any(t: t eq 'hidden_offer')":
            case "(contentType eq 'BundleOffer_V1.0' or contentType eq 'MarketplaceDurableCatalog_V1.2' or contentType eq 'PersonaDurable') and platforms/any(tp: tp eq 'android.googleplay' and tp eq 'title.earth') and (tags/any(t: t eq '230f5996-04b2-4f0e-83e5-4056c7f1d946' or t eq 'd7725840-4376-44fc-9220-585f45775371')) and not tags/any(t: t eq 'hidden_offer')":
                items = [
                    new(
                        "230f5996-04b2-4f0e-83e5-4056c7f1d946",
                        "PersonaDurable",
                        ["android.googleplay", "ios.store", "title.earth"],
                        ["230f5996-04b2-4f0e-83e5-4056c7f1d946"],
                        [
                            new(
                                "00000000-0000-0000-0000-000000000001",
                                "Thumbnail",
                                "Thumbnail",
                                "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-20ca2/172a2675-d175-47bd-8e7b-b82fd8fa7430/Thumbnail.jpg"
                            ),
                        ]
                    ),
                ];
                break;

            default:
                items = [];
                break;
        }

        return JsonPascalCase(new OkResponse(
            200,
            "OK",
            new Dictionary<string, object>()
            {
                ["Count"] = items.Length,
                ["Items"] = items,
                ["ConfigurationName"] = "DEFAULT",
            }
        ));
    }

    [HttpPost("SearchStores")]
    public IActionResult SearchStores()
    {
        return JsonPascalCase(new OkResponse(
            200,
            "OK",
            new Dictionary<string, object>()
            {
                ["Count"] = 0,
                ["Stores"] = Array.Empty<object>(),
                ["ConfigurationName"] = "DEFAULT",
            }
        ));
    }

    [HttpPost("GetPublishedItem")]
    public IActionResult GetPublishedItem()
    {
        // TODO
        return JsonCamelCase(new ErrorResponse(
            400,
            "BadRequest",
            "InvalidParams",
            1000,
            "Invalid input parameters",
            new()
            {
                ["ItemId"] = ["The ItemId field is required."],
            }
        ));
    }

    private sealed record Item(
        string Id,
        string ContentType,
        string[] Platforms,
        string[] Tags,
        Item.Image[] Images
    )
    {
        public sealed record Image(
            string Id,
            string Type,
            string Tag,
            string Url
        );
    }
}
