namespace ViennaDotNet.Launcher;

internal interface IDataProvider<T>
{
    int? Count { get; }

    IEnumerable<T> GetData(int skip, int count);

    IAsyncEnumerable<T> GetDataAsync(int skip, int count, CancellationToken cancellationToken = default);
}

internal sealed class CollectionDataProvider<T> : IDataProvider<T>
{
    private readonly ICollection<T> _collection;

    public CollectionDataProvider(ICollection<T> collection)
    {
        _collection = collection;
    }

    public int? Count => _collection.Count;

    public IEnumerable<T> GetData(int skip, int count)
        => _collection.Skip(skip).Take(count);

    public IAsyncEnumerable<T> GetDataAsync(int skip, int count, CancellationToken cancellationToken = default)
        => GetData(skip, count).ToAsyncEnumerable();
}