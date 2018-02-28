/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/10/2017
 * Time: 10:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of TextureArray.
	/// </summary>
	public class TextureArray
	{
		public int ID {get; private set;}
		private int MipLevelCount = 1;
		private int Width = 1, Height = 1;
		private int LayerCount = 1;
		
		public TextureArray(int Length, SizedInternalFormat InternalFormat = SizedInternalFormat.Rgba16f){
			ID = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2DArray, ID);
			
			GL.TexStorage3D(TextureTarget3d.Texture2DArray, MipLevelCount, InternalFormat, Width, Height, LayerCount);
			
			GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
	        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
	        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
	        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
		}
	}
}
