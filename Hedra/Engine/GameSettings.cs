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
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine
{
    /// <summary>
    ///     Description of GraphicsOptions.
    /// </summary>
    public static class GameSettings
    {
        public static Vector2 SpawnPoint { get; } = new Vector2(0, 0);
        public static int DeviceWidth;
        public static int DeviceHeight;
        public static int Width;
        public static int Height;
        public static float ScreenRatio;
        public static float DefaultScreenHeight;
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
        private static float _fpsLimit;


        [Setting]
        public static float FpsLimit {
            get { return _fpsLimit; }
            set
            {
                _fpsLimit = value;
                Program.GameWindow.TargetRenderFrequency = FpsLimit;
                Program.GameWindow.TargetUpdateFrequency = FpsLimit;
            }
        }

        [Setting] private static bool _bloom;

        [Setting] public static bool Autosave = true;

        [Setting] public static int ChunkLoaderRadius = 20;

        [Setting] public static bool HideObjectives = false;

        [Setting] public static bool InvertMouse = false;

        [Setting] public static float MouseSensibility = 1f;

        [Setting] public static bool ShowChat = true;

        [Setting] public static bool ShowMinimap = true;

        [Setting] public static bool ShowTargetedMobs = true;

        [Setting] public static bool SSAO = true;

        public static bool BakedAO => !SSAO;

        public static bool Bloom
        {
            get { return _bloom; }
            set { _bloom = value; }
        }

        //[SettingAttribute]
        public static bool Fullscreen
        {
            get { return _fullscreen; }
            set
            {
                _fullscreen = value;
                if (_fullscreen)
                {
                    GameSettings.Width = GameSettings.DeviceWidth;
                    GameSettings.Height = GameSettings.DeviceHeight;

                    //Program.GameWindow.Height = GameSettings.Height;
                    Program.GameWindow.WindowBorder = WindowBorder.Hidden;
                    Program.GameWindow.WindowState = WindowState.Fullscreen;
                }
                else
                {
                    Program.GameWindow.WindowBorder = WindowBorder.Resizable;
                    Program.GameWindow.WindowState = WindowState.Maximized;

                    GameSettings.Width = Program.GameWindow.ClientSize.Width;
                    GameSettings.Height = Program.GameWindow.ClientSize.Height;
                }
                MainFBO.DefaultBuffer.Resize();
                UserInterface.PlayerFbo.Dispose();
                UserInterface.PlayerFbo = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);
                SceneManager.Game.LPlayer.UI = new UserInterface(SceneManager.Game.LPlayer);
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
            get { return (int) Program.GameWindow.VSync == 2; }
            set { Program.GameWindow.VSync = (VSyncMode) (value ? 2 : 0); }
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