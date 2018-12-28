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
        private readonly int _perPage;
        private int _currentPage;
        private int _totalPages;

        public CraftingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns) : base(Array, 0, Rows * Columns, Columns, Vector2.One)
        {
            _player = Player;
            _perPage = Rows * Columns;
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

            var barScale = new Vector2(DefaultSize.X * Rows, DefaultSize.Y * 2) * .65f;
            var realScale = Graphics2D.SizeFromAssets("Assets/UI/InventoryBackground.png") * barScale;
            var barPosition = Vector2.UnitY * .0975f * Rows;
            var offset = Rows % 2 == 0 ? Vector2.UnitY * realScale.Y : Vector2.Zero;
            _title = new Texture("Assets/UI/InventoryBackground.png", barPosition - offset, barScale);
            _titleText = new GUIText(TitleTranslation, _title.Position, Color.White, FontCache.Get(AssetManager.BoldFamily, 12, FontStyle.Bold));
            _pageSelector = new Texture("Assets/UI/InventoryBackground.png", -barPosition - offset, barScale);
            
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

        protected virtual Translation TitleTranslation => Translation.Create("recipes");
        
        public override void UpdateView()
        {
            ResetSelector();        
            Array.Empty();
            var outputs = ArrayObjects;
            for (var i = _currentPage * _perPage; i < outputs.Length; i++)
            {
                Array.AddItem(outputs[i]);
            }
            _totalPages = Recipes.Length / _perPage + 1;
            _currentPageText.Text = $"{_currentPage + 1}/{_totalPages}";
            Renderer.UpdateView(); 
        }

        private void ResetSelector()
        {
            if (!Enabled) return;
            for (var i = 0; i < _recipeSelectedTextures.Length; i++)
            {
                _recipeSelectedTextures[i].Disable();
                Buttons[i].Texture.Grayscale = false;
            }
            _recipeSelectedTextures[SelectedIndex].Enable();
        }

        protected virtual Item[] ArrayObjects => _player.Crafting.RecipeOutputs;

        public void Reset()
        {
            _currentPage = 0;
            SelectedIndex = 0;
        }

        private void PreviousPage()
        {
            _currentPage = Mathf.Modulo(_currentPage - 1, _totalPages);
        }
        
        private void NextPage()
        {
            _currentPage = Mathf.Modulo(_currentPage + 1, _totalPages);
        }

        public int SelectedIndex { get; set; }

        private Item[] Recipes => _player.Crafting.Recipes;

        public Item CurrentOutput => Array[SelectedIndex + _perPage * _currentPage];
        public Item CurrentRecipe => Recipes[SelectedIndex + _perPage * _currentPage];
        
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