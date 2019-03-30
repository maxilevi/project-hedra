using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    delegate bool SamplerType(Vector2 Position, out int Seed);
    
    public static class StructureGrid
    {
        /* Big structures */
        public const int GraveyardChance = 2;//125;
        public const int GiantTreeChance = 2;//100;
        public const int WaterGiantTreeChance = 2;//500;
        public const int BanditCampChance = 2;//128;
        public const int VillageChance = 2;//300;
        /* Small structures */
        public const int WellChance = 80;
        public const int ObeliskChance = 10;
        public const int CampfireChance = 12;
        public const int TravellingMerchantChance = 20;


        private static readonly SamplerType[] Ranges;

        static StructureGrid()
        {
            Ranges = new SamplerType[]
            {
                Sample1024,
                Sample512,
                Sample128,
                Sample80
            };
        }
        
        public static bool Sample(Vector2 Position, StructureDesign Design, StructureDesign[] Types)
        {
            var sampler = SelectSampler(Design);
            var isPoint = sampler.Invoke(Position, out var seed);
            var index = new Random(seed).Next(0, Types.Length);
            return isPoint && index == Array.IndexOf(Types, Design);
        }

        private static SamplerType SelectSampler(StructureDesign Design)
        {
            if (Design.PlateauRadius >= 1024)
                return Ranges[0];
            if (Design.PlateauRadius >= 512)
                return Ranges[1];
            if (Design.PlateauRadius >= 128)
                return Ranges[2];
            return Ranges[3];
        }

        private static int BaseSample(Vector2 Position)
        {
            var wSeed = World.Seed * 0.0001f;
            var frequency = 0.0005f;
            return (int) ((float) World.StructureHandler.SeedGenerator.GetValue(Position.X * frequency + wSeed, Position.Y * frequency + wSeed) * 100f);
        }

        private static bool Sample1024(Vector2 Position, out int Seed)
        {
            return Sampler(Position, Array.IndexOf(Ranges, Sample1024), 0.0005f, out Seed);
        }
        
        private static bool Sample512(Vector2 Position, out int Seed)
        {
            return Sampler(Position, Array.IndexOf(Ranges, Sample512), 0.0005f, out Seed);
        }
        
        private static bool Sample128(Vector2 Position, out int Seed)
        {
            return Sampler(Position, Array.IndexOf(Ranges, Sample128), 0.0015f, out Seed); 
        }
        
        private static bool Sample80(Vector2 Position, out int Seed)
        {
            return Sampler(Position, Array.IndexOf(Ranges, Sample80), 0.0025f, out Seed); 
        }

        private static bool Sampler(Vector2 Position, int Index, float Frequency, out int Seed)
        {
            Seed = BaseSample(Position);
            var index = new Random(Seed).Next(0, Ranges.Length);
            return index == Index && IsPoint(Position, Frequency); 
        }
        

        private static bool IsPoint(Vector2 Position, float Frequency)
        {
            var wSeed = World.Seed * 0.0001f;
            var pointCoords = World.StructureHandler.SeedGenerator.GetGridPoint(Position.X * Frequency + wSeed, Position.Y * Frequency + wSeed);
            var chunkCoords = World.ToChunkSpace(new Vector2((int)((pointCoords.X - wSeed) / Frequency), (int)((pointCoords.Y - wSeed) / Frequency)));
            var isPoint = chunkCoords == Position;
            return isPoint;
        }
    }
}