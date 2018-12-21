using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.CraftingSystem;
using Hedra.Engine.Input;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventory : PlayerInterface
    {
        private const int RecipeSpaces = 5; 
        public override Key OpeningKey => Controls.Crafting;
        private readonly IPlayer _player;       
        private readonly InventoryArray _recipeItems;
        private readonly InventoryArrayInterface _recipesItemInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly CraftingInventoryArrayInterfaceManager _interfaceManager;
        private readonly CraftingBackground _craftingBackground;
        private Item[] _allRecipeItems;
        private bool _show;
        
        
        public CraftingInventory(IPlayer Player)
        {
            _player = Player;
            _recipeItems = new InventoryArray(RecipeSpaces);
            _stateManager = new InventoryStateManager(_player);
            _recipesItemInterface = new InventoryArrayInterface(_recipeItems, 0, _recipeItems.Length, 1, Vector2.One)
            {
                Position = Vector2.UnitX * -.2f + Vector2.UnitY * .25f
            };
            var itemInfoInterface = new CraftingInventoryItemInfo(_recipesItemInterface.Renderer)
            {
                Position = Vector2.UnitY * _recipesItemInterface.Position.Y + Vector2.UnitX * .2f
            };
            _interfaceManager = new CraftingInventoryArrayInterfaceManager(itemInfoInterface, _recipesItemInterface);
            _craftingBackground = new CraftingBackground(
                _recipesItemInterface.Position - InventoryArrayInterface.DefaultSize * 5 * Vector2.UnitY,
                InventoryArrayInterface.DefaultSize * 5 * new Vector2(1, .75f)
            );
            _stateManager.OnStateChange += State =>
            {
                base.Invoke(State);
            };
        }


        public void UpdateView()
        {
            _interfaceManager.UpdateView();
            _recipesItemInterface.UpdateView();
            _craftingBackground.UpdateView();
        }

        private void SetInventoryState(bool State)
        {
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                Cursor.Show = true;
            }
            else
            {
                _stateManager.ReleaseState();
            }
        }

        private void SetActive(bool Value)
        {
            if (_show == Value || _stateManager.GetState() != _show) return;
            _show = Value;
            _recipesItemInterface.Enabled = _show;
            _interfaceManager.Enabled = _show;
            _craftingBackground.Enabled = _show;
            SetInventoryState(_show);
            UpdateView();
        }

        public string[] Recipes { get; private set; }

        public void SetRecipes(string[] LearnedRecipes)
        {
            Recipes = LearnedRecipes;
            _allRecipeItems = RecipePool.GetResults(Recipes);
            _recipeItems.SetItems(_player.Inventory.ItemsToArray().Take(RecipeSpaces).ToArray());
        }
        
        public override bool Show
        {
            get => _show;
            set => SetActive(value);
        }
    }
}