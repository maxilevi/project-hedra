using System;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public abstract class LivableBuildingBuilder<T> : Builder<T> where T : ILivableBuildingParameters
    {
        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;
        private float _width;
        
        protected LivableBuildingBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(T Parameters, VillageCache Cache)
        {
            _width = Parameters.GetSize(Cache) * 2f;
            var ground = new RoundedGroundwork(Parameters.Position, _width * .5f * .75f, Parameters.Type)
            {
                NoPlants = true,
                NoTrees = true
            };
            var plateau = CreatePlateau(Parameters);
            return PushGroundwork(new GroundworkItem
            {
                Groundwork = ground,
                Plateau = IsPlateauNeeded(plateau) ? plateau : null
            });
        }

        private BasePlateau CreatePlateau(T Parameters)
        {
            return GroundworkType.Squared == Parameters.GroundworkType
                ? (BasePlateau) new SquaredPlateau(Parameters.Position.Xz, _width) { Hardness = 3.0f }
                : new RoundedPlateau(Parameters.Position.Xz, _width * .5f * 1.5f) { Hardness = 3.0f };
        }

        public override void Polish(T Parameters, VillageRoot Root, Random Rng)
        {
            if (Parameters.Design.HasLamp)
            {
                var width = VillageDesign.Spacing * .5f;
                var offset = Vector3.TransformPosition(-width * .5f * Vector3.UnitZ - width * .5f * Vector3.UnitX,
                    Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
                DecorationsPlacer.PlaceLamp(Parameters.Position + offset, Structure, Root, _width, Rng);
            }
        }

        public override BuildingOutput Build(T Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Cache, Parameters.Design.Doors, transformation, output);
            AddBeds(Parameters, Parameters.Design.Beds, transformation, output);
            AddLights(Parameters, Parameters.Design.Lights, transformation, output);
            return output;
        }
    }
}