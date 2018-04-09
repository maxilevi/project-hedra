/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/09/2017
 * Time: 04:48 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of DropShadowRenderer.
	/// </summary>
	public class DropShadowRenderer
	{
		public DropShadowShader Shader = new DropShadowShader("Shaders/DropShadows.vert", "Shaders/DropShadows.frag");
		private List<DropShadow> Shadows = new List<DropShadow>();
		private List<DropShadow> ShouldShadows = new List<DropShadow>();
        private HashSet<Vector3> ShadowPositions = new HashSet<Vector3>();
		public int Count {get; private set;}

		
		public void Add(DropShadow Shadow){
			lock(Shadows)
				Shadows.Add(Shadow);

		    lock (ShadowPositions)
		        ShadowPositions.Add(Shadow.Position);

		}

	    public void Remove(DropShadow Shadow)
	    {
	        lock (Shadows)
	            Shadows.Remove(Shadow);

            /*TODO: since shadows can be moved outside of the renderer's scope this might create a small memory leak for dynamic shadows because the position on the hashtable hasnt been updated.*/
	        lock (ShadowPositions) {
	            if (ShadowPositions.Contains(Shadow.Position))
	                ShadowPositions.Remove(Shadow.Position);
	        }
	}
		
		public DropShadow Get(Vector3 ShadowPosition){
			return Shadows.Find( Shadow => Shadow.Position == ShadowPosition);
		}
		
		public bool Exists(Vector3 ShadowPosition)
		{
		    lock (ShadowPositions)
                return ShadowPositions.Contains(ShadowPosition);
		    
		}
		

		public void Draw(){
			if(GameSettings.SSAO || GameSettings.ShadowQuality <= 1) return;
			
			lock(Shadows){
				ShouldShadows.Clear();
				for(int i = 0; i < Shadows.Count; i++){
					if(Shadows[i].ShouldDraw && DrawManager.FrustumObject.PointInFrustum(Shadows[i].Position))
						ShouldShadows.Add(Shadows[i]);
				}
			}
			Count = ShouldShadows.Count;
			if(ShouldShadows.Count > 0){

				ShouldShadows = ShouldShadows.OrderBy( Shadow => Shadow.Position.Y).ToList();
				Shader.Bind();
				GL.Enable(EnableCap.Blend);
				GL.Disable(EnableCap.DepthTest);

			    DrawManager.UIRenderer.SetupQuad();

                for (int i = 0; i < ShouldShadows.Count; i++)
			    {

			        if (ShouldShadows[i].DeleteWhen != null && ShouldShadows[i].DeleteWhen())
			        {
			            this.Remove(ShouldShadows[i]);
			            continue;
			        }

			        if (ShouldShadows[i].DepthTest)
			            GL.Enable(EnableCap.DepthTest);

			        Matrix3 rotation = ShouldShadows[i].Rotation;

                    GL.UniformMatrix3(Shader.RotationUniform, false, ref rotation);
					GL.Uniform1(Shader.OpacityUniform, ShouldShadows[i].Opacity);
					GL.Uniform3(Shader.PlayerPositionUniform, GameManager.Player.Position);
					GL.Uniform3(Shader.PositionUniform, ShouldShadows[i].Position);
					GL.Uniform3(Shader.ScaleUniform, ShouldShadows[i].Scale);
					
					DrawManager.UIRenderer.DrawQuad();
					
					if(ShouldShadows[i].DepthTest)
						GL.Disable(EnableCap.DepthTest);
				}
				GL.Enable(EnableCap.DepthTest);
				GL.Disable(EnableCap.Blend);
				Shader.UnBind();
			}
		}
	}
}