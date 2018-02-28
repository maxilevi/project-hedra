/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:51 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GUITexture.
	/// </summary>
	public class GUITexture : IDisposable{
		
		public bool Flipped, Fxaa;
		public uint TextureId;
		public float Opacity = 1;
		public uint BackGroundId = 0;
		public Vector2 Position;
		public Vector2 Scale;
		public Vector4 Color = new Vector4(0,0,0,0);
		public bool IsEnabled{get; set;}
		public Vector4 Tint = new Vector4(1,1,1,1);
		public bool Grayscale = false;
		public float Angle;
		
		public GUITexture(uint Id, Vector2 Scale, Vector2 Pos){
			this.TextureId = Id;
			this.Position = Pos;
			this.Scale = Scale;
			
			DisposeManager.Add(this);
		}
		public Func<uint> IdPointer;
		public uint Id => IdPointer?.Invoke() ?? TextureId;

	    public Matrix3 RotationMatrix => Matrix3.CreateFromAxisAngle(Vector3.UnitZ, Angle * Mathf.Radian);

	    public void Dispose()
	    {
	        Graphics2D.Textures.Remove(TextureId);
			GL.DeleteTextures(1, ref TextureId);
		}

	    ~GUITexture()
	    {
	        ThreadManager.ExecuteOnMainThread( this.Dispose );
	    }

    }
}
