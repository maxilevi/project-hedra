using System.Collections.Generic;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Input;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class SpecializationPanel : Panel
    {
        private readonly IPlayer _player;
        private readonly Button _classSpecialization0;
        private readonly Button _classSpecialization1;
        private readonly Button _defaultClass;
        private readonly Texture _defaultClassMarker;
        private readonly Texture _classSpecialization0Marker;
        private readonly Texture _classSpecialization1Marker;
        private readonly RenderableTexture _specializationBackground;
        private readonly SpecializationInfo _specializationInfo;
        private readonly ArrowSelectorState _selectorState;
        
        public SpecializationPanel(IPlayer Player, RenderableButton[] Buttons, Texture[] Textures, RenderableTexture BackgroundTexture)
        {
            _player = Player;
            var specializationBackgroundScale = new Vector2(BackgroundTexture.Scale.X * .925f,
                InventoryBackground.DefaultSize.Y * .4f);
            _specializationBackground = new RenderableTexture(
                new Texture(
                    InventoryBackground.DefaultId,
                    BackgroundTexture.Position - (BackgroundTexture.Scale.Y + specializationBackgroundScale.Y * 2f) *
                    Vector2.UnitY,
                    specializationBackgroundScale
                ), DrawOrder.Before
            );
            _classSpecialization0 = new Button(
                Buttons[Buttons.Length - 1].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 1].Scale * 1.1f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 1].TextureElement.TextureId
                }
            };
            _classSpecialization0.Click += (S, A) => Show(Player.Class.FirstSpecializationTree);
            _defaultClass = new Button(
                Buttons[Buttons.Length - 2].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 2].Scale * .9f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 2].TextureElement.TextureId
                }
            };
            _defaultClass.Click += (S, A) => Show(Player.Class.MainTree, false);
            _classSpecialization1 = new Button(
                Buttons[Buttons.Length - 3].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 3].Scale * 1.1f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 3].TextureElement.TextureId
                }
            };
            _classSpecialization1.Click += (S, A) => Show(Player.Class.SecondSpecializationTree);

            _specializationInfo = new SpecializationInfo(_player);
            _defaultClassMarker = new Texture(PagedInventoryArrayInterface.SelectedId, _defaultClass.Position, _defaultClass.Scale * 1.1f);
            _classSpecialization0Marker = new Texture(PagedInventoryArrayInterface.SelectedId, _classSpecialization0.Position, _classSpecialization0.Scale * 1.1f);
            _classSpecialization1Marker = new Texture(PagedInventoryArrayInterface.SelectedId, _classSpecialization1.Position, _classSpecialization1.Scale * 1.1f);
            _selectorState = new ArrowSelectorState();
            
            AddElement(_defaultClassMarker);
            AddElement(_classSpecialization0Marker);
            AddElement(_classSpecialization1Marker);
            AddElement(_defaultClass);
            AddElement(_classSpecialization0);
            AddElement(_classSpecialization1);
            AddElement(_specializationBackground);
            OnPanelStateChange += (O, E) =>
            {
                if (E == PanelState.Disabled)
                {
                    //Show(_player.Class.MainTree);
                    _specializationInfo.ShowSpecialization(null);
                }
            };
            EventDispatcher.RegisterKeyDown(this, OnKeyDown);
            EventDispatcher.RegisterKeyUp(this, OnKeyUp);
        }

        public void UpdateView()
        {
            _classSpecialization0.Texture.TextureId = _player.Class.FirstSpecializationTree.Icon;
            _defaultClass.Texture.TextureId = _player.Class.MainTree.Icon;
            _classSpecialization1.Texture.TextureId = _player.Class.SecondSpecializationTree.Icon;

            _classSpecialization0.Texture.Tint = _player.AbilityTree.HasSpecialization && !_player.AbilityTree.HasFirstSpecialization
                ? new Vector4(Vector3.One * .5f, 1f)
                : Vector4.One;
            
            _classSpecialization1.Texture.Tint = _player.AbilityTree.HasSpecialization && !_player.AbilityTree.HasSecondSpecialization
                ? new Vector4(Vector3.One * .5f, 1f)
                : Vector4.One;

            _classSpecialization0.Texture.Grayscale = _classSpecialization0.Texture.Tint != Vector4.One;
            _classSpecialization1.Texture.Grayscale = _classSpecialization1.Texture.Tint != Vector4.One;
            _defaultClass.Texture.Grayscale = _defaultClass.Texture.Tint != Vector4.One;
            
            EnableMarker();
        }

        private void EnableMarker()
        {
            if(!Enabled) return;
            _defaultClassMarker.Disable();
            _classSpecialization0Marker.Disable();
            _classSpecialization1Marker.Disable();
            
            if(IsMainTreeSelected)
                _defaultClassMarker.Enable();
            
            else if(IsFirstTreeSelected)
                _classSpecialization0Marker.Enable();
            
            else if(IsSecondTreeSelected)
                _classSpecialization1Marker.Enable();
        }

        private void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            if(!Enabled) return;
            _selectorState.OnRight = () =>
            {
                if(IsMainTreeSelected) _classSpecialization0.ForceClick();
                else if(IsFirstTreeSelected) _classSpecialization1.ForceClick();
                else if(IsSecondTreeSelected) _defaultClass.ForceClick();
            };
            _selectorState.OnLeft = () =>
            {
                if(IsMainTreeSelected) _classSpecialization1.ForceClick();
                else if(IsFirstTreeSelected) _defaultClass.ForceClick();
                else if(IsSecondTreeSelected) _classSpecialization0.ForceClick();
            };
            ArrowSelector.ProcessKeyDown(Args, _selectorState);
            UpdateView();
        }
        
        private void OnKeyUp(object Sender, KeyEventArgs Args)
        {
            if(!Enabled) return;
            ArrowSelector.ProcessKeyUp(Args, _selectorState);
        }

        private void Show(AbilityTreeBlueprint Blueprint, bool ShowSpecializationInfo = true)
        {
            _player.AbilityTree.ShowBlueprint(Blueprint, null);
            _specializationInfo.ShowSpecialization(ShowSpecializationInfo ? Blueprint : null);
        }
        
        private bool IsMainTreeSelected => Blueprint.Icon == _defaultClass.Texture.TextureId;

        private bool IsFirstTreeSelected => Blueprint.Icon == _classSpecialization0.Texture.TextureId;

        private bool IsSecondTreeSelected => Blueprint.Icon == _classSpecialization1.Texture.TextureId;
        
        public AbilityTreeBlueprint Blueprint { private get; set; }
        
        public SpecializationInfo SpecializationInfo => _specializationInfo;

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyPress(this);
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}