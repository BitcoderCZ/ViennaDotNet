using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.IO.Pipelines;
using System.Xml.Linq;

namespace ViennaDotNet.StaticData;

public sealed class Catalog
{
    public readonly ItemsCatalog itemsCatalog;
    public readonly ItemEfficiencyCategoriesCatalog itemEfficiencyCategoriesCatalog;
    public readonly ItemJournalGroupsCatalog itemJournalGroupsCatalog;
    public readonly RecipesCatalog recipesCatalog;
    public readonly NFCBoostsCatalog nfcBoostsCatalog;

    public Catalog(string dir)
    {
        try
        {
            itemsCatalog = new ItemsCatalog(Path.Combine(dir, "items.json"));
            itemEfficiencyCategoriesCatalog = new ItemEfficiencyCategoriesCatalog(Path.Combine(dir, "itemEfficiencyCategories.json"));
            itemJournalGroupsCatalog = new ItemJournalGroupsCatalog(Path.Combine(dir, "itemJournalGroups.json"));
            recipesCatalog = new RecipesCatalog(Path.Combine(dir, "recipes.json"));
            nfcBoostsCatalog = new NFCBoostsCatalog(Path.Combine(dir, "nfc.json"));
        }
        catch (StaticDataException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new StaticDataException(null, exception);
        }
    }

    public sealed class ItemsCatalog
    {
        public readonly Item[] items;

        private readonly Dictionary<string, Item> itemsById = [];

        internal ItemsCatalog(string file)
        {
            items = JsonConvert.DeserializeObject<Item[]>(File.ReadAllText(file));

            HashSet<String> ids = [];
            HashSet<String> names = [];
            foreach (Item item in items)
            {
                if (!ids.Add(item.id))
                {
                    throw new StaticDataException($"Duplicate item ID {item.id}");
                }
                if (!names.Add(item.name + "." + item.aux))
                {
                    throw new StaticDataException($"Duplicate item name/aux {item.name} {item.aux}");
                }
            }

            foreach (Item item in items)
            {
                itemsById[item.id] = item;
            }
        }

        public Item? getItem(string id)
        {
            return this.itemsById.GetValueOrDefault(id);
        }

