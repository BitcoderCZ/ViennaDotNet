using static ViennaDotNet.ApiServer.Types.Catalog.ItemsCatalog;
using static ViennaDotNet.ApiServer.Types.Catalog.ItemsCatalog.Item;
using static ViennaDotNet.ApiServer.Types.Catalog.ItemsCatalog.Item.ItemData;
using static ViennaDotNet.ApiServer.Types.Catalog.ItemsCatalog.EfficiencyCategory;
using System.Collections;
using ViennaDotNet.ApiServer.Types.Common;

namespace ViennaDotNet.ApiServer.Types.Catalog
{
    public record ItemsCatalog(
        Item[] items,
        Dictionary<string, EfficiencyCategory> efficiencyCategories
    )
    {
        public record Item(
            string id,
            ItemData item,
            string category,
            Rarity rarity,
            int fragmentsRequired,
            bool stacks,
            BurnRate? burnRate,
            ReturnItem[] fuelReturnItems,
            ReturnItem[] consumeReturnItems,
            int? experience,
            Dictionary<string, int?> experiencePoints,
            bool deprecated
        )
        {
            public record ItemData(
                string name,
                int? aux,
                string type,
                string useType,
                int? tapSpeed,
                int? heal,
                int? nutrition,
                int? mobDamage,
                int? blockDamage,
                int? health,
                BlockMetadata? blockMetadata,
                ItemMetadata? itemMetadata,
                BoostMetadata? boostMetadata,
                JournalMetadata? journalMetadata,
                AudioMetadata? audioMetadata,
                IDictionary clientProperties
            )
            {
                public record BlockMetadata(
                    int? health,
                    string? efficiencyCategory
                )
                {
                }

                public record ItemMetadata(
                    string useType,
                    string alternativeUseType,
                    int? mobDamage,
                    int? blockDamage,
                    int? weakDamage,
                    int? nutrition,
                    int? heal,
                    string? efficiencyType,
                    int? maxHealth
                )
                {
                }

                public record JournalMetadata(
                    string groupKey,
                    int experience,
                    int order,
                    string behavior,
                    string biome
                )
                {
                }

                public record AudioMetadata(
                    Dictionary<string, string> sounds,
                    string defaultSound
                )
                {
                }
            }

            public record ReturnItem(
                string id,
                int amount
            )
            {
            }
        }

        public record EfficiencyCategory(
            EfficiencyMap efficiencyMap
        )
        {
            public record EfficiencyMap(
                float hand,
                float hoe,
                float axe,
                float shovel,
                float pickaxe_1,
                float pickaxe_2,
                float pickaxe_3,
                float pickaxe_4,
                float pickaxe_5,
                float sword,
                float sheers
            )
            {
            }
        }
    }
}
