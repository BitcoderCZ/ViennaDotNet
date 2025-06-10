using Newtonsoft.Json;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.DB.Models.Player;

[JsonObject(MemberSerialization.OptIn)]
public sealed class Journal
{
    [JsonProperty]
    private Dictionary<string, ItemJournalEntry> items;

    public Journal()
    {
        items = [];
    }

    public Journal copy()
    {
        Journal journal = new Journal();
        journal.items.AddRange(items);
        return journal;
    }

    public Dictionary<string, ItemJournalEntry> getItems()
        => new(items);

    public ItemJournalEntry? getItem(string uuid)
        => items.GetValueOrDefault(uuid);

    public ItemJournalEntry touchItem(string uuid, long timestamp)
    {
        ItemJournalEntry? itemJournalEntry = items.GetOrDefault(uuid, null);

        if (itemJournalEntry == null)
        {
            return items[uuid] = new ItemJournalEntry(timestamp, timestamp, 0);
        }
        else
        {
            return items[uuid] = new ItemJournalEntry(itemJournalEntry.firstSeen, timestamp, itemJournalEntry.amountCollected);
        }
    }

    public void addCollectedItem(string uuid, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        ItemJournalEntry? itemJournalEntry = items.GetOrDefault(uuid, null);
        if (itemJournalEntry is null)
        {
            throw new InvalidOperationException("Item does not exist in journal, make sure to touch it or otherwise verify that it exists before calling addCollectedItem");
        }

        items[uuid] = new ItemJournalEntry(itemJournalEntry.firstSeen, itemJournalEntry.lastSeen, itemJournalEntry.amountCollected + count);
    }

    public record ItemJournalEntry(
        long firstSeen,
        long lastSeen,
        int amountCollected
    )
    {
    }
}
