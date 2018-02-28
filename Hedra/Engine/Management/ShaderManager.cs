/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/05/2016
 * Time: 12:28 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Management
{
	public class PointLight
	{
	    public const float DefaultRadius = 20f;
        public Vector3 Position;
	    public Vector3 Color;
	    public float Radius = DefaultRadius;
	    public bool Locked;
	    
	}
	
	public static class ShaderManager
	{
		public const int LightDistance = 256;
		public const int MaxLights = 12;
		private static readonly List<Shader> Shaders = new List<Shader>();
		private static readonly PointLight[] PointLights = new PointLight[MaxLights];
		
		static ShaderManager(){
			for(int i = 0; i < PointLights.Length; i++){
				PointLights[i] = new PointLight();
			}
		}
		
		public static void RegisterShader(Shader Entry){
			Shaders.Add(Entry);
			//Entry.ClipPlaneLocation = GL.GetUniformLocation(Entry.ShaderID, "ClipPlane");
			Entry.LightColorLocation = GL.GetUniformLocation(Entry.ShaderID, "LightColor");
			Entry.PointLightsColorUniform = new int[MaxLights];
			Entry.PointLightsPositionUniform = new int[MaxLights];
			Entry.PointLightsRadiusUniform = new int[MaxLights];
			
			for(int i = 0; i < Entry.PointLightsColorUniform.Length; i++){
				Entry.PointLightsColorUniform[i] = GL.GetUniformLocation(Entry.ShaderID, "Lights["+i+"].Color");
			}
			for(int i = 0; i < Entry.PointLightsPositionUniform.Length; i++){
				Entry.PointLightsPositionUniform[i] = GL.GetUniformLocation(Entry.ShaderID, "Lights["+i+"].Position");
			}
			for(int i = 0; i < Entry.PointLightsRadiusUniform.Length; i++){
				Entry.PointLightsRadiusUniform[i] = GL.GetUniformLocation(Entry.ShaderID, "Lights["+i+"].Radius");
			}
	
			//Entry.PlayerPositionUniform = GL.GetUniformLocation(Entry.ShaderID, "PlayerPosition");
			//Entry.LightPositionLocation = GL.GetUniformLocation(Entry.ShaderID, "LightPosition");
			//GL.Uniform1(Entry.ClipPlaneLocation, m_ClipPlaneY);
			//GL.Uniform3(Entry.LightColorLocation, m_LightColor);
			//GL.Uniform3(Entry.LightPositionLocation, m_LightPosition);
			//GL.Uniform3(Entry.PlayerPositionUniform);
		}
		
		public static PointLight GetAvailableLight(){
			for(int i = 0; i < PointLights.Length; i++){
				if(!PointLights[i].Locked){
					PointLights[i].Locked = true;
					return PointLights[i];
				}
			}
			return null;
		}

		public static void UpdateLight(PointLight Light){
            if(!Light.Locked)
                Light.Radius = PointLight.DefaultRadius;

            int PrevShader = GraphicsLayer.ShaderBound;
			for(int i = 0; i < Shaders.Count; i++){
				int k = i;
				int j = Array.IndexOf(PointLights, Light);
				if(j == -1) continue;
				if(Shaders[i].PointLightsColorUniform[ j ] != -1)
					ThreadManager.ExecuteOnMainThread ( delegate{
					                                   	GL.UseProgram(Shaders[k].ShaderID);
					                                   	GL.Uniform3(Shaders[k].PointLightsColorUniform[ j ], Light.Color);
					                                    GL.Uniform3(Shaders[k].PointLightsPositionUniform[ j ], Light.Position);
					                                    GL.Uniform1(Shaders[k].PointLightsRadiusUniform[ j ], Light.Radius);
					                                   } );
			}
			ThreadManager.ExecuteOnMainThread ( delegate{ 
			                                   	GL.UseProgram(PrevShader);
												GraphicsLayer.ShaderBound = PrevShader;
			                                   } );

		}
		
		private static Vector3 m_LightColor = new Vector3(1,1,1);
		public static Vector3 LightColor{
			get{ return m_LightColor; }
			set{
				m_LightColor = value;
				int PrevShader = GraphicsLayer.ShaderBound;
				for(int i = 0; i < Shaders.Count; i++){
					int k = i;
					if(Shaders[i].LightColorLocation != -1)
						ThreadManager.ExecuteOnMainThread ( delegate{ 
						                                   	GL.UseProgram(Shaders[k].ShaderID);
						                                   	GL.Uniform3(Shaders[k].LightColorLocation, value); 
						                                   } );
				}
				GL.UseProgram(PrevShader);
				GraphicsLayer.ShaderBound = PrevShader;
			}
		}
		private static Vector3 m_LightPosition = new Vector3(0,1000,0);
		public static Vector3 LightPosition{
			get{ return m_LightPosition; }
			set{
				if(m_LightPosition == value) return;
				
				m_LightPosition = value;
				int PrevShader = GraphicsLayer.ShaderBound;
				for(int i = 0; i < Shaders.Count; i++){
					int k = i;
					if(Shaders[i].LightPositionLocation != -1)
						ThreadManager.ExecuteOnMainThread ( delegate{
						                                   	GL.UseProgram(Shaders[k].ShaderID);
						                                   	GL.Uniform3(Shaders[k].LightPositionLocation, m_LightPosition);
						                                   } );
				}
				ThreadManager.ExecuteOnMainThread ( delegate{ 
				                                   	GL.UseProgram(PrevShader);
													GraphicsLayer.ShaderBound = PrevShader;
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
		
		private static float m_ClipPlaneY = 0;
		public static float ClipPlaneY{
			get{ return m_ClipPlaneY; }
			set{
				m_ClipPlaneY = value;
				for(int i = 0; i < Shaders.Count; i++){
					int k = 0;
				//	ThreadManager.ExecuteOnMainThread ( () => GL.Uniform1(Shaders[k].ClipPlaneLocation, value) );
				}
			}
		}
	}
}
