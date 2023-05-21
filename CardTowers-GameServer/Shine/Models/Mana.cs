﻿using System;

namespace CardTowers_GameServer.Shine.Models
{
    public class Mana
    {
        public const int INITIAL_MANA = 0;
        public const int MAX_MANA = 10;
        public const long MANA_GENERATION_INTERVAL_MS = 2000;
        public const int MANA_PER_INTERVAL = 1;

        private int currentMana;
        private long lastGenerationTime;

        public Mana()
        {
            currentMana = INITIAL_MANA;
            lastGenerationTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public int GetCurrentMana()
        {
            return currentMana;
        }

        public void UpdateMana(long currentTickTime)
        {
            long deltaTime = currentTickTime - lastGenerationTime;
            int manaGenerated = (int)(deltaTime / MANA_GENERATION_INTERVAL_MS) * MANA_PER_INTERVAL;

            if (manaGenerated > 0)
            {
                currentMana = Math.Min(currentMana + manaGenerated, MAX_MANA);
                lastGenerationTime += manaGenerated * MANA_GENERATION_INTERVAL_MS;
            }
        }

        public bool CanSpendMana(int amount)
        {
            return currentMana >= amount;
        }

        public bool SpendMana(int amount)
        {
            if (CanSpendMana(amount))
            {
                currentMana -= amount;
                return true;
            }
            return false;
        }
    }
}