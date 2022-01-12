/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

#define DONATE_BTC

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.Player;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of OptionsUI.
    /// </summary>
    public class OptionsUI : Panel
    {
        private static readonly Translation OnTranslation;
        private static readonly Translation OffTranslation;
        private readonly Button _audio;
        private readonly Font _boldFont;
        private readonly Button _controls;
        private readonly Button _display;
        private readonly Button _graphics;
        private readonly Button _input;
        private readonly Font _normalFont;
        private readonly List<UIElement> _audioButtons = new List<UIElement>();
        private readonly ControlsUI _controlsPanel;
        private readonly List<UIElement> _displayButtons = new List<UIElement>();
        private readonly List<UIElement> _graphicsButtons = new List<UIElement>();
        private readonly List<UIElement> _inputButtons = new List<UIElement>();
        private Vector2 _previousOffset;

        static OptionsUI()
        {
            OnTranslation = Translation.Create("on");
            OffTranslation = Translation.Create("off");
        }

        public OptionsUI()
        {
            var fontSize = 14;
            var dist = .2f;
            var vDist = .25f;
            _normalFont = FontCache.GetNormal(fontSize - 1);
            _boldFont = FontCache.GetBold(fontSize + 1);
            var fontColor = Color.White;
            _controlsPanel = new ControlsUI();

            var bandPosition = new Vector2(0f, .8f);
            var blackBand = new BackgroundTexture(Color.FromRgb(69, 69, 69), Color.FromRgb(19, 19, 19),
                bandPosition, UserInterface.BlackBandSize, GradientType.LeftRight);

            _graphics = new Button(new Vector2(0f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("graphics"), Color.White, _normalFont);
            _graphics.Click += delegate
            {
                SetGraphicsButtonState(true);
                SetAudioButtonState(false);
                SetInputButtonState(false);
                SetDisplayButtonState(false);
                SetControlsButtonState(false);
            };

            _audio = new Button(new Vector2(-0.2f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("audio"), Color.White, _normalFont);

            _audio.Click += delegate
            {
                SetGraphicsButtonState(false);
                SetAudioButtonState(true);
                SetInputButtonState(false);
                SetDisplayButtonState(false);
                SetControlsButtonState(false);
            };

            _input = new Button(new Vector2(-0.4f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("input"), Color.White, _normalFont);

            _input.Click += delegate
            {
                SetGraphicsButtonState(false);
                SetAudioButtonState(false);
                SetInputButtonState(true);
                SetDisplayButtonState(false);
                SetControlsButtonState(false);
            };

            _display = new Button(new Vector2(0.2f, bandPosition.Y), new Vector2(0.15f, 0.05f),
                Translation.Create("display"), Color.White, _normalFont);

            _display.Click += delegate
            {
                SetGraphicsButtonState(false);
                SetAudioButtonState(false);
                SetInputButtonState(false);
                SetDisplayButtonState(true);
                SetControlsButtonState(false);
            };

            _controls = new Button(new Vector2(0.4f, bandPosition.Y), new Vector2(0.15f, 0.05f),
                Translation.Create("controls"), Color.White, _normalFont);
            _controls.Click += delegate
            {
                SetGraphicsButtonState(false);
                SetAudioButtonState(false);
                SetInputButtonState(false);
                SetDisplayButtonState(false);
                SetControlsButtonState(true);
            };


            GameSettings.ChunkLoaderRadius = Math.Max(GameSettings.ChunkLoaderRadius, GeneralSettings.MinLoadingRadius);
            var viewValuesList = new List<string>();
            for (var i = GeneralSettings.MinLoadingRadius; i < GeneralSettings.MaxLoadingRadius + 1; i++)
                viewValuesList.Add(i.ToString());

            var viewValues = viewValuesList.ToArray();

            var viewDistance = new OptionChooser(new Vector2(dist, vDist * 2f), new Vector2(0.15f, 0.075f),
                Translation.Create("view_distance", "{0} : "),
                fontColor, _normalFont,
                viewValues.Select(Translation.Default).ToArray())
            {
                Index = (GeneralSettings.MaxLoadingRadius - GeneralSettings.MinLoadingRadius) / 2,
                CurrentValue =
                    { Text = viewValues[(GeneralSettings.MaxLoadingRadius - GeneralSettings.MinLoadingRadius) / 2] }
            };

            for (var i = 0; i < viewValues.Length; i++)
                if (int.Parse(viewValues[i]) == GameSettings.ChunkLoaderRadius)
                {
                    viewDistance.CurrentValue.Text = viewValues[i];
                    viewDistance.Index = i;
                }

            viewDistance.LeftArrow.Click += delegate
            {
                GameSettings.ChunkLoaderRadius = viewDistance.Index + GeneralSettings.MinLoadingRadius;
            };

            viewDistance.RightArrow.Click += delegate
            {
                GameSettings.ChunkLoaderRadius = viewDistance.Index + GeneralSettings.MinLoadingRadius;
            };

            var fpsLimitList = new List<Translation>();
            fpsLimitList.AddRange(Enumerable.Range(0, 12 + 1 + 22).Select(I => (I * 5 + 30).ToString())
                .Select(Translation.Default));
            fpsLimitList.Add(Translation.Create("none"));

            var frameLimiterValues = fpsLimitList.ToArray();


            var frameLimiter = new OptionChooser(new Vector2(-dist, 0), new Vector2(0.15f, 0.075f),
                Translation.Create("fps_limit", "{0} :"),
                fontColor, _normalFont,
                frameLimiterValues);

            if (GameSettings.FrameLimit <= 0.0f)
                frameLimiter.Index = fpsLimitList.Count - 1;
            else
                frameLimiter.Index = Enumerable.Range(0, fpsLimitList.Count - 1)
                    .FirstOrDefault(I => GameSettings.FrameLimit == int.Parse(fpsLimitList[I].Get()));
            frameLimiter.CurrentValue.Text = fpsLimitList[frameLimiter.Index].Get();

            void UpdateLimiter(object Sender, MouseButtonEventArgs E)
            {
                GameSettings.FrameLimit =
                    frameLimiter.Index == fpsLimitList.Count - 1 ? 0 : int.Parse(frameLimiter.CurrentValue.Text);
            }

            frameLimiter.LeftArrow.Click += UpdateLimiter;
            frameLimiter.RightArrow.Click += UpdateLimiter;

            var occlusionCulling = new Button(new Vector2(dist, 0),
                new Vector2(0.15f, 0.075f), BuildOnOff("occlusion_culling", () => GameSettings.OcclusionCulling),
                fontColor, _normalFont);

            occlusionCulling.Click += delegate { GameSettings.OcclusionCulling = !GameSettings.OcclusionCulling; };

            var fxaa = new Button(new Vector2(dist, -vDist),
                new Vector2(0.15f, 0.075f), BuildOnOff("fxaa", () => GameSettings.FXAA), fontColor, _normalFont);

            fxaa.Click += delegate { GameSettings.FXAA = !GameSettings.FXAA; };

            var bloom = new Button(new Vector2(dist, -vDist * 2),
                new Vector2(0.15f, 0.075f), BuildOnOff("bloom", () => GameSettings.Bloom), fontColor, _normalFont);


            bloom.Click += delegate { GameSettings.Bloom = !GameSettings.Bloom; };

            var quality = new Button(
                new Vector2(-dist, vDist * 2),
                new Vector2(0.15f, 0.075f),
                BuildOnOff("quality", () => GameSettings.Quality, Translation.Create("quality_fancy"),
                    Translation.Create("quality_fast")),
                fontColor, _normalFont);

            quality.Click += delegate { GameSettings.Quality = !GameSettings.Quality; };


            var vSync = new Button(new Vector2(-dist, vDist),
                new Vector2(0.15f, 0.075f), BuildOnOff("vsync", () => GameSettings.VSync), fontColor, _normalFont);

            vSync.Click += delegate { GameSettings.VSync = !GameSettings.VSync; };

            var invertMouse = new Button(new Vector2(0, .6f),
                new Vector2(0.15f, 0.075f), BuildOnOff("invert_mouse", () => GameSettings.InvertMouse), fontColor,
                _normalFont);

            invertMouse.Click += delegate { GameSettings.InvertMouse = !GameSettings.InvertMouse; };

            var shadowsValues = new[]
            {
                "none", "low", "medium", "high"
            }.Select(S => Translation.Create(S)).ToArray();
            var shadows = new OptionChooser(new Vector2(dist, vDist), new Vector2(0.15f, 0.075f),
                Translation.Create("shadow_quality", "{0}: "),
                fontColor, _normalFont,
                shadowsValues);
            shadows.Index = 1;
            shadows.CurrentValue.Text = Translations.Get("medium");

            for (var i = 0; i < shadowsValues.Length; i++)
                if (i == GameSettings.ShadowQuality)
                {
                    shadows.CurrentValue.Text = i == 0 ? Translations.Get("off") : shadowsValues[i].Get();
                    shadows.Index = i;
                    break;
                }

            shadows.LeftArrow.Click += delegate
            {
                GameSettings.ShadowQuality = shadows.Index;
                if (shadows.Index == 0)
                    shadows.CurrentValue.Text = Translations.Get("off");
            };

            shadows.RightArrow.Click += delegate
            {
                GameSettings.ShadowQuality = shadows.Index;
                if (shadows.Index == 0)
                    shadows.CurrentValue.Text = Translations.Get("off");
            };
            var ssao = new Button(new Vector2(-dist, -vDist * 2),
                new Vector2(0.15f, 0.075f), BuildOnOff("ambient_occlusion", () => GameSettings.SSAO), fontColor,
                _normalFont);

            ssao.Click += delegate { GameSettings.SSAO = !GameSettings.SSAO; };

            var ssr = new Button(new Vector2(-dist, -vDist * 3), Vector2.Zero,
                BuildOnOff("water_reflections", () => GameSettings.EnableReflections), fontColor, _normalFont);

            ssr.Click += delegate { GameSettings.EnableReflections = !GameSettings.EnableReflections; };


            var fullscreen = new Button(new Vector2(-dist, -vDist),
                new Vector2(0.15f, 0.075f), BuildOnOff("fullscreen", () => GameSettings.Fullscreen), fontColor,
                _normalFont);

            fullscreen.Click += delegate { GameSettings.Fullscreen = !GameSettings.Fullscreen; };

            var showChat = new Button(new Vector2(0, .4f),
                new Vector2(0.15f, 0.075f), BuildOnOff("show_chat", () => GameSettings.ShowChat), fontColor,
                _normalFont);

            showChat.Click += delegate { GameSettings.ShowChat = !GameSettings.ShowChat; };

            var showMinimap = new Button(new Vector2(0, .6f),
                new Vector2(0.15f, 0.075f), BuildOnOff("show_minimap", () => GameSettings.ShowMinimap), fontColor,
                _normalFont);

            showMinimap.Click += delegate { GameSettings.ShowMinimap = !GameSettings.ShowMinimap; };

            Button showConsole = null;
            if (OSManager.CanHideConsole)
            {
                showConsole = new Button(new Vector2(0, .2f),
                    new Vector2(0.15f, 0.075f), BuildOnOff("show_console", () => GameSettings.ShowConsole),
                    fontColor, _normalFont);

                showConsole.Click += delegate { GameSettings.ShowConsole = !GameSettings.ShowConsole; };
            }

            var langs = Translations.Languages;
            var languageOptions = new Translation[langs.Length];
            for (var i = 0; i < langs.Length; i++) languageOptions[i] = Translation.Default(langs[i]);

            var language = new OptionChooser(new Vector2(0, -.2f), Vector2.Zero,
                Translation.Create("language", "{0}: "),
                fontColor, _normalFont, languageOptions)
            {
                Index = Array.IndexOf(languageOptions.Select(T => T.Get()).ToArray(), GameSettings.Language)
            };
            language.CurrentValue.Text = languageOptions[language.Index].Get();
            language.LeftArrow.Click += delegate { GameSettings.Language = languageOptions[language.Index].Get(); };
            language.RightArrow.Click += delegate { GameSettings.Language = languageOptions[language.Index].Get(); };


            var fovOptions = Enumerable.Range(40, 80 + 1).Select(I => I.ToString()).Select(Translation.Default)
                .ToArray();
            var fovChooser = new OptionChooser(new Vector2(0, -.4f), new Vector2(0.15f, 0.075f),
                Translation.Create("field_of_view", "{0}: "), fontColor, _normalFont, fovOptions);
            for (var i = 0; i < fovOptions.Length; i++)
            {
                if (Math.Abs(GameSettings.FieldOfView - int.Parse(fovOptions[i].Get())) > 0.005f) continue;
                fovChooser.Index = i;
                fovChooser.CurrentValue.Text = fovOptions[i].Get();
                break;
            }

            fovChooser.LeftArrow.Click += (S, A) =>
            {
                GameSettings.FieldOfView = int.Parse(fovOptions[fovChooser.Index].Get());
            };

            fovChooser.RightArrow.Click += (S, A) =>
            {
                GameSettings.FieldOfView = int.Parse(fovOptions[fovChooser.Index].Get());
            };

            void RestartNotice()
            {
                LocalPlayer.Instance.MessageDispatcher.ShowNotification(
                    Translations.Get("restart_game_changes_take_effect"), Color.Red, 3f);
            }

            ;

            var resolutionChooser = new OptionChooser(new Vector2(0, -.6f), new Vector2(0.15f, 0.075f),
                Translation.Create("available_resolutions", "{0}: "), fontColor, _normalFont,
                GameSettings.AvailableResolutions.Select(V => Translation.Default(V.ToString())).ToArray());

            void SetResolutionName()
            {
                resolutionChooser.CurrentValue.Text = GameSettings.AvailableResolutions[GameSettings.ResolutionIndex]
                    .ToString().Replace("<", "(").Replace(">", ")").Replace(".", ",");
            }

            for (var i = 0; i < GameSettings.AvailableResolutions.Length; i++)
            {
                resolutionChooser.Index = GameSettings.ResolutionIndex;
                SetResolutionName();
                break;
            }

            resolutionChooser.LeftArrow.Click += (S, A) =>
            {
                GameSettings.ResolutionIndex = Mathf.Modulo(GameSettings.ResolutionIndex - 1,
                    GameSettings.AvailableResolutions.Length);
                SetResolutionName();
                RestartNotice();
            };

            resolutionChooser.RightArrow.Click += (S, A) =>
            {
                GameSettings.ResolutionIndex = Mathf.Modulo(GameSettings.ResolutionIndex + 1,
                    GameSettings.AvailableResolutions.Length);
                SetResolutionName();
                RestartNotice();
            };

            #region ScaleUI

            var scaleOptions = new[]
            {
                "0.5", "0.6", "0.7", "0.8", "0.9", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9",
                "2.0"
            }.Select(Translation.Default).ToArray();
            var scaleUI = new OptionChooser(
                new Vector2(0, -.8f), Vector2.Zero,
                Translation.Create("scale_interface", "{0}: "),
                fontColor, _normalFont, scaleOptions);

            scaleUI.LeftArrow.Click += delegate
            {
                GameSettings.UIScaling =
                    float.Parse(scaleOptions[scaleUI.Index].Get(), NumberStyles.Any, CultureInfo.InvariantCulture);
                RestartNotice();
            };

            scaleUI.RightArrow.Click += delegate
            {
                GameSettings.UIScaling =
                    float.Parse(scaleOptions[scaleUI.Index].Get(), NumberStyles.Any, CultureInfo.InvariantCulture);
                RestartNotice();
            };

            for (var i = 0; i < scaleOptions.Length; i++)
                if (Math.Abs(float.Parse(scaleOptions[i].Get(), NumberStyles.Any, CultureInfo.InvariantCulture) -
                             GameSettings.UIScaling) < 0.005f)
                {
                    scaleUI.CurrentValue.Text = scaleOptions[i].Get();
                    scaleUI.Index = i;
                }

            #endregion

            var smoothLod = new Button(new Vector2(0, .0f),
                new Vector2(0.15f, 0.075f), BuildOnOff("smooth_lod", () => GameSettings.SmoothLod),
                fontColor, _normalFont);

            smoothLod.Click += delegate { GameSettings.SmoothLod = !GameSettings.SmoothLod; };

            var volumeOptions = new[]
            {
                "0%", "5%", "10%", "15%", "20%", "25%", "30%", "35%", "40%", "45%", "50%",
                "55%", "60%", "65%", "70%", "75%", "80%", "85%", "90%", "95%", "100%"
            }.Select(Translation.Default).ToArray();
            var musicVolume = new OptionChooser(new Vector2(0, .6f), new Vector2(0.15f, 0.075f),
                Translation.Create("music_volume", "{0}: "), fontColor, _normalFont, volumeOptions);

            for (var i = 0; i < volumeOptions.Length; i++)
                if (Math.Abs(int.Parse(volumeOptions[i].Get().Replace("%", string.Empty)) / 100f -
                             SoundtrackManager.Volume) < 0.005f)
                {
                    musicVolume.Index = i;
                    musicVolume.CurrentValue.Text = volumeOptions[i].Get();
                    break;
                }

            musicVolume.LeftArrow.Click += delegate
            {
                GameSettings.MusicVolume =
                    int.Parse(volumeOptions[musicVolume.Index].Get().Replace("%", string.Empty)) / 100f;
            };

            musicVolume.RightArrow.Click += delegate
            {
                GameSettings.MusicVolume =
                    int.Parse(volumeOptions[musicVolume.Index].Get().Replace("%", string.Empty)) / 100f;
            };

            var sfxVolume = new OptionChooser(new Vector2(0, .4f), new Vector2(0.15f, 0.075f),
                Translation.Create("sfx_volume", "{0}: "), fontColor, _normalFont,
                volumeOptions);

            for (var i = 0; i < volumeOptions.Length; i++)
                if (Math.Abs(int.Parse(volumeOptions[i].Get().Replace("%", string.Empty)) / 100f -
                             SoundPlayer.Volume) < 0.005f)
                {
                    sfxVolume.Index = i;
                    sfxVolume.CurrentValue.Text = volumeOptions[i].Get();
                    break;
                }

            sfxVolume.LeftArrow.Click += (Sender, Args) =>
                SoundPlayer.Volume = int.Parse(volumeOptions[sfxVolume.Index].Get().Replace("%", string.Empty)) / 100f;

            sfxVolume.RightArrow.Click += (Sender, Args) =>
                SoundPlayer.Volume = int.Parse(volumeOptions[sfxVolume.Index].Get().Replace("%", string.Empty)) / 100f;

            var sensitivityOptions = new[]
            {
                "0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0",
                "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0",
                "2.1", "2.2", "2.3", "2.4", "2.5"
            }.Select(Translation.Default).ToArray();
            var mouseSensitivity = new OptionChooser(
                new Vector2(0, .4f), Vector2.Zero,
                Translation.Create("mouse_sensitivity", "{0}: "),
                fontColor, _normalFont, sensitivityOptions);

            mouseSensitivity.LeftArrow.Click += delegate
            {
                GameSettings.MouseSensibility =
                    float.Parse(sensitivityOptions[mouseSensitivity.Index].Get(), NumberStyles.Any,
                        CultureInfo.InvariantCulture);
            };

            mouseSensitivity.RightArrow.Click += delegate
            {
                GameSettings.MouseSensibility =
                    float.Parse(sensitivityOptions[mouseSensitivity.Index].Get(), NumberStyles.Any,
                        CultureInfo.InvariantCulture);
            };

            for (var i = 0; i < sensitivityOptions.Length; i++)
                if (Math.Abs(
                    float.Parse(sensitivityOptions[i].Get(), NumberStyles.Any, CultureInfo.InvariantCulture) -
                    GameSettings.MouseSensibility) < 0.005f)
                {
                    mouseSensitivity.CurrentValue.Text = sensitivityOptions[i].Get();
                    mouseSensitivity.Index = i;
                }

            var autosave = new Button(new Vector2(0f, .2f),
                new Vector2(0.15f, 0.075f), BuildOnOff("autosave", () => GameSettings.Autosave), fontColor,
                _normalFont);

            autosave.Click += delegate { GameSettings.Autosave = !GameSettings.Autosave; };

            var smoothCamera = new Button(new Vector2(0, 0), Vector2.Zero,
                BuildOnOff("smooth_camera", () => GameSettings.SmoothCamera), fontColor,
                _normalFont);

            smoothCamera.Click += delegate { GameSettings.SmoothCamera = !GameSettings.SmoothCamera; };


            _graphicsButtons.Add(quality);
            _graphicsButtons.Add(vSync);
            _graphicsButtons.Add(shadows);
            _graphicsButtons.Add(ssao);
            _graphicsButtons.Add(viewDistance);
            _graphicsButtons.Add(fxaa);
            _graphicsButtons.Add(frameLimiter);
            _graphicsButtons.Add(bloom);
            _graphicsButtons.Add(fullscreen);
            _graphicsButtons.Add(occlusionCulling);
            _graphicsButtons.Add(ssr);
            _inputButtons.Add(invertMouse);
            _inputButtons.Add(mouseSensitivity);
            _inputButtons.Add(autosave);
            _inputButtons.Add(smoothCamera);
            _audioButtons.Add(musicVolume);
            _audioButtons.Add(sfxVolume);
            _displayButtons.Add(showChat);
            _displayButtons.Add(showMinimap);
            _displayButtons.Add(smoothLod);
            _displayButtons.Add(language);
            _displayButtons.Add(fovChooser);
            _displayButtons.Add(scaleUI);
            _displayButtons.Add(resolutionChooser);
            if (showConsole != null) _displayButtons.Add(showConsole);

            AddElement(_controls);
            AddElement(blackBand);
            AddElement(_audio);
            AddElement(_input);
            AddElement(_graphics);
            AddElement(_display);


            for (var i = 0; i < _graphicsButtons.Count; i++) AddElement(_graphicsButtons[i]);
            for (var i = 0; i < _inputButtons.Count; i++) AddElement(_inputButtons[i]);
            for (var i = 0; i < _audioButtons.Count; i++) AddElement(_audioButtons[i]);
            for (var i = 0; i < _displayButtons.Count; i++) AddElement(_displayButtons[i]);
            AddElement(_controlsPanel);

            Disable();

            OnEscapePressed += delegate
            {
                Disable();
                GameManager.Player.UI.Menu.Enable();
            };

            OnPanelStateChange += delegate(object Sender, PanelState E)
            {
                if (E == PanelState.Disabled)
                {
                    if (GameSettings.Loaded)
                        GameSettings.Save($"{AssetManager.AppData}/settings.cfg");
                    GameSettings.DarkEffect = false;
                }

                if (E == PanelState.Enabled)
                {
                    SetGraphicsButtonState(true);
                    SetAudioButtonState(false);
                    SetInputButtonState(false);
                    SetDisplayButtonState(false);
                    SetControlsButtonState(false);
                    GameSettings.DarkEffect = true;
                }
            };
        }

        public void Update()
        {
            _controlsPanel.Update();
        }

        private void ResetFonts()
        {
            _graphics.Text.TextFont = _normalFont;
            _input.Text.TextFont = _normalFont;
            _audio.Text.TextFont = _normalFont;
            _display.Text.TextFont = _normalFont;
            _controls.Text.TextFont = _normalFont;
        }

        private void UpdateFonts()
        {
            _graphics.Text.UpdateText();
            _input.Text.UpdateText();
            _audio.Text.UpdateText();
            _display.Text.UpdateText();
            _controls.Text.UpdateText();
        }

        private void SetGraphicsButtonState(bool Enabled)
        {
            if (Enabled)
            {
                ResetFonts();
                _graphics.Text.TextFont = _boldFont;
                UpdateFonts();
            }

            for (var i = 0; i < _graphicsButtons.Count; i++)
                if (Enabled)
                    _graphicsButtons[i].Enable();
                else
                    _graphicsButtons[i].Disable();
        }

        private void SetInputButtonState(bool Enabled)
        {
            if (Enabled)
            {
                ResetFonts();
                _input.Text.TextFont = _boldFont;
                UpdateFonts();
            }

            for (var i = 0; i < _inputButtons.Count; i++)
                if (Enabled)
                    _inputButtons[i].Enable();
                else
                    _inputButtons[i].Disable();
        }

        private void SetAudioButtonState(bool Enabled)
        {
            if (Enabled)
            {
                ResetFonts();
                _audio.Text.TextFont = _boldFont;
                UpdateFonts();
            }

            for (var i = 0; i < _audioButtons.Count; i++)
                if (Enabled)
                    _audioButtons[i].Enable();
                else
                    _audioButtons[i].Disable();
        }

        private void SetDisplayButtonState(bool Enabled)
        {
            if (Enabled)
            {
                ResetFonts();
                _display.Text.TextFont = _boldFont;
                UpdateFonts();
            }

            for (var i = 0; i < _displayButtons.Count; i++)
                if (Enabled)
                    _displayButtons[i].Enable();
                else
                    _displayButtons[i].Disable();
        }

        private static Translation BuildOnOff(string Key, Func<bool> Getter,
            Translation OptionalOn = null, Translation OptionalOff = null)
        {
            var on = OptionalOn ?? OnTranslation;
            var off = OptionalOff ?? OffTranslation;
            var trans = Translation.Create(Key, "{0} : ");
            trans.Concat(() => Getter() ? on.Get() : off.Get());
            return trans;
        }

        private void SetControlsButtonState(bool Enabled)
        {
            if (Enabled)
            {
                ResetFonts();
                _controls.Text.TextFont = _boldFont;
                UpdateFonts();
            }

            if (Enabled)
                _controlsPanel.Enable();
            else
                _controlsPanel.Disable();
        }
    }
}