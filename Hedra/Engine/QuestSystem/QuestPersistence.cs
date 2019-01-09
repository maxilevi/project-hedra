using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CacheSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public static class QuestPersistence
    {
        private static List<Vector3> _registeredNpcs;
        
        public static IHumanoid BuildQuestVillager(GiverTemplate Template)
        {
            var villager = World.WorldBuilding.SpawnVillager(Template.Position, Template.Seed);
            villager.ShowIcon(CacheItem.AttentionIcon);
            _registeredNpcs.Add(villager.Position);
            return villager;
        }

        public static bool SpawnVillager(Vector3 Position, Random Rng, out IHumanoid Humanoid)
        {
            Humanoid = null;
            if (_registeredNpcs.Any(V => (V - Position).Xz.LengthSquared < 1)) return false;
            Humanoid = World.WorldBuilding.SpawnVillager(Position, Rng);
            return true;
        }

        public static void Discard()
        {
            _registeredNpcs.Clear();
        }
    }
}