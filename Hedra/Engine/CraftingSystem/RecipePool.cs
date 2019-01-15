using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.CraftingSystem
{
    public static class RecipePool
    {
        public static Item[] GetOutputs(params string[] Recipes)
        {
            return Recipes.Select(ItemPool.Grab).Select(I => ItemPool.Grab(I.GetAttribute<string>(CommonAttributes.Output))).ToArray();
        }
    }
}