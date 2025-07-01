using static ViennaDotNet.ApiServer.Models.Tokens.Live;

namespace ViennaDotNet.ApiServer.Models;

public sealed record Token<TData>(
    DateTimeOffset Issued,
    DateTimeOffset Expires,
    bool? Expired,
    TData Data
) where TData : ITokenData<TData>;

public static class Tokens
{
    public static class Live
    {
        public sealed record UserToken(
            string UserId,
            string Username,
            string PasswordSalt,
            string PasswordHash
        ) : ITokenData<UserToken>;

        public sealed record DeviceToken()
            : ITokenData<DeviceToken>;
    }

    public static class Shared
    {
        public sealed record XboxTicketToken(
            string UserId,
            string Username
        ) : ITokenData<XboxTicketToken>;

        public sealed record PlayfabXboxToken(
            string UserId
        ) : ITokenData<PlayfabXboxToken>;

        public sealed record PlayfabSessionTicket(
            string UserId
        ) : ITokenData<PlayfabSessionTicket>;
    }
}

public interface ITokenData<TSelf> where TSelf : ITokenData<TSelf>
{
}