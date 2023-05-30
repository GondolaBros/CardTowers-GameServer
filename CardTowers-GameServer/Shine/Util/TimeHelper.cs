using System;
namespace CardTowers_GameServer.Shine.Util
{
    public static class TimeHelper
    {
        public static double MillisecondsToSeconds(long milliseconds)
        {
            return milliseconds / 1000.0;
        }

        public static long SecondsToMilliseconds(double seconds)
        {
            return (long)(seconds * 1000);
        }
    }
}

