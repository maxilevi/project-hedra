using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : Builder<HouseParameters>
    {
        protected override bool LookAtCenter => true;
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(HouseParameters Parameters, VillageCache Cache)
        {
            var work = CreateGroundwork(Parameters.Position, Parameters.GetSize(Cache), BlockType.None);
            work.NoTrees = true;
            work.NoPlants = true;
            return this.PushGroundwork(work);
        }

        public override BuildingOutput Paint(HouseParameters Parameters, BuildingOutput Input)
        {
            return Input;
        }

        public override BuildingOutput Build(HouseParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = BuildSingleHouse(Parameters, Cache, Rng);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Parameters.Design.Doors, transformation, output);
            return output;
        }

        private BuildingOutput BuildSingleHouse(HouseParameters Parameters, VillageCache Cache, Random Rng)
        {
            var rotationMatrix = LookAtCenter ? Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian) : Matrix4.Identity;
            var transformationMatrix = rotationMatrix * Matrix4.CreateTranslation(Parameters.Position);
            var model = Cache.GrabModel(Parameters.Design.Path);
            model.Transform(transformationMatrix);

            var shapes = Cache.GrabShapes(Parameters.Design.Path);
            shapes.ForEach(S => S.Transform(transformationMatrix));
            return new BuildingOutput
            {
                Models = new[] { model },
                Shapes = shapes
            };
        }
    }
}