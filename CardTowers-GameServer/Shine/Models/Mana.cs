using System;

namespace CardTowers_GameServer.Shine.Models
{
    public class Mana
    {
        public const float INITIAL_MANA = 0f;
        public const float MAX_MANA = 10f;
        public const long MANA_GENERATION_INTERVAL_MS = 1000;  // 1 second
        public const float MANA_PER_INTERVAL = 1f;
        public const long LOG_INTERVAL_MS = 1000; // 1 second

        private float currentMana;
        private long lastGenerationTime;
        private long lastLogTime;

        public Mana()
        {
            currentMana = INITIAL_MANA;
            lastGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            lastLogTime = lastGenerationTime;
        }

        public void SetCurrentMana(float mana)
        {
            currentMana = Math.Min(mana, MAX_MANA);
        }

        public float GetCurrentMana()
        {
            return currentMana;
        }


        public void Update(long deltaTime)
        {
            float manaGenerated = (deltaTime / (float)MANA_GENERATION_INTERVAL_MS) * MANA_PER_INTERVAL;

            if (manaGenerated > 0)
            {
                SetCurrentMana(currentMana + manaGenerated);
                lastGenerationTime += deltaTime;

                if (lastGenerationTime - lastLogTime >= LOG_INTERVAL_MS)
                {
                    Console.WriteLine($"Current mana: {currentMana}");
                    lastLogTime = lastGenerationTime;
                }
            }
        }


        public bool CanSpendMana(float amount)
        {
            return currentMana >= amount;
        }

        public bool SpendMana(float amount)
        {
            if (CanSpendMana(amount))
            {
                SetCurrentMana(currentMana - amount);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
