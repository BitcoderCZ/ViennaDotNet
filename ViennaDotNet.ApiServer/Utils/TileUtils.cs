using ViennaDotNet.TileRenderer;

namespace ViennaDotNet.ApiServer.Utils;

internal static class TileUtils
{
    public static async Task<bool> TryWriteTile(int tileX, int tileY, Stream dest)
    {
        // TODO: get tile object id from db if exists, otherwise send request to event bus
        return false;
    }
}
