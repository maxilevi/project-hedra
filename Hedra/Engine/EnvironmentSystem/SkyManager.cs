/*
 * Author: Zaphyk
 * Date: 10/02/2016
 * Time: 09:56 p.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scenes;
using Gen = Hedra.Engine.Generation;

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Sky.
	/// </summary>
	public static class SkyManager
	{
		public static float DayTime { get; set; } = 12000;
		public static bool IsRaining = false;
		public static ParticleSystem Rain = new ParticleSystem(Vector3.Zero);
		public static bool LoadTime = false, Enabled = true;
	    private static readonly Stack<float> TimeStack = new Stack<float>();
		private static BiomeSystem.Region _currentRegion;
		public static Fog FogManager = new Fog();
		public static Skydome Skydome = new Skydome(12);
		public static Sun Sun = new Sun( new Vector3(-500,1000,0).Normalized() );
	    public static bool UpdateDayColors { get; set; } = true;
	    private static Func<Vector4> _targetTopColor;
	    private static Func<Vector4> _nextTargetTopColor;
	    private static Func<Vector4> _targetBotColor;
	    private static Func<Vector4> _nextTargetBotColor;
	    private static Vector4 _targetBiomeTopColor;
	    private static Vector4 _nextTargetBiomeTopColor;
	    private static Vector4 _targetBiomeBotColor;
	    private static Vector4 _nextTargetBiomeBotColor;
        private static float _minLight;
	    private static float _maxLight;
        public static float DaytimeSpeed = 1f;
	    public static float LastDayFactor, SkyModifier;
	    public static float TargetIntensity = 1;

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
		
		public static void SetTime(float Time){
			DayTime = Time;
			SkyModifier = 6001;
			
			LoadTime = true;
			Networking.NetworkManager.UpdateTime();
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
		    _currentRegion = underChunk.Biome;

            float dayFactor;
			 if(DayTime < 12000)
				dayFactor = DayTime / 12000;
			else
				dayFactor = 1-(DayTime-12000) / 12000;
			if(Enabled){
				if(DayTime >= 24000)//pura noche
					DayTime = 0;
				
				DayTime += Time.ScaledFrameTimeSeconds * 5f * DaytimeSpeed;//20 mins
				SkyModifier += Time.ScaledFrameTimeSeconds * 5f * DaytimeSpeed;
			}

			if(DayTime >= 12000 && DayTime < 18000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.MiddayTop;
				_targetBotColor = () => _currentRegion.Sky.MiddayBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.AfternoonTop;
				_nextTargetBotColor = () => _currentRegion.Sky.AfternoonBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : DayTime - 12000;
			    TargetIntensity = 1;
			}
			
			if(DayTime >= 18000 && DayTime < 24000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.AfternoonTop;
				_targetBotColor = () => _currentRegion.Sky.AfternoonBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.NightTop;
				_nextTargetBotColor = () => _currentRegion.Sky.NightBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : DayTime - 18000;
			    TargetIntensity = 1f;
			}
			
			if(DayTime >= 0 && DayTime < 6000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.NightTop;
				_targetBotColor = () => _currentRegion.Sky.NightBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.SunriseTop;
				_nextTargetBotColor = () => _currentRegion.Sky.SunriseBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : DayTime;
				TargetIntensity = .35f;
			}
			
			if(DayTime >= 6000 && DayTime < 12000 && (SkyModifier >= 6000 || LoadTime) ){
				_targetTopColor = () => _currentRegion.Sky.SunriseTop;
				_targetBotColor = () => _currentRegion.Sky.SunriseBot;
			
				_nextTargetTopColor = () => _currentRegion.Sky.MiddayTop;
				_nextTargetBotColor = () => _currentRegion.Sky.MiddayBot;
			    UpdateTargets();
                SkyModifier = !LoadTime ? 0 : DayTime - 6000;
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
				Vector3 newLightColor = IsRaining 
                    ? Vector3.One * Mathf.Clamp(dayFactor * .7f, _minLight, _maxLight) 
                    : Vector3.One * Mathf.Clamp(dayFactor * 1f, _minLight, _maxLight);

			    newLightColor = new Vector3( Math.Max(0f, newLightColor.X), Math.Max(0f, newLightColor.Y), Math.Max(0f, newLightColor.Z) );
				ShaderManager.LightColor = newLightColor;
			    FogManager.UpdateFogSettings(FogManager.MinDistance, FogManager.MaxDistance);
                LastDayFactor = dayFactor;
			}
			LoadTime = false;

			#region RAIN

			if(Snowing && IsRaining && !GameManager.InMenu && GameManager.Player != null){
				Rain.Position = GameManager.Player.Position + Vector3.UnitY * 160;
				Rain.Color = new Vector4(.5f,.5f,.5f,1);
				Rain.Grayscale = true;
				Rain.VariateUniformly = false;
				Rain.PositionErrorMargin = Vector3.One * new Vector3(320, 64, 320);
				Rain.GravityEffect = 0.0075f;
				Rain.ScaleErrorMargin = Vector3.One * .35f;
				Rain.RandomRotation = true;
				Rain.Scale = Vector3.One * 1.05f;
				Rain.ParticleLifetime = 12.00f;
				
				for(int i = 0; i < 10; i++){
					Rain.Emit();
				}
				
				Rain.Particles[Rain.Particles.Count-1].Collides = true;
			}
			if(IsRaining && Time.timeScale == 1 && GameManager.Player != null && underChunk != null)
            {
				if(!Snowing){
					Rain.Position = GameManager.Player.Position + Vector3.UnitY * 256;
					Rain.Color = underChunk.Biome.Colors.WaterColor * .8f;
					Rain.VariateUniformly = true;
					Rain.Grayscale = false;
					Rain.PositionErrorMargin = Vector3.One * new Vector3(256, 16, 256);
					Rain.GravityEffect = 0.175f;
					Rain.ScaleErrorMargin = Vector3.One * .25f;
					Rain.RandomRotation = true;
					Rain.Scale = Vector3.One * 1f;
					Rain.ParticleLifetime = 8.0f;
					
					for(int i = 0; i < 20; i++){
						Rain.Emit();
					}
					
					Rain.Particles[Rain.Particles.Count-1].Collides = true;
				}
				}
			#endregion

		}
		
		public static void Draw(){
			Skydome.Draw();
		}
	}
}
