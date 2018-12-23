using System;
using System.Collections.Generic;
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
        private const int RecipeSpaces = 20; 
        public override Key OpeningKey => Controls.Crafting;
        private readonly IPlayer _player;       
        private readonly InventoryArray _recipeItems;
        private readonly CraftingInventoryArrayInterface _recipesItemInterface;
        private readonly InventoryStateManager _stateManager;
        private readonly CraftingInventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _background;
        private readonly CraftingBackground _craftingDescription;
        private readonly List<string> _recipes;
        private Item[] _allRecipeItems;
        private bool _show;
        
        
        public CraftingInventory(IPlayer Player)
        {
            _player = Player;
            _recipes = new List<string>();
            _recipeItems = new InventoryArray(RecipeSpaces);
            _stateManager = new InventoryStateManager(_player);
            _background = new InventoryBackground(Vector2.UnitY * .65f);
            var interfacePosition = Vector2.UnitX * -.4f + Vector2.UnitY * .05f;
            _recipesItemInterface = new CraftingInventoryArrayInterface(_recipeItems, _recipeItems.Length)
            {
                Position = interfacePosition
            };
            var itemInfoInterface = new CraftingInventoryItemInfo(_recipesItemInterface.Renderer)
            {
                Position = Vector2.UnitY * _recipesItemInterface.Position.Y + interfacePosition.X * -Vector2.UnitX
            };
            _craftingDescription = new CraftingBackground(
                new Vector2(0, -itemInfoInterface.Scale.Y * 2),
                new Vector2(interfacePosition.X * -2, itemInfoInterface.Scale.Y * 2)
            );
            _interfaceManager = new CraftingInventoryArrayInterfaceManager(itemInfoInterface, _recipesItemInterface);
            _stateManager.OnStateChange += Invoke;
        }

        private void UpdateView()
        {
            _interfaceManager.UpdateView();
            _recipesItemInterface.UpdateView();
            _craftingDescription.UpdateView();
            _background.UpdateView(_player);
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

        public void Update()
        {
            if (_show)
            {
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float) Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float) Math.Acos(-_player.Orientation.X),
                    Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    Time.DeltaTime * 16f);
            }
        }
        
        private void SetActive(bool Value)
        {
            if (_show == Value || _stateManager.GetState() != _show) return;
            _show = Value;
            _recipesItemInterface.Enabled = _show;
            _interfaceManager.Enabled = _show;
            _craftingDescription.Enabled = _show;
            _background.Enabled = _show;
            SetInventoryState(_show);
            UpdateView();
        }

        public string[] GetRecipes()
        {
            return _recipes.ToArray();
        }

        public bool LearnRecipe(string RecipeName)
        {
            if (!_recipes.Contains(RecipeName))
            {
                _recipes.Add(RecipeName);
                return true;
            }
            return false;
        }

        public bool HasRecipe(string RecipeName)
        {
            return _recipes.Contains(RecipeName);
        }
        
        public void SetRecipes(string[] LearnedRecipes)
        {
            _recipeItems.Empty();
            _recipes.Clear();
            _recipes.AddRange(LearnedRecipes);
            _allRecipeItems = RecipePool.GetOutputs(GetRecipes());
            _allRecipeItems.ToList().ForEach(I => _recipeItems.AddItem(I));
        }
        
        public override bool Show
        {
            get => _show;
            set => SetActive(value);
        }

        protected override bool Disabled => true;
    }
}