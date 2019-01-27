using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterface : InventoryArrayInterface
    {
        private static uint DefaultId { get; }  = Graphics2D.LoadFromAssets("Assets/UI/AbilityTreeBackground.png");
        private static Vector2 DefaultSize { get; } = Graphics2D.SizeFromAssets("Assets/UI/AbilityTreeBackground.png").As1920x1080() * UISizeMultiplier;

        private readonly IPlayer _player;
        private readonly Panel _panel;
        private readonly GUIText _titleText;
        private readonly GUIText _availablePointsText;
        private readonly RenderableTexture _backgroundTexture;
        private readonly GUIText[] _skillNames;
        private readonly Texture _specializationBackground;
        private readonly Texture _skillsBackground;
        private readonly GUIText _classSpecializationText0;
        private readonly GUIText _classSpecializationText1;
        private readonly GUIText _defaultClassText;
        private readonly Button _classSpecialization0;
        private readonly Button _classSpecialization1;
        private readonly Button _defaultClass;
        private readonly Vector2 _targetResolution = new Vector2(1366, 768);

        public AbilityTreeInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine)
            : base(Array, Offset, Length, SlotsPerLine, new Vector2(1.45f, 1.35f))
        {
            _player = Player;
            _panel = new Panel();
            _skillNames = new GUIText[Buttons.Length];
            _backgroundTexture = new RenderableTexture(
                new Texture(DefaultId, Mathf.ScaleGui(_targetResolution, new Vector2(.04f, .15f)), DefaultSize * .3f),
                DrawOrder.Before
            );
            _titleText = new GUIText(Translation.Create("skill_tree_title"), _backgroundTexture.Position + _backgroundTexture.Scale.Y * Vector2.UnitY,
                Color.White, FontCache.Get(AssetManager.BoldFamily, 12f, FontStyle.Bold));
            _titleText.Position += _titleText.Scale.Y * Vector2.UnitY;
            _availablePointsText = new GUIText(string.Empty, _backgroundTexture.Position - _backgroundTexture.Scale.Y * Vector2.UnitY,
                Color.White, FontCache.Get(AssetManager.BoldFamily, 12f, FontStyle.Bold));
            _availablePointsText.Position += _availablePointsText.Scale.Y * Vector2.UnitY;
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                Buttons[i].Scale = Textures[i].Scale = Textures[i].Scale * .8f;
                Buttons[i].Position = Textures[i].Position = Textures[i].Position - Vector2.UnitY * Textures[i].Scale.Y * .5f;
                Buttons[i].Texture.IdPointer = null;
                _skillNames[i] = new GUIText(
                    string.Empty,
                    Textures[i].Position + Textures[i].Scale.Y * Vector2.UnitY * 1.5f,
                    Color.White, FontCache.Get(AssetManager.BoldFamily, 9, FontStyle.Bold)
                );
                ButtonsText[i].Color = Color.White;
                ButtonsText[i].TextFont = FontCache.Get(AssetManager.BoldFamily, 12f, FontStyle.Bold);
                ButtonsText[i].Position = Textures[i].Position;
                
                _panel.AddElement(_skillNames[i]);
            }

            var specializationBackgroundScale = new Vector2(_backgroundTexture.Scale.X * .925f,
                InventoryBackground.DefaultSize.Y * .35f);
            _specializationBackground = 
                new Texture(
                    InventoryBackground.DefaultId,
                    _backgroundTexture.Position + (_backgroundTexture.Scale.Y - specializationBackgroundScale.Y * 1.5f) * Vector2.UnitY,
                    specializationBackgroundScale
                );
            _classSpecialization0 = new Button(
                Buttons[Buttons.Length - 1].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 1].Scale * .8f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 1].TextureElement.TextureId
                }
            };
            _classSpecialization0.Click += (S, A) => _player.AbilityTree.ShowBlueprint(_player.Class.FirstSpecializationTree, null);
            _defaultClass = new Button(
                Buttons[Buttons.Length - 2].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 2].Scale * .7f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 2].TextureElement.TextureId
                }
            };
            _defaultClass.Click += (S, A) => _player.AbilityTree.ShowBlueprint(_player.Class.MainTree, null);
            _classSpecialization1 = new Button(
                Buttons[Buttons.Length - 3].Position.X * Vector2.UnitX +
                _specializationBackground.Position.Y * Vector2.UnitY,
                Buttons[Buttons.Length - 3].Scale * .8f,
                GUIRenderer.TransparentTexture
            )
            {
                Texture =
                {
                    MaskId = Textures[Buttons.Length - 3].TextureElement.TextureId
                }
            };
            _classSpecialization1.Click += (S, A) => _player.AbilityTree.ShowBlueprint(_player.Class.SecondSpecializationTree, null);
            
            _defaultClassText = 
                new GUIText("A", _defaultClass.Position + _defaultClass.Scale.Y * Vector2.UnitY, Color.White, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));
            _classSpecializationText0 = 
                new GUIText("A", _classSpecialization0.Position + _classSpecialization0.Scale.Y * Vector2.UnitY, Color.White, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));
            _classSpecializationText1 = 
                new GUIText("A", _classSpecialization1.Position + _classSpecialization1.Scale.Y * Vector2.UnitY, Color.White, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));

            _defaultClassText.Position += _defaultClassText.Scale.Y * Vector2.UnitY * 1.5f;
            _classSpecializationText0.Position += _classSpecializationText0.Scale.Y * Vector2.UnitY * 1.5f;
            _classSpecializationText1.Position += _classSpecializationText1.Scale.Y * Vector2.UnitY * 1.5f;
            
            _skillsBackground = new Texture(DefaultId, _backgroundTexture.Position - _backgroundTexture.Scale.Y * Vector2.UnitY * .3f, _backgroundTexture.Scale * new Vector2(.925f, .675f));
            _skillsBackground.SendBack();
            
            _panel.AddElement(_skillsBackground);
            _panel.AddElement(_titleText);
            _panel.AddElement(_specializationBackground);
            _panel.AddElement(_classSpecializationText0);
            _panel.AddElement(_classSpecializationText1);
            _panel.AddElement(_defaultClassText);
            _panel.AddElement(_classSpecialization0);
            _panel.AddElement(_classSpecialization1);
            _panel.AddElement(_defaultClass);
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
                    this.Buttons[i].Scale = Vector2.Zero;
                    this.Textures[i].Scale = Vector2.Zero;
                    this.ButtonsText[i].Text = string.Empty;
                    _skillNames[i].Text = string.Empty;
                }
                else
                {
                    var level = this.Array[i + Offset].HasAttribute("Level")
                        ? this.Array[i + Offset].GetAttribute<int>("Level")
                        : 0;

                    this.ButtonsText[i].Text = level > 0 ? level.ToString() : string.Empty;
                    _skillNames[i].Text = Array[i + Offset].HasAttribute("Skill") 
                        ? Array[i + Offset].GetAttribute<BaseSkill>("Skill").DisplayName 
                        : string.Empty;

                    var isLocked = this.SetGrayscale(i);

                    if (!this.Array[i + Offset].HasAttribute("ButtonScale") ||
                        !this.Array[i + Offset].HasAttribute("TextureScale")) continue;
                    this.Buttons[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("ButtonScale");
                    this.Textures[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("TextureScale");
                }
            }

            _defaultClassText.Text = Translations.Get(_player.Class.MainTree.Name);
            _classSpecializationText0.Text = Translations.Get(_player.Class.FirstSpecializationTree.Name);
            _classSpecializationText1.Text = Translations.Get(_player.Class.SecondSpecializationTree.Name);
            
            _classSpecialization0.Texture.TextureId = _player.Class.FirstSpecializationTree.Icon;
            _defaultClass.Texture.TextureId = _player.Class.MainTree.Icon;
            _classSpecialization1.Texture.TextureId = _player.Class.SecondSpecializationTree.Icon;

            //_classSpecialization0.Texture.Grayscale = _player.AbilityTree.FirstTreeSave.Length > 0;
            //_classSpecialization1.Texture.Grayscale = _player.AbilityTree.SecondTreeSave.Length > 0;
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
