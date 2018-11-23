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
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
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

        public PlacementObject CanGenerateTree(Vector3 Position, Region BiomeRegion, int Lod)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return default(PlacementObject);
            var rng = new Random(BiomeGenerator.GenerateSeed(Position.Xz));

            var height = Physics.HeightAtPosition(Position, Lod);
            var normal = Physics.NormalAtPosition(Position, Lod);
            
            if (Vector3.Dot(normal, Vector3.UnitY) <= .2f) return default(PlacementObject);
            
            const float valueFactor = 1.05f;
            float spaceBetween;
            float noiseValue;

            if (World.MenuSeed == World.Seed)
            {
                //This old noise doesnt support negative coordinates
                //And I will leave it here because the menu looks good with it.
                spaceBetween = Position.X > 0 && Position.Z > 0 
                    ? Noise.Generate(Position.X * .001f, (Position.Z + 100) * .001f) * 75f
                    : int.MaxValue;
                noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 75f) * valueFactor)+.3f, 1.0f);
            }
            else
            {
                spaceBetween = SpaceNoise(Position.X, Position.Z);
                noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 40f) * valueFactor) + .3f, 1.0f);
            }
            if(spaceBetween < 0) spaceBetween = -spaceBetween * 16f;
            if (PlacementNoise(Position) < 0) return default(PlacementObject);

            if (World.MenuSeed != World.Seed)
                spaceBetween += BiomeRegion.Trees.PrimaryDesign.Spacing;
            else
                spaceBetween = 80f;

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
            var rng = new Random(BiomeGenerator.GenerateSeed(Placement.Position.Xz));
            var extraScale = new Random(World.Seed + 1111).NextFloat() * 5 + 4;
            var scale = 10 + rng.NextFloat() * 3.5f;

            scale *= extraScale * .5f;
            scale += 8 + rng.NextFloat() * 4f;
            scale *= Placement.Noise;

            var originalModel = Design.Model;
            var model = originalModel.Clone();

            var transMatrix = Matrix4.CreateScale(new Vector3(scale, scale, scale) * 1.5f );
            transMatrix *=  Matrix4.CreateRotationY( rng.NextFloat() * 360f);
            transMatrix *= Matrix4.CreateTranslation( Placement.Position );

            var windRng = Utils.Rng.NextFloat(); 
            model.Extradata.AddRange( model.GenerateWindValues(AssetManager.ColorCode1, 1) );
            model.AddExtraData(AssetManager.ColorCode2, model.GenerateWindValues(AssetManager.ColorCode2, 1));

            Vector4 woodColor = rng.Next(0, 5) != 1
                ? BiomeRegion.Colors.WoodColors[new Random(World.Seed + 5232).Next(0, BiomeRegion.Colors.WoodColors.Length)] 
                : BiomeRegion.Colors.WoodColor;

            Vector4 leafColor = rng.Next(0, 5) != 1 
                ? BiomeRegion.Colors.LeavesColors[new Random(World.Seed + 42132).Next(0, BiomeRegion.Colors.LeavesColors.Length)] 
                : BiomeRegion.Colors.LeavesColor;

            model = Design.Paint(model, woodColor, leafColor);
            model.GraduateColor(Vector3.UnitY);

            if (underChunk.Initialized)
            {
                List<CollisionShape> shapes = Design.GetShapes(originalModel);
                foreach (CollisionShape originalShape in shapes)
                {
                    var shape = (CollisionShape) originalShape.Clone();
                    shape.Transform(transMatrix);
                    underChunk.AddCollisionShape(shape);
                }

                var data = new InstanceData
                {
                    ExtraData = model.Extradata,
                    MeshCache = originalModel,
                    Colors = model.Colors,
                    TransMatrix = transMatrix
                };

                CacheManager.Check(data);
                underChunk.StaticBuffer.AddInstance(data);
            }
        }

        public float SpaceNoise(float X, float Z)
        {
            return (float) OpenSimplexNoise.Evaluate(X * .004f, (Z + 100) * .004f) * 40f;
        }

        private static float PlacementNoise(Vector3 Position)
        {
            return (float) OpenSimplexNoise.Evaluate((Position.X + 743) * .01f, (Position.Z + 14352300) * .01f);
        }
    }

    public struct PlacementObject
    {
        public Vector3 Position { get; set; }
        public float Noise { get; set; }
        public bool Placed { get; set; }
    }
}