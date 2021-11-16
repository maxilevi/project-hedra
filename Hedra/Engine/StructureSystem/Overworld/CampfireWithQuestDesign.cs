using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CampfireWithQuestDesign : CampfireDesign
    {
        public override VertexData Icon => null;
        public override bool CanSpawnInside => true;

        public override void Build(CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            Structure.Parameters.Set("HasCauldron", rng.Next(0, 3) == 1);
            base.Build(Structure);
            if (Structure.Parameters.Get<bool>("HasCauldron"))
            {
                var scale = 3f;
                var model = DynamicCache.Get("Assets/Env/Objects/Cauldron.ply", Vector3.One * scale);
                var shapes = DynamicCache.GetShapes("Assets/Env/Objects/Cauldron.ply", Vector3.One * scale);

                model.Translate(Structure.Position);
                for (var i = 0; i < shapes.Count; ++i)
                {
                    shapes[i].Transform(Structure.Position);
                    Structure.AddCollisionShape(shapes[i]);
                }

                Structure.AddStaticElement(model);
            }

            ((Campfire)Structure.WorldObject).SetCanCraft(false);
            //((Campfire) Structure.WorldObject).HasFire = false;
        }

        protected override IHumanoid CreateCampfireNPC(CollidableStructure Structure, Vector3 Position, Random Rng)
        {
            var hasCauldron = Structure.Parameters.Get<bool>("HasCauldron");
            var quest = hasCauldron
                ? MissionPool.Grab(Quests.CraftAPotion)
                : MissionPool.Random(Position, QuestTier.Medium);
            var npc = NPCCreator.SpawnQuestGiver(Position, quest, Rng);
            return npc;
        }
    }
}