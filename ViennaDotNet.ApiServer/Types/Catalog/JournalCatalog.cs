using System.Security.Cryptography.Xml;
using static ViennaDotNet.ApiServer.Types.Catalog.JournalCatalog;

namespace ViennaDotNet.ApiServer.Types.Catalog
{
    public record JournalCatalog(
        Dictionary<string, Item> items
    )
    {
        public record Item(
            string referenceId,
            string parentCollection,
            int overallOrder,
            int collectionOrder,
            string? defaultSound,
            bool deprecated,
            string toolsVersion
        )
        {
        }
    }
}
