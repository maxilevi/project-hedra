using System;
using System.Linq;
using System.Numerics;

namespace Hedra.Engine.StructureSystem
{
    internal delegate bool SamplerType(Vector2 Position, out int Seed);

    public static class StructureGrid
    {
        /* Big structures */
        public const int GraveyardChance = 4;
        public const int WizardTower = 8;
        public const int GiantTreeChance = 4;
        public const int BanditCampChance = 4;
        public const int VillageChance = 8;
        public const int WitchHut = 4;
        public const int Dungeon0Chance = 6;
        public const int Dungeon1Chance = 6;
        public const int Dungeon2Chance = 6;
        public const int GarrisonChance = 4;
        public const int GnollFortressChance = 4;

        /* Small structures */
        public const int FishingPostChance = 4;
        public const int ShroomPortalChance = 4;
        public const int ObeliskChance = 2;
        public const int CampfireChance = 2;
        public const int TravellingMerchantChance = 8;
        public const int CottageWithFarmChance = 8;
        public const int SolitaryFisherman = 4;
        public const int WellChance = 16;
        public const int GhostTownPortalChance = 64;
        public const int GazeboChance = 64;

        /* Dead realm structures */
        public const int TombstoneChance = 2;

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

        public static StructureDesign Sample(Vector2 Position, StructureDesign[] Designs)
        {
            var sampler = SampleDefault;//SelectSampler(Design);
            var isPoint = sampler.Invoke(Position, out var seed);
            var rng = new Random(seed);
            var isValid = rng.Next(0, 5) == 1;
            if (isPoint && isValid)
            {
                return SelectDesignWeighted(rng, Designs);
            }

            return null;
        }

        private static StructureDesign SelectDesignWeighted(Random Rng, StructureDesign[] Designs)
        {
            var total = Designs.Select(D => 1f / D.StructureChance).Sum();
            var accum = 0f;

            var n = Rng.NextSingle() * total;
            for (var i = 0; i < Designs.Length; ++i)
            {
                var c = 1f / Designs[i].StructureChance;
                if (n <= c + accum)
                    return Designs[i];
                accum += c;
            }
            return null;
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
            return (int)((float)World.StructureHandler.SeedGenerator.GetValue(Position.X * Frequency + wSeed,
                Position.Y * Frequency + wSeed) * 100f);
        }

        private static bool Sample1024(Vector2 Position, out int Seed)
        {
            Seed = 0;
            return false;
            //return Sampler(Position, I => I == BigSampleChance, 0.002f, out Seed);
        }

        private static bool SampleDefault(Vector2 Position, out int Seed)
        {
            Seed = new Vector3(Position.X, Position.Y, World.Seed).GetHashCode();
            return (int)Position.X % 11 == 4 && (int)Position.Y % 13 == 6;
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
            var pointCoords =
                World.StructureHandler.SeedGenerator.GetGridPoint(Position.X * Frequency + wSeed,
                    Position.Y * Frequency + wSeed);
            var chunkCoords = World.ToChunkSpace(new Vector2((int)((pointCoords.X - wSeed) / Frequency),
                (int)((pointCoords.Y - wSeed) / Frequency)));
            var isPoint = chunkCoords == Position;
            return isPoint;
        }
    }
}