﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/05/2016
 * Time: 12:28 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{	
	public static class ShaderManager
	{
		public const int LightDistance = 256;
		public const int MaxLights = 12;
	    private static readonly List<Shader> Shaders;
	    private static readonly PointLight[] PointLights;
	    private static Vector3 _lightPosition;
	    private static Vector3 _lightColor;
	    private static float _clipPlaneY;

        static ShaderManager(){
            Shaders = new List<Shader>();
            PointLights = new PointLight[MaxLights];
            _lightPosition = new Vector3(0, 1000, 0);
            _lightColor = new Vector3(1, 1, 1);
            for (int i = 0; i < PointLights.Length; i++){
				PointLights[i] = new PointLight();
			}
		}
		
		public static void RegisterShader(Shader Entry){
			Shaders.Add(Entry);
			//Entry.ClipPlaneLocation = GL.GetUniformLocation(Entry.ShaderID, "ClipPlane");
			Entry.LightColorLocation = GL.GetUniformLocation(Entry.ShaderId, "LightColor");
		    Entry.LightPositionLocation = GL.GetUniformLocation(Entry.ShaderId, "LightPosition");
            Entry.PointLightsColorUniform = new int[MaxLights];
			Entry.PointLightsPositionUniform = new int[MaxLights];
			Entry.PointLightsRadiusUniform = new int[MaxLights];
			
			for(int i = 0; i < Entry.PointLightsColorUniform.Length; i++){
				Entry.PointLightsColorUniform[i] = GL.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Color");
			}
			for(int i = 0; i < Entry.PointLightsPositionUniform.Length; i++){
				Entry.PointLightsPositionUniform[i] = GL.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Position");
			}
			for(int i = 0; i < Entry.PointLightsRadiusUniform.Length; i++){
				Entry.PointLightsRadiusUniform[i] = GL.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Radius");
			}
	
			//Entry.PlayerPositionUniform = GL.GetUniformLocation(Entry.ShaderID, "PlayerPosition");
			//GL.Uniform1(Entry.ClipPlaneLocation, m_ClipPlaneY);
			//GL.Uniform3(Entry.LightColorLocation, m_LightColor);
			//GL.Uniform3(Entry.LightPositionLocation, m_LightPosition);
			//GL.Uniform3(Entry.PlayerPositionUniform);
		}
		
		public static PointLight GetAvailableLight(){
			for(var i = 0; i < PointLights.Length; i++){
				if(!PointLights[i].Locked)
				{
				    PointLights[i].Radius = PointLight.DefaultRadius;
				    PointLights[i].Color = Vector3.Zero;
				    PointLights[i].Position = Vector3.Zero;
                    PointLights[i].Locked = true;
					return PointLights[i];
				}
			}
			return null;
		}

		public static void UpdateLight(PointLight Light){
            if(!Light.Locked)
                Light.Radius = PointLight.DefaultRadius;

            int prevShader = GraphicsLayer.ShaderBound;
			for(int i = 0; i < Shaders.Count; i++){
				int k = i;
				int j = Array.IndexOf(PointLights, Light);
				if(j == -1) continue;
				if(Shaders[i].PointLightsColorUniform[ j ] != -1)
					ThreadManager.ExecuteOnMainThread ( delegate{
					                                   	GL.UseProgram(Shaders[k].ShaderId);
					                                   	GL.Uniform3(Shaders[k].PointLightsColorUniform[ j ], Light.Color);
					                                    GL.Uniform3(Shaders[k].PointLightsPositionUniform[ j ], Light.Position);
					                                    GL.Uniform1(Shaders[k].PointLightsRadiusUniform[ j ], Light.Radius);
					                                   } );
			}
			ThreadManager.ExecuteOnMainThread ( delegate{ 
			                                   	GL.UseProgram(prevShader);
												GraphicsLayer.ShaderBound = prevShader;
			                                   } );

		}
		
		public static Vector3 LightColor{
			get{ return _lightColor; }
			set{
				_lightColor = value;
				int prevShader = GraphicsLayer.ShaderBound;
				for(int i = 0; i < Shaders.Count; i++){
					int k = i;
					if(Shaders[i].LightColorLocation != -1)
						ThreadManager.ExecuteOnMainThread ( delegate{ 
						                                   	GL.UseProgram(Shaders[k].ShaderId);
						                                   	GL.Uniform3(Shaders[k].LightColorLocation, value); 
						                                   } );
				}
				GL.UseProgram(prevShader);
				GraphicsLayer.ShaderBound = prevShader;
			}
		}

		public static Vector3 LightPosition{
			get{ return _lightPosition; }
			set{
				if(_lightPosition == value) return;
				
				_lightPosition = value;
				int prevShader = GraphicsLayer.ShaderBound;
				for(int i = 0; i < Shaders.Count; i++){
					int k = i;
					if(Shaders[i].LightPositionLocation != -1)
						ThreadManager.ExecuteOnMainThread ( delegate{
						                                   	GL.UseProgram(Shaders[k].ShaderId);
						                                   	GL.Uniform3(Shaders[k].LightPositionLocation, _lightPosition);
						                                   } );
				}
				ThreadManager.ExecuteOnMainThread ( delegate{ 
				                                   	GL.UseProgram(prevShader);
													GraphicsLayer.ShaderBound = prevShader;
				                                   } );
			}
		}

		public static int UsedLights{
			get{
				int UsedLights = 0;
				for(int i = 0; i < PointLights.Length; i++)
					if(PointLights[i].Locked) UsedLights++;
				
				return UsedLights;
			}
		}
		
		public static Vector3 PlayerPosition{
			set{
				for(int i = 0; i < Shaders.Count; i++){
					//Shaders[i].Bind();
					//GL.Uniform3(Shaders[i].PlayerPositionUniform, value);
				}
			}
		}

		public static float ClipPlaneY{
			get{ return _clipPlaneY; }
			set{
				_clipPlaneY = value;
				for(int i = 0; i < Shaders.Count; i++){
					int k = 0;
				//	ThreadManager.ExecuteOnMainThread ( () => GL.Uniform1(Shaders[k].ClipPlaneLocation, value) );
				}
			}
		}
	}
}