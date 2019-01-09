using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public static class QuestPersistence
    {
        private static readonly List<IHumanoid> _registeredNpcs = new List<IHumanoid>();
        
        public static IHumanoid BuildQuestVillager(GiverTemplate Template)
        {
            var villager = World.WorldBuilding.SpawnVillager(Template.Position, Template.Seed);
            villager.ShowIcon(CacheItem.AttentionIcon);
            _registeredNpcs.Add(villager);
            return villager;
        }

        public static bool SpawnVillager(Vector3 Position, Random Rng, out IHumanoid Humanoid)
        {
            Humanoid = _registeredNpcs.FirstOrDefault(V => (V.Physics.TargetPosition - Position).Xz.LengthSquared < 1);
            if (Humanoid != null)
            {
                return false;
            }
            Humanoid = World.WorldBuilding.SpawnVillager(Position, Rng);
            return true;
        }

        public static void SetupQuest(QuestObject Quest, IHumanoid Giver)
        {
            World.StructureHandler.AddStructure(
                new CollidableStructure(
                    new QuestStructureDesign(Quest),
                    Quest.Giver.Position,
                    null,
                    new QuestStructure(Quest.Giver.Position, Quest.Giver)
                )
                {
                    Built = true
                }
            );
            Giver.AddComponent(new QuestGiverComponent(Giver, Quest));
        }

        public static void UnregisterNPC(IHumanoid Giver)
        {
            _registeredNpcs.Remove(Giver);
        }

        public static void Discard()
        {
            _registeredNpcs.Clear();
        }
    }
}