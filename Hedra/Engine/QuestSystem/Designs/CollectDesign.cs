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
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CollectDesign : BaseItemQuestDesign
    {
        public override string Name => "CollectQuest";

        public override string GetThoughtsKeyword(QuestObject Quest) => "quest_collect_dialog";

        public override object[] GetThoughtsParameters(QuestObject Quest)
        {
            return new object[]
            {
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString())
                    .Aggregate((S1, S2) => $"{S1}, {S2}")
                    .ToUpperInvariant()
            };
        }

        public override string GetShortDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_collect_short",
                Quest.Giver.Name,
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString())
                    .Aggregate((S1, S2) => $"{S1}{S2}")
            );
        }

        public override string GetDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_collect_description",
                Quest.Giver.Name,
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString(Quest.Owner))
                    .Aggregate((S1, S2) => $"{S1}{Environment.NewLine}{S2}")
            );
        }

        protected override QuestDesign[] GetDescendants(QuestObject Quest) => new QuestDesign[]
        {
            Quest.Parameters.Get<ItemCollect[]>("Items").All(I => I.Recipe != null) 
                ? new CraftDesign() 
                : null
        };

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => new QuestDesign[]
        {
            new SpeakDesign()
        };
        
        private static string RecipeFromItem(string ItemName, Item[] Recipes, Random Rng)
        {
            var possibleRecipes = Recipes.Where(R => CraftingInventory.GetIngredients(R).Any(I => I.Name == ItemName)).ToArray();
            return possibleRecipes.Length > 0 ? possibleRecipes[Rng.Next(0, possibleRecipes.Length)].Name : null;
        }

        private static int AmountFromItem(Item MiscItem, Random Rng)
        {
            if (!MiscItem.HasAttribute(CommonAttributes.Price))
                throw new ArgumentOutOfRangeException($" Item '{MiscItem.DisplayName}' does not have a price attribute.");
            return Rng.Next((int)Math.Round(18f / MiscItem.GetAttribute<int>(CommonAttributes.Price)), (int)Math.Round(42f / MiscItem.GetAttribute<int>(CommonAttributes.Price)));
        }
        
        protected override ItemCollect[] Templates(QuestObject Object, Random Rng)
        {
            var possibleItems = ItemPool.Matching(T => T.Tier == ItemTier.Misc && T.Name != ItemType.Gold.ToString());
            var recipes = ItemPool.Matching(T => T.EquipmentType == EquipmentType.Recipe.ToString());
            var ownerRecipes = Object.Owner.Crafting.RecipeOutputs.Select(S => S.Name);
            possibleItems = possibleItems.Where(
                I => recipes.All(R => CraftingInventory.GetOutputFromRecipe(R).Name != I.Name || ownerRecipes.Contains(I.Name))
            ).ToArray();
            return possibleItems.Select(I => new ItemCollect
            {
                Name = I.Name,
                Amount = AmountFromItem(I, Rng),
                Recipe = RecipeFromItem(I.Name, recipes, Rng)
            }).ToArray();
        }
    }
}