using System;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerStateSnapshot : IGameStateSnapshot<PlayerDelta>
    {
        public PlayerDelta PlayerDelta { get; set; }
        public int Mana { get; set; }

        public PlayerDelta GetDelta()
        {
            return PlayerDelta;
        }
    }
}
