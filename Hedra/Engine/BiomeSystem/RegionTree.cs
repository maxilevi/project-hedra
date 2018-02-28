using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionTree
    {
        public TreeDesign PrimaryDesign { get; }
        private readonly BiomeTreeDesign _treeDesign;
        private readonly int _seed;

        public RegionTree(int Seed, BiomeTreeDesign TreeDesign)
        {
            _seed = Seed;
            _treeDesign = TreeDesign;
            PrimaryDesign = _treeDesign.AvailableTypes[new Random(_seed).Next(0, _treeDesign.AvailableTypes.Length)];
        }

        public TreeDesign GetDesign(int Seed)
        {
            var rng = new Random(Seed);
            TreeDesign design = PrimaryDesign;

            bool otherTree = PrimaryDesign is TallDesign
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
