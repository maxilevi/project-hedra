using System;
using Hedra.Engine.TreeSystem;

namespace Hedra.BiomeSystem
{
    public class RegionTree
    {
        private readonly int _seed;
        private readonly BiomeTreeDesign _treeDesign;

        public RegionTree(int Seed, BiomeTreeDesign TreeDesign)
        {
            _seed = Seed;
            _treeDesign = TreeDesign;
            PrimaryDesign = _treeDesign.AvailableTypes[new Random(_seed).Next(0, _treeDesign.AvailableTypes.Length)];
        }

        public TreeDesign PrimaryDesign { get; }

        public TreeDesign GetDesign(int Seed)
        {
            var rng = new Random(Seed);
            var design = PrimaryDesign;

            var otherTree = PrimaryDesign is TallDesign
                ? rng.Next(1, 10) == 1
                : rng.Next(1, 6) == 1;

            if (!otherTree || _treeDesign.AvailableTypes.Length == 1) return design;

            ROLL:
            design = _treeDesign.AvailableTypes[rng.Next(0, _treeDesign.AvailableTypes.Length)];
            if (design == PrimaryDesign)
                goto ROLL;

            return design;
        }

        public static RegionTree Interpolate(RegionTree[] Regions)
        {
            return Regions[0];
        }
    }
}