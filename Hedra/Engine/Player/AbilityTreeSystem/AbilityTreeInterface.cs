using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterface : InventoryArrayInterface
    {
        private readonly IPlayer _player;
        private readonly Panel _panel;
        private readonly GUIText _availablePointsText;
        private readonly RenderableTexture _backgroundTexture;
        private readonly RenderableTexture[] _skillPointsBackgroundTextures;
        private readonly Vector2 _targetResolution = new Vector2(1366, 768);

        public AbilityTreeInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing)
            : base(Array, Offset, Length, SlotsPerLine, Spacing)
        {
            _player = Player;
            _panel = new Panel();
            _skillPointsBackgroundTextures = new RenderableTexture[this.Buttons.Length];
            _backgroundTexture = new RenderableTexture(new Texture("Assets/UI/AbilityTreeBackground.png",
                Mathf.ScaleGui(_targetResolution, new Vector2(.04f, .15f)), new Vector2(.6f, .55f) * 1f), DrawOrder.Before);
            _availablePointsText = new GUIText(string.Empty, new Vector2(_backgroundTexture.Position.X, -.35f),
                Color.White, FontCache.Get(AssetManager.BoldFamily, 12f, FontStyle.Bold));
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                this.Buttons[i].Scale = this.Textures[i].Scale;
                this.Buttons[i].Texture.IdPointer = null;
                this.ButtonsText[i].TextFont = FontCache.Get(AssetManager.BoldFamily, 10f, FontStyle.Bold);
                this.ButtonsText[i].Position = this.Buttons[i].Position +
                                               Mathf.ScaleGui(_targetResolution, new Vector2(0, -InventoryArrayInterface.DefaultSize.Y) * .65f);
                _skillPointsBackgroundTextures[i] = 
                    new RenderableTexture(new Texture("Assets/UI/InventoryBackground.png",
                    this.ButtonsText[i].Position, new Vector2(.05f, .075f)), DrawOrder.Before);

                _panel.AddElement(_skillPointsBackgroundTextures[i]);
            }
            _panel.AddElement(_availablePointsText);
            _panel.AddElement(_backgroundTexture);
        }

        public override void UpdateView()
        {
            _availablePointsText.Text = $"{Translations.Get("available_points")}: {_player.AbilityTree.AvailablePoints}";
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                if (!this.Array[i + Offset].HasAttribute("Enabled")) continue;

                if (!this.Array[i + Offset].GetAttribute<bool>("Enabled"))
                {
                    if (this.Buttons[i].Scale == Vector2.Zero || this.Textures[i].Scale == Vector2.Zero) continue;
                    this.Array[i + Offset].SetAttribute("ButtonScale", this.Buttons[i].Scale);
                    this.Array[i + Offset].SetAttribute("TextureScale", this.Textures[i].Scale);
                    this.Array[i + Offset].SetAttribute("BackgroundScale", this._skillPointsBackgroundTextures[i].Scale);
                    this.Buttons[i].Scale = Vector2.Zero;
                    this.Textures[i].Scale = Vector2.Zero;
                    this.ButtonsText[i].Text = string.Empty;
                    this._skillPointsBackgroundTextures[i].Scale = Vector2.Zero;
                }
                else
                {
                    var level = this.Array[i + Offset].HasAttribute("Level")
                        ? this.Array[i + Offset].GetAttribute<int>("Level")
                        : 0;

                    this.ButtonsText[i].Text = level > 0 ? level.ToString() : string.Empty;
                    if (level > 0 && this.Array[i + Offset].HasAttribute("BackgroundScale"))
                    {
                        this._skillPointsBackgroundTextures[i].Scale =
                            this.Array[i + Offset].GetAttribute<Vector2>("BackgroundScale");
                    }
                    else if(!this.Array[i + Offset].HasAttribute("BackgroundScale"))
                    {
                        this.Array[i + Offset].SetAttribute("BackgroundScale", this._skillPointsBackgroundTextures[i].Scale);
                        this._skillPointsBackgroundTextures[i].Scale = Vector2.Zero;
                    }
                    var isLocked = this.SetGrayscale(i);

                    if (!this.Array[i + Offset].HasAttribute("ButtonScale") ||
                        !this.Array[i + Offset].HasAttribute("TextureScale")) continue;
                    this.Buttons[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("ButtonScale");
                    this.Textures[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("TextureScale");
                }
            }
        }


        private bool SetGrayscale(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 - (Index - decomposedIndexY) / AbilityTree.Columns;
            this.Buttons[Index].Texture.Grayscale = decomposedIndexX * 5 > _player.Level || !this.PreviousUnlocked(Index);
            return this.Buttons[Index].Texture.Grayscale;
        }

        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 - (Index - decomposedIndexY) / AbilityTree.Columns;
            if (decomposedIndexX == 0) return true;
            else if (!this.Array[Index + AbilityTree.Columns].GetAttribute<bool>("Enabled"))
                return this.PreviousUnlocked(Index + AbilityTree.Columns);
            return this.Array[Index + AbilityTree.Columns].GetAttribute<int>("Level") > 0;
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();
            }
        }

        public override Vector2 Scale
        {
            get { return base.Scale; }
            set
            {
                _backgroundTexture.Scale = new Vector2(_backgroundTexture.Scale.X / base.IndividualScale.X,
                                                  _backgroundTexture.Scale.Y / base.IndividualScale.Y) * value;
                var backgroundPosition = _backgroundTexture.Position - Position;
                _backgroundTexture.Position = new Vector2(backgroundPosition.X / base.Scale.X,
                                                  backgroundPosition.Y / base.Scale.Y) * value + Position;

                for (var i = 0; i < _skillPointsBackgroundTextures.Length; i++)
                {
                    _skillPointsBackgroundTextures[i].Scale = new Vector2(_skillPointsBackgroundTextures[i].Scale.X / base.IndividualScale.X,
                                                        _skillPointsBackgroundTextures[i].Scale.Y / base.IndividualScale.Y) * value;
                    var relativePosition = _skillPointsBackgroundTextures[i].Position - Position;
                    _skillPointsBackgroundTextures[i].Position = new Vector2(relativePosition.X / base.Scale.X,
                                                                    relativePosition.Y / base.Scale.Y) * value + Position;
                }
                base.Scale = value;
            }
        }

        public override Vector2 IndividualScale
        {
            get { return base.IndividualScale; }
            set
            {
                for (var i = 0; i < _skillPointsBackgroundTextures.Length; i++)
                {
                    _skillPointsBackgroundTextures[i].Scale = new Vector2(_skillPointsBackgroundTextures[i].Scale.X / base.IndividualScale.X,
                                                                 _skillPointsBackgroundTextures[i].Scale.Y / base.IndividualScale.Y) * value;
                }
                _backgroundTexture.Scale = new Vector2(_backgroundTexture.Scale.X / base.IndividualScale.X,
                                                  _backgroundTexture.Scale.Y / base.IndividualScale.Y) * value;
                base.IndividualScale = value;
            }
        }

        public override Vector2 Position
        {
            get { return base.Position; }
            set
            {
                for (var i = 0; i < _skillPointsBackgroundTextures.Length; i++)
                {
                    _skillPointsBackgroundTextures[i].Position = _skillPointsBackgroundTextures[i].Position - base.Position + value;
                }
                _backgroundTexture.Position = _backgroundTexture.Position - base.Position + value;
                _availablePointsText.Position = _availablePointsText.Position - base.Position + value;
                base.Position = value;
            }
        }
    }
}
