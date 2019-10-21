/*
 * Author: Zaphyk
 * Date: 13/03/2016
 * Time: 08:39 p.m.
 *
 */
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Effects;
using Hedra.Game;
using Hedra.Engine.Core;

namespace Hedra.Engine.EnvironmentSystem
{
    /// <summary>
    /// Description of Fog.
    /// </summary>
    public sealed class Fog
    {
        public float MaxDistance { get; private set; }
        public float MinDistance { get; private set; }
        public Vector4 BottomColor { get; private set; }
        private readonly int _fogSettingsIndex;
        private readonly int _fogSettingsSize;

        
        public void UpdateFogSettings(float MinDist, float MaxDist)
        {
            MinDistance = MinDist;
            MaxDistance = MaxDist;
            var data = new FogData(MinDist, MaxDist, GameSettings.Height * (1 - GameManager.Player.View.Pitch * .25f), BottomColor = SkyManager.Sky.BotColor, SkyManager.Sky.TopColor);
            ShaderManager.FogUBO.Update(data);
        }
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct FogData
    {
        [FieldOffset(0)]
        public Vector4 U_BotColor;
        [FieldOffset(16)]
        public Vector4 U_TopColor;
        [FieldOffset(32)]
        public float MinDist;
        [FieldOffset(36)]
        public float MaxDist;
        [FieldOffset(40)]
        public float U_Height;
        
        public FogData(float MinDist, float MaxDist, float Height, Vector4 U_BotColor, Vector4 U_TopColor)
        {
            this.MinDist = MinDist;
            this.MaxDist = MaxDist;
            this.U_Height = Height;
            this.U_BotColor = U_BotColor;
            this.U_TopColor = U_TopColor;
        }
    };
}
