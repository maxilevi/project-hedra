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
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : LivableBuildingBuilder<HouseParameters>
    {
        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;
        private float _width;
        
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override void Polish(HouseParameters Parameters, VillageRoot Root, Random Rng)
        {
            var position = Parameters.Position + Vector3.TransformPosition(Vector3.UnitX * _width,
                               Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            
            if (Rng.Next(0, 4) == 1)
            {
                var villager = SpawnVillager(position, Rng);
                if (Utils.Rng.Next(0, 5) == 1)
                {
                    villager.RemoveComponent(villager.SearchComponent<TalkComponent>());
                    villager.RemoveComponent(villager.SearchComponent<ThoughtsComponent>());
                    villager.AddComponent(
                        new QuestGiverComponent(villager, QuestPool.Grab(QuestTier.Easy).Build(villager.Position, Utils.Rng, villager))
                    );
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