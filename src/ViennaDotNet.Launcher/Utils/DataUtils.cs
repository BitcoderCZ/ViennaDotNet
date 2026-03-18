using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Player;

namespace ViennaDotNet.Launcher.Utils;

internal static class DataUtils
{
    public static SqliteConnection? OpenLiveDB(Settings settings)
    {
        try
        {
            var connection = new SqliteConnection("Data Source=" + settings.LiveDatabaseConnectionString);
            connection.Open();
            return connection;
        }
        catch
        {
            return null;
        }
    }

    public static long? GetPlayerCount(EarthDB db)
    {
        long? playerCount = null;
        try
        {
            db.ExecuteCommand(false, command =>
            {
                command.CommandText = $"""
                    SELECT COUNT(DISTINCT id) FROM {EarthDB.ObjectsTable};
                    """;

                playerCount = command.ExecuteScalar() as long?;
            });
        }
        catch
        {
        }

        return playerCount;
    }

    public static async Task<long?> GetPlayerCountAsync(EarthDB db, CancellationToken cancellationToken = default)
    {
        long? playerCount = null;
        try
        {
            await db.ExecuteCommandAsync(false, command =>
            {
                command.CommandText = $"""
                    SELECT COUNT(DISTINCT id) FROM {EarthDB.ObjectsTable};
                    """;

                playerCount = command.ExecuteScalar() as long?;
            }, cancellationToken);
        }
        catch
        {
        }

        return playerCount;
    }

    public static async IAsyncEnumerable<string> GetPlayersAsync(EarthDB db, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = db.OpenConnection(false);

        using (var command = new SqliteCommand($"""
            SELECT DISTINCT id FROM {EarthDB.ObjectsTable};
            """, connection))
        {
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (reader.Read())
                {
                    yield return reader.GetString(0);
                }
            }
        }
    }

    public static async IAsyncEnumerable<(string Id, Profile Profile)> GetAllProfilesAsync(EarthDB db, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = db.OpenConnection(false);

        HashSet<string> returnedPlayers = [];

        using (var command = new SqliteCommand($"""
            SELECT id, value FROM {EarthDB.ObjectsTable} WHERE type = 'profile';
            """, connection))
        {
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    string id = reader.GetString(0);
                    var profile = EarthDB.FromJson<Profile>(reader.GetString(1));
                    Debug.Assert(profile is not null);

                    returnedPlayers.Add(id);
                    yield return (id, profile);
                }
            }
        }

        await foreach (string playerId in GetPlayersAsync(db, cancellationToken))
        {
            if (!returnedPlayers.Contains(playerId))
            {
                yield return (playerId, new Profile());
            }
        }
    }

    public static async Task<string?> GetUsername(string userId, SqliteConnection liveConnection, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var command = new SqliteCommand($"""
                SELECT Username FROM Accounts WHERE Id = @id;
                """, liveConnection))
            {
                command.Parameters.AddWithValue("@id", userId);

                return await command.ExecuteScalarAsync(cancellationToken) as string;
            }
        }
        catch
        {
            return null;
        }
    }

    public static IAsyncEnumerable<(string Id, string? Username, Profile Profile)> GetFullProfilesAsync(EarthDB db, SqliteConnection? liveConnection, CancellationToken cancellationToken = default)
    {
        if (liveConnection is null)
        {
            return GetAllProfilesAsync(db, cancellationToken)
                .Select(item => (item.Id, (string?)null, item.Profile));
        }

        return GetAllProfilesAsync(db, cancellationToken)
            .SelectAwait(async item => (item.Id, await GetUsername(item.Id, liveConnection, cancellationToken), item.Profile));
    }
}
