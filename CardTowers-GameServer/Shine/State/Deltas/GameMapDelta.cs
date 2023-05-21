using System;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State;

namespace CardTowers_GameServer.Shine.State.Deltas
{
    public class GameMapDelta : Delta
    {
        public GameTile[] Tiles { get; set; }
        // Add other properties as needed.
    }
}