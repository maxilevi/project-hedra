using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    delegate bool SamplerType(Vector2 Position, out int Seed);
    
    public static class StructureGrid
    {
        /* Big structures */
        public const int GraveyardChance = 48;
        public const int GiantTreeChance = 48;
        public const int WaterGiantTreeChance = 96;
        public const int BanditCampChance = 48;
        public const int VillageChance = 2;
        /* Small structures */
        public const int GazeboChance = 24;
        public const int WellChance = 16;
        public const int ObeliskChance = 2;
        public const int CampfireChance = 2;
        public const int TravellingMerchantChance = 8;

        private const int SampleTypes = 5;
        private const int BigSampleChance = 1;
        
        
        private static readonly SamplerType[] Ranges;

        static StructureGrid()
        {
            Ranges = new SamplerType[]
            {
                Sample1024,
                SampleDefault
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
            return Ranges[1];
        }

        private static int BaseSample(Vector2 Position, float Frequency)
        {
            var wSeed = World.Seed * 0.0001f;
            return (int) ((float) World.StructureHandler.SeedGenerator.GetValue(Position.X * Frequency + wSeed, Position.Y * Frequency + wSeed) * 100f);
        }

        private static bool Sample1024(Vector2 Position, out int Seed)
        {
            return Sampler(Position, I => I == BigSampleChance, 0.0005f, out Seed);
        }
        
        private static bool SampleDefault(Vector2 Position, out int Seed)
        {
            return Sampler(Position, I => I != BigSampleChance, 0.0075f, out Seed);
        }

        private static bool Sampler(Vector2 Position, Predicate<int> IsType, float Frequency, out int Seed)
        {
            var baseSeed = BaseSample(Position, .0005f);
            Seed = BaseSample(Position, Frequency);
            var index = new Random(baseSeed).Next(0, SampleTypes);
            return IsType(index) && IsPoint(Position, Frequency); 
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