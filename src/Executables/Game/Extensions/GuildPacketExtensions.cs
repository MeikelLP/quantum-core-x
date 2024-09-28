﻿using System.Collections.Immutable;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.Extensions;

public static class GuildPacketExtensions
{
    public static void SendGuildNews(this IConnection connection, ImmutableArray<GuildNewsData> news)
    {
        connection.Send(new GuildNewsPacket
        {
            News = news.Select(x => new GuildNews
            {
                NewsId = x.Id,
                PlayerName = x.PlayerName,
                Message = x.Message
            }).ToArray()
        });
    }
}