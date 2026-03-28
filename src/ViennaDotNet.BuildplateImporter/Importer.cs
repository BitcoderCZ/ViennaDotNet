using Serilog;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using ViennaDotNet.Buildplate.Model;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Global;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.EventBus.Client;
using ViennaDotNet.ObjectStore.Client;

namespace ViennaDotNet.BuildplateImporter;

public sealed class Importer
{
    private readonly EarthDB _earthDB;
    private readonly EventBusClient? _eventBusClient;
    private readonly ObjectStoreClient _objectStoreClient;
    private readonly ILogger _logger;

    public Importer(EarthDB earthDB, EventBusClient? eventBusClient, ObjectStoreClient objectStoreClient, ILogger logger)
    {
        _earthDB = earthDB;
        _eventBusClient = eventBusClient;
        _objectStoreClient = objectStoreClient;
        _logger = logger;
    }

    public async Task<bool> ImportTemplateAsync(string templateId, string name, Stream stream, CancellationToken cancellationToken = default)
    {
        var worldData = await WorldData.LoadFromZipAsync(stream, _logger, cancellationToken);

        if (worldData is null)
        {
            return false;
        }

        byte[] preview = await GeneratePreview(worldData);

        return await StoreTemplate(templateId, name, preview, worldData, cancellationToken);
    }

    public async Task<bool> RemoveTemplateAsync(string templateId, bool removeFromPlayers, CancellationToken cancellationToken = default)
    {
        _logger.Information($"Starting removal of template {templateId}");

        TemplateBuildplate? template;
        try
        {
            var results = await new EarthDB.ObjectQuery(false)
               .GetBuildplate(templateId)
               .ExecuteAsync(_earthDB, cancellationToken);

            template = results.GetBuildplate(templateId);
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to fetch template {templateId}: {ex}");
            return false;
        }

        if (template is null)
        {
            _logger.Warning($"Template {templateId} does not exist. Skipping.");
            return true;
        }

        if (removeFromPlayers)
        {
            var instances = new List<(string PlayerId, string BuildplateId)>();

            try
            {
                using var connection = _earthDB.OpenConnection(false);
                using var command = connection.CreateCommand();

                command.CommandText = """
                    SELECT objects.id, json_each.key 
                    FROM objects, json_each(objects.value, '$.buildplates')
                    WHERE objects.type = 'buildplates' 
                    AND json_extract(json_each.value, '$.templateId') = $templateId
                    """;

                var param = command.CreateParameter();
                param.ParameterName = "$templateId";
                param.Value = templateId;
                command.Parameters.Add(param);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    instances.Add((reader.GetString(0), reader.GetString(1)));
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error scanning players for template {templateId}: {ex}");
                return false;
            }

            _logger.Information($"Found {instances.Count} player buildplates to remove.");

            foreach (var (playerId, buildplateId) in instances)
            {
                await RemoveBuildplateFromPlayer(buildplateId, playerId, cancellationToken);
            }
        }

        try
        {
            await new EarthDB.ObjectQuery(true)
                .UpdateBuildplate(templateId, null)
                .ExecuteAsync(_earthDB, cancellationToken);
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to remove template {templateId} from DB: {ex}");
            return false;
        }

        if (!string.IsNullOrEmpty(template.ServerDataObjectId))
        {
            await _objectStoreClient.Delete(template.ServerDataObjectId).Task;
        }

        if (!string.IsNullOrEmpty(template.PreviewObjectId))
        {
            await _objectStoreClient.Delete(template.PreviewObjectId).Task;
        }

        _logger.Information($"Successfully purged template {templateId} and all associated player buildplates.");
        return true;
    }

