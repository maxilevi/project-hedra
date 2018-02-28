/*
 * Author: Zaphyk
 * Date: 28/02/2016
 * Time: 03:24 a.m.
 *
 */
using OpenTK;
using System;
using System.Linq;
using System.Collections;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
	public class VAO : IDisposable{
		public uint ID;
		private bool Disposed;
		
		public void Bind(){
			GL.BindVertexArray(ID);
		}
		
		public void UnBind(){
			GL.BindVertexArray(0);
		}
		
		public void Dispose(){
			if(!Disposed){
				Disposed = true;
				ThreadManager.ExecuteOnMainThread( delegate{ GL.DeleteVertexArrays(1, ref ID); });
			}
		}
	}
	
	public class VAO<T1> : VAO where T1 : struct
	{
		public VAO(VBO<T1> Buffer){
			GL.GenVertexArrays(1, out ID);
			GL.BindVertexArray(ID);
			
			GL.BindBuffer(Buffer.BufferTarget, Buffer.ID);
			GL.VertexAttribPointer(0, Buffer.Size, Buffer.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}
	}
	
	public class VAO<T1, T2> : VAO where T1 : struct where T2 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2){
			GL.GenVertexArrays(1, out ID);
			GL.BindVertexArray(ID);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer2.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}
	}
	
	public class VAO<T1, T2, T3> : VAO where T1 : struct where T2 : struct where T3 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3){
			GL.GenVertexArrays(1, out ID);
			GL.BindVertexArray(ID);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}
	}
	
	public class VAO<T1, T2, T3, T4> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4){
			GL.GenVertexArrays(1, out ID);
			GL.BindVertexArray(ID);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
			GL.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}
	}
	
	public class VAO<T1, T2, T3, T4, T5> : VAO where T1 : struct where T2 : struct where T3 : struct  where T4 : struct where T5 : struct
	{
		public VAO(VBO<T1> Buffer1, VBO<T2> Buffer2, VBO<T3> Buffer3, VBO<T4> Buffer4, VBO<T5> Buffer5){
			GL.GenVertexArrays(1, out ID);
			GL.BindVertexArray(ID);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer1.ID);
			GL.VertexAttribPointer(0, Buffer1.Size, Buffer1.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer1.BufferTarget, Buffer2.ID);
			GL.VertexAttribPointer(1, Buffer2.Size, Buffer2.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer3.BufferTarget, Buffer3.ID);
			GL.VertexAttribPointer(2, Buffer3.Size, Buffer3.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer4.BufferTarget, Buffer4.ID);
			GL.VertexAttribPointer(3, Buffer4.Size, Buffer4.PointerType, false, 0, 0);
			
			GL.BindBuffer(Buffer5.BufferTarget, Buffer5.ID);
			GL.VertexAttribPointer(4, Buffer5.Size, Buffer5.PointerType, false, 0, 0);
			
			GL.BindVertexArray(0);
		}
	}
}
