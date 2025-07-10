using CommandLine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using OData2Linq;
using Org.BouncyCastle.Math;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ViennaDotNet.ApiServer.Models.Playfab;
using ViennaDotNet.Common.Utils;
using CItem = ViennaDotNet.StaticData.Playfab.Item;

namespace ViennaDotNet.ApiServer.Controllers.PlayfabApi;

[Route("Catalog")]
[Route("20CA2.playfabapi.com/Catalog")]
public class CatalogController : ViennaControllerBase
{
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new UtcDateTimeConverter() },
    };

    private static StaticData.StaticData staticData => Program.staticData;

    private static readonly Item[] itemData;

    static CatalogController()
    {
        var brfPrice = new Item.PriceR([
            new([new("ecd19d3c-7635-402c-a185-eb11cb6c6946", "ecd19d3c-7635-402c-a185-eb11cb6c6946", "ecd19d3c-7635-402c-a185-eb11cb6c6946", 0)]),
            new([new("0113e233-7637-48e7-91b0-349fdc74713d", "0113e233-7637-48e7-91b0-349fdc74713d", "0113e233-7637-48e7-91b0-349fdc74713d", 0)])
        ], []);

        itemData = [
            // shop items/layout
            /*new Item(
                new("B63A0803D3653643", "namespace", "namespace"),
                new("B63A0803D3653643", "namespace", "namespace"),
                Guid.Parse("06e44b91-e7f5-46b6-9986-ca755890f3bf"),
                "catalogItem",
                [],
                null,
                new() { ["en-US"] = "Home L1", ["NEUTRAL"] = "Home L1", ["neutral"] = "Home L1", },
                new() { ["en-US"] = "Home L1", ["NEUTRAL"] = "Home L1", ["neutral"] = "Home L1", },
                new() { ["en-US"] = new([]), ["NEUTRAL"] = new([]), ["neutral"] = new([]), },
                "GenoaQueryManifest_V0.0.3",
                new("3C0BE9326354CBB7", "title_player_account", "title_player_account"),
                new("3C0BE9326354CBB7", "title_player_account", "title_player_account"),
                null, // IsStackable
                ["android.googleplay", "ios.store", "uwp.store", "title.earth"],
                ["mctestdefault"],
                new(2020, 12, 10, 18, 59, 39, 396, DateTimeKind.Utc),
                new(2021, 1, 4, 19, 42, 53, 773, DateTimeKind.Utc), // TODO: get this from file modified date or make it configurable?
                new(1, 1, 1, 0, 0, 0, DateTimeKind.Utc), // originally null, but it must be not null to get filtered correctly
                [],
                [],
                [],
                null,
                null,
                [],
                Item.DisplayPropertiesR.CreateQueryManifest(
                    "0.25.0",
                    "1.0.20",
                    staticData.Playfab.ShopTabs.Select(tab => new Item.DisplayPropertiesR.Tab(
                        tab.ScreenLayoutQueries.Select(layoutQuery => new Item.DisplayPropertiesR.Tab.ScreenLayoutQuery(
                            // TODO: haven't seen it yet, but it's possible these can have properties
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Rectangle ? new object() : null,
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Square ? new object() : null,
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Grid ? new object() : null,
                            layoutQuery.Queries.Select(query => new Item.DisplayPropertiesR.Tab.ScreenLayoutQuery.Query(
                                query.ProductIds,
                                query.QueryContentTypes.Select(type => type.ToString()),
                                query.TopCount
                            )),
                            layoutQuery.ComponentId
                        )),
                        tab.TabIcon,
                        tab.TabTitle,
                        tab.TabId
                    )),
                    staticData.Playfab.ShopNotSearchQueryTags
                )
            ),*/
            // required for shop to load for some reason...
            new Item(
                new("B63A0803D3653643", "namespace", "namespace"),
                new("B63A0803D3653643", "namespace", "namespace"),
                Guid.Parse("230f5996-04b2-4f0e-83e5-4056c7f1d946"),
                "bundle",
                [new("FriendlyId", Guid.Parse("53bee6fe-c9d9-43c9-b3af-4c5438fba4b7"))],
                null,
                new() { ["en-US"] = "Bold Rabbit Feet", ["NEUTRAL"] = "Bold Rabbit Feet", ["neutral"] = "Bold Rabbit Feet", },
                new() { ["en-US"] = "§", ["NEUTRAL"] = "§", ["neutral"] = "§", },
                new() { ["en-US"] = new(["Animal"]), ["NEUTRAL"] = new(["Animal"]), ["neutral"] = new(["Animal"]), },
                "PersonaDurable",
                new("301F442C3B63DC20", "master_player_account", "master_player_account"),
                new("301F442C3B63DC20", "master_player_account", "master_player_account"),
                false, // IsStackable
                ["android.amazonappstore", "android.googleplay",  "b.store",  "ios.store",  "nx.store",  "oculus.store.gearvr", "oculus.store.rift", "uwp.store",  "uwp.store.mobile",  "xboxone.store", "title.bedrockvanilla", "title.earth"],
                ["230f5996-04b2-4f0e-83e5-4056c7f1d946", "4f7cdadd-a33c-489d-8969-752ca689f567", "is_achievement", "earth_achievement", "tag.animal", "1P"],
                new(2020, 12, 7, 22, 46, 33, 066, DateTimeKind.Utc),
                new(2023, 8, 10, 14, 11, 19, 81, DateTimeKind.Utc),
                null,
                [new Dictionary<string,object>() {
                    ["Id"] = "f4a2cf48-45c1-4fda-86d0-9d24c069f0a9",
                    ["Url"] = "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-20ca2/f4a2cf48-45c1-4fda-86d0-9d24c069f0a9/primary.zip",
                    ["MaxClientVersion"] = "65535.65535.65535",
                    ["MinClientVersion"] = "1.13.0",
                    ["Tags"] = Array.Empty<string>(),
                    ["Type"] = "personabinary",
                }],
                [new("e7314d2a-8097-48f0-b0e8-039084a22049", "Thumbnail", "Thumbnail", "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-20ca2/e7314d2a-8097-48f0-b0e8-039084a22049/shoes_bold_striped_rabbit_thumbnail_0.png")],
                [new(Guid.Parse("8eb22e2c-db50-4e30-a3d2-0c355e479e74"), 1)],
                brfPrice,
                brfPrice,
                [],
                Item.DisplayPropertiesR.CreatePersona(
                    "Minecraft",
                    0,
                    true,
                    "rare",
                    [new("persona_piece", Guid.Parse("4f7cdadd-a33c-489d-8969-752ca689f567"), "1.1.0"),],
                    Guid.Parse("53bee6fe-c9d9-43c9-b3af-4c5438fba4b7"),
                    "persona_feet"
                )
            ),
            .. staticData.Playfab.Items.Select(item => CIItemToItem(item.Value, "")),
        ];
    }

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
        try
        {
            if (request.Filter.StartsWith("(contentType eq 'PersonaDurable') and platforms/any(tp: tp eq 'android.googleplay' and tp eq 'title.earth') and not tags/any(t: t eq 'hidden_offer') and (startDate ge ") && request.Filter.EndsWith(")and((displayProperties/pieceType ne 'persona_emote'))"))
            {
                return new VirtualFileResult("playfab/res" + (request.Skip ?? 0), "application/json");
            }
            else if (request.Filter.StartsWith("(contentType eq 'GenoaQueryManifest_V0.0.3') and platforms/any(tp: tp eq 'android.googleplay' and tp eq 'title.earth') and (startDate le "))
            {
                return Content("""
                    {
                      "code": 200,
                      "status": "OK",
                      "data": {
                        "Count": 18,
                        "Items": [
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "06e44b91-e7f5-46b6-9986-ca755890f3bf",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {
                              "en-US": {
                                "Values": []
                              },
                              "NEUTRAL": {
                                "Values": []
                              },
                              "neutral": {
                                "Values": []
                              }
                            },
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-12-10T18:59:39.396Z",
                            "LastModifiedDate": "2021-01-04T19:42:53.773Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.25.0",
                              "maxClientVersion": "1.0.20",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a3e4b8b2-88cb-4d26-a414-ce2c6e61c389",
                                            "83cbec6b-749c-472e-a93b-0003e3aa638b",
                                            "85bdb91e-d2e0-4fc2-8269-d171ac0ca4ac",
                                            "716c33f5-f34f-4a1a-8e95-446b9bfc9127",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                            "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "efa4cd81-fc7b-4806-9419-9141027333f8",
                                            "4e42e674-e337-43d0-9587-b7ac947103ff",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                            "b9272a8c-603c-4188-a75b-e7838bbab567"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                        {
                      "screenLayoutQueries": [
                        {
                          "column_grid": {},
                          "queries": [
                            {
                              "productIds": [
                                "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                "be87981e-52af-41bf-b96f-64f777580d67"
                              ],
                              "queryContentTypes": [
                                "Durable",
                                "Collection",
                                "Bundle",
                                "Persona",
                                "Genoa",
                                "BuildplateOffer",
                                "RubyOffer",
                                "InventoryItemOffer"
                              ],
                              "topCount": 25
                            }
                          ],
                          "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                        }
                      ],
                      "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                      "tabTitle": "editorialtool.earth.Rubies",
                      "tabId": "ruby"
                    }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer",
                                "earth_achievement"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "83c95ac6-d9ba-4707-a232-9b54bb2ac6cf",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "page2",
                              "NEUTRAL": "page2",
                              "neutral": "page2"
                            },
                            "Description": {
                              "en-US": "page2",
                              "NEUTRAL": "page2",
                              "neutral": "page2"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mcpage2-r14-103020-2240"
                            ],
                            "CreationDate": "2020-05-05T11:00:23.21Z",
                            "LastModifiedDate": "2020-05-05T11:00:25.972Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "1.14.0",
                              "maxClientVersion": "1.14.9",
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ],
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "1d80e718-9b74-4edf-9b8d-1316c2aa805d",
                                            "51ca06f2-fdc8-470a-a6fb-435de1e61482",
                                            "a50ec66b-44fd-4718-9a83-383dc18406a3",
                                            "a0341e12-5c8f-4eb4-abbd-bfdbf042665e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "5c2ed9e8-b2fc-4d4c-8e5d-6d322d557e84"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "62ec0109-e202-4705-80b1-c77ba587ea74",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_3"
                                      },
                                      "componentId": "d7c871e7-f5d9-4845-2528-808f3192cf79"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                }
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "7652f95c-4694-4264-9c37-a202dbd87fd7",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-05-05T11:00:05.578Z",
                            "LastModifiedDate": "2020-05-05T11:00:09.219Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "1.14.0",
                              "maxClientVersion": "1.14.9",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "1d80e718-9b74-4edf-9b8d-1316c2aa805d",
                                            "51ca06f2-fdc8-470a-a6fb-435de1e61482",
                                            "a50ec66b-44fd-4718-9a83-383dc18406a3",
                                            "a0341e12-5c8f-4eb4-abbd-bfdbf042665e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "5c2ed9e8-b2fc-4d4c-8e5d-6d322d557e84"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "62ec0109-e202-4705-80b1-c77ba587ea74",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_3"
                                      },
                                      "componentId": "d7c871e7-f5d9-4845-2528-808f3192cf79"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "32293f22-544c-46fd-9cf1-fc85d0f2a73d",
                                            "5f6e13d7-9ee6-4a68-a406-3463dbbf831e",
                                            "b4f5b399-593c-44be-9171-33f77787dc2b",
                                            "80958ae6-03d8-4513-8ee1-4eea4d729458",
                                            "92561822-fa6f-499e-b28d-731dc2273e6b",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "e18a0969-47c7-44be-b52e-0067812c4cce",
                                            "1defa71a-f511-433c-a202-89bed77fbb26",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4",
                                            "2a1644ae-add4-4c88-bd99-701f9ce08d44",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.searchString_14"
                                      },
                                      "componentId": "f5791e95-b87b-4bc7-d166-765f49e68b9f"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "64b528fd-9bfa-4bce-a00c-6b3ccf24bb6a",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "page3",
                              "NEUTRAL": "page3",
                              "neutral": "page3"
                            },
                            "Description": {
                              "en-US": "page3",
                              "NEUTRAL": "page3",
                              "neutral": "page3"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mcpage3-r14-103020-2240"
                            ],
                            "CreationDate": "2020-05-05T11:00:31.24Z",
                            "LastModifiedDate": "2020-05-05T11:00:54.201Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "1.14.0",
                              "maxClientVersion": "1.14.9",
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ],
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "1d80e718-9b74-4edf-9b8d-1316c2aa805d",
                                            "51ca06f2-fdc8-470a-a6fb-435de1e61482",
                                            "a50ec66b-44fd-4718-9a83-383dc18406a3",
                                            "a0341e12-5c8f-4eb4-abbd-bfdbf042665e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "5c2ed9e8-b2fc-4d4c-8e5d-6d322d557e84"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "62ec0109-e202-4705-80b1-c77ba587ea74",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_3"
                                      },
                                      "componentId": "d7c871e7-f5d9-4845-2528-808f3192cf79"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "d33e7a82-dc12-46f4-b5d9-57dd91ee77cf",
                                            "81a1747f-2031-46bb-9ae2-b166105446f0",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "e18a0969-47c7-44be-b52e-0067812c4cce",
                                            "80958ae6-03d8-4513-8ee1-4eea4d729458",
                                            "32293f22-544c-46fd-9cf1-fc85d0f2a73d"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c8d81520-0164-4ed6-e271-60939cd27a80"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "53742bee-b39b-4d94-b62d-d1d0f151d035",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4",
                                            "18cc19f1-96b1-43a5-a16c-491e3b1c6806",
                                            "1defa71a-f511-433c-a202-89bed77fbb26",
                                            "81a1747f-2031-46bb-9ae2-b166105446f0",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "92561822-fa6f-499e-b28d-731dc2273e6b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "a363df1f-3b85-4f77-8a21-03211317a029"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "store.trending.skinPack.searchTerm_3",
                                  "tabId": "boosts"
                                }
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "63c225b8-4784-4534-8aaa-7ef17f2ba5c4",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "page4",
                              "NEUTRAL": "page4",
                              "neutral": "page4"
                            },
                            "Description": {
                              "en-US": "page4",
                              "NEUTRAL": "page4",
                              "neutral": "page4"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mcpage4-r14-103020-2240"
                            ],
                            "CreationDate": "2020-05-05T11:00:58.839Z",
                            "LastModifiedDate": "2020-05-05T11:01:02.767Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "1.14.0",
                              "maxClientVersion": "1.14.9",
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ],
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "1d80e718-9b74-4edf-9b8d-1316c2aa805d",
                                            "51ca06f2-fdc8-470a-a6fb-435de1e61482",
                                            "a50ec66b-44fd-4718-9a83-383dc18406a3",
                                            "a0341e12-5c8f-4eb4-abbd-bfdbf042665e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "5c2ed9e8-b2fc-4d4c-8e5d-6d322d557e84"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "62ec0109-e202-4705-80b1-c77ba587ea74",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_3"
                                      },
                                      "componentId": "d7c871e7-f5d9-4845-2528-808f3192cf79"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                }
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "07c91df9-bb25-4141-99f6-6c566e6b8a4e",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "page1",
                              "NEUTRAL": "page1",
                              "neutral": "page1"
                            },
                            "Description": {
                              "en-US": "page1",
                              "NEUTRAL": "page1",
                              "neutral": "page1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mcpage1-r14-103020-2240"
                            ],
                            "CreationDate": "2020-05-05T11:00:14.228Z",
                            "LastModifiedDate": "2020-05-05T11:00:17.769Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "1.14.0",
                              "maxClientVersion": "1.14.9",
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ],
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "1d80e718-9b74-4edf-9b8d-1316c2aa805d",
                                            "51ca06f2-fdc8-470a-a6fb-435de1e61482",
                                            "a50ec66b-44fd-4718-9a83-383dc18406a3",
                                            "a0341e12-5c8f-4eb4-abbd-bfdbf042665e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "5c2ed9e8-b2fc-4d4c-8e5d-6d322d557e84"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "62ec0109-e202-4705-80b1-c77ba587ea74",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_3"
                                      },
                                      "componentId": "d7c871e7-f5d9-4845-2528-808f3192cf79"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "fc4832fe-516d-402d-88af-df2d3de6805d",
                                            "5ed80df8-755d-4ae2-aff0-f619c3e93a11",
                                            "f99030fe-9f47-4d7d-9b51-35845dcc1afb",
                                            "32293f22-544c-46fd-9cf1-fc85d0f2a73d",
                                            "5f6e13d7-9ee6-4a68-a406-3463dbbf831e",
                                            "b4f5b399-593c-44be-9171-33f77787dc2b",
                                            "80958ae6-03d8-4513-8ee1-4eea4d729458",
                                            "92561822-fa6f-499e-b28d-731dc2273e6b",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "e18a0969-47c7-44be-b52e-0067812c4cce",
                                            "1defa71a-f511-433c-a202-89bed77fbb26",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4",
                                            "2a1644ae-add4-4c88-bd99-701f9ce08d44",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.searchString_14"
                                      },
                                      "componentId": "f5791e95-b87b-4bc7-d166-765f49e68b9f"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8eca57b4-76d0-4932-9984-3f363e5a1773",
                                            "affb5343-b853-4c67-af79-c2ba99e6c0a7",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "1ef09546-f8cb-4bff-9095-b4ee6b892eac",
                                            "d33e7a82-dc12-46f4-b5d9-57dd91ee77cf",
                                            "81a1747f-2031-46bb-9ae2-b166105446f0",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "e18a0969-47c7-44be-b52e-0067812c4cce",
                                            "80958ae6-03d8-4513-8ee1-4eea4d729458",
                                            "32293f22-544c-46fd-9cf1-fc85d0f2a73d"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c8d81520-0164-4ed6-e271-60939cd27a80"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "e98e899b-65d8-47c8-88f9-b5967e4812ec",
                                            "53742bee-b39b-4d94-b62d-d1d0f151d035",
                                            "22d6785a-281f-4bab-93a2-d09a8c4ab933",
                                            "e372e7ee-4348-48ac-b56c-06264173b6bc",
                                            "bbaadf09-2ffc-474c-a355-4e6601f16fd4",
                                            "18cc19f1-96b1-43a5-a16c-491e3b1c6806",
                                            "1defa71a-f511-433c-a202-89bed77fbb26",
                                            "81a1747f-2031-46bb-9ae2-b166105446f0",
                                            "0aece298-1ad7-4d3b-8a33-af0b14ebe621",
                                            "92561822-fa6f-499e-b28d-731dc2273e6b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "headerComp": {
                                        "headerText": "store.trending.skinPack.searchTerm_2"
                                      },
                                      "componentId": "a363df1f-3b85-4f77-8a21-03211317a029"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "store.trending.skinPack.searchTerm_3",
                                  "tabId": "boosts"
                                }
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "3b734278-eee6-41ce-915b-02bc9fb0d68a",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {
                              "en-US": {
                                "Values": []
                              },
                              "NEUTRAL": {
                                "Values": []
                              },
                              "neutral": {
                                "Values": []
                              }
                            },
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-09-18T20:30:24.676Z",
                            "LastModifiedDate": "2020-09-18T20:30:28.41Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.25.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "85bdb91e-d2e0-4fc2-8269-d171ac0ca4ac",
                                            "716c33f5-f34f-4a1a-8e95-446b9bfc9127",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                            "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "efa4cd81-fc7b-4806-9419-9141027333f8",
                                            "4e42e674-e337-43d0-9587-b7ac947103ff",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                            "b9272a8c-603c-4188-a75b-e7838bbab567"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer",
                                "earth_achievement"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "4f866aa4-c8de-4667-9094-388ded50f859",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {
                              "en-US": {
                                "Values": []
                              },
                              "NEUTRAL": {
                                "Values": []
                              },
                              "neutral": {
                                "Values": []
                              }
                            },
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-08-07T19:59:19.052Z",
                            "LastModifiedDate": "2020-08-07T19:59:22.996Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.21.5",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "716c33f5-f34f-4a1a-8e95-446b9bfc9127",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                            "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "efa4cd81-fc7b-4806-9419-9141027333f8",
                                            "4e42e674-e337-43d0-9587-b7ac947103ff",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                            "b9272a8c-603c-4188-a75b-e7838bbab567"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer",
                                "earth_achievement"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "6de00c2a-6b08-4b86-905b-f5a6b2ff4278",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {
                              "en-US": {
                                "Values": []
                              },
                              "NEUTRAL": {
                                "Values": []
                              },
                              "neutral": {
                                "Values": []
                              }
                            },
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-07-16T17:50:27.216Z",
                            "LastModifiedDate": "2020-07-16T17:50:30.508Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.21.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "716c33f5-f34f-4a1a-8e95-446b9bfc9127",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                            "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "efa4cd81-fc7b-4806-9419-9141027333f8",
                                            "4e42e674-e337-43d0-9587-b7ac947103ff",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "ab154b19-1617-45bf-89fe-a84fa9f16397",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                            "b9272a8c-603c-4188-a75b-e7838bbab567"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer",
                                "earth_achievement"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "3f53b7dc-3095-4430-a58f-0895cadc415b",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {
                              "en-US": {
                                "Values": []
                              },
                              "NEUTRAL": {
                                "Values": []
                              },
                              "neutral": {
                                "Values": []
                              }
                            },
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-06-18T22:44:46.127Z",
                            "LastModifiedDate": "2020-06-18T23:32:57.013Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.20.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                            "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "efa4cd81-fc7b-4806-9419-9141027333f8",
                                            "4e42e674-e337-43d0-9587-b7ac947103ff",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "ab154b19-1617-45bf-89fe-a84fa9f16397",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                            "b9272a8c-603c-4188-a75b-e7838bbab567"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer",
                                "earth_achievement"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "2bcc52fd-41c4-4729-bd23-df3d413c07e5",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-04-20T22:37:12.433Z",
                            "LastModifiedDate": "2020-04-20T22:37:17.214Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.17.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b",
                                            "31825064-8fa6-423e-84a0-acec8de62aad",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "8319b4dc-04c3-4454-a87c-dea33812edc7",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-03-12T23:02:11.143Z",
                            "LastModifiedDate": "2020-03-23T18:12:21.893Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.15.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b",
                                            "31825064-8fa6-423e-84a0-acec8de62aad",
                                            "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                            "c4a1cce4-c4ef-4f99-a210-071e2e30f154"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "21c53f9e-49e3-46ec-976f-4fbdb0f34da5",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-03-06T19:33:49.348Z",
                            "LastModifiedDate": "2020-03-10T00:10:04.067Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.14.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "5b97a087-9dd3-4afd-b599-6151011f1783",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-02-26T23:57:41.221Z",
                            "LastModifiedDate": "2020-02-28T17:37:17.592Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.13.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                            "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                            "98c03065-5271-4303-933a-0643af1d1e41"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3244ed1f-038b-48eb-91c4-d6f4a3f4c7e8"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "bd70c04e-93b6-44de-2f59-8131d1ea3bc1"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "aabf15c9-3dec-4845-b2bb-f39801adb848",
                                            "6e55ccc1-4240-4af3-a263-08a07305e52f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "eaad6945-1359-4719-9055-794295beb567"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c7bf1465-72f2-4a39-b978-e4554a4701de"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b15fafa9-09e5-41b7-3c2d-83137efd3291"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c03ec332-c93a-44f6-b96f-4fefad012b6d",
                                            "38c6ba1f-563c-451d-a760-78dbb91b8bd2"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8bef7608-da60-44a3-0a52-14fb7635c118"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b29f5e35-5e01-43f7-821e-690735a99e88"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                            "d96c0f65-1001-4220-bd80-405060bbf3be"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4062100a-ada2-4974-86d2-09b606596bc7",
                                            "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                            "23ce5e1d-2074-4f84-9ccd-249819030054"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                            "055229aa-c10a-40af-92b9-15086096dac4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                            "643826f8-c109-4b42-b763-be039ea7752b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                            "200b31fc-137b-4a05-838e-ff532b95eba3"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "6209f825-d137-48f9-8081-c095aab9849f"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                            "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                                    },
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                                    },
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "5de2c884-8b0d-4db2-893e-6824d603454e",
                                            "46e14441-e111-4773-b845-8ba31bd1db60"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/boost",
                                  "tabTitle": "editorialtool.earth.Boosts",
                                  "tabId": "boosts"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "d9967c9f-c9d6-4254-8e62-8de7746829b1",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-02-19T23:30:49.654Z",
                            "LastModifiedDate": "2020-02-28T17:34:32.114Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.13.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1"
                                          ],
                                          "queryContentTypes": [
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                                    },
                                    {
                                      "column_square": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "editorialtool.earth.Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "editorialtool.earth.Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "56c6775a-21ee-443f-a7cc-e09ad27e5c8f",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-02-13T22:29:12.153Z",
                            "LastModifiedDate": "2020-02-15T01:03:21.001Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.12.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1",
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0a67e640-5551-48cf-66cd-61c031ed9b04"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "bb731928-ed8d-4c9e-a8a7-35dc81b9918b",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.earth"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-02-10T19:42:14.832Z",
                            "LastModifiedDate": "2020-02-13T18:23:42.396Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.12.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1",
                                            "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                            "b3316b41-7bc4-4945-b2b8-c43d560b8d26",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0a67e640-5551-48cf-66cd-61c031ed9b04"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "dd1017e7-b4d4-4535-aa21-ea9fe7a5ea80",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Description": {
                              "en-US": "Home L1",
                              "NEUTRAL": "Home L1",
                              "neutral": "Home L1"
                            },
                            "Keywords": {},
                            "ContentType": "GenoaQueryManifest_V0.0.3",
                            "CreatorEntityKey": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3C0BE9326354CBB7",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "Platforms": [
                              "title.earth",
                              "uwp.store",
                              "ios.store",
                              "android.googleplay"
                            ],
                            "Tags": [
                              "mctestdefault"
                            ],
                            "CreationDate": "2020-01-13T20:04:27.21Z",
                            "LastModifiedDate": "2020-01-15T19:12:20.205Z",
                            "Contents": [],
                            "Images": [],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "minClientVersion": "0.10.0",
                              "maxClientVersion": "1.0.0",
                              "tabs": [
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_rectangle": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "c095d219-d568-408e-ac2f-b432be3559a1",
                                            "717c4f02-56e5-4743-a074-44bcdd461db0",
                                            "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                            "41cddd77-390c-4bea-881b-7bc97be8967b",
                                            "7e237426-f44f-40a3-94b0-2669a0795231",
                                            "fbd147af-4df6-441d-b145-e4688288e5af",
                                            "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                            "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                            "6643edae-3d4a-4932-bae2-cc47317a1041",
                                            "22be46ad-5ca7-4173-b8b0-5ddd09e2ec18",
                                            "30055134-bf86-44fe-915e-e096caae2de1",
                                            "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                            "fa00cea2-2e23-4abc-80bf-737c39a83bcc",
                                            "60826d1a-f1ec-4fba-abf8-e9dd09dbf27b",
                                            "1102b106-9da4-4e82-8fe9-828d617d323f",
                                            "33793450-a92d-43db-8e36-4cc37daba50d",
                                            "2416e480-16ae-46b4-bc6d-08dcf46abe7b"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "0a67e640-5551-48cf-66cd-61c031ed9b04"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                                  "tabTitle": "Buildplates",
                                  "tabId": "buildplate"
                                },
                                {
                                  "screenLayoutQueries": [
                                    {
                                      "column_grid": {},
                                      "queries": [
                                        {
                                          "productIds": [
                                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                                            "be87981e-52af-41bf-b96f-64f777580d67"
                                          ],
                                          "queryContentTypes": [
                                            "Durable",
                                            "Collection",
                                            "Bundle",
                                            "Persona",
                                            "Genoa",
                                            "BuildplateOffer",
                                            "RubyOffer",
                                            "InventoryItemOffer"
                                          ],
                                          "topCount": 25
                                        }
                                      ],
                                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                                    }
                                  ],
                                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                                  "tabTitle": "Rubies",
                                  "tabId": "ruby"
                                }
                              ],
                              "globalNotSearchQueryTags": [
                                "hidden_offer"
                              ]
                            }
                          }
                        ],
                        "ConfigurationName": "DEFAULT"
                      }
                    }
                    """, "application/json");
            }
            else if (request.Filter == "(contentType eq 'RubyOffer') and platforms/any(tt: tt eq 'title.earth')")
            {
                return Content("""
                    {
                      "code": 200,
                      "status": "OK",
                      "data": {
                        "Count": 6,
                        "Items": [
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "c5de9899-bcb9-4a86-91f7-3450201da12b",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.1600",
                              "neutral": "store.ruby.offer.1600"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "c5de9899-bcb9-4a86-91f7-3450201da12b",
                              "has_sku"
                            ],
                            "CreationDate": "2019-08-19T20:48:12.966Z",
                            "LastModifiedDate": "2022-08-18T22:53:23.153Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "8c12e03b-9af9-4dc6-88b5-f5e87556d381",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets002.xboxlive.com/pf-title-b63a0803d3653643-ee7b/8c12e03b-9af9-4dc6-88b5-f5e87556d381/default_Thumbnail.jpg"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "bonusCoinCount": 400,
                              "coinCount": 2000,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.1600"
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "be87981e-52af-41bf-b96f-64f777580d67",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.800",
                              "neutral": "store.ruby.offer.800"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "be87981e-52af-41bf-b96f-64f777580d67",
                              "has_sku"
                            ],
                            "CreationDate": "2019-08-19T20:47:50.866Z",
                            "LastModifiedDate": "2022-08-18T22:53:19.43Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "91e19aa2-4fb1-4b1f-bfa4-169e15d5e5e9",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/91e19aa2-4fb1-4b1f-bfa4-169e15d5e5e9/default_Thumbnail.jpg"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "bonusCoinCount": 150,
                              "coinCount": 950,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.800"
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.200",
                              "neutral": "store.ruby.offer.200"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                              "has_sku"
                            ],
                            "CreationDate": "2019-08-19T20:47:08.988Z",
                            "LastModifiedDate": "2022-08-18T22:52:23.177Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "deb54fe3-e6b4-4e23-bba5-40c069e6dd83",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/deb54fe3-e6b4-4e23-bba5-40c069e6dd83/default_Thumbnail.jpg"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "bonusCoinCount": 20,
                              "coinCount": 220,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.200"
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "536dba5b-408d-4c47-a27d-f8f36707f16a",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.40",
                              "neutral": "store.ruby.offer.40"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "301F442C3B63DC20",
                              "Type": "master_player_account",
                              "TypeString": "master_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "301F442C3B63DC20",
                              "Type": "master_player_account",
                              "TypeString": "master_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "536dba5b-408d-4c47-a27d-f8f36707f16a",
                              "has_sku"
                            ],
                            "CreationDate": "2019-09-19T00:58:54.924Z",
                            "LastModifiedDate": "2022-08-18T22:51:50.082Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "128aed3c-b85b-45f9-a396-c4d6005fb197",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/128aed3c-b85b-45f9-a396-c4d6005fb197/default_Thumbnail.png"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "coinCount": 40,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.40"
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.400",
                              "neutral": "store.ruby.offer.400"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                              "has_sku"
                            ],
                            "CreationDate": "2019-08-19T20:47:30.308Z",
                            "LastModifiedDate": "2022-08-18T22:51:44.205Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "dd643a45-af85-472c-aabe-19d50119ac5d",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/dd643a45-af85-472c-aabe-19d50119ac5d/default_Thumbnail.jpg"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "bonusCoinCount": 50,
                              "coinCount": 450,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.400"
                            }
                          },
                          {
                            "SourceEntity": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "SourceEntityKey": {
                              "Id": "B63A0803D3653643",
                              "Type": "namespace",
                              "TypeString": "namespace"
                            },
                            "Id": "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                            "Type": "catalogItem",
                            "AlternateIds": [],
                            "Title": {
                              "NEUTRAL": "store.ruby.offer.100",
                              "neutral": "store.ruby.offer.100"
                            },
                            "Description": {
                              "NEUTRAL": "no description",
                              "neutral": "no description"
                            },
                            "Keywords": {},
                            "ContentType": "RubyOffer",
                            "CreatorEntityKey": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "CreatorEntity": {
                              "Id": "3B1B443CE3FD8EBA",
                              "Type": "title_player_account",
                              "TypeString": "title_player_account"
                            },
                            "IsStackable": false,
                            "Platforms": [
                              "android.googleplay",
                              "ios.store",
                              "uwp.store",
                              "title.bedrockvanilla",
                              "title.earth"
                            ],
                            "Tags": [
                              "activeoffer",
                              "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                              "has_sku"
                            ],
                            "CreationDate": "2019-08-19T20:46:46.012Z",
                            "LastModifiedDate": "2022-08-18T22:51:22.683Z",
                            "Contents": [],
                            "Images": [
                              {
                                "Id": "f5c84c8b-3613-4783-9f5c-d9f910469332",
                                "Tag": "Thumbnail",
                                "Type": "Thumbnail",
                                "Url": "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/f5c84c8b-3613-4783-9f5c-d9f910469332/default_Thumbnail.jpg"
                              }
                            ],
                            "ItemReferences": [],
                            "DeepLinks": [],
                            "DisplayProperties": {
                              "bonusCoinCount": 5,
                              "coinCount": 105,
                              "originalCreatorId": "2535448579972708",
                              "sku": "ruby.100"
                            }
                          }
                        ],
                        "ConfigurationName": "DEFAULT"
                      }
                    }
                    """);
            }

            string filter = request.Filter
                .Replace("platforms/any(tp: tp eq 'android.googleplay' and tp eq 'title.earth')", "platforms/any(tp: tp eq 'android.googleplay') and platforms/any(tp: tp eq 'title.earth')");

            var oDataQuery = itemData.AsQueryable().OData(settings =>
                {
                    settings.EnableCaseInsensitive = true;
                    settings.ValidationSettings.MaxNodeCount = 10000;
                }, GetEdmModel())
                .Filter(filter);

            if (request.OrderBy is not null)
            {
                oDataQuery = oDataQuery.OrderBy(request.OrderBy);
            }

            var query = oDataQuery.ToOriginalQuery();

            if (request.Skip is { } skip)
            {
                query = query.Skip(skip);
            }

            if (request.Top is { } top)
            {
                query = query.Take(top);
            }

            items = query
                .ToArray() // TODO
                .Select(item => item.ContentType is "GenoaQueryManifest_V0.0.3" ? item with { StartDate = null } : item)
                .ToArray();
        }
        catch (Exception ex)
        {
            items = [];
        }

        var response = new Dictionary<string, object>();

        if (request.Count)
        {
            response["Count"] = items.Length;
        }

        response["Items"] = items;
        response["ConfigurationName"] = "DEFAULT";

        /*return JsonPascalCase(new PlayfabOkResponse(
            200,
            "OK",
            response
        ));*/

        Response.Headers.Append("access-control-allow-credentials", "true");
        Response.Headers.Append("access-control-allow-headers", "Content-Type, Content-Encoding, X-Authentication, X-Authorization, X-PlayFabSDK, X-ReportErrorAsSuccess, X-SecretKey, X-EntityToken, Authorization, x-ms-app, x-ms-client-request-id, x-ms-user-id, traceparent, tracestate, Request-Id");
        Response.Headers.Append("access-control-allow-methods", "GET, POST");
        Response.Headers.Append("access-control-allow-origin", "*");

        return Content(JsonSerializer.Serialize(new PlayfabOkResponse(
            200,
            "OK",
            response
        ), jsonOptions), "application/json");
    }

    [HttpPost("SearchStores")]
    public IActionResult SearchStores()
    {
        return JsonPascalCase(new PlayfabOkResponse(
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

    private sealed record GetPublishedItemRequest(
        string? ItemId
    );

    private sealed record GetPublishedItemResponse(
        Item Item
    );

    [HttpPost("GetPublishedItem")]
    public async Task<IActionResult> GetPublishedItem()
    {
        var cancellationToken = Request.HttpContext.RequestAborted;

        var request = await Request.Body.AsJsonAsync<GetPublishedItemRequest>(cancellationToken);

        if (request is null)
        {
            return BadRequest();
        }

        if (request.ItemId == "06e44b91-e7f5-46b6-9986-ca755890f3bf")
        {
            return Content("""
                {
                  "code": 200,
                  "status": "OK",
                  "data": {
                    "Item": {
                      "SourceEntity": {
                        "Id": "B63A0803D3653643",
                        "Type": "namespace",
                        "TypeString": "namespace"
                      },
                      "SourceEntityKey": {
                        "Id": "B63A0803D3653643",
                        "Type": "namespace",
                        "TypeString": "namespace"
                      },
                      "Id": "06e44b91-e7f5-46b6-9986-ca755890f3bf",
                      "Type": "catalogItem",
                      "AlternateIds": [],
                      "Title": {
                        "NEUTRAL": "Home L1",
                        "en-US": "Home L1",
                        "neutral": "Home L1"
                      },
                      "Description": {
                        "NEUTRAL": "Home L1",
                        "en-US": "Home L1",
                        "neutral": "Home L1"
                      },
                      "Keywords": {},
                      "ContentType": "GenoaQueryManifest_V0.0.3",
                      "CreatorEntityKey": {
                        "Id": "3C0BE9326354CBB7",
                        "Type": "title_player_account",
                        "TypeString": "title_player_account"
                      },
                      "CreatorEntity": {
                        "Id": "3C0BE9326354CBB7",
                        "Type": "title_player_account",
                        "TypeString": "title_player_account"
                      },
                      "Platforms": [
                        "android.googleplay",
                        "ios.store",
                        "uwp.store",
                        "title.earth"
                      ],
                      "Tags": [
                        "mctestdefault"
                      ],
                      "CreationDate": "2020-12-10T18:59:39.396Z",
                      "LastModifiedDate": "2021-01-04T19:42:53.773Z",
                      "StartDate": "2021-01-05T17:00:00Z",
                      "Contents": [
                        {
                          "Id": "f3f2b4fc-f144-4357-9e41-198db3a47957",
                          "Url": "https://xforgeassets002.xboxlive.com/pf-title-b63a0803d3653643-20ca2/f3f2b4fc-f144-4357-9e41-198db3a47957/master_loc_contents.json",
                          "MaxClientVersion": "6555.6555.6555",
                          "MinClientVersion": "1.2.0",
                          "Tags": [],
                          "Type": "resourcebinary"
                        }
                      ],
                      "Images": [],
                      "ItemReferences": [],
                      "DeepLinks": [],
                      "DisplayProperties": {
                        "minClientVersion": "0.25.0",
                        "maxClientVersion": "1.0.20",
                        "tabs": [
                          {
                            "screenLayoutQueries": [
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "c095d219-d568-408e-ac2f-b432be3559a1"
                                    ],
                                    "queryContentTypes": [
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "58b67dbf-49dc-4e6d-2b0a-b6da2554f6e8"
                              },
                              {
                                "column_square": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "a3e4b8b2-88cb-4d26-a414-ce2c6e61c389",
                                      "83cbec6b-749c-472e-a93b-0003e3aa638b",
                                      "85bdb91e-d2e0-4fc2-8269-d171ac0ca4ac",
                                      "716c33f5-f34f-4a1a-8e95-446b9bfc9127",
                                      "937fe1d9-7dff-4112-8c43-943b3e86065a",
                                      "331d952a-081d-4ea5-9581-1cbad1c8176d",
                                      "41cddd77-390c-4bea-881b-7bc97be8967b",
                                      "717c4f02-56e5-4743-a074-44bcdd461db0",
                                      "6643edae-3d4a-4932-bae2-cc47317a1041",
                                      "30055134-bf86-44fe-915e-e096caae2de1",
                                      "dcbc054f-51c1-4d95-96f3-aaa0a2d0d7ff",
                                      "faa5120c-5d20-467a-b53e-0b47a7caf31b",
                                      "af54c6cb-34ac-44e3-ada4-fffd4c580c1e",
                                      "8e8e5af4-7865-43a7-8fa9-847cffff5cf6",
                                      "3c14f929-4f9d-4f94-b5ce-abb22b80e5c6",
                                      "1102b106-9da4-4e82-8fe9-828d617d323f",
                                      "efa4cd81-fc7b-4806-9419-9141027333f8",
                                      "4e42e674-e337-43d0-9587-b7ac947103ff",
                                      "7fd89680-7c9b-4adc-92cf-3e26a2dd71cb",
                                      "6078f5c8-81cf-473b-8ebb-0db0a6edadf8",
                                      "8aaa1577-5a4d-409b-842f-73ae13a05f78",
                                      "998b5e8f-7271-4de6-949f-eda15c7100d7",
                                      "c4a1cce4-c4ef-4f99-a210-071e2e30f154",
                                      "b9272a8c-603c-4188-a75b-e7838bbab567"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "81ef2b04-b29d-45f5-2d7d-19aa74979ea8"
                              }
                            ],
                            "tabIcon": "textures/ui/icons/pixel_icons/buildplate",
                            "tabTitle": "editorialtool.earth.Buildplates",
                            "tabId": "buildplate"
                          },
                          {
                            "screenLayoutQueries": [
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "14573f0c-0e18-4b4c-8868-c4d90e3cd509"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "7862b0de-ecc0-4107-c7e3-2f2cb8c02c41"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "c025702d-5b96-4745-bb4b-93911ee8c32a",
                                      "9f13aa99-9243-47eb-8fe4-82909b49de8e"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "a75ca215-681c-4000-2419-6225577239f0"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "6c7e7718-b800-483b-93f3-80f28a7bc597"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "943c0b1b-f3cf-4675-0208-402b0c86874c"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "43a5011e-911a-4061-b5fb-1f9295bcbba1",
                                      "98c03065-5271-4303-933a-0643af1d1e41"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "874fabfe-0570-483c-22bd-580c2c28ee87"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "b29f5e35-5e01-43f7-821e-690735a99e88"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "938cf1b8-6bc1-4b93-73a9-5baec9b1f1e6"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "eacc48e8-ebbd-4b6f-983f-3522865eccea",
                                      "d96c0f65-1001-4220-bd80-405060bbf3be"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "0fb6b780-73bc-450f-6c74-3fc1b23cb7d4"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "b54a3553-8d6a-4d45-aac4-2b0a904e6f47"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "52e570a0-208b-4295-5702-da746e00b073"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "4062100a-ada2-4974-86d2-09b606596bc7",
                                      "c5ea2fb3-b3f6-4d9f-8b22-c9c2b69eed9e"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "58d4b9a9-536c-47f3-77b9-5606a25ca466"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "c3120a4c-a5c3-4dbb-bbfc-64d55b176952"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "5708c274-b7fe-406d-fcc3-8b85a91e7685"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "4166efe4-26a0-4e20-bc84-84cc78632bbc",
                                      "23ce5e1d-2074-4f84-9ccd-249819030054"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "0d193497-b532-4a12-6f68-35806745e3c1"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "3bedcd77-a506-47a8-ba59-01a915ddb5c5"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "9a7a90b4-4ab1-4825-812f-f6e7a54c1894"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "9d957be1-94c8-4be8-9e1d-d497471943b7",
                                      "055229aa-c10a-40af-92b9-15086096dac4"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "df6b83e0-258d-49ef-a5f0-eb18d0cfcb3c"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "8366474d-7afb-4d6d-a376-ad2b09c807f4"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "11652079-c692-4adf-c5ef-dccb275f5c96"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "3a0fbea6-d40d-40ab-9467-91900438d9b1",
                                      "643826f8-c109-4b42-b763-be039ea7752b"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "5fa59b4f-cf8c-4e4c-f3f5-ee5314ebd8d4"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "995b4adb-e856-4c4b-a559-8af1bfeeb99b"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "c6b6170e-2f99-47d2-0134-ab8706d508ed"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "8ba36e03-6def-4479-8aac-454dc21caa9b",
                                      "200b31fc-137b-4a05-838e-ff532b95eba3"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "fd2d4fd9-7241-4753-a44d-1fcfe9f31005"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "6209f825-d137-48f9-8081-c095aab9849f"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "c6932a96-4f81-47cc-a93a-ed98504f8360"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "8fcf61aa-31f1-43dd-bf20-4ce67f6de2b5",
                                      "ca70fd76-da76-41a2-9159-6dd4c034c156"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "b3d0a464-a686-457e-fe90-f815ac54e375"
                              },
                              {
                                "column_rectangle": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "a4eac031-6f19-41ac-a041-d04902dfd9ba"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "8e7848f2-4fc3-4182-2fea-78a5703b460f"
                              },
                              {
                                "column_grid": {},
                                "queries": [
                                  {
                                    "productIds": [
                                      "5de2c884-8b0d-4db2-893e-6824d603454e",
                                      "46e14441-e111-4773-b845-8ba31bd1db60"
                                    ],
                                    "queryContentTypes": [
                                      "Durable",
                                      "Collection",
                                      "Bundle",
                                      "Persona",
                                      "Genoa",
                                      "BuildplateOffer",
                                      "RubyOffer",
                                      "InventoryItemOffer"
                                    ],
                                    "topCount": 25
                                  }
                                ],
                                "componentId": "27b3732f-3fd6-402e-a128-e4d2576a24a9"
                              }
                            ],
                            "tabIcon": "textures/ui/icons/pixel_icons/boost",
                            "tabTitle": "editorialtool.earth.Boosts",
                            "tabId": "boosts"
                          },
                              {
                  "screenLayoutQueries": [
                    {
                      "column_grid": {},
                      "queries": [
                        {
                          "productIds": [
                            "536dba5b-408d-4c47-a27d-f8f36707f16a",
                            "259ea494-5b5f-4290-a1bb-a1bee005fa35",
                            "7173136a-2bc6-4ec4-b718-3d6fe9f6735e",
                            "4eeda6ab-bd85-4327-9113-3d8fdc4fd61a",
                            "be87981e-52af-41bf-b96f-64f777580d67"
                          ],
                          "queryContentTypes": [
                            "Durable",
                            "Collection",
                            "Bundle",
                            "Persona",
                            "Genoa",
                            "BuildplateOffer",
                            "RubyOffer",
                            "InventoryItemOffer"
                          ],
                          "topCount": 25
                        }
                      ],
                      "componentId": "ad07683e-f114-41a9-25cf-bd2368a7d8e4"
                    }
                  ],
                  "tabIcon": "textures/ui/icons/pixel_icons/ruby",
                  "tabTitle": "editorialtool.earth.Rubies",
                  "tabId": "ruby"
                }
                        ],
                        "globalNotSearchQueryTags": [
                          "hidden_offer",
                          "earth_achievement"
                        ]
                      },
                      "ETag": "\"040060e3-0000-0300-0000-60ef93270000\""
                    }
                  }
                }
                """, "application/json");
        }

        if (!Guid.TryParse(request.ItemId, out var itemId))
        {
            return JsonPascalCase(new PlayfabErrorResponse(
                400,
                "BadRequest",
                "InvalidParams",
                1000,
                "Invalid input parameters",
                new()
                {
                    ["ItemId"] = ["The ItemId field is required."]
                }
            ));
        }

        if (!staticData.Playfab.Items.TryGetValue(itemId, out var item))
        {
            // TODO: fake not found
            return NotFound();
        }

        return Content(JsonSerializer.Serialize(new PlayfabOkResponse(
            200,
            "OK",
            new GetPublishedItemResponse(
                CIItemToItem(item, $"{(Request.IsHttps ? "https://" : "http://")}{Request.Host.Value}") with { ETag = "\"040060e3-0000-0300-0000-60ef93270000\"" }
            )
        ), jsonOptions), "application/json");
    }

    private static Item CIItemToItem(CItem item, string serverHostname)
    {
        Item.PriceR? price = item.Data switch
        {
            CItem.BuildplateData data => new Item.PriceR([
                new([
                    new("8b77345d-6250-4321-b3c2-373468b39457", "8b77345d-6250-4321-b3c2-373468b39457", "8b77345d-6250-4321-b3c2-373468b39457", data.Cost),
                ]),
            ], []),
            CItem.InventoryItemData data => new Item.PriceR([
                new([
                    new("8b77345d-6250-4321-b3c2-373468b39457", "8b77345d-6250-4321-b3c2-373468b39457", "8b77345d-6250-4321-b3c2-373468b39457", data.Cost),
                ]),
            ], []),
            CItem.RubyData => null,
            CItem.QueryManifestData => null,
            _ => throw new UnreachableException(),
        };

        return new Item(
            new Item.Entity(item.SourceEntityId, "namespace", "namespace"),
            new Item.Entity(item.SourceEntityId, "namespace", "namespace"),
            item.Id,
            item.Data switch
            {
                CItem.BuildplateData => "bundle",
                CItem.InventoryItemData => "bundle",
                CItem.RubyData => "catalogItem",
                CItem.QueryManifestData => "catalogItem",
                _ => throw new UnreachableException(),
            },
            item.FriendlyId is null ? [] : [new("FriendlyId", item.FriendlyId.Value)],
            item.FriendlyId,
            ((IEnumerable<KeyValuePair<string, string>>)[new("NEUTRAL", item.Title), .. item.TitleTranslations, new("neutral", item.Title)])
                .ToDictionary(),
            ((IEnumerable<KeyValuePair<string, string>>)[new("NEUTRAL", item.Description), .. item.DescriptionTranslations, new("neutral", item.Description)])
                .ToDictionary(),
            item.Keywords.ToDictionary(item => item.Key, item => new Item.KeywordValues(item.Value.Values)),
            item.Data switch
            {
                CItem.BuildplateData => "BuildplateOffer",
                CItem.InventoryItemData => "InventoryItemOffer",
                CItem.RubyData => "RubyOffer",
                CItem.QueryManifestData => "GenoaQueryManifest_V0.0.3",
                _ => throw new UnreachableException(),
            },
            new Item.Entity(item.CreatorEntityId, "title_player_account", "title_player_account"),
            new Item.Entity(item.CreatorEntityId, "title_player_account", "title_player_account"),
            item.Data is CItem.RubyData ? false : null, // IsStackable
            item.Data switch
            {
                CItem.BuildplateData => ["android.amazonappstore", "android.googleplay", "b.store", "ios.store", "nx.store", "oculus.store.gearvr", "oculus.store.rift", "uwp.store", "uwp.store.mobile", "xboxone.store", "title.bedrockvanilla", "title.earth"],
                CItem.InventoryItemData => ["android.googleplay", "ios.store", "uwp.store", "title.earth"],
                CItem.RubyData => ["android.googleplay", "ios.store", "uwp.store", "title.bedrockvanilla", "title.earth"],
                CItem.QueryManifestData => ["android.googleplay", "ios.store", "uwp.store", "title.earth"],
                _ => throw new UnreachableException(),
            },
            item.Tags,
            item.CreationDate,
            item.LastModifiedDate,
            item.StartDate,
            item.Contents,
            item.ThumbnailImageId is null ? [] : [new(item.ThumbnailImageId, "Thumbnail", "Thumbnail", "https://xforgeassets001.xboxlive.com/pf-title-b63a0803d3653643-ee7b/f5c84c8b-3613-4783-9f5c-d9f910469332/default_Thumbnail.jpg"/*$"{serverHostname}/playfab/images/{item.ThumbnailImageId}.jpg"*/)],
            item.ItemReferences.Select(reference => new Item.ItemReference(reference.Id, reference.Amount)),
            price,
            price,
            [],
            item.Data switch
            {
                CItem.BuildplateData data => Item.DisplayPropertiesR.CreateBuildplate(
                    "Minecraft",
                    data.Cost,
                    item.Purchasable,
                    data.Rarity.ToString().ToLowerInvariant(),
                    [new("entitlement_EarthBuildPlate", data.Id, data.Version)],
                    data.Id,
                    data.Size.ToString().ToLowerInvariant(),
                    data.UnlockLevel
                ),
                CItem.InventoryItemData data => Item.DisplayPropertiesR.CreateInventoryItem(
                    data.Cost,
                    data.Rarity.ToString(),
                    [new("entitlement_InventoryItemOffer", data.Id, data.Version)],
                    data.Id,
                    data.Amount
                ),
                CItem.RubyData data => Item.DisplayPropertiesR.CreateRuby(
                    data.BonusCoinCount,
                    data.CoinCount,
                    data.OriginalCreatorId,
                    data.Sku
                ),
                CItem.QueryManifestData data => Item.DisplayPropertiesR.CreateQueryManifest(
                    data.MinClientVersion,
                    data.MaxClientVersion,
                    data.Tabs.Select(tab => new Item.DisplayPropertiesR.Tab(
                        tab.ScreenLayoutQueries.Select(layoutQuery => new Item.DisplayPropertiesR.Tab.ScreenLayoutQuery(
                            // TODO: haven't seen it yet, but it's possible these can have properties
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Rectangle ? new object() : null,
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Square ? new object() : null,
                            layoutQuery.ColumnType is StaticData.Playfab.Tab.ColumnType.Grid ? new object() : null,
                            layoutQuery.Queries.Select(query => new Item.DisplayPropertiesR.Tab.ScreenLayoutQuery.Query(
                                query.ProductIds,
                                query.QueryContentTypes.Select(type => type.ToString()),
                                query.TopCount
                            )),
                            layoutQuery.ComponentId
                        )),
                        tab.TabIcon,
                        tab.TabTitle,
                        tab.TabId
                    )),
                    data.GlobalNotSearchQueryTags
                ),
                _ => throw new UnreachableException(),
            }
        );
    }

    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Item>("Item");

        builder.ComplexType<Item.Entity>();
        builder.ComplexType<Item.AlternateId>();
        builder.ComplexType<Item.KeywordValues>();
        builder.ComplexType<Item.Image>();
        builder.ComplexType<Item.ItemReference>();
        builder.ComplexType<Item.PriceR>();
        builder.ComplexType<Item.PriceR.Price>();
        builder.ComplexType<Item.CurrencyAmount>();
        builder.ComplexType<Item.DisplayPropertiesR>();
        builder.ComplexType<Item.DisplayPropertiesR.Tab>();
        builder.ComplexType<Item.DisplayPropertiesR.Tab.ScreenLayoutQuery>();
        builder.ComplexType<Item.DisplayPropertiesR.Tab.ScreenLayoutQuery.Query>();

        return builder.GetEdmModel();
    }

    private sealed record Item(
        Item.Entity SourceEntity,
        Item.Entity SourceEntityKey,
        Guid Id,
        string Type,
        IEnumerable<Item.AlternateId> AlternateIds,
        Guid? FriendlyId,
        Dictionary<string, string> Title,
        Dictionary<string, string> Description,
        Dictionary<string, Item.KeywordValues> Keywords,
        string ContentType,
        Item.Entity CreatorEntityKey,
        Item.Entity CreatorEntity,
        bool? IsStackable, // TODO: ??? only used for ruby offer, always false
        IEnumerable<string> Platforms,
        IEnumerable<string> Tags,
        DateTime CreationDate,
        DateTime LastModifiedDate,
        DateTime? StartDate,
        IEnumerable<IReadOnlyDictionary<string, object>> Contents,
        IEnumerable<Item.Image> Images,
        IEnumerable<Item.ItemReference> ItemReferences,
        Item.PriceR? Price,
        Item.PriceR? PriceOptions,
        IEnumerable<object> DeepLinks,
        Item.DisplayPropertiesR DisplayProperties,
        string? ETag = null
    )
    {
        public sealed record Entity(
            string Id,
            string Type,
            string TypeString
        );

        public sealed record AlternateId(
            string Type,
            Guid Value
        );

        public sealed record KeywordValues(
            IEnumerable<string> Values
        );

        public sealed record Image(
            string Id,
            string Tag,
            string Type,
            string Url
        );

        public sealed record ItemReference(
            Guid Id,
            int Amount
        );

        public sealed record PriceR(
            PriceR.Price[] Prices,
            PriceR.Price[] RealPrices
        )
        {
            public sealed record Price(
                CurrencyAmount[] Amounts
            );
        }

        public sealed record CurrencyAmount(
            string CurrencyId,
            string Id,
            string ItemId,
            int Amount
        );

        public sealed record PackIdentity(
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("uuid")] Guid Uuid,
            [property: JsonPropertyName("version")] string Version
        );

        public sealed record DisplayPropertiesR(
            // query manifest
            [property: JsonPropertyName("minClientVersion")] string? MinClientVersion = null,
            [property: JsonPropertyName("maxClientVersion")] string? MaxClientVersion = null,
            [property: JsonPropertyName("tabs")] IEnumerable<DisplayPropertiesR.Tab>? Tabs = null,
            [property: JsonPropertyName("globalNotSearchQueryTags")] IEnumerable<string>? GlobalNotSearchQueryTags = null,

            // buildplate, inventory item, persona
            [property: JsonPropertyName("price")] int? Price = null,
            [property: JsonPropertyName("rarity")] string? Rarity = null,
            [property: JsonPropertyName("packIdentity")] IEnumerable<PackIdentity>? PackIdentity = null,

            // buildplate, persona
            [property: JsonPropertyName("creatorName")] string? CreatorName = null,
            [property: JsonPropertyName("purchasable")] bool? Purchasable = null,

            // buildplate
            [property: JsonPropertyName("buildPlateId")] Guid? BuildPlateId = null,
            [property: JsonPropertyName("buildPlateSize")] string? BuildPlateSize = null,
            [property: JsonPropertyName("buildPlateUnlockLevel"), JsonNumberHandling(JsonNumberHandling.WriteAsString)] int? BuildPlateUnlockLevel = null,

            // inventory item
            [property: JsonPropertyName("itemId")] Guid? ItemId = null,
            [property: JsonPropertyName("amount")] int? Amount = null,

            // ruby
            [property: JsonPropertyName("bonusCoinCount")] int? BonusCoinCount = null,
            [property: JsonPropertyName("coinCount")] int? CoinCount = null,
            [property: JsonPropertyName("originalCreatorId")] string? OriginalCreatorId = null,
            [property: JsonPropertyName("sku")] string? Sku = null,

            // persona
            [property: JsonPropertyName("offerId")] Guid? OfferId = null,
            [property: JsonPropertyName("pieceType")] string? PieceType = null
        )
        {
            public static DisplayPropertiesR CreateQueryManifest(string minClientVersion, string maxClientVersion, IEnumerable<Tab> tabs, IEnumerable<string> globalNotSearchQueryTags)
                => new DisplayPropertiesR(MinClientVersion: minClientVersion, MaxClientVersion: maxClientVersion, Tabs: tabs, GlobalNotSearchQueryTags: globalNotSearchQueryTags);

            public static DisplayPropertiesR CreateBuildplate(string creatorName, int price, bool purchasable, string rarity, IEnumerable<PackIdentity> packIdentity, Guid buildPlateId, string buildPlateSize, int buildPlateUnlockLevel)
                => new DisplayPropertiesR(CreatorName: creatorName, Price: price, Purchasable: purchasable, Rarity: rarity, PackIdentity: packIdentity, BuildPlateId: buildPlateId, BuildPlateSize: buildPlateSize, BuildPlateUnlockLevel: buildPlateUnlockLevel);

            public static DisplayPropertiesR CreateInventoryItem(int price, string rarity, IEnumerable<PackIdentity> packIdentity, Guid itemId, int amount)
                => new DisplayPropertiesR(Price: price, Rarity: rarity, PackIdentity: packIdentity, ItemId: itemId, Amount: amount);

            public static DisplayPropertiesR CreateRuby(int? bonusCoinCount, int coinCount, string originalCreatorId, string sku)
                => new DisplayPropertiesR(BonusCoinCount: bonusCoinCount, CoinCount: coinCount, OriginalCreatorId: originalCreatorId, Sku: sku);

            public static DisplayPropertiesR CreatePersona(string creatorName, int price, bool purchasable, string rarity, IEnumerable<PackIdentity> packIdentity, Guid offerId, string pieceType)
                => new DisplayPropertiesR(CreatorName: creatorName, Price: price, Purchasable: purchasable, Rarity: rarity, PackIdentity: packIdentity, OfferId: offerId, PieceType: pieceType);

            public sealed record Tab(
              [property: JsonPropertyName("screenLayoutQueries")] IEnumerable<Tab.ScreenLayoutQuery> ScreenLayoutQueries,
              [property: JsonPropertyName("tabIcon")] string TabIcon,
              [property: JsonPropertyName("tabTitle")] string TabTitle,
              [property: JsonPropertyName("tabId")] string TabId
          )
            {
                public sealed record ScreenLayoutQuery(
                    [property: JsonPropertyName("column_rectangle")] object? ColumnRectangle,
                    [property: JsonPropertyName("column_square")] object? ColumnSquare,
                    [property: JsonPropertyName("column_grid")] object? ColumnGrid,
                    [property: JsonPropertyName("queries")] IEnumerable<ScreenLayoutQuery.Query> Queries,
                    [property: JsonPropertyName("componentId")] Guid ComponentId
                )
                {
                    public sealed record Query(
                        [property: JsonPropertyName("productIds")] IEnumerable<string> ProductIds,
                        [property: JsonPropertyName("queryContentTypes")] IEnumerable<string> QueryContentTypes,
                        [property: JsonPropertyName("topCount")] int TopCount
                    );
                }
            }
        }
    }
}
