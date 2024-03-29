using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Crafting.Templates;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Crafting
{
    public delegate void OnCraft(IngredientsTemplate[] Ingredients, Item Recipe, Item Output);

    public class CraftingInventory
    {
        private const int CraftingStationRadius = 16;
        private readonly IPlayerInventory _inventory;
        private readonly List<string> _recipeNames;

        public CraftingInventory(IPlayerInventory Inventory)
        {
            _recipeNames = new List<string>();
            _inventory = Inventory;
        }

        public string[] RecipeNames => _recipeNames.ToArray();

        public Item[] Recipes { get; private set; }

        public Item[] RecipeOutputs { get; private set; }

        public event OnCraft Craft;

        public bool CanCraft(Item Recipe, Vector3 Position)
        {
            if (!IsInStation(Recipe, Position)) return false;
            var ingredients = GetIngredients(Recipe);
            return ingredients.All(I =>
                _inventory.Search(
                    T => T.Name == I.Name && T.GetAttribute<int>(CommonAttributes.Amount) >= I.Amount
                ) != null
            );
        }

        public static bool IsInStation(Item Recipe, Vector3 Position)
        {
            var station = Recipe.GetAttribute<CraftingStation>(CommonAttributes.CraftingStation);
            return (GetCurrentStation(Position) & station) == station;
        }

        public static CraftingStation GetCurrentStation(Vector3 Position)
        {
            var entities = World.InRadius<IEntity>(Position, CraftingStationRadius)
                .Where(E => E.MobType == MobType.Cow);
            var structs = World.InRadius<Engine.WorldBuilding.CraftingStation>(Position, CraftingStationRadius)
                .Where(C => C.CanCraft).ToArray();
            var waterStation =
                structs.Any(S => S.StationType == CraftingStation.Well) ||
                World.NearestWaterBlockOnChunk(Position, out _) < 12
                    ? CraftingStation.Water
                    : CraftingStation.None;
            var currentStation = CraftingStation.None;
            for (var i = 0; i < structs.Length; ++i) currentStation |= structs[i].StationType;

            if (entities.Any(E => E.MobType == MobType.Cow)) currentStation |= CraftingStation.Cow;

            return currentStation | waterStation;
        }

        public void CraftItem(Item Recipe, Vector3 Position)
        {
            if (!CanCraft(Recipe, Position))
                throw new ArgumentOutOfRangeException($"Failed to craft {Recipe.Name}");
            var ingredients = GetIngredients(Recipe);
            ingredients.ToList().ForEach(
                I => _inventory.RemoveItem(_inventory.Search(T => T.Name == I.Name), I.Amount)
            );
            var output = GetOutputFromRecipe(Recipe);
            if (!_inventory.AddItem(output)) World.DropItem(output, Position);
            Craft?.Invoke(ingredients, Recipe, output);
        }

        public static Item GetOutputFromRecipe(Item Recipe)
        {
            return GetOutputFromRecipe(Recipe, Unique.RandomSeed());
        }

        public static Item GetOutputFromRecipe(Item Recipe, int Seed)
        {
            var output = Recipe.RawAttribute("Output");
            if (output is string name) return ItemPool.Grab(name, Seed);
            var asJObject = Recipe.GetAttribute<JObject>("Output");
            var item = ItemPool.Grab((string)asJObject["Name"]);
            var amount = (int)asJObject["Amount"];
            if (amount > 1)
                item.SetAttribute(CommonAttributes.Amount, amount);
            return item;
        }

        public static Item GetRecipeThatCrafts(string Name)
        {
            var matching = ItemPool.Matching(X =>
            {
                var outputAttribute = X.Attributes.FirstOrDefault(Z => Z.Name == "Output");
                if (outputAttribute == null) return false;
                var value = outputAttribute.Value;
                if (value is string name) return name == Name;
                var asJObject = (JObject)value;
                return (string)asJObject["Name"] == Name;
            });
            return matching.Length == 0 ? null : ItemPool.Grab(matching.First().Name);
        }

        public bool LearnRecipe(string RecipeName)
        {
            if (!_recipeNames.Contains(RecipeName))
            {
                _recipeNames.Add(RecipeName);
                UpdateRecipes();
                return true;
            }

            return false;
        }

        public bool HasRecipe(string RecipeName)
        {
            return _recipeNames.Contains(RecipeName);
        }

        public void SetRecipes(string[] LearnedRecipes)
        {
            _recipeNames.Clear();
            for (var i = 0; i < LearnedRecipes.Length; i++) _recipeNames.Add(LearnedRecipes[i]);
            UpdateRecipes();
        }

        public static IngredientsTemplate[] GetIngredients(Item Recipe)
        {
            var asJArray = Recipe.GetAttribute<JArray>(CommonAttributes.Ingredients);
            var list = new List<IngredientsTemplate>();
            foreach (var jObject in asJArray)
                list.Add(JsonConvert.DeserializeObject<IngredientsTemplate>(jObject.ToString()));

            return list.ToArray();
        }

        private void UpdateRecipes()
        {
            Recipes = _recipeNames.Select(R => ItemPool.Grab(R)).ToArray();
            RecipeOutputs = Recipes.Select(R => GetOutputFromRecipe(R)).ToArray();
        }
    }
}