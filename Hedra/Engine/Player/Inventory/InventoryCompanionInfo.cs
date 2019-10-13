using System;
using System.Drawing;
using System.Text;
using Hedra.Engine.Events;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scripting;
using Hedra.Engine.Windowing;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;


using Silk.NET.Input.Common;
using Button = Hedra.Engine.Rendering.UI.Button;


namespace Hedra.Engine.Player.Inventory
{
    public sealed class InventoryCompanionInfo : InventoryInterfaceItemInfo, IDisposable
    {
        private static readonly Script Script = Interpreter.GetScript("Companion.py");
        private GUIText Level { get; }
        private GUIText TopLeftText { get;  }
        private GUIText BottomLeftText { get; }
        private GUIText TopRightText { get; }
        private GUIText BottomRightText { get; }
        private readonly Button _renameButton;
        private readonly InventoryCompanionRenamePanel _renamePanel;
        private bool _isRenaming;
        private IEntity _companion;
        
        public InventoryCompanionInfo() : base((1f / (1366f / GameSettings.Width)) * .9f)
        {
            Level = new GUIText(string.Empty, Vector2.Zero, Color.White, FontCache.GetNormal(16));
            TopLeftText = new GUIText(string.Empty, Vector2.Zero, Color.Red, FontCache.GetBold(14));
            BottomLeftText = new GUIText(string.Empty, Vector2.Zero, Color.Yellow, FontCache.GetNormal(10));
            TopRightText = new GUIText(string.Empty, Vector2.Zero, Color.DarkViolet, FontCache.GetBold(14));
            BottomRightText = new GUIText(string.Empty, Vector2.Zero, Color.LightBlue, FontCache.GetNormal(10));
            ItemText.TextFont = FontCache.GetBold(28);
            HintTexture.Scale *= .8f; 
            HintText.TextFont = FontCache.GetBold(12);
            HintText.SetTranslation(Translation.Create("rename_btn"));
            _renamePanel = new InventoryCompanionRenamePanel();
            _renamePanel.Apply += ApplyRename;
            _renameButton = new Button(Vector2.Zero, HintTexture.Scale, GUIRenderer.TransparentTexture);
            _renameButton.Click += OnRename;
            
            Panel.AddElement(_renamePanel);
            Panel.AddElement(_renameButton);
            Panel.AddElement(Level);
            Panel.AddElement(TopLeftText);
            Panel.AddElement(BottomLeftText);
            Panel.AddElement(TopRightText);
            Panel.AddElement(BottomRightText);
            
            EventDispatcher.RegisterKeyDown(this, OnKeyDown);
        }

        public void Show(Item Item, IEntity Companion)
        {
            _companion = Companion;
            base.Show(Item);
        }

        public override void Hide()
        {
            _companion = null;
            base.Hide();
        }

        protected override void AddLayout()
        {
            base.AddLayout();
            var marginX = Vector2.UnitX * BackgroundTexture.Scale * .5f;
            var marginY = BackgroundTexture.Scale * .095f * Vector2.UnitY;
            var offset = -BackgroundTexture.Scale * .7f * Vector2.UnitY;
            var statsOffset = -BackgroundTexture.Scale * .05f * Vector2.UnitY;
            
            Level.Position = Position + marginY * 3f + offset + BackgroundTexture.Scale * .05f * Vector2.UnitY;
            TopLeftText.Position = Position + marginY - marginX + offset + statsOffset;
            BottomLeftText.Position = Position - marginY - marginX + offset + statsOffset;
            TopRightText.Position = Position + marginY + marginX + offset + statsOffset;
            BottomRightText.Position = Position - marginY + marginX + offset + statsOffset;
            
            HintTexture.Position = BackgroundTexture.Position - (HintTexture.Scale.Y * 1.5f + BackgroundTexture.Scale.Y) * Vector2.UnitY;
            HintText.Position = HintTexture.Position;
            _renamePanel.Position = BackgroundTexture.Position - BackgroundTexture.Scale * 1.5f * Vector2.UnitY;
            _renameButton.Position = HintTexture.Position;
            
            ItemTexture.Scale *= .65f;
            ItemTexture.Position += BackgroundTexture.Scale * Vector2.UnitY * .15f;
            UpdateStats(CurrentItem, _companion);
            SetupRenameUI();
        }

        private void SetupRenameUI()
        {
            if (_isRenaming)
            {
                _renamePanel.Enable();
                _renameButton.Disable();
                HintTexture.Disable();
                HintText.Disable();
                _renamePanel.Text = ItemText.Text;
                _renamePanel.UpdateView();
            }
            else
            {
                _renamePanel.Disable();
                _renameButton.Enable();
                HintTexture.Enable();
                HintText.Enable();
            }
        }

        protected override void AddHint()
        {
        }

        public void UpdateStats(Item Item, IEntity Companion)
        {
            Script.Execute("update_ui", Item, Companion, TopLeftText, TopRightText, BottomLeftText, BottomRightText, Level, ItemText);
        }
        
        protected override void UpdateItemMesh()
        {
            UpdateItemMesh(CurrentItem.GetAttribute<VertexData>("CompanionModel"));
        }
        
        protected override float DescriptionHeight => BackgroundTexture.Scale.Y * .1f;

        protected override void AddAttributes()
        {
            ItemAttributes.Text = string.Empty;
        }

        private void OnRename(object Sender, MouseButtonEventArgs Args)
        {
            _isRenaming = true;
            UpdateView();
        }

        private void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            if(!Enabled || !_isRenaming) return;
            if (Args.Key == Key.Enter)
            {
                ApplyRename();
            }
            else if (Args.Key == Key.Escape)
            {
                _isRenaming = false;
                UpdateView();
            }
        }

        private void ApplyRename()
        {
            CurrentItem.SetAttribute("PetName", _renamePanel.Text);
            _isRenaming = false;
            UpdateView();
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (!value)
                    _isRenaming = false;
                base.Enabled = value;
            }
        }
        public Item ShowingCompanion => CurrentItem;

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}