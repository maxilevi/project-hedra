/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 08:49 p.m.
 *
 */
using System;
using Hedra.Engine.Rendering.Effects;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of WaterMeshBuffer.
	/// </summary>
	public class WaterMeshBuffer : ChunkMeshBuffer
	{
		public static float WaveMovement = 0;
		public static bool ShowBackfaces = false;
		public static bool Move = true;
		
		public override void Draw(Vector3 Position, bool Shadows){
			if(Shadows) return;
			throw new Exception("Obsolete");
			//GL.Uniform1(WorldRenderer.WaterShader.WaveMovementUniform, WaveMovement);
			
			WaveMovement += (float) Time.deltaTime * Mathf.Radian * 0.2f;
			if(WaveMovement >= 5)
				WaveMovement = 0;
			
			Data.Bind();
			GraphicsLayer.EnableVertexAttribArray(0);
			GraphicsLayer.EnableVertexAttribArray(1);
			GraphicsLayer.EnableVertexAttribArray(2);

			//GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
			GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Count);//////GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
			
			GraphicsLayer.DisableVertexAttribArray(0);
			GraphicsLayer.DisableVertexAttribArray(1);
			GraphicsLayer.DisableVertexAttribArray(2);
			Data.UnBind();
		}
		
		public override void Bind(){
		    throw new Exception("Obsolete");
            GraphicsLayer.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
           	GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
           	GraphicsLayer.Enable(EnableCap.Texture2D);
           	
           	//BlockShaders.WaterShader.Bind();
           	//GL.Uniform3(BlockShaders.WaterShader.PlayerPositionUniform, GameManager.Player.Position);
           	//GL.Uniform3(BlockShaders.WaterShader.LightColorLocation, ShaderManager.LightColor);

            //Removed the smooth borders because it causes issues on the map rendering.

			
			//BlockShaders.WaterShader.AreaPositionsUniform.LoadVectorArray( World.Highlighter.AreaPositions );
			//BlockShaders.WaterShader.AreaColorsUniform.LoadVectorArray( World.Highlighter.AreaColors );
           	
           	
           	if(ShowBackfaces) 
           		GraphicsLayer.Disable(EnableCap.CullFace);
		}
		
		public override void UnBind(){
		    throw new Exception("Obsolete");
            GraphicsLayer.Disable(EnableCap.Blend);
			GraphicsLayer.Disable(EnableCap.Texture2D);
			//BlockShaders.WaterShader.UnBind();
			GraphicsLayer.Enable(EnableCap.CullFace);
		}
	}
}