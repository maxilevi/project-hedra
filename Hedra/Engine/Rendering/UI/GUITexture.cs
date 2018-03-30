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
		
		public bool Flipped { get; set; }
        public bool Fxaa { get; set; }
        public uint TextureId { get; set; }
        public float Opacity { get; set; }  = 1;
		public uint BackGroundId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
        public Vector4 Color { get; set; }
		public bool Enabled {get; set;}
		public Vector4 Tint { get; set; } = new Vector4(1,1,1,1);
		public bool Grayscale { get; set; }
		public float Angle { get; set; }
	    public uint MaskId { get; set; }
        public bool UseMask => MaskId != 0;
        public Func<uint> IdPointer { get; set; }

        public GUITexture(uint Id, Vector2 Scale, Vector2 Pos){
			this.TextureId = Id;
			this.Position = Pos;
			this.Scale = Scale;
			
			DisposeManager.Add(this);
		}

		public uint Id => IdPointer?.Invoke() ?? TextureId;

	    public Matrix3 RotationMatrix => Matrix3.CreateFromAxisAngle(Vector3.UnitZ, Angle * Mathf.Radian);

	    public void Dispose()
	    {
	        Graphics2D.Textures.Remove(TextureId);
	        var id = TextureId;
			GL.DeleteTextures(1, ref id);
	        TextureId = id;
	    }

	    ~GUITexture()
	    {
	        ThreadManager.ExecuteOnMainThread( this.Dispose );
	    }
    }
}
