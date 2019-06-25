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
using Hedra.Localization;
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
                ? new object[] { CraftingItemName(Quest) }
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
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString(Quest.Owner))
                    .Aggregate((S1, S2) => $"{S1}{Environment.NewLine}{S2}")
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

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => new QuestDesign[]
        {
            new SpeakDesign()
        };

        protected override QuestDesign[] GetDescendants(QuestObject Quest) => null;

        public override void SetupDialog(QuestObject Quest, IPlayer Owner)
        {
            if (!Quest.FirstTime) return;
            var recipe = Quest.Parameters.Get<Item>("Recipe");
            if (!Owner.Crafting.HasRecipe(recipe.Name))
            {
                AddDialogLine(
                    Quest,
                    Translation.Create("quest_craft_take_recipe")
                );
                Owner.AddOrDropItem(recipe);
                Owner.MessageDispatcher.ShowPlaque(Translations.Get("quest_learn_recipe_plaque"), 3);
            }

            var startingItems = Quest.Parameters.Get<ItemCollect[]>("StartingItems");
            if (startingItems == null) return;
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
        }

        protected override ItemCollect[] GetItems(QuestObject Quest)
        {
            if(Quest.Previous == null)
                throw new ArgumentException("Craft designs are not suitable to be first tier quests.");
            var item = Quest.Previous.Parameters.Get<ItemCollect[]>("Items").First();
            var recipe = ItemPool.Grab(item.Recipe);
            Quest.Parameters.Set("Recipe", recipe);
            Quest.Parameters.Set("Station", recipe.GetAttribute<CraftingStation>(CommonAttributes.CraftingStation));
            Quest.Parameters.Set("StartingItems", item.StartingItems);
            var output = CraftingInventory.GetOutputFromRecipe(recipe);
            return new []
            {
                new ItemCollect
                {
                    Name = output.Name,
                    Amount = Math.Max(1, (int)(item.Amount / CraftingInventory.GetIngredients(recipe).First(I => I.Name == item.Name).Amount))
                }
            };
        }

        protected override ItemCollect[] Templates(QuestObject Quest, Random Rng) => throw new NotImplementedException();

        private bool HasCraftingStation(QuestObject Quest) => Quest.Parameters.Get<CraftingStation>("Station") != CraftingStation.None ;
        
        private static string CraftingStationName(QuestObject Quest) => Translations.Get(Station(Quest).ToString().ToLowerInvariant()).ToUpperInvariant();
        
        private static CraftingStation Station(QuestObject Object) => Object.Parameters.Get<CraftingStation>("Station");
        
        private static string CraftingItemName(QuestObject Object) => Object.Parameters.Get<ItemCollect[]>("Items").First().ToString().ToUpperInvariant();
    }
}