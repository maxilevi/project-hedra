/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/05/2016
 * Time: 02:19 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of StaticMeshBuffer.
	/// </summary>
	 public class StaticMeshBuffer : ChunkMeshBuffer, IDisposable
	{
		public bool UseShadows;
		public DrawElementsType DrawType = DrawElementsType.UnsignedShort;
		
		public override void Draw(Vector3 Position, bool Shadows){
			if(Indices == null) return;
			if(Shadows){
				Data.Bind();
				Renderer.EnableVertexAttribArray(0);
				
				//if(GraphicsOptions.Fancy)
					Renderer.EnableVertexAttribArray(1);
	
				if(ShortBuffer){
					Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, UshortIndices.ID);
					////Renderer.DrawElements(PrimitiveType.Triangles, UshortIndices.Count, DrawType, IntPtr.Zero);
				}else{
					Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
					////Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawType, IntPtr.Zero);
				}
				Renderer.DisableVertexAttribArray(0);
				
				if(GameSettings.Fancy)
					Renderer.DisableVertexAttribArray(1);
				Data.Unbind();
				return;
			}

		    //StaticShader["UseShadows"] = UseShadows ? GameSettings.ShadowQuality : 0.0f;

			Data.Bind();

			if(ShortBuffer){
				Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, UshortIndices.ID);
				////Renderer.DrawElements(PrimitiveType.Triangles, UshortIndices.Count, DrawType, IntPtr.Zero);
			}else{
				Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
				////Renderer.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawType, IntPtr.Zero);
			}
			
			Data.Unbind();

		}
		
		public new void Dispose(){
			Blocks.Clear();
			Vertices.Dispose();
			Colors.Dispose();
		    Indices?.Dispose();
		    UshortIndices?.Dispose();
		    Data.Dispose();
		}
		
		public override void Clear(){
			Blocks.Clear();
		}
		
		public override void Bind(){
			Renderer.Disable(EnableCap.Blend);
			/*BlockShaders.StaticShader.Bind();
			Renderer.Uniform3(BlockShaders.StaticShader.LightColorLocation, ShaderManager.LightColor);
			Renderer.Uniform3(BlockShaders.StaticShader.PlayerPositionUniform, GameManager.Player.Position);
		    Renderer.Uniform1(BlockShaders.StaticShader.TimeUniform,
		        !GameManager.InStartMenu ? Time.CurrentFrame : Time.UnPausedCurrentFrame);
		    Renderer.Uniform1(BlockShaders.StaticShader.FancyUniform, (GameSettings.Fancy) ? 1.0f : 0.0f);
			Renderer.Uniform1(BlockShaders.StaticShader.SnowUniform, SkyManager.Snowing ? 1.0f : 0.0f);

			BlockShaders.StaticShader.AreaPositionsUniform.LoadVectorArray( World.Highlighter.AreaPositions);
			BlockShaders.StaticShader.AreaColorsUniform.LoadVectorArray( World.Highlighter.AreaColors);
			
			if(GameSettings.Shadows){
				Renderer.UniformMatrix4(BlockShaders.StaticShader.ShadowMVPUniform, false, ref ShadowRenderer.ShadowMvp);
				Renderer.ActiveTexture(TextureUnit.Texture0);
				Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
				Renderer.Uniform1(BlockShaders.StaticShader.ShadowTexUniform, 0);
			}*/
			
		}
		
		public override void UnBind(){
			//BlockShaders.StaticShader.UnBind();
		}
	}
}
