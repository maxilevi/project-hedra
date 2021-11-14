using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.API;
using Hedra.Components;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Game;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public static class NPCCreator
    {
        public static INPCCreatorProvider Provider { get; set; } = new NPCCreatorProvider();

        public static Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return Provider.SpawnHumanoid(Type, DesiredPosition);
        }
        
        public static Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return Provider.SpawnHumanoid(Type, DesiredPosition);
        }
        
        public static Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng)
        {
            return Provider.SpawnVillager(DesiredPosition, Rng);
        }
        
        public static Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed)
        {
            return Provider.SpawnVillager(DesiredPosition, Seed);
        }
        
        public static Humanoid SpawnVillager(HumanType Type, Vector3 DesiredPosition, int Seed)
        {
            return Provider.SpawnVillager(Type, DesiredPosition, Seed);
        }
        
        public static IHumanoid SpawnQuestGiver(HumanType Type, Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return Provider.SpawnQuestGiver(Type, Position, Quest, Rng);
        }
        
        public static IHumanoid SpawnQuestGiver(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return Provider.SpawnQuestGiver(Position, Quest, Rng);
        }
        
        public static Humanoid SpawnBandit(Vector3 Position, int Level, BanditOptions Options)
        {
            return Provider.SpawnBandit(Position, Level, Options);
        }

    }
}