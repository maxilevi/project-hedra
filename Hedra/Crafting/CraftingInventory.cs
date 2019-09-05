using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Crafting.Templates;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;

namespace Hedra.Crafting
{
    public delegate void OnCraft(IngredientsTemplate[] Ingredients, Item Recipe, Item Output);
    
    public class CraftingInventory
    {
        public event OnCraft Craft;
        private const int CraftingStationRadius = 16;
        private readonly IPlayerInventory _inventory;
        private readonly List<string> _recipeNames;
        private Item[] _recipeOutputs;
        private Item[] _recipes;
                
        public CraftingInventory(IPlayerInventory Inventory)
        {
            _recipeNames = new List<string>();
            _inventory = Inventory;
        }

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
            var structs = World.InRadius<Engine.WorldBuilding.CraftingStation>(Position, CraftingStationRadius);
            var waterStation = structs.Any(S => S.StationType == CraftingStation.Well) || World.NearestWaterBlockOnChunk(Position, out _) < 12 ? CraftingStation.Water : CraftingStation.None;
            var currentStation = CraftingStation.None;
            for (var i = 0; i < structs.Length; ++i)
            {
                currentStation |= structs[i].StationType;
            }
            return currentStation | waterStation;
        }

        public void CraftItem(Item Recipe, Vector3 Position)
        {
            if(!CanCraft(Recipe, Position))
                throw new ArgumentOutOfRangeException($"Failed to craft {Recipe.Name}");
            var ingredients = GetIngredients(Recipe);
            ingredients.ToList().ForEach(
                I => _inventory.RemoveItem(_inventory.Search(T => T.Name == I.Name), I.Amount)
            );
            var output = GetOutputFromRecipe(Recipe);
            if (!_inventory.AddItem(output))
            {
                World.DropItem(output, Position);
            }
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
            if(amount > 1)
                item.SetAttribute(CommonAttributes.Amount, amount);
            return item;
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
            for (var i = 0; i < LearnedRecipes.Length; i++)
            {
                _recipeNames.Add(LearnedRecipes[i]);
            }
            UpdateRecipes();
        }

        public static IngredientsTemplate[] GetIngredients(Item Recipe)
        {
            var asJArray = Recipe.GetAttribute<JArray>(CommonAttributes.Ingredients);
            var list = new List<IngredientsTemplate>();
            foreach (var jObject in asJArray)
            {
                list.Add(JsonConvert.DeserializeObject<IngredientsTemplate>(jObject.ToString()));
            }

            return list.ToArray();
        }
        
        private void UpdateRecipes()
        {
            _recipes = _recipeNames.Select(R => ItemPool.Grab(R)).ToArray(); 
            _recipeOutputs = _recipes.Select(R => GetOutputFromRecipe(R)).ToArray();
        }
        
        public string[] RecipeNames => _recipeNames.ToArray();
        
        public Item[] Recipes => _recipes;

        public Item[] RecipeOutputs => _recipeOutputs;

    }
}