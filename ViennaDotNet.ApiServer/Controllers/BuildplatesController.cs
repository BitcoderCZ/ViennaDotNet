using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using ViennaDotNet.ApiServer.Exceptions;
using ViennaDotNet.ApiServer.Types.Buildplates;
using ViennaDotNet.ApiServer.Types.Common;
using ViennaDotNet.ApiServer.Types.Inventory;
using ViennaDotNet.ApiServer.Utils;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Global;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.ObjectStore.Client;
using ViennaDotNet.StaticData;

namespace ViennaDotNet.ApiServer.Controllers;

[Authorize]
[ApiVersion("1.1")]
[Route("1/api/v{version:apiVersion}")]
public class BuildplatesController : ControllerBase
{
    private static EarthDB earthDB => Program.DB;
    private static ObjectStoreClient objectStoreClient => Program.objectStore;
    private static BuildplateInstancesManager buildplateInstancesManager => Program.buildplateInstancesManager;
    private static Catalog catalog => Program.staticData.catalog;
    private static TappablesManager tappablesManager => Program.tappablesManager;

    [HttpGet("buildplates")]
    public async Task<IActionResult> GetBuildplates(CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
            return BadRequest();

        Buildplates buildplatesModel;
        try
        {
            EarthDB.Results results = await new EarthDB.Query(false)
                .Get("buildplates", playerId, typeof(Buildplates))
                .ExecuteAsync(earthDB, cancellationToken);
            buildplatesModel = (Buildplates)results.Get("buildplates").Value;
        }
        catch (EarthDB.DatabaseException ex)
        {
            throw new ServerErrorException(ex);
        }

        // not null is ensured in .Where
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        OwnedBuildplate[] ownedBuildplates = [.. buildplatesModel.getBuildplates().Select(async buildplateEntry =>
        {
            byte[]? previewData = (await objectStoreClient.get(buildplateEntry.buildplate.previewObjectId).Task) as byte[];
            if (previewData == null)
            {
                Log.Error($"Preview object {buildplateEntry.buildplate.previewObjectId} for buildplate {buildplateEntry.id} could not be loaded from object store");
                return null;
            }

            string model = Encoding.ASCII.GetString(previewData);
            return new OwnedBuildplate(
                buildplateEntry.id,
                "00000000-0000-0000-0000-000000000000",
                new Dimension(buildplateEntry.buildplate.size, buildplateEntry.buildplate.size),
                new Offset(0, buildplateEntry.buildplate.offset, 0),
                buildplateEntry.buildplate.scale,
                OwnedBuildplate.Type.SURVIVAL,
                SurfaceOrientation.HORIZONTAL,
                model,
                0,    // TODO
                false,    // TODO
                0,    // TODO
                false,    // TODO
                TimeFormatter.FormatTime(buildplateEntry.buildplate.lastModified),
                0,    // TODO
                ""
            );
        }).Where(ownedBuildplate => ownedBuildplate != null)
        .Select(task => task.Result)];
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(ownedBuildplates));
        return Content(resp, "application/json");
    }

    [HttpPost("multiplayer/buildplate/{buildplateId}/instances")]
    public Task<IActionResult> CreateBuildInstance(string buildplateId, CancellationToken cancellationToken)
    {
        // TODO: coordinates etc.

        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return Task.FromResult((IActionResult)BadRequest());
        }

        return getNewBuildplateInstanceResponse(playerId, buildplateId, BuildplateInstancesManager.InstanceType.BUILD, cancellationToken);
    }

    [HttpPost("multiplayer/buildplate/{buildplateId}/play/instances")]
    public Task<IActionResult> CreatePlayInstance(string buildplateId, CancellationToken cancellationToken)
    {
        // TODO: coordinates etc.

        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return Task.FromResult((IActionResult)BadRequest());
        }

        return getNewBuildplateInstanceResponse(playerId, buildplateId, BuildplateInstancesManager.InstanceType.PLAY, cancellationToken);
    }

    [HttpPost("buildplates/{buildplateId}/share")]
    public async Task<IActionResult> ShareBuildplate(string buildplateId, CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return BadRequest();
        }

        long requestStartedOn = HttpContext.GetTimestamp();

        DB.Models.Player.Inventory inventory;
        Hotbar hotbar;
        Buildplates.Buildplate? buildplate;
        try
        {
            EarthDB.Results results = await new EarthDB.Query(false)
                .Get("inventory", playerId, typeof(DB.Models.Player.Inventory))
                .Get("hotbar", playerId, typeof(Hotbar))
                .Get("buildplates", playerId, typeof(Buildplates))
                .ExecuteAsync(earthDB, cancellationToken);

            inventory = (DB.Models.Player.Inventory)results.Get("inventory").Value;
            hotbar = (Hotbar)results.Get("hotbar").Value;
            buildplate = ((Buildplates)results.Get("buildplates").Value).getBuildplate(buildplateId);
        }
        catch (EarthDB.DatabaseException exception)
        {
            throw new ServerErrorException(exception);
        }

        if (buildplate is null)
        {
            return NotFound();
        }

        byte[]? serverData = (await objectStoreClient.get(buildplate.serverDataObjectId).Task) as byte[];
        if (serverData is null)
        {
            Log.Error($"Data object {buildplate.serverDataObjectId} for buildplate {buildplateId} could not be loaded from object store");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string? sharedBuildplateServerDataObjectId = (await objectStoreClient.store(serverData).Task) as string;
        if (sharedBuildplateServerDataObjectId is null)
        {
            Log.Error("Could not store data object for shared buildplate in object store");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string sharedBuildplateId = U.RandomUuid().ToString();
        SharedBuildplates.SharedBuildplate sharedBuildplate = new SharedBuildplates.SharedBuildplate(
            playerId,
            buildplate.size,
            buildplate.offset,
            buildplate.scale,
            buildplate.night,
            requestStartedOn,
            buildplate.lastModified,
            sharedBuildplateServerDataObjectId
        );

        for (int index = 0; index < 7; index++)
        {
            Hotbar.Item? item = hotbar.items[index];
            SharedBuildplates.SharedBuildplate.HotbarItem? sharedBuildplateHotbarItem;
            if (item is null)
            {
                sharedBuildplateHotbarItem = null;
            }
            else if (item.instanceId is null)
            {
                sharedBuildplateHotbarItem = new SharedBuildplates.SharedBuildplate.HotbarItem(item.uuid, item.count, null, 0);
            }
            else
            {
                sharedBuildplateHotbarItem = new SharedBuildplates.SharedBuildplate.HotbarItem(item.uuid, 1, item.instanceId, inventory.getItemInstance(item.uuid, item.instanceId)?.wear ?? 0);
            }

            sharedBuildplate.hotbar[index] = sharedBuildplateHotbarItem;
        }

        try
        {
            EarthDB.Results results = await new EarthDB.Query(true)
                .Get("sharedBuildplates", "", typeof(SharedBuildplates))
                .Then(results1 =>
                {
                    SharedBuildplates sharedBuildplates = (SharedBuildplates)results1.Get("sharedBuildplates").Value;

                    sharedBuildplates.addSharedBuildplate(sharedBuildplateId, sharedBuildplate);

                    return new EarthDB.Query(true)
                        .Update("sharedBuildplates", "", sharedBuildplates);
                })
                .ExecuteAsync(earthDB, cancellationToken);
        }
        catch (EarthDB.DatabaseException exception)
        {
            objectStoreClient.delete(sharedBuildplateServerDataObjectId);
            throw new ServerErrorException(exception);
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse($"minecraftearth://sharedbuildplate?id={sharedBuildplateId}"));
        return Content(resp, "application/json");
    }

    [HttpGet("buildplates/shared/{sharedBuildplateId}")]
    public async Task<IActionResult> GetSharedBuildplate(string sharedBuildplateId, CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return BadRequest();
        }

        SharedBuildplates.SharedBuildplate? sharedBuildplate;
        try
        {
            EarthDB.Results results = await new EarthDB.Query(false)
                    .Get("sharedBuildplates", "", typeof(SharedBuildplates))
                        .ExecuteAsync(earthDB, cancellationToken);
            SharedBuildplates sharedBuildplates = (SharedBuildplates)results.Get("sharedBuildplates").Value;
            sharedBuildplate = sharedBuildplates.getSharedBuildplate(sharedBuildplateId);
        }
        catch (EarthDB.DatabaseException exception)
        {
            throw new ServerErrorException(exception);
        }

        if (sharedBuildplate is null)
        {
            return NotFound();
        }

        byte[]? serverData = (await objectStoreClient.get(sharedBuildplate.serverDataObjectId).Task) as byte[];
        if (serverData is null)
        {
            Log.Error($"Data object {sharedBuildplate.serverDataObjectId} for shared buildplate {sharedBuildplateId} could not be loaded from object store");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string? preview = buildplateInstancesManager.getBuildplatePreview(serverData, sharedBuildplate.night);
        if (preview is null)
        {
            Log.Error("Could not get preview for buildplate");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(new SharedBuildplate(
            sharedBuildplate.playerId,    // TODO: supposed to return username here, not player ID
            TimeFormatter.FormatTime(sharedBuildplate.created),
            new SharedBuildplate.BuildplateData(
                new Dimension(sharedBuildplate.size, sharedBuildplate.size),
                new Offset(0, sharedBuildplate.offset, 0),
                sharedBuildplate.scale,
                SharedBuildplate.BuildplateData.Type.SURVIVAL,
                SurfaceOrientation.HORIZONTAL,
                preview,
                0
            ),
            new Types.Inventory.Inventory(
                [.. sharedBuildplate.hotbar.Select(item => item is not null ? new HotbarItem(
                    item.uuid,
                    item.count,
                    item.instanceId,
                    item.instanceId is not null ? ItemWear.wearToHealth(item.uuid, item.wear, catalog.itemsCatalog) : 0.0f
                ) : null)],
                [.. sharedBuildplate.hotbar
                    .Where(item => item is not null && item.instanceId is null)
                    .Select(item => item!.uuid)
                    .Distinct()
                    .Select(uuid => new StackableInventoryItem(
                        uuid,
                        0,
                        1,
                        // TODO: what unlocked/last seen timestamp are we supposed to use here - the player who shared the buildplate or the player who is viewing the buildplate?
                        new StackableInventoryItem.On(TimeFormatter.FormatTime(0)),
                        new StackableInventoryItem.On(TimeFormatter.FormatTime(0))
                    ))],
                [.. sharedBuildplate.hotbar
                    .Where(item => item is not null && item.instanceId is not null)
                    .Select(item => item!.uuid)
                    .Distinct()
                    .Select(uuid => new NonStackableInventoryItem(
                        uuid,
                        [],
                        1,
                        // TODO: what unlocked/last seen timestamp are we supposed to use here - the player who shared the buildplate or the player who is viewing the buildplate?
                        new NonStackableInventoryItem.On(TimeFormatter.FormatTime(0)),
                        new NonStackableInventoryItem.On(TimeFormatter.FormatTime(0))
                    ))]
            )
        )));
        return Content(resp, "application/json");
    }

    [HttpPost("multiplayer/buildplate/shared/{sharedBuildplateId}/play/instances")]
    public async Task<IActionResult> GetSharedBuildplateInstance(string sharedBuildplateId, CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
            return BadRequest();

        // TODO: coordinates etc.

        SharedBuildplateInstanceRequest sharedBuildplateInstanceRequest = (await Request.Body.AsJsonAsync<SharedBuildplateInstanceRequest>(cancellationToken))!;

        return await getNewSharedBuildplateInstanceResponse(playerId, sharedBuildplateId, sharedBuildplateInstanceRequest.fullSize ? BuildplateInstancesManager.InstanceType.SHARED_PLAY : BuildplateInstancesManager.InstanceType.SHARED_BUILD, cancellationToken);
    }

    private sealed record EncounterInstanceRequest(
        string tileId
    );

    [HttpPost("multiplayer/encounters/{encounterId}/instances")]
    public async Task<IActionResult> CreateEncounterInstance(string encounterId, CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return BadRequest();
        }

        var encounterInstanceRequest = await Request.Body.AsJsonAsync<EncounterInstanceRequest>(cancellationToken);

        if (encounterInstanceRequest is null)
        {
            return BadRequest();
        }

        return await getNewEncounterBuildplateInstanceResponse(encounterId, encounterInstanceRequest.tileId, tappablesManager, cancellationToken);
    }

    // TODO: should we restrict this to matching player ID?
    [HttpGet("multiplayer/partitions/{partitionId}/instances/{instanceId}")]
    public async Task<IActionResult> GetInstanceStatus(string partitionId, string instanceId, CancellationToken cancellationToken)
    {
        string? playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
            return BadRequest();

        BuildplateInstancesManager.InstanceInfo? instanceInfo = buildplateInstancesManager.getInstanceInfo(instanceId);
        if (instanceInfo is null || instanceInfo.shuttingDown)
        {
            return NotFound();
        }

        Buildplates.Buildplate? buildplate;
        try
        {
            EarthDB.Results results = await new EarthDB.Query(false)
                    .Get("buildplates", playerId, typeof(Buildplates))
                    .ExecuteAsync(earthDB, cancellationToken);
            buildplate = ((Buildplates)results.Get("buildplates").Value).getBuildplate(instanceInfo.buildplateId);
        }
        catch (EarthDB.DatabaseException ex)
        {
            throw new ServerErrorException(ex);
        }

        if (buildplate is null)
        {
            return NotFound();
        }

        // TODO: the client is supposed to poll until the buildplate server is ready, but instead it just crashes if we tell it that the buildplate server is not ready yet
        // TODO: so instead we just stall the request until it's ready, this is really ugly and eventually we need to figure out why it's crashing and implement this properly
        // TODO: this also relies on the buildplate server starting in less than ~20 seconds as the client will eventually time out the HTTP request and crash anyway
        //BuildplateInstance buildplateInstance = this.instanceInfoToApiResponse(instanceInfo);
        BuildplateInstancesManager.InstanceInfo? instanceInfo1;
        int waitCount = 0;
        do
        {
            instanceInfo1 = buildplateInstancesManager.getInstanceInfo(instanceId);
            if (instanceInfo1 is null || instanceInfo1.shuttingDown)
            {
                return NotFound();
            }

            if (!instanceInfo1.ready)
            {
                await Task.Delay(1000, cancellationToken);

                waitCount++;
            }
        }
        while (!instanceInfo1.ready && waitCount < 35);
        BuildplateInstance? buildplateInstance = await instanceInfoToApiResponse(instanceInfo1, cancellationToken);

        if (buildplateInstance is null)
        {
            return NotFound();
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(buildplateInstance));
        return Content(resp, "application/json");
    }

    private async Task<IActionResult> getNewBuildplateInstanceResponse(string playerId, string buildplateId, BuildplateInstancesManager.InstanceType type, CancellationToken cancellationToken)
    {
        Buildplates.Buildplate? buildplate;
        try

        {
            EarthDB.Results results = await new EarthDB.Query(false)
                .Get("buildplates", playerId, typeof(Buildplates))
                .ExecuteAsync(earthDB, cancellationToken);

            buildplate = ((Buildplates)results.Get("buildplates").Value).getBuildplate(buildplateId);
        }
        catch (EarthDB.DatabaseException exception)
        {
            throw new ServerErrorException(exception);
        }

        if (buildplate is null)
        {
            return NotFound();
        }

        string? instanceId = await buildplateInstancesManager.requestBuildplateInstance(playerId, null, buildplateId, type, 0, buildplate.night);
        if (instanceId is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstancesManager.InstanceInfo? instanceInfo = buildplateInstancesManager.getInstanceInfo(instanceId);
        if (instanceInfo is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstance? buildplateInstance = await instanceInfoToApiResponse(instanceInfo, cancellationToken);

        if (buildplateInstance is null)
        {
            return NotFound();
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(buildplateInstance));
        return Content(resp, "application/json");
    }

    private async Task<IActionResult> getNewSharedBuildplateInstanceResponse(string playerId, string sharedBuildplateId, BuildplateInstancesManager.InstanceType type, CancellationToken cancellationToken)
    {
        SharedBuildplates.SharedBuildplate? sharedBuildplate;
        try
        {
            EarthDB.Results results = await new EarthDB.Query(false)
                .Get("sharedBuildplates", "", typeof(SharedBuildplates))
                .ExecuteAsync(earthDB, cancellationToken);
            sharedBuildplate = ((SharedBuildplates)results.Get("sharedBuildplates").Value).getSharedBuildplate(sharedBuildplateId);
        }
        catch (EarthDB.DatabaseException exception)
        {
            throw new ServerErrorException(exception);
        }

        if (sharedBuildplate is null)
        {
            return NotFound();
        }

        string? instanceId = await buildplateInstancesManager.requestBuildplateInstance(playerId, null, sharedBuildplateId, type, 0, sharedBuildplate.night);
        if (instanceId is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstancesManager.InstanceInfo? instanceInfo = buildplateInstancesManager.getInstanceInfo(instanceId);
        if (instanceInfo is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstance? buildplateInstance = await instanceInfoToApiResponse(instanceInfo, cancellationToken);
        if (buildplateInstance is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(buildplateInstance));
        return Content(resp, "application/json");
    }

    private async Task<IActionResult> getNewEncounterBuildplateInstanceResponse(string encounterId, string tileId, TappablesManager tappablesManager, CancellationToken cancellationToken)
    {
        TappablesManager.Encounter? encounter = tappablesManager.getEncounterWithId(encounterId, tileId);
        if (encounter is null)
        {
            return NotFound();
        }

        string? instanceId = await buildplateInstancesManager.requestBuildplateInstance(null, encounterId, encounter.encounterBuildplateId, BuildplateInstancesManager.InstanceType.ENCOUNTER, encounter.spawnTime + encounter.validFor, false);

        if (instanceId is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstancesManager.InstanceInfo? instanceInfo = buildplateInstancesManager.getInstanceInfo(instanceId);
        if (instanceInfo is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BuildplateInstance? buildplateInstance = await instanceInfoToApiResponse(instanceInfo, cancellationToken);
        if (buildplateInstance is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        string resp = JsonConvert.SerializeObject(new EarthApiResponse(buildplateInstance));
        return Content(resp, "application/json");
    }

    [JsonConverter(typeof(StringEnumConverter))]
    private enum Source
    {
        PLAYER,
        SHARED,
        ENCOUNTER
    }

    private static async Task<BuildplateInstance?> instanceInfoToApiResponse(BuildplateInstancesManager.InstanceInfo instanceInfo, CancellationToken cancellationToken)
    {
        var (fullsize, gameplayMode, source) = instanceInfo.type switch
        {
            BuildplateInstancesManager.InstanceType.BUILD => (false, BuildplateInstance.GameplayMetadata.GameplayMode.BUILDPLATE, Source.PLAYER),
            BuildplateInstancesManager.InstanceType.PLAY => (true, BuildplateInstance.GameplayMetadata.GameplayMode.BUILDPLATE_PLAY, Source.PLAYER),
            BuildplateInstancesManager.InstanceType.SHARED_BUILD => (true, BuildplateInstance.GameplayMetadata.GameplayMode.SHARED_BUILDPLATE_PLAY, Source.SHARED),
            BuildplateInstancesManager.InstanceType.SHARED_PLAY => (true, BuildplateInstance.GameplayMetadata.GameplayMode.SHARED_BUILDPLATE_PLAY, Source.SHARED),
            BuildplateInstancesManager.InstanceType.ENCOUNTER => (true, BuildplateInstance.GameplayMetadata.GameplayMode.ENCOUNTER, Source.ENCOUNTER),
            _ => throw new UnreachableException(),
        };

        int size;
        int offset;
        int scale;
        switch (source)
        {
            case Source.PLAYER:
                {
                    Buildplates.Buildplate buildplate;
                    try
                    {
                        EarthDB.Results results = await new EarthDB.Query(false)
                            .Get("buildplates", instanceInfo.playerId, typeof(Buildplates))
                            .ExecuteAsync(earthDB, cancellationToken);
                        buildplate = ((Buildplates)results.Get("buildplates").Value).getBuildplate(instanceInfo.buildplateId);
                    }
                    catch (EarthDB.DatabaseException exception)
                    {
                        throw new ServerErrorException(exception);
                    }

                    if (buildplate is null)
                    {
                        return null;
                    }

                    size = buildplate.size;
                    offset = buildplate.offset;
                    scale = buildplate.scale;
                }

                break;
            case Source.SHARED:
                {
                    SharedBuildplates.SharedBuildplate? sharedBuildplate;
                    try
                    {
                        EarthDB.Results results = await new EarthDB.Query(false)
                            .Get("sharedBuildplates", "", typeof(SharedBuildplates))
                            .ExecuteAsync(earthDB, cancellationToken);
                        sharedBuildplate = ((SharedBuildplates)results.Get("sharedBuildplates").Value).getSharedBuildplate(instanceInfo.buildplateId);
                    }
                    catch (EarthDB.DatabaseException exception)
                    {
                        throw new ServerErrorException(exception);
                    }

                    if (sharedBuildplate is null)
                    {
                        return null;
                    }

                    size = sharedBuildplate.size;
                    offset = sharedBuildplate.offset;
                    scale = sharedBuildplate.scale;
                }

                break;
            case Source.ENCOUNTER:
                {
                    EncounterBuildplates.EncounterBuildplate encounterBuildplate;

                    try
                    {
                        EarthDB.Results results = await new EarthDB.Query(false)
                            .Get("encounterBuildplates", "", typeof(EncounterBuildplates))
                            .ExecuteAsync(earthDB, cancellationToken);
                        encounterBuildplate = ((EncounterBuildplates)results.Get("encounterBuildplates").Value).getEncounterBuildplate(instanceInfo.buildplateId);
                    }
                    catch (EarthDB.DatabaseException exception)
                    {
                        throw new ServerErrorException(exception);
                    }

                    if (encounterBuildplate is null)
                    {
                        return null;
                    }

                    size = encounterBuildplate.size;
                    offset = encounterBuildplate.offset;
                    scale = encounterBuildplate.scale;
                }

                break;
            default:
                throw new UnreachableException();
        }

        return new BuildplateInstance(
            instanceInfo.instanceId,
            "00000000-0000-0000-0000-000000000000",
            "d.projectearth.dev",    // TODO
            instanceInfo.address,
            instanceInfo.port,
            instanceInfo.ready,
            instanceInfo.ready ? BuildplateInstance.ApplicationStatus.READY : BuildplateInstance.ApplicationStatus.UNKNOWN,
            instanceInfo.ready ? BuildplateInstance.ServerStatus.RUNNING : BuildplateInstance.ServerStatus.RUNNING,
            JsonConvert.SerializeObject(new Dictionary<string, object>()
            {
                { "buildplateid", instanceInfo.buildplateId }
            }),
            new BuildplateInstance.GameplayMetadata(
                instanceInfo.buildplateId,
                "00000000-0000-0000-0000-000000000000",
                instanceInfo.playerId,
                "2020.1217.02",
                "CK06Yzm2",    // TODO
                new Dimension(size, size),
                new Offset(0, offset, 0),
                !fullsize ? scale : 1,
                fullsize,
                gameplayMode,
                SurfaceOrientation.HORIZONTAL,
                null,
                null,    // TODO
                []
            ),
            "776932eeeb69",
            //new Coordinate(50.99636722700025f, -0.7234904312500047f)
            new Coordinate(0.0f, 0.0f)    // TODO
        );
    }

    private sealed record SharedBuildplateInstanceRequest(
        bool fullSize
    );
}
