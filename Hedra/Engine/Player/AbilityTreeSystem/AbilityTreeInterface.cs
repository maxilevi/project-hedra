using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Linq;
using System.Reflection.Emit;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using System.Numerics;
using Hedra.Engine.Windowing;
using Hedra.Numerics;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterface : InventoryArrayInterface
    {
        private static uint LabelId { get; } = Graphics2D.LoadFromAssets("Assets/UI/SkillLabel.png");

        private static Vector2 LabelSize { get; } =
            Graphics2D.SizeFromAssets("Assets/UI/SkillLabel.png").As1920x1080() * UISizeMultiplier;

        private static uint DefaultId { get; } = Graphics2D.LoadFromAssets("Assets/UI/AbilityTreeBackground.png");

        private static Vector2 DefaultSize { get; } =
            Graphics2D.SizeFromAssets("Assets/UI/AbilityTreeBackground.png").As1920x1080() * UISizeMultiplier;

        private readonly Vector2 _targetResolution = new Vector2(1366, 768);
        private readonly IPlayer _player;
        private readonly GUIText _titleText;
        private readonly GUIText _availablePointsText;
        private readonly RenderableTexture _backgroundTexture;
        private readonly RenderableTexture[] _skillPointsBackgroundTextures;
        private readonly TreeLinesUI _linesUI;
        private readonly SpecializationPanel _specializationPanel;
        private AbilityTreeBlueprint _blueprint;
        private readonly Button _confirmButton;
        private readonly GUIText _confirmButtonText;

        public AbilityTreeInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine)
            : base(Array, Offset, Length, SlotsPerLine, new Vector2(1.5f, 1.5f))
        {
            _player = Player;
            _skillPointsBackgroundTextures = new RenderableTexture[Buttons.Length];
            _backgroundTexture = new RenderableTexture(
                new BackgroundTexture(DefaultId,
                    Mathf.ScaleGui(_targetResolution, new Vector2(.04f, .15f)).As1920x1080(), DefaultSize * .3f),
                DrawOrder.Before
            );
            _linesUI = new TreeLinesUI();
            _titleText = new GUIText(string.Empty, Vector2.Zero, Color.White, FontCache.GetNormal(16f));
            _availablePointsText = new GUIText(Translations.Get("available_points"),
                _backgroundTexture.Position - _backgroundTexture.Scale.Y * Vector2.UnitY,
                Color.White, FontCache.GetBold(12f));
            _availablePointsText.Position += _availablePointsText.Scale.Y * Vector2.UnitY;
            for (var i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].Scale = Textures[i].Scale;
                Buttons[i].Position = Textures[i].Position;
                Buttons[i].Texture.IdPointer = null;
                ButtonsText[i].Color = Color.White;
                ButtonsText[i].TextFont = FontCache.GetBold(10f);
                ButtonsText[i].Disable();
                var skillPointSize = LabelSize * (float)(2.0 / 3.0);
                ButtonsText[i].Position =
                    Textures[i].Position - (Textures[i].Scale.Y + skillPointSize.Y) * Vector2.UnitY;
                _skillPointsBackgroundTextures[i] =
                    new RenderableTexture(
                        new BackgroundTexture(LabelId, ButtonsText[i].Position, skillPointSize),
                        DrawOrder.Before
                    );

                _panel.AddElement(_skillPointsBackgroundTextures[i]);
            }

            _specializationPanel = new SpecializationPanel(_player, Buttons, Textures, _backgroundTexture);
            var confirmButtonScale = InventoryBackground.DefaultSize * .2f;
            _confirmButton = new Button(
                _backgroundTexture.Position - (_backgroundTexture.Scale.Y + confirmButtonScale.Y * 2f) * Vector2.UnitY,
                confirmButtonScale,
                InventoryBackground.DefaultId
            );
            _confirmButtonText = new GUIText(
                Translation.Create("confirm_skill_points"),
                _confirmButton.Position,
                Color.White,
                FontCache.GetBold(13)
            );
            //_confirmButton.Texture.Grayscale = true;

            _confirmButton.Click += ConfirmChanges;
            _confirmButton.HoverEnter += () =>
            {
                _confirmButton.Scale *= 1.05f;
                _confirmButtonText.Scale *= 1.05f;
            };
            _confirmButton.HoverExit += () =>
            {
                _confirmButton.Scale /= 1.05f;
                _confirmButtonText.Scale /= 1.05f;
            };

            _panel.AddElement(_confirmButton);
            _panel.AddElement(_confirmButtonText);
            _panel.AddElement(_specializationPanel);
            _panel.AddElement(_linesUI);
            _panel.AddElement(_titleText);
            _panel.AddElement(_availablePointsText);
            _panel.AddElement(_backgroundTexture);
        }

        private void ConfirmChanges(object Sender, MouseButtonEventArgs Args)
        {
            _player.AbilityTree.ConfirmPoints();
        }

        public override void UpdateView()
        {
            if (!Enabled) return;
            var unconfirmedSkills = 0;
            _availablePointsText.Text =
                $"{Translations.Get("available_points")}: {_player.AbilityTree.AvailablePoints}";
            for (var i = 0; i < Buttons.Length; i++)
            {
                _skillPointsBackgroundTextures[i + Offset].Disable();
                ButtonsText[i + Offset].Disable();
                Buttons[i + Offset].Disable();
                Textures[i + Offset].Disable();
                if (!Array[i + Offset].HasAttribute("Enabled")) continue;
                if (Array[i + Offset].GetAttribute<bool>("Enabled"))
                {
                    ButtonsText[i + Offset].Enable();
                    Buttons[i + Offset].Enable();
                    Textures[i + Offset].Enable();

                    var level = Array[i + Offset].HasAttribute("Level")
                        ? Array[i + Offset].GetAttribute<int>("Level")
                        : 0;

                    var isConfirmed = _player.AbilityTree.IsConfirmed(i + Offset);

                    ButtonsText[i].Text = level > 0 ? level.ToString() : string.Empty;
                    ButtonsText[i].Color = isConfirmed ? Color.White : Color.DarkGoldenrod;
                    if (!isConfirmed)
                        unconfirmedSkills++;
                    if (level > 0 && _panel.Enabled)
                        _skillPointsBackgroundTextures[i].Enable();

                    SetGrayscaleIfNecessary(i);
                }
            }

            if (unconfirmedSkills > 0)
            {
                _confirmButton.Enable();
                _confirmButtonText.Enable();
            }
            else
            {
                _confirmButton.Disable();
                _confirmButtonText.Disable();
            }

            _titleText.Text = Translations.Get("skill_tree_title", _blueprint.DisplayName);
            _titleText.Position = _backgroundTexture.Position + _backgroundTexture.Scale.Y * Vector2.UnitY -
                                  _titleText.Scale.Y * Vector2.UnitY;
            _titleText.Grayscale = !IsTreeEnabled;
            UpdateRelationships();
            _specializationPanel.UpdateView();
        }

        private void UpdateRelationships()
        {
            var vertexList = new List<Vector2>();
            var colorList = new List<Vector4>();
            var isFirst = false;
            for (var i = 0; i < _blueprint.Items.Length; i++)
            {
                isFirst = true;
                for (var j = 0; j < _blueprint.Items[i].Length; j++)
                {
                    var index = (_blueprint.Items[i].Length - 1 - j) * AbilityTree.Columns + i;
                    if (!Array[index].HasAttribute("Enabled")) continue;
                    var button = Buttons[index];
                    var item = Array[index];
                    var enabled = item.GetAttribute<bool>("Enabled");
                    var isLocked = item.GetAttribute<int>("Level") == 0;
                    if (enabled)
                    {
                        var skillOffset = !isLocked
                            ? _skillPointsBackgroundTextures[i].Scale.Y * 2 * Vector2.UnitY
                            : Vector2.Zero;
                        var color = new Vector4(isLocked ? Vector3.One * .25f : Vector3.One, button.Texture.Opacity);
                        if (isFirst)
                        {
                            vertexList.Add(button.Texture.AdjustedPosition - button.Scale.Y * Vector2.UnitY -
                                           skillOffset);
                            colorList.Add(color);
                        }
                        else
                        {
                            vertexList.Add(button.Texture.AdjustedPosition + button.Scale.Y * Vector2.UnitY);
                            colorList.Add(color);

                            vertexList.Add(button.Texture.AdjustedPosition - button.Scale.Y * Vector2.UnitY -
                                           skillOffset);
                            colorList.Add(color);
                        }

                        isFirst = false;
                    }
                }

                if (vertexList.Count % 2 != 0) vertexList.RemoveAt(vertexList.Count - 1);
                if (colorList.Count % 2 != 0) colorList.RemoveAt(colorList.Count - 1);
            }

            _linesUI.Update(vertexList.ToArray(), colorList.ToArray());
        }

        public SpecializationInfo SpecializationInfo => _specializationPanel.SpecializationInfo;


        private void SetGrayscaleIfNecessary(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 -
                                   (Index - decomposedIndexY) / AbilityTree.Columns;
            Buttons[Index].Texture.Grayscale =
                decomposedIndexX * 5 > _player.Level || !PreviousUnlocked(Index) || !IsTreeEnabled;
        }

        private bool IsTreeEnabled => _player.AbilityTree.IsTreeEnabled(_blueprint);

        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 -
                                   (Index - decomposedIndexY) / AbilityTree.Columns;
            if (decomposedIndexX == 0) return true;
            else if (!Array[Index + AbilityTree.Columns].GetAttribute<bool>("Enabled"))
                return PreviousUnlocked(Index + AbilityTree.Columns);
            return Array[Index + AbilityTree.Columns].GetAttribute<int>("Level") > 0;
        }

        public void SetBlueprint(AbilityTreeBlueprint Blueprint)
        {
            _blueprint = Blueprint;
            _specializationPanel.Blueprint = Blueprint;
        }

        public void Dispose()
        {
            _specializationPanel.Dispose();
        }
    }
}