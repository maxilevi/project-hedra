/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 09:51 p.m.
 *
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Sound;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;

namespace Hedra.Game
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public static class GameSettings
    {
        public const int MaxCharacters = 4; 
        public static bool HideWorld { get; set; }
        public static bool ContinousMove { get; set; }
        public static float SurfaceWidth { get; set; }
        public static float SurfaceHeight { get; set; }
        public static bool TestingMode { get; set; }
        public static float BloomModifier { get; set; } = 1f;
        public static bool Wireframe { get; set; }
        public static bool LockFrustum { get; set; }
        public static bool DebugMode { get; set; }
        public static bool DebugView { get; set; }
        public static int DeviceWidth { get; set; }
        public static int DeviceHeight { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static float ScreenRatio { get; set; }
        public static bool DebugPhysics { get; set; }
        public static bool WatchScriptChanges { get; set; }
        public static bool UseSSR => SSAO && EnableReflections;
        public static bool Paused { get; set; }
        public static float AmbientOcclusionIntensity = 1;
        public static bool BlurFilter { get; set; } = false;
        public static bool DebugAI { get; set; }
        public static bool DebugFrustum { get; set; }
        public static bool NewWorld { get; set; }
        public static bool DarkEffect = false;
        public static bool DistortEffect = false;
        public const float Fov = 85.0f;
        public static bool GlobalShadows = true;
        public static bool Hardcore = false;
        public static bool UnderWaterEffect = false;
        private static bool _fullscreen;
        private static int _shadowQuality = 2;
        private static int _frameLimit;

        static GameSettings()
        {
//#if DEBUG
            DebugMode = true;
//#endif
            WatchScriptChanges = false; // DebugMode;
        }

        public static string SettingsPath => $"{GameLoader.AppData}settings.cfg";
        
        public static bool Shadows => ShadowQuality != 0 && GlobalShadows;
        
        [Setting] public static bool OcclusionCulling { get; set; } = false;
        
        [Setting] public static bool Quality { get; set; } = true;
        
        [Setting] public static bool SmoothLod { get; set; } = true;
        
        [Setting] public static bool Bloom { get; set; } = true;

        [Setting] public static bool Autosave { get; set; } = true;

        [Setting] public static int ChunkLoaderRadius { get; set; } = 20;

        [Setting] public static bool InvertMouse { get; set; } = false;

        [Setting] public static float MouseSensibility { get; set; } = 1f;

        [Setting] public static bool ShowChat { get; set; } = true;

        [Setting] public static bool ShowMinimap { get; set; } = true;

        [Setting] public static bool SSAO { get; set; } = true;
        
        [Setting] public static bool EnableReflections { get; set; } = true;
        
        [Setting] public static bool FXAA { get; set; } = true;
        
        [Setting]
        public static int FrameLimit
        {
            get => _frameLimit;
            set
            {
                _frameLimit = value;
                Program.GameWindow.TargetFramerate = value == 0 ? 0.0 : 1.0 / value;
            }
        }

        [Setting]
        public static bool ShowConsole
        {
            get => OSManager.ShowConsole;
            set => OSManager.ShowConsole = value;
        }

        [Setting]
        [WindowSetting]
        public static bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                _fullscreen = value;
                if (_fullscreen)
                {
                    Program.GameWindow.WindowBorder = WindowBorder.Hidden;
                    Program.GameWindow.WindowState = WindowState.Fullscreen;
                }
                else
                {
                   Program.GameWindow.WindowBorder = WindowBorder.Resizable;
                   Program.GameWindow.WindowState = WindowState.Maximized;
                }
            }
        }

        [Setting]
        public static float MusicVolume
        {
            get => SoundtrackManager.Volume;
            set => SoundtrackManager.Volume = value;
        }

        [Setting]
        public static float SFXVolume
        {
            get => SoundPlayer.Volume;
            set => SoundPlayer.Volume = value;
        }
        
        [Setting]
        public static string Language
        {
            get => Translations.Language;
            set => Translations.Language = value;
        }

        [Setting]
        public static int ShadowQuality
        {
            get => (int) Mathf.Clamp(_shadowQuality, 0, 3);
            set
            {
                _shadowQuality = value;
                ShadowRenderer.SetQuality(ShadowQuality);
            }
        }

        [Setting]
        [WindowSetting]
        public static bool VSync
        {
            get => (int) Program.GameWindow.VSync == 2;
            set => Program.GameWindow.VSync = (VSyncMode) (value ? 2 : 0);
        }

        public static void Save(string Path)
        {
            var builder = new StringBuilder();
            var properties = GatherProperties();
            for (var i = 0; i < properties.Length; i++)
            {
                builder.AppendLine($"{properties[i].Name}={properties[i].GetValue(null, null)}");
            }
            File.WriteAllText(Path, builder.ToString());
        }

        public static void LoadAll(string Path)
        {
            LoadNormalSettings(Path);
            LoadWindowSettings(Path);
        }

        public static void LoadNormalSettings(string Path)
        {
            Load(Path, P => !P.IsDefined(typeof(WindowSettingAttribute), true));
        }
        
        public static void LoadWindowSettings(string Path)
        {
            Load(Path, P => P.IsDefined(typeof(WindowSettingAttribute), true));
        }

        private static void Load(string Path, Predicate<PropertyInfo> Predicate)
        {
            if (!File.Exists(Path)) return;

            var lines = File.ReadAllLines(Path);
            var properties = GatherProperties();
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('=');
                for (var k = 0; k < properties.Length; k++)
                {
                    if (!Predicate(properties[k])) continue;
                    if (properties[k].Name != parts[0]) continue;
                    var value = ConvertString(parts[1], properties[k].PropertyType);
                    properties[k].SetValue(null, value, null);
                }
            }
        }

        private static PropertyInfo[] GatherProperties()
        {
            return
                typeof(GameSettings).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(P => P.IsDefined(typeof(SettingAttribute), true)).ToArray();
        }
        
        private static object ConvertString(string Value, Type Type)
        {
            return Type.IsEnum ? Enum.Parse(Type, Value) : Convert.ChangeType(Value, Type);
        }
    }
}