using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.API;
using Hedra.Components;
using Hedra.Crafting.Templates;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Mission;
using CraftingStation = Hedra.Crafting.CraftingStation;

namespace Hedra.Engine.Steamworks
{
    public static class AchievementsObserver
    {
        private static HashSet<string> CompletedAchievements = new HashSet<string>();
        /* Hooks into necessary events */
        public static void Initialize()
        {
            CreateCompletedMap();
            LocalPlayer.Instance.Kill += OnPlayerKill;
            LocalPlayer.Instance.Crafting.Craft += OnCraft;
            LocalPlayer.Instance.AbilityTree.SpecializationLearned += OnSpecializationLearned;
            LocalPlayer.Instance.Inventory.InventoryUpdated += OnInventoryUpdated;
            LocalPlayer.Instance.Questing.QuestCompleted += OnQuestCompleted;
            LocalPlayer.Instance.Companion.CompanionChanged += OnCompanionChanged;
            LocalPlayer.Instance.Trade.TransactionComplete += OnTransactionComplete;
            LocalPlayer.Instance.Realms.RealmChanged += OnRealmChanged;
            LocalPlayer.Instance.OnDeath += OnPlayerOnDeath;
            LocalPlayer.Instance.Fishing += OnItemFished;
            LocalPlayer.Instance.Interact += PlayerSleep;

        }
        
        private static void OnCraft(IngredientsTemplate[] Ingredients, Item Recipe, Item Output)
        {
            var station = Recipe.GetAttribute<CraftingStation>(CommonAttributes.CraftingStation);
            if (station == CraftingStation.Campfire)
            {
                TriggerAchievementIfNecessary(Achievement.ACH_CHEF);
            }
            if (string.Equals(Output.Name, ItemType.Boat.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                TriggerAchievementIfNecessary(Achievement.ACH_FLUID_DYNAMICS);
            }
            AddStat(Statistic.CRAFT_COUNT, 1);
        }

        private static void OnPlayerKill(DeadEventArgs Args)
        {
            if (Args.DamageType == DamageType.Fire)
            {
                TriggerAchievementIfNecessary(Achievement.ACH_PYROMANIAC);
            }
            else if (Args.DamageType == DamageType.Bleed)
            { 
                TriggerAchievementIfNecessary(Achievement.ACH_DEEP_CUT);
            }
            AddStat(Statistic.KILL_COUNT, 1);
        }

        private static void OnPlayerOnDeath(DeadEventArgs Args)
        {
            if (Args.DamageType == DamageType.FoodPoison)
            {
                TriggerAchievementIfNecessary(Achievement.ACH_LESSON_LEARNED);
            }
            AddStat(Statistic.DEATH_COUNT, 1);
        }

        private static void OnSpecializationLearned(AbilityTreeBlueprint Blueprint)
        {
            var name = Blueprint.Identifier;

            bool Compare(string A, Class B)
            {
                return string.Equals(A, B.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            var map = new Dictionary<Class, Achievement>()
            {
                {Class.Assassin, Achievement.ACH_LEARN_ASSASSIN},
                {Class.Berserker, Achievement.ACH_LEARN_BERSERKER},
                {Class.Druid, Achievement.ACH_LEARN_DRUID},
                {Class.Hunter, Achievement.ACH_LEARN_HUNTER},
                {Class.Necromancer, Achievement.ACH_LEARN_NECROMANCER},
                {Class.Ninja, Achievement.ACH_LEARN_NINJA},
                {Class.Paladin, Achievement.ACH_LEARN_PALADIN},
                {Class.Scout, Achievement.ACH_LEARN_SCOUT}
            };
            
            foreach(var pair in map)
            {
                if(Compare(name, pair.Key))
                    TriggerAchievementIfNecessary(pair.Value);
            }

            var hasAll = true;
            foreach (var pair in map)
            {
                hasAll &= HasAchievement(pair.Value);
            }
            if (hasAll) TriggerAchievementIfNecessary(Achievement.ACH_JACK_OF_ALL_TRADES);
        }
        
        private static void OnInventoryUpdated()
        {
            var hat = LocalPlayer.Instance.Inventory.Helmet;
            if(hat != null) TriggerAchievementIfNecessary(Achievement.ACH_FASHIONABLE);
        }

        private static void OnCompanionChanged(Item CompanionItem, IEntity Companion)
        {
            if(CompanionItem == null) return;
            if(string.Equals(CompanionItem.Name, ItemType.CompanionBee.ToString(), StringComparison.InvariantCultureIgnoreCase))
                TriggerAchievementIfNecessary(Achievement.ACH_BEE_MOVIE);
        }

        private static void OnQuestCompleted(MissionObject Quest)
        {
            if (!Quest.HasNext)
            {
                AddStat(Statistic.QUEST_COUNT, 1);
            }
        }

        private static void PlayerSleep(InteractableStructure Structure)
        {
            if (Structure is SleepingPad pad && pad.Sleeper == LocalPlayer.Instance)
                TriggerAchievementIfNecessary(Achievement.ACH_BED_THIEF);
        } 

        private static void OnTransactionComplete(Item Object, int Price, TransactionType Type)
        {
            if (Type == TransactionType.Buy)
            {
                AddStat(Statistic.GOLD_SPENT, Price);
            }
        }

        private static void OnRealmChanged(WorldType NewType)
        {
            TriggerAchievementIfNecessary(Achievement.ACH_DIMENSION_TRAVELLER);
        }

        private static void OnItemFished(Item Object)
        {
            AddStat(Statistic.FISH_COUNT, 1);
        }
        
        private static void TriggerAchievementIfNecessary(Achievement Achievement)
        {
            if(HasAchievement(Achievement)) return;
            Steam.Instance.Client.Achievements.Trigger(Achievement.ToString());
            CreateCompletedMap();
        }

        private static void AddStat(Statistic Stat, int Amount)
        {
            Steam.Instance.Client.Stats.Add(Stat.ToString(), Amount);
        }

        private static bool HasAchievement(Achievement Achievement)
        {
            return CompletedAchievements.Contains(Achievement.ToString());
        }

        private static void CreateCompletedMap()
        {
            CompletedAchievements.Clear();
            var all = Steam.Instance.Client.Achievements.All;
            for (var i = 0; i < all.Length; ++i)
            {
                if (all[i].State)
                    CompletedAchievements.Add(all[i].Id);
            }
        }
    }
}