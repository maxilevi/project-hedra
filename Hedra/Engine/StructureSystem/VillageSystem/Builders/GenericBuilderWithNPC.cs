using System;
using System.Numerics;
using Hedra.AISystem.Humanoid;
using Hedra.Engine.Generation;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class GenericBuilderWithNPC : GenericBuilder
    {
        public GenericBuilderWithNPC(CollidableStructure Structure) : base(Structure)
        {
        }

        public override void Polish(GenericParameters Parameters, VillageRoot Root, Random Rng)
        {
            if (!Parameters.HasNPC) return;
            var position = Parameters.Position +
                           Vector3.Transform(Vector3.UnitX * Width * .5f * Parameters.NPCSettings.DistanceFromBuilding,
                               Matrix4x4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            var human = SpawnHumanoid(Parameters.NPCSettings.Type, position);
            human.AddComponent(new NPCAIComponent(human, Parameters.Position.Xz(), Width * .5f));
        }
    }
}