        public record Item(
            string id,
            string name,
            int aux,
            bool stackable,
            Item.Type type,
            Item.Category category,
            Item.Rarity rarity,
            Item.UseType useType,
            Item.UseType alternativeUseType,
            Item.BlockInfo? blockInfo,
            Item.ToolInfo? toolInfo,
            Item.ConsumeInfo? consumeInfo,
            Item.FuelInfo? fuelInfo,
            Item.ProjectileInfo? projectileInfo,
            Item.MobInfo? mobInfo,
            Item.BoostInfo? boostInfo,
            Item.JournalEntry? journalEntry,
            Item.Experience experience
        )
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type
            {
                BLOCK,
                ITEM,
                TOOL,
                MOB,
                ENVIRONMENT_BLOCK,
                BOOST,
                ADVENTURE_SCROLL
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Category
            {
                CONSTRUCTION,
                EQUIPMENT,
                ITEMS,
                MOBS,
                NATURE,
                BOOST_ADVENTURE_XP,
                BOOST_CRAFTING,
                BOOST_DEFENSE,
                BOOST_EATING,
                BOOST_HEALTH,
                BOOST_HOARDING,
                BOOST_ITEM_XP,
                BOOST_MINING_SPEED,
                BOOST_RETENTION,
                BOOST_SMELTING,
                BOOST_STRENGTH,
                BOOST_TAPPABLE_RADIUS
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Rarity
            {
                COMMON,
                UNCOMMON,
                RARE,
                EPIC,
                LEGENDARY,
                OOBE
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum UseType
            {
                NONE,
                BUILD,
                BUILD_ATTACK,
                INTERACT,
                INTERACT_AND_BUILD,
                DESTROY,
                USE,
                CONSUME
            }

            public sealed record BlockInfo(
                int breakingHealth,
                string? efficiencyCategory
            );

            public sealed record ToolInfo(
                int blockDamage,
                int mobDamage,
                int maxWear,
                string? efficiencyCategory
            );

            public sealed record ConsumeInfo(
                int heal,
                string? returnItemId
            );

            public sealed record FuelInfo(
                int burnTime,
                int heatPerSecond,
                string? returnItemId
            );

            public sealed record ProjectileInfo(
                int mobDamage
            );

            public sealed record MobInfo(
                int health
            );

            public sealed record BoostInfo(
                string name,
                int? level,
                BoostInfo.Type type,
                bool canBeRemoved,
                int? duration,
                bool triggeredOnDeath,
                BoostInfo.Effect[] effects
            )
            {
                [JsonConverter(typeof(StringEnumConverter))]
                public enum Type
                {
                    POTION,
                    INVENTORY_ITEM
                }

                public record Effect(
                    Effect.Type type,
                    int value,
                    string[] applicableItemIds,
                    Effect.Activation activation,
                    int duration
                )
                {
                    [JsonConverter(typeof(StringEnumConverter))]
                    public enum Type
                    {
                        ADVENTURE_XP,
                        CRAFTING,
                        DEFENSE,
                        EATING,
                        HEALING,
                        HEALTH,
                        ITEM_XP,
                        MINING_SPEED,
                        RETENTION_BACKPACK,
                        RETENTION_HOTBAR,
                        RETENTION_XP,
                        SMELTING,
                        STRENGTH,
                        TAPPABLE_RADIUS
                    }

                    [JsonConverter(typeof(StringEnumConverter))]
                    public enum Activation
                    {
                        INSTANT,
                        TIMED,
                        TRIGGERED
                    }
                }
            }

            public sealed record JournalEntry(
                string group,
                int order,
                JournalEntry.Biome biome,
                JournalEntry.Behavior behavior,
                string? sound
            )
            {
                [JsonConverter(typeof(StringEnumConverter))]
                public enum Biome
                {
                    NONE,
                    OVERWORLD,
                    NETHER,
                    BIRCH_FOREST,
                    DESERT,
                    FLOWER_FOREST,
                    FOREST,
                    ICE_PLAINS,
                    JUNGLE,
                    MESA,
                    MUSHROOM_ISLAND,
                    OCEAN,
                    PLAINS,
                    RIVER,
                    ROOFED_FOREST,
                    SAVANNA,
                    SUNFLOWER_PLAINS,
                    SWAMP,
                    TAIGA,
                    WARM_OCEAN
                }

                [JsonConverter(typeof(StringEnumConverter))]
                public enum Behavior
                {
                    NONE,
                    PASSIVE,
                    HOSTILE,
                    NEUTRAL
                }
            }

            public sealed record Experience(
                int tappable,
                int encounter,
                int crafting,
                int journal    // TODO: what is this used for?
            )
            {
            }
        }
    }

    public sealed class ItemEfficiencyCategoriesCatalog
    {
        public readonly EfficiencyCategory[] efficiencyCategories;

        internal ItemEfficiencyCategoriesCatalog(string file)
        {
            efficiencyCategories = JsonConvert.DeserializeObject<EfficiencyCategory[]>(File.ReadAllText(file));

            HashSet<String> names = [];
            foreach (EfficiencyCategory efficiencyCategory in efficiencyCategories)
            {
                if (!names.Add(efficiencyCategory.name))
                {
                    throw new StaticDataException($"Duplicate efficiency category name {efficiencyCategory.name}");
                }
            }
        }

        public sealed record EfficiencyCategory(
            string name,
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
        );
    }

    public sealed class ItemJournalGroupsCatalog
    {
        public readonly JournalGroup[] groups;

        internal ItemJournalGroupsCatalog(string file)
        {
            groups = JsonConvert.DeserializeObject<JournalGroup[]>(File.ReadAllText(file));

            HashSet<String> ids = [];
            HashSet<String> names = [];
            foreach (JournalGroup journalGroup in groups)
            {
                if (!ids.Add(journalGroup.id))
                {
                    throw new StaticDataException($"Duplicate journal group ID {journalGroup.id}");
                }
                if (!names.Add(journalGroup.name))
                {
                    throw new StaticDataException($"Duplicate journal group name {journalGroup.name}");
                }
            }
        }

        public record JournalGroup(
            string id,
            string name,
            JournalGroup.ParentCollection parentCollection,
            int order,
            string? defaultSound
        )
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum ParentCollection
            {
                BLOCKS,
                ITEMS_CRAFTED,
                ITEMS_SMELTED,
                MOBS
            }
        }
    }

