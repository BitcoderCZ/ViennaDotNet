using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using ViennaDotNet.Common.Excceptions;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB.Models.Player;

namespace ViennaDotNet.DB;

public sealed class EarthDB : IDisposable
{
    private const int TRANSACTION_TIMEOUT = 60;

    public static EarthDB Open(string connectionString)
    {
        return new EarthDB(connectionString);
    }

    private string connectionString;
    private HashSet<SqliteTransaction> transactions = [];

    private readonly Lock _lock = new();

    private EarthDB(string _connectionString)
    {
        connectionString = _connectionString;

        try
        {
            using var connection = new SqliteConnection("Data Source=" + connectionString);
            connection.Open();
            using (var command = new SqliteCommand("CREATE TABLE IF NOT EXISTS objects (type STRING NOT NULL, id STRING NOT NULL, value STRING NOT NULL, version INTEGER NOT NULL, PRIMARY KEY (type, id))", connection) { CommandTimeout = TRANSACTION_TIMEOUT })
                command.ExecuteNonQuery();

        }
        catch (SqliteException ex)
        {
            throw new DatabaseException(ex);
        }
    }

    private SqliteTransaction transaction(bool write)
    {
        lock (_lock)
        {
            try
            {
                var connection = new SqliteConnection("Data Source=" + connectionString);
                connection.Open();
                var transaction = connection.BeginTransaction(/*!write*/false);// new Transaction(this, connection, write);
                transactions.Add(transaction);
                return transaction;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException(ex);
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var transaction in transactions)
            {
                try
                {
                    transaction.Dispose();
                }
                catch { }
            }
        }
    }

    public class Query
    {
        private bool _write;
        private List<WriteObjectsEntry> writeObjects = [];
        private List<ReadObjectsEntry> readObjects = [];
        private List<ExtrasEntry> extras = [];
        private List<Func<Results, Query>> thenFunctions = [];

        private sealed record WriteObjectsEntry(string type, string id, object value);

        private sealed record ReadObjectsEntry(string type, string id, Type valueType);

        private sealed record ExtrasEntry(string name, object value);

        public Query(bool write)
        {
            _write = write;
        }

        #region methods
        public Query Update(string type, string id, object value)
        {
            if (!_write)
                throw new UnsupportedOperationException();

            writeObjects.Add(new WriteObjectsEntry(type, id, value));
            return this;
        }

        public Query Get(string type, string id, Type valueType)
        {
            readObjects.Add(new ReadObjectsEntry(type, id, valueType));
            return this;
        }

        public Query Extra(string name, object value)
        {
            extras.Add(new ExtrasEntry(name, value));
            return this;
        }

        public Query Then(Func<Results, Query> function)
        {
            thenFunctions.Add(function);
            return this;
        }

        public Query Then(Query query)
        {
            thenFunctions.Add(results => query);
            return this;
        }
        #endregion

        public async Task<Results> ExecuteAsync(EarthDB earthDB, CancellationToken cancellationToken = default)
        {
            try
            {
                using SqliteTransaction transaction = earthDB.transaction(_write);
                Results results = await executeInternalAsync(transaction, _write, null, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                if (transaction.Connection is not null)
                {
                    await transaction.Connection.CloseAsync();
                }

                return results;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException(ex);
            }
        }

        public Results Execute(EarthDB earthDB)
        {
            try
            {
                using SqliteTransaction transaction = earthDB.transaction(_write);
                Results results = executeInternal(transaction, _write, null);
                transaction.Commit();
                transaction.Connection?.Close();
                return results;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException(ex);
            }
        }

        private async Task<Results> executeInternalAsync(SqliteTransaction transaction, bool write, Dictionary<string, int?>? parentUpdates, CancellationToken cancellationToken)
        {
            if (_write && !write)
            {
                throw new UnsupportedOperationException();
            }

            Results results = new Results();
            if (parentUpdates != null)
            {
                results.updates.AddRange(parentUpdates);
            }

            foreach (WriteObjectsEntry entry in writeObjects)
            {
                string json = JsonConvert.SerializeObject(entry.value);

                var command = transaction.Connection!.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "INSERT OR REPLACE INTO objects(type, id, value, version) VALUES ($type, $id, $value, COALESCE((SELECT version FROM objects WHERE type == $type AND id == $id), 1) + 1)";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                command.Parameters.AddWithValue("$value", json);
                await command.ExecuteNonQueryAsync(cancellationToken);

                /*****************************/
                command = transaction.Connection.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "SELECT version FROM objects WHERE type == $type AND id == $id";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        int version = reader.GetInt32(0);
                        results.updates[entry.type] = version;
                    }
                    else
                    {
                        throw new DatabaseException("Could not query updated object");
                    }
                }
            }

