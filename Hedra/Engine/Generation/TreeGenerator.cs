/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 04:26 a.m.
 *
 */

using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.TreeSystem;
using Hedra.Numerics;

namespace Hedra.Engine.Generation
{
    /// <summary>
    ///     Description of TreeGenerator.
    /// </summary>
    public class TreeGenerator
    {
        private readonly float _forestFrequency;
        private readonly float _spaceFrequency;
        public TreeGenerator(Random Rng)
        {
            _spaceFrequency = Rng.NextFloat() * 0.003f + 0.001f;
            _forestFrequency = Rng.NextFloat() * 0.004f + 0.00025f;
        }
        
        private readonly Vector3[] _previousTrees = new Vector3[8];

        public PlacementObject CanGenerateTree(Vector3 Position, Region BiomeRegion)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return default;

            var height = Physics.HeightAtPosition(Position);
            var normal = Physics.NormalAtPosition(Position);

            if (Vector3.Dot(normal, Vector3.UnitY) <= .2f) return default;

            const float valueFactor = 1.05f;
            var spaceBetween = SpaceNoise(Position.X, Position.Z);
            var noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 40f) * valueFactor) + .3f, 1.0f);

            if (spaceBetween < 0) spaceBetween = -spaceBetween * 16f;
            if (PlacementNoise(Position) < 0) return default;

            spaceBetween += BiomeRegion.Trees.PrimaryDesign.Spacing;

            for (var i = 0; i < _previousTrees.Length; i++)
                if ((Position - _previousTrees[i]).LengthSquared() < spaceBetween * spaceBetween)
                    return default;

            for (var i = _previousTrees.Length - 1; i > 0; i--) _previousTrees[i] = _previousTrees[i - 1];

            _previousTrees[0] = Position;

            return new PlacementObject
            {
                Noise = noiseValue,
                Placed = true,
                Position = Position.Xz().ToVector3() + Vector3.UnitY * height
            };
        }

        public void GenerateTree(PlacementObject Placement, Region BiomeRegion, TreeDesign Design)
        {
            var underChunk = World.GetChunkAt(Placement.Position);
            if (underChunk == null) return;
            var rng = new Random(Unique.GenerateSeed(Placement.Position.Xz()));
            var extraScale = new Random(World.Seed + 1111).NextFloat() * 5 + 4;
            var scale = 10 + rng.NextFloat() * 3.5f;

            scale *= extraScale * .5f;
            scale += 8 + rng.NextFloat() * 4f;
            scale *= Placement.Noise;

            var originalModel = Design.Model;
            var model = originalModel.Clone();

            var transMatrix = Matrix4x4.CreateScale(new Vector3(scale, scale, scale) * 1.5f);
            transMatrix *= Matrix4x4.CreateRotationY(rng.NextFloat() * 360f * Mathf.Radian);
            transMatrix *= Matrix4x4.CreateTranslation(Placement.Position);

            model.AddWindValues(AssetManager.ColorCode1);
            model.AddWindValues(AssetManager.ColorCode2);

            var woodColor = BiomeRegion.Colors.WoodColors[rng.Next(0, BiomeRegion.Colors.WoodColors.Length)];

            var leafColor = BiomeRegion.Colors.LeavesColors[rng.Next(0, BiomeRegion.Colors.LeavesColors.Length)];

            model = Design.Paint(model, woodColor, leafColor);
            model.GraduateColor(Vector3.UnitY);

            var shapes = Design.GetShapes(originalModel);
            foreach (var originalShape in shapes)
            {
                var shape = (CollisionShape)originalShape.Clone();
                shape.Transform(transMatrix);
                underChunk.AddCollisionShape(shape);
            }

            var instance = model.ToInstanceData(transMatrix);
            instance.CanSimplifyProgramatically = false;
            underChunk.StaticBuffer.AddInstance(instance);
        }

        private float SpaceNoise(float X, float Z)
        {
            return World.GetNoise(X * _spaceFrequency, (Z + 100) * _spaceFrequency) * 40f;
        }

        public float PlacementNoise(Vector3 Position)
        {
            return World.GetNoise((Position.X + 743) * _forestFrequency, (Position.Z + 14352300) * _forestFrequency);
        }
    }

    public struct PlacementObject
    {
        public Vector3 Position { get; set; }
        public float Noise { get; set; }
        public bool Placed { get; set; }
    }
}