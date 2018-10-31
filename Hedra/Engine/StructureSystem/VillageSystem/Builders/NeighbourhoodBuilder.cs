using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class NeighbourhoodBuilder : Builder<NeighbourhoodParameters>
    {
        public NeighbourhoodBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(NeighbourhoodParameters Parameters, VillageCache Cache)
        {
            var options = new []{BlockType.Path, BlockType.StonePath, BlockType.DarkStonePath};
            var selected = options[Parameters.Rng.Next(0, options.Length)];
            var work = CreateGroundwork(Parameters.Position, Parameters.Size, selected);          
            (work.Groundwork as RoundedGroundwork).Radius *= .4f;
            work.Plateau.NoTrees = true;
            return this.PushGroundwork(work);
        }

        public override BuildingOutput Paint(NeighbourhoodParameters Parameters, BuildingOutput Input)
        {
            return Input;
        }

        public override BuildingOutput Build(NeighbourhoodParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            return CircularNeighbourhoodHouse(Parameters, Cache, Rng);
        }

        private BuildingOutput CircularNeighbourhoodHouse(NeighbourhoodParameters Parameters, VillageCache Cache, Random Rng)
        {
            var houseCount = Parameters.HouseCount;
            var modelsList = new List<VertexData>();
            var shapesList = new List<CollisionShape>();
            for (var i = 0; i < houseCount; i++)
            {
                var design = Parameters.HouseTemplates[Rng.Next(0, Parameters.HouseTemplates.Length)];
                var dist = Parameters.Size - Rng.NextFloat() * 4 * Chunk.BlockSize - Cache.GrabSize(design.Path).Z * 1.5f;
                var rotationMatrix = Matrix4.CreateRotationY((360 / houseCount * i) * Mathf.Radian);
                var distanceMatrix = Matrix4.CreateTranslation(-dist * Vector3.UnitZ);
                var positionMatrix = Matrix4.CreateTranslation(Parameters.Position);
                
                
                if(base.IntersectsWithAnyPath(
                    Vector3.TransformPosition(Vector3.Zero, distanceMatrix * rotationMatrix * positionMatrix).Xz,
                    Cache.GrabSize(design.Path).Xz.LengthFast
                )) continue;
                
                var model = Cache.GrabModel(design.Path);
                model.Transform(distanceMatrix);
                model.Transform(rotationMatrix);
                model.Transform(positionMatrix);
                modelsList.Add(model);

                var shapes = Cache.GrabShapes(design.Path);
                shapes.ForEach(S => S.Transform(distanceMatrix));
                shapes.ForEach(S => S.Transform(rotationMatrix));
                shapes.ForEach(S => S.Transform(positionMatrix));
                shapesList.AddRange(shapes);
            }
            return new BuildingOutput
            {
                Models = modelsList,
                Shapes = shapesList
            };
        }
    }
}