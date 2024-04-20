using Uma.Uuid;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.DB;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ApiServer.Utils
{
    public static class TokenUtils
    {
        public static EarthDB.Query addToken(string playerId, Tokens.Token token)
        {
            EarthDB.Query getQuery = new EarthDB.Query(true);
            getQuery.Get("tokens", playerId, typeof(Tokens));
            getQuery.Then(results =>
            {
                Tokens tokens = (Tokens)results.Get("tokens").Value;
                string id = U.RandomUuid().ToString();
                tokens.addToken(id, token);
                EarthDB.Query updateQuery = new EarthDB.Query(true);
                updateQuery.Update("tokens", playerId, tokens);
                updateQuery.Extra("tokenId", id);
                return updateQuery;
            });
            return getQuery;
        }
    }
}
