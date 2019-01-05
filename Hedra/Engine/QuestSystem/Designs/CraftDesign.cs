using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.CraftingSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CraftDesign : BaseItemQuestDesign
    {
        public override string Name => "CraftingQuest";

        public override string GetThoughtsKeyword(QuestObject Quest)
        {
            return HasCraftingStation(Quest)
                ? "quest_craft_dialog" 
                : "quest_craft_anywhere_dialog";
        }

        public override object[] GetThoughtsParameters(QuestObject Quest)
        {
            return Quest.Parameters.Get<CraftingStation>("Station") == CraftingStation.None 
                ? new object[] { CraftingItemName(Quest).ToString() }
                : new object[]
                {
                    CraftingItemName(Quest),
                    CraftingStationName(Quest)
                };
        }

        public override string GetShortDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_craft_short",
                new object[]
                {
                    CraftingItemName(Quest),
                }
            );
        }

        public override string GetDescription(QuestObject Quest)
        {
            var arguments = new List<object>(new object[]
            {
                Quest.Giver.Name,
                CraftingItemName(Quest)
            });
            var hasStation = HasCraftingStation(Quest);
            if(hasStation) arguments.Add(CraftingStationName(Quest));
            return Translations.Get(
                hasStation
                    ? "quest_craft_description"
                    : "quest_craft_anywhere_description",
                arguments.ToArray()
            );                    
        }

        protected override QuestDesign[] Auxiliaries => new QuestDesign[]
        {
            new SpeakDesign(),
            new TravelDesign()
        };

        protected override QuestDesign[] Descendants => null;

        protected override QuestObject Setup(QuestObject Quest)
        {
            if (!Quest.FirstTime) return base.Setup(Quest);
            var recipe = Quest.Parameters.Get<Item>("Recipe");
            if (!Quest.Owner.Crafting.HasRecipe(recipe.Name))
            {
                Quest.Owner.AddOrDropItem(recipe);
            }

            var startingItems = Quest.Parameters.Get<ItemCollect[]>("StartingItems");
            if (startingItems == null) return base.Setup(Quest);
            for (var i = 0; i < startingItems.Length; ++i)
            {
                var startItem = ItemPool.Grab(startingItems[i].Name);
                startItem.SetAttribute(CommonAttributes.Amount, startingItems[i].Amount);
                AddDialogLine(
                    Quest,
                    Translation.Create("quest_craft_take_item", startingItems[i].ToString().ToUpperInvariant())
                );
                Quest.Owner.AddOrDropItem(startItem);
            }
            return base.Setup(Quest);
        }

        protected override ItemCollect[] GetItems(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            if(Previous == null)
                throw new ArgumentException("Craft designs are not suitable to be first tier quests.");
            var item = Previous.Parameters.Get<ItemCollect[]>("Items").First();
            var recipe = ItemPool.Grab(item.Recipe);
            Parameters.Set("Recipe", recipe);
            Parameters.Set("Station", recipe.GetAttribute<CraftingStation>(CommonAttributes.CraftingStation));
            Parameters.Set("StartingItems", item.StartingItems);
            var output = ItemPool.Grab(recipe.GetAttribute<string>(CommonAttributes.Output));
            return new []
            {
                new ItemCollect
                {
                    Name = output.Name,
                    Amount = Rng.Next(1, 4)
                }
            };
        }
// Charge and fire
        // Right click kick ability

        protected override Item RandomReward(Random Rng)
        {
            Item[] items;
            if (Rng.Next(0, 7) == 1)
                items = ItemPool.Matching(T => T.IsRecipe && (int) T.Tier == (int) ItemTier.Uncommon);
            else if (Rng.Next(0, 4) == 1)
                items = ItemPool.Matching(T => T.IsRecipe && (int) T.Tier == (int) ItemTier.Common);
            else
                items = ItemPool.Matching(T => T.Tier == ItemTier.Misc);

            return items[Rng.Next(0, items.Length)];
        }

        protected override int RandomItemCount(Random Rng, ItemCollect[] Templates) => throw new NotImplementedException();

        protected override ItemCollect[] SpawnTemplates(Random Rng) => throw new NotImplementedException();
        
        protected override ItemCollect[] VillageTemplates(Random Rng) => throw new NotImplementedException();

        protected override ItemCollect[] WildernessTemplates(Random Rng) => throw new NotImplementedException();

        private bool HasCraftingStation(QuestObject Quest) => Quest.Parameters.Get<CraftingStation>("Station") != CraftingStation.None ;
        
        private static string CraftingStationName(QuestObject Quest) => Translations.Get(Station(Quest).ToString().ToLowerInvariant()).ToUpperInvariant();
        
        private static CraftingStation Station(QuestObject Object) => Object.Parameters.Get<CraftingStation>("Station");
        
        private static string CraftingItemName(QuestObject Object) => Object.Parameters.Get<ItemCollect[]>("Items").First().ToString().ToUpperInvariant();
    }
}