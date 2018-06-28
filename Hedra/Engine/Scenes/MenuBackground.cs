/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/01/2017
 * Time: 11:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hedra.Engine.Generation;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// Description of MenuBackground.
	/// </summary>
	internal static class MenuBackground
	{
		
		public static bool Campfire = false, Creator = false;
		public static Vector3 DefaultPosition = new Vector3(50867 - 3500, 28, 49208 - 30);
		public static Vector3 CampfirePosition = new Vector3(DefaultPosition.X + 64, 4.25f,DefaultPosition.Z + 40);
		public static Vector3 CreatorPosition = new Vector3(DefaultPosition.X + 64, 2.75f,DefaultPosition.Z + 80 );
		public static Vector3 FirePosition = CampfirePosition.Xz.ToVector3() + Vector3.UnitX * 4;
		public static Vector3 PlatformPosition = CreatorPosition.Xz.ToVector3() + Vector3.UnitX * 4;
		public static Dictionary<Vector3, int> Locations = new Dictionary<Vector3, int>();
		public static float Increment = 0;
		
		public static void Setup(){
			CoroutineManager.StartCoroutine(MakeFire);
			CoroutineManager.StartCoroutine(MakePlatform);
		}
		
		private static IEnumerator MakePlatform(){
			Chunk UnderChunk = null;
			while(UnderChunk == null){
				Chunk NewChunk = World.GetChunkAt(CreatorPosition);
				if(NewChunk != null){
					if(NewChunk.Landscape.StructuresPlaced)
						UnderChunk = NewChunk; 
				}
				yield return null;
			}
			
			PlatformPosition = CreatorPosition.Xz.ToVector3() + Vector3.UnitX * 4 + Vector3.UnitY * (Physics.HeightAtPosition(PlatformPosition +  Vector3.UnitX * 4));
			
			//VertexData CenterModel = AssetManager.PlyLoader("Assets/Intro/CreationPlatform.ply", Vector3.One);
			//CenterModel.Transform( PlatformPosition );
			//UnderChunk.AddStaticElement(CenterModel);
			//World.AddChunkToQueue(UnderChunk, true);
		}
		
		private static IEnumerator MakeFire(){
      
			Chunk UnderChunk = null;
			while(UnderChunk == null){
				Chunk NewChunk = World.GetChunkAt(CampfirePosition);
				if(NewChunk != null){
					if(NewChunk.Landscape.StructuresPlaced)
						UnderChunk = NewChunk; 
				}
				yield return null;
			}
			FirePosition = CampfirePosition.Xz.ToVector3() + Vector3.UnitX * 4 + Vector3.UnitY * (Physics.HeightAtPosition(CampfirePosition +  Vector3.UnitX * 4));
			
			VertexData CenterModel = AssetManager.PlyLoader("Assets/Env/Campfire2.ply", Vector3.One * 3f);
			CenterModel.Transform( FirePosition );
			UnderChunk.AddStaticElement(CenterModel);
			LocalPlayer.Instance.UI.ChrChooser.ReloadFiles();
			CoroutineManager.StartCoroutine(MenuUpdate);
		}
		
		private static PointLight _light;
	    private static SoundItem _sound;
		private static IEnumerator MenuUpdate(){

            while (World.Seed == World.MenuSeed){
			    if (_light == null)
			    {
			        _light = ShaderManager.GetAvailableLight();
			        if (_light != null)
			        {
			            _light.Color = new Vector3(1f, 0.35f, 0.35f);
			            _light.Position = FirePosition;
			        }
			    }


			    if (_sound == null)
			    {
			        _sound = SoundManager.GetAvailableSource();
			        yield return null;
			        yield return null;
			        yield return null;
			        yield return null;
			        float gain = Math.Max(0, 1 - (FirePosition - SoundManager.ListenerPosition).LengthFast / 32f);
                    _sound?.Source.Play(SoundManager.GetBuffer(SoundType.Fireplace), FirePosition, 1f, gain, true);
			    }

                if (_sound != null)
                {
                    float gain = System.Math.Max(0, 1 - (FirePosition - SoundManager.ListenerPosition).LengthFast / 32f);
                    _sound.Source.Volume = gain;

                }

                yield return null;
				yield return null;
				World.Particles.Color = new Vector4(1f,.5f,0,1f);
				World.Particles.VariateUniformly = false;
				World.Particles.Position = FirePosition + Vector3.UnitY * 1f;
				World.Particles.Scale = Vector3.One * .65f;
				World.Particles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
				World.Particles.Direction = Vector3.UnitY * 0f;
				World.Particles.ParticleLifetime = 1.15f;
				World.Particles.GravityEffect = -0.01f;
				World.Particles.PositionErrorMargin = new Vector3(1.15f, 0.0f, 1.15f);
				Particle3D.UseTimeScale = false;
				
				for(int i = 0; i < 1; i++)
					World.Particles.Emit();
				
				ShaderManager.UpdateLight(_light);
            }

		    if (_light != null)
		    {
		        _light.Color = Vector3.Zero;
		        _light.Position = Vector3.Zero;
		        ShaderManager.UpdateLight(_light);
		        _light.Locked = false;
		        _light = null;
            }

		    if (_sound != null)
		    {
		        _sound.Source.Stop();
		        _sound.Locked = false;
		        _sound = null;
		    }
		    Particle3D.UseTimeScale = true;
		}
		
		private static Vector3 Position;
		private static float LerpTime = 12000;
		public static Vector3 NewLocation{
			get{
				float TargetTime = 0;
				Vector3 TargetPosition = Vector3.Zero;
				
				if(Campfire){
					GameSettings.DarkEffect = false;
					TargetPosition = CampfirePosition;
					TargetTime = 24000;
				}else if(!Campfire && !Creator){
					TargetPosition =  DefaultPosition;
					TargetTime = 12000;
				}else if(Creator){
					TargetPosition = CreatorPosition;
					TargetTime = 12000;
				}
				LerpTime = Mathf.Lerp(LerpTime, TargetTime, Time.unScaledDeltaTime * 2f);
				Position = Mathf.Lerp(Position, TargetPosition, Time.unScaledDeltaTime * 2f);
                SkyManager.SetTime(LerpTime);
                return new Vector3(Position.X, Physics.HeightAtPosition(Position.X, Position.Z)+Position.Y, Position.Z);
			}	
		}
	}
}
