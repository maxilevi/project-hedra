using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;

namespace Hedra.Engine.CraftingSystem
{
    public class CraftingInventory
    {
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
            return (GetCurrentStation(Position) & station) != 0;
        }

        public static CraftingStation GetCurrentStation(Vector3 Position)
        {
            var structs = World.InRadius<WorldBuilding.CraftingStation>(Position, CraftingStationRadius);
            var currentStation = CraftingStation.None;
            var nearWater = structs.Any(
                S => S is Well
            );
            var nearFire = structs.Any(
                S => S is Campfire
            );
            var nearAnvil = structs.Any(
                S => S is Anvil
            );
            if (nearWater) currentStation |= CraftingStation.Well;
            if (nearFire) currentStation |= CraftingStation.Campfire;
            if (nearAnvil) currentStation |= CraftingStation.Anvil;
            return currentStation;
        }

        public void Craft(Item Recipe, Vector3 Position)
        {
            if(!CanCraft(Recipe, Position))
                throw new ArgumentOutOfRangeException($"Failed to craft {Recipe.Name}");
            var ingredients = GetIngredients(Recipe);
            ingredients.ToList().ForEach(
                I => _inventory.RemoveItem(_inventory.Search(T => T.Name == I.Name), I.Amount)
            );
            var output = ItemPool.Grab(Recipe.GetAttribute<string>(CommonAttributes.Output), Unique.RandomSeed);
            if (!_inventory.AddItem(output))
            {
                World.DropItem(output, Position);
            }
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
            _recipeOutputs = _recipes.Select(I => ItemPool.Grab(I.GetAttribute<string>(CommonAttributes.Output))).ToArray();
        }
        
        public string[] RecipeNames => _recipeNames.ToArray();
        
        public Item[] Recipes => _recipes;

        public Item[] RecipeOutputs => _recipeOutputs;

    }
}