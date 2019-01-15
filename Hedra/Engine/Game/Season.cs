using System;

namespace Hedra.Engine.Game
{
    public static class Season
    {
        static Season()
        {
            var date = DateTime.Now;
            IsChristmas = date.Month == 12;
            IsHaloween = date.Month == 10;
            IsAprilFools = date.Day == 1 && date.Month == 4;
        }
        
        public static bool IsChristmas { get; private set; }
        
        public static bool IsHaloween  { get; private set; }
        
        public static bool IsAprilFools  { get; private set; }
    }
}