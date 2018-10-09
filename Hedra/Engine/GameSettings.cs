/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 09:51 p.m.
 *
 */

using System;
using System.Reflection;
using System.Text;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public static class GameSettings
    {
        public static bool freezelod { get; set; }
        public static float SurfaceWidth { get; set; }
        public static float SurfaceHeight { get; set; }
        public static bool TestingMode { get; set; }
        public static Vector2 SpawnPoint { get; } = new Vector2(5000, 5000);
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
        public static float DefaultScreenHeight { get; set; }
        public static bool Paused { get; set; }
        public static int MaxLoadingRadius { get; set; } = 32;
        public static int MinLoadingRadius { get; } = 8;
        public static float AmbientOcclusionIntensity = 1;
        public static bool BlurFilter = false;
        public static bool DarkEffect = false;
        public static bool DistortEffect = false;
        public static bool Fancy = true;
        public const float Fov = 85.0f;
        public static bool GlobalShadows = true;
        public static bool Hardcore = false;
        public static bool Lod = true;
        public static bool MaxResolution = false;
        public static bool UnderWaterEffect = false;
        public static float UpdateDistance = 420;
        private static bool _fullscreen;
        private static int _shadowQuality = 2;
        private static int _frameLimit = 60;

        static GameSettings()
        {
#if DEBUG
            DebugMode = true;
#endif
        }

        [Setting]
        public static int FrameLimit {
            get => _frameLimit;
            set
            {
                _frameLimit = value;
                //Program.GameWindow.TargetRenderPeriod = 1.0 / FrameLimit;
                //Program.GameWindow.TargetRenderFrequency = 60;
                //Program.GameWindow.TargetUpdateFrequency = 60;
            }
        }

        [Setting] public static bool Bloom { get; set; } = true;

        [Setting] public static bool Autosave = true;

        [Setting] public static int ChunkLoaderRadius { get; set; } = 20;

        [Setting] public static bool HideObjectives = false;

        [Setting] public static bool InvertMouse = false;

        [Setting] public static float MouseSensibility = 1f;

        [Setting] public static bool ShowChat = true;

        [Setting] public static bool ShowMinimap = true;

        [Setting] public static bool SSAO = true;

        [Setting]
        public static bool ShowConsole
        {
            get => OSManager.ShowConsole;
            set => OSManager.ShowConsole = value;
        }

        [Setting]
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
        public static bool FXAA { get; set; } = true;

        [Setting]
        public static float MusicVolume
        {
            get { return SoundtrackManager.Volume; }
            set { SoundtrackManager.Volume = value; }
        }

        [Setting]
        public static float SFXVolume
        {
            get { return SoundManager.Volume; }
            set { SoundManager.Volume = value; }
        }

        [Setting]
        public static int ShadowQuality
        {
            get { return (int) Mathf.Clamp(_shadowQuality, 0, 3); }
            set
            {
                _shadowQuality = value;
                ShadowRenderer.SetQuality(ShadowQuality);
            }
        }

        public static bool Shadows => ShadowQuality != 0 && GlobalShadows;

        [Setting]
        public static bool VSync
        {
            get => (int) Program.GameWindow.VSync == 2;
            set => Program.GameWindow.VSync = (VSyncMode) (value ? 2 : 0);
        }

        public static void Save(string File)
        {
            var builder = new StringBuilder();

            foreach (FieldInfo field in typeof(GameSettings).GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                if (field.IsDefined(typeof(SettingAttribute), true))
                    builder.AppendLine(field.Name + "=" + field.GetValue(null));

            foreach (PropertyInfo prop in typeof(GameSettings).GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                if (prop.IsDefined(typeof(SettingAttribute), true))
                    builder.AppendLine(prop.Name + "=" + prop.GetValue(null, null));

            System.IO.File.WriteAllText(File, builder.ToString());
        }

        public static void Load(string File)
        {
            if (!System.IO.File.Exists(File)) return;

            string[] lines = System.IO.File.ReadAllLines(File);
            for (var i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('=');

                foreach (FieldInfo field in typeof(GameSettings).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    if (field.IsDefined(typeof(SettingAttribute), true))
                        if (field.Name == parts[0]) field.SetValue(null, Convert.ChangeType(parts[1], field.FieldType));

                foreach (PropertyInfo prop in typeof(GameSettings).GetProperties(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    if (prop.IsDefined(typeof(SettingAttribute), true))
                        if (prop.Name == parts[0])
                            prop.SetValue(null, Convert.ChangeType(parts[1], prop.PropertyType), null);
            }
        }
    }
}