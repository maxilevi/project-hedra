/*
 * Author: Zaphyk
 * Date: 14/03/2016
 * Time: 01:50 p.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace VoxelShift.Engine.Rendering
{
	/// <summary>
	/// Description of UBO.
	/// </summary>
	public class UBO<T> where T : struct
	{
		public int UboID;
		public int Size;
		
		public UBO(int Size, int Index)
		{
			GL.GenBuffers(1, out UboID);
			
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BufferData(BufferTarget.UniformBuffer, (IntPtr) (Size), IntPtr.Zero, BufferUsageHint.DynamicDraw);
			
			BindBlock(Index);
		}
		
		public void BindBlock(int Index){
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Index, (int) UboID);
		}
		
		public void Update(T Data){
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr) (Size), ref Data);
		}
	}
}
