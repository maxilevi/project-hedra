/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/08/2016
 * Time: 12:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    public class ControlsUI : Panel
    {
        private readonly GUIText[] _labels;
        private readonly Button[] _backgrounds;
        private readonly GUIText[] _keys;
        private readonly string[] _controlMapKeys;
        private readonly Vector2[] _keyPositions;
        private readonly GUIText _resetToDefaultsText;
        private readonly Button _resetToDefaults;
        private readonly Timer _timer;
        private int _currentSelected = -1;
        
        public ControlsUI()
        {
            _timer = new Timer(.5f)
            {
                UseTimeScale = false
            };
            _controlMapKeys = Controls.ChangeableKeys.OrderBy(P => Translations.Get(P.Key)).Select(P => P.Key).ToArray();
            _labels = new GUIText[_controlMapKeys.Length];
            _keys = new GUIText[_controlMapKeys.Length];
            _keyPositions = new Vector2[_controlMapKeys.Length];
            _backgrounds = new Button[_controlMapKeys.Length];
            const float spacing = 2.0f;
            var columnPosition = Vector2.UnitX * .2f;
            var normalFont = FontCache.GetNormal(12);
            var boldFont = FontCache.GetBold(15);
            var accumulatedOffset = Vector2.UnitY * .6f;
            var backgroundTexId = Graphics2D.ColorTexture(Color.FromArgb(170, 20, 20, 20).ToVector4());
            var maxWidth = 0f;
            for (var i = 0; i < _controlMapKeys.Length; ++i)
            {
                _backgrounds[i] = new Button(
                    _keyPositions[i] = columnPosition + accumulatedOffset,
                    Vector2.Zero, 
                    backgroundTexId
                );
                _labels[i] = new GUIText(Translation.Create(_controlMapKeys[i]), -columnPosition + accumulatedOffset, Color.White, normalFont);
                _labels[i].Position += _labels[i].Scale.X * Vector2.UnitX;
                maxWidth = Math.Max(maxWidth, _labels[i].Scale.X);
                _keys[i] = new GUIText(Translation.Default("ControlLeft"), _keyPositions[i], Color.White, boldFont);
                
                SetButtonParameters(_backgrounds[i], _keys[i]);
                
                AddElement(_backgrounds[i]);
                AddElement(_labels[i]);
                AddElement(_keys[i]);
                accumulatedOffset -= Vector2.UnitY * _labels[i].Scale.Y * 2 * spacing;
            }

            for (var i = 0; i < _controlMapKeys.Length; ++i)
                _labels[i].Position -= maxWidth * Vector2.UnitX;
            
            _resetToDefaults = new Button(accumulatedOffset, Vector2.One, backgroundTexId);
            _resetToDefaultsText = new GUIText(Translation.Create("reset_to_default"), _resetToDefaults.Position, Color.White, FontCache.GetBold(14));
            SetButtonParameters(_resetToDefaults, _resetToDefaultsText);
            _resetToDefaults.Position += _resetToDefaults.Scale.X * Vector2.UnitX * 0f;
            _resetToDefaultsText.Position = _resetToDefaults.Position;
            AddElement(_resetToDefaultsText);
            AddElement(_resetToDefaults);
            Controls.OnControlsChanged += FillMappings;
            FillMappings();
            EventDispatcher.RegisterKeyDown(this, OnKeyDown);
        }

        private void SetButtonParameters(Button Background, GUIText Text)
        {
            Background.Click += (Sender, Args) => OnClick(Background, Args);
            Background.HoverEnter += (Sender, Args) =>
            {
                Text.TextColor = Color.OrangeRed;
                Text.Scale *= 1.25f;
                Background.Scale *= 1.25f;
            };
            Background.HoverExit += (Sender, Args) =>
            {
                /* Text is recreated when changing the color, so there is no need to edit the scale */
                Text.TextColor = Color.White;
                Background.Scale /= 1.25f;
            };
            Background.Scale = Text.Scale.X * 1.05f * Vector2.UnitX + Vector2.UnitY * (Text.Scale.Y * 1.25f);
        }
        
        private void FillMappings()
        {
            for (var i = 0; i < _keys.Length; ++i)
            {
                UpdateControlUI(i);
            }
        }

        public void Update()
        {
            if(_currentSelected == -1) return;
            if (_timer.Tick())
            {
                if (_keys[_currentSelected].Text == " ")
                    _keys[_currentSelected].SetTranslation(Translation.Default("."));
                else
                    _keys[_currentSelected].SetTranslation(Translation.Default(" "));
            }
        }
        
        private void OnClick(Button Sender, MouseButtonEventArgs Args)
        {
            if (_currentSelected != -1) Reset();
            _currentSelected = Array.IndexOf(_backgrounds, Sender);
            _keys[_currentSelected].SetTranslation(Translation.Default("."));
        }

        private void Reset()
        {
            UpdateControlUI(_currentSelected);
            _currentSelected = -1;
        }

        private void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            if (_currentSelected == -1) return;
            if (CanUseKey(_controlMapKeys[_currentSelected], Args.Key, out var conflictName))
            {
                Controls.UpdateMapping(_controlMapKeys[_currentSelected], Args.Key);
                _currentSelected = -1;
            }
            else
            {
                var index = Array.IndexOf(_controlMapKeys, conflictName);
                _keys[index].TextColor = Color.Red;
                TaskScheduler.After(.5f,() => _keys[index].TextColor = Color.White);
                GameManager.Player.MessageDispatcher.ShowNotification(Translations.Get("key_already_used", Args.Key), Color.DarkRed, 1f);
            }
            Args.Cancel();
        }
        
        private static bool CanUseKey(string Name, Key New, out string ConflictName)
        {
            var current = Controls.ChangeableKeys;
            foreach (var pair in current)
            {
                if (pair.Value == New && pair.Key != Name)
                {
                    ConflictName = pair.Key;
                    return false;
                }
            }
            ConflictName = default(string);
            return true;
        }

        private void UpdateControlUI(int Index)
        {
            _keys[Index].SetTranslation(Translation.Default(Controls.ChangeableKeys[_controlMapKeys[Index]].ToString()));
            //_backgrounds[Index].Scale = (_keys[Index].Scale.X * 1.5f) * Vector2.UnitX + Vector2.UnitY * (_keys[Index].Scale.Y);
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