            foreach (ReadObjectsEntry entry in readObjects)
            {
                var command = transaction.Connection!.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "SELECT value, version FROM objects WHERE type == $type AND id == $id";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        string json = reader.GetString(0);
                        int version = reader.GetInt32(1);
                        object value = JsonConvert.DeserializeObject(json, entry.valueType,
                            new Tokens.TokenConverter(),
                            new ActivityLog.Entry.EntryConverter()
                        )!;
                        results.getValues[entry.type] = new Results.Result(value, version);
                    }
                    else
                    {
                        try
                        {
                            object value = Activator.CreateInstance(entry.valueType)!;
                            results.getValues[entry.type] = new Results.Result(value, 1);
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException(ex);
                        }
                    }
                }
            }

            foreach (ExtrasEntry entry in extras)
                results.extras[entry.name] = entry.value;

            foreach (Func<Results, Query> function in thenFunctions)
            {
                Query query = function.Invoke(results);
                results = await query.executeInternalAsync(transaction, write, results.updates, cancellationToken);
            }

            return results;
        }

        private Results executeInternal(SqliteTransaction transaction, bool write, Dictionary<string, int?>? parentUpdates)
        {
            if (_write && !write)
                throw new UnsupportedOperationException();

            Results results = new Results();
            if (parentUpdates != null)
                results.updates.AddRange(parentUpdates);

            foreach (WriteObjectsEntry entry in writeObjects)
            {
                string json = JsonConvert.SerializeObject(entry.value);

                var command = transaction.Connection!.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "INSERT OR REPLACE INTO objects(type, id, value, version) VALUES ($type, $id, $value, COALESCE((SELECT version FROM objects WHERE type == $type AND id == $id), 1) + 1)";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                command.Parameters.AddWithValue("$value", json);
                command.ExecuteNonQuery();

                /*****************************/
                command = transaction.Connection.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "SELECT version FROM objects WHERE type == $type AND id == $id";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int version = reader.GetInt32(0);
                        results.updates[entry.type] = version;
                    }
                    else
                        throw new DatabaseException("Could not query updated object");
                }
            }

            foreach (ReadObjectsEntry entry in readObjects)
            {
                var command = transaction.Connection!.CreateCommand();
                command.CommandTimeout = TRANSACTION_TIMEOUT;
                command.CommandText = "SELECT value, version FROM objects WHERE type == $type AND id == $id";

                command.Parameters.AddWithValue("$type", entry.type);
                command.Parameters.AddWithValue("$id", entry.id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string json = reader.GetString(0);
                        int version = reader.GetInt32(1);
                        object value = JsonConvert.DeserializeObject(json, entry.valueType,
                            new Tokens.TokenConverter(),
                            new ActivityLog.Entry.EntryConverter()
                        )!;
                        results.getValues[entry.type] = new Results.Result(value, version);
                    }
                    else
                    {
                        try
                        {
                            object value = Activator.CreateInstance(entry.valueType)!;
                            results.getValues[entry.type] = new Results.Result(value, 1);
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException(ex);
                        }
                    }
                }
            }

            foreach (ExtrasEntry entry in extras)
                results.extras[entry.name] = entry.value;

            foreach (Func<Results, Query> function in thenFunctions)
            {
                Query query = function.Invoke(results);
                results = query.executeInternal(transaction, write, results.updates);
            }

            return results;
        }
    }

    public class Results
    {
        public Dictionary<string, Result> getValues = [];
        public Dictionary<string, object> extras = [];
        public Dictionary<string, int?> updates = [];

        public Results()
        {
            // empty
        }

        public Result Get(string name)
            => !getValues.TryGetValue(name, out Result? value) || value is null
            ? throw new KeyNotFoundException()
            : value;

        public GenericResult<T> GetGeneric<T>(string name)
            => !getValues.TryGetValue(name, out Result? value) || value is null
                ? throw new KeyNotFoundException()
                : new GenericResult<T>((T)value.Value, value.version);

        public Dictionary<string, int?> getUpdates()
            => new Dictionary<string, int?>(updates);

        public object getExtra(string name)
            => !extras.TryGetValue(name, out object? value) || value is null
            ? throw new KeyNotFoundException()
            : value;

        public record Result(object Value, int version);

        public record GenericResult<T>(T GValue, int version)
            : Result(GValue!, version);
    }

    public class DatabaseException : Exception
    {
        public DatabaseException() { }
        public DatabaseException(string message) : base(message) { }
        public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
        public DatabaseException(Exception innerException) : base("Database operation failed.", innerException) { }
    }
}
