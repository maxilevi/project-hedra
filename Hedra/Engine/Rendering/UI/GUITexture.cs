/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:51 a.m.
 *
 */
using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GUITexture.
	/// </summary>
	public class GUITexture : IDisposable, ISimpleTexture
	{
		public bool Flipped { get; set; }
        public bool Fxaa { get; set; }
        public uint TextureId { get; set; }
        public float Opacity { get; set; }  = 1;
		public uint BackGroundId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
		public bool Enabled {get; set;}
		public Vector4 Tint { get; set; } = new Vector4(1,1,1,1);
		public bool Grayscale { get; set; }
		public float Angle { get; set; }
	    public uint MaskId { get; set; }
        public bool UseMask => MaskId != 0;
        public Func<uint> IdPointer { get; set; }
	    private bool _disposed;

        public GUITexture(uint Id, Vector2 Scale, Vector2 Pos)
        {
			this.TextureId = Id;
			this.Position = Pos;
			this.Scale = Scale;
        }

		public uint Id => IdPointer?.Invoke() ?? TextureId;

	    public Matrix3 RotationMatrix => Matrix3.CreateFromAxisAngle(Vector3.UnitZ, Angle * Mathf.Radian);

	    public void Dispose()
	    {
            if(_disposed) return;
	        MaskId = this.DisposeId(MaskId);
	        TextureId = this.DisposeId(TextureId);
	        _disposed = true;
	    }

	    private uint DisposeId(uint DisposeId)
	    {
	        if (Array.IndexOf(GUIRenderer.InmortalTextures, DisposeId) != -1) return DisposeId;
	        Graphics2D.Textures.Remove(DisposeId);
	        GL.DeleteTextures(1, ref DisposeId);
	        return DisposeId;
	    }

	    ~GUITexture()
	    {
	        if (!_disposed)
	        {
                if(Program.GameWindow.IsExiting) return;
                Log.WriteLine($"Texture {Id} failed to dispose correctly.");
	            Executer.ExecuteOnMainThread(this.Dispose);
	        }
	    }
    }
}
