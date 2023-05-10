using System.Collections;
using System.Collections.Generic;


namespace CardTowers_GameServer.Shine.Entities
{
    public abstract class Building
    {
        public int Tier { get; protected set; }
        public int Damage { get; protected set; }
        public float AttackSpeed { get; protected set; }
    }
}