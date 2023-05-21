using System;

namespace CardTowers_GameServer.Shine.Models
{
    public class GameTile
    {
        private TileType type;
        private TileStatus status;

        public TileType Type
        {
            get { return type; }
            set { type = value; }
        }

        public TileStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public void ReloadTile()
        {
            // reload tile on server, so i guess just send update to client
            // to tell their client to reload
        }
    }
}
