using static ViennaDotNet.ApiServer.Types.Catalog.RecipesCatalog;
using static ViennaDotNet.ApiServer.Types.Catalog.RecipesCatalog.CraftingRecipe;

namespace ViennaDotNet.ApiServer.Types.Catalog
{
    public record RecipesCatalog(
        CraftingRecipe[] crafting,
        SmeltingRecipe[] smelting
    )
    {
        public record CraftingRecipe(
            string id,
            string category,
            string duration,
            Ingredient[] ingredients,
            Output output,
            ReturnItem[] returnItems,
            bool deprecated
        )
        {
            public record Ingredient(
                string[] items,
                int quantity
            )
            {
            }

            public record Output(
                string itemId,
                int quantity
            )
            {
            }

            public record ReturnItem(
                string id,
                int amount
            )
            {
            }
        }

        public record SmeltingRecipe(
            string id,
            int heatRequired,
            string inputItemId,
            Output output,
            ReturnItem[] returnItems,
            bool deprecated
        )
        {
            public record Output(
                string itemId,
                int quantity
            )
            {
            }

            public record ReturnItem(
                string id,
                int amount
            )
            {
            }
        }
    }
}
