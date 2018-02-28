﻿/*
 * Author: Zaphyk
 * Date: 04/03/2016
 * Time: 09:51 p.m.
 *
 */

using System;
using System.IO;
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
        public static bool Paused { get; set; }
        private static bool _fullscreen;
        private static int _shadowQuality = 2;
        private static float _fpsLimit;
        public static float AmbientOcclusionIntensity = 1;
        public static bool BlurFilter = false;
        public static bool DarkEffect = false;
        public static bool DistortEffect = false;
        public static bool Fancy = true;
        public const float Fov = 85.0f;
        public static bool GlobalShadows = true;
        public static bool Hardcore = false;
        public static bool LOD = true;
        public static bool MaxResolution = false;
        public static bool UnderWaterEffect = false;
        public static float UpdateDistance = 420;
        public static int MaxLoadingRadius { get; set; } = 32;
        public static int MinLoadingRadius { get; } = 8;


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
                    Constants.WIDTH = Constants.DEVICE_WIDTH;
                    Constants.HEIGHT = Constants.DEVICE_HEIGHT;

                    //Program.GameWindow.Height = Constants.HEIGHT;
                    Program.GameWindow.WindowBorder = WindowBorder.Hidden;
                    Program.GameWindow.WindowState = WindowState.Fullscreen;
                }
                else
                {
                    Program.GameWindow.WindowBorder = WindowBorder.Resizable;
                    Program.GameWindow.WindowState = WindowState.Maximized;

                    Constants.WIDTH = Program.GameWindow.ClientSize.Width;
                    Constants.HEIGHT = Program.GameWindow.ClientSize.Height;
                }
                MainFBO.DefaultBuffer.Resize();
                UserInterface.PlayerFbo.Dispose();
                UserInterface.PlayerFbo = new FBO(Constants.WIDTH / 2, Constants.HEIGHT / 2);
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