    public async Task<string?> AddBuidplateToPlayer(string templateId, string playerId, CancellationToken cancellationToken = default)
    {
        TemplateBuildplate? template;
        try
        {
            var results = await new EarthDB.ObjectQuery(false)
               .GetBuildplate(templateId)
               .ExecuteAsync(_earthDB, cancellationToken);

            template = results.GetBuildplate(templateId);
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to get template buildplate '{templateId}': {ex}");
            return null;
        }

        if (template is null)
        {
            _logger.Error($"Template buildplate {templateId} not found");
            return null;
        }

        byte[]? serverData = (await _objectStoreClient.Get(template.ServerDataObjectId).Task) as byte[];

        if (serverData is null)
        {
            _logger.Error($"Could not get server data for template buildplate {templateId}");
            return null;
        }

        byte[]? preview = (await _objectStoreClient.Get(template.PreviewObjectId).Task) as byte[];

        if (preview is null)
        {
            _logger.Warning($"Could not get preview for template buildplate {templateId}");
            preview = await GeneratePreview(new WorldData(serverData, template.Size, template.Offset, template.Night));
        }

        string buidplateId = U.RandomUuid().ToString();

        if (!await StoreBuildplate(templateId, playerId, buidplateId, template, serverData, preview, cancellationToken))
        {
            return null;
        }

        return buidplateId;
    }

    public async Task<bool> RemoveBuildplateFromPlayer(string buildplateId, string playerId, CancellationToken cancellationToken = default)
    {
        _logger.Information($"Removing buildplate {buildplateId} from player {playerId}");

        string? serverDataObjectId = null;
        string? previewObjectId = null;

        try
        {
            await new EarthDB.Query(true)
                .Get("buildplates", playerId, typeof(Buildplates))
                .Then(results =>
                {
                    Buildplates buildplates = results.Get<Buildplates>("buildplates");

                    var buildplate = buildplates.GetBuildplate(buildplateId);
                    if (buildplate == null)
                    {
                        _logger.Warning($"Buildplate {buildplateId} not found for player {playerId}. Nothing to remove.");
                        return null;
                    }

                    serverDataObjectId = buildplate.ServerDataObjectId;
                    previewObjectId = buildplate.PreviewObjectId;

                    buildplates.RemoveBuildplate(buildplateId);

                    return new EarthDB.Query(true)
                        .Update("buildplates", playerId, buildplates);
                })
                .ExecuteAsync(_earthDB, cancellationToken);

            if (!string.IsNullOrEmpty(serverDataObjectId))
            {
                _logger.Information($"Deleting server data object {serverDataObjectId}");
                await _objectStoreClient.Delete(serverDataObjectId).Task;
            }

            if (!string.IsNullOrEmpty(previewObjectId))
            {
                _logger.Information($"Deleting preview object {previewObjectId}");
                await _objectStoreClient.Delete(previewObjectId).Task;
            }

            return true;
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to remove buildplate '{buildplateId}' from database for player '{playerId}': {ex}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error($"An unexpected error occurred while removing buildplate '{buildplateId}': {ex}");
            return false;
        }
    }

    private async Task<byte[]> GeneratePreview(WorldData worldData)
    {
        string? preview;
        if (_eventBusClient is not null)
        {
            _logger.Information("Generating preview");
            RequestSender requestSender = _eventBusClient.AddRequestSender();
            preview = await requestSender.Request("buildplates", "preview", JsonSerializer.Serialize(new PreviewRequest(Convert.ToBase64String(worldData.ServerData), worldData.Night))).Task;
            requestSender.Close();

            if (preview is null)
            {
                _logger.Warning("Could not get preview for buildplate (preview generator did not respond to event bus request)");
            }
        }
        else
        {
            _logger.Information("Preview was not generated because event bus is not connected");
            preview = null;
        }

        return preview is not null ? Encoding.ASCII.GetBytes(preview) : [];
    }

