using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.CraftingSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.CraftingSystem
{
    public class CraftingInventoryArrayInterface : InventoryArrayInterface
    {
        private readonly RenderableTexture[] _recipeSelectedTextures;
        private readonly Texture _title;
        private readonly GUIText _titleText;
        private readonly Texture _pageSelector;
        private readonly GUIText _currentPageText;
        private readonly Button _previousPage;
        private readonly Button _nextPage;
        private readonly Panel _panel;
        private readonly IPlayer _player;
        private readonly int _recipesPerPage;
        private int _currentPage;
        private int _totalPages;

        public CraftingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Columns, int Rows) : base(Array, 0, Columns * Rows, Rows, Vector2.One)
        {
            _player = Player;
            _recipesPerPage = Columns * Rows;
            _panel = new Panel
            {
                DisableKeys = true
            };
            _recipeSelectedTextures = new RenderableTexture[Buttons.Length];
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                _recipeSelectedTextures[i] = 
                    new RenderableTexture(
                        new Texture(Graphics2D.LoadFromAssets("Assets/UI/SelectedInventorySlot.png"), this.Textures[i].Position, this.Textures[i].Scale),
                        DrawOrder.After
                    );
                _recipeSelectedTextures[i].BaseTexture.TextureElement.MaskId = DefaultId;
                _panel.AddElement(_recipeSelectedTextures[i]);
            }
            _title = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * .35f, new Vector2(DefaultSize.X * Columns, DefaultSize.Y * 2) * .65f);
            _titleText = new GUIText(Translation.Create("recipes"), _title.Position, Color.White, FontCache.Get(AssetManager.BoldFamily, 12, FontStyle.Bold));
            _pageSelector = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * -.45f, new Vector2(DefaultSize.X * Columns, DefaultSize.Y * 2) * .65f);
            
            _currentPageText = new GUIText("00/00", _pageSelector.Position, Color.White, FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
            var footerFont = FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold);
            _previousPage = new Button(_currentPageText.Position - Vector2.UnitX * _currentPageText.Scale.X * 2, Vector2.One, "\u25C0", Color.White, footerFont);
            _previousPage.Click += (O, E) => PreviousPage();
            _nextPage = new Button(_currentPageText.Position + Vector2.UnitX * _currentPageText.Scale.X * 2, Vector2.One, "\u25B6", Color.White, footerFont);
            _previousPage.Click += (O, E) => NextPage();
            
            _panel.AddElement(_currentPageText);
            _panel.AddElement(_previousPage);
            _panel.AddElement(_nextPage);
            _panel.AddElement(_pageSelector);
            _panel.AddElement(_title);
            _panel.AddElement(_titleText);
        }
        
        public override void UpdateView()
        {
            if (Enabled)
            {
                for (var i = 0; i < _recipeSelectedTextures.Length; i++)
                {
                    _recipeSelectedTextures[i].Disable();
                    Buttons[i].Texture.Grayscale = false;
                }
                _recipeSelectedTextures[SelectedRecipeIndex].Enable();
            }
            
            Array.Empty();
            var outputs = _player.Crafting.RecipeOutputs;
            for (var i = _currentPage * _recipesPerPage; i < outputs.Length; i++)
            {
                Array.AddItem(outputs[i]);
            }
            /*for(var i = 0; i < Array.Length; i++)
            {
                if (Array[i] == null) continue;
                var recipe = _player.Crafting.Recipes[System.Array.IndexOf(_player.Crafting.RecipeOutputs, Array[i])];
                if (CraftingInventory.IsInStation(recipe, _player.Position)) continue;
                Buttons[i].Texture.Grayscale = true;
                Buttons[i].Texture.Tint = new Vector4(Vector3.One * .5f, 1);
            }*/
            _totalPages = Recipes.Length / _recipesPerPage + 1;
            _currentPageText.Text = $"{_currentPage + 1}/{_totalPages}";
            Renderer.UpdateView();           
        }

        public void Reset()
        {
            _currentPage = 0;
            SelectedRecipeIndex = 0;
        }

        private void PreviousPage()
        {
            _currentPage = Mathf.Modulo(_currentPage - 1, _totalPages);
        }
        
        private void NextPage()
        {
            _currentPage = Mathf.Modulo(_currentPage + 1, _totalPages);
        }

        public int SelectedRecipeIndex { get; set; }

        private Item[] Recipes => _player.Crafting.Recipes;

        public Item CurrentOutput => Array[SelectedRecipeIndex + _recipesPerPage * _currentPage];
        public Item CurrentRecipe => Recipes[SelectedRecipeIndex + _recipesPerPage * _currentPage];
        
        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();            
            }
        }

        public override Vector2 Scale
        {
            get => base.Scale;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Scale = 
                        new Vector2(elements[i].Scale.X / base.IndividualScale.X, elements[i].Scale.Y / base.IndividualScale.Y) * value;
                    var relativePosition = elements[i].Position - Position;
                    elements[i].Position = 
                        new Vector2(relativePosition.X / base.Scale.X, relativePosition.Y / base.Scale.Y) * value + Position;
                }
                base.Scale = value;
            }
        }

        public override Vector2 IndividualScale
        {
            get => base.IndividualScale;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Scale = new Vector2(elements[i].Scale.X / base.IndividualScale.X,
                                            elements[i].Scale.Y / base.IndividualScale.Y) * value;
                }
                base.IndividualScale = value;
            }
        }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Position = elements[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }      
    }
}