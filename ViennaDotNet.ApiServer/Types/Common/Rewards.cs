namespace ViennaDotNet.ApiServer.Types.Common
{
    // TODO: determine format
    public record Rewards(
        int? rubies,
        int? experiencePoints,
        int? level,
        Rewards.Item[] inventory,
        Rewards.Buildplate[] buildplates,
        Rewards.Challenge[] challenges,
        Rewards.PersonaItem[] personaItems,
        Rewards.UtilityBlock[] utilityBlocks
    )
    {
        public record Item(
            string id,
            int amount
        )
        {
        }

        public record Buildplate(
            string id
        )
        {
        }

        public record Challenge(
            string id
        )
        {
        }

        public record PersonaItem()
        {
        }

        public record UtilityBlock()
        {
        }
    }
}
