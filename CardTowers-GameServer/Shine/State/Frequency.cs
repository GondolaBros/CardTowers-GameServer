using System;
namespace CardTowers_GameServer.Shine.State
{
    public enum Frequency
    {
        VeryFast = 50,  // 20 updates per second
        Fast = 100,     // 10 updates per second
        Moderate = 500, // 2 updates per second
        Slow = 1000,    // 1 update per second
        VerySlow = 2000,// 0.5 updates per second
        EventBased      // Delta generation is event-driven rather than time-driven
    }
}