    public sealed class RecipesCatalog
    {
        public readonly CraftingRecipe[] crafting;
        public readonly SmeltingRecipe[] smelting;

        private readonly Dictionary<string, CraftingRecipe> craftingRecipesById = [];
        private readonly Dictionary<string, SmeltingRecipe> smeltingRecipesById = [];

        private sealed record RecipesCatalogFile(
            CraftingRecipe[] crafting,
            SmeltingRecipe[] smelting
        );

        internal RecipesCatalog(string file)
        {
            RecipesCatalogFile recipesCatalogFile = JsonConvert.DeserializeObject<RecipesCatalogFile>(File.ReadAllText(file));
            crafting = recipesCatalogFile.crafting;
            smelting = recipesCatalogFile.smelting;

            HashSet<String> craftingIds = [];
            HashSet<String> smeltingIds = [];
            foreach (CraftingRecipe craftingRecipe in crafting)
            {
                if (!craftingIds.Add(craftingRecipe.id))
                {
                    throw new StaticDataException($"Duplicate crafting recipe ID {craftingRecipe.id}");
                }
            }

            foreach (SmeltingRecipe smeltingRecipe in smelting)
            {
                if (!smeltingIds.Add(smeltingRecipe.id))
                {
                    throw new StaticDataException($"Duplicate smelting recipe ID {smeltingRecipe.id}");
                }
            }

            foreach (CraftingRecipe craftingRecipe in crafting)
            {
                craftingRecipesById[craftingRecipe.id] = craftingRecipe;
            }

            foreach (SmeltingRecipe smeltingRecipe in smelting)
            {
                smeltingRecipesById[smeltingRecipe.id] = smeltingRecipe;
            }
        }

        public CraftingRecipe? getCraftingRecipe(string id)
        {
            return this.craftingRecipesById.GetValueOrDefault(id);
        }

        public SmeltingRecipe? getSmeltingRecipe(string id)
        {
            return this.smeltingRecipesById.GetValueOrDefault(id);
        }

        public sealed record CraftingRecipe(
            string id,
            int duration,
            CraftingRecipe.Category category,
            CraftingRecipe.Ingredient[] ingredients,
            CraftingRecipe.Output output,
            CraftingRecipe.ReturnItem[] returnItems
        )
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Category
            {
                CONSTRUCTION,
                EQUIPMENT,
                ITEMS,
                NATURE
            }

            public sealed record Ingredient(
                int count,
                string[] possibleItemIds
            );

            public record Output(
                string itemId,
                int count
            );

            public record ReturnItem(
                string itemId,
                int count
            );
        }

        public sealed record SmeltingRecipe(
                string id,
                int heatRequired,
                string input,
                string output,
                string returnItemId
        );
    }

    public sealed class NFCBoostsCatalog
    {
        record NFCBoostsCatalogFile(
        // TODO
        );

        internal NFCBoostsCatalog(string file)
        {
            NFCBoostsCatalogFile nfcBoostsCatalogFile = JsonConvert.DeserializeObject<NFCBoostsCatalogFile>(File.ReadAllText(file));

            // TODO
        }
    }
}
