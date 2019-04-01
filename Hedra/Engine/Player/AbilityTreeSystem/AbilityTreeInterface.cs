using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
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
        private static uint LabelId { get; }  = Graphics2D.LoadFromAssets("Assets/UI/SkillLabel.png");
        private static Vector2 LabelSize { get; } = Graphics2D.SizeFromAssets("Assets/UI/SkillLabel.png").As1920x1080() * UISizeMultiplier;
        private static uint DefaultId { get; }  = Graphics2D.LoadFromAssets("Assets/UI/AbilityTreeBackground.png");
        private static Vector2 DefaultSize { get; } = Graphics2D.SizeFromAssets("Assets/UI/AbilityTreeBackground.png").As1920x1080() * UISizeMultiplier;

        private readonly Vector2 _targetResolution = new Vector2(1366, 768);
        private readonly IPlayer _player;
        private readonly GUIText _titleText;
        private readonly GUIText _availablePointsText;
        private readonly RenderableTexture _backgroundTexture;
        private readonly RenderableTexture[] _skillPointsBackgroundTextures;
        private readonly TreeLinesUI _linesUI;
        private readonly SpecializationPanel _specializationPanel;
        private AbilityTreeBlueprint _blueprint;

        public AbilityTreeInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine)
            : base(Array, Offset, Length, SlotsPerLine, new Vector2(1.5f, 1.5f))
        {
            _player = Player;
            _skillPointsBackgroundTextures = new RenderableTexture[this.Buttons.Length];
            _backgroundTexture = new RenderableTexture(
                new BackgroundTexture(DefaultId, Mathf.ScaleGui(_targetResolution, new Vector2(.04f, .15f)), DefaultSize * .3f),
                DrawOrder.Before
            );
            _linesUI = new TreeLinesUI();
            _titleText = new GUIText(string.Empty, Vector2.Zero, Color.White, FontCache.GetNormal(16f));
            _availablePointsText = new GUIText(string.Empty, _backgroundTexture.Position - _backgroundTexture.Scale.Y * Vector2.UnitY,
                Color.White, FontCache.GetBold(12f));
            _availablePointsText.Position += _availablePointsText.Scale.Y * Vector2.UnitY * 2;
            for (var i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].Scale = Textures[i].Scale = Textures[i].Scale;
                Buttons[i].Position = Textures[i].Position;
                Buttons[i].Texture.IdPointer = null;
                ButtonsText[i].Color = Color.White;
                ButtonsText[i].TextFont = FontCache.GetBold(10f);
                ButtonsText[i].Disable();
                var skillPointSize = LabelSize * (float)(2.0 / 3.0);
                ButtonsText[i].Position = Textures[i].Position - (Textures[i].Scale.Y + skillPointSize.Y) * Vector2.UnitY;
                _skillPointsBackgroundTextures[i] = 
                    new RenderableTexture(
                        new BackgroundTexture(LabelId, ButtonsText[i].Position, skillPointSize),
                        DrawOrder.Before
                    );

                _panel.AddElement(_skillPointsBackgroundTextures[i]);
            }
            _specializationPanel = new SpecializationPanel(_player, Buttons, Textures, _backgroundTexture);

            _panel.AddElement(_specializationPanel);
            _panel.AddElement(_linesUI);
            _panel.AddElement(_titleText);
            _panel.AddElement(_availablePointsText);
            _panel.AddElement(_backgroundTexture);
        }

        public override void UpdateView()
        {
            if(!Enabled) return;
            _availablePointsText.Text = $"{Translations.Get("available_points")}: {_player.AbilityTree.AvailablePoints}";
            for (var i = 0; i < this.Buttons.Length; i++)
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

                    var level = this.Array[i + Offset].HasAttribute("Level")
                        ? this.Array[i + Offset].GetAttribute<int>("Level")
                        : 0;

                    this.ButtonsText[i].Text = level > 0 ? level.ToString() : string.Empty;
                    if(level > 0 && _panel.Enabled)
                        _skillPointsBackgroundTextures[i].Enable();
                    
                    SetGrayscaleIfNecessary(i);
                }
            }

            _titleText.Text = Translations.Get("skill_tree_title", _blueprint.DisplayName);
            _titleText.Position = _backgroundTexture.Position + _backgroundTexture.Scale.Y * Vector2.UnitY - _titleText.Scale.Y * Vector2.UnitY;
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
                    var index = (_blueprint.Items[i].Length-1 - j) * AbilityTree.Columns + i;
                    if (!Array[index].HasAttribute("Enabled")) continue;
                    var button = Buttons[index];
                    var item = Array[index];
                    var enabled = item.GetAttribute<bool>("Enabled");
                    var isLocked = item.GetAttribute<int>("Level") == 0;
                    if (enabled)
                    {
                        var skillOffset = !isLocked ? _skillPointsBackgroundTextures[i].Scale.Y * 2 * Vector2.UnitY : Vector2.Zero;
                        var color = new Vector4(isLocked ? Vector3.One * .25f : Vector3.One, button.Texture.Opacity);
                        if (isFirst)
                        {
                            vertexList.Add(button.Texture.AdjustedPosition - button.Scale.Y * Vector2.UnitY - skillOffset);
                            colorList.Add(color);
                        }
                        else
                        {
                            vertexList.Add(button.Texture.AdjustedPosition + button.Scale.Y * Vector2.UnitY);
                            colorList.Add(color);

                            vertexList.Add(button.Texture.AdjustedPosition - button.Scale.Y * Vector2.UnitY - skillOffset);
                            colorList.Add(color);
                        }
                        isFirst = false;
                    }
                }
                if (vertexList.Count % 2 != 0) vertexList.RemoveAt(vertexList.Count-1);
                if(colorList.Count % 2 != 0) colorList.RemoveAt(colorList.Count-1);
            }
            _linesUI.Update(vertexList.ToArray(), colorList.ToArray());
        }

        public SpecializationInfo SpecializationInfo
        {
            get => _specializationPanel.SpecializationInfo;
        }
        
        
        private void SetGrayscaleIfNecessary(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 - (Index - decomposedIndexY) / AbilityTree.Columns;
            this.Buttons[Index].Texture.Grayscale = (decomposedIndexX * 5 > _player.Level || !this.PreviousUnlocked(Index)) || !IsTreeEnabled;
        }

        private bool IsTreeEnabled => _player.AbilityTree.IsTreeEnabled(_blueprint);
        
        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 - (Index - decomposedIndexY) / AbilityTree.Columns;
            if (decomposedIndexX == 0) return true;
            else if (!this.Array[Index + AbilityTree.Columns].GetAttribute<bool>("Enabled"))
                return this.PreviousUnlocked(Index + AbilityTree.Columns);
            return this.Array[Index + AbilityTree.Columns].GetAttribute<int>("Level") > 0;
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
