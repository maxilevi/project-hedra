using System.Numerics;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnApply();

    public class InventoryCompanionRenamePanel : Panel
    {
        private readonly Button _applyButton;
        private readonly GUIText _applyText;
        private readonly BackgroundTexture _backgroundTexture;
        private readonly TextField _field;

        public InventoryCompanionRenamePanel()
        {
            _backgroundTexture = new BackgroundTexture(InventoryBackground.DefaultId, Vector2.Zero, Vector2.Zero);
            _field = new TextField(Vector2.Zero, _backgroundTexture.Scale * .5f, false);
            _applyButton = new Button(Vector2.Zero, Vector2.Zero, InventoryBackground.DefaultId);
            _applyText = new GUIText(Translation.Create("apply_rename_btn"), Vector2.Zero, Color.White,
                FontCache.GetBold(11));
            _applyButton.Click += OnApply;
            _applyButton.HoverEnter += () =>
            {
                _applyButton.Scale *= 1.05f;
                _applyText.Scale *= 1.05f;
            };
            _applyButton.HoverExit += () =>
            {
                _applyButton.Scale /= 1.05f;
                _applyText.Scale /= 1.05f;
            };

            AddElement(_backgroundTexture);
            AddElement(_applyButton);
            AddElement(_field);
            AddElement(_applyText);
        }

        public string Text
        {
            get => _field.Text;
            set => _field.Text = value;
        }

        public override Vector2 Position
        {
            get => _backgroundTexture.Position;
            set => _backgroundTexture.Position = value;
        }

        public event OnApply Apply;

        public void UpdateView()
        {
            _backgroundTexture.Scale = InventoryBackground.DefaultSize * .25f;
            _field.Position = _backgroundTexture.Position;
            _field.Scale = _backgroundTexture.Scale * new Vector2(1.5f, .75f);
            _applyButton.Position = _backgroundTexture.Position - Vector2.UnitY * _backgroundTexture.Scale * 2;
            _applyText.Position = _applyButton.Position;
            _applyButton.Scale = _applyText.Scale * 2;
            _field.Focus();
        }

        private void OnApply(object Sender, MouseButtonEventArgs Args)
        {
            Apply?.Invoke();
            _field.Defocus();
        }
    }
}