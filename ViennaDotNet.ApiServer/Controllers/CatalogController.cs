using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using ViennaDotNet.ApiServer.Utils;
using ViennaDotNet.StaticData;
using CICICategory = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.Category;
using CICIType = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.Type;
using CICIUseType = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.UseType;
using CICIBIType = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.BoostInfo.Type;
using CICIBIEType = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type;
using CICIBIEActivation = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.BoostInfo.Effect.Activation;
using CICIJEBehavior = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.JournalEntry.Behavior;
using CICIJEBiome = ViennaDotNet.StaticData.Catalog.ItemsCatalog.Item.JournalEntry.Biome;

using ItemsCatalog = ViennaDotNet.ApiServer.Types.Catalog.ItemsCatalog;

namespace ViennaDotNet.ApiServer.Controllers;

[Authorize]
[ApiVersion("1.1")]
[Route("1/api/v{version:apiVersion}")]
public class CatalogController : ControllerBase
{
    private static Catalog catalog => Program.staticData.catalog;

    [HttpGet("inventory/catalogv3")]
    public IActionResult GetItemsCatalog()
    {
        return Content(JsonConvert.SerializeObject(new EarthApiResponse(catalog.itemsCatalog)), "application/json");
    }

    [HttpGet("recipes")]
    public IActionResult GetRecipeCatalog()
    {
        return Content(JsonConvert.SerializeObject(new EarthApiResponse(catalog.recipesCatalog)), "application/json");
    }

    [HttpGet("journal/catalog")]
    public IActionResult GetJournalCatalog()
    {
        return Content(JsonConvert.SerializeObject(new EarthApiResponse(catalog.journalCatalog)), "application/json");
    }

    [HttpGet("products/catalog")]
    public IActionResult GetNFCBoostsCatalog()
    {
        return Content(JsonConvert.SerializeObject(new EarthApiResponse(catalog.nfcBoostsCatalog)), "application/json");
    }

