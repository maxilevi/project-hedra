/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 04:26 a.m.
 *
 */
using System;
using Hedra.Engine.Rendering;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.TreeSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
    /// <summary>
    /// Description of TreeGenerator.
    /// </summary>
    public class TreeGenerator
    {
        private readonly Vector3[] _previousTrees = new Vector3[8];

        public PlacementObject CanGenerateTree(Vector3 Position, Region BiomeRegion)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return default(PlacementObject);

            var height = Physics.HeightAtPosition(Position);
            var normal = Physics.NormalAtPosition(Position);
            
            if (Vector3.Dot(normal, Vector3.UnitY) <= .2f) return default(PlacementObject);
            
            const float valueFactor = 1.05f;
            var spaceBetween = SpaceNoise(Position.X, Position.Z);
            var noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 40f) * valueFactor) + .3f, 1.0f);
            
            if(spaceBetween < 0) spaceBetween = -spaceBetween * 16f;
            if (PlacementNoise(Position) < 0) return default(PlacementObject);

            spaceBetween += BiomeRegion.Trees.PrimaryDesign.Spacing;

            for(var i = 0; i < _previousTrees.Length; i++){
                if( (Position - _previousTrees[i] ).LengthSquared < spaceBetween * spaceBetween)
                    return default(PlacementObject);    
            }
            
            for (var i = _previousTrees.Length-1; i > 0; i--)
            {
                _previousTrees[i] = _previousTrees[i - 1];
            }
                    
            _previousTrees[0] = Position;

            return new PlacementObject
            {
                Noise = noiseValue,
                Placed = true,
                Position = Position.Xz.ToVector3() + Vector3.UnitY * height
            };
        }
        
        public void GenerateTree(PlacementObject Placement, Region BiomeRegion, TreeDesign Design)
        {
            var underChunk = World.GetChunkAt(Placement.Position);
            if(underChunk == null) return;
            var rng = new Random(Unique.GenerateSeed(Placement.Position.Xz));
            var extraScale = new Random(World.Seed + 1111).NextFloat() * 5 + 4;
            var scale = 10 + rng.NextFloat() * 3.5f;

            scale *= extraScale * .5f;
            scale += 8 + rng.NextFloat() * 4f;
            scale *= Placement.Noise;

            var originalModel = Design.Model;
            var model = originalModel.Clone();

            var transMatrix = Matrix4.CreateScale(new Vector3(scale, scale, scale) * 1.5f );
            transMatrix *=  Matrix4.CreateRotationY( rng.NextFloat() * 360f * Mathf.Radian);
            transMatrix *= Matrix4.CreateTranslation( Placement.Position );

            model.AddWindValues(AssetManager.ColorCode1);
            model.AddWindValues(AssetManager.ColorCode2);

            Vector4 woodColor = rng.Next(0, 5) != 1
                ? BiomeRegion.Colors.WoodColors[new Random(World.Seed + 5232).Next(0, BiomeRegion.Colors.WoodColors.Length)] 
                : BiomeRegion.Colors.WoodColor;

            Vector4 leafColor = rng.Next(0, 5) != 1 
                ? BiomeRegion.Colors.LeavesColors[new Random(World.Seed + 42132).Next(0, BiomeRegion.Colors.LeavesColors.Length)] 
                : BiomeRegion.Colors.LeavesColor;

            model = Design.Paint(model, woodColor, leafColor);
            model.GraduateColor(Vector3.UnitY);
            
            var shapes = Design.GetShapes(originalModel);
            foreach (var originalShape in shapes)
            {
                var shape = (CollisionShape) originalShape.Clone();
                shape.Transform(transMatrix);
                underChunk.AddCollisionShape(shape);
            }
            underChunk.StaticBuffer.AddInstance(model.ToInstanceData(transMatrix));
        }

        public float SpaceNoise(float X, float Z)
        {
            return (float) World.GetNoise(X * .004f, (Z + 100) * .004f) * 40f;
        }

        private static float PlacementNoise(Vector3 Position)
        {
            return (float) World.GetNoise((Position.X + 743) * .01f, (Position.Z + 14352300) * .01f);
        }
    }

    public struct PlacementObject
    {
        public Vector3 Position { get; set; }
        public float Noise { get; set; }
        public bool Placed { get; set; }
    }
}