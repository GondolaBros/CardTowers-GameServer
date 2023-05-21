using System;
using CardTowers_GameServer.Shine.Util;

namespace CardTowers_GameServer.Shine.Models
{
    public class GameMap
    {
        private const int NUM_COLS = 6;
        private const int NUM_ROWS = 5;

        public GameTile[] Tiles { get; private set; }
        public Building[] Buildings { get; private set; }
        public Spell[] Spells { get; private set; }
        public Creature[] Creatures { get; private set; }

        private int buildingCount;

        public string MapName { get; private set; }

        public GameMap()
        {
            Tiles = new GameTile[NUM_ROWS * NUM_COLS];

            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new GameTile();
            }

            Buildings = new Building[Constants.MAX_AMOUNT_BUILDINGS];

            buildingCount = 0;
        }


        // eventally this will load from db
        public void LoadFromMapString(string mapString)
        {
            string[] tokens = mapString.Split('|');

            int[] tileTypes = System.Array.ConvertAll(tokens[1].Split(','), int.Parse);
            int[] tileStatus = System.Array.ConvertAll(tokens[2].Split(','), int.Parse);

            for (int i = 0; i < tileTypes.Length; i++)
            {
                Tiles[i].Type = (TileType)tileTypes[i];
                Tiles[i].Status = (TileStatus)tileStatus[i];
                Tiles[i].ReloadTile();
            }

            this.MapName = tokens[0];
        }


        public GameTile GetTile(int row, int col)
        {
            return Tiles[row * NUM_COLS + col];
        }


        public void UpdateTile(int row, int col, TileType tileType)
        {
            Tiles[row * NUM_COLS + col].Type = tileType;
        }


        public bool AddBuilding(Building building)
        {
            if (buildingCount >= Buildings.Length)
            {
                return false; // The array is full, the building cannot be added
            }

            Buildings[buildingCount++] = building;
            return true;
        }


        public bool RemoveBuilding(Building building)
        {
            int index = Array.IndexOf(Buildings, building);

            if (index < 0)
            {
                // The building was not found in the array
                return false; 
            }

            // Shift all elements to the right of the removed building
            // one position to the left
            for (int i = index; i < buildingCount - 1; i++)
            {
                Buildings[i] = Buildings[i + 1];
            }

            // Remove the last reference to avoid duplicates
            Buildings[--buildingCount] = null; 
            return true;
        }



        // we can implement LoadFromFile method to load the map data
        // from a file or other source on the server
    }
}