    // TODO: cache these?
    private static Types.Catalog.ItemsCatalog makeItemsCatalogApiResponse(Catalog catalog)
    {
        ItemsCatalog.Item[] items = [.. catalog.itemsCatalog.items.Select(item =>
        {
            string categoryString = item.category switch
            {
                CICICategory.CONSTRUCTION => "Construction",
                CICICategory.EQUIPMENT => "Equipment",
                CICICategory.ITEMS => "Items",
                CICICategory.MOBS => "Mobs",
                CICICategory.NATURE => "Nature",
                CICICategory.BOOST_ADVENTURE_XP => "adventurexp",
                CICICategory.BOOST_CRAFTING => "crafting",
                CICICategory.BOOST_DEFENSE => "defense",
                CICICategory.BOOST_EATING => "eating",
                CICICategory.BOOST_HEALTH => "maxplayerhealth",
                CICICategory.BOOST_HOARDING => "hoarding",
                CICICategory.BOOST_ITEM_XP => "itemxp",
                CICICategory.BOOST_MINING_SPEED => "miningspeed",
                CICICategory.BOOST_RETENTION => "retention",
                CICICategory.BOOST_SMELTING => "smelting",
                CICICategory.BOOST_STRENGTH => "strength",
                CICICategory.BOOST_TAPPABLE_RADIUS => "tappableRadius",
                _ => throw new UnreachableException(),
            };

            string typeString = item.type switch
            {
                CICIType.BLOCK => "Block",
                CICIType.ITEM => "Item",
                CICIType.TOOL => "Tool",
                CICIType.MOB => "Mob",
                CICIType.ENVIRONMENT_BLOCK => "EnvironmentBlock",
                CICIType.BOOST => "Boost",
                CICIType.ADVENTURE_SCROLL => "AdventureScroll",
                _ => throw new UnreachableException(),
            };

            String useTypeString = item.useType switch
            {
                CICIUseType.NONE => "None",

                CICIUseType.BUILD => "Build",
                CICIUseType.BUILD_ATTACK => "BuildAttack",
                CICIUseType.INTERACT => "Interact",
                CICIUseType.INTERACT_AND_BUILD => "InteractAndBuild",
                CICIUseType.DESTROY => "Destroy",
                CICIUseType.USE => "Use",
                CICIUseType.CONSUME => "Consume",
            }
    ;
            string alternativeUseTypeString = item.alternativeUseType switch
            {
                CICIUseType.NONE => "None",

                CICIUseType.BUILD => "Build",
                CICIUseType.BUILD_ATTACK => "BuildAttack",
                CICIUseType.INTERACT => "Interact",
                CICIUseType.INTERACT_AND_BUILD => "InteractAndBuild",
                CICIUseType.DESTROY => "Destroy",
                CICIUseType.USE => "Use",
                CICIUseType.CONSUME => "Consume",
            }
    ;

            int health;
            if (item.blockInfo is not null)
            {
                health = item.blockInfo.breakingHealth;
            }
            else if (item.toolInfo is not null)
            {
                health = item.toolInfo.maxWear;
            }
            else if (item.mobInfo is not null)
            {
                health = item.mobInfo.health;
            }
            else
            {
                health = 0;
            }

            int blockDamage;
            if (item.toolInfo is not null)
            {
                blockDamage = item.toolInfo.blockDamage;
            }
            else
            {
                blockDamage = 0;
            }

            int mobDamage;
            if (item.toolInfo is not null)
            {
                mobDamage = item.toolInfo.mobDamage;
            }
            else if (item.projectileInfo is not null)
            {
                mobDamage = item.projectileInfo.mobDamage;
            }
            else
            {
                mobDamage = 0;
            }

            Types.Catalog.BoostMetadata? boostMetadata;
            if (item.boostInfo is not null)
            {
                string boostTypeString = item.boostInfo.type switch
                {
                    CICIBIType.POTION => "Potion",
                    CICIBIType.INVENTORY_ITEM => "InventoryItem",
                    _ => throw new UnreachableException(),
                };

                String boostAttributeString = item.boostInfo.effects[0].type switch
                {
                    CICIBIEType.ADVENTURE_XP => "ItemExperiencePoints",
                    CICIBIEType.CRAFTING => "Crafting",
                    CICIBIEType.DEFENSE => "Defense",
                    CICIBIEType.EATING => "Eating",
                    CICIBIEType.HEALING => "Healing",
                    CICIBIEType.HEALTH => "MaximumPlayerHealth",
                    CICIBIEType.ITEM_XP => "ItemExperiencePoints",
                    CICIBIEType.MINING_SPEED => "MiningSpeed",
                    CICIBIEType.RETENTION_BACKPACK or CICIBIEType.RETENTION_HOTBAR or CICIBIEType.RETENTION_XP => "Retention",
                    CICIBIEType.SMELTING => "Smelting",
                    CICIBIEType.STRENGTH => "Strength",
                    CICIBIEType.TAPPABLE_RADIUS => "TappableInteractionRadius",
                    _ => throw new UnreachableException(),
                };

                boostMetadata = new Types.Catalog.BoostMetadata(
                    item.boostInfo.name,
                    boostTypeString,
                    boostAttributeString,
                    false,
                    item.boostInfo.canBeRemoved,
                    item.boostInfo.duration is not null ? TimeFormatter.FormatDuration(item.boostInfo.duration.Value) : null,
                    true,
                    item.boostInfo.level,
                    item.boostInfo.effects.Select(effect =>
                    {
                        string effectTypeString = effect.type switch
                        {
                            CICIBIEType.ADVENTURE_XP => "ItemExperiencePoints",
                            CICIBIEType.CRAFTING => "CraftingSpeed",
                            CICIBIEType.DEFENSE => "PlayerDefense",
                            CICIBIEType.EATING => "FoodHealth",
                            CICIBIEType.HEALING => "Health",
                            CICIBIEType.HEALTH => "MaximumPlayerHealth",
                            CICIBIEType.ITEM_XP => "ItemExperiencePoints",
                            CICIBIEType.MINING_SPEED => "BlockDamage",
                            CICIBIEType.RETENTION_BACKPACK => "RetainBackpack",
                            CICIBIEType.RETENTION_HOTBAR => "RetainHotbar",
                            CICIBIEType.RETENTION_XP => "RetainExperiencePoints",
                            CICIBIEType.SMELTING => "SmeltingFuelIntensity",
                            CICIBIEType.STRENGTH => "AttackDamage",
                            CICIBIEType.TAPPABLE_RADIUS => "TappableInteractionRadius",
                            _ => throw new UnreachableException(),
                        };

                        string activationString = effect.activation switch
                        {
                            CICIBIEActivation.INSTANT => "Instant",
                            CICIBIEActivation.TIMED => "Timed",
                            CICIBIEActivation.TRIGGERED => "Triggered",
                            _ => throw new UnreachableException(),
                        };

                        return new Types.Catalog.BoostMetadata.Effect(
                            effectTypeString,
                            effect.activation == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Activation.TIMED ? TimeFormatter.FormatDuration(effect.duration) : null,
                            effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.RETENTION_BACKPACK || effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.RETENTION_HOTBAR || effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.RETENTION_XP ? null : effect.value,
                            effect.type switch
                            {
                                CICIBIEType.HEALING or CICIBIEType.TAPPABLE_RADIUS => "Increment",
                                CICIBIEType.ADVENTURE_XP or CICIBIEType.CRAFTING or CICIBIEType.DEFENSE or CICIBIEType.EATING or CICIBIEType.HEALTH or CICIBIEType.ITEM_XP or CICIBIEType.MINING_SPEED or CICIBIEType.SMELTING or CICIBIEType.STRENGTH => "Percentage",
                                CICIBIEType.RETENTION_BACKPACK or CICIBIEType.RETENTION_HOTBAR or CICIBIEType.RETENTION_XP => null,
                                _ => throw new UnreachableException(),
                            },
                            effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.CRAFTING || effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.SMELTING ? "UtilityBlock" : "Player",
                            effect.applicableItemIds,

                            effect.type switch
                            {
                                CICIBIEType.ITEM_XP => new string[] { "Tappable" },
                                CICIBIEType.ADVENTURE_XP => new string[] { "Encounter" },
                                _ => Array.Empty<string>(),
                            },
                            activationString,
                            effect.type == Catalog.ItemsCatalog.Item.BoostInfo.Effect.Type.EATING ? "Health" : null
                        );
                    }).ToArray(),
                            item.boostInfo.triggeredOnDeath ? "Death" : null,
                            null
                    );
            }
            else
            {
                boostMetadata = null;
            }

            Types.Catalog.ItemsCatalog.Item.ItemData.JournalMetadata? journalMetadata;
            if (item.journalEntry is not null)
            {
                string behaviorString = item.journalEntry.behavior switch
                {
                    CICIJEBehavior.NONE => "None",
                    CICIJEBehavior.PASSIVE => "Passive",
                    CICIJEBehavior.HOSTILE => "Hostile",
                    CICIJEBehavior.NEUTRAL => "Neutral",
                    _ => throw new UnreachableException(),
                };

                string biomeString = item.journalEntry.biome switch
                {
                    CICIJEBiome.NONE => "None",
                    CICIJEBiome.OVERWORLD => "Overworld",
                    CICIJEBiome.NETHER => "Hell",
                    CICIJEBiome.BIRCH_FOREST => "BirchForest",
                    CICIJEBiome.DESERT => "Desert",
                    CICIJEBiome.FLOWER_FOREST => "FlowerForest",
                    CICIJEBiome.FOREST => "Forest",
                    CICIJEBiome.ICE_PLAINS => "IcePlains",
                    CICIJEBiome.JUNGLE => "Jungle",
                    CICIJEBiome.MESA => "Mesa",
                    CICIJEBiome.MUSHROOM_ISLAND => "MushroomIsland",
                    CICIJEBiome.OCEAN => "Ocean",
                    CICIJEBiome.PLAINS => "Plains",
                    CICIJEBiome.RIVER => "River",
                    CICIJEBiome.ROOFED_FOREST => "RoofedForest",
                    CICIJEBiome.SAVANNA => "Savanna",
                    CICIJEBiome.SUNFLOWER_PLAINS => "SunFlowerPlains",
                    CICIJEBiome.SWAMP => "Swampland",
                    CICIJEBiome.TAIGA => "Taiga",
                    CICIJEBiome.WARM_OCEAN => "WarmOcean",
                    _ => throw new UnreachableException(),
                };

                journalMetadata = new Types.Catalog.ItemsCatalog.Item.ItemData.JournalMetadata(
                    item.journalEntry.group,
                    item.experience.journal,
                    item.journalEntry.order,
                    behaviorString,
                    biomeString
                );
            }
            else
            {
                journalMetadata = null;
            }

            return new Types.Catalog.ItemsCatalog.Item(
                item.id,
                new Types.Catalog.ItemsCatalog.Item.ItemData(
                    item.name,
                    item.aux,
                    typeString,
                    useTypeString,
                    0,
                    item.consumeInfo?.heal,
                    0,
                    mobDamage,
                    blockDamage,
                    health,
                    item.blockInfo is not null ? new Types.Catalog.ItemsCatalog.Item.ItemData.BlockMetadata(item.blockInfo.breakingHealth, item.blockInfo.efficiencyCategory) : null,
                    new Types.Catalog.ItemsCatalog.Item.ItemData.ItemMetadata(
                        useTypeString,
                        alternativeUseTypeString,
                        mobDamage,
                        blockDamage,
                        null,
                        0,
                        item.consumeInfo is not null ? item.consumeInfo.heal : 0,
                        item.toolInfo?.efficiencyCategory,
                        health
                    ),
                    boostMetadata,
                    journalMetadata,
                    item.journalEntry is not null && item.journalEntry.sound is not null ? new Types.Catalog.ItemsCatalog.Item.ItemData.AudioMetadata(
                            new Dictionary<string, string>() { ["journal"] = item.journalEntry.sound },
                            item.journalEntry.sound
                    ) : null,
                    new Dictionary<string, object>()
                ),
                categoryString,
                Enum.Parse<Types.Common.Rarity>(item.rarity.ToString()),
                1,
                item.stackable,
                item.fuelInfo is not null ? new Types.Common.BurnRate(item.fuelInfo.burnTime, item.fuelInfo.heatPerSecond) : null,
                item.fuelInfo is not null && item.fuelInfo.returnItemId != null ? new ItemsCatalog.Item.ReturnItem[] { new ItemsCatalog.Item.ReturnItem(item.fuelInfo.returnItemId, 1) } : Array.Empty<ItemsCatalog.Item.ReturnItem>(),
                item.consumeInfo != null && item.consumeInfo.returnItemId != null ? new ItemsCatalog.Item.ReturnItem[] { new ItemsCatalog.Item.ReturnItem(item.consumeInfo.returnItemId, 1) } : Array.Empty<ItemsCatalog.Item.ReturnItem>(),
                item.experience.tappable,
                new Dictionary<string, int?>() { ["tappable"] = item.experience.tappable, ["encounter"] = item.experience.encounter, ["crafting"] = item.experience.crafting },
                false
            );
        })];

        Dictionary<string, ItemsCatalog.EfficiencyCategory> efficiencyCategories = [];
        foreach (Catalog.ItemEfficiencyCategoriesCatalog.EfficiencyCategory efficiencyCategory in catalog.itemEfficiencyCategoriesCatalog.efficiencyCategories)
        {
            efficiencyCategories.put(efficiencyCategory.name, new ItemsCatalog.EfficiencyCategory(
                    new ItemsCatalog.EfficiencyCategory.EfficiencyMap(
                            efficiencyCategory.hand(),
                            efficiencyCategory.hoe(),
                            efficiencyCategory.axe(),
                            efficiencyCategory.shovel(),
                            efficiencyCategory.pickaxe_1(),
                            efficiencyCategory.pickaxe_2(),
                            efficiencyCategory.pickaxe_3(),
                            efficiencyCategory.pickaxe_4(),
                            efficiencyCategory.pickaxe_5(),
                            efficiencyCategory.sword(),
                            efficiencyCategory.sheers()
                    )
            ));
        }

        return new ItemsCatalog(items, efficiencyCategories);
    }
}
