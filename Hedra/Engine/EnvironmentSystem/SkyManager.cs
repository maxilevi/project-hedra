/*
 * Author: Zaphyk
 * Date: 10/02/2016
 * Time: 09:56 p.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Gen = Hedra.Engine.Generation;

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Sky.
	/// </summary>
	public static class SkyManager
	{
	    public static Fog FogManager { get; }
	    public static Skydome Skydome { get; }
	    public static Sun Sun { get; }
        public static WeatherManager Weather { get; }
        public static float LastDayFactor { get; set; }
	    public static float SkyModifier { get; set; }
	    public static bool LoadTime { get; set; }
	    public static float DaytimeSpeed { get; set; } = 1f;
        public static float DayTime { get; set; } = 12000;
	    public static bool Enabled { get; set; } = true;
	    public static float TargetIntensity { get; set; } = 1;
        public static bool UpdateDayColors { get; set; } = true;

	    private static readonly Stack<float> TimeStack;
        private static BiomeSystem.Region _currentRegion;
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

	    static SkyManager()
	    {
	        TimeStack = new Stack<float>();
            Skydome = new Skydome();
	        FogManager = new Fog();
            Sun = new Sun(new Vector3(-500, 1000, 0).Normalized());
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
			SkyModifier = 6001;
			
			LoadTime = true;
		}
		
		public static bool IsNight => DayTime > 16000 || DayTime < 10000;

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
		    var underChunk = Gen.World.GetChunkAt(LocalPlayer.Instance.BlockPosition);
            if(underChunk == null) return;
		    Weather.Update(underChunk);
		    _currentRegion = underChunk.Biome;

		    var simplifiedTime = DayTime % 24000;
            float dayFactor;
			 if(simplifiedTime < 12000)
				dayFactor = simplifiedTime / 12000;
			else
				dayFactor = 1-(simplifiedTime - 12000) / 12000;
			if(Enabled){
				if(DayTime >= 24000)//pura noche
					DayTime = 0;
				
				DayTime += Time.ScaledFrameTimeSeconds * 5f * DaytimeSpeed;//20 mins
				SkyModifier += Time.ScaledFrameTimeSeconds * 5f * DaytimeSpeed;
			}

			if(simplifiedTime >= 12000 && simplifiedTime < 18000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.MiddayTop;
				_targetBotColor = () => _currentRegion.Sky.MiddayBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.AfternoonTop;
				_nextTargetBotColor = () => _currentRegion.Sky.AfternoonBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : simplifiedTime - 12000;
			    TargetIntensity = 1;
			}
			
			if(simplifiedTime >= 18000 && simplifiedTime < 24000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.AfternoonTop;
				_targetBotColor = () => _currentRegion.Sky.AfternoonBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.NightTop;
				_nextTargetBotColor = () => _currentRegion.Sky.NightBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : simplifiedTime - 18000;
			    TargetIntensity = 1f;
			}
			
			if(simplifiedTime >= 0 && simplifiedTime < 6000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.NightTop;
				_targetBotColor = () => _currentRegion.Sky.NightBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.SunriseTop;
				_nextTargetBotColor = () => _currentRegion.Sky.SunriseBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : simplifiedTime;
				TargetIntensity = .35f;
			}
			
			if(simplifiedTime >= 6000 && simplifiedTime < 12000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.SunriseTop;
				_targetBotColor = () => _currentRegion.Sky.SunriseBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.MiddayTop;
				_nextTargetBotColor = () => _currentRegion.Sky.MiddayBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : simplifiedTime - 6000;
			    TargetIntensity = .5f;
			}
		    const float biomeInterpolateSpeed = .3f;
		    _minLight = Mathf.Lerp(_minLight, _currentRegion.Sky.MinLight, biomeInterpolateSpeed);
		    _maxLight = Mathf.Lerp(_maxLight, _currentRegion.Sky.MaxLight, biomeInterpolateSpeed);

            _targetBiomeTopColor = Mathf.Lerp(_targetBiomeTopColor, _targetTopColor(), Time.FrameTimeSeconds * biomeInterpolateSpeed);
		    _targetBiomeBotColor = Mathf.Lerp(_targetBiomeBotColor, _targetBotColor(), Time.FrameTimeSeconds * biomeInterpolateSpeed);
		    _nextTargetBiomeTopColor = Mathf.Lerp(_nextTargetBiomeTopColor, _nextTargetTopColor(), Time.FrameTimeSeconds * biomeInterpolateSpeed);
		    _nextTargetBiomeBotColor = Mathf.Lerp(_nextTargetBiomeBotColor, _nextTargetBotColor(), Time.FrameTimeSeconds * biomeInterpolateSpeed);

		    if (UpdateDayColors)
		    {
		        Skydome.TopColor = Mathf.Lerp(_targetBiomeTopColor, _nextTargetBiomeTopColor, SkyModifier / 6000);
		        Skydome.BotColor = Mathf.Lerp(_targetBiomeBotColor, _nextTargetBiomeBotColor, SkyModifier / 6000);
		    }
		    if( Math.Abs(dayFactor - LastDayFactor) > .005f || LoadTime){
		        FogManager.UpdateFogSettings(FogManager.MinDistance, FogManager.MaxDistance);
                LastDayFactor = dayFactor;
			}
		    Vector3 newLightColor = Weather.IsRaining
		        ? Vector3.One * Mathf.Clamp(dayFactor * .7f, _minLight, _maxLight)
		        : Vector3.One * Mathf.Clamp(dayFactor * 1f, _minLight, _maxLight);

		    _targetLightColor = new Vector3(Math.Max(0f, newLightColor.X), Math.Max(0f, newLightColor.Y), Math.Max(0f, newLightColor.Z));
            var previousLightColor = ShaderManager.LightColor;
		    var interpolatedLightColor = Mathf.Lerp(ShaderManager.LightColor, _targetLightColor, Time.unScaledDeltaTime * 12f);
		    if ((previousLightColor - interpolatedLightColor).Length > 0.005f)
		    {
		        ShaderManager.LightColor = interpolatedLightColor;
		    }
            LoadTime = false;
		}
		
		public static void Draw(){
			Skydome.Draw();
		}
	}
}
