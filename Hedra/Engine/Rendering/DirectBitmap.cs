/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/08/2016
 * Time: 11:48 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Hedra.Engine.Rendering
{
		/// <summary>
		/// Description of DirectBitmap.
		/// </summary>
		public class DirectBitmap : IDisposable
	{
	    public Bitmap Bitmap { get; private set; }
	    public Int32[] Bits { get; private set; }
	    public bool Disposed { get; private set; }
	    public int Height { get; private set; }
	    public int Width { get; private set; }
	
	    protected GCHandle BitsHandle { get; private set; }
	
	    public DirectBitmap(int width, int height)
	    {
	        Width = width;
	        Height = height;
	        Bits = new Int32[width * height];
	        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
	        Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
	    }
	
	    public void Dispose()
	    {
	        if (Disposed) return;
	        Disposed = true;
	        Bitmap.Dispose();
	        BitsHandle.Free();
	    }
	}
}
