using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BlacksmithBuilder : Builder<BlacksmithParameters>
    {
        private float _width;

        public BlacksmithBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        public override bool Place(BlacksmithParameters Parameters, VillageCache Cache)
        {
            return PlaceGroundwork(Parameters.Position, (_width = ModelRadius(Parameters, Cache)) * .5f,
                BlockType.StonePath);
        }

        public override BuildingOutput Build(BlacksmithParameters Parameters, DesignTemplate Design, VillageCache Cache,
            Random Rng, Vector3 VillageCenter)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, VillageCenter);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Cache, Parameters.Design.Doors, transformation, output);
            AddChimneys(Parameters, Parameters.Design.Chimneys, transformation, output);
            return output;
        }

        public override void Polish(BlacksmithParameters Parameters, VillageRoot Root, Random Rng)
        {
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            if (Rng.Next(0, 3) != 1 && Parameters.Design.HasBlacksmith)
            {
                var blacksmithOffset = Vector3.Transform(Parameters.Design.Blacksmith * Parameters.Design.Scale * 1.5f,
                    transformation);
                var newPosition = Parameters.Position + blacksmithOffset;
                DecorationsPlacer.PlaceWhenWorldReady(newPosition,
                    P =>
                    {
                        var human = SpawnHumanoid(
                            HumanType.Blacksmith,
                            P
                        );
                        human.Position = P;
                    },
                    () => Structure.Disposed);
            }

            var lampOffset =
                Vector3.Transform(Parameters.Design.LampPosition * Parameters.Design.Scale, transformation);
            DecorationsPlacer.PlaceLamp(Parameters.Position + lampOffset, Structure, Root, _width, Rng);
            PlaceAnvilIfNecessary(Parameters, transformation);
            PlaceWorkbenchIfNecessary(Parameters, transformation);
        }

        private void PlaceAnvilIfNecessary(BlacksmithParameters Parameters, Matrix4x4 Transformation)
        {
            if (!Parameters.Design.HasAnvil) return;
            var position = Parameters.Position + Vector3.Transform(
                Parameters.Design.AnvilPosition * Parameters.Design.Scale,
                Transformation);
            DecorationsPlacer.PlaceWhenWorldReady(position,
                P => Structure.WorldObject.AddChildren(new Anvil(P)),
                () => Structure.Disposed
            );
        }

        private void PlaceWorkbenchIfNecessary(BlacksmithParameters Parameters, Matrix4x4 Transformation)
        {
            if (!Parameters.Design.HasWorkbench) return;
            var position = Parameters.Position + Vector3.Transform(
                Parameters.Design.WorkbenchPosition * Parameters.Design.Scale,
                Transformation);
            DecorationsPlacer.PlaceWhenWorldReady(position,
                P => Structure.WorldObject.AddChildren(new Workbench(P)),
                () => Structure.Disposed
            );
        }
    }
}