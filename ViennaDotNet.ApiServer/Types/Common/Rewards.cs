namespace ViennaDotNet.ApiServer.Types.Common;

// TODO: determine format
public record Rewards(
    int? rubies,
    int? experiencePoints,
    int? level,
    Rewards.Item[] inventory,
    string[] buildplates,
    Rewards.Challenge[] challenges,
    string[] personaItems,
    Rewards.UtilityBlock[] utilityBlocks
)
{
    public record Item(
        string id,
        int amount
    )
    {
    }

    public record Challenge(
        string id
    )
    {
    }

    public record UtilityBlock()
    {
    }
}
