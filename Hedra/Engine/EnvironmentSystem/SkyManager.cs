/*
 * Author: Zaphyk
 * Date: 10/02/2016
 * Time: 09:56 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Core;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Numerics;
using Gen = Hedra.Engine.Generation;

namespace Hedra.Engine.EnvironmentSystem
{
    /// <summary>
    /// Description of Sky.
    /// </summary>
    public static class SkyManager
    {
        public static Fog FogManager { get; }
        public static Sky Sky { get; }
        public static WeatherManager Weather { get; }
        public static float LastDayFactor { get; set; }
        public static bool LoadTime { get; set; }
        public static float DaytimeSpeed { get; set; } = 1f;
        public static float DayTime { get; set; } = 12000;
        public static bool Enabled { get; set; } = true;
        public static float TargetIntensity { get; set; } = 1;
        public static bool UpdateDayColors { get; set; } = true;
        public static float StackedDaytime => DayTime + _stackedDaytime;
        public static float StackedDaytimeModifier => Math.Max(0, StackedDaytime / 24000 * 2f);

        private static readonly Stack<float> TimeStack;
        private static Region _currentRegion;
        private static Func<Vector4> _targetTopColor;
        private static Func<Vector4> _nextTargetTopColor;
        private static Func<Vector4> _targetBotColor;
        private static Func<Vector4> _nextTargetBotColor;
        private static Vector4 _targetBiomeTopColor;
        private static Vector4 _nextTargetBiomeTopColor;
        private static Vector4 _targetBiomeBotColor;
        private static Vector4 _nextTargetBiomeBotColor;
        private static Vector3 _targetLightColor;
        private static float _minLight;
        private static float _maxLight;
        private static float _stackedDaytime;
        private static float _intensity;

        static SkyManager()
        {
            TimeStack = new Stack<float>();
            Sky = new Sky();
            FogManager = new Fog();
            Weather = new WeatherManager();
        }

        public static int StackLength => TimeStack.Count;

        public static float PeekTime()
        {
            return TimeStack.Peek();
        }

        public static void PushTime()
        {
            TimeStack.Push(DayTime);
        }

        public static void PopTime()
        {
            SetTime(TimeStack.Peek());
            TimeStack.Pop();
        }
        
        public static void SetTime(float Time)
        {
            DayTime = Time;
            _stackedDaytime = 0;
            LoadTime = true;
        }
        
        public static bool IsSleepTime => DayTime > 18000 || DayTime < 8000;
        
        public static bool IsNight => DayTime > 20000 || DayTime < 6000;

        public static bool Snowing { get; set; }

        private static void UpdateTargets()
        {
            _targetBiomeTopColor = _targetTopColor();
            _targetBiomeBotColor = _targetBotColor();
            _nextTargetBiomeTopColor = _nextTargetTopColor();
            _nextTargetBiomeBotColor = _nextTargetBotColor();
        }

        public static void Update()
        {
            var underChunk = World.GetChunkAt(LocalPlayer.Instance.Position);
            if(underChunk == null) return;
            Weather.Update(underChunk);
            _currentRegion = underChunk.Biome;

            var simplifiedTime = DayTime % 24000;
            var simplifiedFactor = 0f;
            float dayFactor;
             if(simplifiedTime < 12000)
                dayFactor = simplifiedTime / 12000;
            else
                dayFactor = 1-(simplifiedTime - 12000) / 12000;
            if(Enabled)
            {
                if (DayTime >= 24000)
                {
                    _stackedDaytime = DayTime;
                    DayTime = 0;
                }
                DayTime += Time.DeltaTime * 5f * DaytimeSpeed;//20 mins
            }

            if(simplifiedTime >= 10000 && simplifiedTime < 20000 )
            {
                _targetTopColor = () => _currentRegion.Sky.MiddayTop;
                _targetBotColor = () => _currentRegion.Sky.MiddayBot;
            
                _nextTargetTopColor = () => _currentRegion.Sky.AfternoonTop;
                _nextTargetBotColor = () => _currentRegion.Sky.AfternoonBot;
                UpdateTargets();
                TargetIntensity = 1f;
                simplifiedFactor = (simplifiedTime - 10000f) / 10000f;

            }
            
            if(simplifiedTime >= 20000 && simplifiedTime < 22000 )
            {
                _targetTopColor = () => _currentRegion.Sky.AfternoonTop;
                _targetBotColor = () => _currentRegion.Sky.AfternoonBot;
            
                _nextTargetTopColor = () => _currentRegion.Sky.NightTop;
                _nextTargetBotColor = () => _currentRegion.Sky.NightBot;
                UpdateTargets();
                TargetIntensity = .5f;
                simplifiedFactor = (simplifiedTime - 20000f) / 4000f;
            }
            
            if(simplifiedTime >= 22000 && simplifiedTime < 24000 || simplifiedTime >= 0 && simplifiedTime < 8000 )
            {
                _targetTopColor = () => _currentRegion.Sky.NightTop;
                _targetBotColor = () => _currentRegion.Sky.NightBot;
            
                _nextTargetTopColor = () => _currentRegion.Sky.SunriseTop;
                _nextTargetBotColor = () => _currentRegion.Sky.SunriseBot;
                UpdateTargets();
                TargetIntensity = 0f;
                if (simplifiedTime >= 22000 && simplifiedTime < 24000)
                    simplifiedFactor = (24000f - simplifiedTime) / 2000f;
                else
                    simplifiedFactor = simplifiedTime / 8000f;
            }
            
            if(simplifiedTime >= 8000 && simplifiedTime < 10000)
            {
                _targetTopColor = () => _currentRegion.Sky.SunriseTop;
                _targetBotColor = () => _currentRegion.Sky.SunriseBot;
            
                _nextTargetTopColor = () => _currentRegion.Sky.MiddayTop;
                _nextTargetBotColor = () => _currentRegion.Sky.MiddayBot;
                UpdateTargets();
                TargetIntensity = .5f;
                simplifiedFactor = (simplifiedTime - 8000f) / 2000f;
            }
            _intensity = Mathf.Lerp(_intensity, TargetIntensity, Time.IndependentDeltaTime * 2f);
            const float biomeInterpolateSpeed = .3f;
            _minLight = Mathf.Lerp(_minLight, _currentRegion.Sky.MinLight, biomeInterpolateSpeed);
            _maxLight = Mathf.Lerp(_maxLight, _currentRegion.Sky.MaxLight, biomeInterpolateSpeed);

            _targetBiomeTopColor = Mathf.Lerp(_targetBiomeTopColor, _targetTopColor(), Time.IndependentDeltaTime * biomeInterpolateSpeed);
            _targetBiomeBotColor = Mathf.Lerp(_targetBiomeBotColor, _targetBotColor(), Time.IndependentDeltaTime * biomeInterpolateSpeed);
            _nextTargetBiomeTopColor = Mathf.Lerp(_nextTargetBiomeTopColor, _nextTargetTopColor(), Time.IndependentDeltaTime * biomeInterpolateSpeed);
            _nextTargetBiomeBotColor = Mathf.Lerp(_nextTargetBiomeBotColor, _nextTargetBotColor(), Time.IndependentDeltaTime * biomeInterpolateSpeed);

            if (UpdateDayColors)
            {
                Sky.TopColor = Mathf.Lerp(_targetBiomeTopColor, _nextTargetBiomeTopColor, simplifiedFactor);
                Sky.BotColor = Mathf.Lerp(_targetBiomeBotColor, _nextTargetBiomeBotColor, simplifiedFactor);
            }
            if( Math.Abs(dayFactor - LastDayFactor) > .005f || LoadTime)
            {
                FogManager.UpdateFogSettings(FogManager.MinDistance, FogManager.MaxDistance);
                LastDayFactor = dayFactor;
            }
            var avgSkyColor = (Sky.TopColor * .5f + Sky.BotColor * .5f).Xyz() * .5f + Vector3.One * 0.5f * _intensity;
            Vector3 newLightColor = Weather.IsRaining
                ? avgSkyColor * Mathf.Clamp(dayFactor * 0.7f, _minLight, _maxLight)
                : avgSkyColor * Mathf.Clamp(dayFactor * 1.0f, _minLight, _maxLight);

            _targetLightColor = new Vector3(Math.Max(0f, newLightColor.X), Math.Max(0f, newLightColor.Y), Math.Max(0f, newLightColor.Z));
            var previousLightColor = ShaderManager.LightColor;
            var interpolatedLightColor = Mathf.Lerp(ShaderManager.LightColor, _targetLightColor, Time.IndependentDeltaTime * 12f);
            if ((previousLightColor - interpolatedLightColor).Length() > 0.005f)
            {
                ShaderManager.LightColor = interpolatedLightColor;
            }
            LoadTime = false;
        }
        
        public static void Draw()
        {
            Sky.Draw();
        }
    }
}
