using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Items;
using OpenTK;

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

        public Item CurrentOutput => Array[SelectedIndex + PerPage * CurrentPage];
        
        public Item CurrentRecipe => Recipes[SelectedIndex + PerPage * CurrentPage];
        
        protected override Item[] ArrayObjects => Player.Crafting.RecipeOutputs;
    }
}