using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Numerics;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterface : PagedInventoryArrayInterface
    {
        public CraftingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns)
            : base(Player, Array, Rows, Columns, Vector2.One)
        {
        }

        protected override Translation TitleTranslation => Translation.Create("recipes");

        private Item[] Recipes => Player.Crafting.Recipes;

        public Item CurrentOutput => Array[SelectedIndex];

        public Item CurrentRecipe => Recipes[(int)Mathf.Clamp(SelectedIndex + PerPage * CurrentPage, 0, Recipes.Length-1)];

        protected override Item[] ArrayObjects => Player.Crafting.RecipeOutputs;
    }
}