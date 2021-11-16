using System;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Mission;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : LivableBuildingBuilder<HouseParameters>
    {
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;

        public override void Polish(HouseParameters Parameters, VillageRoot Root, Random Rng)
        {
            var position = Parameters.Position + Vector3.Transform(Vector3.UnitX * Width,
                Matrix4x4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));

            var villager = SpawnVillager(position, Rng);
            if (Utils.Rng.NextFloat() <= .4f)
            {
                villager.RemoveComponent(villager.SearchComponent<TalkComponent>());
                villager.RemoveComponent(villager.SearchComponent<ThoughtsComponent>());
                var scholars = new[]
                {
                    HumanType.Scholar.ToString().ToLowerInvariant(),
                    HumanType.Bard.ToString().ToLowerInvariant()
                };
                var questDifficulty = Array.IndexOf(scholars, villager.Type.ToLowerInvariant()) != -1
                    ? QuestTier.Medium
                    : QuestTier.Easy;
                var questDesign = MissionPool.Random(position, questDifficulty);
                villager.AddComponent(new QuestGiverComponent(villager, questDesign));
            }

            if (Rng.Next(0, 6) == 1) SpawnMob(MobType.Pug, position);
            base.Polish(Parameters, Root, Rng);
        }
    }
}