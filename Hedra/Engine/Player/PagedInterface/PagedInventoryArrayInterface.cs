using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.PagedInterface
{
    public abstract class PagedInventoryArrayInterface : InventoryArrayInterface
    {
        protected Texture[] SelectedTextures { get; }
        protected Panel Panel { get; }
        protected IPlayer Player { get; }
        protected int PerPage { get; }
        protected int CurrentPage { get; private set; }
        protected int TotalPages { get; private set; }
        private readonly Texture _title;
        private readonly GUIText _titleText;
        private readonly Texture _pageSelector;
        private readonly GUIText _currentPageText;
        private readonly Button _previousPage;
        private readonly Button _nextPage;

        protected PagedInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns, Vector2 Spacing) 
            : base(Array, 0, Rows * Columns, Columns, Spacing)
        {
            this.Player = Player;
            PerPage = Rows * Columns;
            Panel = new Panel
            {
                DisableKeys = true
            };
            SelectedTextures = new Texture[Buttons.Length];
            for (var i = 0; i < Buttons.Length; i++)
            {
                SelectedTextures[i] =
                    new Texture(
                        Graphics2D.LoadFromAssets("Assets/UI/SelectedInventorySlot.png"),
                        Textures[i].Position,
                        Textures[i].Scale
                    );
                SelectedTextures[i].TextureElement.MaskId = DefaultId;
                Panel.AddElement(SelectedTextures[i]);
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
            
            Panel.AddElement(_currentPageText);
            Panel.AddElement(_previousPage);
            Panel.AddElement(_nextPage);
            Panel.AddElement(_pageSelector);
            Panel.AddElement(_title);
            Panel.AddElement(_titleText);
        }

        protected abstract Translation TitleTranslation { get; }
        
        public override void UpdateView()
        {
            ResetSelector();        
            Array.Empty();
            var outputs = ArrayObjects;
            for (var i = CurrentPage * PerPage; i < outputs.Length; i++)
            {
                Array.AddItem(outputs[i]);
                SetSlot(Array.IndexOf(outputs[i]));
            }
            TotalPages = outputs.Length / PerPage + 1;
            _currentPageText.Text = $"{CurrentPage + 1}/{TotalPages}";
            Renderer.UpdateView(); 
        }

        private void ResetSelector()
        {
            if (!Enabled) return;
            for (var i = 0; i < SelectedTextures.Length; i++)
            {
                ResetSlot(i);
            }
            SelectedTextures[SelectedIndex].Enable();
        }

        protected virtual void ResetSlot(int Index)
        {
            SelectedTextures[Index].Disable();
            Buttons[Index].Texture.Grayscale = false;
        }

        protected virtual void SetSlot(int Index)
        {
        }

        protected abstract Item[] ArrayObjects { get; }

        public void Reset()
        {
            CurrentPage = 0;
            SelectedIndex = 0;
        }

        private void PreviousPage()
        {
            CurrentPage = Mathf.Modulo(CurrentPage - 1, TotalPages);
        }
        
        private void NextPage()
        {
            CurrentPage = Mathf.Modulo(CurrentPage + 1, TotalPages);
        }

        public int SelectedIndex { get; set; }

        
        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (Enabled) Panel.Enable();
                else Panel.Disable();            
            }
        }

        public override Vector2 Scale
        {
            get => base.Scale;
            set
            {
                var elements = Panel.Elements.ToArray();
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
                var elements = Panel.Elements.ToArray();
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
                var elements = Panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Position = elements[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }      
    }
}