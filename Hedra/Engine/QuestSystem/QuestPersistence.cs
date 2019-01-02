using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public static class QuestPersistence
    {
        private static HashSet<Vector3> _registeredNpcs;
        
        public static IHumanoid BuildQuestVillager(GiverTemplate Template)
        {
            var villager = World.WorldBuilding.SpawnVillager(Template.Position, Template.Seed);
            villager.ShowIcon(CacheItem.AttentionIcon);
            return villager;
        }
    }
}