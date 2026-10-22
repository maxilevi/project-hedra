using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.Generation
{
    // One placement pass owns this snapshot. Never retain it across terrain or
    // structure updates, and never change the order of the surviving candidates.
    public sealed class VegetationCandidates
    {
        private readonly GiantTreeZone[] _trees;
        public IGroundwork[] PlantGroundworks { get; }
        public IGroundwork[] TreeGroundworks { get; }
        public BasePlateau[] TreePlateaus { get; }

        private readonly struct GiantTreeZone
        {
            public readonly Vector2 Position;
            public readonly float GrassRadiusSquared, TreeRadiusSquared;

            public GiantTreeZone(RoundedPlateau Mountain)
            {
                Position = Mountain.Position;
                var radius = Mountain.Radius;
                // Preserve the original multiplication order at exclusion edges.
                GrassRadiusSquared = radius * .3f * radius * .3f;
                TreeRadiusSquared = radius * .5f * radius * .5f;
            }
        }

        public VegetationCandidates(int OffsetX, int OffsetZ, CollidableStructure[] Structures,
            IGroundwork[] Groundworks, BasePlateau[] Plateaus)
        {
            var min = new Vector2(OffsetX, OffsetZ);
            var max = min + new Vector2((Chunk.BoundsX - 1) * Chunk.BlockSize,
                (Chunk.BoundsZ - 1) * Chunk.BlockSize);
            var trees = new List<GiantTreeZone>();
            foreach (var structure in Structures)
            {
                if (structure.Design is not GiantTreeDesign) continue;
                var zone = new GiantTreeZone(structure.Mountain);
                var nearest = Vector2.Clamp(zone.Position, min, max);
                var distance = (zone.Position - nearest).LengthSquared();
                if (distance <= zone.GrassRadiusSquared || distance <= zone.TreeRadiusSquared)
                    trees.Add(zone);
            }
            _trees = trees.ToArray();

            var plants = new List<IGroundwork>();
            var treeGroundworks = new List<IGroundwork>();
            foreach (var groundwork in Groundworks)
            {
                if ((!groundwork.NoPlants && !groundwork.NoTrees) || !MayAffect(groundwork, min, max)) continue;
                if (groundwork.NoPlants) plants.Add(groundwork);
                if (groundwork.NoTrees) treeGroundworks.Add(groundwork);
            }
            PlantGroundworks = plants.ToArray();
            TreeGroundworks = treeGroundworks.ToArray();
            var plateaus = new List<BasePlateau>();
            foreach (var plateau in Plateaus)
                if (plateau.NoTrees) plateaus.Add(plateau);
            TreePlateaus = plateaus.ToArray();
        }

        private static bool MayAffect(IGroundwork Groundwork, Vector2 Min, Vector2 Max)
        {
            // LineGroundwork's bounds omit its width; custom shapes may have
            // similarly loose bounds. Only cull exact built-in round/square types.
            var type = Groundwork.GetType();
            if (type != typeof(RoundedGroundwork) && type != typeof(SquaredGroundwork)) return true;
            var bounds = Groundwork.ToBoundingBox();
            if (bounds.Width < 0 || !float.IsFinite(bounds.Width)) return true;
            return bounds.RightCorner.X >= Min.X && bounds.LeftCorner.X <= Max.X &&
                   bounds.FrontCorner.Y >= Min.Y && bounds.BackCorner.Y <= Max.Y;
        }

        public void StructureExclusions(Vector2 Position, out bool NoGrass, out bool NoTrees)
        {
            NoGrass = false;
            NoTrees = false;
            foreach (var zone in _trees)
            {
                var distance = (zone.Position - Position).LengthSquared();
                NoGrass |= distance < zone.GrassRadiusSquared;
                NoTrees |= distance < zone.TreeRadiusSquared;
                if (NoGrass && NoTrees) return;
            }
        }
    }
}
