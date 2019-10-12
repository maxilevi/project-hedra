using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;
using OpenTK.Platform.MacOS;

namespace Hedra.Engine.Player.PagedInterface
{
    public abstract class PagedInventoryArrayInterface : InventoryArrayInterface
    {
        public static uint SelectedId { get; } = Graphics2D.LoadFromAssets("Assets/UI/SelectedInventorySlot.png");
        protected BackgroundTexture[] SelectedTextures { get; }
        protected Panel Panel { get; }
        protected IPlayer Player { get; }
        protected int PerPage { get; }
        protected int CurrentPage { get; private set; }
        protected int TotalPages { get; private set; }
        protected BackgroundTexture Title { get; }
        protected GUIText TitleText { get; }
        protected BackgroundTexture PageSelector { get; }
        protected GUIText CurrentPageText { get; }
        protected Button PreviousPageText { get; }
        protected Button NextPageText { get; }

        protected PagedInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns, Vector2 Spacing) 
            : base(Array, 0, Rows * Columns, Columns, Spacing)
        {
            this.Player = Player;
            PerPage = Math.Max(Rows * Columns, 1);
            Panel = new Panel
            {
                DisableKeys = true
            };
            SelectedTextures = new BackgroundTexture[Buttons.Length];
            for (var i = 0; i < Buttons.Length; i++)
            {
                SelectedTextures[i] =
                    new BackgroundTexture(
                        SelectedId,
                        Textures[i].Position,
                        Textures[i].Scale
                    );
                SelectedTextures[i].TextureElement.MaskId = DefaultId;
                Panel.AddElement(SelectedTextures[i]);
            }

            var barScale = new Vector2(DefaultSize.X * Rows, DefaultSize.Y * 2) * .65f;
            var realScale = InventoryBackground.DefaultSize * barScale;
            var barPosition = Vector2.UnitY * Rows * DefaultSize * .65f;
            var offset = Rows % 2 == 0 ? Vector2.UnitY * realScale.Y : Vector2.Zero;
            Title = new BackgroundTexture(InventoryBackground.DefaultId, barPosition - offset, realScale);
            TitleText = new GUIText(TitleTranslation, Title.Position, Color.White, FontCache.GetBold(12));
            PageSelector = new BackgroundTexture(InventoryBackground.DefaultId, -barPosition - offset, realScale);
            
            CurrentPageText = new GUIText("00/00", PageSelector.Position, Color.White, FontCache.GetBold(11));
            var footerFont = FontCache.GetBold(14);
            PreviousPageText = new Button(CurrentPageText.Position - Vector2.UnitX * CurrentPageText.Scale.X * 1.25f, Vector2.One, "\u25C0", Color.White, footerFont);
            PreviousPageText.Click += (O, E) => PreviousPage();
            NextPageText = new Button(CurrentPageText.Position + Vector2.UnitX * CurrentPageText.Scale.X * 1.25f, Vector2.One, "\u25B6", Color.White, footerFont);
            NextPageText.Click += (O, E) => NextPage();
            
            Panel.AddElement(CurrentPageText);
            Panel.AddElement(PreviousPageText);
            Panel.AddElement(NextPageText);
            Panel.AddElement(PageSelector);
            Panel.AddElement(Title);
            Panel.AddElement(TitleText);
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
            UpdatePages(outputs.Length);
            Renderer.UpdateView();
        }

        protected void UpdatePages(int Length)
        {
            TotalPages = Math.Max(Length / PerPage, 1);
            CurrentPage = Mathf.Modulo(CurrentPage, TotalPages);
            CurrentPageText.Text = $"{CurrentPage + 1}/{Math.Max(1, TotalPages)}";
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

        protected virtual Item[] ArrayObjects => null;

        public void Reset()
        {
            CurrentPage = 0;
            SelectedIndex = 0;
        }

        private void PreviousPage()
        {
            CurrentPage = Mathf.Modulo(CurrentPage - 1, TotalPages);
            UpdateView();
        }
        
        private void NextPage()
        {
            CurrentPage = Mathf.Modulo(CurrentPage + 1, TotalPages);
            UpdateView();
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