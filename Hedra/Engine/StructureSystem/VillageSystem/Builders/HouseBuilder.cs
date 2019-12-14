using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Mission;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : LivableBuildingBuilder<HouseParameters>
    {
        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;

        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override void Polish(HouseParameters Parameters, VillageRoot Root, Random Rng)
        {
            var position = Parameters.Position + Vector3.Transform(Vector3.UnitX * Width,
                               Matrix4x4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            
            if (Rng.Next(0, 2) == 1)
            {
                var villager = SpawnVillager(position, Rng);
                if (Utils.Rng.NextFloat() < .4f)
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
            }
            else if (Rng.Next(0, 6) == 1)
            {
                SpawnMob(MobType.Pug, position);
            }
            base.Polish(Parameters, Root, Rng);
        }
    }
}