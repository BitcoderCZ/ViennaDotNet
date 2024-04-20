using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uma.Uuid;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.DB.Models.Player
{
    public sealed class Journal
    {
        private Dictionary<string, ItemJournalEntry> items;

        public Journal()
        {
            items = new();
        }

        public Journal copy()
        {
            Journal journal = new Journal();
            journal.items.AddRange(items);
            return journal;
        }

        public ItemJournalEntry? getItem(string uuid)
        {
            return items.GetOrDefault(uuid, null);
        }

        public void touchItem(string uuid, long timestamp)
        {
            // TODO: figure out amountCollected
            ItemJournalEntry? itemJournalEntry = items.GetOrDefault(uuid, null);

            if (itemJournalEntry == null)
                items[uuid] = new ItemJournalEntry(timestamp, timestamp, 0);
            else
                items[uuid] = new ItemJournalEntry(itemJournalEntry.firstSeen, timestamp, itemJournalEntry.amountCollected);
        }

        public record ItemJournalEntry(
            long firstSeen,
            long lastSeen,
            int amountCollected
        )
        {
        }
    }
}
