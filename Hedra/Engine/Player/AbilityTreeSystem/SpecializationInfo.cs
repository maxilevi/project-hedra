using System;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class SpecializationInfo : AbilityTreeInterfaceItemInfo
    {
        private readonly Button _learnSpecializationButton;
        private readonly GUIText _learnSpecializationText;
        private readonly IPlayer _player;
        private AbilityTreeBlueprint _blueprint;

        public SpecializationInfo(IPlayer Player)
        {
            _player = Player;
            _learnSpecializationButton = new Button(
                Vector2.Zero,
                InventoryBackground.DefaultSize * .3f,
                InventoryBackground.DefaultId
            );
            _learnSpecializationText = new GUIText(
                Translation.Create("learn_specialization"),
                _learnSpecializationButton.Position,
                Color.White,
                FontCache.GetBold(15)
            );
            _learnSpecializationButton.Texture.Grayscale = true;
            ItemTexture.Scale *= 1.15f;
            ItemTexture.Scale = ItemTexture.Scale.As1920x1080();
            BackgroundTexture.Scale *= 1.1f;
            BackgroundTexture.Scale = BackgroundTexture.Scale.As1920x1080();
            Panel.AddElement(_learnSpecializationButton);
            Panel.AddElement(_learnSpecializationText);

            _learnSpecializationButton.Click += OnClick;
            _learnSpecializationButton.HoverEnter += () =>
            {
                _learnSpecializationButton.Scale *= 1.05f;
                _learnSpecializationText.Scale *= 1.05f;
            };
            _learnSpecializationButton.HoverExit += () =>
            {
                _learnSpecializationButton.Scale /= 1.05f;
                _learnSpecializationText.Scale /= 1.05f;
            };
        }

        private void OnClick(object Sender, MouseButtonEventArgs Args)
        {
            if (_player.Level >= AbilityTree.SpecializationLevelRequirement)
            {
                _player.AbilityTree.LearnSpecialization(_blueprint);
                UpdateView();
            }
            else
            {
                _player.MessageDispatcher.ShowNotification(
                    Translations.Get("specialization_level_requirement", AbilityTree.SpecializationLevelRequirement),
                    Color.Red, 3f);
            }
        }

        protected override void UpdateView()
        {
            ItemText.Text = _blueprint.DisplayName;
            ItemDescription.Text = Utils.FitString(_blueprint.Description, (int)40.As1920x1080());
            ItemDescription.Color = Color.White;
            ItemTexture.TextureElement.TextureId = _blueprint.Icon;
            HintTexture.Disable();
            HintText.Disable();
            SetPosition();
            ItemTexture.Position += ItemTexture.Scale.Y * Vector2.UnitY * .5f;
            _learnSpecializationButton.Texture.Tint = _player.Level < 5
                ? new Vector4(Color.Orange.ToVector4().Xyz() * 2f, 1)
                : new Vector4(Color.Orange.ToVector4().Xyz() * 5f, 1);
            _learnSpecializationText.TextColor = _player.Level < 5
                ? Color.Gray
                : Color.White;
            if (_player.AbilityTree.HasSpecialization)
            {
                _learnSpecializationButton.Disable();
                _learnSpecializationText.Disable();
            }
        }

        public void ShowSpecialization(AbilityTreeBlueprint Blueprint)
        {
            _blueprint = Blueprint;
            if (_blueprint != null)
            {
                Enabled = true;
                Panel.Enable();
                UpdateView();
            }
            else
            {
                Enabled = false;
                Panel.Disable();
            }
        }

        protected override void SetPosition()
        {
            base.SetPosition();
            ItemTexture.Position = Position + DefaultSize.Y * Vector2.UnitY * .1f;
            ItemDescription.Position =
                ItemTexture.Position - ItemTexture.Scale.Y * Vector2.UnitY * 2 - Vector2.UnitY * .01f;
            _learnSpecializationButton.Position = BackgroundTexture.Position - DefaultSize.Y * Vector2.UnitY * .65f;
            _learnSpecializationText.Position = _learnSpecializationButton.Position;
        }

        public override void Show(Item Item)
        {
            throw new NotImplementedException();
        }
    }
}