using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.CraftingSystem
{
    public static class RecipePool
    {
        public static Item[] GetResults(params string[] Recipes)
        {
            return GetIngredients(Recipes.Select(ItemPool.Grab).ToArray()).Select(I => ItemPool.Grab(I.Name)).ToArray();
        }
        
        public static IngredientsTemplate[] GetIngredients(params Item[] Recipes)
        {
            return Recipes.Select(R => R.GetAttribute<IngredientsTemplate>(CommonAttributes.Ingredients)).ToArray();
        }
    }
}