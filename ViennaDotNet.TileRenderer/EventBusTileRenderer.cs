using Npgsql;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB;
using ViennaDotNet.EventBus.Client;
using ViennaDotNet.ObjectStore.Client;
using ViennaDotNet.StaticData;

namespace ViennaDotNet.TileRenderer;

internal sealed class EventBusTileRenderer : IDisposable
{
    private readonly EarthDB _earthDB;
    private readonly NpgsqlDataSource _tileDB;
    private readonly EventBusClient _eventBus;
    private readonly ObjectStoreClient _objectStore;
    private readonly TileRenderer _renderer;
    private readonly Publisher _publisher;

    private readonly ConcurrentQueue<RenderTileRequest> _renderRequests = [];
    private readonly ConcurrentQueue<RenderedTile> _renderedTiles = [];
    private readonly SemaphoreSlim _concurrencyLimiter = new(int.Max(Environment.ProcessorCount / 2, 1));
    private readonly CancellationTokenSource _cts = new();

    public EventBusTileRenderer(EarthDB earthDB, NpgsqlDataSource tileDB, EventBusClient eventBus, ObjectStoreClient objectStore, StaticData.StaticData staticData)
    {
        _earthDB = earthDB;
        _tileDB = tileDB;
        _eventBus = eventBus;
        _objectStore = objectStore;
        _renderer = TileRenderer.Create(staticData.tileRenderer.TagMapJson, Log.Logger);
        _publisher = _eventBus.addPublisher();
    }

    public async Task Run()
    {
        Task.Run(() => DispatcherLoopAsync(_cts.Token)).Forget();

        _eventBus.addRequestHandler("tile", new RequestHandler.Handler(request =>
        {
            if (request.type == "renderTile")
            {
                RenderTileRequest getTile;
                try
                {
                    getTile = JsonSerializer.Deserialize<RenderTileRequest>(request.data)!;
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not deserialise renderTile request: {ex}");
                    return null;
                }

                _renderRequests.Enqueue(getTile);

                return string.Empty;
            }
            else
            {
                return null;
            }
        }, () =>
        {
            Log.Error("Event bus subscriber error");
            Dispose();
            Log.CloseAndFlush();
            Environment.Exit(1);
        }));

        while (true)
        {
            if (!_renderedTiles.TryDequeue(out var tile))
            {
                Thread.Sleep(1);
            }

            string? tileObjectId = await _objectStore.store(tile.Data).Task as string;
            if (tileObjectId is null)
            {
                Log.Error($"Could not store new data object for tile ({tile.TileX}, {tile.TileY}, {tile.Zoom}) in object store");
                // TODO: respond with error
                throw new NotImplementedException();
            }

            // TODO: store tileX,tileY -> id to db (tileX | tileY << ?)

            _publisher.publish("tile", "tileRendered", JsonSerializer.Serialize(new RenderTileResponse(tile.TileX, tile.TileY, tile.Zoom, tileObjectId))).Forget();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _publisher.flush();
        _publisher.close();
        _eventBus.close();
    }

    private async Task DispatcherLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_renderRequests.TryDequeue(out var request))
            {
                await _concurrencyLimiter.WaitAsync(token);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleRenderRequestAsync(request, token);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Render task failed: {ex}");
                    }
                    finally
                    {
                        _concurrencyLimiter.Release();
                    }
                }, token);
            }
            else
            {
                await Task.Delay(50, token);
            }
        }
    }

    private async Task HandleRenderRequestAsync(RenderTileRequest request, CancellationToken cancellationToken)
    {
        using (var bitmap = new SKBitmap(128, 128))
        using (var canvas = new SKCanvas(bitmap))
        {
            await _renderer.RenderAsync(_tileDB, canvas, request.TileX, request.TileY, request.Zoom, Log.Logger, cancellationToken);

            // TODO: higher/lower quality?
            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 80))
            using (var stream = new MemoryStream())
            {
                data.SaveTo(stream);

                _renderedTiles.Enqueue(new RenderedTile(request.TileX, request.TileY, request.Zoom, stream.ToArray()));
            }
        }
    }

    private record struct RenderedTile(int TileX, int TileY, int Zoom, byte[] Data);
}
