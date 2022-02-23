using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Numerics;
using Hedra.Structures;

namespace Hedra.Engine.StructureSystem
{
    internal delegate bool SamplerType(Vector2 Position, out int Seed);

    public static class StructureGrid
    {

        /* Check the excel */
        public const int GhostTownPortalChance = 4;
        public const int GazeboChance = 4;
        public const int TravellingMerchantChance = 1;
        public const int ShroomPortalChance = 3;
        public const int FishingPostChance = 15;
        public const int GraveyardChance = 7;
        public const int WizardTower = 4;
        public const int GiantTreeChance = 7;
        public const int BanditCampChance = 7;
        public const int VillageChance = 15;
        public const int WitchHut = 5;
        public const int Dungeon0Chance = 4;
        public const int Dungeon1Chance = 4;
        public const int Dungeon2Chance = 4;
        public const int GarrisonChance = 6;
        public const int GnollFortressChance = 7;
        
        public const int WellChance = 15;
        public const int CampfireChance = 25;
        public const int ObeliskChance = 20;
        public const int CottageWithFarmChance = 10;
        public const int SolitaryFisherman = 30;

        /* Dead realm structures */
        public const int TombstoneChance = 2;

        private const int SampleTypes = 5;
        private const int BigSampleChance = 1;

        private static bool IsBig(Vector2 Position, RandomDistribution Distribution, out int Seed)
        {
            if ((int)Math.Abs(Position.X % 23) == 4 && Math.Abs((int)Position.Y % 17) == 6)
            {
                Seed = Unique.GetSeed(Position);
                Distribution.Seed = Seed;
                return Distribution.Next(0, 2) == 1;
            }

            Seed = 0;
            return false;
        }
        
        private static bool IsSmall(Vector2 Position, RandomDistribution Distribution, out int Seed)
        {
            if ((int)Math.Abs(Position.X % 11) == 4 && Math.Abs((int)Position.Y % 7) == 6)
            {
                Seed = Unique.GetSeed(Position);
                Distribution.Seed = Seed;
                return Distribution.Next(0, 2) == 1;
            }
            Seed = 0;
            return false;
        }
        
        private static bool HasBigNeighbours(Vector2 Position, RandomDistribution Distribution)
        {
            const int size = 10 * Chunk.Width;
            for (var nx = Position.X - size; nx < Position.X + size; nx += Chunk.Width)
            {
                for (var ny = Position.Y - size; ny < Position.Y + size; ny += Chunk.Width)
                {
                    if (IsBig(new Vector2(nx, ny), Distribution, out _))
                        return true;
                }
            }

            return false;
        }

        private static bool CanPlaceFixedStructure(Vector2 Position, IEnumerable<StructureDesign> FixedDesigns, out StructureDesign Design)
        {
            Design = null;
            foreach (var design in FixedDesigns)
            {
                var p = Position.ToVector3();
                if (design.ShouldSetup(Position, ref p, null, null, null))
                    Design = design;
            }
            return Design != null;
        }

        public static StructureDesign Sample(Vector2 Position, StructureDesign[] Designs)
        {
            if (CanPlaceFixedStructure(Position, Designs.Where(S => S.IsFixed), out var design))
                return design;

            var designs = Designs.Where(S => !S.IsFixed);
            
            var namedStructures = designs.Where(S => S.Icon != null);
            var unnamedStructures = designs.Where(S => S.Icon == null);
            var randomDistribution = new RandomDistribution();

            var isBig = IsBig(Position, randomDistribution, out var seedBig);
            var isSmall = IsSmall(Position, randomDistribution, out var seedSmall);
            
            if (isSmall && !HasBigNeighbours(Position, randomDistribution))
            {
                randomDistribution.Seed = seedSmall;
                return SelectDesignWeighted(randomDistribution, unnamedStructures);
            }
            
            if (isBig)
            {
                randomDistribution.Seed = seedBig;
                return SelectDesignWeighted(randomDistribution, namedStructures);
            }

            return null;
        }

        private static void Shuffle(RandomDistribution Rng, List<StructureDesign> Designs)
        {
            for (var i = 0; i < Designs.Count; ++i)
            {
                var j = Rng.Next(i, Designs.Count);
                (Designs[j], Designs[i]) = (Designs[i], Designs[j]);
            }
        }
        
        private static StructureDesign SelectDesignWeighted(RandomDistribution Rng, IEnumerable<StructureDesign> Designs)
        {
            var total = Designs.Select(D => D.StructureChance).Sum();
            var designs = Designs.ToList();
            Shuffle(Rng, designs);
            var accum = 0f;

            var n = Rng.NextFloat() * total;
            foreach (var design in designs)
            {
                var c = design.StructureChance;
                if (n <= c + accum)
                    return design;
                accum += c;
            }
            return null;
        }
    }
}