    private async Task<bool> StoreTemplate(string templateId, string name, byte[] preview, WorldData worldData, CancellationToken cancellationToken)
    {
        TemplateBuildplate? template;
        try
        {
            var results = await new EarthDB.ObjectQuery(false)
               .GetBuildplate(templateId)
               .ExecuteAsync(_earthDB, cancellationToken);

            template = results.GetBuildplate(templateId);
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to get template buildplate: {ex}");
            return false;
        }

        if (template is not null)
        {
            _logger.Error("Template buidplate already exists");
            return false;
            /*_logger.Information("Template buildplate found, updating");

            _logger.Information("Storing template world");
            string? serverDataObjectId = (string?)await objectStoreClient.Store(worldData.ServerData).Task;
            if (serverDataObjectId is null)
            {
                _logger.Error("Could not store template data object in object store");
                return false;
            }

            _logger.Information("Storing template preview");
            string? previewObjectId = (string?)await objectStoreClient.Store(preview).Task;
            if (previewObjectId is null)
            {
                _logger.Error("Could not store template preview object in object store");
                return false;
            }

            _logger.Information("Updating template object ids");
            string oldDataObjectId = template.ServerDataObjectId;
            string oldPreviewObjectId = template.PreviewObjectId;

            template = template with
            {
                ServerDataObjectId = serverDataObjectId,
                PreviewObjectId = previewObjectId
            };

            try
            {
                var results = await new EarthDB.ObjectQuery(true)
                   .UpdateBuildplate(templateId, template)
                   .ExecuteAsync(earthDB, cancellationToken);
            }
            catch (EarthDB.DatabaseException ex)
            {
                _logger.Error($"Failed to update template buildplate: {ex}");
                return false;
            }

            _logger.Information("Deleting old template objects");
            await objectStoreClient.Delete(oldDataObjectId).Task;
            await objectStoreClient.Delete(oldPreviewObjectId).Task;*/
        }
        else
        {

            _logger.Information("Template buildplate not found");

            _logger.Information("Storing template world");
            string? serverDataObjectId = (string?)await _objectStoreClient.Store(worldData.ServerData).Task;
            if (serverDataObjectId is null)
            {
                _logger.Error("Could not store template data object in object store");
                return false;
            }

            _logger.Information("Storing template preview");
            string? previewObjectId = (string?)await _objectStoreClient.Store(preview).Task;
            if (previewObjectId is null)
            {
                _logger.Error("Could not store template preview object in object store");
                return false;
            }

            int scale = worldData.Size switch
            {
                8 => 14,
                16 => 33,
                32 => 64,
                _ => 33,
            };

            template = new TemplateBuildplate(name, worldData.Size, worldData.Offset, scale, worldData.Night, serverDataObjectId, previewObjectId);

            try
            {
                var results = await new EarthDB.ObjectQuery(true)
                   .UpdateBuildplate(templateId, template)
                   .ExecuteAsync(_earthDB, cancellationToken);
            }
            catch (EarthDB.DatabaseException ex)
            {
                _logger.Error($"Failed to store template buidplate in database: {ex}");
                await _objectStoreClient.Delete(serverDataObjectId).Task;
                await _objectStoreClient.Delete(previewObjectId).Task;
                return false;
            }
        }

        return true;
    }

    private async Task<bool> StoreBuildplate(string templateId, string playerId, string buildplateId, TemplateBuildplate template, byte[] serverData, byte[] preview, CancellationToken cancellationToken)
    {
        _logger.Information("Storing world");
        string? serverDataObjectId = (string?)await _objectStoreClient.Store(serverData).Task;
        if (serverDataObjectId is null)
        {
            _logger.Error("Could not store data object in object store");
            return false;
        }

        _logger.Information("Storing preview");
        string? previewObjectId = (string?)await _objectStoreClient.Store(preview).Task;
        if (previewObjectId is null)
        {
            _logger.Error("Could not store preview object in object store");
            await _objectStoreClient.Delete(serverDataObjectId).Task;
            return false;
        }

        try
        {
            EarthDB.Results results = await new EarthDB.Query(true)
                .Get("buildplates", playerId, typeof(Buildplates))
                .Then(results1 =>
                {
                    Buildplates buildplates = results1.Get<Buildplates>("buildplates");

                    long lastModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    var buildplate = new Buildplates.Buildplate(templateId, template.Name, template.Size, template.Offset, template.Scale, template.Night, lastModified, serverDataObjectId, previewObjectId);

                    buildplates.AddBuildplate(buildplateId, buildplate);

                    return new EarthDB.Query(true)
                        .Update("buildplates", playerId, buildplates);
                })
                .ExecuteAsync(_earthDB, cancellationToken);

            return true;
        }
        catch (EarthDB.DatabaseException ex)
        {
            _logger.Error($"Failed to store buildplate in database: {ex}");
            await _objectStoreClient.Delete(serverDataObjectId).Task;
            await _objectStoreClient.Delete(previewObjectId).Task;
            return false;
        }
    }
}